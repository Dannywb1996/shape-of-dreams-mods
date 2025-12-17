using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using Mirror;

/// <summary>
/// Patches for Merchant Jonas to buy/sell custom RPG items.
/// All item definitions are now in ItemDatabase.cs
/// </summary>
public static class MerchantPatches
{
    // Number of columns for our custom shop layout
    public const int SHOP_COLUMNS = 6;
    
    // Cache the mod instance to avoid expensive FindFirstObjectByType calls
    private static RPGItemsMod _cachedModInstance = null;
    private static float _lastModInstanceCheck = 0f;
    private const float MOD_INSTANCE_CACHE_TIME = 5f; // Re-check every 5 seconds
    
    // Cache item lookups by ID for fast access
    private static Dictionary<int, RPGItem> _itemByIdCache = null;
    
    private static RPGItemsMod GetModInstance()
    {
        // Use cached instance if available and recent
        if (_cachedModInstance != null && Time.time - _lastModInstanceCheck < MOD_INSTANCE_CACHE_TIME)
        {
            return _cachedModInstance;
        }
        
        _cachedModInstance = UnityEngine.Object.FindFirstObjectByType<RPGItemsMod>();
        _lastModInstanceCheck = Time.time;
        return _cachedModInstance;
    }
    
    /// <summary>
    /// Clear the mod instance cache (call when leaving game)
    /// </summary>
    public static void ClearCache()
    {
        _cachedModInstance = null;
        _lastModInstanceCheck = 0f;
        _itemByIdCache = null;
        // Reset InfiniteDungeonMod detection so it re-checks on next merchant visit
        _infiniteDungeonChecked = false;
    }
    
    /// <summary>
    /// Build the item ID cache for fast lookups
    /// </summary>
    private static void EnsureItemIdCache()
    {
        if (_itemByIdCache != null) return;
        
        _itemByIdCache = new Dictionary<int, RPGItem>();
        
        List<RPGItem> allItems = ItemDatabase.GetAllItems();
        if (allItems != null)
        {
            foreach (RPGItem item in allItems)
            {
                if (item != null && !_itemByIdCache.ContainsKey(item.id))
                {
                    _itemByIdCache[item.id] = item;
                }
            }
        }
    }
    
    /// <summary>
    /// Patch PopulatePlayerMerchandises on base class to add custom RPG items
    /// This is called after OnPopulateMerchandises completes
    /// </summary>
    [HarmonyPatch(typeof(PropEnt_Merchant_Base), "PopulatePlayerMerchandises")]
    public static class PopulatePlayerMerchandisesPatch
    {
        public static void Postfix(PropEnt_Merchant_Base __instance, DewPlayer player)
        {
            // Only apply to Jonas merchant
            if (!(__instance is PropEnt_Merchant_Jonas))
            {
                return;
            }
            
            PropEnt_Merchant_Jonas jonas = (PropEnt_Merchant_Jonas)__instance;
            ReplaceWithCustomItems(jonas, player);
        }
    }
    
