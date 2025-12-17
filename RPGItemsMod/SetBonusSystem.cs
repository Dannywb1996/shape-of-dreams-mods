using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// Set Bonus System - Inspired by Diablo Immortal
/// Grants bonus effects when wearing multiple items of the same rarity
/// 2-piece, 4-piece, and 6-piece set bonuses
/// </summary>
public class SetBonusSystem
{
    private EquipmentManager _equipmentManager;
    
    // Current active set bonuses
    private Dictionary<ItemRarity, int> _equippedCountByRarity = new Dictionary<ItemRarity, int>();
    private Dictionary<ItemRarity, SetBonusData> _activeSetBonuses = new Dictionary<ItemRarity, SetBonusData>();
    
    // Stat bonuses applied to hero
    private StatBonus _currentSetStatBonus = null;
    
    // Set bonus definitions
    private static Dictionary<ItemRarity, SetBonusDefinition> _setBonusDefinitions;
    
    // Cooldown tracking for 6-piece proc effects
    private Dictionary<ItemRarity, float> _procCooldowns = new Dictionary<ItemRarity, float>();
    
    // Event subscriptions - using CLIENT-COMPATIBLE events!
    private bool _isSubscribedToEvents = false;
    private Action<EventInfoKill> _onKillHandler;           // For ClientHeroEvent_OnKillOrAssist
    private Action<EventInfoAttackHit> _onAttackHitHandler; // For ClientEventManager.OnAttackHit
    private Hero _subscribedHero;
    
    // Cheat Death tracking
    private bool _cheatDeathActive = false;
    private bool _cheatDeathRegistrationPending = false; // True if registration was deferred due to netId being 0
    
    // Pending reapply tracking (when hero isn't ready during load)
    private bool _needsReapply = false;
    
    public SetBonusSystem(EquipmentManager equipmentManager)
    {
        _equipmentManager = equipmentManager;
        InitializeSetBonusDefinitions();
        
        // Initialize cooldowns
        foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
        {
            _procCooldowns[rarity] = 0f;
        }
    }
    
    /// <summary>
    /// Check if set bonuses are enabled in config
    /// </summary>
    private static bool IsSetBonusEnabled()
    {
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        return config == null || config.enableSetBonuses; // Default to enabled
    }
    
    /// <summary>
    /// Initialize all set bonus definitions
    /// </summary>
    private static void InitializeSetBonusDefinitions()
    {
        if (_setBonusDefinitions != null) return;
        
        _setBonusDefinitions = new Dictionary<ItemRarity, SetBonusDefinition>();
        
        // Get config values for cooldowns
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        float commonCD = config != null ? config.setBonusProcCooldownCommon : 60f;
        float rareCD = config != null ? config.setBonusProcCooldownRare : 15f;
        float epicCD = config != null ? config.setBonusProcCooldownEpic : 40f;
        float legendaryCD = config != null ? config.setBonusProcCooldownLegendary : 40f;
        
        // ============================================================
        // COMMON SET - "Wanderer's Resolve" (流浪者的意志)
        // Theme: Basic survival bonuses for beginners
        // ============================================================
        _setBonusDefinitions[ItemRarity.Common] = new SetBonusDefinition
        {
            setName = "Wanderer's Resolve",
            setNameChinese = "流浪者的意志",
            rarity = ItemRarity.Common,
            
            // 2-piece: +50 Max Health, +5 Defense
            twoSetBonus = new SetBonus
            {
                description = "+50 Max Health, +5 Defense",
                descriptionChinese = "+50 最大生命值, +5 防御",
                healthBonus = 50,
                defenseBonus = 5
            },
            
            // 4-piece: +10% Health Regeneration, +10 Defense
            fourSetBonus = new SetBonus
            {
                description = "+3 HP/s Regeneration, +10 Defense",
                descriptionChinese = "+3 生命/秒 回复, +10 防御",
                regenerationBonus = 3f,
                defenseBonus = 10
            },
            
            // 6-piece: When taking fatal damage, survive with 1 HP and gain 100% max HP shield (configurable cooldown)
            sixSetBonus = new SetBonus
            {
                description = string.Format("Cheat Death: Survive fatal damage with 1 HP + 100% shield ({0}s CD)", commonCD),
                descriptionChinese = string.Format("死里逃生: 致命伤害时存活并获得100%护盾 ({0}秒冷却)", commonCD),
                hasProcEffect = true,
                procType = SetProcType.OnFatalDamage,
                procCooldown = commonCD
            }
        };
        
        // ============================================================
        // RARE SET - "Storm Chaser's Pursuit" (追风者的执念)
        // Theme: Speed and mobility bonuses
        // ============================================================
        _setBonusDefinitions[ItemRarity.Rare] = new SetBonusDefinition
        {
            setName = "Storm Chaser's Pursuit",
            setNameChinese = "追风者的执念",
            rarity = ItemRarity.Rare,
            
            // 2-piece: +15% Movement Speed
            twoSetBonus = new SetBonus
            {
                description = "+15% Movement Speed",
                descriptionChinese = "+15% 移动速度",
                moveSpeedBonus = 15f
            },
            
            // 4-piece: +20% Attack Speed, +10% Movement Speed
            fourSetBonus = new SetBonus
            {
                description = "+20% Attack Speed, +10% Movement Speed",
                descriptionChinese = "+20% 攻击速度, +10% 移动速度",
                attackSpeedBonus = 20f,
                moveSpeedBonus = 10f
            },
            
            // 6-piece: On kill, gain Wind Rush - +50% move speed for 3s (configurable cooldown)
            sixSetBonus = new SetBonus
            {
                description = string.Format("Wind Rush: On kill, +50% speed for 3s ({0}s CD)", rareCD),
                descriptionChinese = string.Format("疾风突袭: 击杀后+50%速度3秒 ({0}秒冷却)", rareCD),
                hasProcEffect = true,
                procType = SetProcType.OnKill,
                procCooldown = rareCD
            }
        };
        
        // ============================================================
        // EPIC SET - "Nightmare's Embrace" (噩梦的拥抱)
        // Theme: Critical strikes and burst damage
        // ============================================================
        _setBonusDefinitions[ItemRarity.Epic] = new SetBonusDefinition
        {
            setName = "Nightmare's Embrace",
            setNameChinese = "噩梦的拥抱",
            rarity = ItemRarity.Epic,
            
            // 2-piece: +15% Critical Chance
            twoSetBonus = new SetBonus
            {
                description = "+15% Critical Chance",
                descriptionChinese = "+15% 暴击率",
                critChanceBonus = 15f
            },
            
            // 4-piece: +50% Critical Damage, +10% Critical Chance
            fourSetBonus = new SetBonus
            {
                description = "+50% Critical Damage, +10% Critical Chance",
                descriptionChinese = "+50% 暴击伤害, +10% 暴击率",
                critDamageBonus = 50f,
                critChanceBonus = 10f
            },
            
            // 6-piece: Critical hits have 10% chance to trigger Nightmare Burst - AoE damage = 300% ATK (configurable cooldown)
            sixSetBonus = new SetBonus
            {
                description = string.Format("Nightmare Burst: Crits have 10% chance for 300% ATK AoE ({0}s CD)", epicCD),
                descriptionChinese = string.Format("噩梦爆发: 暴击10%概率造成300%攻击力范围伤害 ({0}秒冷却)", epicCD),
                hasProcEffect = true,
                procType = SetProcType.OnCrit,
                procCooldown = epicCD
            }
        };
        
        // ============================================================
        // LEGENDARY SET - "El's Divine Blessing" (艾尔的神圣祝福)
        // Theme: Ultimate power with all stats boosted
        // ============================================================
        _setBonusDefinitions[ItemRarity.Legendary] = new SetBonusDefinition
        {
            setName = "El's Divine Blessing",
            setNameChinese = "艾尔的神圣祝福",
            rarity = ItemRarity.Legendary,
            
            // 2-piece: +30 Attack, +100 Health, +15 Defense
            twoSetBonus = new SetBonus
            {
                description = "+30 Attack, +100 Health, +15 Defense",
                descriptionChinese = "+30 攻击, +100 生命, +15 防御",
                attackBonus = 30,
                healthBonus = 100,
                defenseBonus = 15
            },
            
            // 4-piece: +20% All Damage, +10% Lifesteal, +25% Crit Damage
            fourSetBonus = new SetBonus
            {
                description = "+50 Attack, +10% Lifesteal, +25% Crit Damage",
                descriptionChinese = "+50 攻击, +10% 生命偷取, +25% 暴击伤害",
                attackBonus = 50,
                lifestealBonus = 10f,
                critDamageBonus = 25f
            },
            
            // 6-piece: Divine Wrath - On dealing damage, 5% chance to call down holy light dealing 500% ATK 
            // and healing for 20% max HP (configurable cooldown)
            sixSetBonus = new SetBonus
            {
                description = string.Format("Divine Wrath: 5% on hit to deal 500% ATK + heal 20% HP ({0}s CD)", legendaryCD),
                descriptionChinese = string.Format("神圣之怒: 攻击5%概率造成500%攻击力伤害并回复20%生命 ({0}秒冷却)", legendaryCD),
                hasProcEffect = true,
                procType = SetProcType.OnDealDamage,
                procCooldown = legendaryCD
            }
        };
    }
    
