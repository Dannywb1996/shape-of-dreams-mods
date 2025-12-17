using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Language options for the mod (kept for backwards compatibility)
/// </summary>
public enum ModLanguage
{
    English,
    Chinese
}

/// <summary>
/// UI layout style options
/// </summary>
public enum UIStyle
{
    Classic,      // Equipment left, Inventory right
    DiabloStyle   // Stats left, Equipment right, Inventory bottom
}

/// <summary>
/// JSON-based localization system for the RPG Items Mod
/// Auto-detects game language and loads appropriate translation file
/// Supports 13 languages matching the game's supported languages
/// </summary>
public static class Localization
{
    private static Dictionary<string, string> _localizationData = new Dictionary<string, string>();
    private static bool _isInitialized = false;
    private static string _currentLanguageCode = "en-US";
    private static string _modPath = "";

    private static readonly string[] SupportedLanguageCodes = new string[]
    {
        "en-US", "zh-CN", "zh-TW", "ja-JP", "ko-KR", "es-MX", "fr-FR", "de-DE", "it-IT", "pt-BR", "ru-RU", "tr-TR", "pl-PL"
    };

    /// <summary>
    /// Set the mod path (must be called before DetectLanguage)
    /// </summary>
    public static void SetModPath(string path)
    {
        _modPath = path;
    }
    
    /// <summary>
    /// Get the current language code
    /// </summary>
    public static string CurrentLanguageCode { get { return _currentLanguageCode; } }

    /// <summary>
    /// Check if current language is Chinese (Simplified or Traditional)
    /// </summary>
    public static bool IsChinese()
    {
        return _currentLanguageCode == "zh-CN" || _currentLanguageCode == "zh-TW";
    }

    /// <summary>
    /// Get the legacy ModLanguage enum value (for backwards compatibility)
    /// </summary>
    public static ModLanguage CurrentLanguage
    {
        get { return IsChinese() ? ModLanguage.Chinese : ModLanguage.English; }
        set { /* Legacy setter - does nothing, use DetectLanguage() instead */ }
    }

    /// <summary>
    /// Detect the game's language and load the appropriate localization file
    /// Call this in the mod's Awake() method
    /// </summary>
    public static void DetectLanguage()
    {
        try
        {
            string gameLang = "en-US";
            if (DewSave.profileMain != null && !string.IsNullOrEmpty(DewSave.profileMain.language))
            {
                gameLang = DewSave.profileMain.language;
            }

            string detectedLang = SupportedLanguageCodes.FirstOrDefault(
                code => code.Equals(gameLang, StringComparison.OrdinalIgnoreCase) ||
                        gameLang.StartsWith(code.Split('-')[0], StringComparison.OrdinalIgnoreCase)
            );

            _currentLanguageCode = detectedLang ?? "en-US";
            LoadLocalizationData();
            Debug.Log("[RPGItemsMod] Language detected: " + _currentLanguageCode);
        }
        catch (Exception ex)
        {
            Debug.LogError("[RPGItemsMod] Error detecting language: " + ex.Message);
            _currentLanguageCode = "en-US";
            LoadLocalizationData();
        }
    }

    /// <summary>
    /// Force a specific language (for testing or user override)
    /// </summary>
    public static void SetLanguage(string languageCode)
    {
        if (SupportedLanguageCodes.Contains(languageCode))
        {
            _currentLanguageCode = languageCode;
            LoadLocalizationData();
        }
    }

