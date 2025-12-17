using System;
using UnityEngine;

/// <summary>
/// Item Rarity enum - matches game's rarity system
/// </summary>
public enum ItemRarity
{
    Common,     // White/Gray (matches game's Common)
    Rare,       // Green (matches game's Rare)
    Epic,       // Purple (matches game's Epic)
    Legendary   // Orange/Gold (matches game's Legendary)
}

/// <summary>
/// Item Type enum - specific types for each equipment slot
/// </summary>
public enum ItemType
{
    // Weapons (hands)
    Weapon,           // One-handed weapon (main hand)
    TwoHandedWeapon,  // Two-handed weapon (uses both hand slots)
    OffHand,          // Off-hand item (shield, etc.)
    
    // Armor pieces
    Helmet,      // Head slot
    ChestArmor,  // Chest slot
    Pants,       // Legs slot
    Boots,       // Boots slot
    Belt,        // Belt slot
    
    // Accessories
    Amulet,      // Amulet slot
    Ring,        // Ring slots
    
    // Other
    Consumable,
    Material,
    Quest,
    Misc
}

/// <summary>
/// Equipment Slot Type enum
/// </summary>
public enum EquipmentSlotType
{
    Head,
    Chest,
    Legs,
    LeftHand,
    RightHand,
    Amulet,
    LeftRing,
    RightRing,
    Belt,
    Boots
}

/// <summary>
/// RPGItem - Core item data structure
/// Contains all item properties, stats, and display methods
/// </summary>
[Serializable]
public class RPGItem
{
    // ============================================================
    // BASIC PROPERTIES
    // ============================================================
    public int id;
    public string name;
    public string description;
    public ItemType type;
    public ItemRarity rarity;
    public int maxStack = 1;        // For consumables: -1 means unlimited stacks
    public int currentStack = 1;
    public string imagePath;        // Path to custom image file
    public Sprite sprite;           // Loaded sprite
    
    /// <summary>
    /// Returns true if this item can stack infinitely (consumables)
    /// </summary>
    public bool HasUnlimitedStacks { get { return maxStack < 0 || type == ItemType.Consumable; } }
    
    // ============================================================
    // BASE STATS (Upgradeable - increase when item is upgraded)
    // ============================================================
    public int attackBonus = 0;         // ATK - Physical damage
    public int defenseBonus = 0;        // DEF - Armor/damage reduction
    public int healthBonus = 0;         // HP - Max health
    public int abilityPowerBonus = 0;   // AP - Ability/magic damage
    public float criticalChance = 0f;   // % chance to crit
    public float criticalDamage = 0f;   // % bonus crit damage
    
    // ============================================================
    // GEAR EFFECTS (Fixed - NOT upgradeable, stay at base value)
    // ============================================================
    // Movement & Defense
    public float moveSpeedBonus = 0f;   // % movement speed
    public int dodgeChargesBonus = 0;   // Extra dodge charges
    public float dodgeCooldownReduction = 0f; // % dodge cooldown reduction
    
    // On-Kill Effects
    public int goldOnKill = 0;          // Gold dropped on kill (random 5-30 per point)
    public int dustOnKill = 0;          // Dust dropped on kill
    public float lifeSteal = 0f;        // % damage as healing
    public float thorns = 0f;           // % damage reflected
    
    // Regeneration
    public float regeneration = 0f;     // HP per second
    public float shieldRegeneration = 0f; // Shield per second
    
    // Speed & Cooldown
    public float attackSpeed = 0f;      // % attack speed
    public float memoryHaste = 0f;      // % memory cooldown reduction
    
    // Auto Targeting
    public bool autoAttack = false;     // Auto basic attack nearest
    public bool autoAim = false;        // Auto aim memories
    
    // Elemental (weapons only - applies element to basic attacks)
    public ElementalType? elementalType = null;  // Fire, Cold, Light, Dark
    public int elementalStacks = 1;     // Stacks applied per hit (limited for balance)
    
    // Hero restriction (empty = any hero can use)
    public string requiredHero = "";    // e.g., "Hero_Aurena", "Hero_Lacerta"
    
    // ============================================================
    // CONSUMABLE EFFECTS
    // ============================================================
    public float healPercentage = 0f;   // % of max HP to heal
    public float shieldPercentage = 0f; // % of max HP as shield
    
    // Upgrade system
    public int upgradeLevel = 0; // Upgrade level (+1, +2, etc.)
    public int dustSpentUpgrading = 0; // Total dust actually spent upgrading (for cleanse refund)

    // ============================================================
    // CONSTRUCTORS
    // ============================================================
    