    /// <summary>
    /// Refresh cooldown values from config (called when config changes)
    /// </summary>
    public static void RefreshCooldownsFromConfig()
    {
        if (_setBonusDefinitions == null) return;
        
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        if (config == null) return;
        
        // Update cooldowns in existing definitions
        if (_setBonusDefinitions.ContainsKey(ItemRarity.Common) && _setBonusDefinitions[ItemRarity.Common].sixSetBonus != null)
        {
            float cd = config.setBonusProcCooldownCommon;
            _setBonusDefinitions[ItemRarity.Common].sixSetBonus.procCooldown = cd;
            _setBonusDefinitions[ItemRarity.Common].sixSetBonus.description = string.Format("Cheat Death: Survive fatal damage with 1 HP + 100% shield ({0}s CD)", cd);
            _setBonusDefinitions[ItemRarity.Common].sixSetBonus.descriptionChinese = string.Format("死里逃生: 致命伤害时存活并获得100%护盾 ({0}秒冷却)", cd);
        }
        
        if (_setBonusDefinitions.ContainsKey(ItemRarity.Rare) && _setBonusDefinitions[ItemRarity.Rare].sixSetBonus != null)
        {
            float cd = config.setBonusProcCooldownRare;
            _setBonusDefinitions[ItemRarity.Rare].sixSetBonus.procCooldown = cd;
            _setBonusDefinitions[ItemRarity.Rare].sixSetBonus.description = string.Format("Wind Rush: On kill, +50% speed for 3s ({0}s CD)", cd);
            _setBonusDefinitions[ItemRarity.Rare].sixSetBonus.descriptionChinese = string.Format("疾风突袭: 击杀后+50%速度3秒 ({0}秒冷却)", cd);
        }
        
        if (_setBonusDefinitions.ContainsKey(ItemRarity.Epic) && _setBonusDefinitions[ItemRarity.Epic].sixSetBonus != null)
        {
            float cd = config.setBonusProcCooldownEpic;
            _setBonusDefinitions[ItemRarity.Epic].sixSetBonus.procCooldown = cd;
            _setBonusDefinitions[ItemRarity.Epic].sixSetBonus.description = string.Format("Nightmare Burst: Crits have 10% chance for 300% ATK AoE ({0}s CD)", cd);
            _setBonusDefinitions[ItemRarity.Epic].sixSetBonus.descriptionChinese = string.Format("噩梦爆发: 暴击10%概率造成300%攻击力范围伤害 ({0}秒冷却)", cd);
        }
        
        if (_setBonusDefinitions.ContainsKey(ItemRarity.Legendary) && _setBonusDefinitions[ItemRarity.Legendary].sixSetBonus != null)
        {
            float cd = config.setBonusProcCooldownLegendary;
            _setBonusDefinitions[ItemRarity.Legendary].sixSetBonus.procCooldown = cd;
            _setBonusDefinitions[ItemRarity.Legendary].sixSetBonus.description = string.Format("Divine Wrath: 5% on hit to deal 500% ATK + heal 20% HP ({0}s CD)", cd);
            _setBonusDefinitions[ItemRarity.Legendary].sixSetBonus.descriptionChinese = string.Format("神圣之怒: 攻击5%概率造成500%攻击力伤害并回复20%生命 ({0}秒冷却)", cd);
        }
        
        RPGLog.Debug(" Set bonus cooldowns refreshed from config");
    }
    
    /// <summary>
    /// Called when equipment changes - recalculates all set bonuses
    /// </summary>
    public void OnEquipmentChanged()
    {
        // Check if set bonuses are enabled in config
        if (!IsSetBonusEnabled())
        {
            // Remove any existing bonuses and return
            RemoveCurrentStatBonuses();
            UnsubscribeFromEvents();
            _activeSetBonuses.Clear();
            _equippedCountByRarity.Clear();
            return;
        }
        
        // Count equipped items by rarity
        CountEquippedItemsByRarity();
        
        // Remove old stat bonuses
        RemoveCurrentStatBonuses();
        
        // Calculate and apply new set bonuses
        CalculateAndApplySetBonuses();
        
        // Update event subscriptions based on active 6-piece bonuses
        UpdateEventSubscriptions();
    }
    
    // ============================================================
    // PUBLIC GETTERS FOR UI
    // ============================================================
    
    /// <summary>
    /// Get the number of items equipped for a specific rarity
    /// </summary>
    public int GetEquippedCount(ItemRarity rarity)
    {
        if (_equippedCountByRarity.ContainsKey(rarity))
            return _equippedCountByRarity[rarity];
        return 0;
    }
    
