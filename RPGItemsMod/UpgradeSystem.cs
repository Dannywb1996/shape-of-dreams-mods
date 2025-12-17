using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

/// <summary>
/// Patches for Upgrade Well to upgrade custom RPG items
/// </summary>
public static class UpgradeWellPatches
{
    /// <summary>
    /// Patch GetTargetInfo to support custom RPG items
    /// We'll add a custom interaction mode in our inventory UI instead
    /// </summary>
    [HarmonyPatch(typeof(Shrine_UpgradeWell), "GetTargetInfo", new Type[] { typeof(SkillTrigger) })]
    public static class GetTargetInfoSkillPatch
    {
        static void Postfix(SkillTrigger target, ref EditSkillTargetInfo __result)
        {
            // This is for skills, not our items
        }
    }
    
    [HarmonyPatch(typeof(Shrine_UpgradeWell), "GetTargetInfo", new Type[] { typeof(Gem) })]
    public static class GetTargetInfoGemPatch
    {
        static void Postfix(Gem target, ref EditSkillTargetInfo __result)
        {
            // This is for gems, not our items
        }
    }
}

/// <summary>
/// Patches for Merchant Shop UI to display custom RPG items
/// </summary>
public static class MerchantShopUIPatches
{
    private static RPGItemsMod GetModInstance()
    {
        return UnityEngine.Object.FindFirstObjectByType<RPGItemsMod>();
    }
    
    /// <summary>
    /// Patch UpdateContent to display custom RPG items in merchant shop
    /// Use Prefix to intercept BEFORE the game tries to load the item from resources
    /// </summary>
    [HarmonyPatch(typeof(UI_InGame_FloatingWindow_Shop_Item), "UpdateContent")]
    public static class ShopItemUpdateContentPatch
    {
        // Cache for storing custom item data per shop item instance
        private static Dictionary<UI_InGame_FloatingWindow_Shop_Item, MerchandiseData> _customItemData = new Dictionary<UI_InGame_FloatingWindow_Shop_Item, MerchandiseData>();
        
        // Cache the last item name per shop item instance to skip redundant updates (like the game does!)
        private static Dictionary<UI_InGame_FloatingWindow_Shop_Item, string> _cachedItemNames = new Dictionary<UI_InGame_FloatingWindow_Shop_Item, string>();
        
        // Cached reflection fields for performance
        private static FieldInfo _dataField = null;
        private static FieldInfo _indexField = null;
        private static FieldInfo _gemQualityField = null;
        private static FieldInfo _skillPlusField = null;
        private static FieldInfo _quantityField = null;
        private static FieldInfo _currentCachedTypeField = null;
        private static bool _reflectionCached = false;
        
