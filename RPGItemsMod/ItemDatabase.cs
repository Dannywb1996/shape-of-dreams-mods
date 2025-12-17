using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Central database for all RPG items - 478 items total.
/// All item definitions are here - MerchantPatches and MonsterLootSystem use this.
/// Item IDs are consecutive from 1-478 with no gaps.
/// </summary>
public static class ItemDatabase
{
    private static List<RPGItem> _allItems = null;
    private static string _modPath = "";
    private static bool _isInitialized = false;
    
    // Category caches for balanced loot distribution
    private static List<RPGItem> _weapons = null;
    private static List<RPGItem> _armor = null;
    private static List<RPGItem> _accessories = null;
    private static List<RPGItem> _consumables = null;
    
    // Cached reflection for image loading (avoids 223x assembly lookups!)
    private static System.Reflection.MethodInfo _loadImageMethod = null;
    private static bool _loadImageMethodSearched = false;
    
    // Sprite cache to avoid reloading from disk
    private static Dictionary<string, Sprite> _spriteCache = null;
    
    /// <summary>
    /// Initialize the item database with the mod path for loading sprites
    /// Note: Does NOT clear sprite cache - sprites persist for performance
    /// </summary>
    public static void Initialize(string modPath)
    {
        // Only reinitialize if path changed or not initialized
        if (_modPath == modPath && _isInitialized && _allItems != null)
        {
            return; // Already initialized with same path, skip reload
        }
        
        _modPath = modPath;
        _allItems = null;
        _weapons = null;
        _armor = null;
        _accessories = null;
        _consumables = null;
        _isInitialized = false;
        // NOTE: Don't clear _spriteCache - sprites should persist for performance!
    }
    
    /// <summary>
    /// Refresh all items' name and description fields with localized versions
    /// Call this after language detection to ensure tooltips show translated text
    /// </summary>
    public static void RefreshAllLocalizedFields()
    {
        if (_allItems == null) LoadAllItems();
        
        Debug.Log(string.Format("[RPGItems] Refreshing {0} items for language: {1}", _allItems.Count, Localization.CurrentLanguageCode));
        
        foreach (RPGItem item in _allItems)
        {
            if (item != null)
            {
                item.RefreshLocalizedFields();
            }
        }
    }
    
    /// <summary>
    /// Check if the database is initialized
    /// </summary>
    public static bool IsInitialized { get { return _isInitialized && _allItems != null; } }
    
    /// <summary>
    /// Get all items (loads and caches on first call)
    /// </summary>
    public static List<RPGItem> GetAllItems()
    {
        if (_allItems == null)
        {
            LoadAllItems();
        }
        return _allItems;
    }
    
    /// <summary>
    /// Get all weapons (Weapon and TwoHandedWeapon types)
    /// </summary>
    public static List<RPGItem> GetWeapons()
    {
        if (_allItems == null) LoadAllItems();
        if (_weapons == null) CategorizeItems();
        return _weapons;
    }
    
    /// <summary>
    /// Get all armor (Helmet, ChestArmor, Pants, Boots)
    /// </summary>
    public static List<RPGItem> GetArmor()
    {
        if (_allItems == null) LoadAllItems();
        if (_armor == null) CategorizeItems();
        return _armor;
    }
    
    /// <summary>
    /// Get all accessories (Ring, Amulet, Belt)
    /// </summary>
    public static List<RPGItem> GetAccessories()
    {
        if (_allItems == null) LoadAllItems();
        if (_accessories == null) CategorizeItems();
        return _accessories;
    }
    
    /// <summary>
    /// Get all consumables
    /// </summary>
    public static List<RPGItem> GetConsumables()
    {
        if (_allItems == null) LoadAllItems();
        if (_consumables == null) CategorizeItems();
        return _consumables;
    }
    
    /// <summary>
    /// Force reload all items (useful after language change)
    /// </summary>
    public static void Reload()
    {
        _allItems = null;
        _weapons = null;
        _armor = null;
        _accessories = null;
        _consumables = null;
        _isInitialized = false;
        LoadAllItems();
    }
    
    /// <summary>
    /// Get an item by ID
    /// </summary>
    public static RPGItem GetItemById(int id)
    {
        if (_allItems == null) LoadAllItems();
        
        foreach (RPGItem item in _allItems)
        {
            if (item.id == id) return item;
        }
        return null;
    }
    