    /// <summary>
    /// Check if a specific set bonus tier is active
    /// </summary>
    public bool HasSetBonus(ItemRarity rarity, int tier)
    {
        if (!_activeSetBonuses.ContainsKey(rarity)) return false;
        SetBonusData data = _activeSetBonuses[rarity];
        
        switch (tier)
        {
            case 2: return data.hasTwoSet;
            case 4: return data.hasFourSet;
            case 6: return data.hasSixSet;
            default: return false;
        }
    }
    
    /// <summary>
    /// Get the set bonus definition for a rarity
    /// </summary>
    public static SetBonusDefinition GetSetDefinition(ItemRarity rarity)
    {
        if (_setBonusDefinitions == null) return null;
        if (_setBonusDefinitions.ContainsKey(rarity))
            return _setBonusDefinitions[rarity];
        return null;
    }
    
    /// <summary>
    /// Get all active set bonuses for UI display
    /// Returns a list of (rarity, equippedCount, hasTwoSet, hasFourSet, hasSixSet)
    /// </summary>
    public List<SetBonusUIData> GetActiveSetBonusesForUI()
    {
        List<SetBonusUIData> result = new List<SetBonusUIData>();
        
        foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
        {
            int count = GetEquippedCount(rarity);
            if (count > 0)
            {
                SetBonusUIData data = new SetBonusUIData();
                data.rarity = rarity;
                data.equippedCount = count;
                data.hasTwoSet = HasSetBonus(rarity, 2);
                data.hasFourSet = HasSetBonus(rarity, 4);
                data.hasSixSet = HasSetBonus(rarity, 6);
                data.definition = GetSetDefinition(rarity);
                result.Add(data);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Get cooldown remaining for a proc effect
    /// Uses Time.realtimeSinceStartup to match SetProcCooldown (doesn't reset between scenes)
    /// </summary>
    public float GetProcCooldownRemaining(ItemRarity rarity)
    {
        if (!_procCooldowns.ContainsKey(rarity)) return 0f;
        float remaining = _procCooldowns[rarity] - Time.realtimeSinceStartup;
        return remaining > 0 ? remaining : 0f;
    }
    
    /// <summary>
    /// Data structure for UI display
    /// </summary>
    public class SetBonusUIData
    {
        public ItemRarity rarity;
        public int equippedCount;
        public bool hasTwoSet;
        public bool hasFourSet;
        public bool hasSixSet;
        public SetBonusDefinition definition;
    }
    
    // ============================================================
    // INTERNAL METHODS
    // ============================================================
    
    /// <summary>
    /// Count how many items of each rarity are equipped
    /// </summary>
    private void CountEquippedItemsByRarity()
    {
        _equippedCountByRarity.Clear();
        
        // Initialize counts
        foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
        {
            _equippedCountByRarity[rarity] = 0;
        }
        
        // Count equipped items
        if (_equipmentManager == null) return;
        
        foreach (var kvp in _equipmentManager.EquippedItems)
        {
            RPGItem item = kvp.Value;
            if (item != null)
            {
                _equippedCountByRarity[item.rarity]++;
            }
        }
    }
    
    /// <summary>
    /// Remove current stat bonuses from hero
    /// </summary>
    private void RemoveCurrentStatBonuses()
    {
        if (_currentSetStatBonus != null)
        {
            if (DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                try
                {
                    DewPlayer.local.hero.Status.RemoveStatBonus(_currentSetStatBonus);
                }
                catch { }
            }
            _currentSetStatBonus = null;
        }
        
        _activeSetBonuses.Clear();
    }
    
    /// <summary>
    /// Calculate and apply all active set bonuses
    /// </summary>
    private void CalculateAndApplySetBonuses()
    {
        // Accumulate all bonuses
        int totalAttack = 0;
        int totalDefense = 0;
        int totalHealth = 0;
        float totalMoveSpeed = 0f;
        float totalAttackSpeed = 0f;
        float totalCritChance = 0f;
        float totalCritDamage = 0f;
        float totalLifesteal = 0f;
        float totalRegeneration = 0f;
        
        foreach (var kvp in _setBonusDefinitions)
        {
            ItemRarity rarity = kvp.Key;
            SetBonusDefinition definition = kvp.Value;
            
            int equippedCount = _equippedCountByRarity.ContainsKey(rarity) ? _equippedCountByRarity[rarity] : 0;
            
            SetBonusData bonusData = new SetBonusData();
            bonusData.definition = definition;
            bonusData.equippedCount = equippedCount;
            bonusData.hasTwoSet = equippedCount >= 2;
            bonusData.hasFourSet = equippedCount >= 4;
            bonusData.hasSixSet = equippedCount >= 6;
            
            // Apply 2-piece bonus
            if (bonusData.hasTwoSet && definition.twoSetBonus != null)
            {
                SetBonus bonus = definition.twoSetBonus;
                totalAttack += bonus.attackBonus;
                totalDefense += bonus.defenseBonus;
                totalHealth += bonus.healthBonus;
                totalMoveSpeed += bonus.moveSpeedBonus;
                totalAttackSpeed += bonus.attackSpeedBonus;
                totalCritChance += bonus.critChanceBonus;
                totalCritDamage += bonus.critDamageBonus;
                totalLifesteal += bonus.lifestealBonus;
                totalRegeneration += bonus.regenerationBonus;
            }
            
            // Apply 4-piece bonus
            if (bonusData.hasFourSet && definition.fourSetBonus != null)
            {
                SetBonus bonus = definition.fourSetBonus;
                totalAttack += bonus.attackBonus;
                totalDefense += bonus.defenseBonus;
                totalHealth += bonus.healthBonus;
                totalMoveSpeed += bonus.moveSpeedBonus;
                totalAttackSpeed += bonus.attackSpeedBonus;
                totalCritChance += bonus.critChanceBonus;
                totalCritDamage += bonus.critDamageBonus;
                totalLifesteal += bonus.lifestealBonus;
                totalRegeneration += bonus.regenerationBonus;
            }
            
            // 6-piece bonus stats (proc effects handled separately)
            if (bonusData.hasSixSet && definition.sixSetBonus != null)
            {
                SetBonus bonus = definition.sixSetBonus;
                totalAttack += bonus.attackBonus;
                totalDefense += bonus.defenseBonus;
                totalHealth += bonus.healthBonus;
                totalMoveSpeed += bonus.moveSpeedBonus;
                totalAttackSpeed += bonus.attackSpeedBonus;
                totalCritChance += bonus.critChanceBonus;
                totalCritDamage += bonus.critDamageBonus;
                totalLifesteal += bonus.lifestealBonus;
                totalRegeneration += bonus.regenerationBonus;
            }
            
            // Store active bonus data and track for scoring
            if (bonusData.hasTwoSet)
            {
                // Track new set bonus activations for scoring
                bool wasActive = _activeSetBonuses.ContainsKey(rarity);
                if (!wasActive)
                {
                    ScoringPatches.TrackSetBonusActivated();
                }
                
                _activeSetBonuses[rarity] = bonusData;
                
                // Debug log for set bonus tracking
                RPGLog.Debug(string.Format(" SetBonus: {0} set registered - count={1}, 2pc={2}, 4pc={3}, 6pc={4}", 
                    rarity, bonusData.equippedCount, bonusData.hasTwoSet, bonusData.hasFourSet, bonusData.hasSixSet));
            }
        }
        
        // Apply accumulated stat bonuses to hero
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            _needsReapply = false;
            
            StatBonus bonus = new StatBonus();
            bonus.attackDamageFlat = totalAttack;
            bonus.armorFlat = totalDefense;
            bonus.maxHealthFlat = totalHealth;
            bonus.movementSpeedPercentage = totalMoveSpeed;
            bonus.attackSpeedPercentage = totalAttackSpeed;
            bonus.critChanceFlat = totalCritChance / 100f; // Convert to decimal
            bonus.critAmpFlat = totalCritDamage / 100f;    // Convert to decimal
            bonus.healthRegenFlat = totalRegeneration;
            
            // Note: Lifesteal is handled by EquipmentManager's on-hit system
            // We'll need to communicate this to EquipmentManager
            
            if (NetworkServer.active)
            {
                _currentSetStatBonus = DewPlayer.local.hero.Status.AddStatBonus(bonus);
            }
            else
            {
                // Client: Request server to apply stats
                RequestSetBonusStats(bonus);
            }
            
            RPGLog.Debug(" Set bonuses applied to hero");
        }
        else
        {
            // Hero not ready yet - mark for retry in UpdateBuffs
            _needsReapply = true;
            RPGLog.Debug(" Hero not ready, set bonuses will be applied when available");
        }
        
        // Log active set bonuses
        LogActiveSetBonuses();
    }
    
    /// <summary>
    /// Update event subscriptions for 6-piece proc effects
    /// IMPORTANT: Uses CLIENT-COMPATIBLE events that work for both host and clients!
    /// - ClientHeroEvent_OnKillOrAssist: Fires on all clients via RPC
    /// - ClientEventManager.OnAttackHit: Fires on all clients via RPC
    /// </summary>
    private void UpdateEventSubscriptions()
    {
        // Unsubscribe from all events first
        UnsubscribeFromEvents();
        
        // Check which 6-piece bonuses are active and subscribe accordingly
        bool needsOnKill = false;
        bool needsOnAttackHit = false;
        bool needsCheatDeath = false;
        
        foreach (var kvp in _activeSetBonuses)
        {
            SetBonusData data = kvp.Value;
            if (data.hasSixSet && data.definition.sixSetBonus != null && data.definition.sixSetBonus.hasProcEffect)
            {
                switch (data.definition.sixSetBonus.procType)
                {
                    case SetProcType.OnKill:
                        needsOnKill = true;
                        break;
                    case SetProcType.OnFatalDamage:
                        needsCheatDeath = true; // Will use health monitoring in Update()
                        break;
                    case SetProcType.OnDealDamage:
                    case SetProcType.OnCrit:
                        needsOnAttackHit = true;
                        break;
                }
            }
        }
        
        // Subscribe to needed events using CLIENT-COMPATIBLE events
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            Hero hero = DewPlayer.local.hero;
            _subscribedHero = hero;
            
            // Use ClientHeroEvent_OnKillOrAssist - works on both host and clients!
            if (needsOnKill)
            {
                _onKillHandler = new Action<EventInfoKill>(OnHeroKillOrAssist);
                hero.ClientHeroEvent_OnKillOrAssist += _onKillHandler;
                RPGLog.Debug(" SetBonus: Subscribed to ClientHeroEvent_OnKillOrAssist (works on clients!)");
            }
            
            // Use ClientEventManager.OnAttackHit - works on both host and clients!
            if (needsOnAttackHit)
            {
                ClientEventManager clientEventManager = NetworkedManagerBase<ClientEventManager>.instance;
                if (clientEventManager != null)
                {
                    _onAttackHitHandler = new Action<EventInfoAttackHit>(OnAttackHit);
                    clientEventManager.OnAttackHit += _onAttackHitHandler;
                    RPGLog.Debug(" SetBonus: Subscribed to ClientEventManager.OnAttackHit (works on clients!)");
                }
            }
            
            // Cheat Death: Notify server to track this hero (works for both host and clients)
            if (needsCheatDeath)
            {
                _cheatDeathActive = true;
                // Only register if hero has a valid netId (hero might not be initialized yet when loading from save)
                if (hero.netId != 0)
                {
                    RegisterCheatDeathWithServer(hero.netId, true);
                    _cheatDeathRegistrationPending = false;
                    RPGLog.Debug(" SetBonus: Cheat Death registered with server for hero " + hero.netId);
                }
                else
                {
                    _cheatDeathRegistrationPending = true;
                    RPGLog.Debug(" SetBonus: Cheat Death registration deferred - hero netId is 0 (hero not initialized yet)");
                }
            }
            else
            {
                _cheatDeathActive = false;
                _cheatDeathRegistrationPending = false;
                if (hero != null && hero.netId != 0)
                {
                    RegisterCheatDeathWithServer(hero.netId, false);
                }
            }
            
            _isSubscribedToEvents = needsOnKill || needsOnAttackHit || needsCheatDeath;
        }
    }
    
    /// <summary>
    /// Unsubscribe from all events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (!_isSubscribedToEvents) return;
        
        // Unsubscribe from hero events
        if (_subscribedHero != null)
        {
            if (_onKillHandler != null)
            {
                try { _subscribedHero.ClientHeroEvent_OnKillOrAssist -= _onKillHandler; } catch { }
                _onKillHandler = null;
            }
            
            // Unregister Cheat Death from server
            if (_cheatDeathActive)
            {
                if (_subscribedHero != null && _subscribedHero.netId != 0)
                {
                    RegisterCheatDeathWithServer(_subscribedHero.netId, false);
                }
                _cheatDeathActive = false;
                _cheatDeathRegistrationPending = false;
            }
        }
        
        // Unsubscribe from ClientEventManager
        if (_onAttackHitHandler != null)
        {
            ClientEventManager clientEventManager = NetworkedManagerBase<ClientEventManager>.instance;
            if (clientEventManager != null)
            {
                try { clientEventManager.OnAttackHit -= _onAttackHitHandler; } catch { }
            }
            _onAttackHitHandler = null;
        }
        
        _subscribedHero = null;
        _cheatDeathActive = false;
        _cheatDeathRegistrationPending = false;
        _isSubscribedToEvents = false;
    }
    