    private static void LoadLocalizationData()
    {
        _isInitialized = false;
        _localizationData.Clear();

        try
        {
            // Use the mod path set by SetModPath (this is the mod's root folder, e.g. Mods/RPGItemsMod/)
            if (string.IsNullOrEmpty(_modPath))
            {
                Debug.LogWarning("[RPGItemsMod] Mod path not set, using fallback strings. Call Localization.SetModPath() first.");
                return;
            }

            // The i18n folder is in the mod's root folder (same level as images/, about/, etc.)
            string jsonPath = Path.Combine(_modPath, "i18n", _currentLanguageCode + ".json");

            if (!File.Exists(jsonPath))
            {
                Debug.LogWarning("[RPGItemsMod] Localization file not found: " + jsonPath + ", falling back to en-US");
                jsonPath = Path.Combine(_modPath, "i18n", "en-US.json");
                if (!File.Exists(jsonPath))
                {
                    Debug.LogError("[RPGItemsMod] English localization file not found at: " + jsonPath);
                    return;
                }
            }

            string jsonContent = File.ReadAllText(jsonPath);
            _localizationData = ParseSimpleJson(jsonContent);
            _isInitialized = true;
            Debug.Log("[RPGItemsMod] Loaded " + _localizationData.Count + " localization strings for " + _currentLanguageCode);
        }
        catch (Exception ex)
        {
            Debug.LogError("[RPGItemsMod] Error loading localization: " + ex.Message);
        }
    }

    /// <summary>
    /// Simple JSON parser for flat key-value objects
    /// Does not require external JSON libraries
    /// </summary>
    private static Dictionary<string, string> ParseSimpleJson(string json)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        try
        {
            json = json.Trim();
            if (json.StartsWith("{") && json.EndsWith("}"))
                json = json.Substring(1, json.Length - 2).Trim();

            // Handle escaped newlines
            json = json.Replace("\\n", "\n");

            int i = 0;
            while (i < json.Length)
            {
                // Skip whitespace
                while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
                if (i >= json.Length) break;

                // Find key start
                if (json[i] != '"') { i++; continue; }
                i++;
                int keyStart = i;
                while (i < json.Length && json[i] != '"') i++;
                string key = json.Substring(keyStart, i - keyStart);
                i++;

                // Skip to colon
                while (i < json.Length && json[i] != ':') i++;
                i++;
                while (i < json.Length && char.IsWhiteSpace(json[i])) i++;

                // Find value
                if (i >= json.Length || json[i] != '"') { i++; continue; }
                i++;
                int valueStart = i;
                while (i < json.Length && !(json[i] == '"' && json[i - 1] != '\\')) i++;
                string value = json.Substring(valueStart, i - valueStart).Replace("\\\"", "\"");
                i++;

                result[key] = value;

                // Skip to comma or end
                while (i < json.Length && json[i] != ',' && json[i] != '}') i++;
                i++;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[RPGItemsMod] Error parsing JSON: " + ex.Message);
        }
        return result;
    }

    /// <summary>
    /// Get a localized string by key
    /// Supports format arguments: Get("Key", arg1, arg2)
    /// Returns "[key]" if not found
    /// </summary>
    public static string Get(string key, params object[] args)
    {
        if (!_isInitialized) DetectLanguage();

        string value;
        if (_localizationData.TryGetValue(key, out value))
        {
            if (args != null && args.Length > 0)
            {
                try { return string.Format(value, args); }
                catch { return value; }
            }
            return value;
        }
        return "[" + key + "]";
    }
    
    // ============================================================
    // BACKWARDS COMPATIBILITY PROPERTIES
    // These map to Get() calls for existing code
    // ============================================================
    
    // Window Titles
    public static string Character { get { return Get("Character"); } }
    public static string Backpack { get { return Get("Backpack"); } }
    public static string Hotbar { get { return Get("Hotbar"); } }
    public static string Stats { get { return Get("Stats"); } }
    
