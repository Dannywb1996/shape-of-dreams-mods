using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteDungeonMod
{
    public partial class InfiniteDungeonMod
    {
        // ==================== HUNTER INFECTION & CURSES ====================
        /// Register a zone for a node (called from patches)
        /// </summary>
        public static void RegisterNodeZone(int nodeIndex, Zone zone)
        {
            _nodeZoneMap[nodeIndex] = zone;
        }
        
        /// <summary>
        /// Check if a new node position would intersect any existing connection path.
        /// This is the BULLETPROOF check that prevents nodes from appearing on top of lines.
        /// </summary>
        private bool DoesPositionIntersectAnyPath(ZoneManager zm, Vector2 newPos, int parentNodeIndex)
        {
            try
            {
                float threshold = 12f; // Distance threshold - node must be at least this far from any line
                
                // Check against all existing connections
                foreach (var kvp in _nodeConnections)
                {
                    int nodeA = kvp.Key;
                    if (nodeA >= zm.nodes.Count) continue;
                    
                    Vector2 posA = zm.nodes[nodeA].position;
                    
                    foreach (int nodeB in kvp.Value)
                    {
                        // Skip if this is the connection we're about to create
                        if ((nodeA == parentNodeIndex && nodeB == zm.nodes.Count) ||
                            (nodeB == parentNodeIndex && nodeA == zm.nodes.Count))
                        {
                            continue;
                        }
                        
                        // Skip if nodeB is invalid
                        if (nodeB >= zm.nodes.Count) continue;
                        
                        // Only check each edge once (nodeA < nodeB)
                        if (nodeA >= nodeB) continue;
                        
                        Vector2 posB = zm.nodes[nodeB].position;
                        
                        // Calculate distance from newPos to line segment (posA, posB)
                        float dist = DistancePointToLineSegment(newPos, posA, posB);
                        
                        if (dist < threshold)
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in DoesPositionIntersectAnyPath: " + ex.Message);
                return false; // Allow on error
            }
        }
        
        /// <summary>
        /// Calculate the minimum distance from a point to a line segment.
        /// </summary>
        private float DistancePointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLengthSq = line.sqrMagnitude;
            
            if (lineLengthSq < 0.001f)
            {
                // Line segment is essentially a point
                return Vector2.Distance(point, lineStart);
            }
            
            // Project point onto line, clamped to segment
            float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, line) / lineLengthSq);
            Vector2 projection = lineStart + t * line;
            
            return Vector2.Distance(point, projection);
        }
        
        /// <summary>
        /// Check if a new connection line would cross any existing paths.
        /// </summary>
        private bool DoesNewLineCrossExistingPaths(ZoneManager zm, Vector2 lineStart, Vector2 lineEnd, int parentNodeIndex)
        {
            try
            {
                // Check against all existing connections
                foreach (var kvp in _nodeConnections)
                {
                    int nodeA = kvp.Key;
                    if (nodeA >= zm.nodes.Count) continue;
                    
                    Vector2 posA = zm.nodes[nodeA].position;
                    
                    foreach (int nodeB in kvp.Value)
                    {
                        // Skip if nodeB is invalid
                        if (nodeB >= zm.nodes.Count) continue;
                        
                        // Only check each edge once (nodeA < nodeB)
                        if (nodeA >= nodeB) continue;
                        
                        // Skip if this edge shares an endpoint with our new line
                        if (nodeA == parentNodeIndex || nodeB == parentNodeIndex) continue;
                        
                        Vector2 posB = zm.nodes[nodeB].position;
                        
                        // Check if the two line segments intersect
                        if (DoLinesIntersect(lineStart, lineEnd, posA, posB))
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in DoesNewLineCrossExistingPaths: " + ex.Message);
                return false; // Allow on error
            }
        }
        
        /// <summary>
        /// Check if two line segments intersect (proper intersection, not just touching at endpoints).
        /// </summary>
        private bool DoLinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            // Using cross product method for line segment intersection
            float d1 = CrossProduct(p3, p4, p1);
            float d2 = CrossProduct(p3, p4, p2);
            float d3 = CrossProduct(p1, p2, p3);
            float d4 = CrossProduct(p1, p2, p4);
            
            // Check if the signs are different (lines cross)
            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Calculate cross product of vectors (p2-p1) and (p3-p1).
        /// </summary>
        private float CrossProduct(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
        }
        
        /// <summary>
        /// Custom Hunter Infection System - chance to trigger infection event
        /// When triggered:
        /// 1. Creates 1-2 infected CHAINS (each 2-4 nodes connected in sequence, dungeon-like!)
        /// 2. Infects existing forward nodes
        /// </summary>
        private void TryInfectAdjacentNodes(ZoneManager zm, int currentNode)
        {
            try
            {
                // SAFE/BOSS NODE CHECK - Merchant, Start, and Boss nodes can't trigger infection!
                if (currentNode < zm.nodes.Count)
                {
                    WorldNodeType nodeType = zm.nodes[currentNode].type;
                    if (nodeType == WorldNodeType.Start || 
                        nodeType == WorldNodeType.Merchant ||
                        nodeType == WorldNodeType.ExitBoss)
                    {
                        return;
                    }
                }
                
                // Check chance to trigger infection event
                if (_dungeonRandom.Value() >= HUNTER_INFECTION_CHANCE) return;
                
                SendChatMessage(DungeonLocalization.CreateMessageKey("HUNTER_INFECTED"));
                
                // ========================================
                // STEP 1: Generate infected CHAINS (dungeon-like corridors!)
                // Each chain is 2-4 nodes connected in sequence: A→B→C→D
                // ========================================
                int numChains = _dungeonRandom.Range(1, 3); // 1-2 chains
                List<int> allInfectedNodes = new List<int>();
                
                // Get current node position
                Vector2 currentPos = Vector2.zero;
                if (_nodePositions.ContainsKey(currentNode))
                {
                    currentPos = _nodePositions[currentNode];
                }
                else if (currentNode < zm.nodes.Count)
                {
                    currentPos = zm.nodes[currentNode].position;
                    _nodePositions[currentNode] = currentPos; // Cache it!
                }
                
                // Safety check - if position is still zero, we can't generate chains
                if (currentPos == Vector2.zero)
                {
                    // Check if current node is a safe/boss node - don't infect these!
                    WorldNodeType nodeType = zm.nodes[currentNode].type;
                    if (nodeType == WorldNodeType.Start || 
                        nodeType == WorldNodeType.Merchant ||
                        nodeType == WorldNodeType.ExitBoss)
                    {
                        return;
                    }
                    
                    // Just infect the current node and return
                    _hunterInfectedNodes.Add(currentNode);
                    if (currentNode < zm.hunterStatuses.Count)
                    {
                        zm.hunterStatuses[currentNode] = HunterStatus.Level1;
                    }
                    return;
                }
                
                // Get parent direction
                float parentDirection;
                if (!_nodeDirections.TryGetValue(currentNode, out parentDirection))
                {
                    parentDirection = _dungeonRandom.Range(0f, 360f);
                    _nodeDirections[currentNode] = parentDirection; // Cache it!
                }
                
                // Generate each chain at different angles
                for (int chainIdx = 0; chainIdx < numChains; chainIdx++)
                {
                    int chainLength = _dungeonRandom.Range(2, 5); // 2-4 nodes per chain
                    
                    // Pick a direction for this chain (spread chains apart)
                    float chainBaseAngle;
                    if (numChains == 1)
                    {
                        chainBaseAngle = parentDirection + _dungeonRandom.Range(-30f, 30f);
                    }
                    else
                    {
                        // Spread chains 90-180 degrees apart
                        chainBaseAngle = parentDirection + (chainIdx * (180f / numChains)) + _dungeonRandom.Range(-20f, 20f);
                    }
                    
                    // Start chain from current node
                    int chainParent = currentNode;
                    Vector2 chainPos = currentPos;
                    float chainDirection = chainBaseAngle;
                    
                    // Track cumulative turn for winding infected corridors
                    float cumulativeTurn = 0f;
                    float turnBias = _dungeonRandom.Value() < 0.5f ? 1f : -1f;
                    
                    for (int nodeInChain = 0; nodeInChain < chainLength; nodeInChain++)
                    {
                        // WINDING PATH: Each node turns more, creating organic curves
                        float turnAmount = _dungeonRandom.Range(25f, 55f) * turnBias;
                        
                        // Occasionally reverse for S-curves
                        if (_dungeonRandom.Value() < 0.25f)
                        {
                            turnBias *= -1f;
                        }
                        
                        cumulativeTurn += turnAmount;
                        float nodeAngle = chainDirection + cumulativeTurn + _dungeonRandom.Range(-20f, 20f);
                        
                        Vector2 nodePos = FindValidNodePosition(zm, chainPos, nodeAngle, chainParent);
                        
                        if (nodePos == Vector2.zero)
                        {
                            // Try other angles
                            bool found = false;
                            for (int retry = 0; retry < 12; retry++) // Increased retries
                            {
                                nodeAngle = chainDirection + _dungeonRandom.Range(-120f, 120f); // Wider angle range
                                nodePos = FindValidNodePosition(zm, chainPos, nodeAngle, chainParent);
                                if (nodePos != Vector2.zero)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                break;
                            }
                        }
                        
                        // Create node connected to chain parent (not current node!)
                        int newNodeIndex = CreateNodeAtPosition(zm, chainParent, nodePos, nodeAngle);
                        if (newNodeIndex >= 0)
                        {
                            // Check if the created node is a safe type - DON'T infect Merchant or Start nodes!
                            WorldNodeType createdNodeType = zm.nodes[newNodeIndex].type;
                            bool isSafeNode = (createdNodeType == WorldNodeType.Merchant || 
                                               createdNodeType == WorldNodeType.Start ||
                                               createdNodeType == WorldNodeType.ExitBoss);
                            
                            if (!isSafeNode)
                            {
                                allInfectedNodes.Add(newNodeIndex);
                                
                                // Mark as infected!
                                _hunterInfectedNodes.Add(newNodeIndex);
                                
                                // Set game's hunter status for map visual
                                while (zm.hunterStatuses.Count <= newNodeIndex)
                                {
                                    zm.hunterStatuses.Add(HunterStatus.None);
                                }
                                zm.hunterStatuses[newNodeIndex] = HunterStatus.Level1;
                            }
                            
                            // Mark chain parent as having children (so it doesn't regenerate)
                            _nodesWithChildren.Add(chainParent);
                            
                            // Move to next in chain (even if safe, continue the chain)
                            chainParent = newNodeIndex;
                            chainPos = nodePos;
                            chainDirection = nodeAngle;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
                // ========================================
                // STEP 2: Also infect existing forward nodes
                // ========================================
                List<int> eligibleExistingNodes = new List<int>();
                
                if (_nodeConnections.ContainsKey(currentNode))
                {
                    foreach (int adjacentNode in _nodeConnections[currentNode])
                    {
                        if (adjacentNode == 0) continue;
                        if (adjacentNode >= zm.nodes.Count) continue;
                        
                        // SAFE NODES - Only Merchant and Start are truly safe!
                        // ExitBoss is skipped because it's special (boss room)
                        WorldNodeType adjType = zm.nodes[adjacentNode].type;
                        if (adjType == WorldNodeType.ExitBoss) continue; // Boss rooms are special
                        if (adjType == WorldNodeType.Merchant) continue; // Safe
                        if (adjType == WorldNodeType.Start) continue;    // Safe
                        
                        if (_hunterInfectedNodes.Contains(adjacentNode)) continue;
                        if (zm.nodes[adjacentNode].status == WorldNodeStatus.HasVisited) continue;
                        
                        eligibleExistingNodes.Add(adjacentNode);
                    }
                }
                
                // Infect 1-2 existing forward nodes
                int existingToInfect = Mathf.Min(_dungeonRandom.Range(1, 3), eligibleExistingNodes.Count);
                
                // Shuffle
                for (int i = eligibleExistingNodes.Count - 1; i > 0; i--)
                {
                    int j = _dungeonRandom.Range(0, i + 1);
                    int temp = eligibleExistingNodes[i];
                    eligibleExistingNodes[i] = eligibleExistingNodes[j];
                    eligibleExistingNodes[j] = temp;
                }
                
                for (int i = 0; i < existingToInfect; i++)
                {
                    int nodeToInfect = eligibleExistingNodes[i];
                    _hunterInfectedNodes.Add(nodeToInfect);
                    
                    if (nodeToInfect < zm.hunterStatuses.Count)
                    {
                        zm.hunterStatuses[nodeToInfect] = HunterStatus.Level1;
                    }
                }
                
                // ========================================
                // STEP 3: Rebuild matrix and announce
                // ========================================
                RebuildDistanceMatrix(zm);
                
                // Announce the new paths
                if (allInfectedNodes.Count > 0)
                {
                    SendChatMessage(DungeonLocalization.CreateMessageKey("HUNTER_PATH", allInfectedNodes.Count.ToString(), numChains.ToString()));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in TryInfectAdjacentNodes: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
        
        /// <summary>
        /// Apply Hunter effects to all monsters in the current room.
        /// This spawns the RoomMod_Hunted modifier which:
        /// - Applies Se_HunterBuff to all monsters
        /// - Increases monster stats
        /// - Makes them "Hunter" monsters
        /// - Spawns hunter artillery (meteors falling from sky!)
        /// </summary>
        private IEnumerator ApplyHunterEffectsToRoom()
        {
            // Wait for room and monsters to be ready
            yield return new WaitForSeconds(0.5f);
            
            Room room = SingletonDewNetworkBehaviour<Room>.instance;
            if (room == null || room.monsters == null)
            {
                yield break;
            }
            
            // CRITICAL: Double-check that this is NOT a safe room!
            // Safe rooms (Merchant, Start, Boss) should NEVER have hunter effects applied!
            ZoneManager zmCheck = NetworkedManagerBase<ZoneManager>.instance;
            if (zmCheck != null && zmCheck.currentNodeIndex >= 0 && zmCheck.currentNodeIndex < zmCheck.nodes.Count)
            {
                WorldNodeType currentNodeType = zmCheck.nodes[zmCheck.currentNodeIndex].type;
                if (currentNodeType == WorldNodeType.ExitBoss || 
                    currentNodeType == WorldNodeType.Merchant ||
                    currentNodeType == WorldNodeType.Start)
                {
                    yield break;
                }
                
                // CRITICAL FIX: Don't add RoomMod_Hunted to rooms with special clear conditions!
                // These modifiers have their own room clear logic that conflicts with RoomMod_Hunted:
                // - RoomMod_DistantMemories: Crystal break triggers room clear + portal open
                // - RoomMod_Ambush: Spawner destruction triggers room clear + portal open
                // Adding RoomMod_Hunted can cause the portal to never spawn due to timing conflicts.
                foreach (var mod in zmCheck.nodes[zmCheck.currentNodeIndex].modifiers)
                {
                    if (mod.type == "RoomMod_DistantMemories" || mod.type == "RoomMod_Ambush")
                    {
                        yield break;
                    }
                }
            }
            
            // Set the addedHunterChance to 100% so all spawned monsters become hunters
            room.monsters.addedHunterChance = 1.0f;
            
            // Spawn the RoomMod_Hunted modifier to get the full hunter experience
            // This includes the artillery (meteors) that fall from the sky!
            try
            {
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm != null)
                {
                    int currentNode = zm.currentNodeIndex;
                    
                    // Check if node already has RoomMod_Hunted
                    bool hasHuntedMod = false;
                    if (currentNode >= 0 && currentNode < zm.nodes.Count)
                    {
                        foreach (var mod in zm.nodes[currentNode].modifiers)
                        {
                            if (mod.type == "RoomMod_Hunted")
                            {
                                hasHuntedMod = true;
                                break;
                            }
                        }
                    }
                    
                    // Add RoomMod_Hunted if not already present
                    if (!hasHuntedMod)
                    {
                        // Use the game's AddModifier method to spawn the hunter modifier
                        // This creates the actual RoomMod_Hunted instance with artillery
                        _allowModifierAddition = true;
                        zm.AddModifier<RoomMod_Hunted>(currentNode);
                        _allowModifierAddition = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[InfiniteDungeon] Could not spawn RoomMod_Hunted: " + ex.Message);
            }
            
            // Wait a bit more for monsters to spawn
            yield return new WaitForSeconds(1.0f);
            
            // Apply hunter buff to existing monsters (backup in case RoomMod_Hunted didn't apply)
            ApplyHunterBuffToAllMonsters();
            
            // Wait and apply again to catch any late spawns
            yield return new WaitForSeconds(2.0f);
            ApplyHunterBuffToAllMonsters();
        }
        
        /// <summary>
        /// Find all monsters and apply hunter buff to those that don't have it
        /// Also ensures existing hunter buffs have EXP drops enabled!
        /// </summary>
        private void ApplyHunterBuffToAllMonsters()
        {
            try
            {
                ListReturnHandle<Monster> handle;
                List<Monster> monsters = Dew.FindAllActorsOfType<Monster>(out handle);
                
                int hunterCount = 0;
                int fixedExpCount = 0;
                foreach (var monster in monsters)
                {
                    if (monster == null || !monster.isActive || monster.Status == null) continue;
                    
                    // Check if already has hunter buff
                    Se_HunterBuff existingBuff;
                    if (monster.Status.TryGetStatusEffect<Se_HunterBuff>(out existingBuff))
                    {
                        // IMPORTANT: Fix existing buffs that don't have EXP drops enabled!
                        // RoomMod_Hunted doesn't enable this by default, causing 0 EXP drops
                        if (existingBuff != null && !existingBuff.enableGoldAndExpDrops)
                        {
                            existingBuff.enableGoldAndExpDrops = true;
                            fixedExpCount++;
                        }
                        hunterCount++;
                        continue;
                    }
                    
                    // Apply hunter buff - IMPORTANT: Enable gold/exp drops!
                    try
                    {
                        Se_HunterBuff buff = monster.CreateStatusEffect<Se_HunterBuff>(monster, new CastInfo(monster));
                        if (buff != null)
                        {
                            buff.enableGoldAndExpDrops = true; // Without this, hunter monsters give 0 EXP!
                        }
                        hunterCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("[InfiniteDungeon] Failed to apply hunter buff to " + monster.name + ": " + ex.Message);
                    }
                }
                
                handle.Return();
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in ApplyHunterBuffToAllMonsters: " + ex.Message);
            }
        }
        
        // ============================================================
        // CURSE SYSTEM METHODS
        // ============================================================
        
        /// <summary>
        /// Try to apply a random curse when entering an infected node
        /// 40% chance to get cursed
        /// </summary>
        private void TryApplyRandomCurse()
        {
            try
            {
                // 40% chance to get a curse when entering an infected node
                if (_dungeonRandom == null || _dungeonRandom.Value() >= 0.4f) return;
                
                // Get all curse types (excluding None)
                HunterCurseType[] allCurses = new HunterCurseType[]
                {
                    // Environmental Damage (room modifiers - hurt players)
                    HunterCurseType.MeteorFury,
                    HunterCurseType.InkStorm,
                    HunterCurseType.FlameEngulf,
                    HunterCurseType.ToxicMiasma,
                    HunterCurseType.BlackRain,
                    
                    // Player Debuffs (stat penalties - players only)
                    HunterCurseType.DarkVeil,
                    HunterCurseType.GravityCurse,
                    HunterCurseType.WeakenedSpirit,
                    HunterCurseType.SlowMind,
                    HunterCurseType.Vulnerability,
                    
                    // Monster Buffs (stat bonuses - monsters only)
                    HunterCurseType.TimewarpCurse,
                    HunterCurseType.MonsterRage,
                    HunterCurseType.IronHide,
                    HunterCurseType.Frenzy
                };
                
                // Pick a random curse
                HunterCurseType curse = allCurses[_dungeonRandom.Range(0, allCurses.Length)];
                
                // Random charge count between 1-5
                int curseCharges = _dungeonRandom.Range(MIN_CURSE_CHARGES, MAX_CURSE_CHARGES + 1);
                
                // If player already has this curse, refresh it instead
                if (_playerCurses.ContainsKey(curse))
                {
                    _playerCurses[curse] = curseCharges;
                }
                else
                {
                    _playerCurses[curse] = curseCharges;
                    // Announce the curse - use message key for client-side localization
                    SendChatMessage(DungeonLocalization.CreateMessageKey("CURSE", curse.ToString()));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in TryApplyRandomCurse: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Try to apply an additional curse when entering an infected node while already infected
        /// 25% chance to get a new curse (lower than initial 40%)
        /// </summary>
        private void TryApplyAdditionalCurse()
        {
            try
            {
                // 25% chance to get an additional curse
                if (_dungeonRandom == null || _dungeonRandom.Value() >= 0.25f) return;
                
                // Get all curse types
                HunterCurseType[] allCurses = new HunterCurseType[]
                {
                    // Environmental Damage (room modifiers - hurt players)
                    HunterCurseType.MeteorFury,
                    HunterCurseType.InkStorm,
                    HunterCurseType.FlameEngulf,
                    HunterCurseType.ToxicMiasma,
                    HunterCurseType.BlackRain,
                    
                    // Player Debuffs (stat penalties - players only)
                    HunterCurseType.DarkVeil,
                    HunterCurseType.GravityCurse,
                    HunterCurseType.WeakenedSpirit,
                    HunterCurseType.SlowMind,
                    HunterCurseType.Vulnerability,
                    
                    // Monster Buffs (stat bonuses - monsters only)
                    HunterCurseType.TimewarpCurse,
                    HunterCurseType.MonsterRage,
                    HunterCurseType.IronHide,
                    HunterCurseType.Frenzy
                };
                
                // Pick a random curse
                HunterCurseType curse = allCurses[_dungeonRandom.Range(0, allCurses.Length)];
                
                // Random charge count between 1-5
                int curseCharges = _dungeonRandom.Range(MIN_CURSE_CHARGES, MAX_CURSE_CHARGES + 1);
                
                // If player already has this curse, refresh/extend it
                if (_playerCurses.ContainsKey(curse))
                {
                    // Add charges to existing curse (stacking!)
                    _playerCurses[curse] += curseCharges;
                }
                else
                {
                    // New curse!
                    _playerCurses[curse] = curseCharges;
                    // Announce the new curse
                    SendChatMessage(DungeonLocalization.CreateMessageKey("CURSE", curse.ToString()));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in TryApplyAdditionalCurse: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Get the announcement message for a curse
        /// </summary>
        private string GetCurseAnnouncement(HunterCurseType curse)
        {
            switch (curse)
            {
                // Environmental Damage
                case HunterCurseType.MeteorFury: return DungeonLocalization.CurseMeteorFury;
                case HunterCurseType.InkStorm: return DungeonLocalization.CurseInkStorm;
                case HunterCurseType.FlameEngulf: return DungeonLocalization.CurseFlameEngulf;
                case HunterCurseType.ToxicMiasma: return DungeonLocalization.CurseToxicMiasma;
                case HunterCurseType.BlackRain: return DungeonLocalization.CurseBlackRain;
                
                // Player Debuffs
                case HunterCurseType.DarkVeil: return DungeonLocalization.CurseDarkVeil;
                case HunterCurseType.GravityCurse: return DungeonLocalization.CurseGravity;
                case HunterCurseType.WeakenedSpirit: return DungeonLocalization.CurseWeakenedSpirit;
                case HunterCurseType.SlowMind: return DungeonLocalization.CurseSlowMind;
                case HunterCurseType.Vulnerability: return DungeonLocalization.CurseVulnerability;
                
                // Monster Buffs
                case HunterCurseType.TimewarpCurse: return DungeonLocalization.CurseTimewarp;
                case HunterCurseType.MonsterRage: return DungeonLocalization.CurseMonsterRage;
                case HunterCurseType.IronHide: return DungeonLocalization.CurseIronHide;
                case HunterCurseType.Frenzy: return DungeonLocalization.CurseFrenzy;
                default: return null;
            }
        }
        
        /// <summary>
        /// Apply curse effects only - no tick down (used on first infection room)
        /// </summary>
        private void ApplyCurseEffectsOnly()
        {
            try
            {
                if (_playerCurses.Count == 0) return;
                
                foreach (var curse in _playerCurses.Keys)
                {
                    // Apply the curse effect to this room
                    StartCoroutine(ApplyCurseEffect(curse));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in ApplyCurseEffectsOnly: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Process active curses - apply their effects and tick down charges
        /// Curses tick down EVERY room, even infected ones
        /// </summary>
        private void ProcessPlayerCurses()
        {
            try
            {
                if (_playerCurses.Count == 0) return;
                
                // Apply effects for each active curse
                List<HunterCurseType> expiredCurses = new List<HunterCurseType>();
                List<HunterCurseType> cursesToProcess = new List<HunterCurseType>(_playerCurses.Keys);
                
                foreach (var curse in cursesToProcess)
                {
                    // Apply the curse effect to this room
                    StartCoroutine(ApplyCurseEffect(curse));
                    
                    // Tick down the charge
                    _playerCurses[curse]--;
                    
                    // Track curse endurance for scoring
                    IncrementCursesEndured();
                    
                    // Check if expired
                    if (_playerCurses[curse] <= 0)
                    {
                        expiredCurses.Add(curse);
                    }
                }
                
                // Remove expired curses and clean up their effects
                foreach (var curse in expiredCurses)
                {
                    _playerCurses.Remove(curse);
                    SendChatMessage(DungeonLocalization.CreateMessageKey("CURSE_EXPIRED"));
                }
                
                // If any curses expired, clear all debuffs/buffs and reapply remaining ones
                // This ensures expired curse effects are properly removed
                if (expiredCurses.Count > 0)
                {
                    RemoveAllCurseDebuffs();
                    
                    // Reapply effects for remaining active curses (without ticking)
                    foreach (var curse in _playerCurses.Keys)
                    {
                        StartCoroutine(ApplyCurseEffect(curse));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in ProcessPlayerCurses: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Apply the effect of a curse to the current room
        /// 
        /// CURSE CATEGORIES:
        /// ================
        /// ENVIRONMENTAL DAMAGE (spawn room modifiers - these only hurt players):
        /// - MeteorFury, InkStorm, FlameEngulf, ToxicMiasma, BlackRain
        /// 
        /// PLAYER DEBUFFS (apply stat penalties to players only):
        /// - DarkVeil, GravityCurse, WeakenedSpirit, SlowMind, Vulnerability
        /// 
        /// MONSTER BUFFS (apply stat bonuses to monsters only):
        /// - TimewarpCurse, MonsterRage, IronHide, Frenzy
        /// </summary>
        private IEnumerator ApplyCurseEffect(HunterCurseType curse)
        {
            // Wait for room to be ready
            yield return new WaitForSeconds(1.5f);
            
            Room room = SingletonDewNetworkBehaviour<Room>.instance;
            if (room == null) yield break;
            
            switch (curse)
            {
                // === ENVIRONMENTAL DAMAGE CURSES (room modifiers that hurt players) ===
                case HunterCurseType.MeteorFury:
                case HunterCurseType.InkStorm:
                case HunterCurseType.FlameEngulf:
                case HunterCurseType.ToxicMiasma:
                case HunterCurseType.BlackRain:
                    // These room modifiers spawn hazards that damage players
                    SpawnCurseRoomModifier(curse);
                    break;
                    
                // === PLAYER DEBUFF CURSES (stat penalties to players only) ===
                case HunterCurseType.DarkVeil:
                    ApplyDarkVeilDebuffToPlayers();
                    break;
                case HunterCurseType.GravityCurse:
                    ApplyGravityDebuffToPlayers();
                    break;
                case HunterCurseType.WeakenedSpirit:
                    ApplyWeakenedSpiritDebuffToPlayers();
                    break;
                case HunterCurseType.SlowMind:
                    ApplySlowMindDebuffToPlayers();
                    break;
                case HunterCurseType.Vulnerability:
                    ApplyVulnerabilityDebuffToPlayers();
                    break;
                    
                // === MONSTER BUFF CURSES (stat bonuses to monsters only) ===
                case HunterCurseType.TimewarpCurse:
                    ApplyTimewarpBuffToMonsters();
                    break;
                case HunterCurseType.MonsterRage:
                    ApplyMonsterRageBuffToMonsters();
                    break;
                case HunterCurseType.IronHide:
                    ApplyIronHideBuffToMonsters();
                    break;
                case HunterCurseType.Frenzy:
                    ApplyFrenzyBuffToMonsters();
                    break;
            }
        }
        
        /// <summary>
        /// Spawn a room modifier for environmental damage curses
        /// These modifiers spawn hazards that hurt players (meteors, ink, fire)
        /// Uses the game's proper AddModifier API to avoid crashes
        /// </summary>
        private void SpawnCurseRoomModifier(HunterCurseType curse)
        {
            string modifierTypeName = GetModifierTypeForCurse(curse);
            if (string.IsNullOrEmpty(modifierTypeName)) return;
            
            try
            {
                if (!NetworkServer.active) return;
                
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null) return;
                
                int currentNode = zm.currentNodeIndex;
                if (currentNode < 0 || currentNode >= zm.nodes.Count) return;
                
                Room room = SingletonDewNetworkBehaviour<Room>.instance;
                if (room == null || room.modifiers == null) return;
                
                // Check if modifier already exists in the room
                if (room.modifiers.modifierInstances != null)
                {
                    foreach (var mod in room.modifiers.modifierInstances)
                    {
                        if (mod != null && mod.GetType().Name == modifierTypeName)
                        {
                            return; // Already exists
                        }
                    }
                }
                
                // Use reflection to call the generic AddModifier<T> method with the correct type
                Type modifierType = Type.GetType(modifierTypeName + ", Assembly-CSharp");
                if (modifierType == null)
                {
                    // Try without namespace
                    modifierType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.Name == modifierTypeName);
                }
                
                if (modifierType == null)
                {
                    Debug.LogWarning("[InfiniteDungeon] Could not find modifier type: " + modifierTypeName);
                    return;
                }
                
                // Use the game's AddModifier method which handles everything properly
                _allowModifierAddition = true;
                try
                {
                    var addModifierMethod = typeof(ZoneManager).GetMethods()
                        .FirstOrDefault(m => m.Name == "AddModifier" && m.IsGenericMethod);
                    
                    if (addModifierMethod != null)
                    {
                        var genericMethod = addModifierMethod.MakeGenericMethod(modifierType);
                        genericMethod.Invoke(zm, new object[] { currentNode });
                    }
                }
                finally
                {
                    _allowModifierAddition = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error spawning curse modifier: " + ex.Message);
                _allowModifierAddition = false;
            }
        }
        
        // ============================================================
        // PLAYER DEBUFF METHODS (stat penalties to players only)
        // ============================================================
        
        /// <summary>
        /// Helper to apply a stat bonus to a player and track it for removal
        /// </summary>
        private void ApplyTrackedDebuffToPlayer(Hero hero, StatBonus debuff)
        {
            if (hero == null) return;
            
            hero.Status.AddStatBonus(debuff);
            
            // Track for removal when leaving room
            if (!_appliedCurseDebuffs.ContainsKey(hero))
            {
                _appliedCurseDebuffs[hero] = new List<StatBonus>();
            }
            _appliedCurseDebuffs[hero].Add(debuff);
        }
        
        /// <summary>
        /// Remove all tracked curse debuffs from players (called when leaving room)
        /// </summary>
        public static void RemoveAllCurseDebuffs()
        {
            try
            {
                int removed = 0;
                foreach (var kvp in _appliedCurseDebuffs)
                {
                    Hero hero = kvp.Key;
                    if (hero == null || hero.Status == null) continue;
                    
                    foreach (StatBonus debuff in kvp.Value)
                    {
                        hero.Status.RemoveStatBonus(debuff);
                        removed++;
                    }
                }
                _appliedCurseDebuffs.Clear();
                
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error removing curse debuffs: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Apply dark veil debuff to PLAYERS ONLY (reduced crit/damage)
        /// </summary>
        private void ApplyDarkVeilDebuffToPlayers()
        {
            foreach (var player in DewPlayer.gamePlayers)
            {
                if (player != null && player.hero != null && !player.hero.IsNullInactiveDeadOrKnockedOut())
                {
                    StatBonus debuff = new StatBonus
                    {
                        critChanceFlat = -15f,
                        attackDamagePercentage = -15f,
                    };
                    ApplyTrackedDebuffToPlayer(player.hero, debuff);
                }
            }
        }
        
        /// <summary>
        /// Apply gravity debuff to PLAYERS ONLY (reduced movement speed)
        /// </summary>
        private void ApplyGravityDebuffToPlayers()
        {
            foreach (var player in DewPlayer.gamePlayers)
            {
                if (player != null && player.hero != null && !player.hero.IsNullInactiveDeadOrKnockedOut())
                {
                    StatBonus debuff = new StatBonus
                    {
                        movementSpeedPercentage = -25f, // -25% move speed
                    };
                    ApplyTrackedDebuffToPlayer(player.hero, debuff);
                }
            }
        }
        
        /// <summary>
        /// Apply weakened spirit debuff to PLAYERS ONLY (reduced max health)
        /// </summary>
        private void ApplyWeakenedSpiritDebuffToPlayers()
        {
            foreach (var player in DewPlayer.gamePlayers)
            {
                if (player != null && player.hero != null && !player.hero.IsNullInactiveDeadOrKnockedOut())
                {
                    StatBonus debuff = new StatBonus
                    {
                        maxHealthPercentage = -15f, // -15% max health
                    };
                    ApplyTrackedDebuffToPlayer(player.hero, debuff);
                }
            }
        }
        
        /// <summary>
        /// Apply slow mind debuff to PLAYERS ONLY (increased cooldowns)
        /// </summary>
        private void ApplySlowMindDebuffToPlayers()
        {
            foreach (var player in DewPlayer.gamePlayers)
            {
                if (player != null && player.hero != null && !player.hero.IsNullInactiveDeadOrKnockedOut())
                {
                    StatBonus debuff = new StatBonus
                    {
                        abilityHasteFlat = -30f, // -30 ability haste (increases cooldowns)
                    };
                    ApplyTrackedDebuffToPlayer(player.hero, debuff);
                }
            }
        }
        
        /// <summary>
        /// Apply vulnerability debuff to PLAYERS ONLY (reduced armor)
        /// </summary>
        private void ApplyVulnerabilityDebuffToPlayers()
        {
            foreach (var player in DewPlayer.gamePlayers)
            {
                if (player != null && player.hero != null && !player.hero.IsNullInactiveDeadOrKnockedOut())
                {
                    StatBonus debuff = new StatBonus
                    {
                        armorFlat = -30f, // -30 armor
                    };
                    ApplyTrackedDebuffToPlayer(player.hero, debuff);
                }
            }
        }
        
        // ============================================================
        // MONSTER BUFF METHODS (stat bonuses to monsters only)
        // ============================================================
        
        /// <summary>
        /// Apply timewarp buff to MONSTERS ONLY (increased speed)
        /// </summary>
        private void ApplyTimewarpBuffToMonsters()
        {
            StartCoroutine(ApplyBuffToMonstersCoroutine(new StatBonus
            {
                movementSpeedPercentage = 35f,
                attackSpeedPercentage = 35f,
                abilityHasteFlat = 20f,
            }, "timewarp"));
        }
        
        /// <summary>
        /// Apply monster rage buff to MONSTERS ONLY (increased damage)
        /// </summary>
        private void ApplyMonsterRageBuffToMonsters()
        {
            StartCoroutine(ApplyBuffToMonstersCoroutine(new StatBonus
            {
                attackDamagePercentage = 40f,
                abilityPowerPercentage = 40f,
            }, "monster rage"));
        }
        
        /// <summary>
        /// Apply iron hide buff to MONSTERS ONLY (increased armor)
        /// </summary>
        private void ApplyIronHideBuffToMonsters()
        {
            StartCoroutine(ApplyBuffToMonstersCoroutine(new StatBonus
            {
                armorFlat = 50f,
                maxHealthPercentage = 20f,
            }, "iron hide"));
        }
        
        /// <summary>
        /// Apply frenzy buff to MONSTERS ONLY (increased attack speed)
        /// </summary>
        private void ApplyFrenzyBuffToMonsters()
        {
            StartCoroutine(ApplyBuffToMonstersCoroutine(new StatBonus
            {
                attackSpeedPercentage = 50f,
                movementSpeedPercentage = 20f,
            }, "frenzy"));
        }
        
        /// <summary>
        /// Generic coroutine to apply a buff to all monsters
        /// </summary>
        private IEnumerator ApplyBuffToMonstersCoroutine(StatBonus buff, string buffName)
        {
            yield return new WaitForSeconds(2f);
            
            if (!NetworkServer.active) yield break;
            
            try
            {
                Monster[] monsters = UnityEngine.Object.FindObjectsOfType<Monster>();
                int buffedCount = 0;
                foreach (var monster in monsters)
                {
                    if (monster != null && monster.Status != null && !monster.IsNullInactiveDeadOrKnockedOut())
                    {
                        try
                        {
                            monster.Status.AddStatBonus(buff);
                            buffedCount++;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error applying buff to monsters: " + ex.Message);
            }
        }
        
        // ============================================================
        // CURSE HELPER METHODS
        // ============================================================
        
        /// <summary>
        /// Get the room modifier type for ENVIRONMENTAL DAMAGE curses only.
        /// These spawn hazards that only hurt players.
        /// </summary>
        private string GetModifierTypeForCurse(HunterCurseType curse)
        {
            switch (curse)
            {
                // Environmental damage modifiers (spawn hazards that hurt players)
                case HunterCurseType.MeteorFury: return "RoomMod_RiskOfMeteors";
                case HunterCurseType.InkStorm: return "RoomMod_InkStrikeWarning";
                case HunterCurseType.FlameEngulf: return "RoomMod_EngulfedInFlame";
                case HunterCurseType.ToxicMiasma: return "RoomMod_ToxicArea";
                case HunterCurseType.BlackRain: return "RoomMod_BlackRain";
                
                // Player debuffs and monster buffs use custom stat bonuses (not room modifiers)
                default: return null;
            }
        }
        
        /// <summary>
        /// Get the UI color for a curse type
        /// </summary>
        private string GetCurseColor(HunterCurseType curse)
        {
            switch (curse)
            {
                // Environmental Damage
                case HunterCurseType.MeteorFury: return "#ff6600";
                case HunterCurseType.InkStorm: return "#4444ff";
                case HunterCurseType.FlameEngulf: return "#ff4400";
                case HunterCurseType.ToxicMiasma: return "#44ff44";
                case HunterCurseType.BlackRain: return "#445566";
                
                // Player Debuffs
                case HunterCurseType.DarkVeil: return "#8844ff";
                case HunterCurseType.GravityCurse: return "#9988ff";
                case HunterCurseType.WeakenedSpirit: return "#ff8888";
                case HunterCurseType.SlowMind: return "#88aaff";
                case HunterCurseType.Vulnerability: return "#ffaaaa";
                
                // Monster Buffs
                case HunterCurseType.TimewarpCurse: return "#00ffff";
                case HunterCurseType.MonsterRage: return "#ff4444";
                case HunterCurseType.IronHide: return "#aaaaaa";
                case HunterCurseType.Frenzy: return "#ff8800";
                
                default: return "#ffffff";
            }
        }
        
        // ============================================================
        // INFECTION STATUS UI
        // ============================================================
        
        /// <summary>
        /// Create or update the infection status UI at the top of the screen
    }
}