    /// <summary>
    /// Shared method to replace merchant items with custom items, memories, and essences
    /// Shop layout (by row, 6 items each to match grid):
    /// Row 1: 6 custom items
    /// Row 2: 6 memories (no identity/dodge)
    /// Row 3: 6 essences
    /// Custom items use same rarity chances as memories/essences
    /// </summary>
    public static void ReplaceWithCustomItems(PropEnt_Merchant_Jonas __instance, DewPlayer player)
    {
        try
        {
            if (!__instance.merchandises.ContainsKey(player.guid)) return;
            
            RPGItemsMod mod = GetModInstance();
            if (mod == null) return;
            
            // Get item templates from ItemDatabase
            List<RPGItem> itemTemplates = ItemDatabase.GetAllItems();
            if (itemTemplates == null || itemTemplates.Count == 0) return;
            
            // Group items by rarity for weighted selection (matching game's Rarity enum)
            Dictionary<Rarity, List<RPGItem>> itemsByRarity = new Dictionary<Rarity, List<RPGItem>>();
            itemsByRarity[Rarity.Common] = new List<RPGItem>();
            itemsByRarity[Rarity.Rare] = new List<RPGItem>();
            itemsByRarity[Rarity.Epic] = new List<RPGItem>();
            itemsByRarity[Rarity.Legendary] = new List<RPGItem>();
            
            foreach (RPGItem item in itemTemplates)
            {
                // Skip consumables - they go to Smoothie
                if (item.type == ItemType.Consumable) continue;
                
                Rarity gameRarity = ConvertToGameRarity(item.rarity);
                itemsByRarity[gameRarity].Add(item);
            }
            
            List<MerchandiseData> newMerchandise = new List<MerchandiseData>();
            LootManager lootManager = NetworkedManagerBase<LootManager>.instance;
            
            // Fixed 6 items per row (Custom, Memories, Essences)
            // We ignore shopBonus - always 6 per category for clean 3-row layout
            
            // === ROW 1: 6 CUSTOM RPG ITEMS ===
            int customItemCount = SHOP_COLUMNS;
            for (int i = 0; i < customItemCount; i++)
            {
                // Use game's skill rarity selection for consistency
                Rarity selectedRarity = lootManager != null ? lootManager.SelectSkillRarity() : Rarity.Common;
                
                // Get items of that rarity, fallback to any rarity if empty
                List<RPGItem> pool = itemsByRarity[selectedRarity];
                if (pool.Count == 0)
                {
                    // Fallback: try other rarities
                    foreach (var kvp in itemsByRarity)
                    {
                        if (kvp.Value.Count > 0)
                        {
                            pool = kvp.Value;
                            break;
                        }
                    }
                }
                
                if (pool.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, pool.Count);
                    RPGItem item = pool[randomIndex];
                    
                    MerchandiseData customMerch = new MerchandiseData();
                    customMerch.type = MerchandiseType.Treasure;
                    customMerch.itemName = "RPGItem_" + item.id;
                    
                    // Scale upgrade level with zone progression (like memories/essences)
                    // Use the game's skill level formula as reference for scaling
                    int upgradeLevel = GetScaledUpgradeLevel(lootManager, selectedRarity);
                    customMerch.level = upgradeLevel;
                    customMerch.count = 1;
                    
                    // Calculate price based on upgrade level
                    // Clone item and apply upgrades for correct pricing (stats affect price)
                    RPGItem pricingItem = item.Clone();
                    for (int upg = 0; upg < upgradeLevel; upg++)
                    {
                        InventoryMerchantHelper.UpgradeItem(pricingItem, 0);
                    }
                    // Use buy price directly (like game uses SkillTrigger.GetBuyGold)
                    int buyPrice = InventoryMerchantHelper.GetBuyPrice(pricingItem);
                    customMerch.price = Cost.Gold(buyPrice);
                    
                    RPGLog.Debug(string.Format("[Merchant] Created {0} +{1} with price {2}g", item.name, upgradeLevel, buyPrice));
                    
                    newMerchandise.Add(customMerch);
                }
            }
            
            // === ROW 2: 6 MEMORIES (Skills) - No identity or dodge ===
            int memoryCount = SHOP_COLUMNS;
            if (lootManager != null)
            {
                for (int i = 0; i < memoryCount; i++)
                {
                    MerchandiseData memoryMerch = GetMemoryMerchandise(lootManager);
                    if (memoryMerch.itemName != null)
                    {
                        newMerchandise.Add(memoryMerch);
                    }
                }
            }
            
            // === ROW 3: 6 ESSENCES (Gems) ===
            int essenceCount = SHOP_COLUMNS;
            if (lootManager != null)
            {
                for (int i = 0; i < essenceCount; i++)
                {
                    MerchandiseData essenceMerch = GetEssenceMerchandise(lootManager);
                    if (essenceMerch.itemName != null)
                    {
                        newMerchandise.Add(essenceMerch);
                    }
                }
            }
            
            __instance.merchandises[player.guid] = newMerchandise.ToArray();
            
            // Debug: Log the order of items
            string debugOrder = "[RPGItemsMod] Shop order: ";
            for (int i = 0; i < newMerchandise.Count; i++)
            {
                MerchandiseData m = newMerchandise[i];
                string typeChar = m.type == MerchandiseType.Treasure ? "C" : (m.type == MerchandiseType.Skill ? "M" : "E");
                debugOrder += typeChar;
                if ((i + 1) % SHOP_COLUMNS == 0) debugOrder += "|";
            }
            RPGLog.Debug(debugOrder);
            RPGLog.Debug(string.Format(" Shop populated: {0} items (Custom:{1}, Memory:{2}, Essence:{3})", 
                newMerchandise.Count, customItemCount, memoryCount, essenceCount));
        }
        catch (Exception e)
        {
            RPGLog.Error(" Merchant patch error: " + e.Message + "\n" + e.StackTrace);
        }
    }
    
    /// <summary>
    /// Convert mod's ItemRarity to game's Rarity enum
    /// </summary>
    private static Rarity ConvertToGameRarity(ItemRarity itemRarity)
    {
        switch (itemRarity)
        {
            case ItemRarity.Common: return Rarity.Common;
            case ItemRarity.Rare: return Rarity.Rare;
            case ItemRarity.Epic: return Rarity.Epic;
            case ItemRarity.Legendary: return Rarity.Legendary;
            default: return Rarity.Common;
        }
    }
    
    /// <summary>
    /// Get a memory (skill) merchandise - excludes identity and movement skills
    /// </summary>
    private static MerchandiseData GetMemoryMerchandise(LootManager lootManager)
    {
        MerchandiseData merch = new MerchandiseData();
        merch.type = MerchandiseType.Skill;
        
        try
        {
            Rarity rarity = lootManager.SelectSkillRarity();
            SkillTrigger skill;
            int level;
            lootManager.SelectSkillAndLevel(rarity, out skill, out level);
            
            // The game's pool already excludes isCharacterSkill (identity, dodge) via excludeFromPool
            merch.itemName = skill.GetType().Name;
            merch.level = level;
            merch.count = 1;
            merch.price = Cost.Gold(SkillTrigger.GetBuyGold(skill.rarity, level));
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to get memory: " + e.Message);
            merch.itemName = null;
        }
        
        return merch;
    }
    
    /// <summary>
    /// Get an essence (gem) merchandise
    /// </summary>
    private static MerchandiseData GetEssenceMerchandise(LootManager lootManager)
    {
        MerchandiseData merch = new MerchandiseData();
        merch.type = MerchandiseType.Gem;
        
        try
        {
            Rarity rarity = lootManager.SelectGemRarity();
            Gem gem;
            int quality;
            lootManager.SelectGemAndQuality(rarity, out gem, out quality);
            
            merch.itemName = gem.GetType().Name;
            merch.level = quality;
            merch.count = 1;
            merch.price = Cost.Gold(Gem.GetBuyGold(gem.rarity, quality));
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to get essence: " + e.Message);
            merch.itemName = null;
        }
        
        return merch;
    }
    
    /// <summary>
    /// Get scaled upgrade level for custom items based on progression.
    /// Uses zone index for normal game, depth for InfiniteDungeonMod, difficulty for SurvivalWaveMod.
    /// Higher rarity items have better chance for higher upgrade levels.
    /// </summary>
    private static int GetScaledUpgradeLevel(LootManager lootManager, Rarity rarity)
    {
        try
        {
            int baseLevel;
            
            // Check if InfiniteDungeonMod is active - use depth-based scaling
            int infiniteDungeonDepth = GetInfiniteDungeonDepth();
            
            if (infiniteDungeonDepth > 0)
            {
                // Depth-based scaling for Infinite Dungeon - NO CAP!
                // +1 upgrade level every 8 depths
                baseLevel = (infiniteDungeonDepth - 1) / 8;
            }
            else
            {
                // Check if SurvivalWaveMod is active - use difficulty-based scaling
                int survivalDifficulty = GetSurvivalWaveDifficulty();
                
                if (survivalDifficulty > 0)
                {
                    // Difficulty-based scaling for Survival Wave - NO CAP!
                    // +1 upgrade level every 5 difficulty levels
                    baseLevel = (survivalDifficulty - 1) / 5;
                }
                else
                {
                    // Normal game: zone-based scaling
                    int zoneIndex = 0;
                    if (NetworkedManagerBase<ZoneManager>.instance != null)
                    {
                        zoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
                    }
                    // Zone 0-1: 0, Zone 2-3: 0-1, Zone 4-5: 1-2, Zone 6+: 2-3
                    baseLevel = Mathf.Clamp(zoneIndex / 2, 0, 3);
                }
            }
            
            // Add randomness: 50% chance to be +1 higher
            int upgradeLevel = baseLevel;
            if (UnityEngine.Random.value < 0.5f)
            {
                upgradeLevel++;
            }
            
            // Higher rarity = better chance for even higher upgrade
            float bonusChance = 0f;
            switch (rarity)
            {
                case Rarity.Rare: bonusChance = 0.20f; break;
                case Rarity.Epic: bonusChance = 0.35f; break;
                case Rarity.Legendary: bonusChance = 0.50f; break;
            }
            
            if (UnityEngine.Random.value < bonusChance)
            {
                upgradeLevel++;
            }
            
            return upgradeLevel;
        }
        catch
        {
            return 0; // Fallback to +0
        }
    }
    
    // Cache for InfiniteDungeonMod detection
    private static bool _infiniteDungeonChecked = false;
    private static System.Type _infiniteDungeonModType = null;
    private static System.Reflection.MethodInfo _getTotalDepthMethod = null;
    private static System.Reflection.FieldInfo _isModActiveField = null;
    
    // Cache for SurvivalWaveMod detection
    private static bool _survivalWaveChecked = false;
    private static System.Type _survivalWaveModType = null;
    private static System.Type _waveSystemType = null;
    private static System.Reflection.PropertyInfo _isWaveModeActiveProperty = null;
    private static System.Reflection.PropertyInfo _survivalDifficultyProperty = null;
    
    /// <summary>
    /// Get the current depth from InfiniteDungeonMod if active.
    /// Returns 0 if InfiniteDungeonMod is not active or not available.
    /// </summary>
    private static int GetInfiniteDungeonDepth()
    {
        // Try to detect InfiniteDungeonMod if not checked yet
        if (!_infiniteDungeonChecked)
        {
            _infiniteDungeonChecked = true;
            try
            {
                // Look for InfiniteDungeonMod type in all loaded assemblies
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    _infiniteDungeonModType = assembly.GetType("InfiniteDungeonMod.InfiniteDungeonMod");
                    if (_infiniteDungeonModType != null)
                    {
                        // IsModActive is a FIELD, not a property!
                        _isModActiveField = _infiniteDungeonModType.GetField("IsModActive", 
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        _getTotalDepthMethod = _infiniteDungeonModType.GetMethod("GetTotalDepth", 
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                RPGLog.Warning("[RPGItemsMod] Error detecting InfiniteDungeonMod: " + ex.Message);
            }
        }
        
        // If InfiniteDungeonMod is detected and active, return depth
        if (_infiniteDungeonModType != null && _isModActiveField != null && _getTotalDepthMethod != null)
        {
            try
            {
                // IsModActive is a field, use GetValue(null) for static fields
                bool isModActive = (bool)_isModActiveField.GetValue(null);
                if (isModActive)
                {
                    int depth = (int)_getTotalDepthMethod.Invoke(null, null);
                    if (depth > 0)
                    {
                        return depth;
                    }
                }
            }
            catch (Exception ex)
            {
                RPGLog.Warning("[RPGItemsMod] Error getting depth from InfiniteDungeonMod: " + ex.Message);
            }
        }
        
        return 0; // Not in infinite dungeon
    }
    
    /// <summary>
    /// Get the current difficulty from SurvivalWaveMod if active.
    /// Returns 0 if SurvivalWaveMod is not active or not available.
    /// </summary>
    private static int GetSurvivalWaveDifficulty()
    {
        // Try to detect SurvivalWaveMod if not checked yet
        if (!_survivalWaveChecked)
        {
            _survivalWaveChecked = true;
            try
            {
                // Look for SurvivalWaveMod types in all loaded assemblies
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    _survivalWaveModType = assembly.GetType("SurvivalWaveMod.SurvivalWaveMod");
                    if (_survivalWaveModType != null)
                    {
                        _isWaveModeActiveProperty = _survivalWaveModType.GetProperty("IsWaveModeActive", 
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        
                        // WaveSystem has SurvivalDifficulty
                        _waveSystemType = assembly.GetType("SurvivalWaveMod.WaveSystem");
                        if (_waveSystemType != null)
                        {
                            _survivalDifficultyProperty = _waveSystemType.GetProperty("SurvivalDifficulty", 
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                RPGLog.Warning("[RPGItemsMod] Error detecting SurvivalWaveMod: " + ex.Message);
            }
        }
        
        // If SurvivalWaveMod is detected and active, return difficulty
        if (_survivalWaveModType != null && _isWaveModeActiveProperty != null && _survivalDifficultyProperty != null)
        {
            try
            {
                bool isWaveModeActive = (bool)_isWaveModeActiveProperty.GetValue(null, null);
                if (isWaveModeActive)
                {
                    int difficulty = (int)_survivalDifficultyProperty.GetValue(null, null);
                    if (difficulty > 0)
                    {
                        return difficulty;
                    }
                }
            }
            catch (Exception ex)
            {
                RPGLog.Warning("[RPGItemsMod] Error getting difficulty from SurvivalWaveMod: " + ex.Message);
            }
        }
        
        return 0; // Not in survival wave mode
    }
    
    /// <summary>
    /// Also keep the original patch for OnPopulateMerchandises as backup
    /// </summary>
    [HarmonyPatch(typeof(PropEnt_Merchant_Jonas), "OnPopulateMerchandises")]
    public static class OnPopulateMerchandisesPatch
    {
        public static void Postfix(PropEnt_Merchant_Jonas __instance, DewPlayer player)
        {
            ReplaceWithCustomItems(__instance, player);
        }
        
        private static List<RPGItem> GetAvailableItemsForMerchant(InventoryManager invManager)
        {
            List<RPGItem> items = new List<RPGItem>();
            
            // Get all non-null items from inventory
            List<RPGItem> allItems = invManager.GetAllItems();
            
            // Get unique items by ID (items that can be sold)
            HashSet<int> seenIds = new HashSet<int>();
            
            foreach (RPGItem item in allItems)
            {
                if (item != null && !seenIds.Contains(item.id))
                {
                    // Don't include consumables, materials, or quest items
                    if (item.type != ItemType.Consumable && 
                        item.type != ItemType.Material && 
                        item.type != ItemType.Quest)
                    {
                        items.Add(item);
                        seenIds.Add(item.id);
                    }
                }
            }
            
            return items;
        }
    }
    
    // Shared helper method to get available items for merchant
    public static List<RPGItem> GetAvailableItemsForMerchantStatic(InventoryManager invManager)
    {
        List<RPGItem> items = new List<RPGItem>();
        
        // Get all non-null items from inventory
        List<RPGItem> allItems = invManager.GetAllItems();
        
        // Get unique items by ID (items that can be sold)
        HashSet<int> seenIds = new HashSet<int>();
        
        foreach (RPGItem item in allItems)
        {
            if (item != null && !seenIds.Contains(item.id))
            {
                // Don't include consumables, materials, or quest items
                if (item.type != ItemType.Consumable && 
                    item.type != ItemType.Material && 
                    item.type != ItemType.Quest)
                {
                    items.Add(item);
                    seenIds.Add(item.id);
                }
            }
        }
        
        return items;
    }
    
    // Shared helper method - optimized with caching
    public static RPGItem FindItemById(InventoryManager invManager, int itemId)
    {
        // First try the fast cache lookup for templates
        EnsureItemIdCache();
        RPGItem cached;
        if (_itemByIdCache != null && _itemByIdCache.TryGetValue(itemId, out cached))
        {
            return cached;
        }
        
        // If not in cache, check inventory (player's actual items may differ from templates)
        if (invManager != null)
        {
            foreach (RPGItem item in invManager.GetAllItems())
            {
                if (item != null && item.id == itemId)
                {
                    return item;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Patch UpdateItemPrices to handle custom RPG items pricing
    /// </summary>
    [HarmonyPatch(typeof(PropEnt_Merchant_Jonas), "UpdateItemPrices")]
    public static class UpdateItemPricesPatch
    {
        public static void Postfix(PropEnt_Merchant_Jonas __instance, MerchandiseData[] arr)
        {
            if (arr == null) return;
            
            RPGItemsMod mod = GetModInstance();
            if (mod == null) return;
            
            InventoryManager invManager = mod.GetInventoryManager();
            if (invManager == null) return;
            
            for (int i = 0; i < arr.Length; i++)
            {
                MerchandiseData temp = arr[i];
                
                // Check if this is a custom RPG item
                if (temp.type == MerchandiseType.Treasure && temp.itemName.StartsWith("RPGItem_"))
                {
                    string idStr = temp.itemName.Substring(8);
                    int itemId;
                    if (int.TryParse(idStr, out itemId))
                    {
                        // Find the item
                        RPGItem item = FindItemById(invManager, itemId);
                        
                        if (item != null)
                        {
                            // Clone and apply upgrade stats for correct pricing
                            RPGItem pricingItem = item.Clone();
                            for (int upg = 0; upg < temp.level; upg++)
                            {
                                InventoryMerchantHelper.UpgradeItem(pricingItem, 0);
                            }
                            // Use buy price directly (like game uses SkillTrigger.GetBuyGold)
                            int buyPrice = InventoryMerchantHelper.GetBuyPrice(pricingItem);
                            RPGLog.Debug(string.Format("[UpdateItemPrices] {0} +{1} -> price {2}g", item.name, temp.level, buyPrice));
                            temp.price = Cost.Gold(buyPrice);
                            arr[i] = temp;
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Patch SpawnMerchandise to handle custom RPG items
    /// NOTE: This is a fallback - CmdPurchasePatch should handle most purchases
    /// </summary>
    [HarmonyPatch(typeof(PropEnt_Merchant_Base), "SpawnMerchandise")]
    public static class SpawnMerchandisePatch
    {
        public static bool Prefix(PropEnt_Merchant_Base __instance, MerchandiseData data, DewPlayer player, Cost finalPrice)
        {
            // Check if this is a custom RPG item
            if (data.type == MerchandiseType.Treasure && data.itemName != null && data.itemName.StartsWith("RPGItem_"))
            {
                RPGItemsMod mod = GetModInstance();
                if (mod != null)
                {
                    // Parse item ID from name (format: "RPGItem_123")
                    string idStr = data.itemName.Substring(8);
                    int itemId;
                    if (int.TryParse(idStr, out itemId))
                    {
                        // Get item template from database (not from inventory!)
                        RPGItem item = FindItemByIdFromDatabase(itemId);
                        if (item != null)
                        {
                            // Clone the item to get a fresh copy with base stats
                            RPGItem clonedItem = item.Clone();
                            
                            // Apply upgrade stats for the purchased level
                            // data.level contains the upgrade level from merchandise
                            int targetLevel = data.level;
                            for (int i = 0; i < targetLevel; i++)
                            {
                                InventoryMerchantHelper.UpgradeItem(clonedItem, 0); // 0 dust cost for shop items
                            }
                            
                            // Add to player's inventory
                            InventoryManager invManager = mod.GetInventoryManager();
                            if (invManager != null)
                            {
                                invManager.AddItem(clonedItem);
                            }
                            
                            // Spawn effect
                            Vector3 pos = __instance.agentPosition + (player.hero.agentPosition - __instance.agentPosition).normalized * 3f + UnityEngine.Random.insideUnitSphere.Flattened();
                            pos = Dew.GetValidAgentDestination_LinearSweep(player.hero.agentPosition, pos);
                            
                            // Play purchase effect
                            if (__instance.fxPurchase != null)
                            {
                                DewEffect.PlayNew(__instance.fxPurchase, player.hero);
                            }
                            
                            NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemBought(player.hero, null);
                            
                            return false; // Skip original method
                        }
                    }
                }
            }
            
            return true; // Continue with original method
        }
    }
    
    /// <summary>
    /// Patch CmdPurchase to intercept custom RPG item purchases before game tries to resolve them
    /// SERVER-SIDE: This runs on the host when any player (including clients) buys an item
    /// </summary>
    [HarmonyPatch(typeof(PropEnt_Merchant_Base), "UserCode_CmdPurchase__Int32__NetworkConnectionToClient")]
    public static class CmdPurchasePatch
    {
        public static bool Prefix(PropEnt_Merchant_Base __instance, int index, Mirror.NetworkConnectionToClient sender)
        {
            try
            {
                DewPlayer player = sender.GetPlayer();
                if (player == null || player.hero == null) return true;
                
                // Get merchandise array for this player
                MerchandiseData[] arr;
                if (!__instance.merchandises.TryGetValue(player.guid, out arr) || index < 0 || index >= arr.Length)
                {
                    return true;
                }
                
                MerchandiseData item = arr[index];
                
                // Check if this is a custom RPG item
                if (item.type == MerchandiseType.Treasure && item.itemName != null && item.itemName.StartsWith("RPGItem_"))
                {
                    // Handle custom item purchase
                    if (item.count <= 0) return false; // Already sold out
                    
                    // Calculate price
                    Cost price = item.price.MultiplyGold(player.buyPriceMultiplier);
                    
                    // Check if player can afford
                    if (price.CanAfford(player.hero) != AffordType.Yes) return false;
                    
                    // Spend resources
                    player.Spend(price);
                    
                    // Decrease count
                    MerchandiseData updated = item;
                    updated.count--;
                    arr[index] = updated;
                    
                    // Parse item ID
                    string idStr = item.itemName.Substring(8);
                    int itemId;
                    if (int.TryParse(idStr, out itemId))
                    {
                        // Get item template from database (not from host's inventory!)
                        RPGItem rpgItem = FindItemByIdFromDatabase(itemId);
                        if (rpgItem != null)
                        {
                            // Clone the item
                            RPGItem clonedItem = rpgItem.Clone();
                            
                            // Apply upgrade stats for the purchased level
                            // The item from database has base stats (+0), we need to upgrade it to the purchased level
                            int targetLevel = item.level;
                            for (int upg = 0; upg < targetLevel; upg++)
                            {
                                InventoryMerchantHelper.UpgradeItem(clonedItem, 0); // 0 dust cost for shop items
                            }
                            RPGLog.Debug(string.Format("[CmdPurchase] Applied {0} upgrades to {1}, final stats: ATK={2}, DEF={3}, HP={4}", 
                                targetLevel, clonedItem.name, clonedItem.attackBonus, clonedItem.defenseBonus, clonedItem.healthBonus));
                            
                            // Check if buyer is the HOST or a CLIENT
                            bool isLocalPlayer = (DewPlayer.local != null && player.guid == DewPlayer.local.guid);
                            
                            if (isLocalPlayer)
                            {
                                // HOST buying: Add directly to host's inventory
                                // For consumables, use the callback which handles fast slot auto-stacking
                                if (clonedItem.type == ItemType.Consumable && NetworkedItemSystem.OnItemReceivedLocally != null)
                                {
                                    NetworkedItemSystem.OnItemReceivedLocally(clonedItem);
                                }
                                else
                                {
                                    RPGItemsMod mod = GetModInstance();
                                    if (mod != null)
                                    {
                                        InventoryManager invManager = mod.GetInventoryManager();
                                        if (invManager != null)
                                        {
                                            invManager.AddItem(clonedItem);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // CLIENT buying: Send item to client via RPC
                                SendPurchasedItemToClient(player, clonedItem);
                            }
                                    
                                    // Play purchase effect
                                    if (__instance.fxPurchase != null)
                                    {
                                        DewEffect.PlayNew(__instance.fxPurchase, player.hero);
                                    }
                                    
                                    NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemBought(player.hero, null);
                        }
                        else
                        {
                            // Item not found in database - refund the player!
                            RPGLog.Error(" Item ID " + itemId + " not found in database! Refunding player.");
                            // Restore the count
                            MerchandiseData restored = arr[index];
                            restored.count++;
                            arr[index] = restored;
                            // Refund gold (approximate - we can't easily get the exact price back)
                            player.AddGold(item.price.gold);
                        }
                    }
                    else
                    {
                        // Failed to parse item ID - refund
                        RPGLog.Error(" Failed to parse item ID from: " + item.itemName);
                        MerchandiseData restored = arr[index];
                        restored.count++;
                        arr[index] = restored;
                        player.AddGold(item.price.gold);
                    }
                    
                    return false; // Skip original method entirely
                }
            }
            catch (System.Exception e)
            {
                RPGLog.Warning(" CmdPurchase patch error: " + e.Message);
            }
            
            return true; // Continue with original for non-custom items
        }
        
        /// <summary>
        /// Send purchased item to client via NetworkedItemSystem's RPC
        /// </summary>
        private static void SendPurchasedItemToClient(DewPlayer player, RPGItem item)
        {
            try
            {
                if (player == null || player.hero == null)
                {
                    RPGLog.Error(" SendPurchasedItemToClient: player or hero is null!");
                    return;
                }
                
                if (item == null)
                {
                    RPGLog.Error(" SendPurchasedItemToClient: item is null!");
                    return;
                }
                
                // Serialize item to JSON
                NetworkedItemSystem.SerializableItem sItem = NetworkedItemSystem.SerializableItem.FromRPGItem(item);
                if (sItem == null)
                {
                    RPGLog.Error(" SendPurchasedItemToClient: Failed to serialize item!");
                    return;
                }
                
                string itemJson = sItem.ToJson();
                if (string.IsNullOrEmpty(itemJson))
                {
                    RPGLog.Error(" SendPurchasedItemToClient: itemJson is empty!");
                    return;
                }
                
                // Get the serverActor for RPC
                ActorManager am = NetworkedManagerBase<ActorManager>.instance;
                if (am == null || am.serverActor == null)
                {
                    RPGLog.Error(" SendPurchasedItemToClient: ActorManager or serverActor is null!");
                    return;
                }
                
                // Create purchase message
                NetworkedItemSystem.PrivateDropMessage msg = new NetworkedItemSystem.PrivateDropMessage();
                msg.dropId = 0xFFFFFFFF; // Special ID indicating "add to inventory directly"
                msg.itemJson = itemJson;
                msg.posX = 0;
                msg.posY = 0;
                msg.posZ = 0;
                
                // Send to specific client
                am.serverActor.CustomRpc_SendMessageToClient(player, msg);
            }
            catch (System.Exception e)
            {
                RPGLog.Error(" Failed to send item to client: " + e.Message + "\n" + e.StackTrace);
            }
        }
    }
    
    /// <summary>
    /// Find item by ID from ItemDatabase (not from any player's inventory)
    /// </summary>
    private static RPGItem FindItemByIdFromDatabase(int itemId)
    {
        // First try the fast cache lookup for templates
        EnsureItemIdCache();
        RPGItem cached;
        if (_itemByIdCache != null && _itemByIdCache.TryGetValue(itemId, out cached))
        {
            return cached;
        }
        
        // Fallback: search ItemDatabase directly
        System.Collections.Generic.List<RPGItem> allItems = ItemDatabase.GetAllItems();
        if (allItems != null)
        {
            foreach (RPGItem item in allItems)
            {
                if (item != null && item.id == itemId)
                {
                    return item;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the full RPGItem for a custom item in shop (for ping messages)
    /// </summary>
    public static RPGItem GetRPGItemFromMerchandise(string itemName)
    {
        if (string.IsNullOrEmpty(itemName) || !itemName.StartsWith("RPGItem_"))
            return null;
            
        string idStr = itemName.Substring(8);
        int itemId;
        if (!int.TryParse(idStr, out itemId))
            return null;
            
        RPGItemsMod mod = GetModInstance();
        if (mod == null) return null;
        
        InventoryManager invManager = mod.GetInventoryManager();
        if (invManager == null) return null;
        
        return FindItemById(invManager, itemId);
    }
}

/// <summary>
/// Patch for Shop UI to force 6 columns for proper row separation
/// </summary>
public static class ShopUIPatches
{
    /// <summary>
    /// Patch UpdateMerchandise to override grid constraintCount
    /// This ensures our 3 rows (Custom, Memories, Essences) display correctly
    /// </summary>
    [HarmonyPatch(typeof(UI_InGame_FloatingWindow_Shop), "UpdateMerchandise")]
    public static class UpdateMerchandisePatch
    {
        public static void Postfix(UI_InGame_FloatingWindow_Shop __instance)
        {
            try
            {
                // Always force 6 columns for our custom shop layout
                if (__instance.grid == null) return;
                
                MonoBehaviour target = ManagerBase<FloatingWindowManager>.instance.currentTarget;
                if (target == null) return;
                
                PropEnt_Merchant_Jonas merchant = target as PropEnt_Merchant_Jonas;
                if (merchant == null) return;
                if (merchant.IsNullInactiveDeadOrKnockedOut()) return;
                if (!merchant.merchandises.ContainsKey(DewPlayer.local.guid)) return;
                
                // Always force 6 columns - this ensures proper row separation
                // regardless of how many items are in the shop (constellations can add more)
                __instance.grid.constraintCount = MerchantPatches.SHOP_COLUMNS;
            }
            catch (Exception e)
            {
                RPGLog.Warning(" Error in shop UI patch: " + e.Message);
            }
        }
    }
}

/// <summary>
/// Patches for Merchant Smoothie to add consumables
/// Layout:
/// - Row 1: 3 Consumables (our mod's potions)
/// - Row 2: 3 Treasures (his original items)
/// No shopBonus applied
/// </summary>
public static class SmoothieMerchantPatches
{
    public const int SMOOTHIE_COLUMNS = 3;
    
    /// <summary>
    /// Patch OnPopulateMerchandises to add consumables before treasures
    /// </summary>
    [HarmonyPatch(typeof(PropEnt_Merchant_Smoothie), "OnPopulateMerchandises")]
    public static class SmoothiePopulatePatch
    {
        public static void Postfix(PropEnt_Merchant_Smoothie __instance, DewPlayer player)
        {
            try
            {
                AddConsumablesToSmoothie(__instance, player);
            }
            catch (Exception e)
            {
                RPGLog.Warning(" Smoothie patch error: " + e.Message);
            }
        }
    }
    
    /// <summary>
    /// Add consumables to Smoothie's merchandise
    /// </summary>
    private static void AddConsumablesToSmoothie(PropEnt_Merchant_Smoothie merchant, DewPlayer player)
    {
        if (!merchant.merchandises.ContainsKey(player.guid)) return;
        
        // Get existing merchandise (treasures)
        MerchandiseData[] existingItems = merchant.merchandises[player.guid];
        
        // Get consumable templates from ItemDatabase
        List<RPGItem> consumables = ItemDatabase.GetConsumables();
        if (consumables == null || consumables.Count == 0) return;
        
        // Build new merchandise list: Consumables first, then original treasures
        List<MerchandiseData> newMerchandise = new List<MerchandiseData>();
        
        // Add 3 random consumables
        List<RPGItem> availableConsumables = new List<RPGItem>(consumables);
        for (int i = 0; i < SMOOTHIE_COLUMNS && availableConsumables.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableConsumables.Count);
            RPGItem consumable = availableConsumables[randomIndex];
            availableConsumables.RemoveAt(randomIndex);
            
            MerchandiseData consumableMerch = new MerchandiseData();
            consumableMerch.type = MerchandiseType.Treasure;
            consumableMerch.itemName = "RPGItem_" + consumable.id;
            consumableMerch.level = 0;
            consumableMerch.count = 3; // Potions come in stacks of 3
            consumableMerch.price = Cost.Gold(GetConsumablePrice(consumable));
            
            newMerchandise.Add(consumableMerch);
        }
        
        // Add original treasures (limit to 3, no shopBonus)
        // Filter out any RPGItem entries (in case patch runs twice or there's overlap)
        int treasureCount = 0;
        foreach (MerchandiseData item in existingItems)
        {
            if (treasureCount >= SMOOTHIE_COLUMNS) break;
            
            // Skip if this is an RPG item (our consumables use "RPGItem_X" format)
            if (item.itemName != null && item.itemName.StartsWith("RPGItem_"))
                continue;
            
            newMerchandise.Add(item);
            treasureCount++;
        }
        
        merchant.merchandises[player.guid] = newMerchandise.ToArray();
        
        RPGLog.Debug(string.Format(" Smoothie shop: {0} consumables + {1} original treasures (filtered from {2} existing items)", 
            SMOOTHIE_COLUMNS, treasureCount, existingItems.Length));
    }
    
    /// <summary>
    /// Get price for consumable based on rarity
    /// </summary>
    private static int GetConsumablePrice(RPGItem consumable)
    {
        switch (consumable.rarity)
        {
            case ItemRarity.Common: return 50;
            case ItemRarity.Rare: return 150;
            case ItemRarity.Epic: return 400;
            case ItemRarity.Legendary: return 1000;
            default: return 100;
        }
        }
    }
    
    /// <summary>
/// Patch PingManager.Ping.IsValid to fix Smoothie merchant pings
/// The game's original code only checks for PropEnt_Merchant_Jonas, not PropEnt_Merchant_Base
/// This causes pings on Smoothie's shop to be immediately destroyed
    /// </summary>
public static class PingIsValidPatch
{
    /// <summary>
    /// Postfix to fix IsValid for Smoothie merchant
    /// </summary>
    public static void Postfix(ref bool __result, ref PingManager.Ping __instance)
            {
        // Only fix ShopItem pings that failed validation
        if (__result) return;
        if (__instance.type != PingManager.PingType.ShopItem) return;
        
        // Check if it's a PropEnt_Merchant_Base (includes both Jonas and Smoothie)
        PropEnt_Merchant_Base merchant = __instance.target as PropEnt_Merchant_Base;
        if (merchant != null && merchant.isActive)
        {
            __result = true;
                    }
                }
    }
    
    /// <summary>
/// Patch PingManager.ShowPingChatMessage to handle custom RPG item pings in shop
    /// </summary>
public static class PingManagerChatPatch
{
    // Track if we've already shown a message for this ping to prevent duplicates
    private static uint _lastPingSenderNetId = 0;
    private static int _lastPingItemIndex = -1;
    private static float _lastPingTime = 0f;
    
    /// <summary>
    /// Prefix to intercept shop item pings for custom RPG items
    /// </summary>
    public static bool Prefix(PingManager __instance, PingManager.Ping ping)
    {
        try
        {
            // Only handle ShopItem pings
            if (ping.type != PingManager.PingType.ShopItem)
                return true; // Let original handle it
                
            PropEnt_Merchant_Base merchant = ping.target as PropEnt_Merchant_Base;
            if (merchant == null)
                return true;
                
            // Get the merchandise data
            MerchandiseData[] merchandises;
            if (!merchant.merchandises.TryGetValue(ping.sender.guid, out merchandises))
                return true;
                
            if (ping.itemIndex < 0 || ping.itemIndex >= merchandises.Length)
                return true;
                
            MerchandiseData merchItem = merchandises[ping.itemIndex];
            
            // Check if this is a custom RPG item
            if (merchItem.type != MerchandiseType.Treasure || merchItem.itemName == null || !merchItem.itemName.StartsWith("RPGItem_"))
                return true; // Let original handle non-RPG items
            
            // Prevent duplicate messages - check if this is the same ping we just processed
            // This can happen because the host receives the RPC even though they sent it
            float currentTime = UnityEngine.Time.realtimeSinceStartup;
            if (ping.sender.netId == _lastPingSenderNetId && 
                ping.itemIndex == _lastPingItemIndex && 
                currentTime - _lastPingTime < 0.5f) // Within 0.5 seconds = duplicate
            {
                // This is a duplicate ping, skip it entirely
                return false;
            }
            
            // Update tracking
            _lastPingSenderNetId = ping.sender.netId;
            _lastPingItemIndex = ping.itemIndex;
            _lastPingTime = currentTime;
                
            // Get custom item info
            RPGItem item = MerchantPatches.GetRPGItemFromMerchandise(merchItem.itemName);
            if (item == null)
                return true; // Fallback to original if item not found
                
            string itemName = item.GetDisplayName();
            string rarityColor = Dew.GetHex(item.GetRarityColor());
            
            // Build the chat message similar to how the game does it
            string template = PingManager.GetPingUIValue(
                (merchItem.count > 0) ? "Chat_Ping_ShopItem_HasStock" : "Chat_Ping_ShopItem_OutOfStock", 
                rarityColor);
            string content = "<color=#a7bfc4>" + string.Format(template, itemName, merchItem.count, merchItem.price) + "</color>";
            
            // Serialize item data for tooltip
            string itemCustomData = NetworkedItemSystem.SerializeItemForTooltip(item);
            
            // Create and show the chat message
            ChatManager.Message msg = new ChatManager.Message();
            msg.type = ChatManager.MessageType.Chat;
            msg.content = "<color=#bcccd1>" + content + "</color>";
            msg.args = new string[1] { ping.sender.netId.ToString() };
            msg.itemType = merchItem.itemName;
            msg.itemLevel = merchItem.level;
            msg.itemPrice = merchItem.price;
            msg.itemCustomData = itemCustomData;
            
            NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(msg);
            
            // Skip original method - we've handled it
            return false;
        }
        catch (System.Exception e)
        {
            RPGLog.Warning(" PingManagerChatPatch error: " + e.Message);
            return true; // Let original handle it on error
        }
    }
}