    // UI Text
    public static string Inventory { get { return Get("Inventory"); } }
    public static string Equipment { get { return Get("Equipment"); } }
    public static string Consumables { get { return Get("Consumables"); } }
    public static string Drop { get { return Get("Drop"); } }
    public static string Cancel { get { return Get("Cancel"); } }
    public static string DropAmount { get { return Get("DropAmount"); } }
    public static string Close { get { return Get("Close"); } }
    public static string Settings { get { return Get("Settings"); } }
    public static string Confirm { get { return Get("Confirm"); } }
    public static string Use { get { return Get("Use"); } }
    public static string Sell { get { return Get("Sell"); } }
    public static string Upgrade { get { return Get("Upgrade"); } }
    public static string Sort { get { return Get("Sort"); } }
    public static string Cleanse { get { return Get("Cleanse"); } }
    public static string AutoPickup { get { return Get("AutoPickup"); } }
    public static string AutoMerge { get { return Get("AutoMerge"); } }
    public static string AutoMergeHint { get { return Get("AutoMergeHint"); } }
    public static string DreamDust { get { return Get("DreamDust"); } }
    public static string BuyPrice { get { return Get("BuyPrice"); } }
    public static string SellPrice { get { return Get("SellPrice"); } }
    public static string UpgradeCost { get { return Get("UpgradeCost"); } }
    public static string CleanseCost { get { return Get("CleanseCost"); } }
    public static string CleanseRefund { get { return Get("CleanseRefund"); } }
    public static string DismantleValue { get { return Get("DismantleValue"); } }
    public static string Loading { get { return Get("Loading"); } }
    public static string NoHeroActive { get { return Get("NoHeroActive"); } }
    public static string Unknown { get { return Get("Unknown"); } }
    public static string SetBonuses { get { return Get("SetBonuses"); } }
    public static string NoSetBonuses { get { return Get("NoSetBonuses"); } }
    
    // Inventory UI Hints
    public static string PressToClose { get { return Get("PressToClose"); } }
    public static string DragToAssign { get { return Get("DragToAssign"); } }
    public static string DragSameItemToCombine { get { return Get("DragSameItemToCombine"); } }
    public static string DropToCombine { get { return Get("DropToCombine"); } }
    public static string MergeItems { get { return Get("MergeItems"); } }
    public static string MergeConfirmMessage { get { return Get("MergeConfirmMessage"); } }
    public static string MergeWarning { get { return Get("MergeWarning"); } }
    
    // Sell confirmation
    public static string SellItem { get { return Get("SellItem"); } }
    public static string SellConfirmMessage { get { return Get("SellConfirmMessage"); } }
    public static string YouWillReceive { get { return Get("YouWillReceive"); } }
    
    // Dismantle confirmation
    public static string DismantleItem { get { return Get("DismantleItem"); } }
    public static string DismantleConfirmMessage { get { return Get("DismantleConfirmMessage"); } }
    public static string DismantleWarning { get { return Get("DismantleWarning"); } }
    
    // Cleanse confirmation
    public static string CleanseItem { get { return Get("CleanseItem"); } }
    public static string CleanseConfirmMessage { get { return Get("CleanseConfirmMessage"); } }
    public static string ResetTo { get { return Get("ResetTo"); } }
    public static string Cost { get { return Get("Cost"); } }
    public static string Refund { get { return Get("Refund"); } }
    public static string NoUpgradesToCleanse { get { return Get("NoUpgradesToCleanse"); } }
    public static string PressKeyToSell { get { return Get("PressKeyToSell"); } }
    public static string PressKeyToUpgrade { get { return Get("PressKeyToUpgrade"); } }
    
    // Settings Panel
    public static string ConsumableHotkeys { get { return Get("ConsumableHotkeys"); } }
    public static string ClickToRebind { get { return Get("ClickToRebind"); } }
    public static string PressKey { get { return Get("PressKey"); } }
    public static string Slot { get { return Get("Slot"); } }
    
    // Equipment Slot Labels (Short)
    public static string HeadSlotLabel { get { return Get("HeadSlotLabel"); } }
    public static string AmuletSlotLabel { get { return Get("AmuletSlotLabel"); } }
    public static string LeftHandLabel { get { return Get("LeftHandLabel"); } }
    public static string RightHandLabel { get { return Get("RightHandLabel"); } }
    public static string ChestSlotLabel { get { return Get("ChestSlotLabel"); } }
    public static string LeftRingLabel { get { return Get("LeftRingLabel"); } }
    public static string RightRingLabel { get { return Get("RightRingLabel"); } }
    public static string BeltSlotLabel { get { return Get("BeltSlotLabel"); } }
    public static string LegsSlotLabel { get { return Get("LegsSlotLabel"); } }
    public static string BootsSlotLabel { get { return Get("BootsSlotLabel"); } }
    
