using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

/// <summary>
/// Loot category for balanced drops
/// </summary>
public enum LootCategory
{
    Weapons,      // Weapon, TwoHandedWeapon, OffHand
    Armor,        // Helmet, ChestArmor, Pants, Boots
    Accessories   // Ring, Amulet, Belt
}

/// <summary>
/// Handles monster loot drops with full host/client sync.
/// Server determines drops, clients receive synced items via NetworkedItemSystem.
/// Uses category-based pools for balanced loot distribution.
/// </summary>
public static class MonsterLootSystem
{
    private static System.Random _random = new System.Random();
    
    // ============================================================
    // CATEGORY-BASED LOOT POOLS (for balanced drops)
    // Roll category first, then item within category
    // ============================================================
    
    // Weapons: Weapon, TwoHandedWeapon, OffHand
    private static Dictionary<ItemRarity, List<RPGItem>> _weaponPools = new Dictionary<ItemRarity, List<RPGItem>>();
    
    // Armor: Helmet, ChestArmor, Pants, Boots
    private static Dictionary<ItemRarity, List<RPGItem>> _armorPools = new Dictionary<ItemRarity, List<RPGItem>>();
    
    // Accessories: Ring, Amulet, Belt
    private static Dictionary<ItemRarity, List<RPGItem>> _accessoryPools = new Dictionary<ItemRarity, List<RPGItem>>();
    
    // Consumables (separate roll, not part of equipment drops)
    private static List<RPGItem> _consumablePools = new List<RPGItem>();
    
    // Category drop weights (equal chance for each category)
    private static Dictionary<LootCategory, float> _categoryWeights = new Dictionary<LootCategory, float>
    {
        { LootCategory.Weapons, 1.0f },
        { LootCategory.Armor, 1.0f },
        { LootCategory.Accessories, 1.0f }
    };
    
    // Legacy pools for compatibility
    private static List<RPGItem> _allLootPool = new List<RPGItem>();
    
    // Default drop chances by monster type (0.0 - 1.0) for EQUIPMENT
    // These are base values, actual values come from config
    private static Dictionary<Monster.MonsterType, float> _defaultDropChances = new Dictionary<Monster.MonsterType, float>
    {
        { Monster.MonsterType.Lesser, 0.03f },    // 3% base
        { Monster.MonsterType.Normal, 0.06f },    // 6% base
        { Monster.MonsterType.MiniBoss, 0.40f },  // 40% base
        { Monster.MonsterType.Boss, 1.0f }        // 100% guaranteed
    };
    
    // Default drop chances for POTIONS
    private static Dictionary<Monster.MonsterType, float> _defaultPotionDropChances = new Dictionary<Monster.MonsterType, float>
    {
        { Monster.MonsterType.Lesser, 0.02f },    // 2% base
        { Monster.MonsterType.Normal, 0.05f },    // 5% base
        { Monster.MonsterType.MiniBoss, 0.10f },  // 10% base
        { Monster.MonsterType.Boss, 0.25f }       // 25% base
    };
    
    /// <summary>
    /// Get equipment drop chance for monster type (uses config)
    /// </summary>
    private static float GetEquipDropChance(Monster.MonsterType type)
    {
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        float baseChance = config != null ? config.equipmentDropRate / 100f : 0.15f;
        
        // Apply type multipliers
        switch (type)
        {
            case Monster.MonsterType.Lesser:
                return baseChance * 0.2f; // 20% of base
            case Monster.MonsterType.Normal:
                return baseChance * 0.4f; // 40% of base
            case Monster.MonsterType.MiniBoss:
                float miniBossMult = config != null ? config.miniBossDropMultiplier : 2.0f;
                return Mathf.Min(1f, baseChance * miniBossMult);
            case Monster.MonsterType.Boss:
                float bossMult = config != null ? config.bossDropMultiplier : 3.0f;
                return Mathf.Min(1f, baseChance * bossMult);
            default:
                return baseChance;
        }
    }
    
    /// <summary>
    /// Get consumable drop chance for monster type (uses config)
    /// </summary>
    private static float GetConsumableDropChance(Monster.MonsterType type)
    {
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        float baseChance = config != null ? config.consumableDropRate / 100f : 0.08f;
        
        // Apply type multipliers (lower than equipment)
        switch (type)
        {
            case Monster.MonsterType.Lesser:
                return baseChance * 0.25f;
            case Monster.MonsterType.Normal:
                return baseChance * 0.6f;
            case Monster.MonsterType.MiniBoss:
                return baseChance * 1.25f;
            case Monster.MonsterType.Boss:
                return baseChance * 3f;
            default:
                return baseChance;
        }
    }
    
    // Use game's LootManager for rarity selection (like Shrine_Memory)
    // MiniBoss and Boss use "high" rarity chances (like empowered shrines)
    private static Dictionary<Monster.MonsterType, bool> _useHighRarity = new Dictionary<Monster.MonsterType, bool>
    {
        { Monster.MonsterType.Lesser, false },
        { Monster.MonsterType.Normal, false },
        { Monster.MonsterType.MiniBoss, true },   // Uses high rarity like empowered shrines
        { Monster.MonsterType.Boss, true }        // Uses high rarity like empowered shrines
    };
    
    // Track if we're currently subscribed to ActorManager
    private static bool _isSubscribedToActorManager = false;
    
    // Track if pools are initialized
    private static bool _poolsInitialized = false;
    