        private static void EnsureReflectionCached()
        {
            if (_reflectionCached) return;
            _dataField = typeof(UI_InGame_FloatingWindow_Shop_Item).GetField("<data>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            _indexField = typeof(UI_InGame_FloatingWindow_Shop_Item).GetField("_index", BindingFlags.NonPublic | BindingFlags.Instance);
            _gemQualityField = typeof(UI_InGame_FloatingWindow_Shop_Item).GetField("gemQualityText", BindingFlags.Public | BindingFlags.Instance);
            _skillPlusField = typeof(UI_InGame_FloatingWindow_Shop_Item).GetField("skillPlusText", BindingFlags.Public | BindingFlags.Instance);
            _quantityField = typeof(UI_InGame_FloatingWindow_Shop_Item).GetField("quantityText", BindingFlags.Public | BindingFlags.Instance);
            _currentCachedTypeField = typeof(UI_InGame_FloatingWindow_Shop_Item).GetField("_currentCachedType", BindingFlags.NonPublic | BindingFlags.Instance);
            _reflectionCached = true;
        }
        
        /// <summary>
        /// Clear cache when leaving game to prevent memory leaks
        /// </summary>
        public static void ClearCache()
        {
            _customItemData.Clear();
            _cachedItemNames.Clear();
        }
        
        public static bool Prefix(UI_InGame_FloatingWindow_Shop_Item __instance, MerchandiseData d, int index)
        {
            // Check if this is a custom RPG item
            if (d.type == MerchandiseType.Treasure && d.itemName != null && d.itemName.StartsWith("RPGItem_"))
            {
                // Ensure reflection is cached early
                EnsureReflectionCached();
                
                // Store the data for ShowTooltip to use
                _customItemData[__instance] = d;
                
                // Set the data and _index fields via cached reflection so Click() works
                // These need to be set every frame for proper functionality
                try
                {
                    if (_dataField != null)
                    {
                        _dataField.SetValue(__instance, d);
                    }
                    
                    if (_indexField != null)
                    {
                        _indexField.SetValue(__instance, index);
                    }
                    
                    // Also set _currentCachedType so the game knows this slot is in use
                    if (_currentCachedTypeField != null)
                    {
                        _currentCachedTypeField.SetValue(__instance, d.itemName);
                    }
                }
                catch { }
                
                // Check if item has changed - skip expensive operations if same item
                // This is the KEY optimization that the game uses!
                string cachedName;
                bool itemChanged = !_cachedItemNames.TryGetValue(__instance, out cachedName) || cachedName != d.itemName;
                
                if (itemChanged)
                {
                    // Item changed - do full update
                    _cachedItemNames[__instance] = d.itemName;
                    
                    RPGItemsMod mod = GetModInstance();
                    if (mod == null) return true;
                    
                    InventoryManager invManager = mod.GetInventoryManager();
                    if (invManager == null) return true;
                    
                    // Parse item ID
                    string idStr = d.itemName.Substring(8);
                    int itemId;
                    if (!int.TryParse(idStr, out itemId)) return true;
                    
                    // Find the item using cached lookup
                    RPGItem item = MerchantPatches.FindItemById(invManager, itemId);
                    if (item == null) return true;
                    
                    // Set rarity color
                    Color rarityColor = item.GetRarityColor();
                    if (__instance.rarityImage != null)
                    {
                        __instance.rarityImage.color = rarityColor;
                    }
                    
                    // IMPORTANT: Ensure sprite is loaded before checking it!
                    // Sprites are lazy-loaded to avoid loading 200+ sprites at startup
                    ItemDatabase.EnsureSprite(item);
                    
                    // Show treasure icon and set sprite
                    if (__instance.treasureIcon != null)
                    {
                        __instance.treasureIcon.gameObject.SetActive(true);
                        if (item.sprite != null)
                        {
                            __instance.treasureIcon.sprite = item.sprite;
                            __instance.treasureIcon.color = Color.white;
                        }
                        else
                        {
                            // Sprite failed to load - use rarity color as fallback
                            __instance.treasureIcon.sprite = null;
                            __instance.treasureIcon.color = rarityColor;
                            RPGLog.Warning(" Shop sprite not loaded for: " + item.name + " (path: " + item.imagePath + ")");
                        }
                    }
                    
                    // Hide other icons
                    if (__instance.gemIcon != null) __instance.gemIcon.gameObject.SetActive(false);
                    if (__instance.skillIcon != null) __instance.skillIcon.gameObject.SetActive(false);
                    if (__instance.upgradeableObject != null) __instance.upgradeableObject.SetActive(false);
                    if (__instance.alreadyHasObject != null) __instance.alreadyHasObject.SetActive(false);
                    
                    // Show upgrade level in green (like inventory items) if level > 0
                    // Hide gem quality text (not used for custom items)
                    try
                    {
                        if (_gemQualityField != null)
                        {
                            object gemQualityText = _gemQualityField.GetValue(__instance);
                            if (gemQualityText != null)
                            {
                                Component comp = gemQualityText as Component;
                                if (comp != null && comp.gameObject != null)
                                {
                                    comp.gameObject.SetActive(false);
                                }
                            }
                        }
                        
                        // Show upgrade level using skillPlusText (like memories show their level)
                        if (_skillPlusField != null)
                        {
                            object skillPlusText = _skillPlusField.GetValue(__instance);
                            if (skillPlusText != null)
                            {
                                Component comp = skillPlusText as Component;
                                if (comp != null && comp.gameObject != null)
                                {
                                    // Show if upgrade level > 0, hide otherwise
                                    if (d.level > 0)
                                    {
                                        comp.gameObject.SetActive(true);
                                        // Set the text to show +X in green
                                        PropertyInfo textProp = skillPlusText.GetType().GetProperty("text");
                                        if (textProp != null)
                                        {
                                            textProp.SetValue(skillPlusText, "<color=#55efc4>+" + d.level + "</color>", null);
                                        }
                                    }
                                    else
                                {
                                    comp.gameObject.SetActive(false);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
                
                // These lightweight operations run every frame (like the game does)
                // Set quantity text
                try
                {
                    if (_quantityField != null)
                    {
                        object quantityText = _quantityField.GetValue(__instance);
                        if (quantityText != null)
                        {
                            // Use reflection to set text property (works for TMP_Text)
                            PropertyInfo textProp = quantityText.GetType().GetProperty("text");
                            if (textProp != null)
                            {
                                textProp.SetValue(quantityText, d.count.ToString(), null);
                            }
                        }
                    }
                }
                catch { }
                
                // Setup cost display (price can change with multipliers)
                if (__instance.costDisplay != null)
                {
                    __instance.costDisplay.gameObject.SetActive(true);
                    Cost price = d.price;
                    if (DewPlayer.local != null)
                    {
                        price = d.price.MultiplyGold(DewPlayer.local.buyPriceMultiplier);
                    }
                    __instance.costDisplay.Setup(price);
                }
                
                // Enable button based on count
                UnityEngine.UI.Button button = __instance.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    button.interactable = d.count > 0;
                }
                
                __instance.gameObject.SetActive(true);
                
                return false; // Skip original to avoid DewResources error
            }
            
            return true;
        }
        
        public static MerchandiseData GetStoredData(UI_InGame_FloatingWindow_Shop_Item instance)
        {
            MerchandiseData data;
            if (_customItemData.TryGetValue(instance, out data))
            {
                return data;
            }
            return default(MerchandiseData);
        }
    }
    
    /// <summary>
    /// Patch ShowTooltip to show custom RPG item tooltips
    /// </summary>
    [HarmonyPatch(typeof(UI_InGame_FloatingWindow_Shop_Item), "ShowTooltip")]
    public static class ShopItemShowTooltipPatch
    {
        public static bool Prefix(UI_InGame_FloatingWindow_Shop_Item __instance, UI_TooltipManager tooltip)
        {
            // Null checks to prevent errors
            if (__instance == null || tooltip == null) return true;
            
            // First try to get stored data from our cache
            MerchandiseData data = ShopItemUpdateContentPatch.GetStoredData(__instance);
            
            // If not in cache, try the instance data
            if (string.IsNullOrEmpty(data.itemName))
            {
                try
                {
                    data = __instance.data;
                }
                catch { }
            }
            
            // Still no valid data? Let original method handle it
            if (string.IsNullOrEmpty(data.itemName))
            {
                return true; // Let original method try to handle it
            }
            
            // Check if this is a custom RPG item
            if (data.type == MerchandiseType.Treasure && data.itemName.StartsWith("RPGItem_"))
            {
                RPGItemsMod mod = GetModInstance();
                if (mod != null)
                {
                    InventoryManager invManager = mod.GetInventoryManager();
                    if (invManager != null)
                    {
                        // Parse item ID
                        string idStr = data.itemName.Substring(8);
                        int itemId;
                        if (int.TryParse(idStr, out itemId))
                        {
                            // Find the item
                            RPGItem item = MerchantPatches.FindItemById(invManager, itemId);
                            if (item != null)
                            {
                                // Clone item and apply upgrade stats for correct display
                                RPGItem displayItem = item.Clone();
                                int targetLevel = data.level;
                                for (int upg = 0; upg < targetLevel; upg++)
                                {
                                    InventoryMerchantHelper.UpgradeItem(displayItem, 0); // Apply upgrade stats
                                }
                                
                                // Show custom tooltip
                                string tooltipText = displayItem.GetFullTooltip();
                                tooltip.ShowRawTextTooltip(__instance.transform.position, tooltipText);
                                return false; // Skip original method
                            }
                        }
                    }
                }
                
                // If we couldn't find the item, show the item name at least
                tooltip.ShowRawTextTooltip(__instance.transform.position, "Item: " + data.itemName);
                return false;
            }
            
            return true; // Continue with original method
        }
    }
}

/// <summary>
/// Helper class to add upgrade/sell/cleanse functionality to inventory UI
/// Formulas are designed to match the game's memory/gem system
/// </summary>
public static class InventoryMerchantHelper
{
    // ============================================================
    // ============================================================
    // PRICE FORMULA CONSTANTS (Matching game's memory/skill system EXACTLY)
    // ============================================================
    // Game formulas from SkillTrigger.GetGoldValue:
    //   value = skillValue[rarity] × valueGlobalMultiplier × (1 + (valueMultiplierPerSkillLevel - 1) × (level - 1))
    // Game formula for upgrade dust from GameManager.GetSkillUpgradeDreamDustCost:
    //   dust = skillUpgradeDreamDustByLevel.Evaluate(level)
    // Game formula for cleanse from GameManager.GetCleanseGoldCost:
    //   gold = skillCleanseCostByLevel.Evaluate(level)
    // ============================================================
    
    // Base gold values by rarity (matching game's skillValue PerRarityData)
    // From gameplay: Rare level 5 memory = 216 gold
    // Formula: base × (1 + 0.25 × (level-1)) = base × 2.0 for level 5
    // So base = 216 / 2.0 = 108, rounding to nice numbers
    private static readonly int[] BaseValueByRarity = { 55, 110, 220, 440 }; // Common, Rare, Epic, Legendary
    
    // Global multiplier for all values (game's valueGlobalMultiplier = 1.0)
    private const float ValueGlobalMultiplier = 1.0f;
    
    // Value multiplier per level (game's valueMultiplierPerSkillLevel)
    // Game uses ~1.25 meaning each level adds 25% of base
    // Formula: 1 + (1.25 - 1) × (level - 1) = 1 + 0.25 × (level - 1)
    // Level 1 = 1.0x, Level 2 = 1.25x, Level 3 = 1.5x, etc.
    private const float ValueMultiplierPerLevel = 1.25f;
    
    // Sell price multiplier (game's sellValueMultiplier = 0.4)
    private static float SellValueMultiplier 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.sellValuePercent / 100f : 0.4f;
        } 
    }
    
    // ============================================================
    // UPGRADE DUST COST (matching game's skillUpgradeDreamDustByLevel curve)
    // ============================================================
    // Game uses: skillUpgradeDreamDustByLevel.Evaluate(level) - NO rarity factor!
    // From actual gameplay testing:
    // - Level 5 memory cleanse gives 210 dust (70% refund)
    // - Total upgrade cost 1→5 = 210 / 0.7 = 300 dust
    // - Upgrades: 1→2, 2→3, 3→4, 4→5 (4 upgrades)
    // Formula: 45 + 12*level gives ~300 total for levels 1-4
    
    // Base dust cost (level-independent part)
    // From gameplay: Level 5 cleanse gives 210 dust (70% of 300)
    // 4 upgrades totaling 300 dust = avg 75 per upgrade
    // Formula: 50 + 10×level gives 60+70+80+90 = 300 ✓
    private const int UpgradeBaseDust = 50;
    
    // Dust cost increase per level
    private const int UpgradeDustPerLevel = 10;
    
    // ============================================================
    // DISMANTLE VALUES (matching game's skillDismantleDreamDustByLevel)
    // ============================================================
    // Game uses formula based on level, with rarity multiplier
    // Dismantle gives back less dust than upgrade cost
    
    private const int DismantleBaseDust = 20;
    private const int DismantleDustPerLevel = 8;
    // Rarity multipliers (game's skillDismantleDreamDustMultiplier)
    private static readonly float[] DismantleRarityMultiplier = { 1.0f, 1.2f, 1.5f, 2.0f }; // Common, Rare, Epic, Legendary
    
    // ============================================================
    // CLEANSE SYSTEM (matching game's cleanse logic)
    // ============================================================
    // Game's cleanse gold cost: skillCleanseCostByLevel.Evaluate(level) - based on level only!
    // Game's cleanse dust refund: sum of upgrade costs × cleanseRefundMultiplier (0.7)
    // From gameplay: Level 5 cleanse costs 165 gold
    // Formula: ~25 + 28*level gives 25+140=165 for level 5 ✓
    
    // Cleanse gold cost - level-based like game
    private const int CleanseBaseGold = 25;
    private const int CleanseGoldPerLevel = 28;
    
    // Cleanse refund multiplier (game's default cleanseRefundMultiplier = 0.7)
    private const float CleanseRefundMultiplier = 0.7f;
    
    // ============================================================
    // BASE VALUE CALCULATION
    // ============================================================
    
    /// <summary>
    /// Get the base gold value of an item (before sell/buy multipliers)
    /// Matches game's SkillTrigger.GetGoldValue formula:
    /// value = skillValue[rarity] × valueGlobalMultiplier × (1 + (valueMultiplierPerSkillLevel - 1) × (level - 1))
    /// For items: level = upgradeLevel + 1 (so +0 = level 1, +1 = level 2, etc.)
    /// </summary>
    public static int GetGoldValue(RPGItem item)
    {
        if (item == null) return 0;
        
        int rarityIndex = (int)item.rarity;
        if (rarityIndex < 0 || rarityIndex >= BaseValueByRarity.Length)
            rarityIndex = 0;
        
        float baseValue = BaseValueByRarity[rarityIndex];
        
        // Apply global multiplier
        baseValue *= ValueGlobalMultiplier;
        
        // Apply level multiplier using game's formula: 1 + (mult - 1) × (level - 1)
        // For items: level = upgradeLevel + 1 (so +0 item = level 1)
        int level = item.upgradeLevel + 1;
        float levelMult = 1f + (ValueMultiplierPerLevel - 1f) * (level - 1);
        baseValue *= levelMult;
        
        // Add stats value contribution (small bonus for items with good stats)
        int statsValue = CalculateStatsValue(item);
        baseValue += statsValue * 0.5f; // Stats contribute less to price
        
        return Mathf.Max(1, Mathf.RoundToInt(baseValue));
    }
    
    /// <summary>
    /// Calculate additional value from item stats
    /// </summary>
    private static int CalculateStatsValue(RPGItem item)
    {
        float value = 0f;
        
        // Base stats contribute to value
        value += item.attackBonus * 2f;
        value += item.defenseBonus * 2f;
        value += item.healthBonus * 0.2f;
        value += item.abilityPowerBonus * 2f;
        value += item.criticalChance * 5f;
        value += item.criticalDamage * 2f;
        
        // Gear effects contribute less
        value += item.moveSpeedBonus * 3f;
        value += item.dodgeChargesBonus * 20f;
        value += item.goldOnKill * 10f;
        value += item.lifeSteal * 5f;
        value += item.thorns * 3f;
        value += item.regeneration * 5f;
        value += item.attackSpeed * 3f;
        value += item.memoryHaste * 3f;
        
        return Mathf.RoundToInt(value);
    }
    
    // ============================================================
    // BUY PRICE (Shop)
    // ============================================================
    
    /// <summary>
    /// Calculate buy price for an RPG item (what player pays in shop)
    /// Formula: GetGoldValue(item) (100% of base value)
    /// </summary>
    public static int GetBuyPrice(RPGItem item)
    {
        if (item == null) return 0;
        return GetGoldValue(item);
    }
    
    // ============================================================
    // SELL PRICE
    // ============================================================
    
    /// <summary>
    /// Calculate sell price for an RPG item
    /// Formula: GetGoldValue(item) × sellMultiplier (40%) × stackCount (for consumables)
    /// </summary>
    public static int GetSellPrice(RPGItem item)
    {
        if (item == null) return 0;
        
        float sellPrice = GetGoldValue(item) * SellValueMultiplier;
        
        // For consumables, multiply by stack count (fixes bug where 100 potions = price of 1)
        if (item.type == ItemType.Consumable && item.currentStack > 1)
        {
            sellPrice *= item.currentStack;
        }
        
        return Mathf.Max(1, Mathf.RoundToInt(sellPrice));
    }
    
    // ============================================================
    // UPGRADE COST (Dream Dust)
    // ============================================================
    
    /// <summary>
    /// Calculate upgrade cost for an RPG item (Dream Dust)
    /// Matches game's GetSkillUpgradeDreamDustCost - based on LEVEL only, not rarity!
    /// For items: +N item is at "level N+1", upgrading costs dust based on current level
    /// Formula: baseDust + dustPerLevel × (upgradeLevel + 1)
    /// </summary>
    public static int GetUpgradeCost(RPGItem item)
    {
        if (item == null) return 0;
        
        // Game formula: skillUpgradeDreamDustByLevel.Evaluate(level)
        // +0 item = level 1, costs 50+10×1 = 60 to upgrade
        // +4 item = level 5, costs 50+10×5 = 100 to upgrade to +5
        int level = item.upgradeLevel + 1;
        int cost = UpgradeBaseDust + UpgradeDustPerLevel * level;
        
        return Mathf.Max(1, cost);
    }
    
    /// <summary>
    /// Calculate upgrade cost for a specific upgrade level (used for cleanse refund calculation)
    /// upgradeLevel = 0 means +0→+1, upgradeLevel = 3 means +3→+4
    /// </summary>
    public static int GetUpgradeCostAtLevel(ItemRarity rarity, int upgradeLevel)
    {
        // Game ignores rarity for upgrade dust cost - only level matters
        // upgradeLevel 0 = going from +0 to +1 = "level 1" in game terms
        int level = upgradeLevel + 1;
        int cost = UpgradeBaseDust + UpgradeDustPerLevel * level;
        return Mathf.Max(1, cost);
    }
    
    // ============================================================
    // DISMANTLE VALUE (Dream Dust)
    // ============================================================
    
    /// <summary>
    /// Calculate dismantle value for an RPG item (Dream Dust received)
    /// Matches game's pattern: baseDust × level formula × rarityMultiplier
    /// </summary>
    public static int GetDismantleValue(RPGItem item)
    {
        if (item == null) return 0;
        
        // Base formula: baseDust + dustPerLevel × level
        int level = item.upgradeLevel + 1; // +0 = level 1
        float baseValue = DismantleBaseDust + DismantleDustPerLevel * level;
        
        // Apply rarity multiplier (game's gemDismantleDreamDustMultiplier)
        int rarityIndex = (int)item.rarity;
        if (rarityIndex < 0 || rarityIndex >= DismantleRarityMultiplier.Length)
            rarityIndex = 0;
        baseValue *= DismantleRarityMultiplier[rarityIndex];
        
        return Mathf.Max(1, Mathf.RoundToInt(baseValue));
    }
    
    // ============================================================
    // CLEANSE SYSTEM
    // ============================================================
    
    /// <summary>
    /// Calculate cleanse gold cost for an RPG item
    /// Matches game's GetCleanseGoldCost pattern (skillCleanseCostByLevel curve)
    /// Formula: baseGold + goldPerLevel × level (level-based only, like game)
    /// </summary>
    public static int GetCleanseCost(RPGItem item)
    {
        if (item == null) return 0;
        if (item.upgradeLevel <= 0) return 0; // Can't cleanse +0 items
        
        // Game uses level-based formula only
        int level = item.upgradeLevel + 1; // +1 = level 2, etc.
        int cost = CleanseBaseGold + CleanseGoldPerLevel * level;
        
        return Mathf.Max(1, cost);
    }
    
    /// <summary>
    /// Calculate cleanse Dream Dust refund for an RPG item
    /// Formula: Sum of all upgrade costs from level 0 to current level × refundMultiplier (70%)
    /// This matches the game's memory/essence cleanse system:
    /// - Cleanse refunds based on theoretical upgrade cost, NOT what player actually spent
    /// - Buying a pre-upgraded item and cleansing it DOES give dust (same as game)
    /// </summary>
    public static int GetCleanseRefund(RPGItem item)
    {
        if (item == null) return 0;
        if (item.upgradeLevel <= 0) return 0; // No refund for +0 items
        
        // Sum all upgrade costs from level 0 to current level
        // (matches game's GetCleanseReturnedDreamDust which sums upgrade costs)
        float totalCost = 0f;
        for (int level = 0; level < item.upgradeLevel; level++)
        {
            totalCost += GetUpgradeCostAtLevel(item.rarity, level);
        }
        
        // Apply refund multiplier (70%) - matches game's cleanseRefundMultiplier default
        float refund = totalCost * CleanseRefundMultiplier;
        
        return Mathf.Max(1, Mathf.RoundToInt(refund));
    }
    
    /// <summary>
    /// Check if an item can be cleansed (has upgrades to remove)
    /// </summary>
    public static bool CanCleanse(RPGItem item)
    {
        if (item == null) return false;
        if (item.type == ItemType.Consumable) return false; // Can't cleanse consumables
        return item.upgradeLevel > 0;
    }
    
    /// <summary>
    /// Cleanse an item - reset upgrade level to 0 and return refund amount
    /// Returns the Dream Dust refund amount
    /// </summary>
    public static int CleanseItem(RPGItem item, RPGItem originalTemplate)
    {
        if (item == null || originalTemplate == null) return 0;
        if (item.upgradeLevel <= 0) return 0;
        
        int refund = GetCleanseRefund(item);
        
        // Reset stats to original template values
        item.attackBonus = originalTemplate.attackBonus;
        item.defenseBonus = originalTemplate.defenseBonus;
        item.healthBonus = originalTemplate.healthBonus;
        item.abilityPowerBonus = originalTemplate.abilityPowerBonus;
        item.criticalChance = originalTemplate.criticalChance;
        item.criticalDamage = originalTemplate.criticalDamage;
        
        // Reset upgrade level and dust spent
        int previousLevel = item.upgradeLevel;
        item.upgradeLevel = 0;
        item.dustSpentUpgrading = 0;
        
        RPGLog.Debug(" Item cleansed: " + item.name + " from +" + previousLevel + " to +0, refunded " + refund + " Dream Dust");
        
        return refund;
    }
    
    /// <summary>
    /// Cleanse an item without template - calculates original stats based on upgrade level
    /// Uses the multiplicative scaling formula to reverse stats to base values
    /// Returns the Dream Dust refund amount
    /// </summary>
    public static int CleanseItem(RPGItem item)
    {
        if (item == null) return 0;
        if (item.upgradeLevel <= 0) return 0;
        
        int refund = GetCleanseRefund(item);
        int previousLevel = item.upgradeLevel;
        
        // Calculate the current multiplier to reverse stats to base
        float currentMultiplier = GetUpgradeMultiplier(previousLevel);
        
        // Reverse attack bonus using multiplicative formula
        // Current = Base × Multiplier, so Base = Current / Multiplier
        if (item.attackBonus > 0)
        {
            int baseValue = Mathf.RoundToInt(item.attackBonus / currentMultiplier);
            item.attackBonus = Mathf.Max(1, baseValue); // Ensure at least 1 if it had a value
        }
        
        // Reverse defense bonus
        if (item.defenseBonus > 0)
        {
            int baseValue = Mathf.RoundToInt(item.defenseBonus / currentMultiplier);
            item.defenseBonus = Mathf.Max(1, baseValue);
        }
        
        // Reverse health bonus
        if (item.healthBonus > 0)
        {
            int baseValue = Mathf.RoundToInt(item.healthBonus / currentMultiplier);
            item.healthBonus = Mathf.Max(1, baseValue);
        }
        
        // Reverse ability power bonus
        if (item.abilityPowerBonus > 0)
        {
            int baseValue = Mathf.RoundToInt(item.abilityPowerBonus / currentMultiplier);
            item.abilityPowerBonus = Mathf.Max(1, baseValue);
        }
        
        // Reverse crit chance (flat 0.5% per upgrade)
        if (item.criticalChance > 0)
        {
            item.criticalChance -= CritChancePerUpgrade * previousLevel;
            item.criticalChance = Mathf.Max(0f, item.criticalChance);
        }
        
        // Reverse crit damage (flat 2% per upgrade)
        if (item.criticalDamage > 0)
        {
            item.criticalDamage -= CritDamagePerUpgrade * previousLevel;
            item.criticalDamage = Mathf.Max(0f, item.criticalDamage);
        }
        
        // Reset upgrade level and dust spent
        item.upgradeLevel = 0;
        item.dustSpentUpgrading = 0;
        
        RPGLog.Debug(" Item cleansed: " + item.name + " from +" + previousLevel + " to +0, refunded " + refund + " Dream Dust");
        
        return refund;
    }
    
    // ============================================================
    // UPGRADE SYSTEM (Matching Game's Memory System)
    // ============================================================
    
    // Upgrade scaling constants (configurable via ModConfig)
    // Game uses: Gems = 1% per quality, Skills = 25% per level
    // We use a balanced approach for equipment items
    private static float UpgradeScalingPerLevel 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.upgradeScalingPerLevel / 100f : 0.08f;
        } 
    }
    private const float CritChancePerUpgrade = 1.0f;     // +1% crit chance per upgrade (Diablo IV style)
    private const float CritDamagePerUpgrade = 3f;       // +3% crit damage per upgrade (Diablo IV style)
    
    // Minimum stat increases to ensure visible progress (Diablo IV style - more impactful)
    private const int MinAttackIncrease = 2;
    private const int MinDefenseIncrease = 2;
    private const int MinHealthIncrease = 10;
    private const int MinAbilityPowerIncrease = 2;
    
    /// <summary>
    /// Calculate the stat multiplier for a given upgrade level
    /// Formula: 1 + (scalingPerLevel × level)
    /// Similar to game's GetScalingMultiplier: 1 + addedScalingMultiplierPerLevel × (level - 1)
    /// </summary>
    public static float GetUpgradeMultiplier(int upgradeLevel)
    {
        return 1f + (UpgradeScalingPerLevel * upgradeLevel);
    }
    
    /// <summary>
    /// Calculate what a base stat should be at a given upgrade level
    /// Formula: baseStat × GetUpgradeMultiplier(level)
    /// </summary>
    public static int CalculateUpgradedStat(int baseStat, int upgradeLevel)
    {
        if (baseStat <= 0) return baseStat;
        return Mathf.RoundToInt(baseStat * GetUpgradeMultiplier(upgradeLevel));
    }
    
    /// <summary>
    /// Calculate what a base stat should be at a given upgrade level (float version)
    /// </summary>
    public static float CalculateUpgradedStat(float baseStat, int upgradeLevel)
    {
        if (baseStat <= 0f) return baseStat;
        return baseStat * GetUpgradeMultiplier(upgradeLevel);
    }
    
    /// <summary>
    /// Upgrade an item (increase level and stats)
    /// Uses multiplicative scaling similar to game's memory system:
    /// - Stats scale by +8% per upgrade level (compounding)
    /// - Crit chance: +0.5% per upgrade
    /// - Crit damage: +2% per upgrade
    /// Only increases EXISTING base stats - does not add new stats
    /// Gear effects (move speed, dodge charges, etc.) are NOT upgradeable
    /// dustCost: Optional dust cost to track (for cleanse refund). Pass 0 for free upgrades (merge, shop items).
    /// </summary>
    public static void UpgradeItem(RPGItem item, int dustCost = 0)
    {
        if (item == null) return;
        
        // Track for scoring
        ScoringPatches.TrackItemUpgraded();
        
        // Track dust spent for cleanse refund (only if player actually spent dust)
        if (dustCost > 0)
        {
            item.dustSpentUpgrading += dustCost;
        }
        
        int previousLevel = item.upgradeLevel;
        item.upgradeLevel++;
        
        // Calculate stat increases using multiplicative scaling
        // New value = base × (1 + scaling × newLevel)
        // Increase = New value - Old value
        
        // Attack: only if item already has attack
        if (item.attackBonus > 0)
        {
            // Calculate what the stat should be at the new level vs old level
            float oldMultiplier = GetUpgradeMultiplier(previousLevel);
            float newMultiplier = GetUpgradeMultiplier(item.upgradeLevel);
            
            // Get the base value (what it was at +0)
            int baseValue = Mathf.RoundToInt(item.attackBonus / oldMultiplier);
            int newValue = Mathf.CeilToInt(baseValue * newMultiplier); // Use CeilToInt to ensure increase
            
            // Ensure minimum increase - ALWAYS at least MinAttackIncrease
            int increase = newValue - item.attackBonus;
            if (increase < MinAttackIncrease) increase = MinAttackIncrease;
            item.attackBonus += increase;
        }
        
        // Defense: only if item already has defense
        if (item.defenseBonus > 0)
        {
            float oldMultiplier = GetUpgradeMultiplier(previousLevel);
            float newMultiplier = GetUpgradeMultiplier(item.upgradeLevel);
            int baseValue = Mathf.RoundToInt(item.defenseBonus / oldMultiplier);
            int newValue = Mathf.CeilToInt(baseValue * newMultiplier); // Use CeilToInt to ensure increase
            int increase = newValue - item.defenseBonus;
            // ALWAYS increase by at least MinDefenseIncrease
            if (increase < MinDefenseIncrease) increase = MinDefenseIncrease;
            item.defenseBonus += increase;
        }
        
        // Health: only if item already has health
        if (item.healthBonus > 0)
        {
            float oldMultiplier = GetUpgradeMultiplier(previousLevel);
            float newMultiplier = GetUpgradeMultiplier(item.upgradeLevel);
            int baseValue = Mathf.RoundToInt(item.healthBonus / oldMultiplier);
            int newValue = Mathf.CeilToInt(baseValue * newMultiplier); // Use CeilToInt to ensure increase
            int increase = newValue - item.healthBonus;
            // ALWAYS increase by at least MinHealthIncrease
            if (increase < MinHealthIncrease) increase = MinHealthIncrease;
            item.healthBonus += increase;
        }
        
        // Ability Power: only if item already has ability power
        if (item.abilityPowerBonus > 0)
        {
            float oldMultiplier = GetUpgradeMultiplier(previousLevel);
            float newMultiplier = GetUpgradeMultiplier(item.upgradeLevel);
            int baseValue = Mathf.RoundToInt(item.abilityPowerBonus / oldMultiplier);
            int newValue = Mathf.CeilToInt(baseValue * newMultiplier); // Use CeilToInt to ensure increase
            int increase = newValue - item.abilityPowerBonus;
            // ALWAYS increase by at least MinAbilityPowerIncrease
            if (increase < MinAbilityPowerIncrease) increase = MinAbilityPowerIncrease;
            item.abilityPowerBonus += increase;
        }
        
        // Critical Chance: flat increase per upgrade (+0.5% per upgrade)
        if (item.criticalChance > 0)
        {
            item.criticalChance += CritChancePerUpgrade;
        }
        
        // Critical Damage: flat increase per upgrade (+2% per upgrade)
        if (item.criticalDamage > 0)
        {
            item.criticalDamage += CritDamagePerUpgrade;
        }
        
        // GEAR EFFECTS ARE NOT UPGRADED - they remain at their fixed base values:
        // - Move speed bonus, dodge charges, gold on hit, thorns, life steal
        // - Dust on hit, regeneration, attack speed, memory haste
        // - Auto attack, auto aim, heal percentage, elemental type
        
        RPGLog.Debug(" Item upgraded: " + item.name + " to +" + item.upgradeLevel + 
            " (ATK: " + item.attackBonus + ", DEF: " + item.defenseBonus + ", HP: " + item.healthBonus + 
            ", Multiplier: " + GetUpgradeMultiplier(item.upgradeLevel).ToString("F2") + "x)");
    }
    
    /// <summary>
    /// Get the stat preview for an upgrade (shows what stats will be after upgrade)
    /// </summary>
    public static string GetUpgradePreview(RPGItem item)
    {
        if (item == null) return "";
        
        int nextLevel = item.upgradeLevel + 1;
        float currentMult = GetUpgradeMultiplier(item.upgradeLevel);
        float nextMult = GetUpgradeMultiplier(nextLevel);
        
        string preview = "";
        
        if (item.attackBonus > 0)
        {
            int baseAtk = Mathf.RoundToInt(item.attackBonus / currentMult);
            int nextAtk = Mathf.RoundToInt(baseAtk * nextMult);
            preview += string.Format("{0}: {1} → {2}\n", Localization.ATK, item.attackBonus, nextAtk);
        }
        
        if (item.defenseBonus > 0)
        {
            int baseDef = Mathf.RoundToInt(item.defenseBonus / currentMult);
            int nextDef = Mathf.RoundToInt(baseDef * nextMult);
            preview += string.Format("{0}: {1} → {2}\n", Localization.DEF, item.defenseBonus, nextDef);
        }
        
        if (item.healthBonus > 0)
        {
            int baseHp = Mathf.RoundToInt(item.healthBonus / currentMult);
            int nextHp = Mathf.RoundToInt(baseHp * nextMult);
            preview += string.Format("{0}: {1} → {2}\n", Localization.HP, item.healthBonus, nextHp);
        }
        
        if (item.abilityPowerBonus > 0)
        {
            int baseAp = Mathf.RoundToInt(item.abilityPowerBonus / currentMult);
            int nextAp = Mathf.RoundToInt(baseAp * nextMult);
            preview += string.Format("{0}: {1} → {2}\n", Localization.AP, item.abilityPowerBonus, nextAp);
        }
        
        if (item.criticalChance > 0)
        {
            float nextCrit = item.criticalChance + CritChancePerUpgrade;
            preview += string.Format("{0}: {1}% → {2}%\n", Localization.Crit, item.criticalChance, nextCrit);
        }
        
        if (item.criticalDamage > 0)
        {
            float nextCritDmg = item.criticalDamage + CritDamagePerUpgrade;
            preview += string.Format("{0}: {1}% → {2}%\n", Localization.CritDamage, item.criticalDamage, nextCritDmg);
        }
        
        return preview.TrimEnd('\n');
    }
    
    // ============================================================
    // PRICE SUMMARY STRING
    // ============================================================
    
    /// <summary>
    /// Get a formatted string showing all prices for an item
    /// </summary>
    public static string GetPriceSummary(RPGItem item)
    {
        if (item == null) return "";
        
        string summary = "";
        
        // Buy price (only shown in shop context)
        // summary += Localization.FormatBuyPrice(GetBuyPrice(item)) + "\n";
        
        // Sell price
        summary += Localization.FormatSellPrice(GetSellPrice(item)) + "\n";
        
        // Upgrade cost
        summary += Localization.FormatUpgradeCost(GetUpgradeCost(item)) + "\n";
        
        // Dismantle value
        summary += Localization.FormatDismantleValue(GetDismantleValue(item));
        
        // Cleanse info (only if upgradeable)
        if (CanCleanse(item))
        {
            summary += "\n" + Localization.FormatCleanseCost(GetCleanseCost(item));
            summary += " (" + Localization.FormatCleanseRefund(GetCleanseRefund(item)) + ")";
        }
        
        return summary;
    }
}