    // Item Types
    public static string Weapon { get { return Get("Weapon"); } }
    public static string TwoHandedWeapon { get { return Get("TwoHandedWeapon"); } }
    public static string OffHand { get { return Get("OffHand"); } }
    public static string Helmet { get { return Get("Helmet"); } }
    public static string ChestArmor { get { return Get("ChestArmor"); } }
    public static string Pants { get { return Get("Pants"); } }
    public static string Boots { get { return Get("Boots"); } }
    public static string Belt { get { return Get("Belt"); } }
    public static string Amulet { get { return Get("Amulet"); } }
    public static string Ring { get { return Get("Ring"); } }
    public static string Consumable { get { return Get("Consumable"); } }
    public static string Material { get { return Get("Material"); } }
    public static string QuestItem { get { return Get("QuestItem"); } }
    public static string Item { get { return Get("Item"); } }
    public static string Misc { get { return Get("Misc"); } }
    
    // Rarities
    public static string Common { get { return Get("Common"); } }
    public static string Rare { get { return Get("Rare"); } }
    public static string Epic { get { return Get("Epic"); } }
    public static string Legendary { get { return Get("Legendary"); } }
    
    // Stats - Full names
    public static string Attack { get { return Get("Attack"); } }
    public static string Defense { get { return Get("Defense"); } }
    public static string Health { get { return Get("Health"); } }
    public static string AbilityPower { get { return Get("AbilityPower"); } }
    public static string CritChance { get { return Get("CritChance"); } }
    public static string CritDamage { get { return Get("CritDamage"); } }
    public static string MoveSpeed { get { return Get("MoveSpeed"); } }
    public static string DodgeCharges { get { return Get("DodgeCharges"); } }
    public static string DodgeCooldown { get { return Get("DodgeCooldown"); } }
    public static string GoldOnKill { get { return Get("GoldOnKill"); } }
    public static string DustOnKill { get { return Get("DustOnKill"); } }
    public static string LifeSteal { get { return Get("LifeSteal"); } }
    public static string Thorns { get { return Get("Thorns"); } }
    public static string Lifesteal { get { return Get("Lifesteal"); } }
    public static string Regeneration { get { return Get("Regeneration"); } }
    public static string ShieldRegeneration { get { return Get("ShieldRegeneration"); } }
    public static string AttackSpeed { get { return Get("AttackSpeed"); } }
    public static string MemoryHaste { get { return Get("MemoryHaste"); } }
    public static string AutoAttack { get { return Get("AutoAttack"); } }
    public static string AutoAim { get { return Get("AutoAim"); } }
    public static string Heals { get { return Get("Heals"); } }
    public static string Shield { get { return Get("Shield"); } }
    public static string Stack { get { return Get("Stack"); } }
    public static string Requires { get { return Get("Requires"); } }
    public static string Gold { get { return Get("Gold"); } }
    public static string Dust { get { return Get("Dust"); } }
    
    // Stats - Short names
    public static string ATK { get { return Get("ATK"); } }
    public static string DEF { get { return Get("DEF"); } }
    public static string HP { get { return Get("HP"); } }
    public static string AP { get { return Get("AP"); } }
    public static string Speed { get { return Get("Speed"); } }
    public static string Dodge { get { return Get("Dodge"); } }
    public static string Crit { get { return Get("Crit"); } }
    
    // Elemental Types
    public static string Fire { get { return Get("Fire"); } }
    public static string Cold { get { return Get("Cold"); } }
    public static string Light { get { return Get("Light"); } }
    public static string Dark { get { return Get("Dark"); } }
    