    public static void Initialize(List<RPGItem> itemTemplates)
    {
        // Always reinitialize pools even if already initialized (for new runs)
        RPGLog.Debug(" Initializing Monster Loot System...");
        
        // Clear all pools first
        ClearAllPools();
        
        // Initialize category pools for each rarity
        foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
        {
            _weaponPools[rarity] = new List<RPGItem>();
            _armorPools[rarity] = new List<RPGItem>();
            _accessoryPools[rarity] = new List<RPGItem>();
        }
        
        int weaponCount = 0, armorCount = 0, accessoryCount = 0;
        
        foreach (RPGItem item in itemTemplates)
        {
            // Separate consumables into their own pool
            if (item.type == ItemType.Consumable)
            {
                _consumablePools.Add(item);
                continue;
            }
            
            _allLootPool.Add(item);
            
            // Categorize by item type into the appropriate pool
            LootCategory category = GetItemCategory(item.type);
            
            switch (category)
            {
                case LootCategory.Weapons:
                    _weaponPools[item.rarity].Add(item);
                    weaponCount++;
                    break;
                case LootCategory.Armor:
                    _armorPools[item.rarity].Add(item);
                    armorCount++;
                    break;
                case LootCategory.Accessories:
                    _accessoryPools[item.rarity].Add(item);
                    accessoryCount++;
                    break;
            }
        }
        
        RPGLog.Debug(string.Format(" Loot pools by category: Weapons={0}, Armor={1}, Accessories={2}, Consumables={3}",
            weaponCount, armorCount, accessoryCount, _consumablePools.Count));
        
        _poolsInitialized = true;
        
        // NOTE: We no longer subscribe here - subscription is handled by CallOnNetworkedManager<ActorManager>
        // This ensures we always subscribe to the CURRENT ActorManager instance
    }
    
    /// <summary>
    /// Subscribe to the current ActorManager instance.
    /// Called from RPGItemsMod when ActorManager starts (via CallOnNetworkedManager callback).
    /// </summary>
    public static void SubscribeToActorManager()
    {
        // Reset subscription state - we're subscribing to a NEW ActorManager
        _isSubscribedToActorManager = false;
        
        // Initialize pools if needed (they might have been cleaned up between games)
        if (!_poolsInitialized)
        {
            RPGLog.Debug(" Pools not initialized, initializing from ItemDatabase...");
            List<RPGItem> itemTemplates = ItemDatabase.GetAllItems();
            if (itemTemplates != null && itemTemplates.Count > 0)
            {
                Initialize(itemTemplates);
            }
            else
            {
                RPGLog.Warning(" No item templates available, cannot initialize pools!");
                return;
            }
        }
        
        if (NetworkedManagerBase<ActorManager>.instance != null)
        {
            NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += OnEntityAdded;
            _isSubscribedToActorManager = true;
            RPGLog.Debug(" Subscribed to ActorManager.ClientEvent_OnEntityAdd for monster loot drops");
        }
        else
        {
            RPGLog.Warning(" ActorManager.instance is null, cannot subscribe!");
        }
    }
    