    private static void CategorizeItems()
    {
        _weapons = new List<RPGItem>();
        _armor = new List<RPGItem>();
        _accessories = new List<RPGItem>();
        _consumables = new List<RPGItem>();
        
        foreach (RPGItem item in _allItems)
        {
            switch (item.type)
            {
                case ItemType.Weapon:
                case ItemType.TwoHandedWeapon:
                    _weapons.Add(item);
                    break;
                case ItemType.Helmet:
                case ItemType.ChestArmor:
                case ItemType.Pants:
                case ItemType.Boots:
                    _armor.Add(item);
                    break;
                case ItemType.Ring:
                case ItemType.Amulet:
                case ItemType.Belt:
                    _accessories.Add(item);
                    break;
                case ItemType.Consumable:
                    _consumables.Add(item);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Cache the LoadImage method from ImageConversion (only search once!)
    /// </summary>
    private static void EnsureLoadImageMethod()
    {
        if (_loadImageMethodSearched) return;
        _loadImageMethodSearched = true;
        
        // Find ImageConversion assembly - Unity 6 uses separate module
        foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name == "UnityEngine.ImageConversionModule")
            {
                Type imageConversionType = asm.GetType("UnityEngine.ImageConversion");
                if (imageConversionType != null)
                {
                    _loadImageMethod = imageConversionType.GetMethod("LoadImage", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                        null,
                        new Type[] { typeof(Texture2D), typeof(byte[]) },
                        null);
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// LAZY LOADING: Only loads sprite when actually needed (not during initialization!)
    /// This is now a NO-OP during item creation - sprites are loaded on-demand via EnsureSprite()
    /// </summary>
    private static void LoadItemSprite(RPGItem item)
    {
        // NO-OP: Sprites are now loaded lazily via EnsureSprite() when actually displayed
        // This prevents loading 223 sprites when only 6 are shown in merchant!
    }
    
    /// <summary>
    /// Ensure an item has its sprite loaded (lazy loading)
    /// Call this when you need to display the item's sprite
    /// </summary>
    public static void EnsureSprite(RPGItem item)
    {
        if (item == null) return;
        
        // Already has sprite? Skip
        if (item.sprite != null) return;
        
        // No image path? Skip
        if (string.IsNullOrEmpty(item.imagePath))
        {
            RPGLog.Warning("[ItemDatabase] EnsureSprite: No imagePath for item " + item.name);
            return;
        }
        
        if (string.IsNullOrEmpty(_modPath))
        {
            RPGLog.Warning("[ItemDatabase] EnsureSprite: _modPath is empty! Cannot load sprite for " + item.name);
            return;
        }
        
        // Initialize sprite cache if needed
        if (_spriteCache == null)
            _spriteCache = new Dictionary<string, Sprite>();
        
        // Check sprite cache first (avoids disk I/O)
        Sprite cachedSprite;
        if (_spriteCache.TryGetValue(item.imagePath, out cachedSprite))
        {
            item.sprite = cachedSprite;
            return;
        }
            
        string fullPath = Path.Combine(_modPath, "images", item.imagePath);
        if (!File.Exists(fullPath))
        {
            RPGLog.Warning("[ItemDatabase] Sprite file not found: " + fullPath);
            return;
        }
        
        try
        {
            // Ensure we have the LoadImage method cached
            EnsureLoadImageMethod();
            
            if (_loadImageMethod == null)
            {
                RPGLog.Warning("[ItemDatabase] LoadImage method not found! Cannot load sprites.");
                return;
            }
            
            byte[] data = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2);
            _loadImageMethod.Invoke(null, new object[] { tex, data });
            
            // Verify texture loaded correctly
            if (tex.width <= 2 || tex.height <= 2)
            {
                RPGLog.Warning("[ItemDatabase] Texture load failed (size 2x2): " + item.imagePath);
                return;
            }
            
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            item.sprite = sprite;
            
            // Cache the sprite for future use
            _spriteCache[item.imagePath] = sprite;
        }
        catch (Exception e)
        {
            RPGLog.Warning("[ItemDatabase] Failed to load sprite: " + item.imagePath + " - " + e.Message);
        }
    }
    
    private static void LoadAllItems()
    {
        _allItems = new List<RPGItem>();
        RPGItem item;
        

        // ============================================================
        // Aurena Weapons (18 items)
        // ============================================================

        item = new RPGItem(1, "Initiate's Claws", "Simple claws given to new members of the Lunar Arcanum Society. Aurena kept hers even after her expulsion.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Aurena Weapons/common.png";
        item.attackBonus = 3;
        item.abilityPowerBonus = 1;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(2, "Scholar's Talons", "Claws sharpened on forbidden texts. The scratches they leave whisper secrets.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Aurena Weapons/common2.png";
        item.attackBonus = 4;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(3, "Moonlit Claws", "Claws that glow faintly under moonlight. They channel lunar energy into devastating strikes.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Aurena Weapons/common3.png";
        item.attackBonus = 4;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(4, "Arcanum Razors", "Claws inscribed with healing runes - now used to tear rather than mend.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Aurena Weapons/common4.png";
        item.attackBonus = 2;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(5, "Exile's Claws", "Forged in secret after her banishment. Each slash is a reminder of what the Society took from her.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Aurena Weapons/common5.png";
        item.attackBonus = 5;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(6, "Heretic's Grasp", "Claws that once belonged to another expelled sage. Their spirit guides Aurena's strikes.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Aurena Weapons/common6.png";
        item.attackBonus = 3;
        item.abilityPowerBonus = 1;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(7, "Sacrificial Claws", "Claws that grow sharper as Aurena's life force wanes. The Society deemed this research too dangerous.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Aurena Weapons/rare1.png";
        item.attackBonus = 8;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(8, "Frostbound Claws", "Claws frozen with ice from the Society's deepest vault. Their touch numbs both flesh and spirit.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Aurena Weapons/rare2iceelement.png";
        item.attackBonus = 7;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Aurena";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(9, "Lunar Rippers", "Enhanced with forbidden enchantments. Each wound they inflict heals Aurena's allies.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Aurena Weapons/rare3.png";
        item.attackBonus = 10;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(10, "Radiant Claws", "Claws blessed by stolen sunlight. Aurena's research into light magic angered both sun and moon cults.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Aurena Weapons/rare4lightelement.png";
        item.attackBonus = 8;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Aurena";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(11, "Ember Claws", "Claws set ablaze with life force extracted from willing dreamers. The flames never die.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Aurena Weapons/rare5fireelement.png";
        item.attackBonus = 9;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Aurena";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(12, "Umbral Talons", "Claws dipped in the essence of nightmares. Aurena learned that darkness heals what light cannot.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Aurena Weapons/rare6darkelement.png";
        item.attackBonus = 8;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Aurena";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(13, "Glacial Reapers", "Claws carved from eternal ice. They freeze the blood of enemies and preserve Aurena's life force.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Aurena Weapons/epic1iceelement.png";
        item.attackBonus = 15;
        item.abilityPowerBonus = 5;
        item.requiredHero = "Hero_Aurena";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(14, "Phoenix Talons", "Claws forged in phoenix fire and quenched in Aurena's own blood. They burn with undying fury.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Aurena Weapons/epic2fireelement.png";
        item.attackBonus = 17;
        item.abilityPowerBonus = 4;
        item.requiredHero = "Hero_Aurena";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(15, "Void Rippers", "Claws that tear through reality itself. The Society fears what Aurena might discover next.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Aurena Weapons/epic3darkelement.png";
        item.attackBonus = 14;
        item.abilityPowerBonus = 6;
        item.requiredHero = "Hero_Aurena";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(16, "Celestial Claws", "Claws infused with pure starlight. Each strike is a prayer to knowledge the gods wanted hidden.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Aurena Weapons/epic4lightelement.png";
        item.attackBonus = 13;
        item.abilityPowerBonus = 7;
        item.requiredHero = "Hero_Aurena";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(17, "Grand Sage's Claws", "The ceremonial claws of the Society's Grand Sage. Aurena took them as proof of their hypocrisy.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Aurena Weapons/epic5.png";
        item.attackBonus = 20;
        item.abilityPowerBonus = 6;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(18, "Life Weaver's Claws", "Aurena's masterpiece - claws that steal life force without destroying it. The Society called it heresy.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Aurena Weapons/epic6.png";
        item.attackBonus = 17;
        item.abilityPowerBonus = 6;
        item.requiredHero = "Hero_Aurena";
        item.lifeSteal = 3.5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Bismuth Weapons (18 items)
        // ============================================================

        item = new RPGItem(19, "Blank Grimoire", "An empty spellbook waiting to be filled. Like Bismuth herself, it holds infinite potential within its pages.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Bismuth weapons/common1.png";
        item.attackBonus = 4;
        item.abilityPowerBonus = 1;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(20, "Apprentice's Tome", "A beginner's spellbook. The blind girl who became Bismuth once traced its pages with her fingers, dreaming of magic.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Bismuth weapons/common2.png";
        item.attackBonus = 5;
        item.abilityPowerBonus = 1;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(21, "Crystalline Codex", "A tome with pages of thin gemstone. It resonates with Bismuth's crystalline nature.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Bismuth weapons/common3.png";
        item.attackBonus = 3;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(22, "Wanderer's Journal", "A travel journal filled with observations the blind girl could never see but somehow always knew.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Bismuth weapons/common4.png";
        item.attackBonus = 6;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(23, "Gemstone Primer", "A basic guide to crystal magic. Its words shimmer like Bismuth's iridescent form.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Bismuth weapons/common5.png";
        item.attackBonus = 4;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(24, "Sightless Scripture", "A tome written in raised letters for those who cannot see. Bismuth reads it through touch and memory.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Bismuth weapons/common6.png";
        item.attackBonus = 5;
        item.abilityPowerBonus = 1;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(25, "Tome of Ember Words", "A book whose pages burn eternally. The flames speak to Bismuth in colors she feels rather than sees.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Bismuth weapons/rare1fire.png";
        item.attackBonus = 10;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Bismuth";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(26, "Frozen Lexicon", "A tome encased in eternal ice. Its cold pages preserve knowledge from before the dreams began.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Bismuth weapons/rare2ice.png";
        item.attackBonus = 9;
        item.abilityPowerBonus = 4;
        item.requiredHero = "Hero_Bismuth";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(27, "Radiant Manuscript", "A book that glows with inner light. It guided the blind girl through darkness before she became Bismuth.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Bismuth weapons/rare3light.png";
        item.attackBonus = 8;
        item.abilityPowerBonus = 4;
        item.requiredHero = "Hero_Bismuth";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(28, "Grimoire of Shadows", "A tome that absorbs light. Its dark pages contain secrets even Bismuth fears to read.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Bismuth weapons/rare4dark.png";
        item.attackBonus = 11;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Bismuth";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(29, "Living Spellbook", "A sentient tome that chose to merge with the blind girl. Together, they became something greater.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Bismuth weapons/rare5.png";
        item.attackBonus = 12;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(30, "Prismatic Codex", "A book with pages of rainbow crystal. Each chapter shifts color as Bismuth channels her power.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Bismuth weapons/rare6.png";
        item.attackBonus = 10;
        item.abilityPowerBonus = 4;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(31, "Void Chronicle", "A tome that contains the memories of those lost to darkness. Bismuth reads their stories to honor them.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Bismuth weapons/epic1dark.png";
        item.attackBonus = 18;
        item.abilityPowerBonus = 7;
        item.requiredHero = "Hero_Bismuth";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(32, "Gemheart Grimoire", "The original spellbook that fused with the blind girl. Its pages pulse with crystalline life.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Bismuth weapons/epic2.png";
        item.attackBonus = 22;
        item.abilityPowerBonus = 6;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(33, "Tome of Blinding Light", "A book so radiant it would blind those who can see. For Bismuth, it simply feels like warmth.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Bismuth weapons/epic3light.png";
        item.attackBonus = 16;
        item.abilityPowerBonus = 8;
        item.requiredHero = "Hero_Bismuth";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(34, "Frostbound Almanac", "An ancient tome frozen in time. Its prophecies speak of a gemstone that walks like a girl.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Bismuth weapons/epic4cold.png";
        item.attackBonus = 17;
        item.abilityPowerBonus = 8;
        item.requiredHero = "Hero_Bismuth";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(35, "Nameless Codex", "The book had no name, just like the girl had no sight. Together, they found their identity.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Bismuth weapons/epic5.png";
        item.attackBonus = 24;
        item.abilityPowerBonus = 6;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(36, "Inferno Scripture", "A tome that burns with the passion of a dreamer who refused to let blindness define her.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Bismuth weapons/epic6fire.png";
        item.attackBonus = 20;
        item.abilityPowerBonus = 6;
        item.requiredHero = "Hero_Bismuth";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Lacerta Weapons (18 items)
        // ============================================================

        item = new RPGItem(37, "Recruit's Rifle", "Standard issue for new Royal Guard recruits. Lacerta mastered it before his first week of training ended.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Lacerta weapons/common1.png";
        item.attackBonus = 8;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(38, "Hunter's Longarm", "A reliable hunting rifle. Lacerta used one like this to feed his family before joining the Guard.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Lacerta weapons/common2.png";
        item.attackBonus = 7;
        item.criticalChance = 3f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(39, "Patrol Carbine", "Compact and dependable. Perfect for long watches on the kingdom's borders.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Lacerta weapons/common3.png";
        item.attackBonus = 9;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(40, "Marksman's Piece", "A well-balanced rifle for those who value precision over power.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Lacerta weapons/common4.png";
        item.attackBonus = 6;
        item.criticalChance = 5f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(41, "Gunpowder Musket", "Old but reliable. The smell of black powder reminds Lacerta of his first hunt.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Lacerta weapons/common5.png";
        item.attackBonus = 10;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(42, "Scout's Repeater", "A lightweight rifle for reconnaissance missions. Speed and silence over raw power.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Lacerta weapons/common6.png";
        item.attackBonus = 7;
        item.requiredHero = "Hero_Lacerta";
        item.attackSpeed = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(43, "Royal Guard Rifle", "The weapon Lacerta carried during his years of service. Each scratch tells a story.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Lacerta weapons/rare1.png";
        item.attackBonus = 16;
        item.criticalChance = 5f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(44, "Sniper's Pride", "A precision rifle for those who never miss. Lacerta's crimson eyes see what others cannot.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Lacerta weapons/rare2.png";
        item.attackBonus = 14;
        item.criticalChance = 8f;
        item.criticalDamage = 15f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(45, "Incendiary Longrifle", "Loaded with fire-tipped rounds. Enemies burn long after the shot lands.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Lacerta weapons/rare3fire.png";
        item.attackBonus = 15;
        item.requiredHero = "Hero_Lacerta";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(46, "Frostbite Carbine", "Fires rounds chilled to absolute zero. Targets slow to a crawl before the killing shot.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Lacerta weapons/rare4ice.png";
        item.attackBonus = 14;
        item.requiredHero = "Hero_Lacerta";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(47, "Nightfall Rifle", "A weapon for hunting in darkness. Its shots are silent as shadows.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Lacerta weapons/rare5dark.png";
        item.attackBonus = 16;
        item.requiredHero = "Hero_Lacerta";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(48, "Dawnbreaker Musket", "Blessed by the morning light. Its shots pierce through darkness and deception.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Lacerta weapons/rare6light.png";
        item.attackBonus = 15;
        item.requiredHero = "Hero_Lacerta";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(49, "Crimson Eye", "Named after Lacerta's legendary gaze. No target escapes this rifle's sight.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Lacerta weapons/epic1.png";
        item.attackBonus = 30;
        item.criticalChance = 12f;
        item.criticalDamage = 25f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(50, "Royal Executioner", "Reserved for the Guard's elite. Lacerta earned this rifle after saving the kingdom.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Lacerta weapons/epic2.png";
        item.attackBonus = 35;
        item.criticalChance = 8f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(51, "Sunfire Cannon", "A rifle that fires concentrated light. Each shot is a miniature sunrise.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Lacerta weapons/epic3light.png";
        item.attackBonus = 28;
        item.requiredHero = "Hero_Lacerta";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(52, "Void Hunter", "A rifle that fires bullets of pure darkness. Targets vanish without a trace.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Lacerta weapons/epic4dark.png";
        item.attackBonus = 26;
        item.criticalDamage = 20f;
        item.requiredHero = "Hero_Lacerta";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(53, "Absolute Zero", "A rifle that freezes time itself. Targets are dead before they hear the shot.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Lacerta weapons/epic5cold.png";
        item.attackBonus = 27;
        item.requiredHero = "Hero_Lacerta";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(54, "Dragon's Breath", "A rifle loaded with explosive incendiary rounds. The kingdom's enemies learned to fear its roar.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Lacerta weapons/epic5fire.png";
        item.attackBonus = 32;
        item.requiredHero = "Hero_Lacerta";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Mist Weapons (16 items)
        // ============================================================

        item = new RPGItem(55, "Fencing Foil", "A training rapier from Mist's youth. Even then, she outmatched every instructor in Astrid.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Mist Weapons/common1.png";
        item.attackBonus = 6;
        item.criticalChance = 4f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(56, "Noble's Rapier", "A rapier befitting Mist's noble heritage. Light, elegant, and deadly.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Mist Weapons/common2.png";
        item.attackBonus = 7;
        item.criticalChance = 3f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(57, "Duelist's Rapier", "A rapier designed for one-on-one combat. Mist has never lost a duel.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Mist Weapons/common3.png";
        item.attackBonus = 8;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(58, "Swift Rapier", "A lightweight rapier that moves like an extension of Mist's arm.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Mist Weapons/common4.png";
        item.attackBonus = 5;
        item.requiredHero = "Hero_Mist";
        item.attackSpeed = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(59, "Astrid Rapier", "Forged in the fires of Astrid's finest smithy. A symbol of the kingdom's craftsmanship.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Mist Weapons/common5.png";
        item.attackBonus = 9;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(60, "Dancer's Rapier", "A rapier for those who see combat as an art. Mist's movements are poetry in motion.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Mist Weapons/common6.png";
        item.attackBonus = 6;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(61, "Blazing Rapier", "A rapier wreathed in flames. Mist's fury burns as hot as her determination.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Mist Weapons/rare1fire.png";
        item.attackBonus = 14;
        item.criticalChance = 5f;
        item.requiredHero = "Hero_Mist";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(62, "Shadow Rapier", "A rapier that strikes from darkness. Enemies fall before they see Mist move.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Mist Weapons/rare2dark.png";
        item.attackBonus = 15;
        item.criticalChance = 6f;
        item.requiredHero = "Hero_Mist";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(63, "Frostbite Rapier", "A rapier of frozen steel. Its touch numbs both body and soul.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Mist Weapons/rare3ice.png";
        item.attackBonus = 13;
        item.criticalChance = 4f;
        item.requiredHero = "Hero_Mist";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(64, "Camilla's Grace", "A gift from someone dear to Mist. She fights to honor their memory.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Mist Weapons/rare4.png";
        item.attackBonus = 14;
        item.criticalChance = 8f;
        item.criticalDamage = 10f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(65, "Parrying Rapier", "A rapier designed for defense as much as offense. Mist turns every attack into an opportunity.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Mist Weapons/rare5.png";
        item.attackBonus = 12;
        item.defenseBonus = 8;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(66, "Radiant Rapier", "A rapier that gleams with inner light. It pierces through lies and shadows alike.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Mist Weapons/rare6light.png";
        item.attackBonus = 14;
        item.criticalChance = 5f;
        item.requiredHero = "Hero_Mist";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(67, "Inferno Rapier", "A rapier that burns with undying passion. Mist's determination cannot be extinguished.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Mist Weapons/epic1fire.png";
        item.attackBonus = 26;
        item.criticalChance = 10f;
        item.requiredHero = "Hero_Mist";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(68, "Midnight Rapier", "A rapier forged in absolute darkness. It strikes with the silence of death.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Mist Weapons/epic2dark.png";
        item.attackBonus = 28;
        item.criticalChance = 12f;
        item.criticalDamage = 20f;
        item.requiredHero = "Hero_Mist";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(69, "Winter's Rapier", "A rapier carved from eternal ice. Its cold is matched only by Mist's focus in battle.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Mist Weapons/epic3ice.png";
        item.attackBonus = 25;
        item.criticalChance = 8f;
        item.requiredHero = "Hero_Mist";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(70, "Unyielding Rapier", "The legendary rapier of Astrid's greatest duelist. Mist earned this title through countless victories.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Mist Weapons/epic6light.png";
        item.attackBonus = 30;
        item.criticalChance = 15f;
        item.criticalDamage = 25f;
        item.requiredHero = "Hero_Mist";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Nachia Weapons (18 items)
        // ============================================================

        item = new RPGItem(71, "Sapling Staff", "A young branch from the Ethereal Forest. Even saplings answer Nachia's call.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Nachia Weapons/common1.png";
        item.attackBonus = 3;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(72, "Spirit Caller", "A staff carved to channel the voices of forest spirits. They whisper secrets to Nachia.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Nachia Weapons/common2.png";
        item.attackBonus = 2;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(73, "Sylvan Branch", "A living branch that still grows. The forest's magic flows through it.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Nachia Weapons/common3.png";
        item.attackBonus = 4;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(74, "Wildling Rod", "Untamed and unpredictable, like Nachia herself. The spirits love its chaotic energy.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Nachia Weapons/common4.png";
        item.attackBonus = 5;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(75, "Guardian's Staff", "Carried by those who protect the forest. Nachia inherited this duty at birth.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Nachia Weapons/common5.png";
        item.attackBonus = 3;
        item.healthBonus = 15;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(76, "Ethereal Wand", "A wand that bridges the material and spirit worlds. Nachia walks between both.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Nachia Weapons/common6.png";
        item.attackBonus = 2;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(77, "Frostwood Staff", "A staff frozen in eternal winter. The cold spirits of the north heed its call.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Nachia Weapons/rare1icepng.png";
        item.attackBonus = 8;
        item.abilityPowerBonus = 5;
        item.requiredHero = "Hero_Nachia";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(78, "Shadowroot Staff", "Grown in the darkest depths of the forest. Shadow spirits dance around it.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Nachia Weapons/rare2dark.png";
        item.attackBonus = 9;
        item.abilityPowerBonus = 4;
        item.requiredHero = "Hero_Nachia";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(79, "Emberwood Staff", "A staff that never stops smoldering. Fire spirits are drawn to its warmth.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Nachia Weapons/rare3fire.png";
        item.attackBonus = 10;
        item.abilityPowerBonus = 4;
        item.requiredHero = "Hero_Nachia";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(80, "Sunblessed Staff", "Blessed by the spirits of dawn. Its light guides lost souls home.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Nachia Weapons/rare4light.png";
        item.attackBonus = 7;
        item.abilityPowerBonus = 5;
        item.requiredHero = "Hero_Nachia";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(81, "Fenrir's Fang", "A staff topped with a spirit wolf's fang. Nachia's loyal companion guides its power.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Nachia Weapons/rare5.png";
        item.attackBonus = 12;
        item.abilityPowerBonus = 4;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(82, "Ancient Oak Staff", "Carved from a thousand-year oak. The oldest spirits remember when it was planted.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Nachia Weapons/rare6.png";
        item.attackBonus = 8;
        item.healthBonus = 30;
        item.abilityPowerBonus = 6;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(83, "World Tree Branch", "A branch from the legendary World Tree. All forest spirits bow before it.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Nachia Weapons/epic1.png";
        item.attackBonus = 18;
        item.abilityPowerBonus = 9;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(84, "Spirit King's Scepter", "The staff of the Spirit King himself. Nachia earned it by protecting the forest.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Nachia Weapons/epic2.png";
        item.attackBonus = 16;
        item.healthBonus = 50;
        item.abilityPowerBonus = 10;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(85, "Permafrost Staff", "A staff of eternal ice from the frozen heart of the forest. Winter spirits obey its command.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Nachia Weapons/epic3ice.png";
        item.attackBonus = 15;
        item.abilityPowerBonus = 8;
        item.requiredHero = "Hero_Nachia";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(86, "Phoenix Perch", "A staff where fire spirits rest. Its flames bring rebirth, not destruction.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Nachia Weapons/epic4fire.png";
        item.attackBonus = 17;
        item.abilityPowerBonus = 8;
        item.requiredHero = "Hero_Nachia";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(87, "Radiant Grove Staff", "A staff that glows with the light of a thousand fireflies. Hope spirits dance within.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Nachia Weapons/epic5light.png";
        item.attackBonus = 14;
        item.abilityPowerBonus = 9;
        item.requiredHero = "Hero_Nachia";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(88, "Void Warden's Staff", "A staff that guards the boundary between worlds. Shadow spirits protect its wielder.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Nachia Weapons/epic6dark.png";
        item.attackBonus = 16;
        item.abilityPowerBonus = 8;
        item.requiredHero = "Hero_Nachia";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Shell Weapons (18 items)
        // ============================================================

        item = new RPGItem(89, "Utility Blade", "A simple tool for a simple purpose. Shell sees no difference between cutting rope and cutting throats.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Shell Weapons/common1.png";
        item.attackBonus = 5;
        item.criticalChance = 5f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(90, "Silent Edge", "A blade designed to make no sound. Shell's victims never hear death coming.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Shell Weapons/common2.png";
        item.attackBonus = 4;
        item.criticalChance = 6f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(91, "Assassin's Needle", "Thin and precise. One strike to the right spot is all Shell needs.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Shell Weapons/common3.png";
        item.attackBonus = 4;
        item.criticalChance = 8f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(92, "Culling Knife", "Efficient. Practical. Emotionless. Like Shell itself.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Shell Weapons/common4.png";
        item.attackBonus = 6;
        item.criticalChance = 4f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(93, "Pursuit Dagger", "A blade for the hunt. Shell never stops until the target is eliminated.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Shell Weapons/common5.png";
        item.attackBonus = 5;
        item.requiredHero = "Hero_Husk";
        item.attackSpeed = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(94, "Hollow Fang", "Named for the emptiness Shell feels. Or rather, doesn't feel.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Shell Weapons/common6.png";
        item.attackBonus = 4;
        item.criticalChance = 5f;
        item.criticalDamage = 5f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(95, "Executioner's Kiss", "A blade that has ended countless lives. Shell feels nothing for any of them.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Shell Weapons/rare1.png";
        item.attackBonus = 10;
        item.criticalChance = 10f;
        item.criticalDamage = 15f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(96, "Weakness Finder", "A blade attuned to exploit vulnerabilities. Shell calculates the optimal strike.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Shell Weapons/rare2.png";
        item.attackBonus = 8;
        item.criticalChance = 12f;
        item.criticalDamage = 20f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(97, "Frostbite Shiv", "A blade of frozen malice. Its cold matches Shell's empty heart.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Shell Weapons/rare3ice.png";
        item.attackBonus = 9;
        item.criticalChance = 8f;
        item.requiredHero = "Hero_Husk";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(98, "Purifying Stiletto", "A blade of light for dark work. The irony is lost on Shell.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Shell Weapons/rare4light.png";
        item.attackBonus = 8;
        item.criticalChance = 9f;
        item.requiredHero = "Hero_Husk";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(99, "Umbral Kris", "A blade that drinks in shadows. Shell moves unseen, strikes without warning.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Shell Weapons/rare5dark.png";
        item.attackBonus = 10;
        item.criticalChance = 10f;
        item.requiredHero = "Hero_Husk";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(100, "Searing Dirk", "A blade heated by inner fire. Shell's targets burn before they bleed.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Shell Weapons/rare6fire.png";
        item.attackBonus = 11;
        item.criticalChance = 7f;
        item.requiredHero = "Hero_Husk";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(101, "Perfect Instrument", "The ultimate tool for the ultimate killer. Shell was made for this blade.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Shell Weapons/epic1.png";
        item.attackBonus = 20;
        item.criticalChance = 18f;
        item.criticalDamage = 35f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(102, "Relentless Edge", "A blade that never dulls, wielded by a pursuer who never stops.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Shell Weapons/epic2.png";
        item.attackBonus = 21;
        item.criticalChance = 15f;
        item.criticalDamage = 30f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(103, "Infernal Assassin", "A blade forged in hellfire. It burns with the intensity of Shell's singular purpose.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Shell Weapons/epic3fire.png";
        item.attackBonus = 18;
        item.criticalChance = 14f;
        item.requiredHero = "Hero_Husk";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(104, "Void Fang", "A blade of absolute darkness. Like Shell, it exists only to end things.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Shell Weapons/epic4dark.png";
        item.attackBonus = 17;
        item.criticalChance = 16f;
        item.criticalDamage = 25f;
        item.requiredHero = "Hero_Husk";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(105, "Frozen Verdict", "A blade of eternal ice. Its judgment is as cold and final as Shell's.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Shell Weapons/epic5ice.png";
        item.attackBonus = 17;
        item.criticalChance = 15f;
        item.requiredHero = "Hero_Husk";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(106, "Radiant Terminus", "A blade of blinding light. The last thing Shell's targets ever see.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Shell Weapons/epic6light.png";
        item.attackBonus = 19;
        item.criticalChance = 14f;
        item.requiredHero = "Hero_Husk";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Vesper Weapons (18 items)
        // ============================================================

        item = new RPGItem(107, "Initiate's Mace", "The first weapon given to those who join the Order. Vesper remembers when his was new.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Vesper weaponandshield/commonhammer1.png";
        item.attackBonus = 8;
        item.defenseBonus = 5;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(108, "Judgment Hammer", "A tool of the inquisition. Vesper has delivered many verdicts with its weight.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Vesper weaponandshield/commonhammer2.png";
        item.attackBonus = 9;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(109, "Flame Warden's Cudgel", "Carried by those who guard the Sacred Flame. Its strikes are blessed by El.", ItemType.Weapon, ItemRarity.Common);
        item.imagePath = "Vesper weaponandshield/commonhammer3.png";
        item.attackBonus = 7;
        item.defenseBonus = 8;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(110, "Acolyte's Buckler", "A simple shield bearing the Order's crest. Every paladin starts here.", ItemType.OffHand, ItemRarity.Common);
        item.imagePath = "Vesper weaponandshield/commonshield1.png";
        item.defenseBonus = 8;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(111, "Inquisitor's Guard", "A shield that has blocked heresy and blade alike. Vesper's faith is unshakeable.", ItemType.OffHand, ItemRarity.Common);
        item.imagePath = "Vesper weaponandshield/commonshield2.png";
        item.defenseBonus = 10;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(112, "Twilight Defender", "A shield forged at dusk, when El's light meets the darkness.", ItemType.OffHand, ItemRarity.Common);
        item.imagePath = "Vesper weaponandshield/commonshield3.png";
        item.defenseBonus = 8;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(113, "Zealot's Crusher", "A hammer wielded by the most devoted. Vesper's zeal knows no bounds.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Vesper weaponandshield/rarehammer1.png";
        item.attackBonus = 16;
        item.defenseBonus = 5;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(114, "Purifier's Maul", "A weapon blessed to cleanse corruption. Its weight breaks both armor and spirit.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Vesper weaponandshield/rarehammer2.png";
        item.attackBonus = 14;
        item.defenseBonus = 8;
        item.healthBonus = 25;
        item.requiredHero = "Hero_Vesper";
        item.thorns = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(115, "El's Verdict", "A hammer that carries divine judgment. Vesper delivers El's will without mercy.", ItemType.Weapon, ItemRarity.Rare);
        item.imagePath = "Vesper weaponandshield/rarehammer3.png";
        item.attackBonus = 12;
        item.defenseBonus = 5;
        item.healthBonus = 40;
        item.requiredHero = "Hero_Vesper";
        item.regeneration = 2f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(116, "Bulwark of Faith", "A shield empowered by unwavering belief. Vesper's conviction makes it unbreakable.", ItemType.OffHand, ItemRarity.Rare);
        item.imagePath = "Vesper weaponandshield/rareshield1.png";
        item.defenseBonus = 8;
        item.healthBonus = 40;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(117, "Sacred Flame Aegis", "A shield that burns with holy fire. Heretics recoil at its radiance.", ItemType.OffHand, ItemRarity.Rare);
        item.imagePath = "Vesper weaponandshield/rareshield2.png";
        item.defenseBonus = 5;
        item.healthBonus = 30;
        item.requiredHero = "Hero_Vesper";
        item.thorns = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(118, "Inquisition Rampart", "The shield of a senior inquisitor. Vesper earned this through ruthless service.", ItemType.OffHand, ItemRarity.Rare);
        item.imagePath = "Vesper weaponandshield/rareshield3.png";
        item.defenseBonus = 8;
        item.healthBonus = 25;
        item.requiredHero = "Hero_Vesper";
        item.regeneration = 2f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(119, "Dawnbreaker", "A legendary hammer that shatters darkness. Vesper wields El's fury incarnate.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Vesper weaponandshield/epichammer1.png";
        item.attackBonus = 30;
        item.defenseBonus = 10;
        item.healthBonus = 50;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(120, "Sanctified Annihilator", "A hammer blessed by the highest priests. Its strikes carry the weight of divine wrath.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Vesper weaponandshield/epichammer2.png";
        item.attackBonus = 25;
        item.defenseBonus = 5;
        item.healthBonus = 40;
        item.requiredHero = "Hero_Vesper";
        item.thorns = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(121, "El's Right Hand", "The most sacred weapon of the Order. Vesper is the chosen instrument of El's will.", ItemType.Weapon, ItemRarity.Epic);
        item.imagePath = "Vesper weaponandshield/epichammer3.png";
        item.attackBonus = 22;
        item.defenseBonus = 5;
        item.healthBonus = 70;
        item.requiredHero = "Hero_Vesper";
        item.lifeSteal = 2f;
        item.regeneration = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(122, "Bastion of the Sacred Flame", "The ultimate defense of the Order. El's light shields Vesper from all harm.", ItemType.OffHand, ItemRarity.Epic);
        item.imagePath = "Vesper weaponandshield/epicshield1.png";
        item.defenseBonus = 5;
        item.healthBonus = 80;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(123, "Eternal Vigilance", "A shield that never falters. Like Vesper, it stands watch against the darkness forever.", ItemType.OffHand, ItemRarity.Epic);
        item.imagePath = "Vesper weaponandshield/epicshield2.png";
        item.defenseBonus = 10;
        item.healthBonus = 60;
        item.requiredHero = "Hero_Vesper";
        item.thorns = 10f;
        item.regeneration = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(124, "Twilight Paladin's Oath", "The shield of the Order's leader. Vesper swore an oath that binds his soul to El.", ItemType.OffHand, ItemRarity.Epic);
        item.imagePath = "Vesper weaponandshield/epicshield3.png";
        item.defenseBonus = 12;
        item.healthBonus = 70;
        item.requiredHero = "Hero_Vesper";
        item.lifeSteal = 3.5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Yubar Weapons (18 items)
        // ============================================================

        item = new RPGItem(125, "Nascent Star", "A newborn star, barely formed. Yubar remembers when all stars were this small.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Yubar Weapons/common1.png";
        item.attackBonus = 2;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(126, "Cosmic Ember", "A fragment of stellar fire. It burns with the heat of creation itself.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Yubar Weapons/common2.png";
        item.attackBonus = 3;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(127, "Stardust Sphere", "Compressed stardust waiting to be shaped. Yubar weaves galaxies from such matter.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Yubar Weapons/common3.png";
        item.attackBonus = 1;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(128, "Dream Catalyst", "An orb that amplifies dream energy. Born from Yubar's first experiments.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Yubar Weapons/common4.png";
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Yubar";
        item.memoryHaste = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(129, "Nebula Core", "The heart of a distant nebula. Yubar plucked it from the cosmic tapestry.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Yubar Weapons/common5.png";
        item.attackBonus = 4;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(130, "Celestial Seed", "A seed that will one day become a star. Yubar nurtures countless such seeds.", ItemType.TwoHandedWeapon, ItemRarity.Common);
        item.imagePath = "Yubar Weapons/common6.png";
        item.healthBonus = 15;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(131, "Supernova Fragment", "A piece of an exploded star. Yubar witnessed its death and preserved its power.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Yubar Weapons/rare1.png";
        item.attackBonus = 8;
        item.abilityPowerBonus = 6;
        item.criticalChance = 5f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(132, "Gravity Well", "A sphere of compressed gravity. Space itself bends around it.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Yubar Weapons/rare2.png";
        item.attackBonus = 6;
        item.abilityPowerBonus = 6;
        item.requiredHero = "Hero_Yubar";
        item.memoryHaste = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(133, "Void Singularity", "An orb of pure darkness. It devours light and hope alike.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Yubar Weapons/rare3dark.png";
        item.attackBonus = 10;
        item.abilityPowerBonus = 5;
        item.requiredHero = "Hero_Yubar";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(134, "Solar Corona", "The blazing crown of a sun. Yubar shaped it from pure radiance.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Yubar Weapons/rare4light.png";
        item.attackBonus = 7;
        item.abilityPowerBonus = 6;
        item.requiredHero = "Hero_Yubar";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(135, "Frozen Comet", "A comet trapped in eternal ice. Its tail still streams across the cosmos.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Yubar Weapons/rare5ice.png";
        item.attackBonus = 9;
        item.abilityPowerBonus = 5;
        item.requiredHero = "Hero_Yubar";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(136, "Stellar Furnace", "The core of a dying star. It burns with desperate, beautiful fury.", ItemType.TwoHandedWeapon, ItemRarity.Rare);
        item.imagePath = "Yubar Weapons/rare6fire.png";
        item.attackBonus = 11;
        item.abilityPowerBonus = 4;
        item.requiredHero = "Hero_Yubar";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(137, "Big Bang Remnant", "A fragment from the birth of everything. Yubar was there when it happened.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Yubar Weapons/epic1.png";
        item.attackBonus = 15;
        item.abilityPowerBonus = 11;
        item.criticalChance = 10f;
        item.criticalDamage = 15f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(138, "Cosmic Loom", "The tool Yubar uses to weave reality. Stars are merely threads to him.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Yubar Weapons/epic2.png";
        item.attackBonus = 12;
        item.abilityPowerBonus = 12;
        item.requiredHero = "Hero_Yubar";
        item.memoryHaste = 15f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(139, "Radiant Genesis", "The light of creation itself. Yubar used this to birth the first stars.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Yubar Weapons/epic3light.png";
        item.attackBonus = 14;
        item.abilityPowerBonus = 10;
        item.requiredHero = "Hero_Yubar";
        item.elementalType = ElementalType.Light;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(140, "Entropy's Heart", "The essence of universal decay. All things end, and Yubar decides when.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Yubar Weapons/epic4dark.png";
        item.attackBonus = 18;
        item.abilityPowerBonus = 9;
        item.requiredHero = "Hero_Yubar";
        item.elementalType = ElementalType.Dark;
        item.elementalStacks = 3;
        item.lifeSteal = 3.5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(141, "Absolute Zero", "The coldest point in existence. Even time freezes before it.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Yubar Weapons/epic5ice.png";
        item.attackBonus = 16;
        item.abilityPowerBonus = 10;
        item.requiredHero = "Hero_Yubar";
        item.elementalType = ElementalType.Cold;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(142, "Cataclysm Core", "The heart of universal destruction. Yubar unleashes this only in dire need.", ItemType.TwoHandedWeapon, ItemRarity.Epic);
        item.imagePath = "Yubar Weapons/epic6fire.png";
        item.attackBonus = 20;
        item.abilityPowerBonus = 8;
        item.criticalDamage = 20f;
        item.requiredHero = "Hero_Yubar";
        item.elementalType = ElementalType.Fire;
        item.elementalStacks = 3;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Rings (20 items)
        // ============================================================

        item = new RPGItem(143, "Dreamer's Band", "A simple ring worn by those who first enter the dream world. It pulses with faint hope.", ItemType.Ring, ItemRarity.Common);
        item.imagePath = "rings/ring_001.png";
        item.healthBonus = 15;
        item.abilityPowerBonus = 1;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(144, "Stardust Circle", "Forged from condensed stardust. Dreamers say it glows brighter in moments of danger.", ItemType.Ring, ItemRarity.Common);
        item.imagePath = "rings/ring_002.png";
        item.attackBonus = 4;
        item.criticalChance = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(145, "Slumbering Signet", "Bears the mark of peaceful sleep. Its wearer recovers faster from wounds.", ItemType.Ring, ItemRarity.Common);
        item.imagePath = "rings/ring_003.png";
        item.healthBonus = 20;
        item.regeneration = 1f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(146, "Wisp's Loop", "A ring that captures wandering wisps. Their light guides the way through dark dreams.", ItemType.Ring, ItemRarity.Common);
        item.imagePath = "rings/ring_004.png";
        item.abilityPowerBonus = 2;
        item.memoryHaste = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(147, "Vagrant's Ring", "Worn by wanderers of the dream realm. It has seen countless forgotten paths.", ItemType.Ring, ItemRarity.Common);
        item.imagePath = "rings/ring_005.png";
        item.defenseBonus = 8;
        item.moveSpeedBonus = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(148, "Nightmare Shard Ring", "Contains a fragment of a defeated nightmare. Its darkness empowers the bold.", ItemType.Ring, ItemRarity.Common);
        item.imagePath = "rings/ring_006.png";
        item.attackBonus = 5;
        item.criticalDamage = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(149, "Memory Keeper", "A ring that holds precious memories. Smoothie sells similar ones at his shop.", ItemType.Ring, ItemRarity.Common);
        item.imagePath = "rings/ring_007.png";
        item.healthBonus = 10;
        item.abilityPowerBonus = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(150, "Lunar Arcanum Band", "A ring from the Lunar Arcanum Society. Aurena once wore one like it before her exile.", ItemType.Ring, ItemRarity.Rare);
        item.imagePath = "rings/ring_008.png";
        item.healthBonus = 25;
        item.abilityPowerBonus = 5;
        item.regeneration = 2f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(151, "Ethereal Forest Seal", "A ring blessed by the spirits of Nachia's forest. Nature's power flows through it.", ItemType.Ring, ItemRarity.Rare);
        item.imagePath = "rings/ring_009.png";
        item.healthBonus = 20;
        item.abilityPowerBonus = 4;
        item.memoryHaste = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(152, "Royal Guard Insignia", "A ring worn by Lacerta during his service. It still carries the weight of duty.", ItemType.Ring, ItemRarity.Rare);
        item.imagePath = "rings/ring_010.png";
        item.attackBonus = 12;
        item.criticalChance = 8f;
        item.criticalDamage = 12f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(153, "Duelist's Honor", "A ring passed down among noble duelists. Mist's family treasured rings like this.", ItemType.Ring, ItemRarity.Rare);
        item.imagePath = "rings/ring_011.png";
        item.attackBonus = 10;
        item.dodgeChargesBonus = 1;
        item.attackSpeed = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(154, "Sacred Flame Ember", "A ring containing a spark of the Sacred Flame. Vesper's Order guards these jealously.", ItemType.Ring, ItemRarity.Rare);
        item.imagePath = "rings/ring_012.png";
        item.defenseBonus = 5;
        item.healthBonus = 40;
        item.thorns = 4f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(155, "Spellbook Fragment", "A ring made from Bismuth's crystallized magic. It hums with arcane energy.", ItemType.Ring, ItemRarity.Rare);
        item.imagePath = "rings/ring_013.png";
        item.attackBonus = 6;
        item.abilityPowerBonus = 6;
        item.memoryHaste = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(156, "Pursuer's Mark", "A ring that tracks prey. Shell's handlers used these to monitor their instrument.", ItemType.Ring, ItemRarity.Rare);
        item.imagePath = "rings/ring_014.png";
        item.attackBonus = 14;
        item.criticalChance = 10f;
        item.attackSpeed = 6f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(157, "El's Blessing", "A ring blessed by El itself. Its light banishes nightmares and heals the wounded.", ItemType.Ring, ItemRarity.Epic);
        item.imagePath = "rings/ring_015.png";
        item.defenseBonus = 5;
        item.healthBonus = 60;
        item.regeneration = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(158, "Nightmare Lord's Seal", "Taken from a defeated nightmare lord. Its dark power corrupts and empowers.", ItemType.Ring, ItemRarity.Epic);
        item.imagePath = "rings/ring_016.png";
        item.attackBonus = 20;
        item.criticalChance = 15f;
        item.criticalDamage = 25f;
        item.lifeSteal = 3.5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(159, "Astrid's Legacy", "A ring from the noble house of Astrid. Mist's ancestors wore it into countless duels.", ItemType.Ring, ItemRarity.Epic);
        item.imagePath = "rings/ring_017.png";
        item.attackBonus = 18;
        item.moveSpeedBonus = 10f;
        item.dodgeChargesBonus = 2;
        item.attackSpeed = 12f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(160, "Dreamseer's Eye", "A ring that sees through the veil of dreams. Past, present, and future blur together.", ItemType.Ring, ItemRarity.Epic);
        item.imagePath = "rings/ring_018.png";
        item.healthBonus = 40;
        item.abilityPowerBonus = 10;
        item.memoryHaste = 15f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(161, "Primordial Dream Ring", "Forged at the dawn of dreams. It contains the essence of the first dreamer.", ItemType.Ring, ItemRarity.Epic);
        item.imagePath = "rings/ring_019.png";
        item.attackBonus = 15;
        item.healthBonus = 50;
        item.abilityPowerBonus = 6;
        item.criticalChance = 10f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(162, "Eternal Slumber", "A ring that touches the deepest sleep. Those who wear it fear neither death nor waking.", ItemType.Ring, ItemRarity.Epic);
        item.imagePath = "rings/ring_020.png";
        item.defenseBonus = 8;
        item.healthBonus = 80;
        item.thorns = 6f;
        item.regeneration = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Amulets (21 items)
        // ============================================================

        item = new RPGItem(163, "Dreamer's Pendant", "A simple charm given to new dreamers. It glows faintly when hope is near.", ItemType.Amulet, ItemRarity.Common);
        item.imagePath = "amulets/amulet_001.png";
        item.healthBonus = 15;
        item.abilityPowerBonus = 1;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(164, "Flame Touched Locket", "A locket warmed by distant fire. It reminds the wearer of the Sacred Flame.", ItemType.Amulet, ItemRarity.Common);
        item.imagePath = "amulets/amulet_002.png";
        item.attackBonus = 3;
        item.healthBonus = 12;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(165, "Crimson Teardrop", "A blood-red gem said to contain a dreamer's sacrifice. It pulses with life.", ItemType.Amulet, ItemRarity.Common);
        item.imagePath = "amulets/amulet_003.png";
        item.healthBonus = 20;
        item.regeneration = 1f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(166, "Amber Charm", "Preserved dream essence trapped in amber. Time moves differently within.", ItemType.Amulet, ItemRarity.Common);
        item.imagePath = "amulets/amulet_004.png";
        item.healthBonus = 10;
        item.memoryHaste = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(167, "Twilight Medallion", "A medallion that shimmers between light and dark. Vesper's initiates wear these.", ItemType.Amulet, ItemRarity.Common);
        item.imagePath = "amulets/amulet_005.png";
        item.defenseBonus = 5;
        item.healthBonus = 15;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(168, "Stardust Choker", "A necklace dusted with crystallized stars. Smoothie collects these from fallen dreams.", ItemType.Amulet, ItemRarity.Common);
        item.imagePath = "amulets/amulet_006.png";
        item.healthBonus = 8;
        item.abilityPowerBonus = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(169, "Emerald Eye", "A green gem that sees through illusions. Nightmares cannot hide from its gaze.", ItemType.Amulet, ItemRarity.Common);
        item.imagePath = "amulets/amulet_007.png";
        item.healthBonus = 12;
        item.criticalChance = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(170, "Forest Spirit's Heart", "A gem containing a forest spirit's blessing. Nachia's guardians cherish these.", ItemType.Amulet, ItemRarity.Rare);
        item.imagePath = "amulets/amulet_008.png";
        item.healthBonus = 30;
        item.abilityPowerBonus = 4;
        item.regeneration = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(171, "Lunar Arcanum Sigil", "A sigil from the forbidden archives. Aurena risked everything to study these.", ItemType.Amulet, ItemRarity.Rare);
        item.imagePath = "amulets/amulet_009.png";
        item.healthBonus = 25;
        item.abilityPowerBonus = 5;
        item.memoryHaste = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(172, "Bismuth Crystal", "A crystallized fragment of Bismuth's magic. It resonates with arcane energy.", ItemType.Amulet, ItemRarity.Rare);
        item.imagePath = "amulets/amulet_010.png";
        item.attackBonus = 5;
        item.healthBonus = 20;
        item.abilityPowerBonus = 6;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(173, "Sacred Flame Ember", "A contained spark of the Sacred Flame. Vesper's Order guards these relics.", ItemType.Amulet, ItemRarity.Rare);
        item.imagePath = "amulets/amulet_011.png";
        item.defenseBonus = 8;
        item.healthBonus = 40;
        item.thorns = 4f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(174, "Nightmare Fang Pendant", "A fang from a powerful nightmare, now a trophy. Shell wears one as a reminder.", ItemType.Amulet, ItemRarity.Rare);
        item.imagePath = "amulets/amulet_012.png";
        item.attackBonus = 10;
        item.healthBonus = 25;
        item.criticalChance = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(175, "Royal Guard Medal", "A medal of honor from the Royal Guard. Lacerta earned many before his exile.", ItemType.Amulet, ItemRarity.Rare);
        item.imagePath = "amulets/amulet_013.png";
        item.attackBonus = 8;
        item.healthBonus = 30;
        item.attackSpeed = 6f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(176, "Astrid Crest", "The noble crest of House Astrid. Mist's family legacy hangs from this chain.", ItemType.Amulet, ItemRarity.Rare);
        item.imagePath = "amulets/amulet_014.png";
        item.attackBonus = 7;
        item.healthBonus = 25;
        item.dodgeChargesBonus = 1;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(177, "El's Divine Tear", "A tear shed by El itself. Its light protects against the darkest nightmares.", ItemType.Amulet, ItemRarity.Epic);
        item.imagePath = "amulets/amulet_015.png";
        item.defenseBonus = 5;
        item.healthBonus = 70;
        item.regeneration = 6f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(178, "Primordial Emerald", "A gem from the first forest. It contains the dreams of ancient spirits.", ItemType.Amulet, ItemRarity.Epic);
        item.imagePath = "amulets/amulet_016.png";
        item.healthBonus = 50;
        item.abilityPowerBonus = 8;
        item.regeneration = 4f;
        item.memoryHaste = 12f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(179, "Nightmare Lord's Eye", "The eye of a defeated nightmare lord. It sees all fears and exploits them.", ItemType.Amulet, ItemRarity.Epic);
        item.imagePath = "amulets/amulet_017.png";
        item.attackBonus = 18;
        item.healthBonus = 40;
        item.criticalChance = 12f;
        item.criticalDamage = 20f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(180, "Heart of the Sacred Flame", "The core of the Sacred Flame itself. Only the most devoted may wear it.", ItemType.Amulet, ItemRarity.Epic);
        item.imagePath = "amulets/amulet_018.png";
        item.defenseBonus = 5;
        item.healthBonus = 100;
        item.thorns = 10f;
        item.regeneration = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(181, "Dreamseer's Oracle", "An amulet that glimpses all possible futures. Reality bends to its predictions.", ItemType.Amulet, ItemRarity.Epic);
        item.imagePath = "amulets/amulet_019.png";
        item.healthBonus = 60;
        item.abilityPowerBonus = 10;
        item.criticalChance = 8f;
        item.memoryHaste = 15f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(182, "Void Walker's Compass", "An amulet that points toward the void. Those who follow it never return the same.", ItemType.Amulet, ItemRarity.Epic);
        item.imagePath = "amulets/amulet_020.png";
        item.attackBonus = 15;
        item.healthBonus = 50;
        item.lifeSteal = 5.5f;
        item.attackSpeed = 15f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(183, "Midas Amulet", "A legendary amulet touched by the golden dream. Enemies slain drop additional gold (1-5 base, scales with upgrade level and monster strength).", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/midasamulet.png";
        item.attackBonus = 20;
        item.healthBonus = 80;
        item.criticalChance = 10f;
        item.goldOnKill = 1;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(478, "Stardust Collector", "A legendary amulet that gathers crystallized stardust from fallen enemies. Each kill yields precious dream essence (1-5 base, scales with upgrade level and monster strength).", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_031.png";
        item.abilityPowerBonus = 18;
        item.healthBonus = 90;
        item.criticalChance = 12f;
        item.memoryHaste = 20f;
        item.dustOnKill = 1;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Belts (20 items)
        // ============================================================

        item = new RPGItem(184, "Wanderer's Sash", "A simple cloth sash worn by those who drift between dreams. It keeps the soul anchored.", ItemType.Belt, ItemRarity.Common);
        item.imagePath = "belts/belt_001.png";
        item.defenseBonus = 1;
        item.healthBonus = 15;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(185, "Dreamer's Cord", "A rope belt woven from dream threads. It pulses gently with each heartbeat.", ItemType.Belt, ItemRarity.Common);
        item.imagePath = "belts/belt_002.png";
        item.healthBonus = 20;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(186, "Stardust Buckle", "A belt adorned with crystallized stardust. Smoothie sells similar trinkets.", ItemType.Belt, ItemRarity.Common);
        item.imagePath = "belts/belt_003.png";
        item.healthBonus = 12;
        item.abilityPowerBonus = 1;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(187, "Sentinel's Girdle", "A sturdy belt worn by dream watchers. It never loosens, never fails.", ItemType.Belt, ItemRarity.Common);
        item.imagePath = "belts/belt_004.png";
        item.defenseBonus = 5;
        item.healthBonus = 10;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(188, "Mystic Cinch", "A belt infused with faint magic. It tingles when nightmares are near.", ItemType.Belt, ItemRarity.Common);
        item.imagePath = "belts/belt_005.png";
        item.healthBonus = 8;
        item.abilityPowerBonus = 1;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(189, "Hunter's Strap", "A leather belt with pouches for supplies. Essential for long journeys through dreams.", ItemType.Belt, ItemRarity.Common);
        item.imagePath = "belts/belt_006.png";
        item.attackBonus = 2;
        item.healthBonus = 18;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(190, "Pilgrim's Band", "Worn by those who seek El's light. It offers comfort in the darkest dreams.", ItemType.Belt, ItemRarity.Common);
        item.imagePath = "belts/belt_007.png";
        item.healthBonus = 15;
        item.regeneration = 1f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(191, "Nightmare Hunter's Belt", "A belt crafted from defeated nightmares. Their essence strengthens the wearer.", ItemType.Belt, ItemRarity.Rare);
        item.imagePath = "belts/belt_008.png";
        item.attackBonus = 6;
        item.healthBonus = 35;
        item.criticalChance = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(192, "Arcanum Waistguard", "A belt from the Lunar Arcanum Society. It amplifies magical energy.", ItemType.Belt, ItemRarity.Rare);
        item.imagePath = "belts/belt_009.png";
        item.healthBonus = 30;
        item.abilityPowerBonus = 4;
        item.memoryHaste = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(193, "Sacred Flame Cincture", "A belt blessed by Vesper's Order. It burns with protective fire.", ItemType.Belt, ItemRarity.Rare);
        item.imagePath = "belts/belt_010.png";
        item.defenseBonus = 8;
        item.healthBonus = 45;
        item.thorns = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(194, "Duelist's Swordbelt", "A fine belt worn by Astrid's noble warriors. It holds blades and honor alike.", ItemType.Belt, ItemRarity.Rare);
        item.imagePath = "belts/belt_011.png";
        item.attackBonus = 8;
        item.healthBonus = 25;
        item.attackSpeed = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(195, "Spirit Weaver's Cord", "A belt woven by forest spirits for Nachia. It hums with natural energy.", ItemType.Belt, ItemRarity.Rare);
        item.imagePath = "belts/belt_012.png";
        item.healthBonus = 30;
        item.abilityPowerBonus = 3;
        item.regeneration = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(196, "Assassin's Utility Belt", "A belt with hidden compartments. Shell's handlers equipped their tools with these.", ItemType.Belt, ItemRarity.Rare);
        item.imagePath = "belts/belt_013.png";
        item.attackBonus = 10;
        item.healthBonus = 20;
        item.criticalDamage = 10f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(197, "Gunslinger's Holster", "A belt designed for Lacerta's firearms. Quick draw, quicker kill.", ItemType.Belt, ItemRarity.Rare);
        item.imagePath = "belts/belt_014.png";
        item.attackBonus = 7;
        item.healthBonus = 25;
        item.attackSpeed = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(198, "El's Divine Girdle", "A belt blessed by El itself. Its light anchors the soul against all darkness.", ItemType.Belt, ItemRarity.Epic);
        item.imagePath = "belts/belt_015.png";
        item.defenseBonus = 5;
        item.healthBonus = 80;
        item.regeneration = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(199, "Nightmare Sovereign's Chain", "A belt forged from nightmare lord's chains. It binds fear to the wearer's will.", ItemType.Belt, ItemRarity.Epic);
        item.imagePath = "belts/belt_016.png";
        item.attackBonus = 15;
        item.healthBonus = 60;
        item.criticalChance = 10f;
        item.lifeSteal = 3.5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(200, "Primordial Dream Sash", "A belt from the first dream. It contains echoes of creation itself.", ItemType.Belt, ItemRarity.Epic);
        item.imagePath = "belts/belt_017.png";
        item.healthBonus = 70;
        item.abilityPowerBonus = 7;
        item.memoryHaste = 12f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(201, "World Tree Root Belt", "A belt grown from the World Tree's roots. Nachia's forest blessed its creation.", ItemType.Belt, ItemRarity.Epic);
        item.imagePath = "belts/belt_018.png";
        item.healthBonus = 100;
        item.thorns = 8f;
        item.regeneration = 8f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(202, "Astrid's Heirloom", "The ancestral belt of House Astrid. Mist's bloodline is woven into its fabric.", ItemType.Belt, ItemRarity.Epic);
        item.imagePath = "belts/belt_019.png";
        item.attackBonus = 12;
        item.healthBonus = 50;
        item.dodgeChargesBonus = 1;
        item.attackSpeed = 12f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(203, "Twilight Inquisitor's Cincture", "The belt of Vesper's highest rank. It judges all who stand before it.", ItemType.Belt, ItemRarity.Epic);
        item.imagePath = "belts/belt_020.png";
        item.attackBonus = 10;
        item.defenseBonus = 5;
        item.healthBonus = 90;
        item.thorns = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Helmets (3 items)
        // ============================================================

        item = new RPGItem(204, "Bone Sentinel Mask", "A mask carved from the skull of a fallen nightmare. Its empty eyes see through illusions.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_001.png";
        item.defenseBonus = 8;
        item.healthBonus = 10;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(205, "Ethereal Guardian Helm", "A helm infused with spirits from Nachia's forest. Whispers of protection surround the wearer.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_002.png";
        item.defenseBonus = 8;
        item.healthBonus = 25;
        item.abilityPowerBonus = 2;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(206, "Nightmare Sovereign Crown", "A crown forged from conquered nightmares. Its wearer commands fear itself.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_003.png";
        item.defenseBonus = 5;
        item.healthBonus = 50;
        item.abilityPowerBonus = 5;
        item.criticalChance = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Chest Armor (3 items)
        // ============================================================

        item = new RPGItem(207, "Bone Sentinel Vest", "Ribcage armor from creatures that died dreaming. Their protection lingers beyond death.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_001.png";
        item.defenseBonus = 5;
        item.healthBonus = 25;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(208, "Ethereal Guardian Plate", "Chest armor woven from solidified dreams. It shifts and adapts to incoming blows.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_002.png";
        item.defenseBonus = 5;
        item.healthBonus = 50;
        item.regeneration = 2f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(209, "Nightmare Sovereign Cuirass", "Armor that pulses with dark energy. Nightmares bow before its wearer.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_003.png";
        item.defenseBonus = 10;
        item.healthBonus = 100;
        item.thorns = 5f;
        item.regeneration = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Pants (3 items)
        // ============================================================

        item = new RPGItem(210, "Bone Sentinel Greaves", "Leg guards reinforced with nightmare bone. They never tire, never falter.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_001.png";
        item.defenseBonus = 10;
        item.healthBonus = 15;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(211, "Ethereal Guardian Cuisses", "Leg armor blessed by forest spirits. The wearer moves like wind through leaves.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_002.png";
        item.defenseBonus = 10;
        item.healthBonus = 30;
        item.dodgeChargesBonus = 1;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(212, "Nightmare Sovereign Tassets", "Leg armor that drinks in shadow. The darkness empowers every step.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_003.png";
        item.defenseBonus = 8;
        item.healthBonus = 60;
        item.dodgeChargesBonus = 2;
        item.lifeSteal = 2f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Boots (3 items)
        // ============================================================

        item = new RPGItem(213, "Bone Sentinel Treads", "Boots that walk silently through the dream realm. The dead make no sound.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_001.png";
        item.defenseBonus = 5;
        item.moveSpeedBonus = 3f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(214, "Ethereal Guardian Sabatons", "Boots that leave no trace in the dream realm. Perfect for those who hunt nightmares.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_002.png";
        item.defenseBonus = 5;
        item.moveSpeedBonus = 8f;
        item.attackSpeed = 5f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(215, "Nightmare Sovereign Boots", "Boots that stride between worlds. Reality bends beneath each step.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_003.png";
        item.defenseBonus = 10;
        item.criticalDamage = 10f;
        item.moveSpeedBonus = 15f;
        item.attackSpeed = 10f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // AURENA HELMETS (Mage - AP, HP focus)
        // ============================================================

        item = new RPGItem(216, "Aurena's Apprentice Cap", "A simple cap worn by initiates of the Lunar Arcanum.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_004.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 1;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(217, "Aurena's Mystic Turban", "A turban imbued with faint lunar energy.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_005.png";
        item.defenseBonus = 3;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(218, "Aurena's Charmer Tiara", "A tiara that enhances the wearer's magical affinity.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_006.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 3;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(219, "Aurena's Ancient Tiara", "An ancient artifact passed down through generations.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_007.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 4;
        item.regeneration = 1f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(220, "Aurena's Falcon Circlet", "A circlet blessed by spirits of the sky.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_008.png";
        item.defenseBonus = 6;
        item.abilityPowerBonus = 5;
        item.healthBonus = 25;
        item.criticalChance = 3f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(221, "Aurena's Tiara of Power", "A powerful tiara radiating arcane might.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_009.png";
        item.defenseBonus = 7;
        item.abilityPowerBonus = 6;
        item.healthBonus = 30;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(222, "Aurena's Lunar Crown", "The legendary crown of the Lunar Arcanum's grandmaster.", ItemType.Helmet, ItemRarity.Legendary);
        item.imagePath = "helmets/helmet_010.png";
        item.defenseBonus = 10;
        item.abilityPowerBonus = 8;
        item.healthBonus = 50;
        item.criticalChance = 5f;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // AURENA ARMOR (Mage - AP, HP focus)
        // ============================================================

        item = new RPGItem(223, "Aurena's Blue Robe", "A simple robe worn by lunar mages.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_004.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 1;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(224, "Aurena's Magician Robe", "A robe favored by practicing mages.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_005.png";
        item.defenseBonus = 3;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(225, "Aurena's Spellweaver Robe", "A robe woven with magical threads.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_006.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 4;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(226, "Aurena's Enlightened Robe", "A robe that channels lunar wisdom.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_007.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 5;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(227, "Aurena's Dream Shroud", "A shroud woven from solidified dreams.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_008.png";
        item.defenseBonus = 7;
        item.abilityPowerBonus = 6;
        item.healthBonus = 35;
        item.lifeSteal = 1.5f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(228, "Aurena's Royal Scale Robe", "A robe fit for lunar royalty.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_009.png";
        item.defenseBonus = 8;
        item.abilityPowerBonus = 7;
        item.healthBonus = 40;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(229, "Aurena's Ice Queen Robe", "The legendary robe of the Ice Queen herself.", ItemType.ChestArmor, ItemRarity.Legendary);
        item.imagePath = "armor/armor_010.png";
        item.defenseBonus = 12;
        item.abilityPowerBonus = 10;
        item.healthBonus = 60;
        item.criticalChance = 5f;
        item.regeneration = 4f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // AURENA LEGS (Mage - AP, HP focus)
        // ============================================================

        item = new RPGItem(230, "Aurena's Blue Leggings", "Simple leggings for aspiring mages.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_004.png";
        item.defenseBonus = 2;
        item.healthBonus = 8;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(231, "Aurena's Bast Skirt", "A light skirt allowing free movement.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_005.png";
        item.defenseBonus = 2;
        item.moveSpeedBonus = 2f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(232, "Aurena's Elven Leggings", "Elegant leggings of elven design.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_006.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 2;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(233, "Aurena's Enlightened Leggings", "Leggings blessed with wisdom.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_007.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 3;
        item.regeneration = 1f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(234, "Aurena's Glacier Kilt", "A kilt infused with icy magic.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_008.png";
        item.defenseBonus = 6;
        item.abilityPowerBonus = 5;
        item.healthBonus = 25;
        item.criticalChance = 3f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(235, "Aurena's Icy Culottes", "Culottes that radiate cold power.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_009.png";
        item.defenseBonus = 7;
        item.abilityPowerBonus = 6;
        item.healthBonus = 30;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(236, "Aurena's Fabulous Leggings", "Legendary leggings of unmatched elegance.", ItemType.Pants, ItemRarity.Legendary);
        item.imagePath = "legs/legs_010.png";
        item.defenseBonus = 10;
        item.abilityPowerBonus = 8;
        item.healthBonus = 50;
        item.criticalChance = 5f;
        item.moveSpeedBonus = 5f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // AURENA BOOTS (Healer/Support - AP, HP, Life Steal focus)
        // ============================================================

        item = new RPGItem(237, "Soft Lunar Slippers", "Gentle footwear worn by acolytes of the Lunar Arcanum. They cushion each step with moonlight.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_004.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 2;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(238, "Pilgrim's Sandals", "Simple sandals blessed by the Society's elders. Aurena still remembers their warmth.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_005.png";
        item.defenseBonus = 3;
        item.abilityPowerBonus = 1;
        item.moveSpeedBonus = 3f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(239, "Eastern Mystic Shoes", "Exotic footwear from distant lands. They channel healing energy through the earth.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_006.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 4;
        item.healthBonus = 25;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(240, "Dreamwalker's Grace", "Boots that walk between the waking and dreaming worlds. Perfect for a healer seeking lost souls.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_007.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 5;
        item.moveSpeedBonus = 8f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(241, "Soulwalker Treads", "Boots infused with the essence of departed healers. Their wisdom guides each step.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_008.png";
        item.defenseBonus = 6;
        item.abilityPowerBonus = 8;
        item.healthBonus = 50;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(242, "Soulstalker Boots", "Dark boots that track the wounded. Aurena uses them to find those in need... or those who wronged her.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_009.png";
        item.defenseBonus = 7;
        item.abilityPowerBonus = 10;
        item.moveSpeedBonus = 12f;
        item.lifeSteal = 3.5f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(243, "Wings of the Fallen Sage", "Legendary boots said to grant flight to those pure of heart. Aurena's exile proved her worthy.", ItemType.Boots, ItemRarity.Legendary);
        item.imagePath = "boots/boots_010.png";
        item.defenseBonus = 10;
        item.abilityPowerBonus = 15;
        item.healthBonus = 80;
        item.moveSpeedBonus = 20f;
        item.lifeSteal = 5.5f;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // BISMUTH HELMETS (Crystal Mage - AP, Crit focus)
        // ============================================================

        item = new RPGItem(244, "Bismuth's Coned Hat", "A simple hat for crystal apprentices.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_011.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 1;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(245, "Bismuth's Jade Hat", "A hat with jade crystal inlays.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_012.png";
        item.defenseBonus = 3;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(246, "Bismuth's Glacier Mask", "A mask of frozen crystal.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_013.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 3;
        item.criticalChance = 2f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(247, "Bismuth's Yalahari Mask", "An ancient mask of crystalline power.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_014.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 4;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(248, "Bismuth's Prismatic Helmet", "A helmet refracting light into rainbows.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_015.png";
        item.defenseBonus = 6;
        item.abilityPowerBonus = 5;
        item.criticalChance = 4f;
        item.criticalDamage = 10f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(249, "Bismuth's Reflecting Crown", "A crown that mirrors magical energy.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_016.png";
        item.defenseBonus = 7;
        item.abilityPowerBonus = 6;
        item.healthBonus = 30;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(250, "Bismuth's Incandescent Crown", "A legendary crown blazing with crystal fire.", ItemType.Helmet, ItemRarity.Legendary);
        item.imagePath = "helmets/helmet_017.png";
        item.defenseBonus = 10;
        item.abilityPowerBonus = 8;
        item.criticalChance = 6f;
        item.criticalDamage = 15f;
        item.healthBonus = 40;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // BISMUTH ARMOR (Crystal Mage - AP, Crit focus)
        // ============================================================

        item = new RPGItem(251, "Bismuth's Ice Robe", "A robe chilled by crystal magic.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_011.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 1;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(252, "Bismuth's Monk Robe", "A simple robe for meditation.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_012.png";
        item.defenseBonus = 3;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(253, "Bismuth's Glacier Robe", "A robe frozen in eternal ice.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_013.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 4;
        item.criticalChance = 2f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(254, "Bismuth's Crystalline Armor", "Armor formed from living crystal.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_014.png";
        item.defenseBonus = 6;
        item.abilityPowerBonus = 3;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(255, "Bismuth's Prismatic Armor", "Armor that bends light itself.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_015.png";
        item.defenseBonus = 8;
        item.abilityPowerBonus = 6;
        item.criticalChance = 4f;
        item.criticalDamage = 10f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(256, "Bismuth's Frozen Plate", "Plate armor of eternal frost.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_016.png";
        item.defenseBonus = 10;
        item.abilityPowerBonus = 5;
        item.healthBonus = 40;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(257, "Bismuth's Divine Plate", "Legendary armor blessed by crystal gods.", ItemType.ChestArmor, ItemRarity.Legendary);
        item.imagePath = "armor/armor_017.png";
        item.defenseBonus = 12;
        item.abilityPowerBonus = 10;
        item.criticalChance = 6f;
        item.criticalDamage = 15f;
        item.healthBonus = 50;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // BISMUTH LEGS (Crystal Mage - AP, Crit focus)
        // ============================================================

        item = new RPGItem(258, "Bismuth's Chain Leggings", "Basic leggings with crystal links.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_011.png";
        item.defenseBonus = 2;
        item.healthBonus = 8;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(259, "Bismuth's Bast Leggings", "Light leggings for crystal mages.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_012.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 1;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(260, "Bismuth's Jade Leggings", "Leggings adorned with jade crystals.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_013.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 3;
        item.criticalChance = 2f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(261, "Bismuth's Eldritch Breeches", "Breeches pulsing with eldritch energy.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_014.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 4;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(262, "Bismuth's Prismatic Leggings", "Leggings that shimmer with prismatic light.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_015.png";
        item.defenseBonus = 6;
        item.abilityPowerBonus = 5;
        item.criticalChance = 4f;
        item.criticalDamage = 10f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(263, "Bismuth's Yalahari Leg Piece", "Ancient leggings of the Yalahari.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_016.png";
        item.defenseBonus = 7;
        item.abilityPowerBonus = 6;
        item.healthBonus = 30;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(264, "Bismuth's Falcon Greaves", "Legendary greaves of the Falcon Order.", ItemType.Pants, ItemRarity.Legendary);
        item.imagePath = "legs/legs_017.png";
        item.defenseBonus = 10;
        item.abilityPowerBonus = 8;
        item.criticalChance = 6f;
        item.criticalDamage = 15f;
        item.healthBonus = 40;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // BISMUTH BOOTS (Mage - AP, Crit, Elemental focus)
        // ============================================================

        item = new RPGItem(265, "Scholar's Slippers", "Comfortable footwear for long hours in the library. Knowledge flows through their soles.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_011.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 3;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(266, "Yalahari Footwraps", "Ancient wrappings from a lost civilization. They whisper forgotten spells.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_012.png";
        item.defenseBonus = 1;
        item.abilityPowerBonus = 4;
        item.moveSpeedBonus = 2f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(267, "Glacier Shoes", "Boots carved from eternal ice. They leave frost patterns wherever Bismuth walks.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_013.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 6;
        item.criticalChance = 3f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(268, "Frostflower Steps", "Delicate boots that bloom with ice crystals. Each step creates a garden of frost.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_014.png";
        item.defenseBonus = 3;
        item.abilityPowerBonus = 7;
        item.moveSpeedBonus = 6f;
        item.criticalChance = 2f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(269, "Crystal Resonance Boots", "Boots that amplify magical energy. The crystals hum with untapped power.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_015.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 10;
        item.criticalChance = 5f;
        item.criticalDamage = 15f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(270, "Prismatic Arcane Boots", "Boots that refract light into pure magical energy. Reality bends around each step.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_016.png";
        item.defenseBonus = 6;
        item.abilityPowerBonus = 12;
        item.moveSpeedBonus = 10f;
        item.criticalChance = 4f;
        item.criticalDamage = 10f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(271, "Void Strider Boots", "Legendary boots that step through the void itself. Bismuth sees paths others cannot imagine.", ItemType.Boots, ItemRarity.Legendary);
        item.imagePath = "boots/boots_017.png";
        item.defenseBonus = 8;
        item.abilityPowerBonus = 18;
        item.moveSpeedBonus = 15f;
        item.criticalChance = 8f;
        item.criticalDamage = 25f;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // LACERTA HELMETS (Dragon Warrior - ATK, DEF, HP focus)
        // ============================================================

        item = new RPGItem(272, "Lacerta's Leather Helmet", "A basic helmet for dragon warriors.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_018.png";
        item.defenseBonus = 3;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(273, "Lacerta's Chain Helmet", "A sturdy chain helmet.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_019.png";
        item.defenseBonus = 4;
        item.healthBonus = 8;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(274, "Lacerta's Warrior Helmet", "A helmet worn by seasoned warriors.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_020.png";
        item.defenseBonus = 6;
        item.attackBonus = 2;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(275, "Lacerta's Dragon Scale Helmet", "A helmet forged from dragon scales.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_021.png";
        item.defenseBonus = 7;
        item.attackBonus = 3;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(276, "Lacerta's Royal Helmet", "A helmet fit for dragon royalty.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_022.png";
        item.defenseBonus = 9;
        item.attackBonus = 4;
        item.healthBonus = 30;
        item.criticalChance = 3f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(277, "Lacerta's Elite Draken Helmet", "An elite helmet of the Draken order.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_023.png";
        item.defenseBonus = 10;
        item.attackBonus = 5;
        item.healthBonus = 35;
        item.attackSpeed = 5f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(278, "Lacerta's Golden Helmet", "The legendary golden helmet of dragon kings.", ItemType.Helmet, ItemRarity.Legendary);
        item.imagePath = "helmets/helmet_024.png";
        item.defenseBonus = 14;
        item.attackBonus = 8;
        item.healthBonus = 60;
        item.criticalChance = 5f;
        item.criticalDamage = 10f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // LACERTA ARMOR (Dragon Warrior - ATK, DEF, HP focus)
        // ============================================================

        item = new RPGItem(279, "Lacerta's Leather Armor", "Basic armor for dragon warriors.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_018.png";
        item.defenseBonus = 4;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(280, "Lacerta's Scale Armor", "Armor reinforced with scales.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_019.png";
        item.defenseBonus = 5;
        item.healthBonus = 12;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(281, "Lacerta's Knight Armor", "Heavy armor for dragon knights.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_020.png";
        item.defenseBonus = 7;
        item.attackBonus = 2;
        item.healthBonus = 25;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(282, "Lacerta's Dragon Scale Mail", "Mail forged from dragon scales.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_021.png";
        item.defenseBonus = 8;
        item.attackBonus = 3;
        item.healthBonus = 30;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(283, "Lacerta's Royal Draken Mail", "Royal mail of the Draken order.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_022.png";
        item.defenseBonus = 10;
        item.attackBonus = 5;
        item.healthBonus = 45;
        item.thorns = 3f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(284, "Lacerta's Falcon Plate", "Plate armor of the Falcon Order.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_023.png";
        item.defenseBonus = 11;
        item.attackBonus = 6;
        item.healthBonus = 50;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(285, "Lacerta's Golden Armor", "The legendary golden armor of dragon kings.", ItemType.ChestArmor, ItemRarity.Legendary);
        item.imagePath = "armor/armor_024.png";
        item.defenseBonus = 15;
        item.attackBonus = 10;
        item.healthBonus = 80;
        item.thorns = 5f;
        item.regeneration = 4f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // LACERTA LEGS (Dragon Warrior - ATK, DEF, HP focus)
        // ============================================================

        item = new RPGItem(286, "Lacerta's Leather Leggings", "Basic leggings for dragon warriors.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_018.png";
        item.defenseBonus = 3;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(287, "Lacerta's Studded Leggings", "Leggings reinforced with studs.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_019.png";
        item.defenseBonus = 4;
        item.healthBonus = 8;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(288, "Lacerta's Knight Leggings", "Heavy leggings for dragon knights.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_020.png";
        item.defenseBonus = 6;
        item.attackBonus = 2;
        item.healthBonus = 18;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(289, "Lacerta's Dragon Scale Leggings", "Leggings forged from dragon scales.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_021.png";
        item.defenseBonus = 7;
        item.attackBonus = 3;
        item.healthBonus = 22;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(290, "Lacerta's Crown Leggings", "Leggings fit for royalty.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_022.png";
        item.defenseBonus = 8;
        item.attackBonus = 4;
        item.healthBonus = 35;
        item.criticalChance = 3f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(291, "Lacerta's Ornate Leggings", "Ornate leggings of master craftsmanship.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_023.png";
        item.defenseBonus = 9;
        item.attackBonus = 5;
        item.healthBonus = 40;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(292, "Lacerta's Golden Leggings", "The legendary golden leggings of dragon kings.", ItemType.Pants, ItemRarity.Legendary);
        item.imagePath = "legs/legs_024.png";
        item.defenseBonus = 12;
        item.attackBonus = 8;
        item.healthBonus = 60;
        item.criticalChance = 5f;
        item.moveSpeedBonus = 5f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // LACERTA BOOTS (Melee Warrior - ATK, DEF, HP focus)
        // ============================================================

        item = new RPGItem(293, "Worn Leather Boots", "Simple boots that have seen many battles. They carry the scent of adventure.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_018.png";
        item.defenseBonus = 4;
        item.attackBonus = 2;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(294, "Patched Combat Boots", "Boots repaired countless times. Each patch tells a story of survival.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_019.png";
        item.defenseBonus = 3;
        item.attackBonus = 3;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(295, "Steel Warrior Boots", "Heavy boots forged for battle. They anchor Lacerta to the ground during combat.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_020.png";
        item.defenseBonus = 8;
        item.attackBonus = 5;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(296, "Guardian's Greaves", "Boots worn by elite protectors. They never retreat, never surrender.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_021.png";
        item.defenseBonus = 10;
        item.attackBonus = 4;
        item.healthBonus = 30;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(297, "Dragon Scale Greaves", "Boots crafted from dragon scales. They grant the wearer draconic resilience.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_022.png";
        item.defenseBonus = 12;
        item.attackBonus = 8;
        item.healthBonus = 50;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(298, "Draken Warlord Boots", "Boots of an ancient draken general. They command respect on any battlefield.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_023.png";
        item.defenseBonus = 10;
        item.attackBonus = 10;
        item.healthBonus = 40;
        item.criticalChance = 5f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(299, "Golden Champion Boots", "Legendary boots worn by the greatest warrior of an age. Lacerta's destiny awaits.", ItemType.Boots, ItemRarity.Legendary);
        item.imagePath = "boots/boots_024.png";
        item.defenseBonus = 15;
        item.attackBonus = 15;
        item.healthBonus = 80;
        item.criticalChance = 8f;
        item.lifeSteal = 3.5f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // MIST HELMETS (Assassin - Speed, Crit focus)
        // ============================================================

        item = new RPGItem(300, "Mist's Bandana", "A simple bandana for scouts.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_025.png";
        item.defenseBonus = 1;
        item.attackSpeed = 3f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(301, "Mist's Light Bandana", "A light bandana for quick movement.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_026.png";
        item.defenseBonus = 2;
        item.moveSpeedBonus = 3f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(302, "Mist's Dark Vision Bandana", "A bandana that enhances sight.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_027.png";
        item.defenseBonus = 3;
        item.criticalChance = 3f;
        item.attackSpeed = 5f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(303, "Mist's Lightning Headband", "A headband crackling with energy.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_028.png";
        item.defenseBonus = 4;
        item.attackSpeed = 8f;
        item.moveSpeedBonus = 5f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(304, "Mist's Cobra Hood", "A hood of deadly precision.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_029.png";
        item.defenseBonus = 5;
        item.criticalChance = 5f;
        item.criticalDamage = 15f;
        item.attackSpeed = 8f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(305, "Mist's Hellstalker Visor", "A visor that sees through shadows.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_030.png";
        item.defenseBonus = 6;
        item.criticalChance = 6f;
        item.attackSpeed = 10f;
        item.moveSpeedBonus = 8f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(306, "Mist's Dark Whispers", "A legendary mask that whispers death.", ItemType.Helmet, ItemRarity.Legendary);
        item.imagePath = "helmets/helmet_031.png";
        item.defenseBonus = 8;
        item.criticalChance = 8f;
        item.criticalDamage = 25f;
        item.attackSpeed = 12f;
        item.moveSpeedBonus = 10f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // MIST ARMOR (Assassin - Speed, Crit focus)
        // ============================================================

        item = new RPGItem(307, "Mist's Leather Harness", "A light harness for agility.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_025.png";
        item.defenseBonus = 2;
        item.attackSpeed = 3f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(308, "Mist's Ranger Cloak", "A cloak for swift movement.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_026.png";
        item.defenseBonus = 2;
        item.moveSpeedBonus = 3f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(309, "Mist's Midnight Tunic", "A tunic for nocturnal strikes.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_027.png";
        item.defenseBonus = 4;
        item.criticalChance = 3f;
        item.attackSpeed = 5f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(310, "Mist's Lightning Robe", "A robe charged with lightning.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_028.png";
        item.defenseBonus = 4;
        item.attackSpeed = 8f;
        item.moveSpeedBonus = 5f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(311, "Mist's Voltage Armor", "Armor surging with electric power.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_029.png";
        item.defenseBonus = 6;
        item.criticalChance = 5f;
        item.criticalDamage = 15f;
        item.attackSpeed = 10f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(312, "Mist's Master Archer Armor", "Armor of legendary archers.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_030.png";
        item.defenseBonus = 7;
        item.criticalChance = 6f;
        item.attackSpeed = 12f;
        item.moveSpeedBonus = 8f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(313, "Mist's Dark Lord Cape", "The legendary cape of shadow lords.", ItemType.ChestArmor, ItemRarity.Legendary);
        item.imagePath = "armor/armor_031.png";
        item.defenseBonus = 10;
        item.criticalChance = 8f;
        item.criticalDamage = 25f;
        item.attackSpeed = 15f;
        item.moveSpeedBonus = 12f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // MIST LEGS (Assassin - Speed, Crit focus)
        // ============================================================

        item = new RPGItem(314, "Mist's Ranger Leggings", "Light leggings for quick movement.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_025.png";
        item.defenseBonus = 2;
        item.moveSpeedBonus = 3f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(315, "Mist's Jungle Survivor Leggings", "Leggings for jungle traversal.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_026.png";
        item.defenseBonus = 2;
        item.attackSpeed = 3f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(316, "Mist's Midnight Sarong", "A sarong for silent movement.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_027.png";
        item.defenseBonus = 3;
        item.criticalChance = 3f;
        item.moveSpeedBonus = 5f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(317, "Mist's Lightning Leggings", "Leggings crackling with energy.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_028.png";
        item.defenseBonus = 4;
        item.attackSpeed = 8f;
        item.moveSpeedBonus = 5f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(318, "Mist's Grasshopper Leggings", "Leggings for incredible leaps.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_029.png";
        item.defenseBonus = 5;
        item.criticalChance = 5f;
        item.moveSpeedBonus = 10f;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(319, "Mist's Green Demon Leggings", "Leggings of demonic speed.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_030.png";
        item.defenseBonus = 6;
        item.criticalChance = 6f;
        item.attackSpeed = 10f;
        item.moveSpeedBonus = 8f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(320, "Mist's Soulstrider Leggings", "Legendary leggings that stride between worlds.", ItemType.Pants, ItemRarity.Legendary);
        item.imagePath = "legs/legs_031.png";
        item.defenseBonus = 8;
        item.criticalChance = 8f;
        item.criticalDamage = 20f;
        item.attackSpeed = 12f;
        item.moveSpeedBonus = 15f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // MIST BOOTS (Ranged Scout - ATK Speed, Move Speed, Crit focus)
        // ============================================================

        item = new RPGItem(321, "Makeshift Scout Boots", "Boots cobbled together from scraps. Light and quiet for reconnaissance.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_025.png";
        item.defenseBonus = 2;
        item.moveSpeedBonus = 5f;
        item.attackSpeed = 3f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(322, "Swamp Runner Boots", "Waterproof boots for traversing difficult terrain. Mist knows every shortcut.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_026.png";
        item.defenseBonus = 3;
        item.moveSpeedBonus = 4f;
        item.attackBonus = 2;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(323, "Haste Striders", "Enchanted boots that quicken the wearer's pace. Perfect for hit-and-run tactics.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_027.png";
        item.defenseBonus = 4;
        item.moveSpeedBonus = 10f;
        item.attackSpeed = 8f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(324, "Stag Hunter Boots", "Boots crafted from the hide of a legendary stag. They grant preternatural agility.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_028.png";
        item.defenseBonus = 5;
        item.moveSpeedBonus = 8f;
        item.attackSpeed = 6f;
        item.criticalChance = 4f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(325, "Lightning Striker Boots", "Boots charged with electrical energy. Each step crackles with power.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_029.png";
        item.defenseBonus = 6;
        item.moveSpeedBonus = 15f;
        item.attackSpeed = 12f;
        item.criticalChance = 6f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(326, "Firewalker Combat Boots", "Boots that leave trails of flame. Enemies fear to follow.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_030.png";
        item.defenseBonus = 7;
        item.moveSpeedBonus = 12f;
        item.attackSpeed = 10f;
        item.attackBonus = 8;
        item.criticalDamage = 15f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(327, "Treader of Torment", "Legendary boots that walk through nightmares unscathed. Mist becomes the hunter of hunters.", ItemType.Boots, ItemRarity.Legendary);
        item.imagePath = "boots/boots_031.png";
        item.defenseBonus = 10;
        item.moveSpeedBonus = 20f;
        item.attackSpeed = 15f;
        item.attackBonus = 12;
        item.criticalChance = 10f;
        item.criticalDamage = 25f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // NACHIA HELMETS (Nature Hunter - ATK, Nature focus)
        // ============================================================

        item = new RPGItem(328, "Nachia's Fur Cap", "A warm cap made from forest creatures.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_032.png";
        item.defenseBonus = 2;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(329, "Nachia's Feather Headdress", "A headdress adorned with feathers.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_033.png";
        item.defenseBonus = 2;
        item.attackBonus = 1;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(330, "Nachia's Shamanic Mask", "A mask of spiritual power.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_034.png";
        item.defenseBonus = 4;
        item.attackBonus = 3;
        item.lifeSteal = 1.5f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(331, "Nachia's Terra Hood", "A hood blessed by earth spirits.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_035.png";
        item.defenseBonus = 5;
        item.attackBonus = 3;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(332, "Nachia's Helmet of Nature", "A helmet infused with nature's power.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_036.png";
        item.defenseBonus = 7;
        item.attackBonus = 5;
        item.healthBonus = 30;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(333, "Nachia's Arboreal Crown", "A crown grown from ancient trees.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_037.png";
        item.defenseBonus = 8;
        item.attackBonus = 6;
        item.healthBonus = 35;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(334, "Nachia's Leaf Crown", "The legendary crown of the forest guardian.", ItemType.Helmet, ItemRarity.Legendary);
        item.imagePath = "helmets/helmet_038.png";
        item.defenseBonus = 12;
        item.attackBonus = 8;
        item.healthBonus = 50;
        item.lifeSteal = 3.5f;
        item.regeneration = 4f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // NACHIA ARMOR (Nature Hunter - ATK, Nature focus)
        // ============================================================

        item = new RPGItem(335, "Nachia's Fur Armor", "Armor made from forest creatures.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_032.png";
        item.defenseBonus = 3;
        item.healthBonus = 12;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(336, "Nachia's Native Armor", "Traditional armor of the forest tribes.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_033.png";
        item.defenseBonus = 3;
        item.attackBonus = 1;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(337, "Nachia's Greenwood Coat", "A coat woven from living vines.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_034.png";
        item.defenseBonus = 5;
        item.attackBonus = 3;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(338, "Nachia's Terra Mantle", "A mantle blessed by earth spirits.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_035.png";
        item.defenseBonus = 6;
        item.attackBonus = 4;
        item.lifeSteal = 1.5f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(339, "Nachia's Embrace of Nature", "Armor that is one with nature.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_036.png";
        item.defenseBonus = 8;
        item.attackBonus = 6;
        item.healthBonus = 40;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(340, "Nachia's Swamplair Armor", "Armor from the deep swamps.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_037.png";
        item.defenseBonus = 9;
        item.attackBonus = 7;
        item.healthBonus = 45;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(341, "Nachia's Leaf Robe", "The legendary robe of the forest guardian.", ItemType.ChestArmor, ItemRarity.Legendary);
        item.imagePath = "armor/armor_038.png";
        item.defenseBonus = 12;
        item.attackBonus = 10;
        item.healthBonus = 70;
        item.lifeSteal = 3.5f;
        item.regeneration = 5f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // NACHIA LEGS (Nature Hunter - ATK, Nature focus)
        // ============================================================

        item = new RPGItem(342, "Nachia's Mammoth Fur Shorts", "Warm shorts from mammoth fur.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_032.png";
        item.defenseBonus = 2;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(343, "Nachia's Lederhosen", "Traditional hunting leggings.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_033.png";
        item.defenseBonus = 2;
        item.attackBonus = 1;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(344, "Nachia's Stag Leggings", "Leggings made from stag hide.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_034.png";
        item.defenseBonus = 4;
        item.attackBonus = 3;
        item.moveSpeedBonus = 5f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(345, "Nachia's Terra Leggings", "Leggings blessed by earth spirits.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_035.png";
        item.defenseBonus = 5;
        item.attackBonus = 3;
        item.healthBonus = 18;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(346, "Nachia's Leaf Leggings", "Leggings woven from enchanted leaves.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_036.png";
        item.defenseBonus = 6;
        item.attackBonus = 5;
        item.healthBonus = 30;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(347, "Nachia's Wereboar Loincloth", "A loincloth from a mighty wereboar.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_037.png";
        item.defenseBonus = 7;
        item.attackBonus = 6;
        item.healthBonus = 35;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(348, "Nachia's Sanguine Greaves", "Legendary greaves of the blood hunt.", ItemType.Pants, ItemRarity.Legendary);
        item.imagePath = "legs/legs_038.png";
        item.defenseBonus = 10;
        item.attackBonus = 8;
        item.healthBonus = 50;
        item.lifeSteal = 3.5f;
        item.moveSpeedBonus = 8f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // NACHIA BOOTS (Ranger - ATK, Move Speed, Nature focus)
        // ============================================================

        item = new RPGItem(349, "Fur-Lined Boots", "Warm boots for tracking prey through any weather. A hunter's faithful companion.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_032.png";
        item.defenseBonus = 3;
        item.attackBonus = 2;
        item.moveSpeedBonus = 3f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(350, "Badger Hide Boots", "Tough boots made from badger hide. They grip any terrain.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_033.png";
        item.defenseBonus = 4;
        item.attackBonus = 1;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(351, "Cobra Strike Boots", "Boots that grant serpent-like speed. Strike before the enemy knows you're there.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_034.png";
        item.defenseBonus = 5;
        item.attackBonus = 5;
        item.moveSpeedBonus = 8f;
        item.attackSpeed = 5f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(352, "Forest Stalker Wraps", "Footwraps blessed by forest spirits. They leave no trace in the wilderness.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_035.png";
        item.defenseBonus = 4;
        item.attackBonus = 6;
        item.moveSpeedBonus = 10f;
        item.criticalChance = 3f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(353, "Terra Hunter Boots", "Boots infused with earth magic. They root Nachia to the land she protects.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_036.png";
        item.defenseBonus = 8;
        item.attackBonus = 10;
        item.moveSpeedBonus = 12f;
        item.criticalChance = 5f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(354, "Feverbloom Ranger Boots", "Boots adorned with fever flowers. They grant feverish speed and deadly accuracy.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_037.png";
        item.defenseBonus = 6;
        item.attackBonus = 12;
        item.moveSpeedBonus = 10f;
        item.attackSpeed = 8f;
        item.criticalDamage = 15f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(355, "Sanguine Predator Boots", "Legendary boots stained with the blood of countless hunts. Nachia becomes the apex predator.", ItemType.Boots, ItemRarity.Legendary);
        item.imagePath = "boots/boots_038.png";
        item.defenseBonus = 10;
        item.attackBonus = 15;
        item.moveSpeedBonus = 18f;
        item.attackSpeed = 10f;
        item.criticalChance = 8f;
        item.criticalDamage = 20f;
        item.lifeSteal = 3.5f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // HUSK/SHELL HELMETS (Tank - DEF, HP, Thorns focus)
        // ============================================================

        item = new RPGItem(356, "Shell's Damaged Helmet", "A battered helmet from the underworld.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_039.png";
        item.defenseBonus = 4;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(357, "Shell's Broken Visor", "A cracked visor still offering protection.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_040.png";
        item.defenseBonus = 5;
        item.healthBonus = 8;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(358, "Shell's Bonelord Helmet", "A helmet forged from bonelord remains.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_041.png";
        item.defenseBonus = 7;
        item.healthBonus = 20;
        item.thorns = 2f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(359, "Shell's Skull Helmet", "A helmet shaped from skulls.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_042.png";
        item.defenseBonus = 8;
        item.healthBonus = 25;
        item.thorns = 3f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(360, "Shell's Galea Mortis", "The helmet of death itself.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_043.png";
        item.defenseBonus = 10;
        item.healthBonus = 40;
        item.thorns = 5f;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(361, "Shell's Norcferatu Skullguard", "A helmet of vampiric bone.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_044.png";
        item.defenseBonus = 11;
        item.healthBonus = 45;
        item.thorns = 6f;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(362, "Shell's Demon Helmet", "The legendary helmet of demon lords.", ItemType.Helmet, ItemRarity.Legendary);
        item.imagePath = "helmets/helmet_045.png";
        item.defenseBonus = 15;
        item.healthBonus = 70;
        item.thorns = 10f;
        item.lifeSteal = 3.5f;
        item.regeneration = 4f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // HUSK/SHELL ARMOR (Tank - DEF, HP, Thorns focus)
        // ============================================================

        item = new RPGItem(363, "Shell's Burial Shroud", "A shroud from the grave.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_039.png";
        item.defenseBonus = 4;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(364, "Shell's Old Cape", "A tattered cape from ages past.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_040.png";
        item.defenseBonus = 5;
        item.healthBonus = 12;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(365, "Shell's Norcferatu Bonecloak", "A cloak woven from bones.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_041.png";
        item.defenseBonus = 7;
        item.healthBonus = 25;
        item.thorns = 3f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(366, "Shell's Spirit Cloak", "A cloak of restless spirits.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_042.png";
        item.defenseBonus = 8;
        item.healthBonus = 30;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(367, "Shell's Toga Mortis", "The toga of death.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_043.png";
        item.defenseBonus = 11;
        item.healthBonus = 50;
        item.thorns = 6f;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(368, "Shell's Robe of the Underworld", "A robe from the depths.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_044.png";
        item.defenseBonus = 12;
        item.healthBonus = 55;
        item.thorns = 7f;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(369, "Shell's Demon Armor", "The legendary armor of demon lords.", ItemType.ChestArmor, ItemRarity.Legendary);
        item.imagePath = "armor/armor_045.png";
        item.defenseBonus = 18;
        item.healthBonus = 90;
        item.thorns = 12f;
        item.lifeSteal = 3.5f;
        item.regeneration = 5f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // HUSK/SHELL LEGS (Tank - DEF, HP, Thorns focus)
        // ============================================================

        item = new RPGItem(370, "Shell's Broken Faulds", "Damaged leg armor still functional.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_039.png";
        item.defenseBonus = 4;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(371, "Shell's Mutant Bone Kilt", "A kilt of mutant bones.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_040.png";
        item.defenseBonus = 5;
        item.healthBonus = 8;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(372, "Shell's Norcferatu Thornwraps", "Leg wraps covered in thorns.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_041.png";
        item.defenseBonus = 7;
        item.healthBonus = 20;
        item.thorns = 3f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(373, "Shell's Norcferatu Fleshguards", "Guards made from preserved flesh.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_042.png";
        item.defenseBonus = 8;
        item.healthBonus = 25;
        item.thorns = 4f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(374, "Shell's Sanguine Leggings", "Leggings soaked in blood.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_043.png";
        item.defenseBonus = 10;
        item.healthBonus = 40;
        item.thorns = 6f;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(375, "Shell's Norcferatu Bloodstrider", "Leggings that stride through blood.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_044.png";
        item.defenseBonus = 11;
        item.healthBonus = 45;
        item.thorns = 7f;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(376, "Shell's Demon Leggings", "The legendary leggings of demon lords.", ItemType.Pants, ItemRarity.Legendary);
        item.imagePath = "legs/legs_045.png";
        item.defenseBonus = 14;
        item.healthBonus = 70;
        item.thorns = 10f;
        item.lifeSteal = 3.5f;
        item.regeneration = 4f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // HUSK/SHELL BOOTS (Tank - DEF, HP, Thorns focus)
        // ============================================================

        item = new RPGItem(377, "Ironclad Spats", "Heavy metal boots that crush anything underfoot. Defense is the best offense.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_039.png";
        item.defenseBonus = 6;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(378, "Seafarer's Boots", "Sturdy boots for weathering any storm. They anchor Shell against the tide of battle.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_040.png";
        item.defenseBonus = 5;
        item.healthBonus = 20;
        item.moveSpeedBonus = 2f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(379, "Abyssal Greaves", "Boots forged in the depths of the ocean. They resist all pressure.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_041.png";
        item.defenseBonus = 10;
        item.healthBonus = 35;
        item.thorns = 5f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(380, "Bone Crusher Boots", "Boots reinforced with monster bones. Each step is a threat.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_042.png";
        item.defenseBonus = 12;
        item.healthBonus = 30;
        item.attackBonus = 3;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(381, "Magma Fortress Boots", "Boots forged in volcanic fire. They radiate heat that burns attackers.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_043.png";
        item.defenseBonus = 15;
        item.healthBonus = 60;
        item.thorns = 10f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(382, "Goretrampler Boots", "Brutal boots that leave destruction in their wake. Enemies fear to engage.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_044.png";
        item.defenseBonus = 14;
        item.healthBonus = 50;
        item.attackBonus = 6;
        item.thorns = 8f;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(383, "Stoic Guardian Boots", "Legendary boots of an unbreakable defender. Shell becomes an immovable fortress.", ItemType.Boots, ItemRarity.Legendary);
        item.imagePath = "boots/boots_045.png";
        item.defenseBonus = 20;
        item.healthBonus = 100;
        item.thorns = 15f;
        item.lifeSteal = 3.5f;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // VESPER HELMETS (Dream Mage - AP, Utility focus)
        // ============================================================

        item = new RPGItem(384, "Vesper's Witch Hat", "A simple hat for dream weavers.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_046.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(385, "Vesper's Spooky Hood", "A hood that whispers secrets.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_047.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 1;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(386, "Vesper's Eldritch Hood", "A hood pulsing with eldritch energy.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_048.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 4;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(387, "Vesper's Eldritch Cowl", "A cowl of nightmare energy.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_049.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 5;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(388, "Vesper's Shroud of Despair", "A shroud that feeds on despair.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_050.png";
        item.defenseBonus = 7;
        item.abilityPowerBonus = 7;
        item.healthBonus = 30;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(389, "Vesper's Dark Wizard Crown", "A crown of dark magic.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_051.png";
        item.defenseBonus = 8;
        item.abilityPowerBonus = 8;
        item.healthBonus = 35;
        item.criticalChance = 4f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(390, "Vesper's Ferumbras Hat", "The legendary hat of the dark master.", ItemType.Helmet, ItemRarity.Legendary);
        item.imagePath = "helmets/helmet_052.png";
        item.defenseBonus = 12;
        item.abilityPowerBonus = 12;
        item.healthBonus = 60;
        item.criticalChance = 6f;
        item.lifeSteal = 3.5f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // VESPER ARMOR (Dream Mage - AP, Utility focus)
        // ============================================================

        item = new RPGItem(391, "Vesper's Red Robe", "A robe for aspiring dream mages.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_046.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(392, "Vesper's Spirit Bind", "A binding that channels spirits.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_047.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 1;
        item.healthBonus = 12;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(393, "Vesper's Energy Robe", "A robe crackling with energy.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_048.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 5;
        item.healthBonus = 18;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(394, "Vesper's Spectral Dress", "A dress woven from spectral energy.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_049.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 5;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(395, "Vesper's Soulmantle", "A mantle that holds souls.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_050.png";
        item.defenseBonus = 8;
        item.abilityPowerBonus = 8;
        item.healthBonus = 40;
        item.lifeSteal = 2f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(396, "Vesper's Soulshroud", "A shroud of captured souls.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_051.png";
        item.defenseBonus = 9;
        item.abilityPowerBonus = 9;
        item.healthBonus = 45;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(397, "Vesper's Arcane Dragon Robe", "The legendary robe of arcane dragons.", ItemType.ChestArmor, ItemRarity.Legendary);
        item.imagePath = "armor/armor_052.png";
        item.defenseBonus = 14;
        item.abilityPowerBonus = 14;
        item.healthBonus = 70;
        item.criticalChance = 6f;
        item.lifeSteal = 3.5f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // VESPER LEGS (Dream Mage - AP, Utility focus)
        // ============================================================

        item = new RPGItem(398, "Vesper's Soulful Leggings", "Leggings imbued with soul energy.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_046.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 1;
        item.healthBonus = 8;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(399, "Vesper's Exotic Leggings", "Exotic leggings from distant realms.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_047.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 2;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(400, "Vesper's Soulshanks", "Leggings that channel soul power.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_048.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 4;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(401, "Vesper's Sanguine Trousers", "Trousers stained with blood magic.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_049.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 5;
        item.lifeSteal = 1.5f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(402, "Vesper's Magma Leggings", "Leggings forged in magical fire.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_050.png";
        item.defenseBonus = 7;
        item.abilityPowerBonus = 7;
        item.healthBonus = 30;
        item.criticalChance = 4f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(403, "Vesper's Leggings of Wisdom", "Leggings of ancient wisdom.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_051.png";
        item.defenseBonus = 8;
        item.abilityPowerBonus = 8;
        item.healthBonus = 35;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(404, "Vesper's Trousers of the Ancients", "Legendary trousers from the ancients.", ItemType.Pants, ItemRarity.Legendary);
        item.imagePath = "legs/legs_052.png";
        item.defenseBonus = 12;
        item.abilityPowerBonus = 12;
        item.healthBonus = 55;
        item.criticalChance = 6f;
        item.lifeSteal = 3.5f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // VESPER BOOTS (Support Mage - AP, HP, Utility focus)
        // ============================================================

        item = new RPGItem(405, "Acolyte Slippers", "Soft slippers worn by temple initiates. They carry prayers in every step.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_046.png";
        item.defenseBonus = 2;
        item.abilityPowerBonus = 2;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(406, "Temple Shoes", "Traditional footwear of El's servants. They ground Vesper to her faith.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_047.png";
        item.defenseBonus = 3;
        item.abilityPowerBonus = 3;
        item.moveSpeedBonus = 2f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(407, "Eldritch Monk Boots", "Boots worn by monks who study forbidden texts. Knowledge and faith intertwine.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_048.png";
        item.defenseBonus = 5;
        item.abilityPowerBonus = 6;
        item.healthBonus = 25;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(408, "Gnomish Blessed Wraps", "Footwraps enchanted by gnomish artificers. Technology meets divinity.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_049.png";
        item.defenseBonus = 4;
        item.abilityPowerBonus = 7;
        item.moveSpeedBonus = 6f;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(409, "Vampire Silk Slippers", "Elegant slippers woven from vampire silk. They drain life from the earth itself.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_050.png";
        item.defenseBonus = 6;
        item.abilityPowerBonus = 10;
        item.healthBonus = 45;
        item.lifeSteal = 3.5f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(410, "Demon Slayer Slippers", "Green slippers blessed to destroy evil. They burn demons with each step.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_051.png";
        item.defenseBonus = 7;
        item.abilityPowerBonus = 12;
        item.healthBonus = 40;
        item.moveSpeedBonus = 8f;
        item.criticalChance = 4f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(411, "Nightmare Banisher Boots", "Legendary boots that walk through nightmares to save the innocent. Vesper's ultimate calling.", ItemType.Boots, ItemRarity.Legendary);
        item.imagePath = "boots/boots_052.png";
        item.defenseBonus = 10;
        item.abilityPowerBonus = 18;
        item.healthBonus = 80;
        item.moveSpeedBonus = 12f;
        item.lifeSteal = 5.5f;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // YUBAR HELMETS (Tribal Warrior - ATK, DEF, HP focus)
        // ============================================================

        item = new RPGItem(412, "Yubar's Tribal Mask", "A mask of tribal warriors.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_053.png";
        item.defenseBonus = 3;
        item.attackBonus = 1;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(413, "Yubar's Viking Helmet", "A helmet of northern warriors.", ItemType.Helmet, ItemRarity.Common);
        item.imagePath = "helmets/helmet_054.png";
        item.defenseBonus = 4;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(414, "Yubar's Horned Helmet", "A helmet with fearsome horns.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_055.png";
        item.defenseBonus = 6;
        item.attackBonus = 3;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(415, "Yubar's Stoic Iks Headpiece", "A headpiece of the Iks tribe.", ItemType.Helmet, ItemRarity.Rare);
        item.imagePath = "helmets/helmet_056.png";
        item.defenseBonus = 7;
        item.attackBonus = 4;
        item.healthBonus = 20;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(416, "Yubar's Dreadfire Headpiece", "A headpiece burning with dread.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_057.png";
        item.defenseBonus = 9;
        item.attackBonus = 6;
        item.healthBonus = 35;
        item.criticalChance = 3f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(417, "Yubar's Stoic Iks Casque", "A casque of the Iks champions.", ItemType.Helmet, ItemRarity.Epic);
        item.imagePath = "helmets/helmet_058.png";
        item.defenseBonus = 10;
        item.attackBonus = 7;
        item.healthBonus = 40;
        item.thorns = 4f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(418, "Yubar's Visage of the End Days", "The legendary visage of apocalypse.", ItemType.Helmet, ItemRarity.Legendary);
        item.imagePath = "helmets/helmet_059.png";
        item.defenseBonus = 14;
        item.attackBonus = 10;
        item.healthBonus = 70;
        item.criticalChance = 5f;
        item.thorns = 6f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // YUBAR ARMOR (Tribal Warrior - ATK, DEF, HP focus)
        // ============================================================

        item = new RPGItem(419, "Yubar's Bear Skin", "Armor from a mighty bear.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_053.png";
        item.defenseBonus = 4;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(420, "Yubar's Mammoth Fur Cape", "A cape from a mammoth.", ItemType.ChestArmor, ItemRarity.Common);
        item.imagePath = "armor/armor_054.png";
        item.defenseBonus = 5;
        item.healthBonus = 12;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(421, "Yubar's Stoic Iks Robe", "A robe of the Iks tribe.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_055.png";
        item.defenseBonus = 7;
        item.attackBonus = 3;
        item.healthBonus = 25;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(422, "Yubar's Magma Coat", "A coat forged in magma.", ItemType.ChestArmor, ItemRarity.Rare);
        item.imagePath = "armor/armor_056.png";
        item.defenseBonus = 8;
        item.attackBonus = 4;
        item.healthBonus = 28;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(423, "Yubar's Stoic Iks Cuirass", "A cuirass of Iks champions.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_057.png";
        item.defenseBonus = 11;
        item.attackBonus = 6;
        item.healthBonus = 50;
        item.thorns = 4f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(424, "Yubar's Molten Plate", "Plate armor of molten rock.", ItemType.ChestArmor, ItemRarity.Epic);
        item.imagePath = "armor/armor_058.png";
        item.defenseBonus = 12;
        item.attackBonus = 7;
        item.healthBonus = 55;
        item.regeneration = 3f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(425, "Yubar's Fireborn Giant Armor", "The legendary armor of fire giants.", ItemType.ChestArmor, ItemRarity.Legendary);
        item.imagePath = "armor/armor_059.png";
        item.defenseBonus = 16;
        item.attackBonus = 12;
        item.healthBonus = 90;
        item.thorns = 8f;
        item.regeneration = 5f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // YUBAR LEGS (Tribal Warrior - ATK, DEF, HP focus)
        // ============================================================

        item = new RPGItem(426, "Yubar's Stoic Iks Culet", "Leg armor of the Iks tribe.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_053.png";
        item.defenseBonus = 3;
        item.healthBonus = 10;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(427, "Yubar's Mutant Hide Trousers", "Trousers from mutant hide.", ItemType.Pants, ItemRarity.Common);
        item.imagePath = "legs/legs_054.png";
        item.defenseBonus = 4;
        item.healthBonus = 8;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(428, "Yubar's Stoic Iks Faulds", "Faulds of the Iks warriors.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_055.png";
        item.defenseBonus = 6;
        item.attackBonus = 3;
        item.healthBonus = 18;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(429, "Yubar's Dwarven Leggings", "Sturdy dwarven leggings.", ItemType.Pants, ItemRarity.Rare);
        item.imagePath = "legs/legs_056.png";
        item.defenseBonus = 7;
        item.attackBonus = 3;
        item.healthBonus = 22;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(430, "Yubar's Alloy Leggings", "Leggings of reinforced alloy.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_057.png";
        item.defenseBonus = 9;
        item.attackBonus = 5;
        item.healthBonus = 38;
        item.thorns = 3f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(431, "Yubar's Plate Leggings", "Heavy plate leggings.", ItemType.Pants, ItemRarity.Epic);
        item.imagePath = "legs/legs_058.png";
        item.defenseBonus = 10;
        item.attackBonus = 6;
        item.healthBonus = 42;
        item.regeneration = 2f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(432, "Yubar's Gnome Leggings", "Legendary leggings of gnome craftsmanship.", ItemType.Pants, ItemRarity.Legendary);
        item.imagePath = "legs/legs_059.png";
        item.defenseBonus = 13;
        item.attackBonus = 10;
        item.healthBonus = 65;
        item.thorns = 6f;
        item.moveSpeedBonus = 5f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // YUBAR BOOTS (Heavy Melee - ATK, DEF, HP focus)
        // ============================================================

        item = new RPGItem(433, "Makeshift War Boots", "Boots assembled from battlefield scraps. Yubar makes do with what he finds.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_053.png";
        item.defenseBonus = 4;
        item.attackBonus = 3;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(434, "Tribal Footwraps", "Traditional wraps of Yubar's people. They connect him to his ancestors.", ItemType.Boots, ItemRarity.Common);
        item.imagePath = "boots/boots_054.png";
        item.defenseBonus = 3;
        item.attackBonus = 2;
        item.healthBonus = 15;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(435, "Stag Warrior Greaves", "Boots adorned with stag antlers. They grant the strength of the forest king.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_055.png";
        item.defenseBonus = 8;
        item.attackBonus = 6;
        item.healthBonus = 25;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(436, "Fangstomper Boots", "Brutal boots with fang-like spikes. They crush enemies underfoot.", ItemType.Boots, ItemRarity.Rare);
        item.imagePath = "boots/boots_056.png";
        item.defenseBonus = 7;
        item.attackBonus = 8;
        item.healthBonus = 20;
        item.thorns = 3f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(437, "Sanguine Berserker Boots", "Blood-red boots that fuel Yubar's rage. Pain becomes power.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_057.png";
        item.defenseBonus = 10;
        item.attackBonus = 12;
        item.healthBonus = 50;
        item.lifeSteal = 3.5f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(438, "Soulsole Crushers", "Boots that harvest the souls of fallen enemies. Their power grows with each battle.", ItemType.Boots, ItemRarity.Epic);
        item.imagePath = "boots/boots_058.png";
        item.defenseBonus = 9;
        item.attackBonus = 14;
        item.healthBonus = 40;
        item.criticalChance = 6f;
        item.criticalDamage = 15f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(439, "Boots of the Homebound Champion", "Legendary boots that carry Yubar back to his homeland in spirit. His ancestors fight alongside him.", ItemType.Boots, ItemRarity.Legendary);
        item.imagePath = "boots/boots_059.png";
        item.defenseBonus = 15;
        item.attackBonus = 18;
        item.healthBonus = 100;
        item.criticalChance = 8f;
        item.criticalDamage = 20f;
        item.lifeSteal = 5.5f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // UNIVERSAL LEGENDARY HELMET (Anyone can equip)
        // ============================================================

        item = new RPGItem(440, "Winged Helmet of the Dreamer", "A legendary helmet that grants the power of flight to those who dream. Automatically attacks the nearest enemy.", ItemType.Helmet, ItemRarity.Legendary);
        item.imagePath = "helmets/helmet_060.png";
        item.defenseBonus = 12;
        item.healthBonus = 50;
        item.attackBonus = 5;
        item.abilityPowerBonus = 5;
        item.criticalChance = 5f;
        item.moveSpeedBonus = 8f;
        item.autoAttack = true;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // UNIVERSAL LEGENDARY ARMOR (Anyone can equip)
        // ============================================================

        item = new RPGItem(441, "Magic Plate Armor of Dreams", "Legendary armor that protects dreamers across all realms. Automatically aims memories at the nearest enemy.", ItemType.ChestArmor, ItemRarity.Legendary);
        item.imagePath = "armor/armor_060.png";
        item.defenseBonus = 15;
        item.healthBonus = 80;
        item.attackBonus = 5;
        item.abilityPowerBonus = 5;
        item.regeneration = 4f;
        item.thorns = 5f;
        item.autoAim = true;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // UNIVERSAL LEGENDARY LEGS (Anyone can equip)
        // ============================================================

        item = new RPGItem(442, "Depth Ocrea of the Abyss", "Legendary leggings forged in the deepest depths of dreams.", ItemType.Pants, ItemRarity.Legendary);
        item.imagePath = "legs/legs_060.png";
        item.defenseBonus = 12;
        item.healthBonus = 60;
        item.attackBonus = 5;
        item.abilityPowerBonus = 5;
        item.moveSpeedBonus = 10f;
        item.dodgeChargesBonus = 1;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // UNIVERSAL LEGENDARY BOOTS (Anyone can equip)
        // ============================================================

        item = new RPGItem(443, "Boots of Waterwalking", "Legendary boots that allow the wearer to walk on water. A treasure sought by dreamers across all realms.", ItemType.Boots, ItemRarity.Legendary);
        item.imagePath = "boots/boots_060.png";
        item.defenseBonus = 12;
        item.moveSpeedBonus = 25f;
        item.dodgeChargesBonus = 2;
        item.healthBonus = 60;
        item.attackSpeed = 10f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // UNIVERSAL LEGENDARY BELT (Anyone can equip)
        // ============================================================

        item = new RPGItem(448, "Belt of Infinite Dreams", "A legendary belt that binds the wearer to the eternal cycle of dreams. Its power grows with each nightmare conquered.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_021.png";
        item.defenseBonus = 8;
        item.healthBonus = 100;
        item.attackBonus = 8;
        item.abilityPowerBonus = 8;
        item.criticalChance = 8f;
        item.regeneration = 6f;
        item.lifeSteal = 4f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // HERO-SPECIFIC LEGENDARY BELTS
        // ============================================================

        item = new RPGItem(449, "Aurena's Life Weaver Belt", "A belt forged from Aurena's stolen life force research. It weaves life and death into perfect balance.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_022.png";
        item.healthBonus = 120;
        item.abilityPowerBonus = 12;
        item.lifeSteal = 6f;
        item.regeneration = 8f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(450, "Bismuth's Crystalline Girdle", "A belt of pure crystal that pulses with the blind girl's vision. It sees what eyes cannot.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_023.png";
        item.defenseBonus = 10;
        item.healthBonus = 100;
        item.abilityPowerBonus = 15;
        item.memoryHaste = 20f;
        item.criticalChance = 10f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(451, "Lacerta's Crimson Eye Belt", "A belt that channels Lacerta's legendary gaze. No target escapes its watchful power.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_024.png";
        item.attackBonus = 18;
        item.healthBonus = 90;
        item.criticalChance = 15f;
        item.criticalDamage = 25f;
        item.attackSpeed = 15f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(452, "Mist's Duelist Champion Belt", "The ultimate belt of House Astrid. Mist's honor is woven into every thread.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_025.png";
        item.attackBonus = 16;
        item.healthBonus = 100;
        item.dodgeChargesBonus = 2;
        item.attackSpeed = 18f;
        item.moveSpeedBonus = 12f;
        item.criticalChance = 12f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(453, "Nachia's World Tree Belt", "A belt grown from the World Tree itself. Nature's fury flows through it.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_026.png";
        item.healthBonus = 150;
        item.abilityPowerBonus = 10;
        item.thorns = 12f;
        item.regeneration = 10f;
        item.memoryHaste = 15f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(454, "Husk's Unbreakable Chain", "A belt forged from unbreakable resolve. Husk's determination is its strength.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_027.png";
        item.defenseBonus = 15;
        item.healthBonus = 130;
        item.attackBonus = 10;
        item.thorns = 10f;
        item.regeneration = 8f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(455, "Shell's Shadow Assassin Belt", "A belt of perfect darkness. Shell's handlers could never break its will.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_028.png";
        item.attackBonus = 20;
        item.healthBonus = 80;
        item.criticalChance = 18f;
        item.criticalDamage = 30f;
        item.attackSpeed = 20f;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Shell";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(456, "Vesper's Twilight Inquisitor Belt", "The highest rank belt of El's Order. It judges nightmares and banishes darkness.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_029.png";
        item.defenseBonus = 12;
        item.healthBonus = 140;
        item.abilityPowerBonus = 14;
        item.thorns = 8f;
        item.regeneration = 10f;
        item.lifeSteal = 5f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(457, "Yubar's Ancestral Champion Belt", "A belt that calls upon Yubar's ancestors. Their strength flows through every fiber.", ItemType.Belt, ItemRarity.Legendary);
        item.imagePath = "belts/belt_030.png";
        item.defenseBonus = 14;
        item.attackBonus = 18;
        item.healthBonus = 120;
        item.criticalChance = 12f;
        item.criticalDamage = 20f;
        item.lifeSteal = 6f;
        item.thorns = 8f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // UNIVERSAL LEGENDARY RING (Anyone can equip)
        // ============================================================

        item = new RPGItem(458, "Ring of Eternal Dreams", "A legendary ring that connects the wearer to the infinite realm of dreams. Its power transcends all boundaries.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_021.png";
        item.attackBonus = 10;
        item.abilityPowerBonus = 10;
        item.healthBonus = 80;
        item.criticalChance = 10f;
        item.criticalDamage = 20f;
        item.lifeSteal = 5f;
        item.memoryHaste = 15f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // HERO-SPECIFIC LEGENDARY RINGS
        // ============================================================

        item = new RPGItem(459, "Aurena's Grand Sage Ring", "The ring of the Lunar Arcanum's Grand Sage. Aurena took it as proof of their hypocrisy.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_022.png";
        item.healthBonus = 100;
        item.abilityPowerBonus = 18;
        item.lifeSteal = 8f;
        item.regeneration = 10f;
        item.memoryHaste = 20f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(460, "Bismuth's Gemheart Ring", "A ring of pure crystalline magic. It pulses with the blind girl's transformed essence.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_023.png";
        item.attackBonus = 8;
        item.abilityPowerBonus = 20;
        item.healthBonus = 90;
        item.memoryHaste = 25f;
        item.criticalChance = 12f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(461, "Lacerta's Royal Executioner Ring", "The ring of the Royal Guard's elite executioner. Lacerta earned it through countless battles.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_024.png";
        item.attackBonus = 22;
        item.healthBonus = 80;
        item.criticalChance = 20f;
        item.criticalDamage = 35f;
        item.attackSpeed = 18f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(462, "Mist's Astrid Legacy Ring", "The ancestral ring of House Astrid. Mist's bloodline flows through its metal.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_025.png";
        item.attackBonus = 20;
        item.healthBonus = 90;
        item.dodgeChargesBonus = 2;
        item.attackSpeed = 22f;
        item.moveSpeedBonus = 15f;
        item.criticalChance = 15f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(463, "Nachia's Forest Guardian Ring", "A ring blessed by the spirits of Nachia's forest. Nature's power is its essence.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_026.png";
        item.healthBonus = 130;
        item.abilityPowerBonus = 12;
        item.thorns = 15f;
        item.regeneration = 12f;
        item.memoryHaste = 18f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(464, "Husk's Unyielding Resolve Ring", "A ring forged from unbreakable will. Husk's determination is its power.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_027.png";
        item.defenseBonus = 8;
        item.healthBonus = 140;
        item.attackBonus = 12;
        item.thorns = 12f;
        item.regeneration = 10f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(465, "Shell's Perfect Shadow Ring", "A ring of absolute darkness. Shell's handlers could never control its power.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_028.png";
        item.attackBonus = 24;
        item.healthBonus = 70;
        item.criticalChance = 22f;
        item.criticalDamage = 40f;
        item.attackSpeed = 25f;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Shell";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(466, "Vesper's Sacred Flame Ring", "A ring containing the pure essence of El's Sacred Flame. It banishes all darkness.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_029.png";
        item.defenseBonus = 10;
        item.healthBonus = 150;
        item.abilityPowerBonus = 16;
        item.thorns = 10f;
        item.regeneration = 12f;
        item.lifeSteal = 6f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(467, "Yubar's Tribal Champion Ring", "A ring that channels the strength of Yubar's ancestors. Their fury empowers every strike.", ItemType.Ring, ItemRarity.Legendary);
        item.imagePath = "rings/ring_030.png";
        item.defenseBonus = 8;
        item.attackBonus = 20;
        item.healthBonus = 110;
        item.criticalChance = 15f;
        item.criticalDamage = 30f;
        item.lifeSteal = 7f;
        item.thorns = 10f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // UNIVERSAL LEGENDARY AMULET (Anyone can equip)
        // ============================================================

        item = new RPGItem(468, "Amulet of Infinite Dreams", "A legendary amulet that binds the wearer to the eternal cycle of dreams. Its power transcends all boundaries.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_021.png";
        item.attackBonus = 12;
        item.abilityPowerBonus = 12;
        item.healthBonus = 100;
        item.criticalChance = 12f;
        item.criticalDamage = 25f;
        item.lifeSteal = 6f;
        item.memoryHaste = 18f;
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // HERO-SPECIFIC LEGENDARY AMULETS
        // ============================================================

        item = new RPGItem(469, "Aurena's Life Weaver Amulet", "An amulet forged from Aurena's stolen life force research. It weaves life and death into perfect balance.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_022.png";
        item.healthBonus = 130;
        item.abilityPowerBonus = 20;
        item.lifeSteal = 10f;
        item.regeneration = 12f;
        item.memoryHaste = 22f;
        item.requiredHero = "Hero_Aurena";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(470, "Bismuth's Crystalline Heart", "An amulet of pure crystal that pulses with the blind girl's vision. It sees what eyes cannot.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_023.png";
        item.defenseBonus = 8;
        item.healthBonus = 110;
        item.abilityPowerBonus = 22;
        item.memoryHaste = 28f;
        item.criticalChance = 15f;
        item.requiredHero = "Hero_Bismuth";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(471, "Lacerta's Crimson Eye Amulet", "An amulet that channels Lacerta's legendary gaze. No target escapes its watchful power.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_024.png";
        item.attackBonus = 24;
        item.healthBonus = 100;
        item.criticalChance = 22f;
        item.criticalDamage = 40f;
        item.attackSpeed = 20f;
        item.requiredHero = "Hero_Lacerta";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(472, "Mist's Duelist Champion Amulet", "The ultimate amulet of House Astrid. Mist's honor is woven into every gem.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_025.png";
        item.attackBonus = 22;
        item.healthBonus = 110;
        item.dodgeChargesBonus = 2;
        item.attackSpeed = 25f;
        item.moveSpeedBonus = 15f;
        item.criticalChance = 18f;
        item.requiredHero = "Hero_Mist";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(473, "Nachia's World Tree Amulet", "An amulet grown from the World Tree itself. Nature's fury flows through it.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_026.png";
        item.healthBonus = 160;
        item.abilityPowerBonus = 14;
        item.thorns = 15f;
        item.regeneration = 15f;
        item.memoryHaste = 20f;
        item.requiredHero = "Hero_Nachia";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(474, "Husk's Unbreakable Amulet", "An amulet forged from unbreakable resolve. Husk's determination is its strength.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_027.png";
        item.defenseBonus = 12;
        item.healthBonus = 150;
        item.attackBonus = 14;
        item.thorns = 14f;
        item.regeneration = 12f;
        item.requiredHero = "Hero_Husk";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(475, "Shell's Shadow Assassin Amulet", "An amulet of perfect darkness. Shell's handlers could never break its will.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_028.png";
        item.attackBonus = 26;
        item.healthBonus = 90;
        item.criticalChance = 25f;
        item.criticalDamage = 45f;
        item.attackSpeed = 28f;
        item.dodgeChargesBonus = 1;
        item.requiredHero = "Hero_Shell";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(476, "Vesper's Twilight Inquisitor Amulet", "The highest rank amulet of El's Order. It judges nightmares and banishes darkness.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_029.png";
        item.defenseBonus = 10;
        item.healthBonus = 160;
        item.abilityPowerBonus = 18;
        item.thorns = 12f;
        item.regeneration = 15f;
        item.lifeSteal = 8f;
        item.requiredHero = "Hero_Vesper";
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(477, "Yubar's Ancestral Champion Amulet", "An amulet that calls upon Yubar's ancestors. Their strength flows through every gem.", ItemType.Amulet, ItemRarity.Legendary);
        item.imagePath = "amulets/amulet_030.png";
        item.defenseBonus = 10;
        item.attackBonus = 22;
        item.healthBonus = 130;
        item.criticalChance = 18f;
        item.criticalDamage = 35f;
        item.lifeSteal = 8f;
        item.thorns = 12f;
        item.requiredHero = "Hero_Yubar";
        LoadItemSprite(item);
        _allItems.Add(item);

        // ============================================================
        // Consumables (4 items)
        // ============================================================

        item = new RPGItem(444, "Dreamer's Tonic", "A simple remedy brewed from herbs found in the waking world. Even the faintest dream begins with a single drop of hope.", ItemType.Consumable, ItemRarity.Common);
        item.imagePath = "consumables/commonhealth_potion.png";
        item.healPercentage = 15f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(445, "Reverie Essence", "Distilled from the memories of peaceful slumbers. Those who drink it feel the warmth of forgotten dreams washing over their wounds.", ItemType.Consumable, ItemRarity.Rare);
        item.imagePath = "consumables/rarehealth_potion.png";
        item.healPercentage = 30f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(446, "Lucid Vitality", "Crafted by Smoothie using stardust and nightmare fragments. The liquid shimmers with the light of a thousand sleeping stars.", ItemType.Consumable, ItemRarity.Epic);
        item.imagePath = "consumables/epichealth_potion.png";
        item.healPercentage = 50f;
        LoadItemSprite(item);
        _allItems.Add(item);

        item = new RPGItem(447, "Elixir of El", "A sacred draught blessed by the light of El itself. Vesper's order guards its recipe jealously, for it can mend even wounds inflicted by the darkest nightmares.", ItemType.Consumable, ItemRarity.Legendary);
        item.imagePath = "consumables/legendaryhealth_potion.png";
        item.healPercentage = 75f;
        item.shieldPercentage = 50f;
        LoadItemSprite(item);
        _allItems.Add(item);

        
        _isInitialized = true;
        RPGLog.Debug("[ItemDatabase] Loaded " + _allItems.Count + " items");
    }
}