    // Actions (Dropped Items)
    public static string LeftClickPickup { get { return Get("LeftClickPickup"); } }
    public static string MiddleClickShare { get { return Get("MiddleClickShare"); } }
    public static string RightClickDismantle { get { return Get("RightClickDismantle"); } }
    public static string Shared { get { return Get("Shared"); } }
    public static string Pickup { get { return Get("Pickup"); } }
    public static string Equip { get { return Get("Equip"); } }
    public static string Share { get { return Get("Share"); } }
    public static string Dismantle { get { return Get("Dismantle"); } }
    public static string LMB { get { return Get("LMB"); } }
    public static string MMB { get { return Get("MMB"); } }
    public static string RMB { get { return Get("RMB"); } }
    
    // Equipment Slots (Full names)
    public static string Head { get { return Get("Head"); } }
    public static string Chest { get { return Get("Chest"); } }
    public static string Legs { get { return Get("Legs"); } }
    public static string LeftHand { get { return Get("LeftHand"); } }
    public static string RightHand { get { return Get("RightHand"); } }
    public static string LeftRing { get { return Get("LeftRing"); } }
    public static string RightRing { get { return Get("RightRing"); } }
    public static string BeltSlot { get { return Get("BeltSlot"); } }
    public static string AmuletSlot { get { return Get("AmuletSlot"); } }
    public static string BootsSlot { get { return Get("BootsSlot"); } }
    
    // Stats System - Window
    public static string StatsWindowTitle { get { return Get("StatsWindowTitle"); } }
    public static string StatsAvailablePoints { get { return Get("StatsAvailablePoints"); } }
    public static string StatsReset { get { return Get("StatsReset"); } }
    public static string StatsResetConfirm { get { return Get("StatsResetConfirm"); } }
    public static string StatsPressToClose { get { return Get("StatsPressToClose"); } }
    public static string StatsLevelUp { get { return Get("StatsLevelUp"); } }
    
    // Stats System - Stat Names
    public static string StatStrength { get { return Get("StatStrength"); } }
    public static string StatVitality { get { return Get("StatVitality"); } }
    public static string StatDefenseName { get { return Get("StatDefenseName"); } }
    public static string StatIntelligence { get { return Get("StatIntelligence"); } }
    public static string StatAgility { get { return Get("StatAgility"); } }
    public static string StatLuck { get { return Get("StatLuck"); } }
    
    // Stats System - Stat Descriptions
    public static string StatStrengthDesc { get { return Get("StatStrengthDesc"); } }
    public static string StatVitalityDesc { get { return Get("StatVitalityDesc"); } }
    public static string StatDefenseDesc { get { return Get("StatDefenseDesc"); } }
    public static string StatIntelligenceDesc { get { return Get("StatIntelligenceDesc"); } }
    public static string StatAgilityDesc { get { return Get("StatAgilityDesc"); } }
    public static string StatLuckDesc { get { return Get("StatLuckDesc"); } }
    
    // Stats System - Bonus Display
    public static string StatAttack { get { return Get("StatAttack"); } }
    public static string StatHealth { get { return Get("StatHealth"); } }
    public static string StatDefense { get { return Get("StatDefense"); } }
    public static string StatAbilityPower { get { return Get("StatAbilityPower"); } }
    public static string StatMoveSpeed { get { return Get("StatMoveSpeed"); } }
    public static string StatAttackSpeed { get { return Get("StatAttackSpeed"); } }
    public static string StatCritChance { get { return Get("StatCritChance"); } }
    
    // Weapon Mastery System
    public static string WeaponMastery { get { return Get("WeaponMastery"); } }
    public static string MasteryLevel { get { return Get("MasteryLevel"); } }
    public static string MasteryXP { get { return Get("MasteryXP"); } }
    public static string MasteryMax { get { return Get("MasteryMax"); } }
    public static string MasteryLevelUp { get { return Get("MasteryLevelUp"); } }
    public static string MasteryBonus { get { return Get("MasteryBonus"); } }
    