    /// <summary>
    /// Unsubscribe from ActorManager.
    /// Called from RPGItemsMod when ActorManager stops (via CallOnNetworkedManager callback).
    /// </summary>
    public static void UnsubscribeFromActorManager()
    {
        if (!_isSubscribedToActorManager)
        {
            return;
        }
        
        try
        {
            if (NetworkedManagerBase<ActorManager>.instance != null)
            {
                NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= OnEntityAdded;
                RPGLog.Debug(" Unsubscribed from ActorManager for monster loot");
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error unsubscribing from ActorManager: " + e.Message);
        }
        
        _isSubscribedToActorManager = false;
    }
    
    /// <summary>
    /// Legacy method - no longer needed, subscription handled via CallOnNetworkedManager
    /// </summary>
    public static void CheckDelayedSubscription()
    {
        // No longer needed - kept for compatibility but does nothing
    }
    
    /// <summary>
    /// Get the loot category for an item type
    /// </summary>
    private static LootCategory GetItemCategory(ItemType type)
    {
        switch (type)
        {
            // Weapons
            case ItemType.Weapon:
            case ItemType.TwoHandedWeapon:
            case ItemType.OffHand:
                return LootCategory.Weapons;
            
            // Armor
            case ItemType.Helmet:
            case ItemType.ChestArmor:
            case ItemType.Pants:
            case ItemType.Boots:
                return LootCategory.Armor;
            
            // Accessories
            case ItemType.Ring:
            case ItemType.Amulet:
            case ItemType.Belt:
            default:
                return LootCategory.Accessories;
        }
    }
    
    /// <summary>
    /// Clear all loot pools
    /// </summary>
    private static void ClearAllPools()
    {
        _weaponPools.Clear();
        _armorPools.Clear();
        _accessoryPools.Clear();
        _consumablePools.Clear();
        _allLootPool.Clear();
    }
    
    public static void Cleanup()
    {
        RPGLog.Debug(" MonsterLootSystem Cleanup called");
        
        // Note: Unsubscription from ActorManager is handled by CallOnNetworkedManager callback
        // We just need to clear pools and reset state
        
        _poolsInitialized = false;
        ClearAllPools();
        
        RPGLog.Debug(" MonsterLootSystem cleaned up");
    }
    
    private static void OnEntityAdded(Entity entity)
    {
        // Only handle monsters on server
        if (!NetworkServer.active) return;
        
        Monster monster = entity as Monster;
        if (monster == null) return;
        
        // Subscribe to death event
        monster.EntityEvent_OnDeath += OnMonsterDeath;
    }
    
    private static void OnMonsterDeath(EventInfoKill killInfo)
    {
        // Server only
        if (!NetworkServer.active) return;
        
        Monster monster = killInfo.victim as Monster;
        if (monster == null) return;
        
        // Check if loot is disabled for this monster
        if (monster.disableLoot) return;
        
        // Handle Gold on Kill for the killer (if they have the Midas Amulet effect)
        HandleGoldOnKill(killInfo);
        
        // NEW SYSTEM: Generate INDIVIDUAL drops for EACH player
        // Each player gets their own private drop chance, so everyone has fair loot opportunity
        // This prevents "strong get stronger" problem
        
        // Get all human players
        List<DewPlayer> allPlayers = new List<DewPlayer>();
        foreach (DewPlayer player in DewPlayer.allHumanPlayers)
        {
            if (player != null && player.hero != null && !player.hero.Status.isDead)
            {
                allPlayers.Add(player);
            }
        }
        
        if (allPlayers.Count == 0) return;
        
        // For each player, roll for their own individual drop
        foreach (DewPlayer player in allPlayers)
        {
            uint playerHeroNetId = player.hero.netId;
            
            // Roll for EQUIPMENT drop for this player (uses config)
            float equipDropChance = GetEquipDropChance(monster.type);
            
            float equipRoll = (float)_random.NextDouble();
            if (equipRoll <= equipDropChance)
            {
                // Determine rarity for equipment
                ItemRarity selectedRarity = SelectRarityForMonster(monster.type, false);
                
                // Select an equipment item
                RPGItem droppedItem = SelectItemForDrop(selectedRarity, player.hero);
                if (droppedItem != null)
                {
                    RPGItem itemClone = droppedItem.Clone();
                    
                    // Randomize stats Diablo-style!
                    RandomizeItemStats(itemClone);
                    
                    // Roll for pre-upgraded drop (+1, +2, etc.)
                    RollForUpgradedDrop(itemClone, monster.type);
                    
                    Vector3 dropPos = GetRandomDropPosition(monster.position);
                    // PRIVATE DROP for this specific player only!
                    SpawnMonsterLoot(itemClone, dropPos, monster.type, playerHeroNetId, true);
                }
            }
            
            // Roll for POTION drop for this player (separate roll, uses config)
            if (_consumablePools.Count > 0)
            {
                float potionDropChance = GetConsumableDropChance(monster.type);
                
                float potionRoll = (float)_random.NextDouble();
                if (potionRoll <= potionDropChance)
                {
                    // Determine rarity for potion (uses separate, heavily Common-weighted selection)
                    ItemRarity potionRarity = SelectPotionRarity(monster.type);
                    
                    // Select a potion
                    RPGItem potion = SelectPotionForDrop(potionRarity);
                    if (potion != null)
                    {
                        RPGItem potionClone = potion.Clone();
                        // Potions drop with 1-3 stacks based on monster type
                        potionClone.currentStack = GetPotionDropAmount(monster.type);
                        
                        Vector3 dropPos = GetRandomDropPosition(monster.position);
                        // PRIVATE DROP for this specific player only!
                        SpawnMonsterLoot(potionClone, dropPos, monster.type, playerHeroNetId, true);
                    }
                }
            }
        }
    }
    
    // Track gold/dust on kill bonuses for each player (synced from clients)
    // Now includes upgrade level for scaling
    private static Dictionary<uint, int> _playerGoldOnKillBonus = new Dictionary<uint, int>();
    private static Dictionary<uint, int> _playerGoldOnKillUpgrade = new Dictionary<uint, int>(); // Upgrade level of Midas item
    private static Dictionary<uint, int> _playerDustOnKillBonus = new Dictionary<uint, int>();
    private static Dictionary<uint, int> _playerDustOnKillUpgrade = new Dictionary<uint, int>(); // Upgrade level of dust item
    
    /// <summary>
    /// Register gold/dust on kill bonuses for a player (called by server when client sends update)
    /// Now includes upgrade level for scaling
    /// Thread-safe: Uses dictionary indexer (safe for single-threaded Unity main thread)
    /// </summary>
    public static void RegisterOnKillBonuses(uint heroNetId, int goldOnKill, int goldUpgrade, int dustOnKill, int dustUpgrade)
    {
        // Thread-safe: Unity main thread only, but add null check for safety
        if (heroNetId == 0) return;
        
        _playerGoldOnKillBonus[heroNetId] = goldOnKill;
        _playerGoldOnKillUpgrade[heroNetId] = goldUpgrade;
        _playerDustOnKillBonus[heroNetId] = dustOnKill;
        _playerDustOnKillUpgrade[heroNetId] = dustUpgrade;
    }
    
    /// <summary>
    /// Clear on kill bonuses for a player (called when leaving game)
    /// Thread-safe: Uses TryGetValue pattern for safety
    /// </summary>
    public static void ClearOnKillBonuses(uint heroNetId)
    {
        if (heroNetId == 0) return;
        
        // Use TryGetValue pattern for thread safety
        if (_playerGoldOnKillBonus.ContainsKey(heroNetId))
        _playerGoldOnKillBonus.Remove(heroNetId);
        if (_playerGoldOnKillUpgrade.ContainsKey(heroNetId))
        _playerGoldOnKillUpgrade.Remove(heroNetId);
        if (_playerDustOnKillBonus.ContainsKey(heroNetId))
        _playerDustOnKillBonus.Remove(heroNetId);
        if (_playerDustOnKillUpgrade.ContainsKey(heroNetId))
        _playerDustOnKillUpgrade.Remove(heroNetId);
    }
    
    /// <summary>
    /// Handle gold and dust on kill effects (Midas Amulet, etc.)
    /// Base drop: 1-5 gold/dust per point (displayed in tooltip)
    /// Lesser monsters: 1-3 (reduced from base)
    /// Normal monsters: 1-5 (base amount as displayed)
    /// MiniBoss: 3-8 (bonus)
    /// Boss: 5-15 (big bonus)
    /// Upgrade scaling: Same as other items (+50% per upgrade level)
    /// </summary>
    private static void HandleGoldOnKill(EventInfoKill killInfo)
    {
        // Get the victim position for drops
        if (killInfo.victim == null) return;
        Vector3 dropPosition = killInfo.victim.position;
        
        // Get monster type for scaling
        Monster monster = killInfo.victim as Monster;
        Monster.MonsterType monsterType = Monster.MonsterType.Normal;
        if (monster != null)
        {
            monsterType = monster.type;
        }
        
        // Find the killer player (actor is the killer in EventInfoKill)
        if (killInfo.actor == null) return;
        
        // Check if the actor is a Hero
        Hero killerHero = killInfo.actor as Hero;
        if (killerHero == null)
        {
            // Try to find hero from the actor's owner chain
            killerHero = killInfo.actor.FindFirstOfType<Hero>();
        }
        
        if (killerHero == null) return;
        
        uint heroNetId = killerHero.netId;
        
        // Get bonuses from registry (synced from all players including host)
        // Use TryGetValue for thread safety
        int goldOnKillBonus = 0;
        int goldUpgradeLevel = 0;
        int dustOnKillBonus = 0;
        int dustUpgradeLevel = 0;
        
        _playerGoldOnKillBonus.TryGetValue(heroNetId, out goldOnKillBonus);
        _playerGoldOnKillUpgrade.TryGetValue(heroNetId, out goldUpgradeLevel);
        _playerDustOnKillBonus.TryGetValue(heroNetId, out dustOnKillBonus);
        _playerDustOnKillUpgrade.TryGetValue(heroNetId, out dustUpgradeLevel);
        
        // Early exit if no bonuses
        if (goldOnKillBonus <= 0 && dustOnKillBonus <= 0) return;
        
        PickupManager pickupManager = NetworkedManagerBase<PickupManager>.instance;
        if (pickupManager == null) return;
        
        // Get base drop range based on monster type
        int minDrop, maxDrop;
        GetMonsterDropRange(monsterType, out minDrop, out maxDrop);
        
        // Gold on kill
        if (goldOnKillBonus > 0)
        {
            // Roll base gold based on monster type
            int baseGold = _random.Next(minDrop, maxDrop + 1) * goldOnKillBonus;
            // Upgrade scaling: uses configurable scaling from UpgradeSystem
            float upgradeMultiplier = InventoryMerchantHelper.GetUpgradeMultiplier(goldUpgradeLevel);
            int goldAmount = (int)(baseGold * upgradeMultiplier);
            
            if (goldAmount > 0)
            {
                pickupManager.DropGold(false, false, goldAmount, dropPosition, killerHero);
                
                // Track for scoring
                ScoringPatches.TrackGoldFromMidas(goldAmount);
            }
        }
        
        // Dust on kill
        if (dustOnKillBonus > 0)
        {
            // Get dust-specific drop range
            int dustMinDrop, dustMaxDrop;
            GetDustDropRange(monsterType, out dustMinDrop, out dustMaxDrop);
            
            int baseDust = _random.Next(dustMinDrop, dustMaxDrop + 1) * dustOnKillBonus;
            // Upgrade scaling: uses configurable scaling from UpgradeSystem
            float upgradeMultiplier = InventoryMerchantHelper.GetUpgradeMultiplier(dustUpgradeLevel);
            int dustAmount = (int)(baseDust * upgradeMultiplier);
            
            if (dustAmount > 0)
            {
                pickupManager.DropDreamDust(false, dustAmount, dropPosition, killerHero);
            }
        }
    }
    
    /// <summary>
    /// Get drop range based on monster type
    /// Lesser: 1-3 (less than base)
    /// Normal: 1-5 (base, as displayed in tooltip)
    /// MiniBoss: 3-8 (bonus)
    /// Boss: 5-15 (big bonus)
    /// </summary>
    private static void GetMonsterDropRange(Monster.MonsterType monsterType, out int minDrop, out int maxDrop)
    {
        // Get config values for Normal monster drops
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        int normalMin = config != null ? config.goldOnKillMinNormal : 1;
        int normalMax = config != null ? config.goldOnKillMaxNormal : 5;
        
        switch (monsterType)
        {
            case Monster.MonsterType.Lesser:
                // Lesser gets 60% of normal range
                minDrop = Mathf.Max(1, normalMin);
                maxDrop = Mathf.Max(1, (int)(normalMax * 0.6f));
                break;
            case Monster.MonsterType.Normal:
                minDrop = normalMin;
                maxDrop = normalMax;
                break;
            case Monster.MonsterType.MiniBoss:
                // MiniBoss gets 160% of normal range
                minDrop = (int)(normalMin * 1.6f);
                maxDrop = (int)(normalMax * 1.6f);
                break;
            case Monster.MonsterType.Boss:
                // Boss gets 300% of normal range
                minDrop = normalMin * 3;
                maxDrop = normalMax * 3;
                break;
            default:
                minDrop = normalMin;
                maxDrop = normalMax;
                break;
        }
    }
    
    /// <summary>
    /// Get dust drop range based on monster type (uses config)
    /// </summary>
    private static void GetDustDropRange(Monster.MonsterType monsterType, out int minDrop, out int maxDrop)
    {
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        int dustMin = config != null ? config.dustOnKillMin : 5;
        int dustMax = config != null ? config.dustOnKillMax : 30;
        
        switch (monsterType)
        {
            case Monster.MonsterType.Lesser:
                minDrop = Mathf.Max(1, dustMin);
                maxDrop = Mathf.Max(1, (int)(dustMax * 0.6f));
                break;
            case Monster.MonsterType.Normal:
                minDrop = dustMin;
                maxDrop = dustMax;
                break;
            case Monster.MonsterType.MiniBoss:
                minDrop = (int)(dustMin * 1.6f);
                maxDrop = (int)(dustMax * 1.6f);
                break;
            case Monster.MonsterType.Boss:
                minDrop = dustMin * 3;
                maxDrop = dustMax * 3;
                break;
            default:
                minDrop = dustMin;
                maxDrop = dustMax;
                break;
        }
    }
    
    private static Vector3 GetRandomDropPosition(Vector3 basePosition)
    {
        Vector3 dropPos = basePosition;
        dropPos.x += UnityEngine.Random.Range(-1f, 1f);
        dropPos.z += UnityEngine.Random.Range(-1f, 1f);
        return Dew.GetValidAgentDestination_LinearSweep(basePosition, dropPos);
    }
    
    private static int GetPotionDropAmount(Monster.MonsterType monsterType)
    {
        switch (monsterType)
        {
            case Monster.MonsterType.Lesser:
                return 1;
            case Monster.MonsterType.Normal:
                return _random.Next(1, 3); // 1-2
            case Monster.MonsterType.MiniBoss:
                return _random.Next(2, 4); // 2-3
            case Monster.MonsterType.Boss:
                return _random.Next(3, 6); // 3-5
            default:
                return 1;
        }
    }
    
    private static RPGItem SelectPotionForDrop(ItemRarity rarity)
    {
        // Filter potions by rarity
        List<RPGItem> matchingPotions = new List<RPGItem>();
        foreach (RPGItem potion in _consumablePools)
        {
            if (potion.rarity == rarity)
            {
                matchingPotions.Add(potion);
            }
        }
        
        // Fallback to any potion if no match
        if (matchingPotions.Count == 0)
        {
            // Try lower rarities
            foreach (RPGItem potion in _consumablePools)
            {
                if (potion.rarity <= rarity)
                {
                    matchingPotions.Add(potion);
                }
            }
        }
        
        if (matchingPotions.Count == 0 && _consumablePools.Count > 0)
        {
            return _consumablePools[_random.Next(_consumablePools.Count)];
        }
        
        if (matchingPotions.Count > 0)
        {
            return matchingPotions[_random.Next(matchingPotions.Count)];
        }
        
        return null;
    }
    
    /// <summary>
    /// Select potion rarity - heavily favors Common potions
    /// </summary>
    private static ItemRarity SelectPotionRarity(Monster.MonsterType monsterType)
    {
        float roll = (float)_random.NextDouble();
        
        switch (monsterType)
        {
            case Monster.MonsterType.Lesser:
                // 100% Common
                return ItemRarity.Common;
                
            case Monster.MonsterType.Normal:
                // 90% Common, 10% Rare
                if (roll < 0.10f) return ItemRarity.Rare;
                return ItemRarity.Common;
                
            case Monster.MonsterType.MiniBoss:
                // 70% Common, 25% Rare, 5% Epic
                if (roll < 0.05f) return ItemRarity.Epic;
                if (roll < 0.30f) return ItemRarity.Rare;
                return ItemRarity.Common;
                
            case Monster.MonsterType.Boss:
                // 50% Common, 35% Rare, 12% Epic, 3% Legendary
                if (roll < 0.03f) return ItemRarity.Legendary;
                if (roll < 0.15f) return ItemRarity.Epic;
                if (roll < 0.50f) return ItemRarity.Rare;
                return ItemRarity.Common;
                
            default:
                return ItemRarity.Common;
        }
    }
    
    private static ItemRarity SelectRarityForMonster(Monster.MonsterType monsterType, bool isPotion = false)
    {
        // For Lesser and Normal monsters: only Common and Rare drops
        // Epic and Legendary are reserved for MiniBoss and Boss
        // This only affects our custom RPG items, NOT game's shrine memories/essences
        
        if (monsterType == Monster.MonsterType.Lesser || monsterType == Monster.MonsterType.Normal)
        {
            // Use config weights for Common/Rare only
            return SelectRarityWithConfigWeights(true);
        }
        
        // MiniBoss and Boss can drop all rarities using config weights
        return SelectRarityWithConfigWeights(false);
    }
    
    /// <summary>
    /// Select rarity using config weights
    /// </summary>
    private static ItemRarity SelectRarityWithConfigWeights(bool commonRareOnly)
    {
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        
        int commonW = config != null ? config.commonWeight : 60;
        int rareW = config != null ? config.rareWeight : 30;
        int epicW = config != null ? config.epicWeight : 8;
        int legendaryW = config != null ? config.legendaryWeight : 2;
        
        // Debug: Log config values (only first few times to avoid spam)
        if (config != null && UnityEngine.Random.value < 0.01f) // 1% chance to log
        {
            RPGLog.Debug(string.Format("[Loot] Rarity weights - Common: {0}, Rare: {1}, Epic: {2}, Legendary: {3}", 
                commonW, rareW, epicW, legendaryW));
        }
        
        int totalWeight;
        if (commonRareOnly)
        {
            // Only Common and Rare for lesser/normal monsters
            totalWeight = commonW + rareW;
        }
        else
        {
            // All rarities for MiniBoss/Boss
            totalWeight = commonW + rareW + epicW + legendaryW;
        }
        
        if (totalWeight <= 0) return ItemRarity.Common;
        
        int roll = _random.Next(totalWeight);
        
        if (commonRareOnly)
        {
            // Common or Rare only
            if (roll < rareW) return ItemRarity.Rare;
            return ItemRarity.Common;
        }
        else
        {
            // All rarities - use cumulative ranges
            // Legendary: 0 to legendaryW-1
            // Epic: legendaryW to legendaryW+epicW-1
            // Rare: legendaryW+epicW to legendaryW+epicW+rareW-1
            // Common: legendaryW+epicW+rareW to totalWeight-1
            
            int legendaryEnd = legendaryW;
            int epicEnd = legendaryEnd + epicW;
            int rareEnd = epicEnd + rareW;
            
            if (roll < legendaryEnd) return ItemRarity.Legendary;
            if (roll < epicEnd) return ItemRarity.Epic;
            if (roll < rareEnd) return ItemRarity.Rare;
            return ItemRarity.Common;
        }
    }
    
    private static RPGItem SelectItemForDrop(ItemRarity rarity, Hero killerHero)
    {
        // Step 1: Roll for category (equal chance: Weapons, Armor, Accessories)
        LootCategory category = SelectRandomCategory();
        
        // Step 2: Get the pool for this category and rarity
        List<RPGItem> pool = GetCategoryPool(category, rarity);
        
        // If pool is empty, try other rarities in this category
        if (pool == null || pool.Count == 0)
        {
            pool = GetFallbackCategoryPool(category, rarity);
        }
        
        // If still empty, try another category
        if (pool == null || pool.Count == 0)
        {
            // Fallback to any available pool
            if (_allLootPool.Count > 0)
            {
                return _allLootPool[_random.Next(_allLootPool.Count)];
            }
            return null;
        }
        
        // Step 3: Filter by hero if heroOnlyDrops is enabled
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        bool heroOnly = config != null && config.heroOnlyDrops;
        
        if (heroOnly && killerHero != null)
        {
            string heroTypeName = killerHero.GetType().Name;
            List<RPGItem> filteredPool = new List<RPGItem>();
            
            foreach (RPGItem item in pool)
            {
                // Include items with no hero restriction OR items for this hero
                // Use HeroMatchesRequirement to handle Shell/Husk equivalence
                if (string.IsNullOrEmpty(item.requiredHero) || RPGItem.HeroMatchesRequirement(heroTypeName, item.requiredHero))
                {
                    filteredPool.Add(item);
                }
            }
            
            // Use filtered pool if it has items, otherwise fall back to full pool
            if (filteredPool.Count > 0)
            {
                pool = filteredPool;
            }
        }
        
        // Step 4: Select random item from the pool
        return pool[_random.Next(pool.Count)];
    }
    
    /// <summary>
    /// Roll for a random loot category (equal chance for each)
    /// </summary>
    private static LootCategory SelectRandomCategory()
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var weight in _categoryWeights.Values)
        {
            totalWeight += weight;
        }
        