    /// <summary>
    /// Called when hero kills or assists in killing an enemy
    /// Uses ClientHeroEvent_OnKillOrAssist which works on BOTH host and clients!
    /// NOTE: This event fires for both kills AND assists
    /// We check if the actor is the hero or owned by the hero to distinguish kills from assists
    /// </summary>
    private void OnHeroKillOrAssist(EventInfoKill killInfo)
    {
        // Basic validation - check hero is still valid
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        if (DewPlayer.local.hero.IsNullOrInactive()) return;
        if (killInfo.victim == null) return;
        if (killInfo.actor == null) return;
        
        // Only count kills on enemies (monsters), not friendly fire
        if (!(killInfo.victim is Monster)) return;
        
        // Check if this is a KILL (not just an assist)
        // The actor must be the hero OR owned by the hero (pet/summon)
        Hero hero = DewPlayer.local.hero;
        bool isKill = false;
        
        // Check if actor IS the hero
        if (killInfo.actor == hero)
        {
            isKill = true;
        }
        // Check if actor's first entity is the hero (pet/summon/ability)
        else if (killInfo.actor != null && killInfo.actor.firstEntity == hero)
        {
            isKill = true;
        }
        
        if (!isKill)
        {
            // This is an assist, not a kill - don't trigger Wind Rush
            return;
        }
        
        // Check for Rare 6-piece: Wind Rush
        if (_activeSetBonuses.ContainsKey(ItemRarity.Rare))
        {
            SetBonusData data = _activeSetBonuses[ItemRarity.Rare];
            
            // STRICT cooldown check - use realtime (doesn't reset between scenes!)
            float realtime = Time.realtimeSinceStartup;
            bool onCooldown = realtime < _windRushCooldownEndTime;
            
            // Only trigger if: has 6-piece, buff not active, and not on cooldown
            if (data.hasSixSet && !_windRushIsActive && !onCooldown)
            {
                // Set cooldown FIRST to prevent any possibility of double triggers
                _windRushCooldownEndTime = realtime + 15f; // 15 second cooldown (using realtime!)
                SetProcCooldown(ItemRarity.Rare); // Also set the dictionary-based cooldown
                
                RPGLog.Debug(" Wind Rush TRIGGERED! +50% speed for 3s (15s cooldown)");
                RequestTriggerProc(ItemRarity.Rare, 0, Vector3.zero);
            }
        }
    }
    