    // Weapon type names
    public static string WeaponSword { get { return Get("WeaponSword"); } }
    public static string WeaponTwoHandedSword { get { return Get("WeaponTwoHandedSword"); } }
    public static string WeaponWand { get { return Get("WeaponWand"); } }
    public static string WeaponStaff { get { return Get("WeaponStaff"); } }
    public static string WeaponBow { get { return Get("WeaponBow"); } }
    public static string WeaponCrossbow { get { return Get("WeaponCrossbow"); } }
    public static string WeaponDagger { get { return Get("WeaponDagger"); } }
    public static string WeaponDualDaggers { get { return Get("WeaponDualDaggers"); } }
    public static string WeaponHammer { get { return Get("WeaponHammer"); } }
    public static string WeaponShield { get { return Get("WeaponShield"); } }
    public static string WeaponRifle { get { return Get("WeaponRifle"); } }
    public static string WeaponPistol { get { return Get("WeaponPistol"); } }
    
    // Auto-Merge Lore Messages
    public static string MergeLore1 { get { return Get("MergeLore1"); } }
    public static string MergeLore2 { get { return Get("MergeLore2"); } }
    public static string MergeLore3 { get { return Get("MergeLore3"); } }
    public static string MergeLore4 { get { return Get("MergeLore4"); } }
    public static string MergeLore5 { get { return Get("MergeLore5"); } }
    public static string MergeLore6 { get { return Get("MergeLore6"); } }
    public static string MergeLore7 { get { return Get("MergeLore7"); } }
    public static string MergeLore8 { get { return Get("MergeLore8"); } }
    public static string MergeLore9 { get { return Get("MergeLore9"); } }
    public static string MergeLore10 { get { return Get("MergeLore10"); } }
    
    // ============================================================
    // HERO NAMES
    // ============================================================
    public static string GetHeroName(string heroType)
    {
        if (string.IsNullOrEmpty(heroType)) return "";
        
        string baseName = heroType.Replace("Hero_", "");
        string key = "Hero_" + baseName;
        
        string value;
        if (_localizationData.TryGetValue(key, out value))
        {
            return value;
        }
        
        // Fallback: Husk -> Shell in English
        if (baseName == "Husk" && _currentLanguageCode.StartsWith("en"))
        {
            return "Shell";
        }
        
        return baseName;
    }
    
    // ============================================================
    // HELPER METHODS
    // ============================================================
    
    public static string GetSlotLabel(EquipmentSlotType slot)
    {
        switch (slot)
        {
            case EquipmentSlotType.Head: return HeadSlotLabel;
            case EquipmentSlotType.Chest: return ChestSlotLabel;
            case EquipmentSlotType.Legs: return LegsSlotLabel;
            case EquipmentSlotType.LeftHand: return LeftHandLabel;
            case EquipmentSlotType.RightHand: return RightHandLabel;
            case EquipmentSlotType.Amulet: return AmuletSlotLabel;
            case EquipmentSlotType.LeftRing: return LeftRingLabel;
            case EquipmentSlotType.RightRing: return RightRingLabel;
            case EquipmentSlotType.Belt: return BeltSlotLabel;
            case EquipmentSlotType.Boots: return BootsSlotLabel;
            default: return "";
        }
    }
    