        // Roll
        float roll = (float)_random.NextDouble() * totalWeight;
        float cumulative = 0f;
        
        foreach (var kvp in _categoryWeights)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
            {
                return kvp.Key;
            }
        }
        
        return LootCategory.Accessories; // Fallback
    }
    
    /// <summary>
    /// Get the item pool for a specific category and rarity
    /// </summary>
    private static List<RPGItem> GetCategoryPool(LootCategory category, ItemRarity rarity)
    {
        Dictionary<ItemRarity, List<RPGItem>> pools;
        
        switch (category)
        {
            case LootCategory.Weapons:
                pools = _weaponPools;
                break;
            case LootCategory.Armor:
                pools = _armorPools;
                break;
            case LootCategory.Accessories:
            default:
                pools = _accessoryPools;
                break;
        }
        
        if (pools.ContainsKey(rarity) && pools[rarity].Count > 0)
        {
            return pools[rarity];
        }
        
        return null;
    }
    
    /// <summary>
    /// Get a fallback pool if the requested rarity is empty
    /// </summary>
    private static List<RPGItem> GetFallbackCategoryPool(LootCategory category, ItemRarity requestedRarity)
    {
        Dictionary<ItemRarity, List<RPGItem>> pools;
        
        switch (category)
        {
            case LootCategory.Weapons:
                pools = _weaponPools;
                break;
            case LootCategory.Armor:
                pools = _armorPools;
                break;
            case LootCategory.Accessories:
            default:
                pools = _accessoryPools;
                break;
        }
        
        // Try adjacent rarities
        ItemRarity[] fallbackOrder;
        switch (requestedRarity)
        {
            case ItemRarity.Common:
                fallbackOrder = new[] { ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary };
                break;
            case ItemRarity.Rare:
                fallbackOrder = new[] { ItemRarity.Common, ItemRarity.Epic, ItemRarity.Legendary };
                break;
            case ItemRarity.Epic:
                fallbackOrder = new[] { ItemRarity.Rare, ItemRarity.Legendary, ItemRarity.Common };
                break;
            case ItemRarity.Legendary:
                fallbackOrder = new[] { ItemRarity.Epic, ItemRarity.Rare, ItemRarity.Common };
                break;
            default:
                fallbackOrder = new[] { ItemRarity.Common, ItemRarity.Rare, ItemRarity.Epic };
                break;
        }
        
        foreach (ItemRarity rarity in fallbackOrder)
        {
            if (pools.ContainsKey(rarity) && pools[rarity].Count > 0)
            {
                return pools[rarity];
            }
        }
        
        return null;
    }
    
    
    private static void SpawnMonsterLoot(RPGItem item, Vector3 position, Monster.MonsterType monsterType, uint targetPlayerNetId, bool isPrivate)
    {
        // Generate a unique drop ID for server-spawned items
        uint dropId = GenerateServerDropId();
        
        if (isPrivate && targetPlayerNetId != 0)
        {
            // Create private drop for the killer only
            NetworkedItemSystem.CreatePrivateDropForPlayer(item, position, dropId, targetPlayerNetId);
        }
        else
        {
            // Create shared drop (anyone can pick it up)
            NetworkedItemSystem.CreateSharedDrop(item, position, dropId);
        }
    }
    
    private static uint _serverDropIdCounter = 0x80000000; // Start high to avoid collision with player drops
    
    private static uint GenerateServerDropId()
    {
        return _serverDropIdCounter++;
    }
    
    /// <summary>
    /// Manually trigger a loot drop (for testing or special events)
    /// </summary>
    public static void ForceDrop(Vector3 position, ItemRarity rarity)
    {
        if (!NetworkServer.active)
        {
            RPGLog.Warning(" ForceDrop can only be called on server");
            return;
        }
        
        RPGItem item = SelectItemForDrop(rarity, null);
        if (item != null)
        {
            RPGItem clone = item.Clone();
            // Force drops are shared (no specific killer)
            SpawnMonsterLoot(clone, position, Monster.MonsterType.Normal, 0, false);
        }
    }
    
    /// <summary>
    /// Update drop chances at runtime (DEPRECATED - use config instead)
    /// </summary>
    public static void SetDropChance(Monster.MonsterType type, float chance)
    {
        // Drop chances are now controlled by config
        // This method is kept for backwards compatibility but does nothing
        RPGLog.Warning(" SetDropChance is deprecated. Use mod config to change drop rates.");
    }
    
    /// <summary>
    /// Update whether a monster type uses "high" rarity (like empowered shrines)
    /// </summary>
    public static void SetUseHighRarity(Monster.MonsterType type, bool useHigh)
    {
        _useHighRarity[type] = useHigh;
    }
    
    // ============================================================
    // DIABLO-STYLE STAT RANDOMIZATION
    // ============================================================
    
    /// <summary>
    /// Randomize item stats within a range based on base values.
    /// Higher rarity items get better stat rolls on average.
    /// </summary>
    public static void RandomizeItemStats(RPGItem item)
    {
        if (item == null || item.type == ItemType.Consumable) return;
        
        // Get the variance range based on rarity
        // Common: 70%-100%, Rare: 80%-110%, Epic: 90%-120%, Legendary: 100%-130%
        float minMultiplier, maxMultiplier;
        GetRarityMultipliers(item.rarity, out minMultiplier, out maxMultiplier);
        
        // Randomize BASE STATS (these are the upgradeable ones)
        item.attackBonus = RandomizeStat(item.attackBonus, minMultiplier, maxMultiplier);
        item.defenseBonus = RandomizeStat(item.defenseBonus, minMultiplier, maxMultiplier);
        item.healthBonus = RandomizeStat(item.healthBonus, minMultiplier, maxMultiplier);
        item.abilityPowerBonus = RandomizeStat(item.abilityPowerBonus, minMultiplier, maxMultiplier);
        item.criticalChance = RandomizeStatFloat(item.criticalChance, minMultiplier, maxMultiplier);
        item.criticalDamage = RandomizeStatFloat(item.criticalDamage, minMultiplier, maxMultiplier);
        
        // Randomize GEAR EFFECTS (fixed effects, still randomized on drop)
        item.moveSpeedBonus = RandomizeStatFloat(item.moveSpeedBonus, minMultiplier, maxMultiplier);
        item.attackSpeed = RandomizeStatFloat(item.attackSpeed, minMultiplier, maxMultiplier);
        item.memoryHaste = RandomizeStatFloat(item.memoryHaste, minMultiplier, maxMultiplier);
        item.lifeSteal = RandomizeStatFloat(item.lifeSteal, minMultiplier, maxMultiplier);
        item.thorns = RandomizeStatFloat(item.thorns, minMultiplier, maxMultiplier);
        item.regeneration = RandomizeStatFloat(item.regeneration, minMultiplier, maxMultiplier);
        
        // On-hit effects - use integer randomization
        item.goldOnKill = RandomizeStat(item.goldOnKill, minMultiplier, maxMultiplier);
        item.dustOnKill = RandomizeStat(item.dustOnKill, minMultiplier, maxMultiplier);
        item.dodgeChargesBonus = RandomizeStat(item.dodgeChargesBonus, minMultiplier, maxMultiplier);
        
        // Elemental stacks can vary slightly
        if (item.elementalStacks > 0)
        {
            item.elementalStacks = RandomizeStat(item.elementalStacks, minMultiplier, maxMultiplier);
            if (item.elementalStacks < 1) item.elementalStacks = 1; // Minimum 1 stack
        }
    }
    
    /// <summary>
    /// Roll for pre-upgraded item based on monster type.
    /// Higher tier monsters have better chances for upgraded drops.
    /// </summary>
    public static void RollForUpgradedDrop(RPGItem item, Monster.MonsterType monsterType)
    {
        if (item == null || item.type == ItemType.Consumable) return;
        
        // Get upgrade chances based on monster type
        // Format: [chance for +1, chance for +2, chance for +3, etc.]
        float[] upgradeChances;
        
        switch (monsterType)
        {
            case Monster.MonsterType.Lesser:
                // Lesser: 5% for +1, that's it
                upgradeChances = new float[] { 0.05f };
                break;
            case Monster.MonsterType.Normal:
                // Normal: 10% for +1, 2% for +2
                upgradeChances = new float[] { 0.10f, 0.02f };
                break;
            case Monster.MonsterType.MiniBoss:
                // MiniBoss: 25% for +1, 10% for +2, 3% for +3
                upgradeChances = new float[] { 0.25f, 0.10f, 0.03f };
                break;
            case Monster.MonsterType.Boss:
                // Boss: 40% for +1, 20% for +2, 8% for +3, 2% for +4
                upgradeChances = new float[] { 0.40f, 0.20f, 0.08f, 0.02f };
                break;
            default:
                upgradeChances = new float[] { 0.05f };
                break;
        }
        
        // Roll for each upgrade level (cumulative - must pass each check)
        int upgradeLevel = 0;
        for (int i = 0; i < upgradeChances.Length; i++)
        {
            float roll = (float)_random.NextDouble();
            if (roll <= upgradeChances[i])
            {
                upgradeLevel = i + 1;
            }
            else
            {
                break; // Failed this level, stop checking higher levels
            }
        }
        
        // Apply upgrades if any
        if (upgradeLevel > 0)
        {
            item.upgradeLevel = upgradeLevel;
            
            // Apply the stat upgrades (same formula as manual upgrades)
            ApplyUpgradeStats(item, upgradeLevel);
            
            RPGLog.Debug(string.Format(" Item {0} dropped with +{1} upgrade!", item.name, upgradeLevel));
        }
    }
    
    /// <summary>
    /// Apply upgrade stat bonuses to an item (matches the upgrade system formula)
    /// Uses configurable scaling from UpgradeSystem
    /// </summary>
    private static void ApplyUpgradeStats(RPGItem item, int levels)
    {
        // Use the configurable upgrade multiplier from UpgradeSystem
        float upgradeMultiplier = InventoryMerchantHelper.GetUpgradeMultiplier(levels);
        
        // Apply to base stats only (gear effects stay fixed)
        if (item.attackBonus > 0)
            item.attackBonus = Mathf.RoundToInt(item.attackBonus * upgradeMultiplier);
        if (item.defenseBonus > 0)
            item.defenseBonus = Mathf.RoundToInt(item.defenseBonus * upgradeMultiplier);
        if (item.healthBonus > 0)
            item.healthBonus = Mathf.RoundToInt(item.healthBonus * upgradeMultiplier);
        if (item.abilityPowerBonus > 0)
            item.abilityPowerBonus = Mathf.RoundToInt(item.abilityPowerBonus * upgradeMultiplier);
        if (item.criticalChance > 0)
            item.criticalChance = Mathf.Round(item.criticalChance * upgradeMultiplier * 10f) / 10f;
        if (item.criticalDamage > 0)
            item.criticalDamage = Mathf.Round(item.criticalDamage * upgradeMultiplier * 10f) / 10f;
    }
    
    /// <summary>
    /// Get multiplier range based on rarity
    /// All items have at least 100% base stats, higher rarity = higher bonus potential
    /// </summary>
    private static void GetRarityMultipliers(ItemRarity rarity, out float min, out float max)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                min = 1.00f; max = 1.10f; // 100%-110% of base
                break;
            case ItemRarity.Rare:
                min = 1.00f; max = 1.20f; // 100%-120% of base
                break;
            case ItemRarity.Epic:
                min = 1.00f; max = 1.30f; // 100%-130% of base
                break;
            case ItemRarity.Legendary:
                min = 1.00f; max = 1.50f; // 100%-150% of base (best potential!)
                break;
            default:
                min = 1.00f; max = 1.10f;
                break;
        }
    }
    
    /// <summary>
    /// Randomize an integer stat within the multiplier range
    /// </summary>
    private static int RandomizeStat(int baseStat, float minMult, float maxMult)
    {
        if (baseStat == 0) return 0;
        
        // Calculate range
        float multiplier = minMult + (float)_random.NextDouble() * (maxMult - minMult);
        int result = Mathf.RoundToInt(baseStat * multiplier);
        
        // Ensure at least 1 if base was positive
        if (baseStat > 0 && result < 1) result = 1;
        
        return result;
    }
    
    /// <summary>
    /// Randomize a float stat within the multiplier range
    /// </summary>
    private static float RandomizeStatFloat(float baseStat, float minMult, float maxMult)
    {
        if (Mathf.Approximately(baseStat, 0f)) return 0f;
        
        // Calculate range
        float multiplier = minMult + (float)_random.NextDouble() * (maxMult - minMult);
        float result = baseStat * multiplier;
        
        // Round to 1 decimal place for cleaner display
        result = Mathf.Round(result * 10f) / 10f;
        
        // Ensure at least 0.1 if base was positive
        if (baseStat > 0f && result < 0.1f) result = 0.1f;
        
        return result;
    }
}

