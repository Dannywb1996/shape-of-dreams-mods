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
        // ==================== WEAKENED BOSS SYSTEM ====================
        /// Spawn a miniboss for Pot of Greed
        /// </summary>
        private IEnumerator SpawnPotOfGreedMiniBoss()
        {
            // Wait for room to be fully loaded and monsters to spawn
            yield return new WaitForSeconds(2.0f);
            
            Room room = SingletonDewNetworkBehaviour<Room>.instance;
            if (room == null) yield break;
            
            // Wait for initial monsters to spawn
            yield return new WaitForSeconds(1.0f);
            
            try
            {
                // Find the section closest to the exit portal
                RoomSection targetSection = null;
                Rift_RoomExit exitPortal = Rift_RoomExit.instance;
                
                if (exitPortal != null)
                {
                    float bestDist = float.MaxValue;
                    foreach (RoomSection section in room.sections)
                    {
                        float dist = Vector2.Distance(exitPortal.transform.position.ToXY(), section.transform.position.ToXY());
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            targetSection = section;
                        }
                    }
                }
                
                if (targetSection == null && room.sections.Count > 0)
                {
                    targetSection = room.sections[room.sections.Count - 1];
                }
                
                if (targetSection != null)
                {
                    // Spawn the miniboss
                    SpawnMonsterSettings settings = new SpawnMonsterSettings
                    {
                        section = targetSection,
                        random = room.GetRoomRandom(-98765)
                    };
                    
                    room.monsters.SpawnMiniBoss(settings);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error spawning Pot of Greed miniboss: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Called when a player uses Pot of Greed shrine
        /// </summary>
        public static void OnPotOfGreedUsed(string playerGuid)
        {
            if (!IsModActive) return;
            
            _potOfGreedPlayers.Add(playerGuid);
            _pendingMiniBossSpawn = true;
        }
        
        /// <summary>
        /// Try to spawn a weakened boss in this combat room
        /// These bosses (Forest Demon, Nyx, Skoll) work fine in normal combat rooms
        /// </summary>
        private void TrySpawnWeakenedBoss()
        {
            if (_dungeonRandom == null) return;
            if (WeakenedBosses.Length == 0) return;
            
            // Roll for weakened boss chance
            if (_dungeonRandom.Value() >= WEAKENED_BOSS_CHANCE) return;
            
            // Pick a random boss
            string bossName = WeakenedBosses[_dungeonRandom.Range(0, WeakenedBosses.Length)];
            
            // Mark this node as a weakened boss node
            ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
            if (zm == null) return;
            
            int currentNode = zm.currentNodeIndex;
            _weakenedBossNodes.Add(currentNode);
            
            // Announce the encounter
            // Send localized boss prompt - each client resolves to their language
            SendChatMessage(DungeonLocalization.CreateMessageKey("BOSS_PROMPT", bossName));
            
            // Spawn the boss directly in the current room (these bosses work in normal rooms)
            StartCoroutine(SpawnWeakenedBossCoroutine(bossName));
        }
        
        /// <summary>
        /// Called when room loads - check if we need to spawn a weakened boss (legacy, kept for compatibility)
        /// </summary>
        public void CheckAndSpawnWeakenedBoss()
        {
            // No longer needed since bosses spawn directly, but kept for compatibility
            if (string.IsNullOrEmpty(_pendingWeakenedBossName)) return;
            
            string bossName = _pendingWeakenedBossName;
            _pendingWeakenedBossName = null;
            
            StartCoroutine(SpawnWeakenedBossCoroutine(bossName));
        }
        
        /// <summary>
        /// Coroutine to spawn a weakened boss in the current room
        /// These bosses (Forest Demon, Nyx, Skoll) work fine in normal combat rooms
        /// </summary>
        private IEnumerator SpawnWeakenedBossCoroutine(string bossName)
        {
            // Wait for room to fully load and settle
            yield return new WaitForSeconds(1.5f);
            
            Room room = SingletonDewNetworkBehaviour<Room>.instance;
            if (room == null) yield break;
            
            // Get the boss prefab
            Monster bossPrefab = DewResources.GetByShortTypeName<Monster>(bossName);
            if (bossPrefab == null)
            {
                Debug.LogWarning("[InfiniteDungeon] Could not find boss prefab: " + bossName);
                yield break;
            }
            
            // Find spawn position - use center of room or first section
            Vector3 spawnPos = room.transform.position;
            if (room.sections.Count > 0)
            {
                spawnPos = room.sections[0].transform.position;
            }
            spawnPos = Dew.GetPositionOnGround(spawnPos);
            
            // Wait a moment for dramatic effect
            yield return new WaitForSeconds(0.5f);
            
            // Spawn the weakened boss
            SpawnWeakenedBossActual(bossName, spawnPos);
        }
        
        /// <summary>
        /// Actually spawn the weakened boss (separated from coroutine to avoid try-catch yield issues)
        /// </summary>
        private void SpawnWeakenedBossActual(string bossName, Vector3 spawnPos)
        {
            try
            {
                // Get the specific boss prefab by name
                Monster bossPrefab = DewResources.GetByShortTypeName<Monster>(bossName);
                if (bossPrefab == null)
                {
                    Debug.LogError("[InfiniteDungeon] Could not find boss prefab for spawn: " + bossName);
                    return;
                }
                
                // Create the boss using the specific prefab
                Monster boss = Dew.CreateActor<Monster>(bossPrefab, spawnPos, null, null, delegate(Monster b)
                {
                    // Apply weakening effects BEFORE the boss is fully initialized
                    ApplyWeakeningToBoss(b);
                });
                
                if (boss != null)
                {
                    // Add death listener to open exit rifts when the weakened boss is defeated
                    boss.EntityEvent_OnDeath += delegate(EventInfoKill info)
                    {
                        InfiniteDungeonMod.IncrementWeakenedBossesDefeated();
                        StartCoroutine(OpenExitRiftsAfterWeakenedBoss());
                    };
                }
                else
                {
                    Debug.LogWarning("[InfiniteDungeon] Boss spawn returned null!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error spawning weakened boss: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
        
        /// <summary>
        /// Open exit rifts after defeating a weakened boss
        /// Since we loaded a boss arena room but it's not a real boss fight, we need to manually open rifts
        /// </summary>
        private IEnumerator OpenExitRiftsAfterWeakenedBoss()
        {
            // Wait a moment for death animation
            yield return new WaitForSeconds(1.5f);
            
            try
            {
                Room room = SingletonDewNetworkBehaviour<Room>.instance;
                if (room == null)
                {
                    Debug.LogError("[InfiniteDungeon] Room is null when trying to open rifts!");
                    yield break;
                }
                
                // Send victory message
                SendChatMessage(DungeonLocalization.CreateMessageKey("BOSS_DEFEATED"));
                
                // Find and open all exit rifts in the room
                RoomRifts rifts = room.GetComponent<RoomRifts>();
                if (rifts != null)
                {
                    // Use reflection to call the open rifts method
                    var openMethod = typeof(RoomRifts).GetMethod("OpenExitRifts", 
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    
                    if (openMethod != null)
                    {
                        openMethod.Invoke(rifts, null);
                    }
                    else
                    {
                        // Fallback: Try to find and activate rift objects directly
                        foreach (var rift in UnityEngine.Object.FindObjectsOfType<Rift_RoomExit>())
                        {
                            if (rift != null && !rift.isOpen)
                            {
                                rift.Open();
                            }
                        }
                    }
                }
                else
                {
                    // No RoomRifts component, try direct rift activation
                    foreach (var rift in UnityEngine.Object.FindObjectsOfType<Rift_RoomExit>())
                    {
                        if (rift != null && !rift.isOpen)
                        {
                            rift.Open();
                        }
                    }
                }
                
                // Also mark the room as cleared so the game knows
                if (room.didClearRoom == false)
                {
                    // Use reflection to set didClearRoom
                    var didClearField = typeof(Room).GetField("didClearRoom", 
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (didClearField != null)
                    {
                        didClearField.SetValue(room, true);
                    }
                    
                    // Invoke room clear event
                    room.onRoomClear.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error opening exit rifts: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Apply weakening effects to a boss (smaller size, reduced stats, no soul reward)
        /// </summary>
        private void ApplyWeakeningToBoss(Monster boss)
        {
            if (boss == null) return;
            
            try
            {
                // Make the boss smaller
                boss.Visual.GetNewTransformModifier().scaleMultiplier = Vector3.one * WEAKENED_BOSS_SCALE;
                boss.Control.outerRadius *= WEAKENED_BOSS_SCALE;
                boss.Control.innerRadius *= WEAKENED_BOSS_SCALE;
                
                // Reduce stats significantly
                StatBonus debuff = new StatBonus
                {
                    maxHealthPercentage = (WEAKENED_BOSS_HEALTH_MULT - 1f) * 100f, // -65% health
                    abilityPowerPercentage = (WEAKENED_BOSS_DAMAGE_MULT - 1f) * 100f, // -50% ability power
                    attackDamagePercentage = (WEAKENED_BOSS_DAMAGE_MULT - 1f) * 100f, // -50% attack damage
                };
                boss.Status.AddStatBonus(debuff);
                
                // CRITICAL: Skip boss soul reward for weakened bosses!
                // This prevents the Shrine_BossSoul from spawning (no skill upgrade)
                // Other drops (gold, items, custom items) are still allowed
                BossMonster bossMonster = boss as BossMonster;
                if (bossMonster != null)
                {
                    bossMonster.NetworkskipBossSoulFlow = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error applying weakening to boss: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Clear infection from a node when it's been cleared.
        /// This removes the infection status and the RoomMod_Hunted modifier so it doesn't respawn on revisit.
        /// IMPORTANT: This is called from onRoomClear which fires on all clients, so we must check NetworkServer.active!
        /// </summary>
        private void ClearNodeInfection(int nodeIndex)
        {
            // CRITICAL: Only server can modify SyncLists (hunterStatuses, nodes)!
            // onRoomClear fires on all clients, so we must check here
            if (!NetworkServer.active) return;
            
            try
            {
                if (_hunterInfectedNodes.Contains(nodeIndex))
                {
                    _hunterInfectedNodes.Remove(nodeIndex);
                    
                    ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                    if (zm != null)
                    {
                        // Clear the game's hunter status
                        if (nodeIndex < zm.hunterStatuses.Count)
                    {
                        zm.hunterStatuses[nodeIndex] = HunterStatus.None;
                    }
                    
                        // Remove RoomMod_Hunted from node's modifier data so it doesn't respawn on revisit
                        if (nodeIndex >= 0 && nodeIndex < zm.nodes.Count)
                        {
                            var nodeData = zm.nodes[nodeIndex];
                            nodeData.modifiers.RemoveAll(m => m.type == "RoomMod_Hunted");
                        }
                        
                        // Use ZoneManager's RemoveModifier to properly clean up the active instance
                        try
                        {
                            zm.RemoveModifier<RoomMod_Hunted>(nodeIndex);
                        }
                        catch { }
                    }
                    
                    SendChatMessage(DungeonLocalization.CreateMessageKey("HUNTER_PURIFIED"));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error clearing node infection: " + ex.Message);
            }
        }
    }
}
