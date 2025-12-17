using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Mirror;

/// <summary>
/// Patches to enhance BOTH game score and hero mastery points based on RPG mod activities.
/// Tracks: items collected, upgrades done, set bonuses activated, etc.
/// Score bonus is added to the game's total score display.
/// Mastery bonus is added to hero mastery points for unlocking content.
/// </summary>
public static class ScoringPatches
{
    // ============================================================
    // TRACKING VARIABLES (reset each run)
    // ============================================================
    
    private static int _itemsCollected = 0;
    private static int _itemsUpgraded = 0;
    private static int _itemsDismantled = 0;
    private static int _itemsSold = 0;
    private static int _itemsCleansed = 0;
    private static int _itemsMerged = 0;
    private static int _legendaryItemsFound = 0;
    private static int _epicItemsFound = 0;
    private static int _rareItemsFound = 0;
    private static int _setBonusesActivated = 0;
    private static int _consumablesUsed = 0;
    private static int _weaponMasteryLevelsGained = 0;
    private static int _statPointsSpent = 0;
    
    // ============================================================
    // PUBLIC TRACKING METHODS (called from other parts of the mod)
    // ============================================================
    
    public static void TrackItemCollected(ItemRarity rarity)
    {
        _itemsCollected++;
        switch (rarity)
        {
            case ItemRarity.Legendary:
                _legendaryItemsFound++;
                break;
            case ItemRarity.Epic:
                _epicItemsFound++;
                break;
            case ItemRarity.Rare:
                _rareItemsFound++;
                break;
        }
    }
    
    public static void TrackItemUpgraded()
    {
        _itemsUpgraded++;
    }
    
    public static void TrackItemDismantled()
    {
        _itemsDismantled++;
    }
    
    public static void TrackItemSold()
    {
        _itemsSold++;
    }
    
    public static void TrackItemCleansed()
    {
        _itemsCleansed++;
    }
    
    public static void TrackItemMerged()
    {
        _itemsMerged++;
    }
    
    public static void TrackSetBonusActivated()
    {
        _setBonusesActivated++;
    }
    
    public static void TrackConsumableUsed()
    {
        _consumablesUsed++;
    }
    
    public static void TrackWeaponMasteryLevelGained()
    {
        _weaponMasteryLevelsGained++;
    }
    
    public static void TrackStatPointSpent()
    {
        _statPointsSpent++;
    }
    
    // Legacy methods - kept for backwards compatibility but do nothing
    public static void TrackGoldFromMidas(int amount) { }
    public static void TrackDustFromMod(int amount) { }
    
    /// <summary>
    /// Reset all tracking variables (call at start of new run)
    /// </summary>
    public static void ResetTracking()
    {
        _itemsCollected = 0;
        _itemsUpgraded = 0;
        _itemsDismantled = 0;
        _itemsSold = 0;
        _itemsCleansed = 0;
        _itemsMerged = 0;
        _legendaryItemsFound = 0;
        _epicItemsFound = 0;
        _rareItemsFound = 0;
        _setBonusesActivated = 0;
        _consumablesUsed = 0;
        _weaponMasteryLevelsGained = 0;
        _statPointsSpent = 0;
        
        RPGLog.Debug(" Scoring tracking reset for new run");
    }
    
    // ============================================================
    // GETTERS FOR UI
    // ============================================================
    
    public static int GetItemsCollected() { return _itemsCollected; }
    public static int GetItemsUpgraded() { return _itemsUpgraded; }
    public static int GetItemsDismantled() { return _itemsDismantled; }
    public static int GetItemsSold() { return _itemsSold; }
    public static int GetItemsCleansed() { return _itemsCleansed; }
    public static int GetItemsMerged() { return _itemsMerged; }
    public static int GetLegendaryItemsFound() { return _legendaryItemsFound; }
    public static int GetEpicItemsFound() { return _epicItemsFound; }
    public static int GetRareItemsFound() { return _rareItemsFound; }
    public static int GetSetBonusesActivated() { return _setBonusesActivated; }
    public static int GetConsumablesUsed() { return _consumablesUsed; }
    public static int GetWeaponMasteryLevelsGained() { return _weaponMasteryLevelsGained; }
    public static int GetStatPointsSpent() { return _statPointsSpent; }
    
    /// <summary>
    /// Calculate bonus SCORE from RPG mod activities.
    /// This is added to the game's Total Score display.
    /// </summary>
    public static long CalculateBonusScore()
    {
        long bonus = 0;
        
        // Item collection score bonuses
        bonus += _itemsCollected * 100;            // 100 score per item collected
        bonus += _legendaryItemsFound * 5000;      // 5000 score for legendary
        bonus += _epicItemsFound * 2000;           // 2000 score for epic
        bonus += _rareItemsFound * 500;            // 500 score for rare
        
        // Crafting/upgrade score bonuses
        bonus += _itemsUpgraded * 500;             // 500 score per upgrade
        bonus += _itemsMerged * 1000;              // 1000 score per merge
        bonus += _itemsCleansed * 300;             // 300 score per cleanse
        bonus += _itemsDismantled * 50;            // 50 score per dismantle
        bonus += _itemsSold * 25;                  // 25 score per sell
        
        // Progression score bonuses
        bonus += _setBonusesActivated * 1500;      // 1500 score per set bonus
        bonus += _weaponMasteryLevelsGained * 800; // 800 score per mastery level
        bonus += _statPointsSpent * 200;           // 200 score per stat point
        
        // Consumable usage
        bonus += _consumablesUsed * 75;            // 75 score per consumable used
        
        return bonus;
    }
    