    public RPGItem(int id, string name, string description, ItemType type, ItemRarity rarity, int maxStack = 1)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
        this.rarity = rarity;
        this.maxStack = maxStack;
        this.currentStack = 1;
    }

    // ============================================================
    // COLOR METHODS
    // ============================================================
    
    public Color GetRarityColor()
    {
        // Use game's exact rarity colors from Dew.cs
        try
        {
            Rarity gameRarity;
            switch (rarity)
            {
                case ItemRarity.Common: gameRarity = Rarity.Common; break;
                case ItemRarity.Rare: gameRarity = Rarity.Rare; break;
                case ItemRarity.Epic: gameRarity = Rarity.Epic; break;
                case ItemRarity.Legendary: gameRarity = Rarity.Legendary; break;
                default: gameRarity = Rarity.Common; break;
            }
            return Dew.GetRarityColor(gameRarity);
        }
        catch
        {
            // Fallback to hardcoded values matching game's Dew.cs
            switch (rarity)
            {
                case ItemRarity.Common: return new Color(76f/85f, 0.9411765f, 0.9490196f);  // Light cyan
                case ItemRarity.Rare: return new Color(0.20392157f, 84f/85f, 1f);           // Cyan
                case ItemRarity.Epic: return new Color(0.78039217f, 31f/85f, 0.96862745f);  // Purple
                case ItemRarity.Legendary: return new Color(1f, 0.24313726f, 0.20392157f);  // Red-orange
                default: return new Color(76f/85f, 0.9411765f, 0.9490196f);
            }
        }
    }
    
    public string GetRarityColorHex()
    {
        try
        {
            Rarity gameRarity;
            switch (rarity)
            {
                case ItemRarity.Common: gameRarity = Rarity.Common; break;
                case ItemRarity.Rare: gameRarity = Rarity.Rare; break;
                case ItemRarity.Epic: gameRarity = Rarity.Epic; break;
                case ItemRarity.Legendary: gameRarity = Rarity.Legendary; break;
                default: gameRarity = Rarity.Common; break;
            }
            return Dew.GetRarityColorHex(gameRarity);
        }
        catch
        {
            switch (rarity)
            {
                case ItemRarity.Common: return "#E4F0F2";
                case ItemRarity.Rare: return "#34FCFF";
                case ItemRarity.Epic: return "#C75CF7";
                case ItemRarity.Legendary: return "#FF3E34";
                default: return "#E4F0F2";
            }
        }
    }
    
    /// <summary>
    /// Get the color hex for an elemental type
    /// </summary>
    public static string GetElementalColorHex(ElementalType type)
    {
        switch (type)
        {
            case ElementalType.Fire: return "#ff6b35";   // Orange-red
            case ElementalType.Cold: return "#74b9ff";   // Ice blue
            case ElementalType.Light: return "#ffeaa7";  // Golden yellow
            case ElementalType.Dark: return "#a55eea";   // Purple
            default: return "#ffffff";
        }
    }

    // ============================================================
    // CLONE METHOD
    // ============================================================
    
    public RPGItem Clone()
    {
        RPGItem clone = new RPGItem(id, name, description, type, rarity, maxStack);
        
        // Refresh localized fields so tooltips show translated text
        clone.RefreshLocalizedFields();
        
        clone.currentStack = currentStack;
        clone.imagePath = imagePath;
        clone.sprite = sprite;
        // Base stats
        clone.attackBonus = attackBonus;
        clone.defenseBonus = defenseBonus;
        clone.healthBonus = healthBonus;
        clone.abilityPowerBonus = abilityPowerBonus;
        clone.criticalChance = criticalChance;
        clone.criticalDamage = criticalDamage;
        // Gear effects
        clone.moveSpeedBonus = moveSpeedBonus;
        clone.dodgeChargesBonus = dodgeChargesBonus;
        clone.dodgeCooldownReduction = dodgeCooldownReduction;
        clone.goldOnKill = goldOnKill;
        clone.thorns = thorns;
        clone.lifeSteal = lifeSteal;
        clone.dustOnKill = dustOnKill;
        clone.regeneration = regeneration;
        clone.autoAttack = autoAttack;
        clone.autoAim = autoAim;
        clone.attackSpeed = attackSpeed;
        clone.memoryHaste = memoryHaste;
        clone.elementalType = elementalType;
        clone.elementalStacks = elementalStacks;
        clone.requiredHero = requiredHero;
        clone.healPercentage = healPercentage;
        clone.shieldPercentage = shieldPercentage;
        clone.upgradeLevel = upgradeLevel;
        clone.dustSpentUpgrading = dustSpentUpgrading;
        return clone;
    }

    // ============================================================
    // DISPLAY NAME METHODS
    // ============================================================
    
    /// <summary>
    /// Get the localized name of this item
    /// </summary>
    public string GetLocalizedName()
    {
        // Try to get translated name for current language
        string translatedName = ItemTranslations.GetName(id);
        if (!string.IsNullOrEmpty(translatedName))
            return translatedName;
        
        // Fallback to English name
        return name;
    }
    
    /// <summary>
    /// Update the item's name and description fields with localized versions
    /// This is needed for game systems that access name/description fields directly
    /// </summary>
    public void RefreshLocalizedFields()
    {
        string localizedName = GetLocalizedName();
        if (!string.IsNullOrEmpty(localizedName))
            name = localizedName;
            
        string localizedDesc = GetLocalizedDescription();
        if (!string.IsNullOrEmpty(localizedDesc))
            description = localizedDesc;
    }
    
    /// <summary>
    /// Get the localized description of this item
    /// </summary>
    public string GetLocalizedDescription()
    {
        // Try to get translated description for current language
        string translatedDesc = ItemTranslations.GetDescription(id);
        if (!string.IsNullOrEmpty(translatedDesc))
            return translatedDesc;
        
        // Fallback to English description
        return description;
    }
    
    /// <summary>
    /// Get display name with upgrade level (e.g., "Hermes Boots +1")
    /// </summary>
    public string GetDisplayName()
    {
        string localizedName = GetLocalizedName();
        if (upgradeLevel > 0)
        {
            return localizedName + " +" + upgradeLevel;
        }
        return localizedName;
    }
    
    public string GetTypeName()
    {
        return Localization.GetItemTypeName(type);
    }
    
    /// <summary>
    /// Convert hero type name to display name (e.g., "Hero_Lacerta" -> "Lacerta" or localized)
    /// </summary>
    public string GetHeroDisplayName()
    {
        return Localization.GetHeroName(requiredHero);
    }

    /// <summary>
    /// Check if a hero type matches this item's requirement
    /// Handles Shell/Husk name equivalence (both refer to the same hero)
    /// </summary>
    public static bool HeroMatchesRequirement(string heroTypeName, string requiredHero)
    {
        if (string.IsNullOrEmpty(requiredHero)) return true;
        if (string.IsNullOrEmpty(heroTypeName)) return false;
        
        // Direct match
        if (heroTypeName == requiredHero) return true;
        
        // Handle Shell/Husk equivalence - both refer to the same hero
        // The game uses "Hero_Husk" but some items might reference "Hero_Shell"
        if ((heroTypeName == "Hero_Husk" || heroTypeName == "Hero_Shell") && 
            (requiredHero == "Hero_Husk" || requiredHero == "Hero_Shell"))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if the current player's hero matches this item's requirement
    /// </summary>
    public bool CanCurrentHeroUse()
    {
        if (string.IsNullOrEmpty(requiredHero)) return true;
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return false;
        
        string heroTypeName = DewPlayer.local.hero.GetType().Name;
        return HeroMatchesRequirement(heroTypeName, requiredHero);
    }

    // ============================================================
    // TOOLTIP METHODS
    // ============================================================
    
    /// <summary>
    /// Get the full tooltip content for this item - for share/show off (normal size title)
    /// </summary>
    public string GetFullTooltip()
    {
        return GetTooltipContent(false, true);
    }
    
    /// <summary>
    /// Get tooltip for inventory UI (large title)
    /// </summary>
    public string GetInventoryTooltip()
    {
        return GetTooltipContent(true, true);
    }
    
    /// <summary>
    /// Get tooltip for equipped items (no comparison since it's already equipped)
    /// </summary>
    public string GetEquippedTooltip()
    {
        return GetTooltipContent(true, false);
    }
    
    /// <summary>
    /// Internal method to generate tooltip content
    /// </summary>
    /// <param name="largeTitle">Use large title size</param>
    /// <param name="showComparison">Show comparison with equipped items (false for already equipped items)</param>
    private string GetTooltipContent(bool largeTitle, bool showComparison = true)
    {
        string rarityColorHex = GetRarityColorHex();
        string rarityName = Localization.GetRarityName(rarity);
        string typeName = GetTypeName();
        
        string tooltip;
        if (largeTitle)
        {
            tooltip = string.Format("<size=14><color={0}><b>{1}</b></color></size>\n", rarityColorHex, GetDisplayName());
        }
        else
        {
            tooltip = string.Format("<color={0}><b>{1}</b></color>\n", rarityColorHex, GetDisplayName());
        }
        
        // Type info
        tooltip += string.Format("<color=#888888>{0} {1}</color>\n", rarityName, typeName);
        
        // Hero requirement
        if (!string.IsNullOrEmpty(requiredHero))
        {
            string heroDisplayName = GetHeroDisplayName();
            
            // Check if current player can use it
            bool canUse = CanCurrentHeroUse();
            
            if (canUse)
            {
                tooltip += string.Format("<color=#55efc4>{0}: {1} ✓</color>\n", Localization.Requires, heroDisplayName);
            }
            else
            {
                tooltip += string.Format("<color=#ff6b6b>{0}: {1}</color>\n", Localization.Requires, heroDisplayName);
            }
        }
        
        tooltip += "\n";
        
        // Get equipped item(s) for comparison (only if showComparison is true)
        RPGItem equippedItem = null;
        RPGItem equippedItem2 = null; // For rings (second slot)
        bool isRing = (type == ItemType.Ring);
        
        if (showComparison)
        {
            if (isRing)
            {
                GetBothRingsForComparison(out equippedItem, out equippedItem2);
            }
            else if (type == ItemType.Weapon || type == ItemType.TwoHandedWeapon || type == ItemType.OffHand)
            {
                // For weapons/offhands, compare to both hand slots
                GetBothHandsForComparison(out equippedItem, out equippedItem2);
            }
            else
            {
                equippedItem = GetEquippedItemForComparison();
            }
        }
        
        // Add stats with colors and comparison (comparison only shown if equippedItem is not null)
        tooltip += FormatStatWithComparison(attackBonus, equippedItem, equippedItem2, "attackBonus", Localization.Attack, "#ff6b6b", false);
        tooltip += FormatDefenseWithReduction(defenseBonus, equippedItem, equippedItem2, showComparison);
        tooltip += FormatStatWithComparison(healthBonus, equippedItem, equippedItem2, "healthBonus", Localization.Health, "#95e66b", false);
        tooltip += FormatStatWithComparison(abilityPowerBonus, equippedItem, equippedItem2, "abilityPowerBonus", Localization.AbilityPower, "#CC66FF", false);
        tooltip += FormatStatWithComparison(criticalChance, equippedItem, equippedItem2, "criticalChance", Localization.CritChance, "#ff7675", true);
        tooltip += FormatStatWithComparison(criticalDamage, equippedItem, equippedItem2, "criticalDamage", Localization.CritDamage, "#e17055", true);
        tooltip += FormatStatWithComparison(moveSpeedBonus, equippedItem, equippedItem2, "moveSpeedBonus", Localization.MoveSpeed, "#ffeaa7", true);
        tooltip += FormatStatWithComparison(dodgeChargesBonus, equippedItem, equippedItem2, "dodgeChargesBonus", Localization.DodgeCharges, "#a29bfe", false);
        tooltip += FormatStatWithComparison(dodgeCooldownReduction, equippedItem, equippedItem2, "dodgeCooldownReduction", Localization.DodgeCooldown, "#a29bfe", true);
        // Gold/Dust on kill show range instead of raw value (each point = 5-30 gold/dust)
        tooltip += FormatOnKillStatWithComparison(goldOnKill, equippedItem, equippedItem2, "goldOnKill", Localization.GoldOnKill, "#ffd700");
        tooltip += FormatOnKillStatWithComparison(dustOnKill, equippedItem, equippedItem2, "dustOnKill", Localization.DustOnKill, "#b2bec3");
        tooltip += FormatStatWithComparison(lifeSteal, equippedItem, equippedItem2, "lifeSteal", Localization.LifeSteal, "#e84393", true);
        tooltip += FormatStatWithComparison(thorns, equippedItem, equippedItem2, "thorns", Localization.Thorns, "#d63031", true);
        tooltip += FormatStatWithComparison(regeneration, equippedItem, equippedItem2, "regeneration", Localization.Regeneration, "#00b894", false, "/s");
        tooltip += FormatStatWithComparison(shieldRegeneration, equippedItem, equippedItem2, "shieldRegeneration", Localization.ShieldRegeneration, "#74b9ff", false, "/s");
        tooltip += FormatStatWithComparison(attackSpeed, equippedItem, equippedItem2, "attackSpeed", Localization.AttackSpeed, "#fdcb6e", true);
        tooltip += FormatStatWithComparison(memoryHaste, equippedItem, equippedItem2, "memoryHaste", Localization.MemoryHaste, "#74b9ff", true);
        if (autoAttack) tooltip += string.Format("<color=#fab1a0>{0}</color>\n", Localization.AutoAttack);
        if (autoAim) tooltip += string.Format("<color=#81ecec>{0}</color>\n", Localization.AutoAim);
        
        // Show stats that would be LOST (on equipped item but not on this item) - only if comparing
        if (showComparison)
        {
            bool isDualSlot = isRing || type == ItemType.Weapon || type == ItemType.TwoHandedWeapon || type == ItemType.OffHand;
            string lostStats = GetLostStatsComparison(equippedItem, equippedItem2, isDualSlot);
            if (!string.IsNullOrEmpty(lostStats))
            {
                tooltip += lostStats;
            }
        }
        
        // Elemental
        if (elementalType.HasValue)
        {
            string elemColor = GetElementalColorHex(elementalType.Value);
            string elemName = Localization.GetElementalName(elementalType.Value);
            tooltip += string.Format("<color={0}>{1} +{2}</color>\n", elemColor, elemName, elementalStacks);
        }
        
        // Consumable effects
        if (healPercentage > 0) tooltip += string.Format("<color=#55efc4>{0} {1}% HP</color>\n", Localization.Heals, healPercentage);
        if (shieldPercentage > 0) tooltip += string.Format("<color=#74b9ff>+{0}% {1}</color>\n", shieldPercentage, Localization.Shield);
        
        // Stack info for stackable items
        if (HasUnlimitedStacks)
        {
            tooltip += string.Format("<color=#AAAAAA>{0}: {1}</color>\n", Localization.Stack, currentStack);
        }
        else if (maxStack > 1)
        {
            tooltip += string.Format("<color=#AAAAAA>{0}: {1}/{2}</color>\n", Localization.Stack, currentStack, maxStack);
        }
        
        // Description (localized)
        string localizedDesc = GetLocalizedDescription();
        if (!string.IsNullOrEmpty(localizedDesc))
        {
            tooltip += string.Format("\n<color=#AAAAAA><i>{0}</i></color>", localizedDesc);
        }
        
        return tooltip;
    }
    
    /// <summary>
    /// Get stats string for detailed display
    /// </summary>
    public string GetStatsString()
    {
        string stats = "";
        
        // Base Stats
        if (attackBonus > 0) stats += string.Format("\n<color=#ff6b6b>{0} +{1}</color>", Localization.ATK, attackBonus);
        if (defenseBonus > 0)
        {
            // Show armor value - the tooltip comparison will show the actual DR impact
            stats += string.Format("\n<color=#4ecdc4>{0} +{1}</color>", Localization.DEF, defenseBonus);
        }
        if (healthBonus > 0) stats += string.Format("\n<color=#95e66b>{0} +{1}</color>", Localization.HP, healthBonus);
        if (abilityPowerBonus > 0) stats += string.Format("\n<color=#CC66FF>{0} +{1}</color>", Localization.AP, abilityPowerBonus);
        if (criticalChance > 0) stats += string.Format("\n<color=#ff7675>{0} +{1}%</color>", Localization.Crit, criticalChance);
        if (criticalDamage > 0) stats += string.Format("\n<color=#e17055>{0} +{1}%</color>", Localization.CritDamage, criticalDamage);
        
        // Gear Effects
        if (moveSpeedBonus > 0) stats += string.Format("\n<color=#ffeaa7>{0} +{1}%</color>", Localization.Speed, moveSpeedBonus);
        if (dodgeChargesBonus > 0) stats += string.Format("\n<color=#a29bfe>{0} +{1}</color>", Localization.Dodge, dodgeChargesBonus);
        if (dodgeCooldownReduction > 0) stats += string.Format("\n<color=#a29bfe>{0} -{1}%</color>", Localization.DodgeCooldown, dodgeCooldownReduction);
        if (goldOnKill > 0)
        {
            // Base: 1-5 per point, scales with upgrade (uses configurable scaling)
            float mult = InventoryMerchantHelper.GetUpgradeMultiplier(upgradeLevel);
            int minG = (int)(goldOnKill * 1 * mult);
            int maxG = (int)(goldOnKill * 5 * mult);
            stats += string.Format("\n<color=#ffd700>{0} ({1}-{2}g)</color>", Localization.GoldOnKill, minG, maxG);
        }
        if (dustOnKill > 0)
        {
            // Base: 1-5 per point, scales with upgrade (uses configurable scaling)
            float mult = InventoryMerchantHelper.GetUpgradeMultiplier(upgradeLevel);
            int minD = (int)(dustOnKill * 1 * mult);
            int maxD = (int)(dustOnKill * 5 * mult);
            stats += string.Format("\n<color=#b2bec3>{0} ({1}-{2})</color>", Localization.DustOnKill, minD, maxD);
        }
        if (lifeSteal > 0) stats += string.Format("\n<color=#e84393>{0} {1}%</color>", Localization.LifeSteal, lifeSteal);
        if (thorns > 0) stats += string.Format("\n<color=#d63031>{0} {1}%</color>", Localization.Thorns, thorns);
        if (regeneration > 0) stats += string.Format("\n<color=#00b894>{0} +{1}/s</color>", Localization.Regeneration, regeneration);
        if (shieldRegeneration > 0) stats += string.Format("\n<color=#74b9ff>{0} +{1}/s</color>", Localization.ShieldRegeneration, shieldRegeneration);
        if (attackSpeed > 0) stats += string.Format("\n<color=#fdcb6e>{0} +{1}%</color>", Localization.AttackSpeed, attackSpeed);
        if (memoryHaste > 0) stats += string.Format("\n<color=#74b9ff>{0} +{1}%</color>", Localization.MemoryHaste, memoryHaste);
        if (autoAttack) stats += string.Format("\n<color=#fab1a0>{0}</color>", Localization.AutoAttack);
        if (autoAim) stats += string.Format("\n<color=#81ecec>{0}</color>", Localization.AutoAim);
        
        // Elemental
        if (elementalType.HasValue)
        {
            string elemColor = GetElementalColorHex(elementalType.Value);
            string elemName = Localization.GetElementalName(elementalType.Value);
            stats += string.Format("\n<color={0}>{1} +{2}</color>", elemColor, elemName, elementalStacks);
        }
        
        // Consumable Effects
        if (healPercentage > 0) stats += string.Format("\n<color=#55efc4>{0} {1}%</color>", Localization.Heals, healPercentage);
        if (shieldPercentage > 0) stats += string.Format("\n<color=#74b9ff>+{0}% {1}</color>", shieldPercentage, Localization.Shield);
        
        // Hero Restriction
        if (!string.IsNullOrEmpty(requiredHero))
        {
            string heroName = GetHeroDisplayName();
            stats += string.Format("\n<color=#e17055>{0}: {1}</color>", Localization.Requires, heroName);
        }
        
        return stats;
    }
    
    /// <summary>
    /// Get short stats string with colors for dropped item UI
    /// </summary>
    public string GetShortStatsString()
    {
        string stats = "";
        if (attackBonus > 0) stats += string.Format("<color=#ff6b6b>+{0} {1}</color>", attackBonus, Localization.ATK);
        if (defenseBonus > 0)
        {
            stats += (stats.Length > 0 ? ", " : "") + string.Format("<color=#4ecdc4>+{0} {1}</color>", defenseBonus, Localization.DEF);
        }
        if (healthBonus > 0) stats += (stats.Length > 0 ? ", " : "") + string.Format("<color=#95e66b>+{0} {1}</color>", healthBonus, Localization.HP);
        if (abilityPowerBonus > 0) stats += (stats.Length > 0 ? ", " : "") + string.Format("<color=#CC66FF>+{0} {1}</color>", abilityPowerBonus, Localization.AP);
        if (moveSpeedBonus > 0) stats += (stats.Length > 0 ? ", " : "") + string.Format("<color=#ffeaa7>+{0}% {1}</color>", moveSpeedBonus, Localization.Speed);
        if (dodgeChargesBonus > 0) stats += (stats.Length > 0 ? ", " : "") + string.Format("<color=#a29bfe>+{0} {1}</color>", dodgeChargesBonus, Localization.Dodge);
        if (dodgeCooldownReduction > 0) stats += (stats.Length > 0 ? ", " : "") + string.Format("<color=#a29bfe>-{0}% {1}</color>", dodgeCooldownReduction, Localization.DodgeCooldown);
        if (goldOnKill > 0) stats += (stats.Length > 0 ? ", " : "") + string.Format("<color=#ffd700>{0}</color>", Localization.GoldOnKill);
        if (healPercentage > 0) stats += (stats.Length > 0 ? ", " : "") + string.Format("<color=#55efc4>{0} {1}%</color>", Localization.Heals, healPercentage);
        if (shieldPercentage > 0) stats += (stats.Length > 0 ? ", " : "") + string.Format("<color=#74b9ff>+{0}% {1}</color>", shieldPercentage, Localization.Shield);
        return stats;
    }
    
    // ============================================================
    // STAT COMPARISON HELPERS
    // ============================================================
    
    /// <summary>
    /// Get the currently equipped item in the slot this item would go into
    /// </summary>
    private RPGItem GetEquippedItemForComparison()
    {
        // Get EquipmentManager from the mod instance
        RPGItemsMod modInstance = UnityEngine.Object.FindFirstObjectByType<RPGItemsMod>();
        if (modInstance == null) return null;
        
        EquipmentManager equipManager = modInstance.GetEquipmentManager();
        if (equipManager == null) return null;
        
        // Determine which slot this item type goes into
        EquipmentSlotType? slot = GetSlotForType();
        if (!slot.HasValue) return null;
        
        return equipManager.GetEquippedItem(slot.Value);
    }
    
    /// <summary>
    /// Get both equipped rings for comparison (for ring items)
    /// </summary>
    private void GetBothRingsForComparison(out RPGItem leftRing, out RPGItem rightRing)
    {
        leftRing = null;
        rightRing = null;
        
        RPGItemsMod modInstance = UnityEngine.Object.FindFirstObjectByType<RPGItemsMod>();
        if (modInstance == null) return;
        
        EquipmentManager equipManager = modInstance.GetEquipmentManager();
        if (equipManager == null) return;
        
        leftRing = equipManager.GetEquippedItem(EquipmentSlotType.LeftRing);
        rightRing = equipManager.GetEquippedItem(EquipmentSlotType.RightRing);
    }
    
    /// <summary>
    /// Get both equipped hand items for comparison (for weapon/offhand items)
    /// </summary>
    private void GetBothHandsForComparison(out RPGItem leftHand, out RPGItem rightHand)
    {
        leftHand = null;
        rightHand = null;
        
        RPGItemsMod modInstance = UnityEngine.Object.FindFirstObjectByType<RPGItemsMod>();
        if (modInstance == null) return;
        
        EquipmentManager equipManager = modInstance.GetEquipmentManager();
        if (equipManager == null) return;
        
        leftHand = equipManager.GetEquippedItem(EquipmentSlotType.LeftHand);
        rightHand = equipManager.GetEquippedItem(EquipmentSlotType.RightHand);
    }
    
    /// <summary>
    /// Get the equipment slot type for this item's type
    /// </summary>
    private EquipmentSlotType? GetSlotForType()
    {
        switch (type)
        {
            case ItemType.Weapon:
            case ItemType.TwoHandedWeapon:
                return EquipmentSlotType.RightHand;
            case ItemType.OffHand:
                return EquipmentSlotType.LeftHand;
            case ItemType.Helmet:
                return EquipmentSlotType.Head;
            case ItemType.ChestArmor:
                return EquipmentSlotType.Chest;
            case ItemType.Pants:
                return EquipmentSlotType.Legs;
            case ItemType.Boots:
                return EquipmentSlotType.Boots;
            case ItemType.Belt:
                return EquipmentSlotType.Belt;
            case ItemType.Amulet:
                return EquipmentSlotType.Amulet;
            case ItemType.Ring:
                return EquipmentSlotType.LeftRing; // Fallback, but GetEquippedItemForComparison handles rings specially
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Get stat value from item by stat name
    /// </summary>
    private float GetStatValue(RPGItem item, string statName)
    {
        if (item == null) return 0;
        
        switch (statName)
        {
            case "attackBonus": return item.attackBonus;
            case "defenseBonus": return item.defenseBonus;
            case "healthBonus": return item.healthBonus;
            case "abilityPowerBonus": return item.abilityPowerBonus;
            case "criticalChance": return item.criticalChance;
            case "criticalDamage": return item.criticalDamage;
            case "moveSpeedBonus": return item.moveSpeedBonus;
            case "dodgeChargesBonus": return item.dodgeChargesBonus;
            case "dodgeCooldownReduction": return item.dodgeCooldownReduction;
            case "goldOnKill": return item.goldOnKill;
            case "dustOnKill": return item.dustOnKill;
            case "lifeSteal": return item.lifeSteal;
            case "thorns": return item.thorns;
            case "regeneration": return item.regeneration;
            case "attackSpeed": return item.attackSpeed;
            case "memoryHaste": return item.memoryHaste;
            default: return 0;
        }
    }
    
    /// <summary>
    /// Format a single diff indicator
    /// Uses threshold to avoid floating point precision errors showing as scientific notation
    /// </summary>
    private string FormatDiff(float diff)
    {
        // Use a small threshold to avoid floating point precision errors (like 1.192093E-07)
        const float EPSILON = 0.001f;
        
        if (diff > EPSILON)
        {
            // Format with 1 decimal place to avoid scientific notation
            if (diff >= 1f)
                return string.Format("<color=#55efc4>▲{0:F0}</color>", diff);
            else
                return string.Format("<color=#55efc4>▲{0:F1}</color>", diff);
        }
        else if (diff < -EPSILON)
        {
            float absDiff = -diff;
            if (absDiff >= 1f)
                return string.Format("<color=#ff6b6b>▼{0:F0}</color>", absDiff);
            else
                return string.Format("<color=#ff6b6b>▼{0:F1}</color>", absDiff);
        }
        else
        {
            return "<color=#888888>=</color>";
        }
    }
    
    /// <summary>
    /// Calculate damage reduction percentage from armor value
    /// Uses game's formula: damageMultiplier = 100 / (100 + armor)
    /// </summary>
    private float CalculateDamageReduction(float armor)
    {
        if (armor <= 0) return 0f;
        return (1f - (100f / (100f + armor))) * 100f;
    }
    
    /// <summary>
    /// Get current total armor from hero (excluding the item being compared)
    /// </summary>
    private float GetCurrentTotalArmor()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return 0f;
        return DewPlayer.local.hero.Status.armor;
    }
    
    /// <summary>
    /// Format defense stat showing ACTUAL DR change when equipping this item
    /// Shows: "+X Armor (Total DR: Y% → Z%)" so players understand the real impact
    /// For rings/weapons: shows comparison against both slots (L: R:)
    /// </summary>
    private string FormatDefenseWithReduction(int defValue, RPGItem equipped1, RPGItem equipped2, bool showComparison)
    {
        if (defValue <= 0) return "";
        
        // If not showing comparison (this is an equipped item), show flat stat with current total DR
        if (!showComparison)
        {
            float currentTotalArmorEquipped = GetCurrentTotalArmor();
            float currentTotalDREquipped = CalculateDamageReduction(currentTotalArmorEquipped);
            string drLabelEquipped = Localization.CurrentLanguage == ModLanguage.Chinese ? "总减伤" : "Total DR";
            return string.Format("<color=#4ecdc4>+{0} {1}</color> <color=#888>({2}: {3:F1}%)</color>\n", 
                defValue, Localization.Defense, drLabelEquipped, currentTotalDREquipped);
        }
        
        // Get current total armor from hero
        float currentTotalArmor = GetCurrentTotalArmor();
        float currentTotalDR = CalculateDamageReduction(currentTotalArmor);
        
        string drLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "减伤" : "DR";
        string totalLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "总" : "Total";
        
        // Check if this is a dual-slot comparison (rings or hands)
        bool isRing = type == ItemType.Ring;
        bool isHand = type == ItemType.Weapon || type == ItemType.TwoHandedWeapon || type == ItemType.OffHand;
        bool isDualSlot = (equipped1 != null || equipped2 != null) && (isRing || isHand);
        
        string result;
        
        if (isDualSlot)
        {
            // Dual slot comparison - show both slots
            float eq1Def = GetStatValue(equipped1, "defenseBonus");
            float eq2Def = GetStatValue(equipped2, "defenseBonus");
            
            // Calculate DR change for each slot
            float newArmor1 = currentTotalArmor - eq1Def + defValue;
            float newArmor2 = currentTotalArmor - eq2Def + defValue;
            float newDR1 = CalculateDamageReduction(newArmor1);
            float newDR2 = CalculateDamageReduction(newArmor2);
            float drChange1 = newDR1 - currentTotalDR;
            float drChange2 = newDR2 - currentTotalDR;
            
            // If neither slot has defense and this item does, it's a pure gain
            if (eq1Def <= 0 && eq2Def <= 0)
            {
                float newTotalArmor = currentTotalArmor + defValue;
                float newTotalDR = CalculateDamageReduction(newTotalArmor);
                result = string.Format("<color=#55efc4>+{0} {1} ({2} {3}: {4:F1}% → {5:F1}%)</color>\n", 
                    defValue, Localization.Defense, totalLabel, drLabel, currentTotalDR, newTotalDR);
            }
            else
            {
                // Show comparison for both: L=Left, R=Right
                string lLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "左" : "L";
                string rLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "右" : "R";
                
                string leftDiff = equipped1 != null ? FormatDiffPercent(drChange1) : "<color=#55efc4>+</color>";
                string rightDiff = equipped2 != null ? FormatDiffPercent(drChange2) : "<color=#55efc4>+</color>";
                
                result = string.Format("<color=#4ecdc4>+{0} {1}</color> ({2}:{3} {4}:{5})\n", 
                    defValue, Localization.Defense,
                    lLabel, leftDiff, rLabel, rightDiff);
            }
        }
        else
        {
            // Single slot comparison
            float equippedDef = GetStatValue(equipped1, "defenseBonus");
            
            // Calculate new total if we swap items
            float newTotalArmor = currentTotalArmor - equippedDef + defValue;
            float newTotalDR = CalculateDamageReduction(newTotalArmor);
            float drChange = newTotalDR - currentTotalDR;
            
            if (equippedDef <= 0)
            {
                // No equipped item in this slot - this is pure gain
                float newTotalArmor2 = currentTotalArmor + defValue;
                float newTotalDR2 = CalculateDamageReduction(newTotalArmor2);
                result = string.Format("<color=#55efc4>+{0} {1} ({2} {3}: {4:F1}% → {5:F1}%)</color>\n", 
                    defValue, Localization.Defense, totalLabel, drLabel, currentTotalDR, newTotalDR2);
            }
            else if (drChange > 0.1f)
            {
                // Better than equipped
                result = string.Format("<color=#4ecdc4>+{0} {1}</color> <color=#55efc4>({2} {3}: {4:F1}% → {5:F1}% ▲{6:F1}%)</color>\n", 
                    defValue, Localization.Defense, totalLabel, drLabel, currentTotalDR, newTotalDR, drChange);
            }
            else if (drChange < -0.1f)
            {
                // Worse than equipped
                result = string.Format("<color=#4ecdc4>+{0} {1}</color> <color=#ff6b6b>({2} {3}: {4:F1}% → {5:F1}% ▼{6:F1}%)</color>\n", 
                    defValue, Localization.Defense, totalLabel, drLabel, currentTotalDR, newTotalDR, -drChange);
            }
            else
            {
                // Same
                result = string.Format("<color=#4ecdc4>+{0} {1}</color> <color=#888>({2} {3}: {4:F1}%)</color>\n", 
                    defValue, Localization.Defense, totalLabel, drLabel, currentTotalDR);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Format difference for percentage values
    /// </summary>
    private string FormatDiffPercent(float diff)
    {
        if (diff > 0.1f)
            return string.Format("<color=#55efc4>▲{0:F1}%</color>", diff);
        else if (diff < -0.1f)
            return string.Format("<color=#ff6b6b>▼{0:F1}%</color>", -diff);
        else
            return "<color=#888888>=</color>";
    }
    
    /// <summary>
    /// Format a stat with comparison to equipped item(s)
    /// For rings/hands: shows (L: ▲5 R: ▼2) for both slots
    /// For other items: shows (▲5) or (▼3)
    /// </summary>
    private string FormatStatWithComparison(float thisValue, RPGItem equipped1, RPGItem equipped2, string statName, string displayName, string baseColor, bool isPercent, string suffix = "")
    {
        if (thisValue <= 0) return "";
        
        string percentSign = isPercent ? "%" : "";
        float eq1Value = GetStatValue(equipped1, statName);
        float eq2Value = GetStatValue(equipped2, statName);
        
        // Check if this is a dual-slot comparison (rings or hands)
        bool isRing = type == ItemType.Ring;
        bool isHand = type == ItemType.Weapon || type == ItemType.TwoHandedWeapon || type == ItemType.OffHand;
        bool isDualSlot = (equipped1 != null || equipped2 != null) && (isRing || isHand);
        
        string result;
        
        if (isDualSlot)
        {
            // Dual slot comparison - show both slots
            float diff1 = thisValue - eq1Value;
            float diff2 = thisValue - eq2Value;
            
            // If neither slot has this stat and this item does, it's a pure gain
            if (eq1Value <= 0 && eq2Value <= 0)
            {
                result = string.Format("<color=#55efc4>+{0}{1} {2}{3}</color>\n", thisValue, percentSign, displayName, suffix);
            }
            else
            {
                // Show comparison for both: L=Left, R=Right
                string lLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "左" : "L";
                string rLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "右" : "R";
                
                string leftDiff = equipped1 != null ? FormatDiff(diff1) : "<color=#55efc4>+</color>";
                string rightDiff = equipped2 != null ? FormatDiff(diff2) : "<color=#55efc4>+</color>";
                
                result = string.Format("<color={0}>+{1}{2} {3}{4}</color> ({5}:{6} {7}:{8})\n", 
                    baseColor, thisValue, percentSign, displayName, suffix,
                    lLabel, leftDiff, rLabel, rightDiff);
            }
        }
        else
        {
            // Single slot comparison
            float equippedValue = eq1Value; // Use first equipped item
            float diff = thisValue - equippedValue;
            
            // Use threshold to avoid floating point precision errors
            const float EPSILON = 0.001f;
            
            if (equippedValue <= 0)
            {
                // No equipped item or equipped item has 0 of this stat - just show the value in green (it's a gain)
                result = string.Format("<color=#55efc4>+{0}{1} {2}{3}</color>\n", thisValue, percentSign, displayName, suffix);
            }
            else if (diff > EPSILON)
            {
                // This item is better - show green arrow with difference (format to avoid scientific notation)
                string diffStr = diff >= 1f ? string.Format("{0:F0}", diff) : string.Format("{0:F1}", diff);
                result = string.Format("<color={0}>+{1}{2} {3}{4}</color> <color=#55efc4>(▲{5})</color>\n", 
                    baseColor, thisValue, percentSign, displayName, suffix, diffStr);
            }
            else if (diff < -EPSILON)
            {
                // This item is worse - show red arrow with difference (format to avoid scientific notation)
                float absDiff = -diff;
                string diffStr = absDiff >= 1f ? string.Format("{0:F0}", absDiff) : string.Format("{0:F1}", absDiff);
                result = string.Format("<color={0}>+{1}{2} {3}{4}</color> <color=#ff6b6b>(▼{5})</color>\n", 
                    baseColor, thisValue, percentSign, displayName, suffix, diffStr);
            }
            else
            {
                // Same value (within epsilon) - just show normally
                result = string.Format("<color={0}>+{1}{2} {3}{4}</color>\n", baseColor, thisValue, percentSign, displayName, suffix);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Format an integer stat with comparison
    /// </summary>
    private string FormatStatWithComparison(int thisValue, RPGItem equipped1, RPGItem equipped2, string statName, string displayName, string baseColor, bool isPercent, string suffix = "")
    {
        return FormatStatWithComparison((float)thisValue, equipped1, equipped2, statName, displayName, baseColor, isPercent, suffix);
    }
    
    /// <summary>
    /// Format on-kill stats (gold/dust) with range display
    /// Base: 1-5 per point (Normal monsters), scales with upgrade level (+50% per level)
    /// </summary>
    private string FormatOnKillStatWithComparison(int thisValue, RPGItem equipped1, RPGItem equipped2, string statName, string displayName, string baseColor)
    {
        if (thisValue <= 0) return "";
        
        int eq1Value = (int)GetStatValue(equipped1, statName);
        int eq2Value = (int)GetStatValue(equipped2, statName);
        
        // Calculate actual range based on upgrade level
        // Base: 1-5 per point (what Normal monsters drop)
        // Uses configurable upgrade scaling from UpgradeSystem
        float upgradeMultiplier = InventoryMerchantHelper.GetUpgradeMultiplier(upgradeLevel);
        int minGain = (int)(thisValue * 1 * upgradeMultiplier);
        int maxGain = (int)(thisValue * 5 * upgradeMultiplier);
        string rangeText = string.Format("{0}-{1}", minGain, maxGain);
        
        // Check if this is a dual-slot comparison (rings or hands)
        bool isRing = type == ItemType.Ring;
        bool isHand = type == ItemType.Weapon || type == ItemType.TwoHandedWeapon || type == ItemType.OffHand;
        bool isDualSlot = (equipped1 != null || equipped2 != null) && (isRing || isHand);
        
        string result;
        
        if (isDualSlot)
        {
            // Dual slot comparison - show both slots
            int diff1 = thisValue - eq1Value;
            int diff2 = thisValue - eq2Value;
            
            // If neither slot has this stat and this item does, it's a pure gain
            if (eq1Value <= 0 && eq2Value <= 0)
            {
                result = string.Format("<color=#55efc4>{0} ({1})</color>\n", displayName, rangeText);
            }
            else
            {
                // Show comparison for both: L=Left, R=Right
                string lLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "左" : "L";
                string rLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "右" : "R";
                
                string leftDiff = equipped1 != null ? FormatDiff(diff1) : "<color=#55efc4>+</color>";
                string rightDiff = equipped2 != null ? FormatDiff(diff2) : "<color=#55efc4>+</color>";
                
                result = string.Format("<color={0}>{1} ({2})</color> ({3}:{4} {5}:{6})\n", 
                    baseColor, displayName, rangeText,
                    lLabel, leftDiff, rLabel, rightDiff);
            }
        }
        else
        {
            // Single slot comparison
            int equippedValue = eq1Value > 0 ? eq1Value : eq2Value;
            int diff = thisValue - equippedValue;
            
            if (equippedValue <= 0)
            {
                // No equipped item or equipped item has 0 of this stat - just show the value in green (it's a gain)
                result = string.Format("<color=#55efc4>{0} ({1})</color>\n", displayName, rangeText);
            }
            else if (diff > 0)
            {
                // This item is better - show green arrow with difference
                result = string.Format("<color={0}>{1} ({2})</color> <color=#55efc4>(▲{3})</color>\n", 
                    baseColor, displayName, rangeText, diff);
            }
            else if (diff < 0)
            {
                // This item is worse - show red arrow with difference
                result = string.Format("<color={0}>{1} ({2})</color> <color=#ff6b6b>(▼{3})</color>\n", 
                    baseColor, displayName, rangeText, -diff);
            }
            else
            {
                // Same value - just show normally
                result = string.Format("<color={0}>{1} ({2})</color>\n", baseColor, displayName, rangeText);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Get stats that would be LOST when equipping this item (stats on equipped item that this item doesn't have)
    /// For dual-slot items (rings/hands), shows lost stats from BOTH slots with (L: R:) format
    /// </summary>
    private string GetLostStatsComparison(RPGItem equipped1, RPGItem equipped2, bool isDualSlot)
    {
        // If nothing is equipped, nothing to lose
        if (equipped1 == null && equipped2 == null) return "";
        
        string result = "";
        string loseLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "失去" : "Lose";
        string lLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "左" : "L";
        string rLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "右" : "R";
        
        if (isDualSlot)
        {
            // For dual-slot items, show lost stats from BOTH equipped items
            result += GetDualSlotLostStat(equipped1, equipped2, "attackBonus", attackBonus, Localization.Attack, false, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "defenseBonus", defenseBonus, Localization.Defense, false, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "healthBonus", healthBonus, Localization.Health, false, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "abilityPowerBonus", abilityPowerBonus, Localization.AbilityPower, false, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "criticalChance", criticalChance, Localization.CritChance, true, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "criticalDamage", criticalDamage, Localization.CritDamage, true, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "moveSpeedBonus", moveSpeedBonus, Localization.MoveSpeed, true, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "dodgeChargesBonus", dodgeChargesBonus, Localization.DodgeCharges, false, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "dodgeCooldownReduction", dodgeCooldownReduction, Localization.DodgeCooldown, true, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "goldOnKill", goldOnKill, Localization.GoldOnKill, false, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "dustOnKill", dustOnKill, Localization.DustOnKill, false, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "lifeSteal", lifeSteal, Localization.LifeSteal, true, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "thorns", thorns, Localization.Thorns, true, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStatWithSuffix(equipped1, equipped2, "regeneration", regeneration, Localization.Regeneration, false, "/s", lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "attackSpeed", attackSpeed, Localization.AttackSpeed, true, lLabel, rLabel, loseLabel);
            result += GetDualSlotLostStat(equipped1, equipped2, "memoryHaste", memoryHaste, Localization.MemoryHaste, true, lLabel, rLabel, loseLabel);
            
            // Special effects - check both items
            if ((GetBoolStat(equipped1, "autoAttack") || GetBoolStat(equipped2, "autoAttack")) && !autoAttack)
            {
                string which = "";
                if (GetBoolStat(equipped1, "autoAttack") && GetBoolStat(equipped2, "autoAttack"))
                    which = string.Format(" ({0}+{1})", lLabel, rLabel);
                else if (GetBoolStat(equipped1, "autoAttack"))
                    which = string.Format(" ({0})", lLabel);
                else
                    which = string.Format(" ({0})", rLabel);
                result += string.Format("<color=#ff6b6b>-{0}{1} ({2})</color>\n", Localization.AutoAttack, which, loseLabel);
            }
            
            if ((GetBoolStat(equipped1, "autoAim") || GetBoolStat(equipped2, "autoAim")) && !autoAim)
            {
                string which = "";
                if (GetBoolStat(equipped1, "autoAim") && GetBoolStat(equipped2, "autoAim"))
                    which = string.Format(" ({0}+{1})", lLabel, rLabel);
                else if (GetBoolStat(equipped1, "autoAim"))
                    which = string.Format(" ({0})", lLabel);
                else
                    which = string.Format(" ({0})", rLabel);
                result += string.Format("<color=#ff6b6b>-{0}{1} ({2})</color>\n", Localization.AutoAim, which, loseLabel);
            }
            
            // Elemental - check both items
            if (!elementalType.HasValue)
            {
                if (equipped1 != null && equipped1.elementalType.HasValue)
                {
                    string elemName = Localization.GetElementalName(equipped1.elementalType.Value);
                    result += string.Format("<color=#ff6b6b>-{0} +{1} ({2}) ({3})</color>\n", elemName, equipped1.elementalStacks, lLabel, loseLabel);
                }
                if (equipped2 != null && equipped2.elementalType.HasValue)
                {
                    string elemName = Localization.GetElementalName(equipped2.elementalType.Value);
                    result += string.Format("<color=#ff6b6b>-{0} +{1} ({2}) ({3})</color>\n", elemName, equipped2.elementalStacks, rLabel, loseLabel);
                }
            }
        }
        else
        {
            // Single slot - use original logic
            RPGItem equippedItem = equipped1;
            if (equippedItem == null) return "";
            
            if (equippedItem.attackBonus > 0 && attackBonus <= 0)
                result += string.Format("<color=#ff6b6b>-{0} {1} ({2})</color>\n", equippedItem.attackBonus, Localization.Attack, loseLabel);
            
            if (equippedItem.defenseBonus > 0 && defenseBonus <= 0)
                result += string.Format("<color=#ff6b6b>-{0} {1} ({2})</color>\n", equippedItem.defenseBonus, Localization.Defense, loseLabel);
            
            if (equippedItem.healthBonus > 0 && healthBonus <= 0)
                result += string.Format("<color=#ff6b6b>-{0} {1} ({2})</color>\n", equippedItem.healthBonus, Localization.Health, loseLabel);
            
            if (equippedItem.abilityPowerBonus > 0 && abilityPowerBonus <= 0)
                result += string.Format("<color=#ff6b6b>-{0} {1} ({2})</color>\n", equippedItem.abilityPowerBonus, Localization.AbilityPower, loseLabel);
            
            if (equippedItem.criticalChance > 0 && criticalChance <= 0)
                result += string.Format("<color=#ff6b6b>-{0}% {1} ({2})</color>\n", equippedItem.criticalChance, Localization.CritChance, loseLabel);
            
            if (equippedItem.criticalDamage > 0 && criticalDamage <= 0)
                result += string.Format("<color=#ff6b6b>-{0}% {1} ({2})</color>\n", equippedItem.criticalDamage, Localization.CritDamage, loseLabel);
            
            if (equippedItem.moveSpeedBonus > 0 && moveSpeedBonus <= 0)
                result += string.Format("<color=#ff6b6b>-{0}% {1} ({2})</color>\n", equippedItem.moveSpeedBonus, Localization.MoveSpeed, loseLabel);
            
            if (equippedItem.dodgeChargesBonus > 0 && dodgeChargesBonus <= 0)
                result += string.Format("<color=#ff6b6b>-{0} {1} ({2})</color>\n", equippedItem.dodgeChargesBonus, Localization.DodgeCharges, loseLabel);
            
            if (equippedItem.dodgeCooldownReduction > 0 && dodgeCooldownReduction <= 0)
                result += string.Format("<color=#ff6b6b>-{0}% {1} ({2})</color>\n", equippedItem.dodgeCooldownReduction, Localization.DodgeCooldown, loseLabel);
            
            if (equippedItem.goldOnKill > 0 && goldOnKill <= 0)
                result += string.Format("<color=#ff6b6b>-{0} ({1})</color>\n", Localization.GoldOnKill, loseLabel);
            
            if (equippedItem.dustOnKill > 0 && dustOnKill <= 0)
                result += string.Format("<color=#ff6b6b>-{0} ({1})</color>\n", Localization.DustOnKill, loseLabel);
            
            if (equippedItem.lifeSteal > 0 && lifeSteal <= 0)
                result += string.Format("<color=#ff6b6b>-{0}% {1} ({2})</color>\n", equippedItem.lifeSteal, Localization.LifeSteal, loseLabel);
            
            if (equippedItem.thorns > 0 && thorns <= 0)
                result += string.Format("<color=#ff6b6b>-{0}% {1} ({2})</color>\n", equippedItem.thorns, Localization.Thorns, loseLabel);
            
            if (equippedItem.regeneration > 0 && regeneration <= 0)
                result += string.Format("<color=#ff6b6b>-{0} {1}/s ({2})</color>\n", equippedItem.regeneration, Localization.Regeneration, loseLabel);
            
            if (equippedItem.attackSpeed > 0 && attackSpeed <= 0)
                result += string.Format("<color=#ff6b6b>-{0}% {1} ({2})</color>\n", equippedItem.attackSpeed, Localization.AttackSpeed, loseLabel);
            
            if (equippedItem.memoryHaste > 0 && memoryHaste <= 0)
                result += string.Format("<color=#ff6b6b>-{0}% {1} ({2})</color>\n", equippedItem.memoryHaste, Localization.MemoryHaste, loseLabel);
            
            // Special effects
            if (equippedItem.autoAttack && !autoAttack)
                result += string.Format("<color=#ff6b6b>-{0} ({1})</color>\n", Localization.AutoAttack, loseLabel);
            
            if (equippedItem.autoAim && !autoAim)
                result += string.Format("<color=#ff6b6b>-{0} ({1})</color>\n", Localization.AutoAim, loseLabel);
            
            // Elemental
            if (equippedItem.elementalType.HasValue && !elementalType.HasValue)
            {
                string elemName = Localization.GetElementalName(equippedItem.elementalType.Value);
                result += string.Format("<color=#ff6b6b>-{0} +{1} ({2})</color>\n", elemName, equippedItem.elementalStacks, loseLabel);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Helper to get lost stat comparison for dual-slot items
    /// Shows what you'd lose from BOTH equipped items
    /// </summary>
    private string GetDualSlotLostStat(RPGItem eq1, RPGItem eq2, string statName, float thisValue, string displayName, bool isPercent, string lLabel, string rLabel, string loseLabel)
    {
        return GetDualSlotLostStatWithSuffix(eq1, eq2, statName, thisValue, displayName, isPercent, "", lLabel, rLabel, loseLabel);
    }
    
    private string GetDualSlotLostStatWithSuffix(RPGItem eq1, RPGItem eq2, string statName, float thisValue, string displayName, bool isPercent, string suffix, string lLabel, string rLabel, string loseLabel)
    {
        float eq1Val = GetStatValue(eq1, statName);
        float eq2Val = GetStatValue(eq2, statName);
        
        // Only show if this item doesn't have this stat but equipped items do
        if (thisValue > 0) return "";
        if (eq1Val <= 0 && eq2Val <= 0) return "";
        
        string percentSign = isPercent ? "%" : "";
        
        // Show what you'd lose from each slot
        string losses = "";
        if (eq1Val > 0)
            losses += string.Format("{0}:-{1}{2}", lLabel, eq1Val, percentSign);
        if (eq2Val > 0)
        {
            if (!string.IsNullOrEmpty(losses)) losses += " ";
            losses += string.Format("{0}:-{1}{2}", rLabel, eq2Val, percentSign);
        }
        
        return string.Format("<color=#ff6b6b>{0} ({1}) ({2})</color>\n", displayName, losses, loseLabel);
    }
    
    /// <summary>
    /// Helper to get boolean stat from item
    /// </summary>
    private bool GetBoolStat(RPGItem item, string statName)
    {
        if (item == null) return false;
        
        switch (statName)
        {
            case "autoAttack": return item.autoAttack;
            case "autoAim": return item.autoAim;
            default: return false;
        }
    }
    
    /// <summary>
    /// Calculate total stat value for comparison purposes
    /// </summary>
    private float GetTotalStatValue(RPGItem item)
    {
        if (item == null) return 0f;
        
        float total = 0f;
        total += item.attackBonus;
        total += item.defenseBonus;
        total += item.healthBonus * 0.1f; // Weight health less
        total += item.abilityPowerBonus;
        total += item.criticalChance * 10f; // Weight crit more
        total += item.criticalDamage * 5f;
        total += item.moveSpeedBonus * 5f;
        total += item.dodgeChargesBonus * 20f;
        total += item.dodgeCooldownReduction * 3f;
        total += item.goldOnKill * 5f;
        total += item.dustOnKill * 5f;
        total += item.lifeSteal * 10f;
        total += item.thorns * 5f;
        total += item.regeneration * 5f;
        total += item.attackSpeed * 5f;
        total += item.memoryHaste * 5f;
        
        return total;
    }
}

