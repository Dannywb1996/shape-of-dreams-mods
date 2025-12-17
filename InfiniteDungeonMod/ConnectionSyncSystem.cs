// ============================================================
// CONNECTION SYNC SYSTEM
// Uses the game's persistentSyncedData (SyncDictionary) to sync node connections
// from host to clients efficiently, avoiding the n² matrix problem
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace InfiniteDungeonMod
{
    public partial class InfiniteDungeonMod
    {
        // Key for storing connection data in serverActor.persistentSyncedData
        private const string CONNECTION_DATA_KEY = "InfiniteDungeon::Connections";
        
        // Local cache of connection data (for quick lookup)
        private static HashSet<string> _connectionCache = new HashSet<string>();
        private static int _lastSyncedVersion = -1;
        
        // Flag to track if we've registered the sync callback
        private static bool _syncCallbackRegistered = false;
        
        /// <summary>
        /// Helper to safely get serverActor (C# 5 compatible - no ?. operator)
        /// </summary>
        private static Actor GetServerActor()
        {
            ActorManager actorManager = NetworkedManagerBase<ActorManager>.instance;
            if (actorManager == null) return null;
            return actorManager.serverActor;
        }
        
        /// <summary>
        /// Check if two nodes are connected (works for both server and client)
        /// Server uses _nodeConnections, Client uses ONLY synced data from serverActor
        /// </summary>
        public static bool AreNodesConnected(int nodeA, int nodeB)
        {
            // Server: Use authoritative _nodeConnections
            if (NetworkServer.active)
            {
                if (_nodeConnections.ContainsKey(nodeA))
                {
                    return _nodeConnections[nodeA].Contains(nodeB);
                }
                if (_nodeConnections.ContainsKey(nodeB))
                {
                    return _nodeConnections[nodeB].Contains(nodeA);
                }
                return false;
            }
            
            // Client: Use ONLY synced connection cache from server
            // DO NOT use local _nodeConnections - it may have stale/incorrect data!
            UpdateConnectionCacheFromSync();
            
            string key = GetConnectionKey(nodeA, nodeB);
            return _connectionCache.Contains(key);
        }
        
        /// <summary>
        /// Get a consistent key for a connection (smaller index first)
        /// </summary>
        private static string GetConnectionKey(int nodeA, int nodeB)
        {
            int min = Mathf.Min(nodeA, nodeB);
            int max = Mathf.Max(nodeA, nodeB);
            return min + "-" + max;
        }
        
        /// <summary>
        /// Get the total number of tracked connections (for debugging)
        /// </summary>
        public static int GetConnectionCount()
        {
            if (NetworkServer.active)
            {
                int count = 0;
                foreach (var kvp in _nodeConnections)
                {
                    count += kvp.Value.Count;
                }
                return count / 2; // Each connection is stored twice (bidirectional)
            }
            else
            {
                return _connectionCache.Count;
            }
        }
        
        /// <summary>
        /// Force refresh the connection cache from server data (for clients)
        /// Called when opening the map to ensure we have the latest connections
        /// </summary>
        public static void ForceRefreshConnectionCache()
        {
            if (NetworkServer.active) return; // Server doesn't need this
            
            // Reset version to force a full refresh
            _lastSyncedVersion = -1;
            
            // Update cache WITHOUT triggering map refresh (we're already in RefreshNodes!)
            UpdateConnectionCacheFromSyncNoRefresh();
        }
        
        /// <summary>
        /// Update connection cache without triggering map refresh
        /// Used when called from within RefreshNodes to avoid infinite recursion
        /// </summary>
        private static void UpdateConnectionCacheFromSyncNoRefresh()
        {
            if (NetworkServer.active) return;
            
            try
            {
                Actor serverActor = GetServerActor();
                if (serverActor == null) return;
                
                string connectionData;
                if (!serverActor.persistentSyncedData.TryGetValue(CONNECTION_DATA_KEY, out connectionData))
                {
                    return;
                }
                
                if (string.IsNullOrEmpty(connectionData)) return;
                
                string[] parts = connectionData.Split('|');
                if (parts.Length < 1) return;
                
                int version;
                if (!int.TryParse(parts[0], out version)) return;
                
                if (version <= _lastSyncedVersion) return;
                
                _connectionCache.Clear();
                for (int i = 1; i < parts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(parts[i]))
                    {
                        _connectionCache.Add(parts[i]);
                    }
                }
                
                int oldVersion = _lastSyncedVersion;
                _lastSyncedVersion = version;
                
            }
            catch (Exception)
            {
                // Silently ignore sync errors - will retry
            }
        }
        
        /// <summary>
        /// SERVER: Sync all connections to clients via serverActor.persistentSyncedData
        /// This uses the game's built-in sync mechanism which is efficient and reliable
        /// Uses raw string storage (like RPGItemsMod) for compatibility
        /// </summary>
        public static void SyncConnectionsToClients()
        {
            if (!NetworkServer.active) return;
            if (!IsModActive) return;
            
            try
            {
                Actor serverActor = GetServerActor();
                if (serverActor == null)
                    return;
                
                // Build connection string: "version|nodeA1-nodeB1|nodeA2-nodeB2|..."
                // Using simple string format for maximum compatibility
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                HashSet<string> addedPairs = new HashSet<string>();
                
                int version = _lastSyncedVersion + 1;
                _lastSyncedVersion = version;
                
                sb.Append(version.ToString());
                
                foreach (var kvp in _nodeConnections)
                {
                    int nodeA = kvp.Key;
                    foreach (int nodeB in kvp.Value)
                    {
                        string pairKey = GetConnectionKey(nodeA, nodeB);
                        if (!addedPairs.Contains(pairKey))
                        {
                            addedPairs.Add(pairKey);
                            sb.Append("|");
                            sb.Append(pairKey);
                        }
                    }
                }
                
                string connectionData = sb.ToString();
                
                // Store in serverActor's synced data - this automatically syncs to all clients!
                // Using dictionary indexer directly (like RPGItemsMod does)
                serverActor.persistentSyncedData[CONNECTION_DATA_KEY] = connectionData;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error syncing connections: " + ex.Message);
            }
        }
        
        /// <summary>
        /// CLIENT: Update local connection cache from serverActor's synced data
        /// Called automatically when checking connections
        /// Returns true if cache was actually updated (new version received)
        /// </summary>
        private static bool UpdateConnectionCacheFromSync()
        {
            if (NetworkServer.active) return false; // Server doesn't need this
            
            try
            {
                Actor serverActor = GetServerActor();
                if (serverActor == null) return false;
                
                // Try to get connection data from synced dictionary
                string connectionData;
                if (!serverActor.persistentSyncedData.TryGetValue(CONNECTION_DATA_KEY, out connectionData))
                {
                    return false; // No data yet
                }
                
                if (string.IsNullOrEmpty(connectionData))
                {
                    return false;
                }
                
                // Parse: "version|nodeA1-nodeB1|nodeA2-nodeB2|..."
                string[] parts = connectionData.Split('|');
                if (parts.Length < 1) return false;
                
                int version;
                if (!int.TryParse(parts[0], out version))
                {
                    return false;
                }
                
                // Check if we need to update
                if (version <= _lastSyncedVersion)
                {
                    return false; // Already up to date
                }
                
                // Update cache
                _connectionCache.Clear();
                for (int i = 1; i < parts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(parts[i]))
                    {
                        _connectionCache.Add(parts[i]);
                    }
                }
                
                int oldVersion = _lastSyncedVersion;
                _lastSyncedVersion = version;
                
                return true; // Cache was updated
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Register callback to detect when synced data changes (for clients)
        /// </summary>
        private void RegisterConnectionSyncCallback()
        {
            if (_syncCallbackRegistered) return;
            
            try
            {
                Actor serverActor = GetServerActor();
                if (serverActor == null) return;
                
                // IMPORTANT: Unsubscribe first to prevent duplicate callbacks
                // This handles cases where serverActor was recreated
                try
                {
                    serverActor.persistentSyncedData.Callback -= OnConnectionDataChanged;
                }
                catch { }
                
                // Register callback for when the synced dictionary changes
                serverActor.persistentSyncedData.Callback += OnConnectionDataChanged;
                _syncCallbackRegistered = true;
            }
            catch (Exception)
            {
                // Will retry on next attempt
            }
        }
        
        /// <summary>
        /// Unregister the sync callback (called on cleanup)
        /// </summary>
        private static void UnregisterConnectionSyncCallback()
        {
            if (!_syncCallbackRegistered) return;
            
            try
            {
                Actor serverActor = GetServerActor();
                if (serverActor != null)
                {
                    serverActor.persistentSyncedData.Callback -= OnConnectionDataChanged;
                }
            }
            catch { }
            
            _syncCallbackRegistered = false;
        }
        
        /// <summary>
        /// Callback when serverActor.persistentSyncedData changes
        /// Only processes our connection data key, ignores other keys
        /// </summary>
        private static void OnConnectionDataChanged(SyncIDictionary<string, string>.Operation op, 
                                                     string key, string value)
        {
            // Safety check - don't process if mod is not active
            if (!IsModActive) return;
            
            // Handle null key for OP_CLEAR
            if (op == SyncIDictionary<string, string>.Operation.OP_CLEAR)
            {
                _connectionCache.Clear();
                _lastSyncedVersion = -1;
                return;
            }
            
            if (string.IsNullOrEmpty(key)) return;
            if (key != CONNECTION_DATA_KEY) return;
            
            // Don't spam logs - UpdateConnectionCacheFromSync will log if actually updated
            
            // Force cache update on next connection check
            _lastSyncedVersion = -1;
            
            // Immediately update cache (this will only log if version actually changed)
            bool wasUpdated = UpdateConnectionCacheFromSync();
            
            // Only trigger map refresh if something actually changed
            if (wasUpdated)
            {
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm != null && zm.ClientEvent_OnNodesChanged != null)
                {
                    zm.ClientEvent_OnNodesChanged.Invoke();
                }
            }
        }
        
        /// <summary>
        /// Clear synced connection data (called on mod reset)
        /// </summary>
        public static void ClearSyncedConnections()
        {
            _connectionCache.Clear();
            _lastSyncedVersion = -1;
            
            // Unregister callback before clearing flag
            UnregisterConnectionSyncCallback();
            
            // Also clear from serverActor if we're the server
            if (NetworkServer.active)
            {
                try
                {
                    Actor serverActor = GetServerActor();
                    if (serverActor != null && serverActor.persistentSyncedData.ContainsKey(CONNECTION_DATA_KEY))
                    {
                        serverActor.persistentSyncedData.Remove(CONNECTION_DATA_KEY);
                    }
                }
                catch (Exception)
                {
                    // Ignore clear errors
                }
            }
        }
        
        /// <summary>
        /// Client-side coroutine to monitor connection sync and refresh map
        /// Only triggers refresh when actual changes are detected
        /// </summary>
        private IEnumerator MonitorConnectionSync()
        {
            int lastCacheCount = 0;
            int lastVersion = _lastSyncedVersion;
            float checkInterval = 1.0f; // Check every 1 second (reduced frequency)
            
            while (IsModActive && !NetworkServer.active)
            {
                yield return new WaitForSeconds(checkInterval);
                
                // Try to register callback if not done yet
                if (!_syncCallbackRegistered)
                {
                    RegisterConnectionSyncCallback();
                }
                
                // Check for updates (don't force refresh, just check)
                bool wasUpdated = UpdateConnectionCacheFromSync();
                
                // Only trigger map refresh if cache was actually updated
                if (wasUpdated && (_connectionCache.Count != lastCacheCount || _lastSyncedVersion != lastVersion))
                {
                    lastCacheCount = _connectionCache.Count;
                    lastVersion = _lastSyncedVersion;
                    
                    // Trigger map refresh
                    ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                    if (zm != null && zm.ClientEvent_OnNodesChanged != null)
                    {
                        zm.ClientEvent_OnNodesChanged.Invoke();
                    }
                }
            }
        }
    }
}