    /// <summary>
    /// Called when any attack hits
    /// Uses ClientEventManager.OnAttackHit which works on BOTH host and clients!
    /// </summary>
    private void OnAttackHit(EventInfoAttackHit hitInfo)
    {
        // Only process hits by local hero - check hero is still valid
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        if (DewPlayer.local.hero.IsNullOrInactive()) return;
        if (hitInfo.attacker != DewPlayer.local.hero) return;
        if (hitInfo.victim == null) return;
        
        // Use realtime - doesn't reset between scenes!
        float realtime = Time.realtimeSinceStartup;
        
        // Check for Epic 6-piece: Nightmare Burst (on crit)
        if (_activeSetBonuses.ContainsKey(ItemRarity.Epic))
        {
            SetBonusData data = _activeSetBonuses[ItemRarity.Epic];
            bool onCooldown = realtime < _nightmareBurstCooldownEndTime;
            
            // Has 6-piece, not on cooldown
            if (data.hasSixSet && !_nightmareBurstOnCooldown && !onCooldown)
            {
                // Check if it was a crit (10% chance to proc on crit)
                if (hitInfo.isCrit)
                {
                    float roll = UnityEngine.Random.Range(0f, 1f);
                    if (roll <= 0.10f) // 10% chance
                    {
                        // Set cooldown FIRST to prevent double triggers
                        _nightmareBurstOnCooldown = true;
                        _nightmareBurstCooldownEndTime = realtime + 40f;
                        SetProcCooldown(ItemRarity.Epic);
                        
                        Vector3 targetPos = hitInfo.victim.position;
                        RequestTriggerProc(ItemRarity.Epic, 0, targetPos);
                        RPGLog.Debug(" === NIGHTMARE BURST TRIGGERED === Next available in 40s");
                    }
                }
            }
        }
        
        // Check for Legendary 6-piece: Divine Wrath (5% on any hit)
        if (_activeSetBonuses.ContainsKey(ItemRarity.Legendary))
        {
            SetBonusData data = _activeSetBonuses[ItemRarity.Legendary];
            bool onCooldown = realtime < _divineWrathCooldownEndTime;
            
            // Has 6-piece, not on cooldown
            if (data.hasSixSet && !_divineWrathOnCooldown && !onCooldown)
            {
                float roll = UnityEngine.Random.Range(0f, 1f);
                if (roll <= 0.05f) // 5% chance
                {
                    // Set cooldown FIRST to prevent double triggers
                    _divineWrathOnCooldown = true;
                    _divineWrathCooldownEndTime = realtime + 40f;
                    SetProcCooldown(ItemRarity.Legendary);
                    
                    Vector3 targetPos = hitInfo.victim.position;
                    RequestTriggerProc(ItemRarity.Legendary, 0, targetPos);
                    RPGLog.Debug(" === DIVINE WRATH TRIGGERED === Next available in 40s");
                }
            }
        }
    }
    
