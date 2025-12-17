using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace InfiniteDungeonMod
{
    // ============================================================
    // LOCALIZATION SYSTEM - JSON-based Multi-language Support
    // Supports all game languages: en-US, zh-CN, zh-TW, ja-JP, ko-KR, 
    // es-MX, fr-FR, de-DE, it-IT, pt-BR, ru-RU, tr-TR, pl-PL
    // Uses i18n/[language-code].json files
    // ============================================================
    
    /// <summary>
    /// Localization for the Infinite Dungeon mod.
    /// All text matches the game's lore and dream-themed narrative.
    /// Loads translations from JSON files in the i18n folder.
    /// Automatically detects language from game settings.
    /// </summary>
    public static class DungeonLocalization
    {
        private static string _currentLanguageCode = "en-US";
        private static Dictionary<string, string> _localizationData = new Dictionary<string, string>();
        private static bool _isInitialized = false;
        private static string _modPath = null; // Set by mod during initialization
        
        // Supported language codes (matches game's supported languages)
        private static readonly string[] SupportedLanguages = new string[]
        {
            "en-US", "zh-CN", "zh-TW", "ja-JP", "ko-KR", "es-MX", 
            "fr-FR", "de-DE", "it-IT", "pt-BR", "ru-RU", "tr-TR", "pl-PL"
        };
        
        public static string CurrentLanguageCode
        {
            get { return _currentLanguageCode; }
        }
        
        /// <summary>
        /// Set the mod path (from mod.path) - must be called before DetectLanguage()
        /// This is the correct path whether the mod is installed locally or via Steam Workshop
        /// </summary>
        public static void SetModPath(string path)
        {
            _modPath = path;
            Debug.Log("[InfiniteDungeon] Localization mod path set to: " + path);
        }
        
        // Auto-detect language from game settings
        public static void DetectLanguage()
        {
            try
            {
                string gameLang = "en-US"; // Default fallback
                if (DewSave.profileMain != null && !string.IsNullOrEmpty(DewSave.profileMain.language))
                {
                    gameLang = DewSave.profileMain.language;
                }
                
                // Validate language code - use en-US if not supported
                bool isSupported = false;
                foreach (string lang in SupportedLanguages)
                {
                    if (lang == gameLang)
                    {
                        isSupported = true;
                        break;
                    }
                }
                
                if (!isSupported)
                {
                    // Try to find a matching language by prefix (e.g., "zh" -> "zh-CN")
                    foreach (string lang in SupportedLanguages)
                    {
                        if (lang.StartsWith(gameLang.Substring(0, Math.Min(2, gameLang.Length))))
                        {
                            gameLang = lang;
                            isSupported = true;
                            break;
                        }
                    }
                }
                
                if (!isSupported)
                {
                    gameLang = "en-US"; // Fallback to English
                }
                
                _currentLanguageCode = gameLang;
                LoadLocalizationData();
            }
            catch
            {
                _currentLanguageCode = "en-US";
                LoadLocalizationData();
            }
        }
        
        public static bool IsChinese { get { return _currentLanguageCode.StartsWith("zh"); } }
        
        /// <summary>
        /// Load localization data from JSON file based on current language
        /// Falls back to en-US if the current language file doesn't exist
        /// </summary>
        private static void LoadLocalizationData()
        {
            try
            {
                // Find the mod directory
                string modDir = GetModDirectory();
                if (string.IsNullOrEmpty(modDir))
                {
                    Debug.LogWarning("[InfiniteDungeon] Could not find mod directory for i18n files. Using hardcoded fallbacks.");
                    _localizationData = new Dictionary<string, string>();
                    _isInitialized = true; // Mark as initialized so we use fallbacks
                    return;
                }
                
                // Try to load the current language file
                string jsonPath = Path.Combine(modDir, "i18n", _currentLanguageCode + ".json");
                
                // If current language file doesn't exist, fall back to en-US
                if (!File.Exists(jsonPath))
                {
                    Debug.LogWarning("[InfiniteDungeon] Localization file not found: " + jsonPath + ", falling back to en-US");
                    jsonPath = Path.Combine(modDir, "i18n", "en-US.json");
                    
                    if (!File.Exists(jsonPath))
                    {
                        Debug.LogWarning("[InfiniteDungeon] English localization file not found: " + jsonPath + ". Using hardcoded fallbacks.");
                        _localizationData = new Dictionary<string, string>();
                        _isInitialized = true; // Mark as initialized so we use fallbacks
                        return;
            }
        }
        
                // Read and parse JSON
                string jsonContent = File.ReadAllText(jsonPath);
                
                // JsonUtility doesn't support Dictionary directly, so we parse manually
                _localizationData = ParseSimpleJson(jsonContent);
                
                if (_localizationData.Count == 0)
                {
                    Debug.LogWarning("[InfiniteDungeon] JSON parsing returned 0 entries for: " + jsonPath);
                }
                else
                {
                    Debug.Log("[InfiniteDungeon] Loaded " + _localizationData.Count + " localization entries for language: " + _currentLanguageCode);
                }
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error loading localization: " + ex.Message + "\nStackTrace: " + ex.StackTrace);
                _localizationData = new Dictionary<string, string>();
                _isInitialized = true; // Mark as initialized so we use fallbacks
            }
        }
        
        /// <summary>
        /// Simple JSON parser for key-value string pairs
        /// Handles basic JSON format: {"key1": "value1", "key2": "value2"}
        /// </summary>
        private static Dictionary<string, string> ParseSimpleJson(string json)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            
            try
            {
                // Remove whitespace and braces
                json = json.Trim();
                if (json.StartsWith("{")) json = json.Substring(1);
                if (json.EndsWith("}")) json = json.Substring(0, json.Length - 1);
                json = json.Trim();
                
                if (string.IsNullOrEmpty(json)) return result;
                
                // Split by comma, but respect quoted strings
                List<string> pairs = new List<string>();
                bool inString = false;
                bool escaped = false;
                int start = 0;
                
                for (int i = 0; i < json.Length; i++)
                {
                    char c = json[i];
                    
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }
                    
                    if (c == '\\')
                    {
                        escaped = true;
                        continue;
                    }
                    
                    if (c == '"')
                    {
                        inString = !inString;
                    }
                    else if (c == ',' && !inString)
                    {
                        string pair = json.Substring(start, i - start).Trim();
                        if (!string.IsNullOrEmpty(pair))
                        {
                            pairs.Add(pair);
                        }
                        start = i + 1;
                    }
                }
                
                // Add last pair
                if (start < json.Length)
                {
                    string pair = json.Substring(start).Trim();
                    if (!string.IsNullOrEmpty(pair))
                    {
                        pairs.Add(pair);
                    }
                }
                
                // Parse each key-value pair
                foreach (string pair in pairs)
                {
                    int colonIndex = pair.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        string key = ExtractQuotedString(pair.Substring(0, colonIndex).Trim());
                        string value = ExtractQuotedString(pair.Substring(colonIndex + 1).Trim());
                        
                        if (!string.IsNullOrEmpty(key))
                        {
                            result[key] = value ?? "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error parsing JSON: " + ex.Message);
            }
            
            return result;
        }
        
        /// <summary>
        /// Extract string value from JSON quoted string, handling escape sequences
        /// </summary>
        private static string ExtractQuotedString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            
            str = str.Trim();
            
            // Remove surrounding quotes
            if (str.StartsWith("\"") && str.EndsWith("\""))
            {
                str = str.Substring(1, str.Length - 2);
            }
            
            // Handle escape sequences
            str = str.Replace("\\n", "\n")
                     .Replace("\\r", "\r")
                     .Replace("\\t", "\t")
                     .Replace("\\\"", "\"")
                     .Replace("\\\\", "\\");
            
            return str;
        }
        
        /// <summary>
        /// Get the mod directory path
        /// </summary>
        private static string GetModDirectory()
        {
            try
            {
                // PRIORITY 1: Use mod.path if it was set (correct for both local and workshop mods)
                if (!string.IsNullOrEmpty(_modPath))
                {
                    if (Directory.Exists(_modPath) && Directory.Exists(Path.Combine(_modPath, "i18n")))
                    {
                        return _modPath;
                    }
                    else
                    {
                        Debug.LogWarning("[InfiniteDungeon] Mod path set but i18n folder not found: " + _modPath);
                    }
                }
                
                // PRIORITY 2: Fallback to searching common locations (for development/testing)
                string[] possiblePaths = new string[]
                {
                    Path.Combine(Application.dataPath, "..", "Mods", "InfiniteDungeonMod"),
                    Path.Combine(Application.dataPath, "Mods", "InfiniteDungeonMod"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Mods", "InfiniteDungeonMod"),
                };
                
                foreach (string path in possiblePaths)
                {
                    string normalizedPath = Path.GetFullPath(path);
                    if (Directory.Exists(normalizedPath) && Directory.Exists(Path.Combine(normalizedPath, "i18n")))
                    {
                        return normalizedPath;
                    }
                }
                
                // PRIORITY 3: Search all mod directories
                string modsDir = Path.Combine(Application.dataPath, "..", "Mods");
                modsDir = Path.GetFullPath(modsDir);
                
                if (Directory.Exists(modsDir))
                {
                    foreach (string dir in Directory.GetDirectories(modsDir))
                    {
                        string dirName = Path.GetFileName(dir).ToLowerInvariant();
                        if (dirName.Contains("infinitedungeon"))
                        {
                            if (File.Exists(Path.Combine(dir, "i18n", "en-US.json")))
                            {
                                return dir;
                            }
                        }
                    }
                }
                
                Debug.LogWarning("[InfiniteDungeon] Could not find mod directory. ModPath=" + (_modPath ?? "null") + ", Searched: " + modsDir);
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error finding mod directory: " + ex.Message);
            }
            
            return null;
        }
        
        /// <summary>
        /// Get localized string by key
        /// </summary>
        private static string GetString(string key, params object[] args)
        {
            if (!_isInitialized)
            {
                LoadLocalizationData();
            }
            
            string value;
            if (_localizationData.TryGetValue(key, out value))
            {
                if (args != null && args.Length > 0)
                {
                    try
                    {
                        return string.Format(value, args);
                    }
                    catch
                    {
                        return value;
                    }
                }
                return value;
            }
            
            // Fallback to hardcoded English for critical UI elements
            string fallback = GetHardcodedFallback(key);
            if (fallback != null)
            {
                if (args != null && args.Length > 0)
                {
                    try { return string.Format(fallback, args); } catch { return fallback; }
                }
                return fallback;
            }
            
            Debug.LogWarning("[InfiniteDungeon] Missing localization key: " + key);
            return "[" + key + "]";
        }
        
        /// <summary>
        /// Hardcoded English fallbacks for critical UI elements (Result window, Intro window, etc.)
        /// Used when JSON loading fails completely
        /// </summary>
        private static string GetHardcodedFallback(string key)
        {
            switch (key)
            {
                // Result window
                case "ResultTitle": return "Endless Dream - Journey's End";
                case "ResultSubtitle": return "Your Dream Exploration Statistics";
                case "ResultDepthReached": return "Depth Reached";
                case "ResultNodesExplored": return "Nodes Explored";
                case "ResultInfectedNodes": return "Infected Nodes";
                case "ResultCursesEndured": return "Curses Endured";
                case "ResultWeakenedBosses": return "Weakened Bosses";
                case "ResultScoreBonus": return "Score Bonus";
                case "ResultMasteryBonus": return "Mastery Bonus";
                case "ResultSummaryVictory": return "You have conquered the endless dream.";
                case "ResultSummaryGameOver": return "The dream consumed your consciousness...";
                case "ResultSummaryConcede": return "You chose to awaken from the dream...";
                case "ResultDepthRank_100": return "Dream Conqueror";
                case "ResultDepthRank_75": return "Abyss Walker";
                case "ResultDepthRank_50": return "Dream Explorer";
                case "ResultDepthRank_30": return "Awakened";
                case "ResultDepthRank_20": return "Traveler";
                case "ResultDepthRank_10": return "Adventurer";
                case "ResultDepthRank_Default": return "Dreamwalker";
                case "ResultButtonClose": return "Continue";
                
                // Intro window
                case "IntroTitle": return "The Endless Dream";
                case "IntroSubtitle": return "An infinite dungeon exploration mod";
                case "IntroButtonOk": return "~ Let the Dream Begin ~";
                
                // Common messages
                case "WelcomeMessage": return "The Endless Dream awakens...";
                case "DepthLocationFormat": return "Depth {0} - {1}";
                case "FallbackZoneName": return "Infinite Dungeon";
                
                default: return null;
            }
        }
        
        // ============================================================
        // DUNGEON ENTRY / WELCOME MESSAGES
        // ============================================================
        public static string WelcomeMessage { get { return GetString("WelcomeMessage"); } }
        public static string DepthAnnounce { get { return GetString("DepthAnnounce", "{0}"); } }
        public static string DepthLocationFormat { get { return GetString("DepthLocationFormat"); } }
        public static string FallbackZoneName { get { return GetString("FallbackZoneName"); } }
        
        // ============================================================
        // ZONE ENTRY MESSAGES
        // ============================================================
        public static string GetZoneEntryMessage(string zoneName)
        {
            // zoneName comes in as "Zone_Forest", "Zone_LavaLand", etc.
            // The JSON keys are already "Zone_Forest", "Zone_LavaLand", etc.
            // So we use the zoneName directly as the key
            string key = zoneName;
            
            // Handle case where zoneName doesn't have Zone_ prefix (shouldn't happen, but just in case)
            if (!string.IsNullOrEmpty(zoneName) && !zoneName.StartsWith("Zone_"))
            {
                key = "Zone_" + zoneName;
            }
            
            if (_localizationData.ContainsKey(key))
            {
                return GetString(key);
            }
            return GetString("Zone_Default");
        }
        
        // ============================================================
        // ROOM MODIFIER MESSAGES
        // ============================================================
        public static string GetModifierMessage(string modifierName)
        {
            string key = "Mod_" + modifierName;
            return _localizationData.ContainsKey(key) ? GetString(key) : null;
        }
        
        // ============================================================
        // BOSS ROOM MESSAGES
        // ============================================================
        public static string BossRoomEntry { get { return GetString("BossRoomEntry"); } }
        public static string BossRoomVictory { get { return GetString("BossRoomVictory"); } }
        
        // ============================================================
        // EXPLORATION MESSAGES
        // ============================================================
        public static string ShortcutDiscovered { get { return GetString("ShortcutDiscovered"); } }
        public static string DeadEndReached { get { return GetString("DeadEndReached"); } }
        public static string RevisitingRoom { get { return GetString("RevisitingRoom"); } }
        
        // ============================================================
        // DEPTH MILESTONE MESSAGES
        // ============================================================
        public static string GetDepthMilestone(int depth)
        {
            if (depth == 5) return GetString("Milestone_5");
            if (depth == 10) return GetString("Milestone_10");
            if (depth == 15) return GetString("Milestone_15");
            if (depth == 20) return GetString("Milestone_20");
            if (depth == 25) return GetString("Milestone_25");
            if (depth % 10 == 0 && depth > 25) return GetString("Milestone_Generic_10", depth);
            if (depth % 5 == 0) return GetString("Milestone_Generic_5", depth);
            return null;
        }
        
        // ============================================================
        // MERCHANT / EVENT ROOMS
        // ============================================================
        public static string MerchantRoom { get { return GetString("MerchantRoom"); } }
        public static string EventRoom { get { return GetString("EventRoom"); } }
        
        // ============================================================
        // HUNTER INFECTION
        // ============================================================
        public static string HunterInfected { get { return GetString("HunterInfected"); } }
        public static string HunterEncounter { get { return GetString("HunterEncounter"); } }
        public static string PlayerInfected { get { return GetString("PlayerInfected"); } }
        public static string PlayerSpreadInfection { get { return GetString("PlayerSpreadInfection"); } }
        public static string PlayerInfectionCured { get { return GetString("PlayerInfectionCured"); } }
        public static string InfectionResisted { get { return GetString("InfectionResisted"); } }
        public static string PlayerInfectionCharges(int remaining) { return GetString("PlayerInfectionCharges", remaining); }
        public static string HunterPathWarning { get { return GetString("HunterPathWarning"); } }
        
        // ============================================================
        // HUNTER CURSES
        // ============================================================
        public static string CurseMeteorFury { get { return GetString("Curse_MeteorFury"); } }
        public static string CurseInkStorm { get { return GetString("Curse_InkStorm"); } }
        public static string CurseFlameEngulf { get { return GetString("Curse_FlameEngulf"); } }
        public static string CurseToxicMiasma { get { return GetString("Curse_ToxicMiasma"); } }
        public static string CurseBlackRain { get { return GetString("Curse_BlackRain"); } }
        public static string CurseDarkVeil { get { return GetString("Curse_DarkVeil"); } }
        public static string CurseGravity { get { return GetString("Curse_GravityCurse"); } }
        public static string CurseWeakenedSpirit { get { return GetString("Curse_WeakenedSpirit"); } }
        public static string CurseSlowMind { get { return GetString("Curse_SlowMind"); } }
        public static string CurseVulnerability { get { return GetString("Curse_Vulnerability"); } }
        public static string CurseTimewarp { get { return GetString("Curse_TimewarpCurse"); } }
        public static string CurseMonsterRage { get { return GetString("Curse_MonsterRage"); } }
        public static string CurseIronHide { get { return GetString("Curse_IronHide"); } }
        public static string CurseFrenzy { get { return GetString("Curse_Frenzy"); } }
        public static string CurseExpired { get { return GetString("Curse_Expired"); } }
        
        public static string GetCurseName(InfiniteDungeonMod.HunterCurseType curse)
        {
            switch (curse)
            {
                case InfiniteDungeonMod.HunterCurseType.MeteorFury: return GetString("CurseName_MeteorFury");
                case InfiniteDungeonMod.HunterCurseType.InkStorm: return GetString("CurseName_InkStorm");
                case InfiniteDungeonMod.HunterCurseType.FlameEngulf: return GetString("CurseName_FlameEngulf");
                case InfiniteDungeonMod.HunterCurseType.ToxicMiasma: return GetString("CurseName_ToxicMiasma");
                case InfiniteDungeonMod.HunterCurseType.BlackRain: return GetString("CurseName_BlackRain");
                case InfiniteDungeonMod.HunterCurseType.DarkVeil: return GetString("CurseName_DarkVeil");
                case InfiniteDungeonMod.HunterCurseType.GravityCurse: return GetString("CurseName_GravityCurse");
                case InfiniteDungeonMod.HunterCurseType.WeakenedSpirit: return GetString("CurseName_WeakenedSpirit");
                case InfiniteDungeonMod.HunterCurseType.SlowMind: return GetString("CurseName_SlowMind");
                case InfiniteDungeonMod.HunterCurseType.Vulnerability: return GetString("CurseName_Vulnerability");
                case InfiniteDungeonMod.HunterCurseType.TimewarpCurse: return GetString("CurseName_TimewarpCurse");
                case InfiniteDungeonMod.HunterCurseType.MonsterRage: return GetString("CurseName_MonsterRage");
                case InfiniteDungeonMod.HunterCurseType.IronHide: return GetString("CurseName_IronHide");
                case InfiniteDungeonMod.HunterCurseType.Frenzy: return GetString("CurseName_Frenzy");
                default: return "";
            }
        }
        
        // ============================================================
        // WEAKENED BOSS MESSAGES
        // ============================================================
        public static string WeakenedBossDefeated { get { return GetString("WeakenedBossDefeated"); } }
        
        private static string GetBossDisplayName(string bossName)
        {
            string key = "BossName_" + bossName;
            if (_localizationData.ContainsKey(key))
            {
                return GetString(key);
            }
            
            // Fallback: Clean up the prefab name
            string cleanName = bossName;
            if (cleanName.Contains("Boss"))
            {
                int bossIndex = cleanName.LastIndexOf("Boss");
                if (bossIndex >= 0 && bossIndex + 4 < cleanName.Length)
                {
                    cleanName = cleanName.Substring(bossIndex + 4);
                }
            }
            return cleanName;
        }
        
        private static string GetWeakenedBossPrompt(string bossName)
        {
            string key = "BossPrompt_" + bossName;
            if (_localizationData.ContainsKey(key))
            {
                return GetString(key);
            }
            
            // Default prompt with boss name
            string displayName = GetBossDisplayName(bossName);
            return GetString("BossPrompt_Default", displayName);
        }
        
        // ============================================================
        // INTRODUCTION WINDOW
        // ============================================================
        public static string IntroTitle { get { return GetString("IntroTitle"); } }
        public static string IntroSubtitle { get { return GetString("IntroSubtitle"); } }
        public static string IntroLore1 { get { return GetString("IntroLore1"); } }
        public static string IntroLore2 { get { return GetString("IntroLore2"); } }
        public static string IntroLore3 { get { return GetString("IntroLore3"); } }
        public static string IntroFeatureTitle { get { return GetString("IntroFeatureTitle"); } }
        public static string IntroFeature1 { get { return GetString("IntroFeature1"); } }
        public static string IntroFeature2 { get { return GetString("IntroFeature2"); } }
        public static string IntroFeature3 { get { return GetString("IntroFeature3"); } }
        public static string IntroFeature4 { get { return GetString("IntroFeature4"); } }
        public static string IntroFeature5 { get { return GetString("IntroFeature5"); } }
        public static string IntroFeature6 { get { return GetString("IntroFeature6"); } }
        public static string IntroEssenceNerfTitle { get { return GetString("IntroEssenceNerfTitle"); } }
        public static string IntroEssenceNerfDesc { get { return GetString("IntroEssenceNerfDesc"); } }
        public static string IntroCredits { get { return GetString("IntroCredits"); } }
        public static string IntroPersonalMessage { get { return GetString("IntroPersonalMessage"); } }
        public static string IntroButtonOk { get { return GetString("IntroButtonOk"); } }
        
        // ============================================================
        // GAME RESULT WINDOW
        // ============================================================
        public static string ResultTitle { get { return GetString("ResultTitle"); } }
        public static string ResultSubtitle { get { return GetString("ResultSubtitle"); } }
        public static string ResultDepthReached { get { return GetString("ResultDepthReached"); } }
        public static string ResultNodesExplored { get { return GetString("ResultNodesExplored"); } }
        public static string ResultInfectedNodes { get { return GetString("ResultInfectedNodes"); } }
        public static string ResultCursesEndured { get { return GetString("ResultCursesEndured"); } }
        public static string ResultWeakenedBosses { get { return GetString("ResultWeakenedBosses"); } }
        public static string ResultScoreBonus { get { return GetString("ResultScoreBonus"); } }
        public static string ResultMasteryBonus { get { return GetString("ResultMasteryBonus"); } }
        public static string ResultSummaryVictory { get { return GetString("ResultSummaryVictory"); } }
        public static string ResultSummaryGameOver { get { return GetString("ResultSummaryGameOver"); } }
        public static string ResultSummaryConcede { get { return GetString("ResultSummaryConcede"); } }
        
        public static string ResultDepthRank(int depth)
        {
            if (depth >= 100) return GetString("ResultDepthRank_100");
            if (depth >= 75) return GetString("ResultDepthRank_75");
            if (depth >= 50) return GetString("ResultDepthRank_50");
            if (depth >= 30) return GetString("ResultDepthRank_30");
            if (depth >= 20) return GetString("ResultDepthRank_20");
            if (depth >= 10) return GetString("ResultDepthRank_10");
            return GetString("ResultDepthRank_Default");
        }
        
        public static string ResultButtonClose { get { return GetString("ResultButtonClose"); } }
        
        // ============================================================
        // LOCALIZED MESSAGE KEY SYSTEM
        // Messages are sent as [IDM:KEY] or [IDM:KEY:param1:param2:...]
        // Each client resolves the key to their local language
        // ============================================================
        
        public const string MSG_PREFIX = "[IDM:";
        public const string MSG_SUFFIX = "]";
        
        public static string CreateMessageKey(string key, params string[] args)
        {
            if (args == null || args.Length == 0)
                return MSG_PREFIX + key + MSG_SUFFIX;
            return MSG_PREFIX + key + ":" + string.Join(":", args) + MSG_SUFFIX;
        }
        
        public static bool IsLocalizedMessage(string message)
        {
            return message != null && message.StartsWith(MSG_PREFIX) && message.EndsWith(MSG_SUFFIX);
        }
        
        public static string ResolveMessageKey(string message)
        {
            if (!IsLocalizedMessage(message)) return message;
            
            DetectLanguage();
            
            string inner = message.Substring(MSG_PREFIX.Length, message.Length - MSG_PREFIX.Length - MSG_SUFFIX.Length);
            string[] parts = inner.Split(new char[] { ':' }, 2);
            string key = parts[0];
            string[] args = parts.Length > 1 ? parts[1].Split(':') : new string[0];
            
            return GetLocalizedMessage(key, args);
        }
        
        private static string GetLocalizedMessage(string key, string[] args)
        {
            switch (key)
            {
                case "WELCOME": return WelcomeMessage;
                case "DEPTH": return string.Format(DepthAnnounce, args.Length > 0 ? args[0] : "?");
                case "BOSS_ROOM": return BossRoomEntry;
                case "MERCHANT_ROOM": return MerchantRoom;
                case "EVENT_ROOM": return EventRoom;
                case "DEAD_END": return DeadEndReached;
                case "SHORTCUT": return ShortcutDiscovered;
                case "ZONE_ENTRY": return args.Length > 0 ? GetZoneEntryMessage(args[0]) : "";
                case "MOD_ANNOUNCE": return args.Length > 0 ? GetModifierMessage(args[0]) : "";
                case "MILESTONE": return args.Length > 0 ? GetDepthMilestone(int.Parse(args[0])) : "";
                case "HUNTER_ENCOUNTER": return HunterEncounter;
                case "HUNTER_INFECTED": return HunterInfected;
                case "PLAYER_INFECTED": return PlayerInfected;
                case "PLAYER_SPREAD": return PlayerSpreadInfection;
                case "PLAYER_CURED": return PlayerInfectionCured;
                case "HUNTER_PATH": return HunterPathWarning;
                case "CURSE": return args.Length > 0 ? GetCurseAnnouncement(args[0]) : "";
                case "CURSE_EXPIRED": return CurseExpired;
                case "BOSS_DEFEATED": return WeakenedBossDefeated;
                case "BOSS_PROMPT": return args.Length > 0 ? GetWeakenedBossPrompt(args[0]) : "";
                case "POT_GREED": return GetString("POT_GREED");
                case "BOSS_SPAWN": return args.Length > 0 ? GetString("BOSS_SPAWN", args[0]) : "";
                case "HUNTER_PURIFIED": return GetString("HUNTER_PURIFIED");
                case "PURE_WHITE_RIFT": return GetString("PURE_WHITE_RIFT");
                default:
                    Debug.LogWarning("[InfiniteDungeon] Unknown message key: " + key);
                    return "[Unknown: " + key + "]";
            }
        }
        
        private static string GetModifierAnnouncement(string modName)
        {
            return GetModifierMessage(modName);
        }
        
        private static string GetMilestoneMessage(int depth)
        {
            return GetDepthMilestone(depth);
        }
        
        private static string GetCurseAnnouncement(string curseTypeName)
        {
            string key = "Curse_" + curseTypeName;
            if (_localizationData.ContainsKey(key))
            {
                return GetString(key);
            }
            return GetString("CURSE_UNKNOWN");
        }
    }
}
