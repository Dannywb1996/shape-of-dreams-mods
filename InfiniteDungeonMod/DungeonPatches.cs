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
    // ==================== HARMONY PATCHES ====================
    // ==================== HARMONY PATCHES ====================
    
    /// <summary>
    /// Intercept chat messages and localize them based on each client's language setting.
    /// Messages with [IDM:KEY] format are resolved to the local player's language.
    /// </summary>
    [HarmonyPatch(typeof(ChatManager), "ShowMessageLocally")]
    public static class LocalizeChatMessagePatch
    {
        static void Prefix(ref ChatManager.Message message)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            
            try
            {
                // Check if this is a localized message key
                if (DungeonLocalization.IsLocalizedMessage(message.content))
                {
                    // Resolve the key to the local player's language
                    message.content = DungeonLocalization.ResolveMessageKey(message.content);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error localizing chat message: " + ex.Message);
            }
        }
    }
    
    [HarmonyPatch(typeof(ZoneManager), "LoadNextZoneByContentSettings")]
    public static class PreventZoneAdvancementPatch
    {
        static bool Prefix()
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
            if (zm == null || zm.currentZone == null)
            {
                return true;
            }
            
            return false;
        }
    }
    
    [HarmonyPatch(typeof(UI_InGame_Location), "OnRoomLoaded")]
    public static class ZoneIndexDisplayPatch
    {
        static void Postfix(UI_InGame_Location __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            
            try
            {
                var textField = AccessTools.Field(typeof(UI_InGame_Location), "_text");
                if (textField != null)
                {
                    var textComponent = textField.GetValue(__instance) as TMPro.TextMeshProUGUI;
                    if (textComponent != null)
                    {
                        // Get current depth and check if we need to show incremented value
                        int depth = InfiniteDungeonMod.GetTotalDepth();
                        
                        // Check if this is a NEW combat/boss node that will increment depth
                        // The UI fires before our OnRoomLoaded, so we need to predict the depth
                        ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                        if (zm != null)
                        {
                            int currentNode = zm.currentNodeIndex;
                            if (currentNode >= 0 && currentNode < zm.nodes.Count)
                            {
                                WorldNodeType nodeType = zm.nodes[currentNode].type;
                                bool isNewNode = !InfiniteDungeonMod.IsNodeVisited(currentNode);
                                bool isCombatOrBoss = (nodeType == WorldNodeType.Combat || nodeType == WorldNodeType.ExitBoss);
                                
                                // If this is a new combat/boss node, show the NEXT depth value
                                if (isNewNode && isCombatOrBoss)
                                {
                                    depth = depth + 1;
                                }
                            }
                        }
                        
                        string zoneName = DungeonLocalization.FallbackZoneName;
                        Zone nodeZone = InfiniteDungeonMod.GetNodeZone(
                            NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex);
                        if (nodeZone != null)
                        {
                            zoneName = DewLocalization.GetUIValue(nodeZone.name + "_Name");
                        }
                        
                        textComponent.text = string.Format(DungeonLocalization.DepthLocationFormat, depth, zoneName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error updating location display: " + ex.Message);
            }
        }
    }
    
    [HarmonyPatch(typeof(DewDifficultySettings), "GetScaledZoneIndexForDamage")]
    public static class DifficultyScalingDamagePatch
    {
        static bool Prefix(ref float __result, float zi)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            if (float.IsNaN(zi))
            {
                int depth = InfiniteDungeonMod.GetTotalDepth();
                __result = Mathf.Sqrt(depth) * 0.5f;
                return false;
            }
            
            return true;
        }
    }
    
    [HarmonyPatch(typeof(DewDifficultySettings), "GetScaledZoneIndexForHealth")]
    public static class DifficultyScalingHealthPatch
    {
        static bool Prefix(ref float __result, float zi)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            if (float.IsNaN(zi))
            {
                int depth = InfiniteDungeonMod.GetTotalDepth();
                __result = Mathf.Sqrt(depth) * 0.5f;
                return false;
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// Add depth-based scaling for monsters: armor, move speed, and attack speed.
    /// All values are configurable via mod settings in the mod loader.
    /// 
    /// DEFAULT VALUES:
    /// - Move Speed: starts depth 10, +2%/depth, caps at 150%
    /// - Attack Speed: starts depth 20, +2.5%/depth, caps at 200%
    /// - Armor: (depth * 0.5) + (depth^0.7 * 2), with type multipliers
    /// </summary>
    [HarmonyPatch(typeof(DewDifficultySettings), "ApplyDifficultyModifiers")]
    public static class MonsterDepthScalingPatch
    {
        static void Postfix(DewDifficultySettings __instance, Entity entity)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            
            // Only apply to monsters
            Monster monster = entity as Monster;
            if (monster == null) return;
            
            try
            {
                // Get config values (with fallback defaults)
                var config = InfiniteDungeonMod.Instance != null ? InfiniteDungeonMod.Instance.config : null;
                
                // Speed config
                int moveSpeedStartDepth = config != null ? config.moveSpeedStartDepth : 10;
                float moveSpeedPerDepth = config != null ? config.moveSpeedPerDepth : 2.0f;
                float moveSpeedCap = config != null ? (float)config.moveSpeedCap : 150f;
                
                int attackSpeedStartDepth = config != null ? config.attackSpeedStartDepth : 20;
                float attackSpeedPerDepth = config != null ? config.attackSpeedPerDepth : 2.5f;
                float attackSpeedCap = config != null ? (float)config.attackSpeedCap : 200f;
                
                // Health scaling config (now with exponent)
                float healthMultiplier = config != null ? config.monsterHealthMultiplier : 1.0f;
                float healthPerDepth = config != null ? config.healthPerDepth : 4.0f;
                int healthStartDepth = config != null ? config.healthStartDepth : 3;
                float healthExponent = config != null ? config.healthExponent : 1.3f;
                
                // Damage scaling config (now with exponent)
                float damageMultiplier = config != null ? config.monsterDamageMultiplier : 1.0f;
                float damagePerDepth = config != null ? config.damagePerDepth : 3.0f;
                int damageStartDepth = config != null ? config.damageStartDepth : 1;
                float damageExponent = config != null ? config.damageExponent : 1.2f;
                
                // Armor scaling config (now configurable per depth)
                float armorPerDepth = config != null ? config.armorPerDepth : 1.5f;
                int armorStartDepth = config != null ? config.armorStartDepth : 5;
                float armorExponent = config != null ? config.armorExponent : 1.1f;
                float armorScaling = config != null ? config.armorScaling : 1.0f;
                
                int depth = InfiniteDungeonMod.GetTotalDepth();
                if (depth < 1) depth = 1;
                
                // ========== DAMAGE SCALING (EXPONENTIAL) ==========
                // Formula: damagePerDepth * (depthDelta ^ damageExponent)
                // Example at depth 50 with defaults (3.0, 1.2): 3.0 * (49^1.2) = ~300% bonus
                // Example at depth 100: 3.0 * (99^1.2) = ~700% bonus
                float damageBonus = 0f;
                if (depth > damageStartDepth && damagePerDepth > 0)
                {
                    int depthDelta = depth - damageStartDepth;
                    damageBonus = damagePerDepth * Mathf.Pow(depthDelta, damageExponent);
                }
                
                // Apply base multiplier for damage
                float totalDamageMultiplier = damageMultiplier;
                if (damageBonus > 0)
                {
                    totalDamageMultiplier += damageBonus / 100f; // Convert % to multiplier
                }
                
                // Monster type adjustments for damage
                switch (monster.type)
                {
                    case Monster.MonsterType.Lesser:
                        totalDamageMultiplier *= 0.7f;  // Lesser monsters deal less damage bonus
                        break;
                    case Monster.MonsterType.Normal:
                        // Standard scaling
                        break;
                    case Monster.MonsterType.MiniBoss:
                        totalDamageMultiplier *= 1.15f;  // Mini-bosses deal more damage
                        break;
                    case Monster.MonsterType.Boss:
                        totalDamageMultiplier *= 1.3f;  // Bosses deal much more damage
                        break;
                }
                
                // ========== HEALTH SCALING (EXPONENTIAL) ==========
                // Formula: healthPerDepth * (depthDelta ^ healthExponent)
                // Example at depth 50 with defaults (4.0, 1.3): 4.0 * (47^1.3) = ~600% bonus
                // Example at depth 100: 4.0 * (97^1.3) = ~1500% bonus
                float healthBonus = 0f;
                if (depth > healthStartDepth && healthPerDepth > 0)
                {
                    int depthDelta = depth - healthStartDepth;
                    healthBonus = healthPerDepth * Mathf.Pow(depthDelta, healthExponent);
                }
                
                // Apply base multiplier
                float totalHealthMultiplier = healthMultiplier;
                if (healthBonus > 0)
                {
                    totalHealthMultiplier += healthBonus / 100f; // Convert % to multiplier
                }
                
                // Monster type adjustments for health
                switch (monster.type)
                {
                    case Monster.MonsterType.Lesser:
                        totalHealthMultiplier *= 0.7f;  // Lesser monsters get less health bonus
                        break;
                    case Monster.MonsterType.Normal:
                        // Standard scaling
                        break;
                    case Monster.MonsterType.MiniBoss:
                        totalHealthMultiplier *= 1.2f;  // Mini-bosses get more health
                        break;
                    case Monster.MonsterType.Boss:
                        totalHealthMultiplier *= 1.5f;  // Bosses get much more health
                        break;
                }
                
                // ========== ARMOR SCALING (CONFIGURABLE EXPONENTIAL) ==========
                // Formula: armorPerDepth * (depthDelta ^ armorExponent) * armorScaling
                // Example at depth 50 with defaults (1.5, 1.1): 1.5 * (45^1.1) * 1.0 = ~100 armor
                // Example at depth 100: 1.5 * (95^1.1) * 1.0 = ~230 armor
                float depthArmor = 0f;
                if (depth > armorStartDepth && armorPerDepth > 0 && armorScaling > 0)
                {
                    int depthDelta = depth - armorStartDepth;
                    depthArmor = armorPerDepth * Mathf.Pow(depthDelta, armorExponent) * armorScaling;
                }
                
                // Monster type multipliers for armor
                switch (monster.type)
                {
                    case Monster.MonsterType.Lesser:
                        depthArmor *= 0.5f; // Half armor scaling
                        break;
                    case Monster.MonsterType.Normal:
                        // Standard scaling
                        break;
                    case Monster.MonsterType.MiniBoss:
                        depthArmor *= 1.5f; // 50% more armor scaling
                        break;
                    case Monster.MonsterType.Boss:
                        depthArmor *= 2f; // Double armor scaling
                        break;
                }
                
                // ========== MOVE SPEED SCALING ==========
                float moveSpeedBonus = 0f;
                if (depth > moveSpeedStartDepth)
                {
                    moveSpeedBonus = (depth - moveSpeedStartDepth) * moveSpeedPerDepth;
                    moveSpeedBonus = Mathf.Min(moveSpeedBonus, moveSpeedCap);
                }
                
                // ========== ATTACK SPEED SCALING ==========
                float attackSpeedBonus = 0f;
                if (depth > attackSpeedStartDepth)
                {
                    attackSpeedBonus = (depth - attackSpeedStartDepth) * attackSpeedPerDepth;
                    attackSpeedBonus = Mathf.Min(attackSpeedBonus, attackSpeedCap);
                }
                
                // Monster type adjustments for speed (bosses slightly slower scaling)
                switch (monster.type)
                {
                    case Monster.MonsterType.Lesser:
                        moveSpeedBonus *= 0.7f;   // Lesser monsters are still slower
                        attackSpeedBonus *= 0.7f;
                        break;
                    case Monster.MonsterType.Normal:
                        // Standard scaling
                        break;
                    case Monster.MonsterType.MiniBoss:
                        moveSpeedBonus *= 0.85f;  // Mini-bosses slightly slower (they're already tough)
                        attackSpeedBonus *= 0.85f;
                        break;
                    case Monster.MonsterType.Boss:
                        moveSpeedBonus *= 0.7f;   // Bosses are slower (they have other mechanics)
                        attackSpeedBonus *= 0.7f;
                        break;
                }
                
                // ========== APPLY HEALTH MULTIPLIER ==========
                // Health is applied differently - we modify maxHealth directly
                if (totalHealthMultiplier > 1.0f)
                {
                    float currentMaxHealth = entity.Status.maxHealth;
                    
                    // Use stat bonus for health percentage increase
                    float healthPercentIncrease = (totalHealthMultiplier - 1.0f) * 100f;
                    
                    StatBonus healthStatBonus = new StatBonus();
                    healthStatBonus.maxHealthPercentage = healthPercentIncrease;
                    entity.Status.AddStatBonus(healthStatBonus);
                    
                    // Also heal to full if this is initial spawn
                    if (entity.Status.currentHealth >= currentMaxHealth * 0.99f)
                    {
                        entity.Status.SetHealth(entity.Status.maxHealth);
                    }
                }
                
                // ========== APPLY DAMAGE MULTIPLIER ==========
                if (totalDamageMultiplier > 1.0f)
                {
                    float damagePercentIncrease = (totalDamageMultiplier - 1.0f) * 100f;
                    
                    StatBonus damageStatBonus = new StatBonus();
                    damageStatBonus.attackDamagePercentage = damagePercentIncrease;
                    entity.Status.AddStatBonus(damageStatBonus);
                }
                
                // ========== APPLY OTHER BONUSES ==========
                StatBonus bonus = new StatBonus();
                
                if (depthArmor > 0)
                {
                    bonus.armorFlat = depthArmor;
                }
                
                if (moveSpeedBonus > 0)
                {
                    bonus.movementSpeedPercentage = moveSpeedBonus;
                }
                
                if (attackSpeedBonus > 0)
                {
                    bonus.attackSpeedPercentage = attackSpeedBonus;
                }
                
                // Only add if we have any bonuses
                if (depthArmor > 0 || moveSpeedBonus > 0 || attackSpeedBonus > 0)
                {
                    entity.Status.AddStatBonus(bonus);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error applying depth scaling: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Block the default hunter advance system - we use our own custom infection system
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), "AdvanceHunterTurn")]
    public static class BlockHunterAdvancePatch
    {
        static bool Prefix()
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            // Block the default hunter following mechanic entirely
            // Our custom system handles hunter infection via TryInfectAdjacentNodes
            return false;
        }
    }
    
    [HarmonyPatch(typeof(RoomMonsters), "OnRoomStartServer")]
    public static class MonsterSpawnPatch
    {
        static void Prefix(RoomMonsters __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (!NetworkServer.active) return;
            
            // Don't interfere when traveling to Zone_Primus
            if (InfiniteDungeonMod.IsTravelingToPrimus()) return;
            
            try
            {
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null) return;
                
                int currentNode = zm.currentNodeIndex;
                if (currentNode < 0 || currentNode >= zm.nodes.Count) return;
                
                // Skip sidetrack, boss, and special rooms - they have their own monster spawning logic
                if (zm.isSidetracking)
                    return;
                
                WorldNodeType nodeType = zm.nodes[currentNode].type;
                if (nodeType == WorldNodeType.ExitBoss || nodeType == WorldNodeType.Special)
                    return;
                
                Zone nodeZone = InfiniteDungeonMod.GetNodeZone(currentNode);
                
                if (nodeZone != null && nodeZone.defaultMonsters != null)
                {
                    __instance.defaultRule = nodeZone.defaultMonsters;
                }
                else if (zm.currentZone != null && zm.currentZone.defaultMonsters != null)
                {
                    __instance.defaultRule = zm.currentZone.defaultMonsters;
                }
                
                // ========== POPULATION SCALING BY DEPTH ==========
                var config = InfiniteDungeonMod.Instance != null ? InfiniteDungeonMod.Instance.config : null;
                float populationCap = config != null ? config.populationCap : 3.0f;
                float populationPerDepth = config != null ? config.populationPerDepth : 2.0f;
                int populationStartDepth = config != null ? config.populationStartDepth : 5;
                
                int depth = InfiniteDungeonMod.GetTotalDepth();
                
                // Calculate population multiplier
                float populationMultiplier = 1.0f;
                if (depth > populationStartDepth && populationPerDepth > 0)
                {
                    // Add percentage per depth after start depth
                    populationMultiplier = 1.0f + ((depth - populationStartDepth) * populationPerDepth / 100f);
                    
                    // Cap at maximum
                    populationMultiplier = Mathf.Min(populationMultiplier, populationCap);
                }
                
                // Apply population multiplier
                if (populationMultiplier > 1.0f)
                {
                    __instance.spawnedPopMultiplier *= populationMultiplier;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error setting monster spawn rule: " + ex.Message);
            }
        }
    }
    
    [HarmonyPatch(typeof(ZoneManager), "OnLoopStart")]
    public static class PreventLoopPatch
    {
        static bool Prefix()
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            return false;
        }
    }
    
    /// <summary>
    /// Change the zone when loading a node to match the node's assigned zone.
    /// This makes each node load rooms from its assigned zone (Forest, LavaLand, etc.)
    /// IMPORTANT: Don't interfere with explicit zone travels (like Rift_Sidetrack_TheDream to Primus)
    /// 
    /// Also fixes a game bug: When loading a save while in a sidetrack room, the game
    /// doesn't set isSidetrackTransition=true, causing sidetrackReturnNodeIndex to be reset.
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), "LoadNode")]
    public static class ChangeZoneOnLoadPatch
    {
        [HarmonyPrefix]
        static void Prefix(ZoneManager __instance, LoadNodeSettings s)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (!NetworkServer.active) return;
            
            // CRITICAL: If we're traveling to Zone_Primus, don't interfere at all!
            if (InfiniteDungeonMod.IsTravelingToPrimus()) return;
            
            try
            {
                if (s == null) return;
                
                // FIX: When loading from save while in a sidetrack, the game incorrectly sets
                // isSidetrackTransition = false, which causes sidetrackReturnNodeIndex to be reset.
                // We need to preserve it if we're currently sidetracking.
                if (s.isLoadingFromSave && __instance.sidetrackReturnNodeIndex >= 0)
                {
                    // We're loading a save while in a sidetrack - preserve the sidetrack state
                    s.isSidetrackTransition = true;
                }
                
                if (s.to < 0) return;
                
                // If the settings already specify a zone (explicit travel like Primus), don't override
                if (s.newZone != null && s.isTravelingZone)
                    return;
                
                // Get the zone assigned to this node
                Zone nodeZone = InfiniteDungeonMod.GetNodeZone(s.to);
                
                if (nodeZone != null)
                {
                    // Set the zone for this node - this makes the game load rooms from this zone
                    s.newZone = nodeZone;
                    s.newZoneNoAdvance = true; // Don't advance zone index
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in ChangeZoneOnLoadPatch: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Force monster spawning on REVISITED combat nodes.
    /// Dev suggestion: Set isMarkedAsCombatArea = true on all Section_Monsters instances.
    /// This triggers monster spawning when players enter sections, even on revisit.
    /// 
    /// KEY INSIGHT: The section's onEnterFirstTime event is gated by _didInvokeOnEnterFirstTime (a SaveVar).
    /// On revisit, this is already true, so the event never fires. We must reset it via reflection.
    /// 
    /// EXCEPTION: Merchant/Boss/Start nodes keep default behavior.
    /// </summary>
    [HarmonyPatch(typeof(RoomMonsters), "OnRoomStartServer")]
    public static class ForceMonsterRespawnPatch
    {
        private static System.Reflection.FieldInfo _didInvokeOnEnterFirstTimeField;
        private static System.Reflection.FieldInfo _didInvokeOnEveryonePresentField;
        
        static void Postfix(RoomMonsters __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (!NetworkServer.active) return;
            
            // Don't interfere when traveling to Zone_Primus
            if (InfiniteDungeonMod.IsTravelingToPrimus()) return;
            
            try
            {
                // Only process revisits - new nodes already spawn monsters normally
                if (!__instance.isRevisit) return;
                
                Room room = __instance.room;
                if (room == null) return;
                
                // Get current node type
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null) return;
                
                int currentNode = zm.currentNodeIndex;
                if (currentNode < 0 || currentNode >= zm.nodes.Count) return;
                
                // Skip sidetrack rooms and non-combat nodes
                if (zm.isSidetracking)
                    return;
                
                WorldNodeType nodeType = zm.nodes[currentNode].type;
                
                // Skip merchant, boss, start, and special nodes - they shouldn't respawn monsters
                if (nodeType == WorldNodeType.Merchant ||
                    nodeType == WorldNodeType.ExitBoss ||
                    nodeType == WorldNodeType.Start ||
                    nodeType == WorldNodeType.Special)
                    return;
                
                // Cache reflection fields
                if (_didInvokeOnEnterFirstTimeField == null)
                {
                    _didInvokeOnEnterFirstTimeField = typeof(RoomSection).GetField("_didInvokeOnEnterFirstTime", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                }
                if (_didInvokeOnEveryonePresentField == null)
                {
                    _didInvokeOnEveryonePresentField = typeof(RoomSection).GetField("_didInvokeOnEveryonePresent", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                }
                
                // DEV SUGGESTION: Set isMarkedAsCombatArea = true on all Section_Monsters
                // PLUS: Reset the "first time enter" flags so the event can fire again
                int markedCount = 0;
                foreach (RoomSection section in room.sections)
                {
                    if (section == null) continue;
                    if (section.monsters == null) continue;
                    
                    // Reset the combat area state so monsters can spawn again
                    section.monsters.isMarkedAsCombatArea = true;
                    section.monsters.didClearCombatArea = false;
                    
                    // CRITICAL: Reset the "first time" flags on RoomSection so onEnterFirstTime fires again
                    // These are [SaveVar] fields that persist across revisits
                    if (_didInvokeOnEnterFirstTimeField != null)
                    {
                        _didInvokeOnEnterFirstTimeField.SetValue(section, false);
                    }
                    if (_didInvokeOnEveryonePresentField != null)
                    {
                        _didInvokeOnEveryonePresentField.SetValue(section, false);
                    }
                    
                    markedCount++;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in ForceMonsterRespawnPatch: " + ex.Message);
            }
        }
    }
    
    // Note: Shrine persistence is handled by the game's modifier system (modifierServerData)
    // which tracks whether modifiers have been used. We don't need to restore savedObjects
    // because shrines are spawned fresh but their "used" state is tracked in modifierServerData.
    
    /// <summary>
    /// Destroy all room barriers when a room starts.
    /// This allows free movement in the infinite dungeon without having to clear combat areas.
    /// Also handles spawning weakened bosses after room load.
    /// </summary>
    [HarmonyPatch(typeof(Room), "StartRoom")]
    public static class DestroyBarriersPatch
    {
        static void Postfix()
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (!NetworkServer.active) return;
            
            try
            {
                // Find and destroy all barriers in the room
                Room_Barrier[] barriers = UnityEngine.Object.FindObjectsOfType<Room_Barrier>();
                int count = 0;
                foreach (Room_Barrier barrier in barriers)
                {
                    if (barrier != null)
                    {
                        NetworkServer.Destroy(barrier.gameObject);
                        count++;
                    }
                }
                
                // Check if we need to spawn a weakened boss
                if (InfiniteDungeonMod.Instance != null)
                {
                    InfiniteDungeonMod.Instance.CheckAndSpawnWeakenedBoss();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error destroying barriers: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Clean up lingering status effects AND environmental hazard modifiers when leaving a room.
    /// This ensures environmental damage effects (ToxicArea, InkStrike, Meteors, etc.) don't persist after leaving.
    /// We ONLY destroy environmental hazard modifiers - NOT shrines, wells, or other interactables.
    /// Shrines and wells use PlaceShrine<>() and are preserved by the game's save system.
    /// </summary>
    [HarmonyPatch(typeof(Room), "StopRoom")]
    public static class CleanupModifiersPatch
    {
        // List of status effect type names that come from room modifiers and should be cleaned up
        // These are environmental effects that shouldn't persist between rooms
        private static readonly HashSet<string> RoomModifierStatusEffects = new HashSet<string>
        {
            // Effects that start with Se_RoomMod_
            "Se_RoomMod_ToxicArea_Debuff",
            "Se_RoomMod_MisticIceVein_Shield",
            "Se_RoomMod_Symbiote",
            "Se_RoomMod_WarpingField_Warp",
            "Se_RoomMod_WarpingField_Unstable",
            "Se_RoomMod_UnstableVeilOfTime_ModifyCooldowns",
            "Se_RoomMod_InkStrikeWarning_Slow",
            "Se_RoomMod_BlackRain",
            
            // Effects with different naming convention
            "Se_AcceleratedTime",
            "Se_EngulfedInFlame",
            "Se_VeilOfDark",
            "Se_DarkCondensationZone",
            "Se_ArcticTerritory",
            "Se_GravityTraining",
            "Se_PureDream",
            "Se_GoldEverywhere",
            "Se_DistantMemories",
            "Se_LingeringAuraOfGuidance",
        };
        
        // Environmental hazard modifiers that run coroutines/continuous effects and must be destroyed
        // These are NOT shrines - they spawn projectiles, apply effects over time, etc.
        private static readonly HashSet<string> EnvironmentalHazardModifiers = new HashSet<string>
        {
            // Miniboss spawner - MUST be removed so players can't farm minibosses by revisiting
            "RoomMod_SpawnMiniBoss",         // Spawns miniboss - one-time only!
            
            // Projectile/damage hazards
            "RoomMod_InkStrikeWarning",      // Spawns ink artillery strikes
            "RoomMod_RiskOfMeteors",         // Spawns meteor strikes
            "RoomMod_ToxicArea",             // Toxic damage zones
            "RoomMod_BlackRain",             // Applies status effect to all entities
            "RoomMod_EngulfedInFlame",       // Fire damage
            "RoomMod_VeilOfDark",            // Darkness effect
            "RoomMod_ArcticTerritory",       // Cold/ice effect
            "RoomMod_GravityTraining",       // Gravity effect
            "RoomMod_PureDream",             // Dream effect
            "RoomMod_DarkCondensationZone",  // Dark zone effect
            "RoomMod_AcceleratedTime",       // Time acceleration
            "RoomMod_WarpingField",          // Warping effect
            "RoomMod_UnstableVeilOfTime",    // Time modifier
            "RoomMod_GazeOfErebos",          // Erebos gaze effect
            "RoomMod_FireDevil",             // Fire devil spawner
            "RoomMod_MeteoricLife",          // Meteoric life effect
            "RoomMod_StardustEverywhere",    // Stardust effect
            "RoomMod_GoldEverywhere",        // Gold effect
            "RoomMod_DistantMemories",       // Memory effect
            "RoomMod_LingeringAuraOfGuidance", // Aura effect
            "RoomMod_MisticIceVein",         // Ice vein effect
            "RoomMod_SymbioteHabitat",       // Symbiote spawner
            "RoomMod_UnstableRatSwarm",      // Rat swarm spawner
            "RoomMod_GlacialBarrier",        // Glacial barrier
            "RoomMod_InversionSigil",        // Inversion effect
            "RoomMod_Ambush",                // Ambush spawner
            "RoomMod_ChallengingFight",      // Fight modifier
            "RoomMod_HarderFightBetterReward", // Fight modifier
        };
        
        static void Prefix(Room __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (!NetworkServer.active) return;
            
            try
            {
                // 1. Clean up status effects from room modifiers
                int effectCount = 0;
                foreach (Entity entity in NetworkedManagerBase<ActorManager>.instance.allEntities)
                {
                    if (entity == null || entity.Status == null) continue;
                    
                    // Get all status effects and check for room modifier ones
                    // Make a copy of the list since we'll be modifying it
                    var effects = new List<StatusEffect>(entity.Status.statusEffects);
                    foreach (var effect in effects)
                    {
                        if (effect == null) continue;
                        string typeName = effect.GetType().Name;
                        
                        // Check if this is a known room modifier status effect
                        if (RoomModifierStatusEffects.Contains(typeName) || typeName.StartsWith("Se_RoomMod_"))
                        {
                            effect.Destroy();
                            effectCount++;
                        }
                    }
                }
                
                // 1.5. Remove curse stat debuffs from players (so they don't stack!)
                InfiniteDungeonMod.RemoveAllCurseDebuffs();
                
                // 2. Destroy environmental hazard modifier instances (NOT shrines!)
                // These are modifiers that run coroutines spawning projectiles, applying continuous effects, etc.
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm != null)
                {
                    int modifierCount = 0;
                    var modifiersToDestroy = new List<RoomModifierBase>();
                    
                    foreach (var modifier in UnityEngine.Object.FindObjectsOfType<RoomModifierBase>())
                    {
                        if (modifier == null) continue;
                        string typeName = modifier.GetType().Name;
                        
                        // Only destroy environmental hazard modifiers, NOT shrines
                        if (EnvironmentalHazardModifiers.Contains(typeName))
                        {
                            modifiersToDestroy.Add(modifier);
                        }
                    }
                    
                    foreach (var modifier in modifiersToDestroy)
                    {
                        try
                        {
                            // Stop all coroutines on this modifier
                            modifier.StopAllCoroutines();
                            
                            // Call Destroy on the Actor to trigger OnDestroyActor cleanup
                            // This ensures ModifyEntities cleanup actions are called
                            modifier.Destroy();
                            modifierCount++;
                        }
                        catch { }
                    }
                    
                    // 3. Remove one-time event modifiers from node DATA so they don't respawn on revisit
                    // This prevents players from farming these events by leaving and re-entering
                    int currentNode = zm.currentNodeIndex;
                    if (currentNode >= 0 && currentNode < zm.nodes.Count)
                    {
                        var nodeData = zm.nodes[currentNode];
                        var modifiers = nodeData.modifiers;
                        
                        // List of modifiers that should only trigger once per node
                        var oneTimeModifiers = new HashSet<string>
                        {
                            "RoomMod_SpawnMiniBoss",    // Miniboss - can't farm
                            "RoomMod_Ambush",           // Ambush event - one time
                            "RoomMod_DistantMemories",  // Distant Memories event - one time
                        };
                        
                        modifiers.RemoveAll(m => oneTimeModifiers.Contains(m.type));
                    }
                    
                    // NOTE: We do NOT regenerate modifiers for visited nodes anymore.
                    // Save data is preserved, so shrines/wells keep their used state.
                    // Map view correctly shows the saved modifier state.
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error cleaning up modifiers: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// After depth 50, spawn a pure white rift (Rift_Sidetrack_TheDream)
    /// beside the normal exit rift when defeating a boss in boss nodes.
    /// This gives players a special reward/challenge opportunity at deeper depths.
    /// </summary>
    [HarmonyPatch(typeof(RoomRifts), "OpenRifts")]
    public static class SpawnPureWhiteRiftPatch
    {
        static void Postfix(RoomRifts __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (!NetworkServer.active) return;
            
            try
            {
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null) return;
                
                // Only spawn in boss rooms
                if (zm.currentNode.type != WorldNodeType.ExitBoss) return;
                
                // Check depth requirement (configurable)
                int depth = InfiniteDungeonMod.GetTotalDepth();
                int minDepth = InfiniteDungeonMod.Instance != null ? InfiniteDungeonMod.Instance.config.pureWhiteRiftMinDepth : 50;
                if (depth < minDepth)
                    return;
                
                // Get the pure white rift prefab (TheDream rift)
                Rift_Sidetrack dreamRiftPrefab = DewResources.GetByName<Rift_Sidetrack>("Rift_Sidetrack_TheDream");
                if (dreamRiftPrefab == null)
                {
                    Debug.LogWarning("[InfiniteDungeon] Could not find Rift_Sidetrack_TheDream prefab!");
                    return;
                }
                
                // Spawn the rift using the RoomRifts system
                __instance.StartCoroutine(SpawnPureWhiteRiftDelayed(__instance, dreamRiftPrefab));
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in SpawnPureWhiteRiftPatch: " + ex.Message);
            }
        }
        
        private static IEnumerator SpawnPureWhiteRiftDelayed(RoomRifts rifts, Rift_Sidetrack prefab)
        {
            // Wait a bit after the normal rifts open
            yield return new WaitForSeconds(2.5f);
            
            try
            {
                // Use the RoomRifts.CreateSidetrackRift method to spawn properly
                rifts.CreateSidetrackRift(prefab);
                
                // Send announcement
                InfiniteDungeonMod.SendChatMessage(DungeonLocalization.CreateMessageKey("PURE_WHITE_RIFT"));
                
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error spawning pure white rift: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Prevent boss room portal from advancing to next zone.
    /// In infinite dungeon mode, the boss room portal should just open the world map
    /// so you can travel to adjacent nodes like any other room.
    /// </summary>
    [HarmonyPatch(typeof(Rift_RoomExit), "UserCode_TpcInteract__NetworkConnectionToClient")]
    public static class BossRoomPortalPatch
    {
        static bool Prefix(Rift_RoomExit __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            try
            {
                // Check if we're in a boss room
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null) return true;
                
                // If there's a next node index set (normal room), let original behavior handle it
                if (__instance.nextNodeIndex >= 0)
                {
                    return true;
                }
                
                // If we're in a boss room (ExitBoss type), DON'T advance to next zone!
                // Instead, just open the world map so player can travel to adjacent nodes
                if (zm.currentNode.type == WorldNodeType.ExitBoss)
                {
                    InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.Shown;
                    return false; // Skip original method (which would call CmdTravelToNextZone)
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in BossRoomPortalPatch: " + ex.Message);
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// Block the CmdTravelToNextZone command in infinite dungeon mode.
    /// This prevents the normal boss room from advancing to the next zone.
    /// NOTE: This does NOT affect Rift_Sidetrack_TheDream which uses TravelToZone directly.
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), "CmdTravelToNextZone")]
    public static class BlockNextZonePatch
    {
        static bool Prefix()
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            return false; // Block the command
        }
    }
    
    /// <summary>
    /// Detect when traveling to Zone_Primus (final boss zone via pure white rift).
    /// Sets a flag so other patches know to step aside and let the game handle it.
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), "TravelToZone")]
    public static class TravelToZonePatch
    {
        static void Prefix(Zone prefab)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (prefab == null) return;
            
            // Check if we're traveling to the final boss zone (Zone_Primus)
            if (prefab.name == "Zone_Primus")
            {
                InfiniteDungeonMod.SetTravelingToPrimus(true);
            }
        }
    }
    
    // NOTE: We don't block GameMod_Obliviax because:
    // 1. The TargetMethod() approach fails when the type can't be found (Assembly-CSharp may load later)
    // 2. This causes Harmony to fail ALL patches in the mod
    // 3. Our own Obliviax trigger (TryTriggerObliviax) works alongside the game's system
    // The game's Quest_HuntedByObliviax only triggers at hunt level >= 5, which we may not reach
    
    // ============================================================
    // INFINITE LEVELING SYSTEM
    // Patches the game's native leveling system to allow infinite levels
    // EXP scales with depth for balanced progression
    // ============================================================
    
    /// <summary>
    /// Override Hero.maxLevel property to allow infinite leveling (cap at 9999)
    /// This bypasses the DewGameplayExperienceSettings.maxHeroLevel check
    /// </summary>
    [HarmonyPatch(typeof(Hero), "maxLevel", MethodType.Getter)]
    public static class HeroMaxLevelPatch
    {
        static bool Prefix(ref int __result)
        {
            if (!InfiniteDungeonMod.IsInGameSession) return true;
            
            // Set max level to 9999 (effectively infinite)
            __result = 9999;
            return false;
        }
    }
    
    /// <summary>
    /// Override Hero.maxExp to use custom EXP curve:
    /// - Level 1-30: Easier progression (base formula with slight increase)
    /// - Level 31+: Harder progression (exponential scaling)
    /// 
    /// Original formula: (50 + 10 * level) * 1.2^level
    /// New formula:
    ///   Level 1-30: (80 + 15 * level) * 1.15^level (slightly easier early game)
    ///   Level 31+:  Level30_EXP * 1.12^(level - 30) (harder late game)
    /// </summary>
    [HarmonyPatch(typeof(Hero), "maxExp", MethodType.Getter)]
    public static class InfiniteExpCurvePatch
    {
        // Soft cap where progression becomes harder
        private const int SOFT_CAP_LEVEL = 30;
        
        // Cache level 30 EXP requirement for efficiency
        private static int _level30Exp = 0;
        
        static bool Prefix(Hero __instance, ref int __result)
        {
            if (!InfiniteDungeonMod.IsInGameSession) return true;
            
            int level = __instance.level;
            
            // Always allow leveling (no max level check)
            if (level <= 0)
            {
                __result = 100; // Minimum EXP
                return false;
            }
            
            if (level <= SOFT_CAP_LEVEL)
            {
                // Easier early game formula:
                // Level 1: (80 + 15) * 1.15^1 = 109
                // Level 10: (80 + 150) * 1.15^10 = 930
                // Level 20: (80 + 300) * 1.15^20 = 6,242
                // Level 30: (80 + 450) * 1.15^30 = 35,121
                float baseExp = 80f + (15f * level);
                __result = (int)(baseExp * Mathf.Pow(1.15f, level));
                
                // Cache level 30 EXP
                if (level == SOFT_CAP_LEVEL)
                {
                    _level30Exp = __result;
                }
            }
            else
            {
                // Harder late game: exponential growth
                // Each level after 30 requires 12% more EXP than the previous
                // Level 31: 35,121 * 1.12^1 = 39,336
                // Level 40: 35,121 * 1.12^10 = 109,097
                // Level 50: 35,121 * 1.12^20 = 338,842
                // Level 100: 35,121 * 1.12^70 = 96,891,883
                
                // Ensure we have level 30 cached
                if (_level30Exp == 0)
                {
                    float baseExp30 = 80f + (15f * SOFT_CAP_LEVEL);
                    _level30Exp = (int)(baseExp30 * Mathf.Pow(1.15f, SOFT_CAP_LEVEL));
                }
                
                int levelsAboveCap = level - SOFT_CAP_LEVEL;
                __result = (int)(_level30Exp * Mathf.Pow(1.12f, levelsAboveCap));
            }
            
            // Minimum 100 EXP per level
            if (__result < 100) __result = 100;
            
            return false;
        }
    }
    
    /// <summary>
    /// Scale memory (skill) level with dungeon depth instead of zone index.
    /// This makes memories from shrines and rewards stronger as you go deeper.
    /// Only applies scaling after depth 10 to avoid affecting early game.
    /// </summary>
    [HarmonyPatch(typeof(LootManager), "SelectSkillLevel")]
    public static class DepthBasedSkillLevelPatch
    {
        static bool Prefix(LootManager __instance, Rarity rarity, ref int __result)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            try
            {
                // Get depth from our mod (not the game's currentZoneIndex which can be wrong)
                int depth = InfiniteDungeonMod.GetTotalDepth();
                
                // IMPORTANT: We ALWAYS use depth-based zone index, even at low depths!
                // This is because our mod changes zones frequently, causing the game's
                // currentZoneIndex to be very high even at low depths.
                // 
                // Zone index mapping based on depth:
                // Depth 1-10  -> Zone 0 (early game, basic skills)
                // Depth 11-20 -> Zone 1
                // Depth 21-30 -> Zone 2
                // Depth 31-40 -> Zone 3
                // Depth 41+   -> Zone 4 (max base zone)
                int effectiveZoneIndex;
                if (depth <= 10)
                {
                    effectiveZoneIndex = 0; // Early game - use zone 0 for basic skills
                }
                else
                {
                    effectiveZoneIndex = Mathf.Min((depth - 1) / 10, 4);
                }
                
                // Use the game's formulas with our depth-based zone index
                float minLevel = __instance.skillLevelMinByZoneIndex.Get(rarity).Evaluate(effectiveZoneIndex);
                float maxLevel = __instance.skillLevelMaxByZoneIndex.Get(rarity).Evaluate(effectiveZoneIndex);
                
                // Add depth bonus: +1 level per 10 depths after depth 50
                float depthBonus = 0f;
                if (depth > 50)
                {
                    depthBonus = (depth - 50) / 10f;
                }
                
                minLevel += depthBonus;
                maxLevel += depthBonus;
                
                // Apply random curve between min and max
                float floatLevel = Mathf.Lerp(minLevel, maxLevel, 
                    __instance.skillLevelRandomCurve.Evaluate(UnityEngine.Random.value));
                
                __result = Mathf.Clamp(Mathf.RoundToInt(floatLevel), 1, 100);
                
                return false; // Skip original - ALWAYS use our logic
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in DepthBasedSkillLevelPatch: " + ex.Message);
                return true; // Fall back to original
            }
        }
    }
    
    /// <summary>
    /// Scale essence (gem) quality with dungeon depth instead of zone index.
    /// This makes essences from shrines and rewards scale properly with depth.
    /// </summary>
    [HarmonyPatch(typeof(LootManager), "SelectGemQuality")]
    public static class DepthBasedGemQualityPatch
    {
        static bool Prefix(LootManager __instance, Rarity rarity, ref int __result)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            try
            {
                // Get depth from our mod (not the game's currentZoneIndex which can be wrong)
                int depth = InfiniteDungeonMod.GetTotalDepth();
                
                // IMPORTANT: We ALWAYS use depth-based zone index, even at low depths!
                // This is because our mod changes zones frequently, causing the game's
                // currentZoneIndex to be very high even at low depths.
                // 
                // Zone index mapping based on depth:
                // Depth 1-10  -> Zone 0 (early game, basic quality)
                // Depth 11-20 -> Zone 1
                // Depth 21-30 -> Zone 2
                // Depth 31-40 -> Zone 3
                // Depth 41+   -> Zone 4 (max base zone)
                int effectiveZoneIndex;
                if (depth <= 10)
                {
                    effectiveZoneIndex = 0; // Early game - use zone 0 for basic quality
                }
                else
                {
                    effectiveZoneIndex = Mathf.Min((depth - 1) / 10, 4);
                }
                
                // Use the game's formulas with our depth-based zone index
                float minQuality = __instance.gemQualityMinByZoneIndex.Get(rarity).Evaluate(effectiveZoneIndex);
                float maxQuality = __instance.gemQualityMaxByZoneIndex.Get(rarity).Evaluate(effectiveZoneIndex);
                
                // Add depth bonus: +5% quality per 10 depths after depth 50
                float depthBonus = 0f;
                if (depth > 50)
                {
                    depthBonus = ((depth - 50) / 10f) * 5f;
                }
                
                minQuality += depthBonus;
                maxQuality += depthBonus;
                
                // Apply random curve between min and max
                float floatQuality = Mathf.Lerp(minQuality, maxQuality, 
                    __instance.gemQualityRandomCurve.Evaluate(UnityEngine.Random.value));
                
                // Round to nearest 10 (game convention for gem quality)
                __result = Mathf.Clamp(Mathf.RoundToInt(floatQuality / 10f) * 10, 10, 200);
                
                return false; // Skip original - ALWAYS use our logic
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in DepthBasedGemQualityPatch: " + ex.Message);
                return true; // Fall back to original
            }
        }
    }
    
    /// <summary>
    /// Override monster EXP drops to scale with dungeon depth.
    /// Deeper = more EXP, making leveling viable even at high levels.
    /// EXP multiplier is configurable via mod settings.
    /// </summary>
    [HarmonyPatch(typeof(GameManager), "GetExpDropFromEntity")]
    public static class DepthBasedExpPatch
    {
        static bool Prefix(GameManager __instance, Entity ent, ref float __result)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            Monster monster = ent as Monster;
            if (monster == null)
            {
                __result = 0f;
                return false;
            }
            
            // Get config multiplier (with fallback default)
            var config = InfiniteDungeonMod.Instance != null ? InfiniteDungeonMod.Instance.config : null;
            float expMultiplier = config != null ? config.expMultiplier : 1.0f;
            
            // Get current depth from our dungeon system
            int depth = InfiniteDungeonMod.GetTotalDepth();
            if (depth < 1) depth = 1;
            
            // Base EXP scales with depth:
            // Depth 1: 15 base
            // Depth 10: 30 base
            // Depth 20: 50 base
            // Depth 50: 125 base
            // Depth 100: 265 base
            float baseExp = 15f + (depth * 1.5f) + (depth * depth * 0.01f);
            
            // Monster type multipliers
            float typeMultiplier = 1f;
            switch (monster.type)
            {
                case Monster.MonsterType.Lesser:
                    typeMultiplier = 0.5f;
                    break;
                case Monster.MonsterType.Normal:
                    typeMultiplier = 1f;
                    break;
                case Monster.MonsterType.MiniBoss:
                    typeMultiplier = 8f;
                    break;
                case Monster.MonsterType.Boss:
                    typeMultiplier = 50f;
                    break;
            }
            
            // Add some randomness (±10%)
            float deviation = UnityEngine.Random.Range(0.9f, 1.1f);
            
            // Calculate final EXP
            float finalExp = baseExp * typeMultiplier * deviation * expMultiplier;
            
            // Ensure minimum EXP - all monsters should give at least SOME exp
            // Minimum: 5 EXP for lesser, 10 for normal, 50 for miniboss, 200 for boss
            float minExp = 5f;
            switch (monster.type)
            {
                case Monster.MonsterType.Lesser:
                    minExp = 5f;
                    break;
                case Monster.MonsterType.Normal:
                    minExp = 10f;
                    break;
                case Monster.MonsterType.MiniBoss:
                    minExp = 50f;
                    break;
                case Monster.MonsterType.Boss:
                    minExp = 200f;
                    break;
            }
            
            __result = Mathf.Max(finalExp, minExp);
            
            return false;
        }
    }
    
    // ============================================================
    // MAP PANNING SYSTEM
    // Allows players to pan the world map view using mouse drag or keyboard
    // ============================================================
    
    /// <summary>
    /// Fix for clients: When there are many nodes, the nodeDistanceMatrix SyncList
    /// may not be fully synced when the map tries to draw connection lines.
    /// This patch prevents index out of bounds errors by validating indices.
    /// IMPORTANT: Only catch actual errors, don't block valid operations!
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), "GetNodeDistance")]
    public static class FixNodeDistanceForClientsPatch
    {
        static bool Prefix(ZoneManager __instance, int a, int b, ref int __result)
        {
            // Only apply fix when mod is active
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            try
            {
                int nodeCount = __instance.nodes.Count;
                int actualMatrixSize = __instance.nodeDistanceMatrix.Count;
                
                // Validate indices are in bounds to prevent crashes
                int index = nodeCount * a + b;
                if (index < 0 || index >= actualMatrixSize || actualMatrixSize == 0)
                {
                    // Return "not connected" only when we'd crash otherwise
                    __result = 10000;
                    return false;
                }
                
                // Let original method run - matrix is accessible
                return true;
            }
            catch
            {
                // On any error, return "not connected" to prevent crash
                __result = 10000;
                return false;
            }
        }
    }
    
    /// <summary>
    /// FIX FOR CLIENT CONNECTION LINES:
    /// 
    /// PROBLEM: The game uses an n² distance matrix (nodeDistanceMatrix) which becomes huge with many nodes.
    /// At 100 nodes = 10,000 entries, at 200 nodes = 40,000 entries. Mirror SyncList struggles with this.
    /// 
    /// SOLUTION: REPLACE the original RefreshNodes method entirely when mod is active.
    /// Use our own connection tracking system that syncs via serverActor.persistentSyncedData.
    /// This prevents double edges and ensures correct connections for both host and client.
    /// </summary>
    [HarmonyPatch(typeof(UI_InGame_WorldMap), "RefreshNodes")]
    public static class FixMapConnectionLinesPatch
    {
        // Prefix returns false to SKIP original method entirely when mod is active
        static bool Prefix(UI_InGame_WorldMap __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return true; // Let original run
            
            try
            {
                // IMPORTANT: For clients, force refresh connection cache from server
                // This ensures we always have the latest data when drawing the map
                if (!NetworkServer.active)
                {
                    InfiniteDungeonMod.ForceRefreshConnectionCache();
                }
                
                // Destroy all existing children (nodes and edges) - same as original
                for (int i = __instance.nodeParent.childCount - 1; i >= 0; i--)
                {
                    UnityEngine.Object.DestroyImmediate(__instance.nodeParent.GetChild(i).gameObject);
                }
                
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null || zm.currentZone == null || zm.currentNodeIndex < 0)
                {
                    return false; // Skip original
                }
                
                // Create node items (same as original)
                List<UI_InGame_World_NodeItem> nodes = new List<UI_InGame_World_NodeItem>();
                int skippedSidetrack = 0;
                for (int j = 0; j < zm.nodes.Count; j++)
                {
                    if (!zm.nodes[j].IsSidetrackNode())
                    {
                        UI_InGame_World_NodeItem newNode = UnityEngine.Object.Instantiate(
                            __instance.nodePrefab, __instance.nodeParent);
                        newNode.Setup(j, __instance);
                        nodes.Add(newNode);
                    }
                    else
                    {
                        skippedSidetrack++;
                    }
                }
                
                // Create edges using OUR connection data (not game's IsNodeConnected)
                int edgesCreated = 0;
                int connectionCount = InfiniteDungeonMod.GetConnectionCount();
                
                for (int i = 0; i < nodes.Count; i++)
                {
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        // Use ACTUAL node indices from the UI items, not list indices!
                        int nodeIndexA = nodes[i].index;
                        int nodeIndexB = nodes[j].index;
                        
                        // Use our AreNodesConnected which works for both server and client
                        bool isConnected = InfiniteDungeonMod.AreNodesConnected(nodeIndexA, nodeIndexB);
                        
                        if (isConnected)
                        {
                            UI_InGame_World_Edge newEdge = UnityEngine.Object.Instantiate(
                                __instance.edgePrefab, __instance.nodeParent);
                            newEdge.Setup(nodes[i], nodes[j], __instance);
                            edgesCreated++;
                        }
                    }
                }
                
                // Logging removed for performance
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in RefreshNodes replacement: " + ex.Message);
            }
            
            return false; // Skip original method - we handled everything
        }
    }
    
    /// <summary>
    /// FIX FOR HUNTER MONSTERS NOT GIVING EXP
    /// 
    /// PROBLEM:
    /// - When Se_HunterBuff is applied to monsters, enableGoldAndExpDrops defaults to false
    /// - PickupManager.HandleDeathDrop checks this flag and returns early if false
    /// - This means hunter monsters give 0 EXP, even though our DepthBasedExpPatch exists
    /// 
    /// ROOT CAUSE (in game code):
    /// - Se_HunterBuff.enableGoldAndExpDrops is [NonSerialized] and defaults to false
    /// - RoomMonsters.cs line 963-966 creates the buff WITHOUT setting enableGoldAndExpDrops
    /// - RoomMod_Hunted.OnAfterSpawn also doesn't set it
    /// - PickupManager.HandleDeathDrop line 242: "if (buff && !buff.enableGoldAndExpDrops) return;"
    /// 
    /// SOLUTION:
    /// Patch Se_HunterBuff.OnCreate to ALWAYS enable gold/exp drops when our mod is active.
    /// This ensures ALL hunter monsters (whether from vanilla hunter system or our infection system)
    /// will properly drop EXP.
    /// </summary>
    [HarmonyPatch(typeof(Se_HunterBuff), "OnCreate")]
    public static class HunterBuffEnableExpDropsPatch
    {
        static void Postfix(Se_HunterBuff __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            
            // CRITICAL: Enable gold and EXP drops for ALL hunter monsters!
            // Without this, hunter monsters give 0 EXP because PickupManager.HandleDeathDrop
            // returns early when enableGoldAndExpDrops is false.
            __instance.enableGoldAndExpDrops = true;
        }
    }
    
    /// <summary>
    /// FIX FOR NAVMESH LOG SPAM
    /// The game's GetValidAgentDestination_LinearSweep logs a warning every frame when
    /// a knocked-out hero's position is not on the NavMesh. This patch suppresses the
    /// spam by skipping the validation entirely for knocked-out heroes.
    /// 
    /// PROBLEM:
    /// - DoMovementObserverFrameUpdate calls GetValidAgentDestination_LinearSweep every frame
    /// - When a hero is knocked out and their position is invalid, it logs a warning
    /// - At 100+ FPS, this creates massive log spam until the player disconnects
    /// 
    /// SOLUTION:
    /// Use our own implementation that suppresses the warning after logging it once.
    /// This prevents the log spam while maintaining the same behavior (hero stays at their position)
    /// 
    /// The fix is minimal and only affects knocked-out heroes to avoid any unintended side effects on normal gameplay.
    /// </summary>
    [HarmonyPatch(typeof(Dew), "GetValidAgentDestination_LinearSweep")]
    public static class SuppressNavMeshLogSpamPatch
    {
        private static float _lastWarningTime = 0f;
        private const float WARNING_COOLDOWN = 5f; // Only log once every 5 seconds
        
        static bool Prefix(Vector3 start, Vector3 end, ref Vector3 __result)
        {
            // CRITICAL: Only apply when in a game session - don't run in main menu!
            if (!InfiniteDungeonMod.IsInGameSession) return true;
            
            // Filter non-okay values first (same as original)
            Dew.FilterNonOkayValues(ref start);
            Dew.FilterNonOkayValues(ref end, start);
            
            float maxDistance = 0.5f;
            
            // Try to find start position on NavMesh
            while (maxDistance <= 10f)
            {
                UnityEngine.AI.NavMeshHit startHit;
                if (UnityEngine.AI.NavMesh.SamplePosition(start, out startHit, maxDistance, -1))
                {
                    // Found valid start position - let original method handle it
                    return true;
                }
                maxDistance += 0.5f;
            }
            
            // Start position is not on NavMesh - this is where the spam happens
            // Only log warning occasionally instead of every frame
            if (Time.time - _lastWarningTime > WARNING_COOLDOWN)
            {
                _lastWarningTime = Time.time;
                Debug.LogWarning("[InfiniteDungeon] NavMesh position not found (suppressing further warnings for " + WARNING_COOLDOWN + "s)");
            }
            
            // Return the start position (same as original behavior)
            __result = start;
            return false; // Skip original method
        }
    }
    
    // ============================================================
    // INFINITE DUNGEON SCORING & MASTERY ENHANCEMENTS
    // ============================================================
    
    /// <summary>
    /// Enhance mastery points calculation to account for Infinite Dungeon depth.
    /// KEPT MODEST - should not overshadow base game mastery (typical run = ~100k-150k points)
    /// Our bonus is a small percentage addition (~2-5% of base), not a replacement.
    /// 
    /// Original formula: masteryPoints = effectiveMinutes * 1000 * 1.6
    /// Our enhancement: Small bonus based on depth reached
    /// </summary>
    [HarmonyPatch(typeof(Dew), "GetRewardedMasteryPoints")]
    public static class EnhancedMasteryPointsPatch
    {
        static void Postfix(float minutes, ref long __result)
        {
            if (!InfiniteDungeonMod.IsInGameSession) return;
            
            try
            {
                int depth = InfiniteDungeonMod.GetTotalDepth();
                if (depth <= 0) return;
                
                int infectedNodes = InfiniteDungeonMod.GetInfectedNodeCount();
                int cursesEndured = InfiniteDungeonMod.GetCursesEndured();
                
                // Calculate MODEST depth bonus:
                // - Small bonus: 50 points per depth (depth 50 = 2,500)
                // - Small milestone: 200 points every 10 depths (depth 50 = 1,000)
                // - Hunter bonus: 25 per infected node
                // - Curse bonus: 10 per curse charge
                // Total at depth 50 with 10 infected nodes and 20 curses = ~4,000 points (~3% of base)
                long depthBonus = 0;
                
                // Base bonus (50 per depth)
                depthBonus += depth * 50L;
                
                // Milestone bonus (200 every 10 depths)
                int milestones = depth / 10;
                depthBonus += milestones * 200L;
                
                // Hunter encounter bonus (25 per infected node)
                depthBonus += infectedNodes * 25L;
                
                // Curse survival bonus (10 per curse charge)
                depthBonus += cursesEndured * 10L;
                
                __result += depthBonus;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in EnhancedMasteryPointsPatch: " + ex.Message);
            }
        }
    }
    
    
    /// <summary>
    /// Add Infinite Dungeon score bonus to the total score calculation.
    /// Patches UI_InGame_ResultView.Refresh to add our bonus to the total score display.
    /// </summary>
    [HarmonyPatch(typeof(UI_InGame_ResultView), "Refresh")]
    public static class EnhancedScorePatch
    {
        static void Postfix(UI_InGame_ResultView __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            
            try
            {
                // Get our score bonus
                long scoreBonus = InfiniteDungeonResultUI.GetScoreBonus();
                if (scoreBonus <= 0) return;
                
                // Get the totalScoreText field
                TMPro.TextMeshProUGUI totalScoreText = __instance.totalScoreText;
                if (totalScoreText == null) return;
                
                // Parse current score from the text
                // Format is like "Total Score: 123,456"
                string currentText = totalScoreText.text;
                
                // Try to extract the number and add our bonus
                // The format uses DewLocalization.GetUIValue("InGame_Result_ScoreFormat") which is "{0}"
                string numberPart = System.Text.RegularExpressions.Regex.Replace(currentText, "[^0-9]", "");
                long currentScore;
                if (long.TryParse(numberPart, out currentScore))
                {
                    long newScore = currentScore + scoreBonus;
                    totalScoreText.text = string.Format(DewLocalization.GetUIValue("InGame_Result_ScoreFormat"), newScore.ToString("#,##0"));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in EnhancedScorePatch: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Enhance combat time calculation for mastery to reward Infinite Dungeon progression.
    /// The game calculates: minimumMinutes = heroicBossKills * 7 + miniBossKills * 1.5
    /// We add extra combat time credit for depth, hunter encounters, and curse survival.
    /// </summary>
    [HarmonyPatch(typeof(DewSave), "ConsumeGameResult")]
    public static class EnhancedConsumeGameResultPatch
    {
        static void Prefix(DewGameResult result)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (result == null) return;
            
            try
            {
                // Find the local player
                DewGameResult.PlayerData localPlayer = result.players.Find(p => p.isLocalPlayer);
                if (localPlayer == null) return;
                
                int depth = InfiniteDungeonMod.GetTotalDepth();
                int infectedEncounters = InfiniteDungeonMod.GetInfectedNodeCount();
                int cursesEndured = InfiniteDungeonMod.GetCursesEndured();
                
                // Add extra combat time credit (in seconds) for our custom content
                // This naturally increases mastery points through the existing formula
                // Formula: combatTime directly affects "minutes" in mastery calculation
                //   - Each depth: +30 seconds of combat time credit
                //   - Each hunter encounter: +45 seconds (harder fights)
                //   - Each curse endured: +15 seconds (survival challenge)
                
                int bonusCombatTime = 0;
                bonusCombatTime += depth * 30;              // 30 sec per depth
                bonusCombatTime += infectedEncounters * 45; // 45 sec per hunter encounter
                bonusCombatTime += cursesEndured * 15;      // 15 sec per curse charge endured
                
                // Add to actual combat time (this affects mastery calculation naturally)
                localPlayer.combatTime += bonusCombatTime;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in EnhancedConsumeGameResultPatch: " + ex.Message);
            }
        }
    }
    
    // ============================================================
    // QUEST/GOAL NODE DISTANCE FIX
    // Ensures quest destinations and soul nodes are placed nearby,
    // not 15+ nodes away in the infinite dungeon
    // ============================================================
    
    /// <summary>
    /// Patch TryGetNodeIndexForNextGoal to limit the search to nearby nodes.
    /// This fixes quests like Fragment of Radiance placing destinations too far away.
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), "TryGetNodeIndexForNextGoal")]
    public static class LimitQuestNodeDistancePatch
    {
        static bool Prefix(ZoneManager __instance, GetNodeIndexSettings s, ref int nodeIndex, ref bool __result)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            try
            {
                int currentNode = __instance.currentNodeIndex;
                
                // Find nodes within a reasonable distance (max 5 steps)
                // Priority: unvisited nodes within desired distance, then any nearby unvisited
                List<int> candidates = new List<int>();
                List<int> visitedCandidates = new List<int>();
                
                // First, check our tracked connections for adjacent nodes
                HashSet<int> directConnections;
                if (InfiniteDungeonMod.TryGetNodeConnections(currentNode, out directConnections))
                {
                    foreach (int adj in directConnections)
                    {
                        if (adj < 0 || adj >= __instance.nodes.Count) continue;
                        if (adj == currentNode) continue;
                        
                        WorldNodeData nodeData = __instance.nodes[adj];
                        
                        // Skip sidetrack nodes
                        if (nodeData.IsSidetrackNode()) continue;
                        
                        // Check allowed types
                        if (!s.allowedTypes.Contains(nodeData.type)) continue;
                        
                        // Check if has main modifier (if avoiding)
                        if (s.avoidMainModifier && nodeData.HasMainModifier()) continue;
                        
                        if (nodeData.status == WorldNodeStatus.HasVisited)
                        {
                            visitedCandidates.Add(adj);
                        }
                        else
                        {
                            candidates.Add(adj);
                        }
                    }
                }
                
                // Also check using game's distance matrix for nodes within 5 steps
                for (int i = 0; i < __instance.nodes.Count; i++)
                {
                    if (i == currentNode) continue;
                    if (candidates.Contains(i) || visitedCandidates.Contains(i)) continue;
                    
                    WorldNodeData nodeData = __instance.nodes[i];
                    
                    // Skip sidetrack nodes
                    if (nodeData.IsSidetrackNode()) continue;
                    
                    // Check allowed types
                    if (!s.allowedTypes.Contains(nodeData.type)) continue;
                    
                    // Check if has main modifier (if avoiding)
                    if (s.avoidMainModifier && nodeData.HasMainModifier()) continue;
                    
                    int distance = __instance.GetNodeDistance(currentNode, i);
                    
                    // Only consider nodes within 5 steps (reasonable quest distance)
                    if (distance <= 0 || distance > 5) continue;
                    
                    if (nodeData.status == WorldNodeStatus.HasVisited)
                    {
                        visitedCandidates.Add(i);
                    }
                    else
                    {
                        candidates.Add(i);
                    }
                }
                
                // Prefer unvisited nodes
                if (candidates.Count > 0)
                {
                    // Sort by distance preference if specified
                    if (s.desiredDistance.x > 0 || s.desiredDistance.y > 0)
                    {
                        candidates.Sort((a, b) => {
                            int distA = __instance.GetNodeDistance(currentNode, a);
                            int distB = __instance.GetNodeDistance(currentNode, b);
                            
                            // Score based on how close to desired range
                            int scoreA = Mathf.Abs(distA - s.desiredDistance.x);
                            int scoreB = Mathf.Abs(distB - s.desiredDistance.x);
                            
                            return scoreA.CompareTo(scoreB);
                        });
                    }
                    
                    nodeIndex = candidates[0];
                    __result = true;
                    return false;
                }
                
                // Fall back to visited nodes if no unvisited available
                if (visitedCandidates.Count > 0)
                {
                    nodeIndex = visitedCandidates[UnityEngine.Random.Range(0, visitedCandidates.Count)];
                    __result = true;
                    return false;
                }
                
                // No suitable node found - let original method handle it
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in LimitQuestNodeDistancePatch: " + ex.Message);
                return true;
            }
        }
    }
    
    /// <summary>
    /// Patch Se_HeroKnockedOut.CheckAndAddHeroSoul to place the soul node
    /// on a nearby adjacent node instead of using the game's distance-based algorithm
    /// which can pick nodes very far away in our infinite dungeon.
    /// </summary>
    [HarmonyPatch(typeof(Se_HeroKnockedOut), "CheckAndAddHeroSoul")]
    public static class LimitSoulNodeDistancePatch
    {
        static bool Prefix(Se_HeroKnockedOut __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            if (!NetworkServer.active) return true;
            
            try
            {
                // Access the disableQuest field via reflection
                var disableQuestField = AccessTools.Field(typeof(Se_HeroKnockedOut), "disableQuest");
                bool disableQuest = disableQuestField != null && (bool)disableQuestField.GetValue(__instance);
                
                if (disableQuest) return false;
                
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null) return true;
                
                // Don't add soul in boss rooms
                if (zm.currentNode.type == WorldNodeType.ExitBoss) return false;
                
                // Check if soul already exists for this player
                Entity victim = __instance.victim;
                if (victim == null || victim.owner == null) return false;
                
                string playerGuid = victim.owner.guid;
                bool soulExists = zm.nodes.Any(n => n.modifiers.Any(m => 
                    m.type == "RoomMod_HeroSoul" && m.clientData == playerGuid));
                
                if (soulExists) return false;
                
                // Check if there are other alive heroes
                if (Dew.GetAliveHeroCount() == 0) return false;
                
                // CUSTOM: Find a nearby node instead of using the game's algorithm
                // This prevents the soul from being placed 15+ nodes away
                int currentNode = zm.currentNodeIndex;
                int targetNode = FindNearbyNodeForSoul(zm, currentNode);
                
                if (targetNode < 0)
                {
                    // Fallback to game's default behavior
                    return true;
                }
                
                // Add the soul modifier to the nearby node
                zm.AddModifier<RoomMod_HeroSoul>(targetNode, playerGuid);
                
                return false; // Skip original method
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in LimitSoulNodeDistancePatch: " + ex.Message);
                return true; // Fallback to original on error
            }
        }
        
        /// <summary>
        /// Find a nearby unvisited node for the soul, preferring nodes 1-3 connections away
        /// </summary>
        private static int FindNearbyNodeForSoul(ZoneManager zm, int currentNode)
        {
            // Priority 1: Direct adjacent nodes that haven't been visited
            List<int> adjacentUnvisited = new List<int>();
            List<int> adjacentVisited = new List<int>();
            
            // Check our connection tracking first
            HashSet<int> connections;
            if (InfiniteDungeonMod.TryGetNodeConnections(currentNode, out connections))
            {
                foreach (int adj in connections)
                {
                    if (adj < 0 || adj >= zm.nodes.Count) continue;
                    if (adj == currentNode) continue;
                    
                    // Skip boss nodes
                    if (zm.nodes[adj].type == WorldNodeType.ExitBoss) continue;
                    
                    if (zm.nodes[adj].status == WorldNodeStatus.HasVisited)
                    {
                        adjacentVisited.Add(adj);
                    }
                    else
                    {
                        adjacentUnvisited.Add(adj);
                    }
                }
            }
            
            // Prefer unvisited adjacent nodes
            if (adjacentUnvisited.Count > 0)
            {
                return adjacentUnvisited[UnityEngine.Random.Range(0, adjacentUnvisited.Count)];
            }
            
            // Fall back to visited adjacent nodes (still close!)
            if (adjacentVisited.Count > 0)
            {
                return adjacentVisited[UnityEngine.Random.Range(0, adjacentVisited.Count)];
            }
            
            // Priority 2: Use game's distance matrix for nodes within 3 steps
            List<int> nearbyNodes = new List<int>();
            for (int i = 0; i < zm.nodes.Count; i++)
            {
                if (i == currentNode) continue;
                if (zm.nodes[i].type == WorldNodeType.ExitBoss) continue;
                
                int distance = zm.GetNodeDistance(currentNode, i);
                if (distance > 0 && distance <= 3)
                {
                    nearbyNodes.Add(i);
                }
            }
            
            if (nearbyNodes.Count > 0)
            {
                // Prefer unvisited
                var unvisited = nearbyNodes.Where(n => zm.nodes[n].status != WorldNodeStatus.HasVisited).ToList();
                if (unvisited.Count > 0)
                {
                    return unvisited[UnityEngine.Random.Range(0, unvisited.Count)];
                }
                return nearbyNodes[UnityEngine.Random.Range(0, nearbyNodes.Count)];
            }
            
            return -1; // No suitable node found
        }
    }
    
    /// <summary>
    /// Platinum Coin Drop System
    /// Gives players a chance to receive platinum coins when killing bosses/minibosses.
    /// Uses configurable drop chances for MiniBoss and Boss monsters.
    /// 
    /// We patch Entity.NotifyEntityDeathToClients which is the RPC called after death.
    /// This gives us access to the EventInfoKill which contains the killer info.
    /// </summary>
    [HarmonyPatch(typeof(Entity), "NotifyEntityDeathToClients")]
    public static class PlatinumCoinDropPatch
    {
        static void Prefix(Entity __instance, EventInfoKill info)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (!NetworkServer.active) return;
            
            try
            {
                // Check if the victim is a Monster
                Monster monster = __instance as Monster;
                if (monster == null) return;
                
                // Get config
                var config = InfiniteDungeonMod.Instance != null ? InfiniteDungeonMod.Instance.config : null;
                if (config == null) return;
                
                float dropChance = 0f;
                string bossType = "";
                
                // Check monster type
                if (monster.type == Monster.MonsterType.Boss)
                {
                    dropChance = config.bossPlatinumChance / 100f;
                    bossType = "Boss";
                }
                else if (monster.type == Monster.MonsterType.MiniBoss)
                {
                    dropChance = config.miniBossPlatinumChance / 100f;
                    bossType = "MiniBoss";
                }
                else
                {
                    return; // Not a boss or miniboss
                }
                
                // Skip if chance is 0
                if (dropChance <= 0f) return;
                
                // Get the killer from EventInfoKill
                if (info.actor == null) return;
                
                // Get the first entity in the actor chain (the actual killer)
                Entity killerEntity = info.actor.firstEntity;
                if (killerEntity == null) return;
                
                // Find the player who should get the coin
                DewPlayer rewardPlayer = null;
                
                // Check if killer is a hero
                Hero killerHero = killerEntity as Hero;
                if (killerHero != null && killerHero.owner != null && killerHero.owner.isHumanPlayer)
                {
                    rewardPlayer = killerHero.owner;
                }
                else if (killerEntity.owner != null && killerEntity.owner.isHumanPlayer)
                {
                    // Killer is a summon or other player-owned entity
                    rewardPlayer = killerEntity.owner;
                }
                
                if (rewardPlayer == null) return;
                
                // Roll for platinum coin
                if (UnityEngine.Random.value <= dropChance)
                {
                    rewardPlayer.platinumCoin++;
                    
                    if (InfiniteDungeonMod.DEBUG_MODE)
                    {
                        Debug.Log("[InfiniteDungeon] " + bossType + " killed! Awarded platinum coin to " + rewardPlayer.playerName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[InfiniteDungeon] Error in platinum coin drop: " + ex.Message);
            }
        }
    }
}