    /// <summary>
    /// Register/unregister Cheat Death with server
    /// Works for both host and clients - server will handle damage interception
    /// </summary>
    private void RegisterCheatDeathWithServer(uint heroNetId, bool register)
    {
        try
        {
            // Validate netId - must be non-zero
            if (heroNetId == 0)
            {
                RPGLog.Debug(" Cannot register Cheat Death: hero netId is 0 (hero may not be initialized)");
                return;
            }
            
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null)
            {
                RPGLog.Debug(" Cannot register Cheat Death: ChatManager not found");
                return;
            }
        
            // Format: [RPGCHEATDEATH]heroNetId|register (1=register, 0=unregister)
            string content = string.Format("[RPGCHEATDEATH]{0}|{1}", heroNetId, register ? "1" : "0");
            chatManager.CmdSendChatMessage(content, null);
            
            RPGLog.Debug(string.Format(" CheatDeath registration sent: netId={0}, register={1}", heroNetId, register));
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to register Cheat Death with server: " + e.Message);
        }
    }
    
    /// <summary>
    /// Request server to trigger a proc effect
    /// For host: triggers directly
    /// For client: sends chat message to server
    /// </summary>
    private void RequestTriggerProc(ItemRarity rarity, int extraData, Vector3 position)
    {
        if (NetworkServer.active)
        {
            // Host: trigger directly
            ExecuteProc(rarity, extraData, position);
        }
        else
        {
            // Client: request server to trigger
            try
            {
                ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
                if (chatManager == null) return;
                
                uint heroNetId = (DewPlayer.local != null && DewPlayer.local.hero != null) ? DewPlayer.local.hero.netId : 0;
                
                // Format: [RPGSETPROC]heroNetId|rarity|extraData|posX|posY|posZ
                string content = string.Format("[RPGSETPROC]{0}|{1}|{2}|{3}|{4}|{5}",
                    heroNetId,
                    (int)rarity,
                    extraData,
                    position.x,
                    position.y,
                    position.z);
                
                chatManager.CmdSendChatMessage(content, null);
            }
            catch (Exception e)
            {
                RPGLog.Warning(" Failed to request set proc: " + e.Message);
            }
        }
    }
    
    /// <summary>
    /// Execute a proc effect (called on server)
    /// </summary>
    public void ExecuteProc(ItemRarity rarity, int extraData, Vector3 position)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                // Cheat Death is now handled entirely server-side via ChatPatches.OnServerCheatDeathDamage
                // This method should not be called for Cheat Death
                RPGLog.Warning(" ExecuteProc called for Cheat Death - this should not happen!");
                break;
            case ItemRarity.Rare:
                TriggerWindRush();
                break;
            case ItemRarity.Epic:
                TriggerNightmareBurstAtPosition(position);
                break;
            case ItemRarity.Legendary:
                TriggerDivineWrathAtPosition(position);
                break;
        }
    }
    
    // ============================================================
    // PROC EFFECT IMPLEMENTATIONS
    // ============================================================
    
    // Wind Rush buff tracking (Rare 6-piece)
    // NOTE: Using Time.realtimeSinceStartup instead of Time.time because Time.time can reset between scenes!
    private StatBonus _windRushSpeedBonus = null;
    private float _windRushEndTime = 0f;           // When buff expires (realtime)
    private bool _windRushIsActive = false;         // Is the 3s buff currently active?
    private float _windRushCooldownEndTime = 0f;    // When 15s cooldown expires (realtime)
    
    // Nightmare Burst tracking (Epic 6-piece)
    private bool _nightmareBurstOnCooldown = false;
    private float _nightmareBurstCooldownEndTime = 0f; // realtime
    
    // Divine Wrath tracking (Legendary 6-piece)
    private bool _divineWrathOnCooldown = false;
    private float _divineWrathCooldownEndTime = 0f; // realtime
    
    // Cheat Death tracking (Common 6-piece) - already has _cheatDeathActive
    // Note: Cooldown is now tracked server-side in ChatPatches._heroCheatDeathCooldowns
    
    /// <summary>
    /// Rare 6-piece: Wind Rush - +50% speed for 3s (15s cooldown)
    /// </summary>
    private void TriggerWindRush()
    {
        // Double-check: Prevent multiple triggers while buff is active
        if (_windRushIsActive)
        {
            RPGLog.Debug(" Wind Rush: Already active, ignoring trigger call");
            return;
        }
        
        Hero hero = (DewPlayer.local != null) ? DewPlayer.local.hero : null;
        if (hero == null)
        {
            RPGLog.Debug(" Wind Rush: No hero found!");
            return;
        }
        
        RPGLog.Debug(" === WIND RUSH ACTIVATED === Hero: " + hero.name);
        
        if (NetworkServer.active)
        {
            // Remove old speed buff if exists (shouldn't happen but just in case)
            if (_windRushSpeedBonus != null)
            {
                try { hero.Status.RemoveStatBonus(_windRushSpeedBonus); } catch { }
                _windRushSpeedBonus = null;
            }
            
            float realtime = Time.realtimeSinceStartup;
            
            // Apply 50% movement speed buff
            StatBonus speedBuff = new StatBonus();
            speedBuff.movementSpeedPercentage = 50f;
            _windRushSpeedBonus = hero.Status.AddStatBonus(speedBuff);
            _windRushEndTime = realtime + 3f; // 3 second duration (using realtime!)
            _windRushIsActive = true; // Mark as active - this prevents re-triggering during buff
            
            RPGLog.Debug(string.Format(" Wind Rush: +50% speed for 3s. Buff ends at {0:F1}, Cooldown ends at {1:F1} (realtime: {2:F1})", 
                _windRushEndTime, _windRushCooldownEndTime, realtime));
            
            // Force stats update
            hero.Status.UpdateStats();
        }
        else
        {
            RPGLog.Debug(" Wind Rush: Not server, cannot apply buff directly");
        }
    }
    
    /// <summary>
    /// Common 6-piece: Cheat Death - Survive with 1 HP + 30% shield
    /// Called when player would take fatal damage
    /// IMPORTANT: This must be called BEFORE Kill() is called on the hero
    /// </summary>
    private void TriggerCheatDeath()
    {
        if (!NetworkServer.active) return;
        
        Hero hero = _subscribedHero;
        if (hero == null) return;
        
        RPGLog.Debug(" Set Bonus PROC: Cheat Death activated!");
        
        // CRITICAL: Set health to 1 HP FIRST to prevent death
        // This must happen synchronously before Kill() is called in the damage processing
        if (hero.Status.currentHealth <= 0f)
        {
            hero.Status.SetHealth(1f);
            RPGLog.Debug(" Cheat Death: Set health to 1 HP to prevent death");
        }
        
            // Give shield equal to 30% max HP for 10 seconds
            float shieldAmount = hero.Status.maxHealth * 0.30f;
            hero.GiveShield(hero, shieldAmount, 10f, false);
            
        RPGLog.Debug(string.Format(" Cheat Death: Shield {0}, survived with 1 HP!", shieldAmount));
    }
    
    /// <summary>
    /// Epic 6-piece: Nightmare Burst - AoE damage = 300% ATK (40s cooldown)
    /// </summary>
    private void TriggerNightmareBurstAtPosition(Vector3 centerPos)
    {
        Hero hero = (DewPlayer.local != null) ? DewPlayer.local.hero : null;
        if (hero == null) return;
        
        RPGLog.Debug(" Set Bonus PROC: Nightmare Burst activated!");
        _nightmareBurstOnCooldown = true; // Mark as on cooldown
        
        if (NetworkServer.active)
        {
            float aoeDamage = hero.Status.attackDamage * 3f; // 300% ATK
            float aoeRadius = 5f; // 5 unit radius
            
            // Deal AoE damage to all enemies in range
            DealAoEDamage(hero, centerPos, aoeRadius, aoeDamage);
            
            RPGLog.Debug(string.Format(" Nightmare Burst: {0} damage in {1} radius!", aoeDamage, aoeRadius));
        }
    }
    
    /// <summary>
    /// Legendary 6-piece: Divine Wrath - 500% ATK damage + heal 20% HP (40s cooldown)
    /// </summary>
    private void TriggerDivineWrathAtPosition(Vector3 centerPos)
    {
        Hero hero = (DewPlayer.local != null) ? DewPlayer.local.hero : null;
        if (hero == null) return;
        
        RPGLog.Debug(" Set Bonus PROC: Divine Wrath activated!");
        _divineWrathOnCooldown = true; // Mark as on cooldown
        
        if (NetworkServer.active)
        {
            float aoeDamage = hero.Status.attackDamage * 5f; // 500% ATK
            float aoeRadius = 6f; // 6 unit radius
            
            // Deal AoE damage to all enemies in range
            DealAoEDamage(hero, centerPos, aoeRadius, aoeDamage);
            
            // Heal for 20% max HP
            float healAmount = hero.Status.maxHealth * 0.20f;
            HealData healData = new HealData(healAmount);
            healData.SetActor(hero);
            hero.DoHeal(healData, hero);
            
            RPGLog.Debug(string.Format(" Divine Wrath: {0} damage, healed {1}!", aoeDamage, healAmount));
        }
    }
    
    // Cache for enemy target validator (reused for AoE damage)
    private static AbilityTargetValidator _enemyValidator = new AbilityTargetValidator
    {
        targets = EntityRelation.Enemy
    };
    
    /// <summary>
    /// Deal AoE damage to all enemies within radius of a position
    /// Uses the game's proper DewPhysics.OverlapCircleAllEntities method (like Essence of Confidence and other AoE effects)
    /// </summary>
    private void DealAoEDamage(Hero attacker, Vector3 center, float radius, float damage)
    {
        if (!NetworkServer.active) return;
        
        int hitCount = 0;
        
        // Use the game's proper physics-based AoE detection (like Ai_Mon_Despair_WretchedArtillery_BarrageAtk_AoE)
        // This automatically handles entity filtering and avoids "Collection was modified" exceptions
        ListReturnHandle<Entity> handle;
        List<Entity> entitiesInRange = DewPhysics.OverlapCircleAllEntities(out handle, center, radius, _enemyValidator, attacker);
        
        // Deal damage to all entities in range
        foreach (Entity entity in entitiesInRange)
        {
            if (entity == null || entity.IsNullInactiveDeadOrKnockedOut()) continue;
            
            // Deal damage
            DamageData dmg = attacker.MagicDamage(damage, 0f); // 0 proc coefficient to avoid infinite loops
            attacker.DealDamage(dmg, entity);
            hitCount++;
        }
        
        // Return the handle to the pool (important for memory management)
        handle.Return();
        
        RPGLog.Debug(string.Format(" AoE hit {0} enemies for {1} damage each", hitCount, damage));
    }
    
    /// <summary>
    /// Called every frame to manage buff durations and check triggers
    /// </summary>
    public void UpdateBuffs()
    {
        // Check if set bonuses need to be reapplied (hero wasn't ready during load)
        if (_needsReapply && DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            RPGLog.Debug(" SetBonus: Hero now ready, applying pending set bonuses...");
            OnEquipmentChanged();
        }
        
        // Retry CheatDeath registration if hero now has a valid netId (was 0 during load)
        if (_cheatDeathActive && _cheatDeathRegistrationPending && DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            Hero hero = DewPlayer.local.hero;
            if (hero.netId != 0)
            {
                // Hero now has a valid netId, register CheatDeath
                RegisterCheatDeathWithServer(hero.netId, true);
                _cheatDeathRegistrationPending = false;
                RPGLog.Debug(" SetBonus: Cheat Death registration retried - hero now has netId " + hero.netId);
            }
        }
        
        // Use realtime - doesn't reset between scenes!
        float realtime = Time.realtimeSinceStartup;
        
        // Check Wind Rush buff expiration (buff lasts 3 seconds)
        if (_windRushIsActive && realtime >= _windRushEndTime)
        {
            Hero hero = (DewPlayer.local != null) ? DewPlayer.local.hero : null;
            if (hero != null && NetworkServer.active && _windRushSpeedBonus != null)
            {
                try { hero.Status.RemoveStatBonus(_windRushSpeedBonus); } catch { }
            }
            _windRushSpeedBonus = null;
            _windRushIsActive = false; // Buff ended, but still on cooldown!
            
            RPGLog.Debug(" Wind Rush buff expired (3s duration)");
        }
        
        // Reset cooldown flags when cooldown expires (so they can trigger again)
        if (_nightmareBurstOnCooldown && realtime >= _nightmareBurstCooldownEndTime)
        {
            _nightmareBurstOnCooldown = false;
            RPGLog.Debug(" Nightmare Burst cooldown expired, ready to trigger again");
        }
        if (_divineWrathOnCooldown && realtime >= _divineWrathCooldownEndTime)
        {
            _divineWrathOnCooldown = false;
            RPGLog.Debug(" Divine Wrath cooldown expired, ready to trigger again");
        }
        
        // Note: Cheat Death now uses EntityEvent_OnTakeDamage instead of health monitoring
        
        // Re-subscribe if hero changed
        if (_isSubscribedToEvents && DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            if (_subscribedHero != DewPlayer.local.hero)
            {
                RPGLog.Debug(" SetBonus: Hero changed, re-subscribing to events...");
                UpdateEventSubscriptions();
            }
        }
    }
    
    // ============================================================
    // COOLDOWN MANAGEMENT
    // ============================================================
    
    private bool CanProc(ItemRarity rarity)
    {
        if (!_procCooldowns.ContainsKey(rarity)) return true;
        return Time.realtimeSinceStartup >= _procCooldowns[rarity];
    }
    
    private void SetProcCooldown(ItemRarity rarity)
    {
        // Use each set's individual procCooldown value from definition
        float cooldown = 40f; // Default fallback
        
        if (_setBonusDefinitions.ContainsKey(rarity) && _setBonusDefinitions[rarity].sixSetBonus != null)
        {
            cooldown = _setBonusDefinitions[rarity].sixSetBonus.procCooldown;
        }
        
        _procCooldowns[rarity] = Time.realtimeSinceStartup + cooldown;
    }
    
    // ============================================================
    // UTILITY METHODS
    // ============================================================
    
    /// <summary>
    /// Request server to apply set bonus stats (for clients)
    /// Format: [RPGSETBONUS]heroNetId|atk|def|hp|moveSpd|atkSpd|critCh|critDmg|regen
    /// </summary>
    private void RequestSetBonusStats(StatBonus bonus)
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            uint heroNetId = DewPlayer.local.hero.netId;
            
            // Format: [RPGSETBONUS]heroNetId|atk|def|hp|moveSpd|atkSpd|critCh|critDmg|regen
            string content = string.Format("[RPGSETBONUS]{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                heroNetId,
                (int)bonus.attackDamageFlat,
                (int)bonus.armorFlat,
                (int)bonus.maxHealthFlat,
                bonus.movementSpeedPercentage,
                bonus.attackSpeedPercentage,
                bonus.critChanceFlat,
                bonus.critAmpFlat,
                bonus.healthRegenFlat);
            
            chatManager.CmdSendChatMessage(content, null);
            RPGLog.Debug(" Client requesting set bonus stats: " + content);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to request set bonus stats: " + e.Message);
        }
    }
    
    /// <summary>
    /// Log active set bonuses for debugging
    /// </summary>
    private void LogActiveSetBonuses()
    {
        foreach (var kvp in _activeSetBonuses)
        {
            SetBonusData data = kvp.Value;
            string bonusLevel = data.hasSixSet ? "6-piece" : (data.hasFourSet ? "4-piece" : "2-piece");
            RPGLog.Debug(string.Format(" Active Set: {0} ({1}) - {2} items equipped", 
                data.definition.setName, bonusLevel, data.equippedCount));
        }
    }
    
    /// <summary>
    /// Get tooltip text for set bonuses (for UI display)
    /// </summary>
    public string GetSetBonusTooltip()
    {
        if (_activeSetBonuses.Count == 0) return "";
        
        string tooltip = "\n<color=#ffd700>═══ Set Bonuses ═══</color>\n";
        
        foreach (var kvp in _activeSetBonuses)
        {
            SetBonusData data = kvp.Value;
            SetBonusDefinition def = data.definition;
            
            string setColor = GetRarityColor(data.definition.rarity);
            tooltip += string.Format("\n<color={0}>{1}</color> ({2}/6)\n", 
                setColor, def.setName, data.equippedCount);
            
            // 2-piece
            if (def.twoSetBonus != null)
            {
                string checkmark = data.hasTwoSet ? "<color=#55efc4>✓</color>" : "<color=#636e72>○</color>";
                string textColor = data.hasTwoSet ? "#dfe6e9" : "#636e72";
                tooltip += string.Format("  {0} <color={1}>(2) {2}</color>\n", 
                    checkmark, textColor, def.twoSetBonus.description);
            }
            
            // 4-piece
            if (def.fourSetBonus != null)
            {
                string checkmark = data.hasFourSet ? "<color=#55efc4>✓</color>" : "<color=#636e72>○</color>";
                string textColor = data.hasFourSet ? "#dfe6e9" : "#636e72";
                tooltip += string.Format("  {0} <color={1}>(4) {2}</color>\n", 
                    checkmark, textColor, def.fourSetBonus.description);
            }
            
            // 6-piece
            if (def.sixSetBonus != null)
            {
                string checkmark = data.hasSixSet ? "<color=#55efc4>✓</color>" : "<color=#636e72>○</color>";
                string textColor = data.hasSixSet ? "#ffd700" : "#636e72";
                tooltip += string.Format("  {0} <color={1}>(6) {2}</color>\n", 
                    checkmark, textColor, def.sixSetBonus.description);
            }
        }
        
        return tooltip;
    }
    
    /// <summary>
    /// Get Chinese tooltip text for set bonuses
    /// </summary>
    public string GetSetBonusTooltipChinese()
    {
        if (_activeSetBonuses.Count == 0) return "";
        
        string tooltip = "\n<color=#ffd700>═══ 套装效果 ═══</color>\n";
        
        foreach (var kvp in _activeSetBonuses)
        {
            SetBonusData data = kvp.Value;
            SetBonusDefinition def = data.definition;
            
            string setColor = GetRarityColor(data.definition.rarity);
            tooltip += string.Format("\n<color={0}>{1}</color> ({2}/6)\n", 
                setColor, def.setNameChinese, data.equippedCount);
            
            // 2件套
            if (def.twoSetBonus != null)
            {
                string checkmark = data.hasTwoSet ? "<color=#55efc4>✓</color>" : "<color=#636e72>○</color>";
                string textColor = data.hasTwoSet ? "#dfe6e9" : "#636e72";
                tooltip += string.Format("  {0} <color={1}>(2件) {2}</color>\n", 
                    checkmark, textColor, def.twoSetBonus.descriptionChinese);
            }
            
            // 4件套
            if (def.fourSetBonus != null)
            {
                string checkmark = data.hasFourSet ? "<color=#55efc4>✓</color>" : "<color=#636e72>○</color>";
                string textColor = data.hasFourSet ? "#dfe6e9" : "#636e72";
                tooltip += string.Format("  {0} <color={1}>(4件) {2}</color>\n", 
                    checkmark, textColor, def.fourSetBonus.descriptionChinese);
            }
            
            // 6件套
            if (def.sixSetBonus != null)
            {
                string checkmark = data.hasSixSet ? "<color=#55efc4>✓</color>" : "<color=#636e72>○</color>";
                string textColor = data.hasSixSet ? "#ffd700" : "#636e72";
                tooltip += string.Format("  {0} <color={1}>(6件) {2}</color>\n", 
                    checkmark, textColor, def.sixSetBonus.descriptionChinese);
            }
        }
        
        return tooltip;
    }
    
    private string GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "#b2bec3";    // Gray
            case ItemRarity.Rare: return "#74b9ff";      // Blue
            case ItemRarity.Epic: return "#a55eea";      // Purple
            case ItemRarity.Legendary: return "#ffd700"; // Gold
            default: return "#ffffff";
        }
    }
    
    /// <summary>
    /// Get total lifesteal bonus from set bonuses (for EquipmentManager)
    /// </summary>
    public float GetTotalLifestealBonus()
    {
        float total = 0f;
        
        foreach (var kvp in _activeSetBonuses)
        {
            SetBonusData data = kvp.Value;
            SetBonusDefinition def = data.definition;
            
            if (data.hasTwoSet && def.twoSetBonus != null)
                total += def.twoSetBonus.lifestealBonus;
            if (data.hasFourSet && def.fourSetBonus != null)
                total += def.fourSetBonus.lifestealBonus;
            if (data.hasSixSet && def.sixSetBonus != null)
                total += def.sixSetBonus.lifestealBonus;
        }
        
        return total;
    }
    
    /// <summary>
    /// Cleanup when leaving game
    /// </summary>
    public void Cleanup()
    {
        UnsubscribeFromEvents();
        RemoveCurrentStatBonuses();
        _equippedCountByRarity.Clear();
        _activeSetBonuses.Clear();
        _needsReapply = false;
        
        // Reset all proc tracking
        _windRushSpeedBonus = null;
        _windRushIsActive = false;
        _windRushEndTime = 0f;
        _windRushCooldownEndTime = 0f;
        
        _nightmareBurstOnCooldown = false;
        _nightmareBurstCooldownEndTime = 0f;
        
        _divineWrathOnCooldown = false;
        _divineWrathCooldownEndTime = 0f;
        
        // Unregister Cheat Death from server if active
        if (_cheatDeathActive && _subscribedHero != null && _subscribedHero.netId != 0)
        {
            RegisterCheatDeathWithServer(_subscribedHero.netId, false);
        }
        _cheatDeathActive = false;
        _cheatDeathRegistrationPending = false;
        
        _procCooldowns.Clear(); // Clear all cooldowns on cleanup
    }
}

