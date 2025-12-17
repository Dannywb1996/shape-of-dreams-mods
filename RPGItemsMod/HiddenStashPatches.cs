using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Mirror;

/// <summary>
/// Patches for Hidden Stash to also drop custom RPG items
/// </summary>
public static class HiddenStashPatches
{
    private static System.Random _random = new System.Random();
    
    // Chance to drop a custom RPG item from stash (in addition to normal rewards)
    // This is checked once per stash opening
    public const float STASH_RPG_ITEM_CHANCE = 0.25f; // 25% chance
    
    /// <summary>
    /// Postfix for Shrine_HiddenStash.OnUse - drops bonus RPG items
    /// Called via reflection patching from RPGItemsMod.ApplyHarmonyPatches
    /// </summary>
    public static class OnUsePatch
    {
        public static void Postfix(object __instance, Entity entity, bool __result)
        {
            // Only run on server and if the shrine was successfully used
            if (!NetworkServer.active || !__result) return;
            
            // Roll for RPG item drop
            float roll = (float)_random.NextDouble();
            if (roll > STASH_RPG_ITEM_CHANCE) return;
            
            // Get item templates from ItemDatabase (central source)
            List<RPGItem> templates = ItemDatabase.GetAllItems();
            if (templates == null || templates.Count == 0) return;
            
            // Filter to only equipment (no consumables from stash)
            List<RPGItem> equipmentTemplates = new List<RPGItem>();
            foreach (RPGItem item in templates)
            {
                if (item.type != ItemType.Consumable)
                {
                    equipmentTemplates.Add(item);
                }
            }
            
            if (equipmentTemplates.Count == 0) return;
            
            // Use game's LootManager to select rarity (high rarity like empowered shrine)
            ItemRarity selectedRarity = ItemRarity.Common;
            try
            {
                LootManager lootManager = NetworkedManagerBase<LootManager>.instance;
                if (lootManager != null)
                {
                    Rarity gameRarity = lootManager.SelectSkillRarity(true); // High rarity
                    switch (gameRarity)
                    {
                        case Rarity.Common: selectedRarity = ItemRarity.Common; break;
                        case Rarity.Rare: selectedRarity = ItemRarity.Rare; break;
                        case Rarity.Epic: selectedRarity = ItemRarity.Epic; break;
                        case Rarity.Legendary: selectedRarity = ItemRarity.Legendary; break;
                    }
                }
            }
            catch { }
            
            // Filter by rarity
            List<RPGItem> rarityPool = new List<RPGItem>();
            foreach (RPGItem item in equipmentTemplates)
            {
                if (item.rarity == selectedRarity)
                {
                    rarityPool.Add(item);
                }
            }
            
            // Fallback if no items of that rarity
            if (rarityPool.Count == 0)
            {
                rarityPool = equipmentTemplates;
            }
            
            // Get drop position near the stash using reflection
            Vector3 dropPos = Vector3.zero;
            try
            {
                // __instance is Shrine_HiddenStash which inherits from Shrine -> PropEnt -> Entity
                // Entity has a "position" property
                PropertyInfo positionProp = __instance.GetType().GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
                if (positionProp != null)
                {
                    dropPos = (Vector3)positionProp.GetValue(__instance, null);
                }
                else
                {
                    // Try getting it from transform
                    PropertyInfo transformProp = __instance.GetType().GetProperty("transform", BindingFlags.Instance | BindingFlags.Public);
                    if (transformProp != null)
                    {
                        Transform t = (Transform)transformProp.GetValue(__instance, null);
                        if (t != null) dropPos = t.position;
                    }
                }
            }
            catch { }
            
            // Create PRIVATE drops for EACH player (like monster loot)
            // Each player gets their own item so everyone benefits equally
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player == null || player.hero == null || player.hero.Status.isDead) continue;
                
                uint playerHeroNetId = player.hero.netId;
                
                // Select random item for THIS player (each player gets their own roll)
                RPGItem selectedItem = rarityPool[_random.Next(rarityPool.Count)];
                RPGItem itemClone = selectedItem.Clone();
                
                // Randomize stats Diablo-style!
                MonsterLootSystem.RandomizeItemStats(itemClone);
                
                // Roll for pre-upgraded drop (stash uses MiniBoss chances)
                MonsterLootSystem.RollForUpgradedDrop(itemClone, Monster.MonsterType.MiniBoss);
                
                // Offset drop position slightly for each player
                Vector3 playerDropPos = dropPos;
                playerDropPos.x += UnityEngine.Random.Range(-1f, 1f);
                playerDropPos.z += UnityEngine.Random.Range(-1f, 1f);
                playerDropPos = Dew.GetValidAgentDestination_LinearSweep(playerDropPos, playerDropPos);
                
                // Create PRIVATE drop for this specific player
                uint dropId = (uint)(0x90000000 + _random.Next(0x0FFFFFFF));
                NetworkedItemSystem.CreatePrivateDropForPlayer(itemClone, playerDropPos, dropId, playerHeroNetId);
                
                RPGLog.Debug(string.Format(" Hidden Stash dropped bonus RPG item for {0}: {1} ({2})", 
                    player.playerName, itemClone.name, itemClone.rarity));
            }
        }
    }
}