    /// <summary>
    /// Calculate bonus MASTERY POINTS from RPG mod activities.
    /// This is added to hero mastery points for unlocking heroes/skins.
    /// IMPORTANT: Keep these values LOW - this is a mod bonus, not a replacement for natural gameplay!
    /// Base game gives ~3000-10000 mastery per run. Our bonus should be ~5-15% of that at most.
    /// </summary>
    public static long CalculateBonusMasteryPoints()
    {
        long bonus = 0;
        
        // Item collection mastery bonuses - SMALL values
        bonus += _itemsCollected * 5;              // 5 mastery per item collected
        bonus += _legendaryItemsFound * 100;       // 100 mastery for legendary (rare find!)
        bonus += _epicItemsFound * 30;             // 30 mastery for epic
        bonus += _rareItemsFound * 10;             // 10 mastery for rare
        
        // Crafting/upgrade mastery bonuses - reward engagement
        bonus += _itemsUpgraded * 15;              // 15 mastery per upgrade
        bonus += _itemsMerged * 25;                // 25 mastery per merge
        bonus += _itemsCleansed * 10;              // 10 mastery per cleanse
        bonus += _itemsDismantled * 2;             // 2 mastery per dismantle
        bonus += _itemsSold * 1;                   // 1 mastery per sell
        
        // Progression mastery bonuses
        bonus += _setBonusesActivated * 50;        // 50 mastery per set bonus
        bonus += _weaponMasteryLevelsGained * 20;  // 20 mastery per mastery level
        bonus += _statPointsSpent * 5;             // 5 mastery per stat point
        
        // Consumable usage
        bonus += _consumablesUsed * 3;             // 3 mastery per consumable used
        
        return bonus;
    }
    
    /// <summary>
    /// Get a summary string of RPG activities for logging
    /// </summary>
    public static string GetSummary()
    {
        return string.Format(
            "Items: {0} (L:{1} E:{2} R:{3}) | Upgrades: {4} | Merges: {5} | Sets: {6} | Mastery Lvls: {7} | Stats: {8}",
            _itemsCollected, _legendaryItemsFound, _epicItemsFound, _rareItemsFound,
            _itemsUpgraded, _itemsMerged, _setBonusesActivated, _weaponMasteryLevelsGained, _statPointsSpent);
    }
    
    // ============================================================
    // HARMONY PATCHES
    // ============================================================
    
    /// <summary>
    /// Enhance mastery points calculation to include RPG mod bonuses.
    /// Patches Dew.GetRewardedMasteryPoints to add our mastery bonus.
    /// </summary>
    [HarmonyPatch(typeof(Dew), "GetRewardedMasteryPoints")]
    public static class EnhancedMasteryPointsPatch
    {
        public static void Postfix(ref long __result, float minutes)
        {
            try
            {
                long rpgBonus = CalculateBonusMasteryPoints();
                
                if (rpgBonus > 0)
                {
                    long originalPoints = __result;
                    __result += rpgBonus;
                    
                    RPGLog.Debug(string.Format(" Mastery bonus: Base {0:N0} + RPG {1:N0} = {2:N0}", 
                        originalPoints, rpgBonus, __result));
                }
            }
            catch (Exception ex)
            {
                RPGLog.Error(" Error in EnhancedMasteryPointsPatch: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Enhance total score calculation to include RPG mod bonuses.
    /// Patches UI_InGame_ResultView.Refresh to add our score bonus to totalScore.
    /// </summary>
    [HarmonyPatch(typeof(UI_InGame_ResultView), "Refresh")]
    public static class EnhancedScorePatch
    {
        public static void Postfix(UI_InGame_ResultView __instance)
        {
            try
            {
                long rpgScoreBonus = CalculateBonusScore();
                
                if (rpgScoreBonus > 0 && __instance.totalScoreText != null)
                {
                    // Parse current score from text
                    string currentText = __instance.totalScoreText.text;
                    
                    // The format is something like "77,722 points" - extract the number
                    string numberPart = "";
                    foreach (char c in currentText)
                    {
                        if (char.IsDigit(c))
                        {
                            numberPart += c;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(numberPart))
                    {
                        long currentScore = 0;
                        if (long.TryParse(numberPart, out currentScore))
                        {
                            long newScore = currentScore + rpgScoreBonus;
                            
                            // Update the text with new score
                            __instance.totalScoreText.text = string.Format(
                                DewLocalization.GetUIValue("InGame_Result_ScoreFormat"), 
                                newScore.ToString("#,##0"));
                            
                            RPGLog.Debug(string.Format(" Score bonus: Base {0:N0} + RPG {1:N0} = {2:N0}", 
                                currentScore, rpgScoreBonus, newScore));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RPGLog.Error(" Error in EnhancedScorePatch: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Log RPG stats when game result is updated (for debugging)
    /// </summary>
    [HarmonyPatch(typeof(GameResultManager), "UpdateGameResult")]
    public static class EnhancedGameResultPatch
    {
        public static void Postfix(GameResultManager __instance)
        {
            try
            {
                if (__instance.tracked == null) return;
                
                // Log summary for debugging (only if there were RPG activities)
                if (_itemsCollected > 0)
                {
                    RPGLog.Debug(" Run stats: " + GetSummary());
                }
            }
            catch (Exception ex)
            {
                RPGLog.Error(" Error in EnhancedGameResultPatch: " + ex.Message);
            }
        }
    }
}