// ============================================================
// DATA STRUCTURES
// ============================================================

/// <summary>
/// Definition of a complete set bonus (all 3 tiers)
/// </summary>
public class SetBonusDefinition
{
    public string setName;
    public string setNameChinese;
    public ItemRarity rarity;
    public SetBonus twoSetBonus;
    public SetBonus fourSetBonus;
    public SetBonus sixSetBonus;
}

/// <summary>
/// Individual set bonus tier data
/// </summary>
public class SetBonus
{
    public string description;
    public string descriptionChinese;
    
    // Stat bonuses
    public int attackBonus = 0;
    public int defenseBonus = 0;
    public int healthBonus = 0;
    public float moveSpeedBonus = 0f;
    public float attackSpeedBonus = 0f;
    public float critChanceBonus = 0f;
    public float critDamageBonus = 0f;
    public float lifestealBonus = 0f;
    public float regenerationBonus = 0f;
    
    // Proc effect (for 6-piece bonuses)
    public bool hasProcEffect = false;
    public SetProcType procType = SetProcType.None;
    public float procCooldown = 40f;
}

/// <summary>
/// Runtime data for active set bonuses
/// </summary>
public class SetBonusData
{
    public SetBonusDefinition definition;
    public int equippedCount;
    public bool hasTwoSet;
    public bool hasFourSet;
    public bool hasSixSet;
}

/// <summary>
/// Types of proc effects for 6-piece bonuses
/// </summary>
public enum SetProcType
{
    None,
    OnKill,
    OnTakeDamage,
    OnFatalDamage,
    OnDealDamage,
    OnCrit
}

