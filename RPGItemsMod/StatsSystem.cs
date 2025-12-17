using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

/// <summary>
/// RPG Stats System - Adds stat points when leveling up
/// Fully synchronized between host and clients using Actor.persistentSyncedData
/// </summary>
public class StatsSystem
{
    // Stat types that can be upgraded
    public enum StatType
    {
        Strength,      // +ATK per point
        Vitality,      // +HP per point
        Defense,       // +DEF per point
        Intelligence,  // +AP per point
        Agility,       // +Move Speed, +Attack Speed per point
        Luck           // +Crit Chance per point
    }
    
    // Points per level up (configurable via ModConfig)
    public static int POINTS_PER_LEVEL 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.pointsPerLevel : 5;
        } 
    }
    
    // Stat bonuses per point (configurable via ModConfig)
    // Default values balanced to match game's scaling
    public static int ATK_PER_STRENGTH 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.atkPerStrength : 1;
        } 
    }
    
    public static int HP_PER_VITALITY 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.hpPerVitality : 8;
        } 
    }
    
    public static int DEF_PER_DEFENSE 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.defPerDefense : 1;
        } 
    }
    
    public static int AP_PER_INTELLIGENCE 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.apPerIntelligence : 1;
        } 
    }
    
    public static float MOVE_SPEED_PER_AGILITY 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.moveSpeedPerAgility : 0.5f;
        } 
    }
    
    public static float ATTACK_SPEED_PER_AGILITY 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.attackSpeedPerAgility : 0.5f;
        } 
    }
    
    // Crit chance stored as decimal internally (0.005 = 0.5%), config is in %
    public static float CRIT_CHANCE_PER_LUCK 
    { 
        get 
        { 
            RPGItemsConfig config = RPGItemsMod.GetConfig();
            return config != null ? config.critChancePerLuck / 100f : 0.005f;
        } 
    }
    
    // Current stat allocations
    private Dictionary<StatType, int> _allocatedStats = new Dictionary<StatType, int>();
    
    // Available stat points
    private int _availablePoints = 0;
    
    // Last known hero level (to detect level ups)
    private int _lastKnownLevel = 1;
    
    // Events
    public Action OnStatsChanged;
    public Action<int> OnPointsGained;
    
    // Network sync key prefix
    private const string SYNC_KEY_PREFIX = "RPGStats_";
    
    // Reference to equipment manager for applying bonuses
    private EquipmentManager _equipmentManager;
    
    // Applied stat bonuses (to track what we've added)
    private StatBonus _appliedStatBonus;
    
    // Track which hero the bonus was applied to (to avoid removing from wrong hero)
    private Hero _bonusAppliedToHero;
    
    // Flag to track if stats need to be reapplied (hero wasn't ready during load)
    private bool _needsStatReapply = false;
    
    public StatsSystem()
    {
        // Initialize all stats to 0
        _allocatedStats[StatType.Strength] = 0;
        _allocatedStats[StatType.Vitality] = 0;
        _allocatedStats[StatType.Defense] = 0;
        _allocatedStats[StatType.Intelligence] = 0;
        _allocatedStats[StatType.Agility] = 0;
        _allocatedStats[StatType.Luck] = 0;
    }
    
    public void SetEquipmentManager(EquipmentManager manager)
    {
        _equipmentManager = manager;
    }
    
    /// <summary>
    /// Get total allocated points for a stat
    /// </summary>
    public int GetStatPoints(StatType stat)
    {
        if (_allocatedStats.ContainsKey(stat))
        {
            return _allocatedStats[stat];
        }
        return 0;
    }
    
    /// <summary>
    /// Get available unspent points
    /// </summary>
    public int GetAvailablePoints()
    {
        return _availablePoints;
    }
    
    /// <summary>
    /// Get total level (sum of all allocated points / POINTS_PER_LEVEL + 1)
    /// </summary>
    public int GetTotalAllocatedPoints()
    {
        int total = 0;
        foreach (KeyValuePair<StatType, int> kvp in _allocatedStats)
        {
            total += kvp.Value;
        }
        return total;
    }
    
    /// <summary>
    /// Allocate a point to a stat (called by UI)
    /// </summary>
    public bool AllocatePoint(StatType stat)
    {
        if (_availablePoints <= 0)
        {
            RPGLog.Debug(" No available stat points!");
            return false;
        }
        
        _allocatedStats[stat] = _allocatedStats[stat] + 1;
        _availablePoints--;
        
        // Track for scoring
        ScoringPatches.TrackStatPointSpent();
        
        // Apply the stat bonus
        ApplyStatBonuses();
        
        // Sync to network
        SyncToNetwork();
        
        if (OnStatsChanged != null)
        {
            OnStatsChanged();
        }
        
        RPGLog.Debug(" Allocated 1 point to " + stat + ". Now: " + _allocatedStats[stat] + " points, " + _availablePoints + " remaining");
        
        return true;
    }
    
    /// <summary>
    /// Reset all stats (refund all points)
    /// </summary>
    public void ResetStats()
    {
        int totalRefund = GetTotalAllocatedPoints();
        
        _allocatedStats[StatType.Strength] = 0;
        _allocatedStats[StatType.Vitality] = 0;
        _allocatedStats[StatType.Defense] = 0;
        _allocatedStats[StatType.Intelligence] = 0;
        _allocatedStats[StatType.Agility] = 0;
        _allocatedStats[StatType.Luck] = 0;
        
        _availablePoints += totalRefund;
        
        // Remove old bonuses and reapply (which will be nothing now)
        ApplyStatBonuses();
        
        // Sync to network
        SyncToNetwork();
        
        if (OnStatsChanged != null)
        {
            OnStatsChanged();
        }
        
        RPGLog.Debug(" Reset all stats. Refunded " + totalRefund + " points.");
    }
    
    /// <summary>
    /// Check for level ups and grant points
    /// Called every frame from RPGItemsMod
    /// </summary>
    public void Update()
    {
        // Check if stats need to be reapplied (hero wasn't ready during load)
        CheckPendingStatReapply();
        
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        int currentLevel = hero.level;
        
        // First-time sync: if _lastKnownLevel is still default (1) but hero is higher level,
        // this means the system was just created and needs to catch up
        if (_lastKnownLevel == 1 && currentLevel > 1 && _availablePoints == 0 && GetTotalAllocatedPoints() == 0)
        {
            // Grant points for all levels gained so far (new run scenario)
            int pointsGained = (currentLevel - 1) * POINTS_PER_LEVEL;
            _availablePoints = pointsGained;
            _lastKnownLevel = currentLevel;
            
            RPGLog.Debug(" Stats catch-up: Hero at level " + currentLevel + ", granting " + pointsGained + " stat points");
            
            if (OnStatsChanged != null)
            {
                OnStatsChanged();
            }
        }
        else if (currentLevel > _lastKnownLevel)
        {
            int levelsGained = currentLevel - _lastKnownLevel;
            int pointsGained = levelsGained * POINTS_PER_LEVEL;
            
            _availablePoints += pointsGained;
            _lastKnownLevel = currentLevel;
            
            // Sync to network
            SyncToNetwork();
            
            if (OnPointsGained != null)
            {
                OnPointsGained(pointsGained);
            }
            if (OnStatsChanged != null)
            {
                OnStatsChanged();
            }
            
            RPGLog.Debug(" Level up! " + (currentLevel - levelsGained) + " -> " + currentLevel + ". Gained " + pointsGained + " stat points. Total available: " + _availablePoints);
        }
    }
    
    /// <summary>
    /// Apply stat bonuses to the hero
    /// </summary>
    private void ApplyStatBonuses()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        
        // Calculate all stat bonuses
        int str = GetStatPoints(StatType.Strength);
        int vit = GetStatPoints(StatType.Vitality);
        int def = GetStatPoints(StatType.Defense);
        int intel = GetStatPoints(StatType.Intelligence);
        int agi = GetStatPoints(StatType.Agility);
        int luck = GetStatPoints(StatType.Luck);
        
        float atkBonus = str * ATK_PER_STRENGTH;
        float hpBonus = vit * HP_PER_VITALITY;
        float defBonus = def * DEF_PER_DEFENSE;
        float apBonus = intel * AP_PER_INTELLIGENCE;
        float moveSpdBonus = agi * MOVE_SPEED_PER_AGILITY;
        float atkSpdBonus = agi * ATTACK_SPEED_PER_AGILITY;
        float critBonus = luck * CRIT_CHANCE_PER_LUCK;
        
        if (NetworkServer.active)
        {
            // HOST: Apply directly
            // Remove old bonus if exists
            if (_appliedStatBonus != null && _bonusAppliedToHero != null)
            {
                if (_bonusAppliedToHero == hero)
                {
                    try
                    {
                        hero.Status.RemoveStatBonus(_appliedStatBonus);
                    }
                    catch (System.Exception e)
                    {
                        RPGLog.Warning(" Failed to remove old stat bonus: " + e.Message);
                    }
                }
                _appliedStatBonus = null;
                _bonusAppliedToHero = null;
            }
            
            // Create new bonus with all stats
            _appliedStatBonus = new StatBonus();
            _appliedStatBonus.attackDamageFlat = atkBonus;
            _appliedStatBonus.maxHealthFlat = hpBonus;
            _appliedStatBonus.armorFlat = defBonus;
            _appliedStatBonus.abilityPowerFlat = apBonus;
            _appliedStatBonus.movementSpeedPercentage = moveSpdBonus;
            _appliedStatBonus.attackSpeedPercentage = atkSpdBonus;
            _appliedStatBonus.critChanceFlat = critBonus;
            
            // Apply the bonus and track which hero it was applied to
            hero.Status.AddStatBonus(_appliedStatBonus);
            _bonusAppliedToHero = hero;
            
            RPGLog.Debug(" Host applied stat bonuses to hero: " + hero.name);
        }
        else
        {
            // CLIENT: Request server to apply stats via chat message
            RequestStatsApply(hero.netId, atkBonus, hpBonus, defBonus, apBonus, moveSpdBonus, atkSpdBonus, critBonus);
            RPGLog.Debug(" Client requested stat bonuses for hero: " + hero.name);
        }
    }
    
    /// <summary>
    /// Request server to apply stats (for clients)
    /// </summary>
    private void RequestStatsApply(uint heroNetId, float atk, float hp, float def, float ap, float moveSpd, float atkSpd, float crit)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            // Format: [RPGSTATS]heroNetId|atk|hp|def|ap|moveSpd|atkSpd|crit
            string content = string.Format("[RPGSTATS]{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                heroNetId, 
                (int)atk, (int)hp, (int)def, (int)ap, 
                moveSpd.ToString("F2"), atkSpd.ToString("F2"), crit.ToString("F4"));
            chatManager.CmdSendChatMessage(content, null);
            RPGLog.Debug(" Sent stats apply request: " + content);
        }
        catch (System.Exception e)
        {
            RPGLog.Warning(" Failed to send stats apply request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Get the bonus value for a specific stat type (for UI display)
    /// </summary>
    public string GetStatBonusDescription(StatType stat)
    {
        int points = GetStatPoints(stat);
        
        switch (stat)
        {
            case StatType.Strength:
                return "+" + (points * ATK_PER_STRENGTH) + " " + Localization.StatAttack;
            case StatType.Vitality:
                return "+" + (points * HP_PER_VITALITY) + " " + Localization.StatHealth;
            case StatType.Defense:
                return "+" + (points * DEF_PER_DEFENSE) + " " + Localization.StatDefense;
            case StatType.Intelligence:
                return "+" + (points * AP_PER_INTELLIGENCE) + " " + Localization.StatAbilityPower;
            case StatType.Agility:
                return "+" + (points * MOVE_SPEED_PER_AGILITY).ToString("F1") + "% " + Localization.StatMoveSpeed + ", +" + (points * ATTACK_SPEED_PER_AGILITY).ToString("F1") + "% " + Localization.StatAttackSpeed;
            case StatType.Luck:
                // CRIT_CHANCE_PER_LUCK is stored as decimal (0.005 = 0.5%), display as percentage
                return "+" + (points * CRIT_CHANCE_PER_LUCK * 100f).ToString("F1") + "% " + Localization.StatCritChance;
            default:
                return "";
        }
    }
    
    /// <summary>
    /// Get localized stat name
    /// </summary>
    public static string GetStatName(StatType stat)
    {
        switch (stat)
        {
            case StatType.Strength: return Localization.StatStrength;
            case StatType.Vitality: return Localization.StatVitality;
            case StatType.Defense: return Localization.StatDefenseName;
            case StatType.Intelligence: return Localization.StatIntelligence;
            case StatType.Agility: return Localization.StatAgility;
            case StatType.Luck: return Localization.StatLuck;
            default: return stat.ToString();
        }
    }
    
    /// <summary>
    /// Get stat description (what it does)
    /// </summary>
    public static string GetStatDescription(StatType stat)
    {
        switch (stat)
        {
            case StatType.Strength:
                return string.Format(Localization.StatStrengthDesc, ATK_PER_STRENGTH);
            case StatType.Vitality:
                return string.Format(Localization.StatVitalityDesc, HP_PER_VITALITY);
            case StatType.Defense:
                return string.Format(Localization.StatDefenseDesc, DEF_PER_DEFENSE);
            case StatType.Intelligence:
                return string.Format(Localization.StatIntelligenceDesc, AP_PER_INTELLIGENCE);
            case StatType.Agility:
                return string.Format(Localization.StatAgilityDesc, MOVE_SPEED_PER_AGILITY, ATTACK_SPEED_PER_AGILITY);
            case StatType.Luck:
                // Display as percentage (0.005 * 100 = 0.5%)
                return string.Format(Localization.StatLuckDesc, CRIT_CHANCE_PER_LUCK * 100f);
            default:
                return "";
        }
    }
    
    // ============================================================
    // NETWORK SYNC
    // ============================================================
    
    /// <summary>
    /// Sync stats to network using Actor.persistentSyncedData
    /// </summary>
    private void SyncToNetwork()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        if (!NetworkServer.active) return; // Only server can set synced data
        
        Hero hero = DewPlayer.local.hero;
        
        // Serialize stats data manually (JsonUtility doesn't work well with Dictionary)
        string data = SerializeStats();
        
        // Store in hero's persistent synced data
        hero.persistentSyncedData[SYNC_KEY_PREFIX + "data"] = data;
        
        RPGLog.Debug(" Synced stats to network: " + data);
    }
    
    /// <summary>
    /// Load stats from network sync data
    /// </summary>
    public void LoadFromNetwork()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        
        string key = SYNC_KEY_PREFIX + "data";
        if (!hero.persistentSyncedData.ContainsKey(key)) return;
        
        try
        {
            string data = hero.persistentSyncedData[key];
            DeserializeStats(data);
            
            // Reapply bonuses
            ApplyStatBonuses();
            
            if (OnStatsChanged != null)
            {
                OnStatsChanged();
            }
            
            RPGLog.Debug(" Loaded stats from network: " + data);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to load stats from network: " + e.Message);
        }
    }
    
    /// <summary>
    /// Serialize stats to string
    /// </summary>
    private string SerializeStats()
    {
        // Format: availablePoints|lastKnownLevel|str|vit|def|int|agi|luck
        return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
            _availablePoints,
            _lastKnownLevel,
            GetStatPoints(StatType.Strength),
            GetStatPoints(StatType.Vitality),
            GetStatPoints(StatType.Defense),
            GetStatPoints(StatType.Intelligence),
            GetStatPoints(StatType.Agility),
            GetStatPoints(StatType.Luck));
    }
    
    /// <summary>
    /// Deserialize stats from string
    /// </summary>
    private void DeserializeStats(string data)
    {
        if (string.IsNullOrEmpty(data)) return;
        
        string[] parts = data.Split('|');
        if (parts.Length < 8) return;
        
        _availablePoints = int.Parse(parts[0]);
        _lastKnownLevel = int.Parse(parts[1]);
        _allocatedStats[StatType.Strength] = int.Parse(parts[2]);
        _allocatedStats[StatType.Vitality] = int.Parse(parts[3]);
        _allocatedStats[StatType.Defense] = int.Parse(parts[4]);
        _allocatedStats[StatType.Intelligence] = int.Parse(parts[5]);
        _allocatedStats[StatType.Agility] = int.Parse(parts[6]);
        _allocatedStats[StatType.Luck] = int.Parse(parts[7]);
    }
    
    /// <summary>
    /// Save stats for persistence
    /// </summary>
    public string Serialize()
    {
        return SerializeStats();
    }
    
    /// <summary>
    /// Load stats from persistence
    /// </summary>
    public void Deserialize(string data)
    {
        if (string.IsNullOrEmpty(data)) return;
        
        try
        {
            DeserializeStats(data);
            
            // Try to reapply bonuses after loading
            if (DewPlayer.local == null || DewPlayer.local.hero == null)
            {
                // Hero not ready, mark for retry
                _needsStatReapply = true;
                RPGLog.Debug(" Stats loaded but hero not ready, will apply when available");
            }
            else
            {
                ApplyStatBonuses();
            }
            
            if (OnStatsChanged != null)
            {
                OnStatsChanged();
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to deserialize stats: " + e.Message);
        }
    }
    
    /// <summary>
    /// Check if stats need to be reapplied (called from Update)
    /// </summary>
    public void CheckPendingStatReapply()
    {
        if (!_needsStatReapply) return;
        
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        RPGLog.Debug(" Hero now ready, applying saved stats...");
        _needsStatReapply = false;
        ApplyStatBonuses();
    }
    
    /// <summary>
    /// Reset for new run
    /// </summary>
    public void ResetForNewRun()
    {
        _allocatedStats[StatType.Strength] = 0;
        _allocatedStats[StatType.Vitality] = 0;
        _allocatedStats[StatType.Defense] = 0;
        _allocatedStats[StatType.Intelligence] = 0;
        _allocatedStats[StatType.Agility] = 0;
        _allocatedStats[StatType.Luck] = 0;
        
        // Grant starting points based on current hero level
        int startingLevel = 1;
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            startingLevel = DewPlayer.local.hero.level;
        }
        
        // Grant points for all levels (level 1 = 0 points, level 2 = 3 points, etc.)
        _availablePoints = (startingLevel - 1) * POINTS_PER_LEVEL;
        _lastKnownLevel = startingLevel;
        
        // Remove applied bonus - only from the hero it was applied to
        if (_appliedStatBonus != null && _bonusAppliedToHero != null)
        {
            try
            {
                _bonusAppliedToHero.Status.RemoveStatBonus(_appliedStatBonus);
            }
            catch { }
        }
        _appliedStatBonus = null;
        _bonusAppliedToHero = null;
        
        if (OnStatsChanged != null)
        {
            OnStatsChanged();
        }
        
        RPGLog.Debug(" Stats reset for new run. Starting level: " + startingLevel + ", Available points: " + _availablePoints);
    }
    
    /// <summary>
    /// Cleanup when leaving game
    /// </summary>
    public void Cleanup()
    {
        // Remove applied bonus - only from the hero it was applied to
        if (_appliedStatBonus != null && _bonusAppliedToHero != null)
        {
            try
            {
                _bonusAppliedToHero.Status.RemoveStatBonus(_appliedStatBonus);
            }
            catch { }
        }
        _appliedStatBonus = null;
        _bonusAppliedToHero = null;
    }
}
