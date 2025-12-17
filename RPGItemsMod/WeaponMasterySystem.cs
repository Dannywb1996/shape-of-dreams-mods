using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

/// <summary>
/// Weapon Mastery System - Heroes gain proficiency with RPGItemsMod custom weapons through use
/// Each hero has their own mastery levels for SPECIFIC custom weapons they've used
/// Mastery provides bonuses like increased damage, crit chance, attack speed
/// ONLY applies to custom RPGItemsMod items, NOT base game weapons!
/// </summary>
public class WeaponMasterySystem
{
    // ============================================================
    // MASTERY CONFIGURATION (configurable via ModConfig)
    // ============================================================
    
    // Experience formula: XP needed = BASE_XP * (GROWTH_RATE ^ level)
    // This allows unlimited levels with scaling difficulty
    public const int BASE_XP_FOR_LEVEL = 100;      // XP for level 1
    public const float XP_GROWTH_RATE = 1.15f;     // 15% more XP needed per level
    
    // No max level - unlimited mastery!
    // Soft cap where bonuses become less impactful but still grow
    public const int SOFT_CAP_LEVEL = 50;
    
    // XP gained per action (configurable)
    public static int XP_PER_BASIC_ATTACK 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.masteryXpPerHit : 1;
        } 
    }
    public const int XP_PER_KILL = 5;
    public const int XP_PER_BOSS_KILL = 25;
    
    // Crit XP multiplier (configurable)
    public static float CRIT_XP_MULTIPLIER 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.masteryCritMultiplier : 1.5f;
        } 
    }
    
    // Bonuses per mastery level (configurable, scales with diminishing returns after soft cap)
    public static float DAMAGE_BONUS_PER_LEVEL 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.masteryDamagePerLevel : 2f;
        } 
    }
    
    public static float CRIT_CHANCE_PER_LEVEL 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.masteryCritPerLevel : 0.5f;
        } 
    }
    
    public const float ATTACK_SPEED_PER_LEVEL = 1f;      // +1% attack speed per level (not configurable)
    
    // After soft cap, bonuses are reduced by this factor per level above cap
    public const float DIMINISHING_RETURNS_FACTOR = 0.95f;
    
    // ============================================================
    // MASTERY DATA
    // ============================================================
    
    [Serializable]
    public class MasteryData
    {
        public string weaponName;     // The actual weapon name (e.g., "Flame Blade", "Ice Staff")
        public string weaponId;       // Unique identifier (name without spaces, lowercase)
        public int itemId;            // Item ID for translation lookup
        public int currentXP;
        public int level;
        
        public MasteryData(string name, string id, int itemId = 0)
        {
            weaponName = name;
            weaponId = id;
            this.itemId = itemId;
            currentXP = 0;
            level = 0;
        }
        
        /// <summary>
        /// Get translated weapon name based on current language
        /// </summary>
        public string GetDisplayName()
        {
            if (itemId > 0)
            {
                string translated = ItemTranslations.GetName(itemId);
                if (!string.IsNullOrEmpty(translated))
                    return translated;
            }
            return weaponName;
        }
    }
    
    // Per-hero mastery data: heroId -> (weaponId -> masteryData)
    // Each hero tracks their own progress with each custom weapon they've used
    private Dictionary<string, Dictionary<string, MasteryData>> _heroMasteries = 
        new Dictionary<string, Dictionary<string, MasteryData>>();
    
    // Currently applied stat bonus
    private StatBonus _appliedMasteryBonus;
    private Hero _bonusAppliedToHero;
    private string _currentlyAppliedWeaponId;
    
    // Events
    public Action<string, string, int> OnMasteryLevelUp; // heroId, weaponName, newLevel
    public Action OnMasteryXPGained;
    
    // Track attacks for XP
    private bool _isSubscribedToAttacks = false;
    private Hero _subscribedHero = null;
    
    private EquipmentManager _equipmentManager;
    
    // Flag to track if mastery bonus needs to be reapplied (hero wasn't ready during load)
    private bool _needsMasteryReapply = false;
    
    // ============================================================
    // INITIALIZATION
    // ============================================================
    
    public void Initialize()
    {
        RPGLog.Debug(" Weapon Mastery System initialized (Custom Items Only)");
    }
    
    public void Cleanup()
    {
        UnsubscribeFromAttacks();
        RemoveMasteryBonus();
        RPGLog.Debug(" Weapon Mastery System cleaned up");
    }
    
    public void SetEquipmentManager(EquipmentManager manager)
    {
        _equipmentManager = manager;
    }
    
    // ============================================================
    // HERO MASTERY MANAGEMENT
    // ============================================================
    
    /// <summary>
    /// Get or create mastery data dictionary for a hero
    /// </summary>
    private Dictionary<string, MasteryData> GetHeroMasteries(string heroId)
    {
        if (!_heroMasteries.ContainsKey(heroId))
        {
            _heroMasteries[heroId] = new Dictionary<string, MasteryData>();
        }
        return _heroMasteries[heroId];
    }
    
    /// <summary>
    /// Get weapon ID from weapon name (normalized for storage)
    /// </summary>
    private string GetWeaponId(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName)) return null;
        return weaponName.ToLower().Replace(" ", "_").Replace("'", "");
    }
    
    /// <summary>
    /// Get or create mastery data for a specific weapon
    /// </summary>
    private MasteryData GetOrCreateMasteryData(string heroId, string weaponName, int itemId = 0)
    {
        var masteries = GetHeroMasteries(heroId);
        string weaponId = GetWeaponId(weaponName);
        
        if (!masteries.ContainsKey(weaponId))
        {
            masteries[weaponId] = new MasteryData(weaponName, weaponId, itemId);
        }
        else if (masteries[weaponId].itemId == 0 && itemId > 0)
        {
            // Update item ID if it wasn't set before (for legacy data)
            masteries[weaponId].itemId = itemId;
        }
        return masteries[weaponId];
    }
    
    /// <summary>
    /// Get mastery level for a specific weapon
    /// </summary>
    public int GetMasteryLevel(string heroId, string weaponName)
    {
        var masteries = GetHeroMasteries(heroId);
        string weaponId = GetWeaponId(weaponName);
        
        if (masteries.ContainsKey(weaponId))
        {
            return masteries[weaponId].level;
        }
        return 0;
    }
    
    /// <summary>
    /// Get current XP for a weapon
    /// </summary>
    public int GetMasteryXP(string heroId, string weaponName)
    {
        var masteries = GetHeroMasteries(heroId);
        string weaponId = GetWeaponId(weaponName);
        
        if (masteries.ContainsKey(weaponId))
        {
            return masteries[weaponId].currentXP;
        }
        return 0;
    }
    
    /// <summary>
    /// Get XP needed for a specific level (cumulative)
    /// Uses exponential growth formula for unlimited scaling
    /// </summary>
    public int GetXPForLevel(int level)
    {
        if (level <= 0) return 0;
        // Cumulative XP = BASE * ((GROWTH^level - 1) / (GROWTH - 1))
        float totalXP = BASE_XP_FOR_LEVEL * ((Mathf.Pow(XP_GROWTH_RATE, level) - 1f) / (XP_GROWTH_RATE - 1f));
        return (int)totalXP;
    }
    
    /// <summary>
    /// Get XP needed for next level
    /// </summary>
    public int GetXPForNextLevel(int currentLevel)
    {
        return GetXPForLevel(currentLevel + 1);
    }
    
    /// <summary>
    /// Get XP needed just for this level (not cumulative)
    /// </summary>
    public int GetXPForLevelOnly(int level)
    {
        if (level <= 0) return 0;
        return GetXPForLevel(level) - GetXPForLevel(level - 1);
    }
    
    /// <summary>
    /// Get all mastery data for a hero (all weapons they've used)
    /// </summary>
    public List<MasteryData> GetAllMasteries(string heroId)
    {
        var masteries = GetHeroMasteries(heroId);
        return new List<MasteryData>(masteries.Values);
    }
    
    /// <summary>
    /// Get mastery data for a specific weapon
    /// </summary>
    public MasteryData GetMastery(string heroId, string weaponName)
    {
        var masteries = GetHeroMasteries(heroId);
        string weaponId = GetWeaponId(weaponName);
        
        if (masteries.ContainsKey(weaponId))
        {
            return masteries[weaponId];
        }
        return null;
    }
    
    /// <summary>
    /// Get currently equipped custom weapon name (if any)
    /// </summary>
    public string GetEquippedCustomWeaponName()
    {
        RPGItem weapon = GetEquippedCustomWeapon();
        return weapon != null ? weapon.name : null;
    }
    
    /// <summary>
    /// Get currently equipped custom weapon item (if any)
    /// </summary>
    public RPGItem GetEquippedCustomWeapon()
    {
        if (_equipmentManager == null) return null;
        
        // Check right hand (main hand / two-handed weapon slot)
        RPGItem rightHand = _equipmentManager.GetEquippedItem(EquipmentSlotType.RightHand);
        if (rightHand != null && IsWeaponItem(rightHand))
        {
            return rightHand;
        }
        
        // Check left hand (off-hand or two-handed weapon)
        RPGItem leftHand = _equipmentManager.GetEquippedItem(EquipmentSlotType.LeftHand);
        if (leftHand != null && IsWeaponItem(leftHand))
        {
            return leftHand;
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if an RPGItem is a weapon (not armor, accessory, etc.)
    /// </summary>
    private bool IsWeaponItem(RPGItem item)
    {
        if (item == null) return false;
        return item.type == ItemType.Weapon || item.type == ItemType.TwoHandedWeapon;
    }
    
    // ============================================================
    // XP GAIN
    // ============================================================
    
    /// <summary>
    /// Add XP to a weapon mastery
    /// </summary>
    public void AddMasteryXP(string heroId, string weaponName, int xp, int itemId = 0)
    {
        if (string.IsNullOrEmpty(weaponName)) return;
        
        MasteryData data = GetOrCreateMasteryData(heroId, weaponName, itemId);
        
        int oldLevel = data.level;
        data.currentXP += xp;
        
        // Check for level up (unlimited levels)
        while (data.currentXP >= GetXPForLevel(data.level + 1))
        {
            data.level++;
            RPGLog.Debug(string.Format(" {0}'s mastery of '{1}' leveled up to {2}!", 
                heroId, weaponName, data.level));
            
            if (OnMasteryLevelUp != null)
            {
                OnMasteryLevelUp(heroId, weaponName, data.level);
            }
        }
        
        // Reapply bonuses if level changed
        if (data.level != oldLevel)
        {
            ApplyMasteryBonus();
        }
        
        if (OnMasteryXPGained != null)
        {
            OnMasteryXPGained();
        }
    }
    
    // ============================================================
    // ATTACK TRACKING
    // ============================================================
    
    /// <summary>
    /// Subscribe to attack events to gain XP.
    /// IMPORTANT: We subscribe to ClientEventManager.OnAttackHit because:
    /// - Entity.EntityEvent_OnAttackHit is ONLY invoked on the server (inside DoBasicAttackHit which is [Server])
    /// - ClientEventManager subscribes to that event on the server and sends an RPC to clients
    /// - The RPC invokes ClientEventManager.OnAttackHit on clients
    /// So for clients to receive attack hit events, we MUST use ClientEventManager.OnAttackHit!
    /// </summary>
    public void SubscribeToAttacks(EquipmentManager equipmentManager)
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        
        if (_isSubscribedToAttacks && _subscribedHero == hero) return;
        
        UnsubscribeFromAttacks();
        
        _subscribedHero = hero;
        _equipmentManager = equipmentManager;
        
        // Subscribe to ClientEventManager.OnAttackHit - this is the ONLY way to get attack events on clients!
        // The server sends an RPC that invokes this event on all clients
        ClientEventManager clientEventManager = NetworkedManagerBase<ClientEventManager>.instance;
        if (clientEventManager != null)
        {
            clientEventManager.OnAttackHit += OnHeroAttackHit;
            _isSubscribedToAttacks = true;
            RPGLog.Debug(" Subscribed to ClientEventManager.OnAttackHit for weapon mastery (works on clients!)");
        }
        else
        {
            RPGLog.Warning(" ClientEventManager not available for mastery subscription!");
        }
    }
    
    public void UnsubscribeFromAttacks()
    {
        if (!_isSubscribedToAttacks) return;
        
        try
        {
            ClientEventManager clientEventManager = NetworkedManagerBase<ClientEventManager>.instance;
            if (clientEventManager != null)
            {
                clientEventManager.OnAttackHit -= OnHeroAttackHit;
            }
        }
        catch { }
        
        _isSubscribedToAttacks = false;
        _subscribedHero = null;
    }
    
    /// <summary>
    /// Called when our hero's attack hits something.
    /// This event fires on BOTH server and clients via RPC!
    /// Also handles summon attacks - if a summon owned by our hero attacks, we gain XP!
    /// </summary>
    private void OnHeroAttackHit(EventInfoAttackHit info)
    {
        if (_equipmentManager == null)
        {
            return;
        }
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        bool isOurAttack = false;
        bool isSummonAttack = false;
        
        // Check if WE are the attacker directly
        if (info.attacker == hero)
        {
            isOurAttack = true;
        }
        // Check if attacker is one of our summons (Nachia's summons, etc.)
        else
        {
            Summon summon = info.attacker as Summon;
            if (summon != null)
            {
                // Check if summons grant XP is enabled
                RPGItemsConfig config = RPGItemsMod.GetConfig();
                if (config != null && !config.summonsGrantMasteryXP)
                {
                    return; // Summon XP disabled
                }
                
                // Summon's hero property points to the hero that owns it
                if (summon.hero == hero)
                {
                    isOurAttack = true;
                    isSummonAttack = true;
                }
            }
        }
        
        if (!isOurAttack) return;
        
        string heroId = hero.GetType().Name;
        
        // Get equipped CUSTOM weapon (RPGItemsMod only!)
        RPGItem weapon = GetEquippedCustomWeapon();
        if (weapon == null) return; // No custom weapon equipped, no XP
        
        // Base XP for hitting something
        int xp = XP_PER_BASIC_ATTACK;
        
        // Bonus XP for critical hits
        if (info.isCrit)
        {
            xp = (int)(xp * 1.5f);
        }
        
        // Check if target died (give bonus XP)
        if (info.victim != null && info.victim.Status != null && info.victim.currentHealth <= 0)
        {
            Monster monster = info.victim as Monster;
            if (monster != null)
            {
                if (monster.type == Monster.MonsterType.Boss)
                {
                    xp = XP_PER_BOSS_KILL;
                }
                else if (monster.type == Monster.MonsterType.MiniBoss)
                {
                    xp = XP_PER_BOSS_KILL / 2;
                }
                else
                {
                    xp = XP_PER_KILL;
                }
            }
        }
        
        // Apply summon XP multiplier if this was a summon attack
        if (isSummonAttack)
        {
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            float multiplier = config != null ? config.summonMasteryXpMultiplier : 0.5f;
            xp = (int)(xp * multiplier);
            if (xp < 1) xp = 1; // Minimum 1 XP
        }
        
        AddMasteryXP(heroId, weapon.name, xp, weapon.id);
    }
    
    // ============================================================
    // STAT BONUSES
    // ============================================================
    
    /// <summary>
    /// Apply mastery bonuses based on equipped custom weapon
    /// </summary>
    public void ApplyMasteryBonus()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        if (_equipmentManager == null) return;
        
        Hero hero = DewPlayer.local.hero;
        string heroId = hero.GetType().Name;
        
        // Get equipped custom weapon
        string weaponName = GetEquippedCustomWeaponName();
        string weaponId = GetWeaponId(weaponName);
        
        // If same weapon already applied, skip
        if (_appliedMasteryBonus != null && _currentlyAppliedWeaponId == weaponId && _bonusAppliedToHero == hero)
        {
            return;
        }
        
        // Remove old bonus
        RemoveMasteryBonus();
        
        // No custom weapon? No bonus
        if (string.IsNullOrEmpty(weaponName)) return;
        
        // Get mastery level for this weapon
        int level = GetMasteryLevel(heroId, weaponName);
        if (level <= 0) return;
        
        // Calculate bonuses with diminishing returns after soft cap
        float dmgBonus = CalculateBonusWithDiminishingReturns(level, DAMAGE_BONUS_PER_LEVEL);
        float critBonus = CalculateBonusWithDiminishingReturns(level, CRIT_CHANCE_PER_LEVEL);
        float atkSpdBonus = CalculateBonusWithDiminishingReturns(level, ATTACK_SPEED_PER_LEVEL);
        
        // Apply based on host/client
        if (NetworkServer.active)
        {
            // HOST: Apply directly
            _appliedMasteryBonus = new StatBonus();
            _appliedMasteryBonus.attackDamagePercentage = dmgBonus;
            _appliedMasteryBonus.critChanceFlat = critBonus / 100f; // Convert to decimal
            _appliedMasteryBonus.attackSpeedPercentage = atkSpdBonus;
            
            hero.Status.AddStatBonus(_appliedMasteryBonus);
            _bonusAppliedToHero = hero;
            _currentlyAppliedWeaponId = weaponId;
            
            RPGLog.Debug(string.Format(" Host applied '{0}' mastery bonus (Lv{1}): +{2:F1}% DMG, +{3:F1}% Crit, +{4:F1}% AtkSpd",
                weaponName, level, dmgBonus, critBonus, atkSpdBonus));
        }
        else
        {
            // CLIENT: Request server to apply bonus
            RequestMasteryBonus(hero.netId, dmgBonus, critBonus, atkSpdBonus);
            _currentlyAppliedWeaponId = weaponId;
            
            RPGLog.Debug(string.Format(" Client requested '{0}' mastery bonus (Lv{1}): +{2:F1}% DMG, +{3:F1}% Crit, +{4:F1}% AtkSpd",
                weaponName, level, dmgBonus, critBonus, atkSpdBonus));
        }
    }
    
    /// <summary>
    /// Request server to apply mastery bonus (for clients)
    /// </summary>
    private void RequestMasteryBonus(uint heroNetId, float dmgBonus, float critBonus, float atkSpdBonus)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            // Format: [RPGMASTERY]heroNetId|dmgBonus|critBonus|atkSpdBonus
            string content = string.Format("[RPGMASTERY]{0}|{1}|{2}|{3}",
                heroNetId, dmgBonus.ToString("F2"), critBonus.ToString("F2"), atkSpdBonus.ToString("F2"));
            chatManager.CmdSendChatMessage(content, null);
        }
        catch (System.Exception e)
        {
            RPGLog.Warning(" Failed to send mastery bonus request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Calculate bonus with diminishing returns after soft cap
    /// </summary>
    private float CalculateBonusWithDiminishingReturns(int level, float bonusPerLevel)
    {
        if (level <= SOFT_CAP_LEVEL)
        {
            // Linear growth up to soft cap
            return level * bonusPerLevel;
        }
        else
        {
            // Full bonus up to soft cap
            float baseBonus = SOFT_CAP_LEVEL * bonusPerLevel;
            
            // Diminishing returns for levels above soft cap
            int levelsAboveCap = level - SOFT_CAP_LEVEL;
            float diminishedBonus = 0f;
            float currentMultiplier = 1f;
            
            for (int i = 0; i < levelsAboveCap; i++)
            {
                currentMultiplier *= DIMINISHING_RETURNS_FACTOR;
                diminishedBonus += bonusPerLevel * currentMultiplier;
            }
            
            return baseBonus + diminishedBonus;
        }
    }
    
    public void RemoveMasteryBonus()
    {
        if (_appliedMasteryBonus != null && _bonusAppliedToHero != null)
        {
            try
            {
                _bonusAppliedToHero.Status.RemoveStatBonus(_appliedMasteryBonus);
            }
            catch { }
        }
        _appliedMasteryBonus = null;
        _bonusAppliedToHero = null;
        _currentlyAppliedWeaponId = null;
    }
    
    /// <summary>
    /// Get mastery bonus description for UI
    /// </summary>
    public string GetMasteryBonusDescription(int level)
    {
        if (level <= 0) return "";
        
        // Calculate actual bonuses with diminishing returns
        float dmgBonus = CalculateBonusWithDiminishingReturns(level, DAMAGE_BONUS_PER_LEVEL);
        float critBonus = CalculateBonusWithDiminishingReturns(level, CRIT_CHANCE_PER_LEVEL);
        float atkSpdBonus = CalculateBonusWithDiminishingReturns(level, ATTACK_SPEED_PER_LEVEL);
        
        bool isChinese = Localization.CurrentLanguage == ModLanguage.Chinese;
        
        if (isChinese)
        {
            return string.Format("+{0:F1}% 伤害, +{1:F1}% 暴击, +{2:F1}% 攻速", dmgBonus, critBonus, atkSpdBonus);
        }
        else
        {
            return string.Format("+{0:F1}% DMG, +{1:F1}% Crit, +{2:F1}% AS", dmgBonus, critBonus, atkSpdBonus);
        }
    }
    
    // ============================================================
    // UPDATE
    // ============================================================
    
    public void Update()
    {
        // Check if mastery bonus needs to be reapplied (hero wasn't ready during load)
        CheckPendingMasteryReapply();
        
        // Periodically check if we need to subscribe to attacks
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            if (!_isSubscribedToAttacks || _subscribedHero != DewPlayer.local.hero)
            {
                RPGLog.Debug(" Mastery Update: Subscribing to attacks...");
                SubscribeToAttacks(_equipmentManager);
            }
        }
    }
    
    /// <summary>
    /// Check if mastery bonus needs to be reapplied (called from Update)
    /// </summary>
    public void CheckPendingMasteryReapply()
    {
        if (!_needsMasteryReapply) return;
        
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        RPGLog.Debug(" Hero now ready, applying saved mastery bonus...");
        _needsMasteryReapply = false;
        ApplyMasteryBonus();
    }
    
    /// <summary>
    /// Mark mastery bonus for reapply (used when loading data before hero is ready)
    /// </summary>
    public void MarkForReapply()
    {
        _needsMasteryReapply = true;
    }
    
    // ============================================================
    // PERSISTENCE
    // ============================================================
    
    /// <summary>
    /// Serialize all mastery data
    /// </summary>
    public string Serialize()
    {
        // Format: heroId:weaponId~weaponName~xp~level;weaponId~weaponName~xp~level|heroId:...
        List<string> heroData = new List<string>();
        
        foreach (var heroKvp in _heroMasteries)
        {
            List<string> masteryStrings = new List<string>();
            foreach (var masteryKvp in heroKvp.Value)
            {
                MasteryData data = masteryKvp.Value;
                // Use ~ as separator within weapon data to avoid conflicts
                masteryStrings.Add(string.Format("{0}~{1}~{2}~{3}", 
                    data.weaponId, data.weaponName, data.currentXP, data.level));
            }
            heroData.Add(heroKvp.Key + ":" + string.Join(";", masteryStrings.ToArray()));
        }
        
        return string.Join("|", heroData.ToArray());
    }
    
    /// <summary>
    /// Deserialize mastery data
    /// </summary>
    public void Deserialize(string data)
    {
        if (string.IsNullOrEmpty(data)) return;
        
        _heroMasteries.Clear();
        
        try
        {
            string[] heroEntries = data.Split('|');
            foreach (string heroEntry in heroEntries)
            {
                if (string.IsNullOrEmpty(heroEntry)) continue;
                
                string[] heroSplit = heroEntry.Split(':');
                if (heroSplit.Length < 2) continue;
                
                string heroId = heroSplit[0];
                _heroMasteries[heroId] = new Dictionary<string, MasteryData>();
                
                string[] masteryEntries = heroSplit[1].Split(';');
                foreach (string masteryEntry in masteryEntries)
                {
                    if (string.IsNullOrEmpty(masteryEntry)) continue;
                    
                    string[] parts = masteryEntry.Split('~');
                    if (parts.Length < 4) continue;
                    
                    string weaponId = parts[0];
                    string weaponName = parts[1];
                    int xp = int.Parse(parts[2]);
                    int level = int.Parse(parts[3]);
                    
                    MasteryData mData = new MasteryData(weaponName, weaponId);
                    mData.currentXP = xp;
                    mData.level = level;
                    
                    _heroMasteries[heroId][weaponId] = mData;
                }
            }
            
            RPGLog.Debug(" Loaded weapon mastery data");
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to load mastery data: " + e.Message);
        }
    }
    
    /// <summary>
    /// Reset mastery for new run - clears all mastery progress
    /// </summary>
    public void ResetForNewRun()
    {
        // Remove any applied bonuses
        RemoveMasteryBonus();
        
        // Clear all mastery data for fresh start each run
        _heroMasteries.Clear();
        
        // Clear the saved mastery file
        ItemPersistence.SaveMastery("");
        
        RPGLog.Debug(" Weapon mastery reset for new run");
    }
}