    public static string GetRarityName(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Common;
            case ItemRarity.Rare: return Rare;
            case ItemRarity.Epic: return Epic;
            case ItemRarity.Legendary: return Legendary;
            default: return Common;
        }
    }
    
    public static string GetItemTypeName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return Weapon;
            case ItemType.TwoHandedWeapon: return TwoHandedWeapon;
            case ItemType.OffHand: return OffHand;
            case ItemType.Helmet: return Helmet;
            case ItemType.ChestArmor: return ChestArmor;
            case ItemType.Pants: return Pants;
            case ItemType.Boots: return Boots;
            case ItemType.Belt: return Belt;
            case ItemType.Amulet: return Amulet;
            case ItemType.Ring: return Ring;
            case ItemType.Consumable: return Consumable;
            case ItemType.Material: return Material;
            case ItemType.Quest: return QuestItem;
            case ItemType.Misc: return Misc;
            default: return Item;
        }
    }
    
    public static string GetElementalName(ElementalType type)
    {
        switch (type)
        {
            case ElementalType.Fire: return Fire;
            case ElementalType.Cold: return Cold;
            case ElementalType.Light: return Light;
            case ElementalType.Dark: return Dark;
            default: return "";
        }
    }
    
    public static string GetEquipmentSlotName(EquipmentSlotType slot)
    {
        switch (slot)
        {
            case EquipmentSlotType.Head: return Head;
            case EquipmentSlotType.Chest: return Chest;
            case EquipmentSlotType.Legs: return Legs;
            case EquipmentSlotType.LeftHand: return LeftHand;
            case EquipmentSlotType.RightHand: return RightHand;
            case EquipmentSlotType.Amulet: return AmuletSlot;
            case EquipmentSlotType.LeftRing: return LeftRing;
            case EquipmentSlotType.RightRing: return RightRing;
            case EquipmentSlotType.Belt: return BeltSlot;
            case EquipmentSlotType.Boots: return BootsSlot;
            default: return "";
        }
    }
    
    public static string GetWeaponTypeName(string weaponType)
    {
        switch (weaponType)
        {
            case "Sword": return WeaponSword;
            case "TwoHandedSword": return WeaponTwoHandedSword;
            case "Wand": return WeaponWand;
            case "Staff": return WeaponStaff;
            case "Bow": return WeaponBow;
            case "Crossbow": return WeaponCrossbow;
            case "Dagger": return WeaponDagger;
            case "DualDaggers": return WeaponDualDaggers;
            case "Hammer": return WeaponHammer;
            case "Shield": return WeaponShield;
            case "Rifle": return WeaponRifle;
            case "Pistol": return WeaponPistol;
            default: return weaponType;
        }
    }
    
    /// <summary>
    /// Get a random merge lore message
    /// </summary>
    public static string GetRandomMergeLore()
    {
        int index = UnityEngine.Random.Range(1, 11);
        return Get("MergeLore" + index);
    }
    
    // ============================================================
    // FORMAT HELPERS
    // ============================================================
    
    public static string FormatSellPrice(int price)
    {
        return Get("FormatSellPrice", price);
    }
    
    public static string FormatUpgradeCost(int cost)
    {
        return Get("FormatUpgradeCost", cost);
    }
    
    public static string FormatSellHint(int price)
    {
        return Get("FormatSellHint", price);
    }
    
    public static string FormatUpgradeHint(int cost)
    {
        return Get("FormatUpgradeHint", cost);
    }
    
    public static string FormatCleanseCost(int cost)
    {
        return Get("FormatCleanseCost", cost);
    }
    
    public static string FormatCleanseRefund(int refund)
    {
        return Get("FormatCleanseRefund", refund);
    }
    
    public static string FormatCleanseHint(int cost, int refund)
    {
        return Get("FormatCleanseHint", cost, refund);
    }
    
    public static string FormatDismantleHint(int value)
    {
        string keyText = GetDismantleKeyText();
        return Get("FormatDismantleHint", keyText, value);
    }
    
    public static string GetDismantleKeyText()
    {
        try
        {
            if (DewSave.profileMain != null && DewSave.profileMain.controls != null)
            {
                return DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.interactAlt);
            }
        }
        catch { }
        return "G";
    }
    
    public static string FormatDismantleValue(int value)
    {
        return Get("FormatDismantleValue", value);
    }
    
    public static string FormatBuyPrice(int price)
    {
        return Get("FormatBuyPrice", price);
    }
}
