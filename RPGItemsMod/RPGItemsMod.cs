using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HarmonyLib;
using System.Runtime.InteropServices;
using Mirror;
using DuloGames.UI;
using TMPro;

// Note: ModLanguage, Localization, ItemRarity, ItemType, EquipmentSlotType, and RPGItem
// are now defined in separate files: Localization.cs and ItemTypes.cs

/// <summary>
/// Custom attribute to mark the start of a collapsible section
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ConfigSectionAttribute : Attribute
{
    public readonly string title;
    public readonly bool defaultExpanded;
    
    public ConfigSectionAttribute(string title, bool defaultExpanded = false)
    {
        this.title = title;
        this.defaultExpanded = defaultExpanded;
    }
}

/// <summary>
/// Configuration class for the RPG Items Mod.
/// These settings appear in the mod's configuration menu.
/// Uses collapsible sections for better organization.
/// </summary>
[Serializable]
public class RPGItemsConfig : ModConfig
{
    // Track which sections are expanded (persists during session)
    [NonSerialized] private static Dictionary<string, bool> _sectionStates = new Dictionary<string, bool>();
    // ============================================================
    // GENERAL SETTINGS
    // ============================================================
    
    [ConfigSection("▶ GENERAL SETTINGS / 通用设置", false)]
    [HideInInspector] public bool _sectionGeneral;
    
    [ModConfig.LabelText("Language / 语言")]
    [ModConfig.Description("Select mod language. 选择模组语言。")]
    public ModLanguage language = ModLanguage.English;
    
    [ModConfig.LabelText("Inventory Key")]
    [ModConfig.Description("Key to open/close inventory. Examples: I, Tab, F1, BackQuote")]
    public string inventoryKey = "I";
    
    [ModConfig.LabelText("Require Shift")]
    [ModConfig.Description("If enabled, Left Shift must be held with the key.")]
    public bool requireShift = false;
    
    [ModConfig.LabelText("Require Ctrl")]
    [ModConfig.Description("If enabled, Left Ctrl must be held with the key.")]
    public bool requireCtrl = false;
    
    [ModConfig.LabelText("Require Alt")]
    [ModConfig.Description("If enabled, Left Alt must be held with the key.")]
    public bool requireAlt = false;
    
    [ModConfig.LabelText("Sell Key / 出售键")]
    [ModConfig.Description("Key to sell items when near merchant. Hold to confirm, Ctrl+key for instant sell. Default: S. 靠近商人时出售物品的按键。按住确认，Ctrl+按键立即出售。默认：S")]
    public string sellKey = "S";
    
    [ModConfig.LabelText("UI Style / UI样式")]
    [ModConfig.Description("Choose UI layout style. Classic: Equipment left, Inventory right. Diablo-style: Stats left, Equipment right, Inventory bottom. 选择UI布局样式。经典：装备左侧，背包右侧。暗黑风格：属性左侧，装备右侧，背包底部。")]
    public UIStyle uiStyle = UIStyle.Classic;
    
    [ModConfig.LabelText("Block Movement When Inventory Open / 打开背包时禁止移动")]
    [ModConfig.Description("If enabled, character cannot move while inventory is open. Click background to close. 启用时，打开背包后角色无法移动。点击空白处关闭背包。")]
    public bool blockMovementWhenInventoryOpen = false;
    
    [ModConfig.LabelText("UI Scale (%) / UI缩放")]
    [ModConfig.Description("Scale UI elements. 50-200%. Default: 100. Restart inventory (I key) to apply. UI缩放比例，50-200%，默认100。按I键重新打开背包生效。")]
    public float uiScale = 100f;
    
    // ============================================================
    // STAT SYSTEM SETTINGS
    // ============================================================
    
    [ModConfig.LabelText("Stat Points Per Level / 每级属性点")]
    [ModConfig.Description("Stat points gained per hero level. Default: 5. 每升一级获得的属性点数，默认5。")]
    public int pointsPerLevel = 5;
    
    [ModConfig.LabelText("ATK Per Strength / 每点力量攻击")]
    [ModConfig.Description("Attack damage bonus per Strength point. Default: 1. 每点力量增加的攻击力，默认1。")]
    public int atkPerStrength = 1;
    
    [ModConfig.LabelText("HP Per Vitality / 每点体力生命")]
    [ModConfig.Description("Max HP bonus per Vitality point. Default: 8. 每点体力增加的生命值，默认8。")]
    public int hpPerVitality = 8;
    
    [ModConfig.LabelText("DEF Per Defense / 每点防御护甲")]
    [ModConfig.Description("Armor bonus per Defense point. Default: 1. 每点防御增加的护甲值，默认1。")]
    public int defPerDefense = 1;
    
    [ModConfig.LabelText("AP Per Intelligence / 每点智力法强")]
    [ModConfig.Description("Ability Power bonus per Intelligence point. Default: 1. 每点智力增加的法术强度，默认1。")]
    public int apPerIntelligence = 1;
    
    [ModConfig.LabelText("Move Speed Per Agility (%) / 每点敏捷移速")]
    [ModConfig.Description("Move speed % bonus per Agility point. Default: 0.5. 每点敏捷增加的移速%，默认0.5。")]
    public float moveSpeedPerAgility = 0.5f;
    
    [ModConfig.LabelText("Attack Speed Per Agility (%) / 每点敏捷攻速")]
    [ModConfig.Description("Attack speed % bonus per Agility point. Default: 0.5. 每点敏捷增加的攻速%，默认0.5。")]
    public float attackSpeedPerAgility = 0.5f;
    
    [ModConfig.LabelText("Crit Chance Per Luck (%) / 每点幸运暴击")]
    [ModConfig.Description("Crit chance % bonus per Luck point. Default: 0.5. 每点幸运增加的暴击率%，默认0.5。")]
    public float critChancePerLuck = 0.5f;
    
    // ============================================================
    // ITEM DROP SETTINGS
    // ============================================================
    
    [ConfigSection("▶ ITEM DROPS / 物品掉落", false)]
    [HideInInspector] public bool _sectionDrops;
    
    [ModConfig.LabelText("Equipment Drop Rate (%) / 装备掉落率")]
    [ModConfig.Description("Base chance for monsters to drop equipment. Default: 15. 怪物掉落装备的基础几率，默认15。")]
    public float equipmentDropRate = 15f;
    
    [ModConfig.LabelText("Consumable Drop Rate (%) / 消耗品掉落率")]
    [ModConfig.Description("Base chance for monsters to drop consumables. Default: 8. 怪物掉落消耗品的基础几率，默认8。")]
    public float consumableDropRate = 8f;
    
    [ModConfig.LabelText("Boss Drop Multiplier / Boss掉落倍率")]
    [ModConfig.Description("Drop rate multiplier for Boss monsters. Default: 3.0. Boss怪物的掉落率倍率，默认3.0。")]
    public float bossDropMultiplier = 3.0f;
    
    [ModConfig.LabelText("MiniBoss Drop Multiplier / 小Boss掉落倍率")]
    [ModConfig.Description("Drop rate multiplier for MiniBoss monsters. Default: 2.0. 小Boss怪物的掉落率倍率，默认2.0。")]
    public float miniBossDropMultiplier = 2.0f;
    
    // ============================================================
    // RARITY WEIGHTS
    // ============================================================
    
    [ModConfig.LabelText("Common Weight / 普通权重")]
    [ModConfig.Description("Weight for Common rarity drops. Default: 60. 普通稀有度的权重，默认60。")]
    public int commonWeight = 60;
    
    [ModConfig.LabelText("Rare Weight / 稀有权重")]
    [ModConfig.Description("Weight for Rare rarity drops. Default: 30. 稀有稀有度的权重，默认30。")]
    public int rareWeight = 30;
    
    [ModConfig.LabelText("Epic Weight / 史诗权重")]
    [ModConfig.Description("Weight for Epic rarity drops. Default: 8. 史诗稀有度的权重，默认8。")]
    public int epicWeight = 8;
    
    [ModConfig.LabelText("Legendary Weight / 传说权重")]
    [ModConfig.Description("Weight for Legendary rarity drops. Default: 2. 传说稀有度的权重，默认2。")]
    public int legendaryWeight = 2;
    
    [ModConfig.LabelText("Hero-Only Drops / 仅本英雄掉落")]
    [ModConfig.Description("If enabled, only drops items your hero can equip. If disabled, all hero items can drop. 启用时只掉落你英雄能装备的物品，禁用时所有英雄物品都会掉落。")]
    public bool heroOnlyDrops = false;
    
    // ============================================================
    // UPGRADE SYSTEM SETTINGS
    // ============================================================
    
    [ConfigSection("▶ UPGRADE SYSTEM / 强化系统", false)]
    [HideInInspector] public bool _sectionUpgrade;
    
    [ModConfig.LabelText("Upgrade Scaling Per Level (%) / 每级强化倍率")]
    [ModConfig.Description("Stat scaling % per upgrade level. Default: 15 (Diablo IV style). 每级强化增加的属性%，默认15（暗黑4风格）。")]
    public float upgradeScalingPerLevel = 15f;
    
    [ModConfig.LabelText("Sell Value (%) / 出售价值")]
    [ModConfig.Description("% of item value when selling. Default: 40. 出售时获得的价值%，默认40。")]
    public float sellValuePercent = 40f;
    
    // ============================================================
    // WEAPON MASTERY SETTINGS
    // ============================================================
    
    [ModConfig.LabelText("Mastery XP Per Hit / 每次攻击精通经验")]
    [ModConfig.Description("Base XP gained per weapon hit. Default: 1. 每次武器攻击获得的基础经验，默认1。")]
    public int masteryXpPerHit = 1;
    
    [ModConfig.LabelText("Mastery Crit XP Multiplier / 暴击精通倍率")]
    [ModConfig.Description("XP multiplier for critical hits. Default: 1.5. 暴击时的经验倍率，默认1.5。")]
    public float masteryCritMultiplier = 1.5f;
    
    [ModConfig.LabelText("Mastery Damage Per Level (%) / 每级精通伤害")]
    [ModConfig.Description("Damage % bonus per mastery level. Default: 2. 每级精通增加的伤害%，默认2。")]
    public float masteryDamagePerLevel = 2f;
    
    [ModConfig.LabelText("Mastery Crit Per Level (%) / 每级精通暴击")]
    [ModConfig.Description("Crit chance % bonus per mastery level. Default: 0.5. 每级精通增加的暴击率%，默认0.5。")]
    public float masteryCritPerLevel = 0.5f;
    
    [ModConfig.LabelText("Summons Grant Mastery XP / 召唤物获得精通经验")]
    [ModConfig.Description("If enabled, summon attacks (Nachia, etc.) grant weapon mastery XP. Default: true. 启用时，召唤物攻击也能获得武器精通经验（娜琪雅等）。")]
    public bool summonsGrantMasteryXP = true;
    
    [ModConfig.LabelText("Summon Mastery XP Multiplier / 召唤物精通经验倍率")]
    [ModConfig.Description("XP multiplier for summon attacks. Default: 0.5 (50%). 召唤物攻击的经验倍率，默认0.5（50%）。")]
    public float summonMasteryXpMultiplier = 0.5f;
    
    // ============================================================
    // GOLD/DUST ON KILL SETTINGS
    // ============================================================
    
    [ConfigSection("▶ GOLD/DUST ON KILL / 击杀金币尘埃", false)]
    [HideInInspector] public bool _sectionGoldDust;
    
    [ModConfig.LabelText("Gold On Kill Min (Normal) / 击杀金币最小值")]
    [ModConfig.Description("Min gold from Normal monster kills with Midas. Default: 1. 米达斯击杀普通怪物最小金币，默认1。")]
    public int goldOnKillMinNormal = 1;
    
    [ModConfig.LabelText("Gold On Kill Max (Normal) / 击杀金币最大值")]
    [ModConfig.Description("Max gold from Normal monster kills with Midas. Default: 5. 米达斯击杀普通怪物最大金币，默认5。")]
    public int goldOnKillMaxNormal = 5;
    
    [ModConfig.LabelText("Dust On Kill Min / 击杀尘埃最小值")]
    [ModConfig.Description("Min dust from kills with dust items. Default: 5. 尘埃装备击杀最小尘埃，默认5。")]
    public int dustOnKillMin = 5;
    
    [ModConfig.LabelText("Dust On Kill Max / 击杀尘埃最大值")]
    [ModConfig.Description("Max dust from kills with dust items. Default: 30. 尘埃装备击杀最大尘埃，默认30。")]
    public int dustOnKillMax = 30;
    
    // ============================================================
    // SET BONUS SETTINGS
    // ============================================================
    
    [ConfigSection("▶ SET BONUSES / 套装效果", false)]
    [HideInInspector] public bool _sectionSetBonus;
    
    [ModConfig.LabelText("Enable Set Bonuses / 启用套装效果")]
    [ModConfig.Description("Enable set bonus system for matching rarity items. Default: true. 启用同稀有度装备的套装效果，默认开启。")]
    public bool enableSetBonuses = true;
    
    [ModConfig.LabelText("Common Set Proc Cooldown (s) / 普通套装触发冷却")]
    [ModConfig.Description("Cooldown in seconds for Common 6-piece set proc (Cheat Death). Default: 60. 普通6件套触发效果冷却时间（秒），默认60。")]
    public float setBonusProcCooldownCommon = 60f;
    
    [ModConfig.LabelText("Rare Set Proc Cooldown (s) / 稀有套装触发冷却")]
    [ModConfig.Description("Cooldown in seconds for Rare 6-piece set proc (Wind Rush). Default: 15. 稀有6件套触发效果冷却时间（秒），默认15。")]
    public float setBonusProcCooldownRare = 15f;
    
    [ModConfig.LabelText("Epic Set Proc Cooldown (s) / 史诗套装触发冷却")]
    [ModConfig.Description("Cooldown in seconds for Epic 6-piece set proc (Nightmare Burst). Default: 40. 史诗6件套触发效果冷却时间（秒），默认40。")]
    public float setBonusProcCooldownEpic = 40f;
    
    [ModConfig.LabelText("Legendary Set Proc Cooldown (s) / 传说套装触发冷却")]
    [ModConfig.Description("Cooldown in seconds for Legendary 6-piece set proc (Divine Wrath). Default: 40. 传说6件套触发效果冷却时间（秒），默认40。")]
    public float setBonusProcCooldownLegendary = 40f;
    
    // ============================================================
    // DEBUG SETTINGS
    // ============================================================
    
    [ConfigSection("▶ DEBUG SETTINGS / 调试设置", false)]
    [HideInInspector] public bool _sectionDebug;
    
    [ModConfig.LabelText("Debug Mode / 调试模式")]
    [ModConfig.Description("Enable verbose logging for debugging. Disable for better performance. 启用详细日志用于调试，禁用以提高性能。")]
    public bool debugMode = false;
    
    /// <summary>
    /// Parse the inventory key string to KeyCode
    /// </summary>
    public KeyCode GetInventoryKeyCode()
    {
        if (string.IsNullOrEmpty(inventoryKey))
            return KeyCode.I;
        
        // Try to parse the key string
        try
        {
            KeyCode result = (KeyCode)System.Enum.Parse(typeof(KeyCode), inventoryKey, true);
            return result;
        }
        catch
        {
            // Common key name mappings
            string key = inventoryKey.ToLower().Trim();
            switch (key)
            {
                case "`":
                case "~":
                case "backquote":
                case "tilde":
                    return KeyCode.BackQuote;
                case "tab":
                    return KeyCode.Tab;
                case "1": return KeyCode.Alpha1;
                case "2": return KeyCode.Alpha2;
                case "3": return KeyCode.Alpha3;
                case "4": return KeyCode.Alpha4;
                case "5": return KeyCode.Alpha5;
                case "6": return KeyCode.Alpha6;
                case "7": return KeyCode.Alpha7;
                case "8": return KeyCode.Alpha8;
                case "9": return KeyCode.Alpha9;
                case "0": return KeyCode.Alpha0;
                default:
                    return KeyCode.I; // Default fallback
            }
        }
    }
    
    /// <summary>
    /// Custom widget builder that creates collapsible sections
    /// </summary>
    public override void BuildWidgets(Transform parent, out SafeAction onChanged, out SafeAction requestUpdate)
    {
        SafeAction _onChanged = new SafeAction();
        onChanged = _onChanged;
        SafeAction _requestUpdate = new SafeAction();
        requestUpdate = _requestUpdate;
        
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        // Track current section content container (null = not in a section or section is header-only)
        GameObject currentSectionContent = null;
        string currentSectionName = null;
        
        foreach (FieldInfo f in fields)
        {
            // Skip private fields without SerializeField and fields with HideInInspector
            if ((!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null) || f.GetCustomAttribute<HideInInspector>() != null)
            {
                // Check if this is a section marker (has ConfigSection attribute)
                ConfigSectionAttribute sectionAttr = f.GetCustomAttribute<ConfigSectionAttribute>();
                if (sectionAttr != null)
                {
                    // Create collapsible section header
                    currentSectionName = sectionAttr.title;
                    bool isExpanded = _sectionStates.ContainsKey(currentSectionName) ? _sectionStates[currentSectionName] : sectionAttr.defaultExpanded;
                    
                    // Create header button
                    Button headerButton = UnityEngine.Object.Instantiate(DewGUI.widgetButton, parent);
                    // Get UI scale once for this section (with screen factor for proper scaling)
                    float configUIScale = RPGItemsMod.GetUIScaleWithScreenFactor();
                    
                    TextMeshProUGUI headerText = headerButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (headerText != null)
                    {
                        float headerFontSize = Mathf.Max(18f * configUIScale, 14f);
                        headerText.text = (isExpanded ? "▼ " : "▶ ") + sectionAttr.title.Substring(2); // Remove the ▶ prefix from title
                        headerText.fontSize = headerFontSize;
                        headerText.fontStyle = FontStyles.Bold;
                        headerText.alignment = TextAlignmentOptions.Left;
                    }
                    
                    // Style the button (scaled height)
                    RectTransform buttonRect = headerButton.GetComponent<RectTransform>();
                    if (buttonRect != null)
                    {
                        float buttonHeight = Mathf.Max(40f * configUIScale, 32f);
                        buttonRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x, buttonHeight);
                    }
                    
                    // Create content container for this section
                    GameObject sectionGO = new GameObject(currentSectionName + "_Content");
                    sectionGO.transform.SetParent(parent, false);
                    VerticalLayoutGroup vlg = sectionGO.AddComponent<VerticalLayoutGroup>();
                    vlg.childControlHeight = true;
                    vlg.childControlWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.childForceExpandWidth = true;
                    // Scale spacing and padding
                    vlg.spacing = 5f * configUIScale;
                    int paddingLeft = Mathf.RoundToInt(20f * configUIScale);
                    int paddingTop = Mathf.RoundToInt(5f * configUIScale);
                    int paddingBottom = Mathf.RoundToInt(10f * configUIScale);
                    vlg.padding = new RectOffset(paddingLeft, 0, paddingTop, paddingBottom); // Indent content
                    ContentSizeFitter csf = sectionGO.AddComponent<ContentSizeFitter>();
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    
                    // Set initial visibility
                    sectionGO.SetActive(isExpanded);
                    currentSectionContent = sectionGO;
                    
                    // Setup toggle behavior
                    string sectionKey = currentSectionName;
                    GameObject contentGO = sectionGO;
                    TextMeshProUGUI btnText = headerText;
                    string baseTitle = sectionAttr.title.Substring(2);
                    
                    headerButton.onClick.AddListener(() => {
                        bool nowExpanded = !contentGO.activeSelf;
                        contentGO.SetActive(nowExpanded);
                        _sectionStates[sectionKey] = nowExpanded;
                        if (btnText != null)
                        {
                            btnText.text = (nowExpanded ? "▼ " : "▶ ") + baseTitle;
                        }
                    });
                }
                continue;
            }
            
            // Determine which parent to use (section content or main parent)
            Transform widgetParent = currentSectionContent != null ? currentSectionContent.transform : parent;
            
            // Create the widget for this field using game's default widget builders
            try
            {
                // Create horizontal group for label + widget
                HorizontalLayoutGroup group = DewGUI.CreateHorizontalLayoutGroup(widgetParent);
                
                // Get label text
                ModConfig.LabelTextAttribute labelAttr = f.GetCustomAttribute<ModConfig.LabelTextAttribute>();
                string labelText = labelAttr != null ? labelAttr.text : f.Name.NicifyVariableName();
                
                // Create label
                TextMeshProUGUI label = UnityEngine.Object.Instantiate(DewGUI.widgetTextLabel, group.transform);
                label.SetExpandWidth(true);
                label.SetText(labelText);
                
                // Create widget based on field type
                object state = this;
                FieldInfo field = f;
                
                if (f.FieldType == typeof(bool))
                {
                    UI_Toggle toggle = UnityEngine.Object.Instantiate(DewGUI.widgetToggleCheck, group.transform);
                    toggle.isChecked = (bool)f.GetValue(this);
                    toggle.onIsCheckedChanged.AddListener((newVal) => {
                        field.SetValue(state, newVal);
                        _onChanged.Invoke();
                    });
                    _requestUpdate.Add(() => toggle.isChecked = (bool)field.GetValue(state));
                }
                else if (f.FieldType == typeof(int))
                {
                    TMP_InputField inputField = UnityEngine.Object.Instantiate(DewGUI.widgetInputField, group.transform);
                    inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                    inputField.text = f.GetValue(this).ToString();
                    inputField.onEndEdit.AddListener((newVal) => {
                        int result;
                        if (int.TryParse(newVal, out result))
                        {
                            field.SetValue(state, result);
                            _onChanged.Invoke();
                        }
                    });
                    _requestUpdate.Add(() => inputField.text = field.GetValue(state).ToString());
                }
                else if (f.FieldType == typeof(float))
                {
                    TMP_InputField inputField = UnityEngine.Object.Instantiate(DewGUI.widgetInputField, group.transform);
                    inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                    inputField.text = ((float)f.GetValue(this)).ToString("F2");
                    inputField.onEndEdit.AddListener((newVal) => {
                        float result;
                        if (float.TryParse(newVal, out result))
                        {
                            field.SetValue(state, result);
                            _onChanged.Invoke();
                        }
                    });
                    _requestUpdate.Add(() => inputField.text = ((float)field.GetValue(state)).ToString("F2"));
                }
                else if (f.FieldType == typeof(string))
                {
                    TMP_InputField inputField = UnityEngine.Object.Instantiate(DewGUI.widgetInputField, group.transform);
                    inputField.text = (string)f.GetValue(this) ?? "";
                    inputField.onEndEdit.AddListener((newVal) => {
                        field.SetValue(state, newVal);
                        _onChanged.Invoke();
                    });
                    _requestUpdate.Add(() => inputField.text = (string)field.GetValue(state) ?? "");
                }
                else if (f.FieldType.IsEnum)
                {
                    TMP_Dropdown dropdown = UnityEngine.Object.Instantiate(DewGUI.widgetDropdown, group.transform);
                    string[] names = Enum.GetNames(f.FieldType);
                    object[] values = Enum.GetValues(f.FieldType).Cast<object>().ToArray();
                    dropdown.ClearOptions();
                    dropdown.AddOptions(names.ToList());
                    object currentVal = f.GetValue(this);
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i].Equals(currentVal))
                        {
                            dropdown.value = i;
                            break;
                        }
                    }
                    dropdown.onValueChanged.AddListener((idx) => {
                        field.SetValue(state, values[idx]);
                        _onChanged.Invoke();
                    });
                    _requestUpdate.Add(() => {
                        object val = field.GetValue(state);
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (values[i].Equals(val))
                            {
                                dropdown.value = i;
                                break;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("[RPGItemsMod] Failed to create widget for field " + f.Name + ": " + ex.Message);
            }
        }
    }
}

/// <summary>
/// Centralized logging utility - respects debug mode setting
/// </summary>
public static class RPGLog
{
    private const string PREFIX = "[RPGItemsMod] ";
    
    /// <summary>
    /// Log verbose debug messages (only when debug mode is enabled)
    /// </summary>
    public static void Debug(string message)
    {
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        if (config != null && config.debugMode)
        {
            UnityEngine.Debug.Log(PREFIX + message);
        }
    }
    
    /// <summary>
    /// Log important info messages (always shown)
    /// </summary>
    public static void Info(string message)
    {
        UnityEngine.Debug.Log(PREFIX + message);
    }
    
    /// <summary>
    /// Log warnings (always shown)
    /// </summary>
    public static void Warning(string message)
    {
        UnityEngine.Debug.LogWarning(PREFIX + message);
    }
    
    /// <summary>
    /// Log errors (always shown)
    /// </summary>
    public static void Error(string message)
    {
        UnityEngine.Debug.LogError(PREFIX + message);
    }
}

/// <summary>
/// RPG Items Mod - Adds an inventory system with custom items and UGUI interface
/// Features:
/// - Custom item system with types, rarities, and stats
/// - Inventory grid UI using UGUI
/// - Custom item images support
/// - Drag and drop items
/// - Item tooltips
/// </summary>
public class RPGItemsMod : ModBehaviour
{
    // Static instance for config access
    public static RPGItemsMod Instance { get; private set; }
    
    // Public config field - shown in mod settings menu
    public RPGItemsConfig config = new RPGItemsConfig();
    
    /// <summary>
    /// Get the mod config (safe access with null check)
    /// </summary>
    public static RPGItemsConfig GetConfig()
    {
        return Instance != null ? Instance.config : null;
    }
    
    /// <summary>
    /// Get the UI scale factor combining game settings and mod config.
    /// Respects game's UI scale setting from options menu.
    /// Note: When used with CanvasScaler.ScaleWithScreenSize, resolution scaling is already handled,
    /// so we only apply gameUIScale and modScale (not screenHeightFactor).
    /// </summary>
    public static float GetUIScale()
    {
        var config = GetConfig();
        float modScale = 1f;
        if (config != null)
        {
            // Clamp between 50% and 200%
            modScale = Mathf.Clamp(config.uiScale, 50f, 200f) / 100f;
        }
        
        // Get game's UI scale setting (default 1.0)
        float gameUIScale = 1f;
        try
        {
            if (DewSave.profileMain != null && DewSave.profileMain.gameplay != null)
            {
                gameUIScale = DewSave.profileMain.gameplay.uiScale;
            }
        }
        catch { }
        
        return gameUIScale * modScale;
    }
    
    /// <summary>
    /// Get UI scale for ConstantPixelSize canvases (like the game's DewCanvas uses).
    /// Includes screen height factor for proper high-DPI/resolution support.
    /// This version matches InfiniteDungeonMod's approach for consistency.
    /// </summary>
    public static float GetUIScaleWithScreenFactor()
    {
        // Get game's UI scale setting (default 1.0)
        float gameUIScale = 1f;
        try
        {
            if (DewSave.profileMain != null && DewSave.profileMain.gameplay != null)
            {
                gameUIScale = DewSave.profileMain.gameplay.uiScale;
            }
        }
        catch { }
        
        // Apply screen height factor like the game does (base reference: 1440p)
        // Clamp to reasonable range to avoid extreme scaling
        float screenHeightFactor = Mathf.Clamp((float)Screen.height / 1440f, 0.5f, 2f);
        
        return screenHeightFactor * gameUIScale;
    }
    
    /// <summary>
    /// Get just the game's UI scale setting (without screen height factor)
    /// </summary>
    public static float GetGameUIScale()
    {
        try
        {
            if (DewSave.profileMain != null && DewSave.profileMain.gameplay != null)
            {
                return DewSave.profileMain.gameplay.uiScale;
            }
        }
        catch { }
        return 1f;
    }
    
    /// <summary>
    /// Get UI scale for ConstantPixelSize canvases (like the game's DewCanvas uses).
    /// Includes screen height factor for proper high-DPI/resolution support.
    /// </summary>
    public static float GetUIScaleForConstantPixelSize()
    {
        float baseScale = GetUIScale();
        
        // Apply screen height factor like the game does (base reference: 1440p)
        float screenHeightFactor = Mathf.Clamp((float)Screen.height / 1440f, 0.5f, 2f);
        
        return screenHeightFactor * baseScale;
    }
    
    /// <summary>
    /// Get the raw mod UI scale factor without game settings (0.5 to 2.0 based on config)
    /// Used for elements that should only respect mod config, not game settings
    /// </summary>
    public static float GetModUIScaleOnly()
    {
        var config = GetConfig();
        if (config == null) return 1f;
        
        // Clamp between 50% and 200%
        float scale = Mathf.Clamp(config.uiScale, 50f, 200f) / 100f;
        return scale;
    }
    
    private InventoryManager inventoryManager;
    private EquipmentManager equipmentManager;
    
    // Made public static for Harmony patch access (UIWindowManagerPatch)
    public static InventoryUI inventoryUI;
    public static SettingsPanel settingsPanel;
    private ConsumableBar consumableBar;
    private AutoTargetSystem autoTargetSystem;
    private StatsSystem statsSystem;
    public static StatsUI statsUI;
    private WeaponMasterySystem masterySystem;
    public static WeaponMasteryUI masteryUI;
    private RPGResultsUI resultsUI;
    public static bool isInventoryOpen = false;
    public static bool isStatsOpen = false;
    public static bool isMasteryOpen = false;
    
    /// <summary>
    /// BackHandler for ESC key - registered with GlobalUIManager
    /// </summary>
    private BackHandler _escBackHandler = null;
    
    private bool isInGame = false;
    private string currentScene = "";
    
    // Persistence check state
    private bool _persistenceCheckPending = false;
    private bool _persistenceCheckDone = false;
    
    // Auto-save timer
    private float _autoSaveTimer = 0f;
    private const float AUTO_SAVE_INTERVAL = 30f;
    
    private static Harmony _harmony;

    private void Awake()
    {
        Instance = this;
        
        // Mark as gameplay-altering mod (required for server-side mods)
        instance.isAlteringGameplay = true;
        
        RPGLog.Info("RPG Items Mod v2.0 loaded!");
        
        // Initialize localization system - set mod path first, then detect language
        if (mod != null && !string.IsNullOrEmpty(mod.path))
        {
            Localization.SetModPath(mod.path);
            RPGLog.Debug("Localization mod path set to: " + mod.path);
        }
        Localization.DetectLanguage();
        
        // Refresh all item database entries with localized text
        ItemDatabase.RefreshAllLocalizedFields();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Apply Harmony patch to intercept chat messages
        ApplyHarmonyPatches();
        
        // Register for ActorManager lifecycle - this ensures we re-subscribe when a new game starts
        // The game creates a NEW ActorManager instance for each game session
        CallOnNetworkedManager<ActorManager>(
            OnActorManagerStartClient,
            OnActorManagerStopClient
        );
        
        RPGLog.Debug("Registered for ActorManager lifecycle callbacks");
    }
    
    /// <summary>
    /// Called when ActorManager starts (new game session begins)
    /// </summary>
    private void OnActorManagerStartClient()
    {
        RPGLog.Debug("ActorManager started - initializing systems");
        
        // Subscribe to monster deaths (server only)
        if (NetworkServer.active)
        {
            MonsterLootSystem.SubscribeToActorManager();
        }
        
        // Register RPC handlers on the NEW serverActor
        // This is critical - each game session has a NEW ActorManager with a NEW serverActor
        NetworkedItemSystem.OnActorManagerStarted();
    }
    
    /// <summary>
    /// Called when ActorManager stops (game session ends)
    /// </summary>
    private void OnActorManagerStopClient()
    {
        RPGLog.Debug("ActorManager stopped - cleaning up systems");
        MonsterLootSystem.UnsubscribeFromActorManager();
        NetworkedItemSystem.OnActorManagerStopped();
    }
    
    private void ApplyHarmonyPatches()
    {
        try
        {
            _harmony = new Harmony("com.chino.rpgitemsmod.chatpatch");
            
            // Patch 1: ChatManager to intercept mod requests
            MethodInfo original = typeof(ChatManager).GetMethod(
                "UserCode_CmdSendChatMessage__String__NetworkConnectionToClient",
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (original != null)
            {
                MethodInfo prefix = typeof(ChatManagerPatch).GetMethod(
                    "Prefix_CmdSendChatMessage",
                    BindingFlags.Static | BindingFlags.Public);
                
                if (prefix != null)
                {
                    _harmony.Patch(original, new HarmonyMethod(prefix));
                    RPGLog.Debug("Harmony patch applied to ChatManager");
                }
            }
            else
            {
                RPGLog.Warning(" Could not find ChatManager method to patch");
            }
            
            // Patch 2: Chat tooltip to show RPG item info
            MethodInfo tooltipOriginal = typeof(UI_Common_ChatBox_Item).GetMethod(
                "ShowTooltip",
                BindingFlags.Instance | BindingFlags.Public);
            
            if (tooltipOriginal != null)
            {
                MethodInfo tooltipPrefix = typeof(ChatTooltipPatch).GetMethod(
                    "Prefix_ShowTooltip",
                    BindingFlags.Static | BindingFlags.Public);
                
                if (tooltipPrefix != null)
                {
                    _harmony.Patch(tooltipOriginal, new HarmonyMethod(tooltipPrefix));
                    RPGLog.Debug("Harmony patch applied to ChatBox tooltip");
                }
            }
            
            // Patch 3: Merchant patches for custom items
            try
            {
                // Patch PopulatePlayerMerchandises on base class (public method, more reliable)
                MethodInfo populatePlayerMethod = typeof(PropEnt_Merchant_Base).GetMethod(
                    "PopulatePlayerMerchandises",
                    BindingFlags.Instance | BindingFlags.Public);
                if (populatePlayerMethod != null)
                {
                    Type[] allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
                    Type patchType = null;
                    foreach (Type t in allTypes)
                    {
                        if (t.Name == "PopulatePlayerMerchandisesPatch" && t.IsNested)
                        {
                            patchType = t;
                            break;
                        }
                    }
                    if (patchType != null)
                    {
                        MethodInfo postfix = patchType.GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public);
                        if (postfix != null)
                        {
                            _harmony.Patch(populatePlayerMethod, null, new HarmonyMethod(postfix));
                            RPGLog.Debug("Harmony patch applied to PopulatePlayerMerchandises");
                        }
                    }
                }
                
                // Also patch OnPopulateMerchandises as backup
                MethodInfo populateMethod = typeof(PropEnt_Merchant_Jonas).GetMethod(
                    "OnPopulateMerchandises",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (populateMethod != null)
                {
                    // Find nested class by searching all types in current assembly
                    Type[] allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
                    Type patchType = null;
                    foreach (Type t in allTypes)
                    {
                        if (t.Name == "OnPopulateMerchandisesPatch" && t.IsNested)
                        {
                            patchType = t;
                            break;
                        }
                    }
                    if (patchType != null)
                    {
                        MethodInfo populatePostfix = patchType.GetMethod(
                            "Postfix",
                            BindingFlags.Static | BindingFlags.Public);
                        if (populatePostfix != null)
                        {
                            _harmony.Patch(populateMethod, null, new HarmonyMethod(populatePostfix));
                            RPGLog.Debug("Harmony patch applied to OnPopulateMerchandises");
                        }
                        else
                        {
                            RPGLog.Warning(" Could not find Postfix method in OnPopulateMerchandisesPatch");
                        }
                    }
                    else
                    {
                        RPGLog.Warning(" Could not find OnPopulateMerchandisesPatch class");
                    }
                }
                else
                {
                    RPGLog.Warning(" Could not find OnPopulateMerchandises method");
                }
                
                // Patch UpdateItemPrices
                MethodInfo updatePricesMethod = typeof(PropEnt_Merchant_Jonas).GetMethod(
                    "UpdateItemPrices",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (updatePricesMethod != null)
                {
                    // Find nested class by searching all types in current assembly
                    Type[] allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
                    Type patchType = null;
                    foreach (Type t in allTypes)
                    {
                        if (t.Name == "UpdateItemPricesPatch" && t.IsNested)
                        {
                            patchType = t;
                            break;
                        }
                    }
                    if (patchType != null)
                    {
                        MethodInfo updatePricesPostfix = patchType.GetMethod(
                            "Postfix",
                            BindingFlags.Static | BindingFlags.Public);
                        if (updatePricesPostfix != null)
                        {
                            _harmony.Patch(updatePricesMethod, null, new HarmonyMethod(updatePricesPostfix));
                            RPGLog.Debug("Harmony patch applied to UpdateItemPrices");
                        }
                    }
                }
                
                // Patch SpawnMerchandise
                MethodInfo spawnMethod = typeof(PropEnt_Merchant_Base).GetMethod(
                    "SpawnMerchandise",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (spawnMethod != null)
                {
                    MethodInfo spawnPrefix = typeof(MerchantPatches.SpawnMerchandisePatch).GetMethod(
                        "Prefix",
                        BindingFlags.Static | BindingFlags.Public);
                    if (spawnPrefix != null)
                    {
                        _harmony.Patch(spawnMethod, new HarmonyMethod(spawnPrefix));
                        RPGLog.Debug("Harmony patch applied to SpawnMerchandise");
                    }
                }
                
                // Patch CmdPurchase to intercept before game tries to resolve custom items
                MethodInfo cmdPurchaseMethod = typeof(PropEnt_Merchant_Base).GetMethod(
                    "UserCode_CmdPurchase__Int32__NetworkConnectionToClient",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (cmdPurchaseMethod != null)
                {
                    MethodInfo cmdPurchasePrefix = typeof(MerchantPatches.CmdPurchasePatch).GetMethod(
                        "Prefix",
                        BindingFlags.Static | BindingFlags.Public);
                    if (cmdPurchasePrefix != null)
                    {
                        _harmony.Patch(cmdPurchaseMethod, new HarmonyMethod(cmdPurchasePrefix));
                        RPGLog.Debug("Harmony patch applied to CmdPurchase");
                    }
                    else
                    {
                        RPGLog.Warning(" Could not find CmdPurchasePatch.Prefix method");
                    }
                }
                else
                {
                    RPGLog.Warning(" Could not find UserCode_CmdPurchase method");
                }
                
                // Patch merchant UI UpdateContent (use Prefix to intercept before DewResources errors)
                MethodInfo updateContentMethod = typeof(UI_InGame_FloatingWindow_Shop_Item).GetMethod(
                    "UpdateContent",
                    BindingFlags.Instance | BindingFlags.Public);
                if (updateContentMethod != null)
                {
                    Type[] allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
                    Type patchType = null;
                    foreach (Type t in allTypes)
                    {
                        if (t.Name == "ShopItemUpdateContentPatch" && t.IsNested)
                        {
                            patchType = t;
                            break;
                        }
                    }
                    if (patchType != null)
                    {
                        // Use Prefix instead of Postfix to intercept before game tries to load resources
                        MethodInfo updateContentPrefix = patchType.GetMethod(
                            "Prefix",
                            BindingFlags.Static | BindingFlags.Public);
                        if (updateContentPrefix != null)
                        {
                            _harmony.Patch(updateContentMethod, new HarmonyMethod(updateContentPrefix), null);
                            RPGLog.Debug("Harmony patch applied to ShopItem UpdateContent");
                        }
                    }
                }
                
                // Patch merchant UI ShowTooltip
                MethodInfo showTooltipMethod = typeof(UI_InGame_FloatingWindow_Shop_Item).GetMethod(
                    "ShowTooltip",
                    BindingFlags.Instance | BindingFlags.Public);
                if (showTooltipMethod != null)
                {
                    // Find nested class by searching all types in current assembly
                    Type[] allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
                    Type patchType2 = null;
                    foreach (Type t in allTypes)
                    {
                        if (t.Name == "ShopItemShowTooltipPatch" && t.IsNested)
                        {
                            patchType2 = t;
                            break;
                        }
                    }
                    if (patchType2 != null)
                    {
                        MethodInfo showTooltipPrefix = patchType2.GetMethod(
                            "Prefix",
                            BindingFlags.Static | BindingFlags.Public);
                        if (showTooltipPrefix != null)
                        {
                            _harmony.Patch(showTooltipMethod, new HarmonyMethod(showTooltipPrefix));
                            RPGLog.Debug("Harmony patch applied to ShopItem ShowTooltip");
                        }
                    }
                }
                
                // Patch Shop UI UpdateMerchandise to force 6 columns for proper row separation
                MethodInfo updateMerchandiseMethod = typeof(UI_InGame_FloatingWindow_Shop).GetMethod(
                    "UpdateMerchandise",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (updateMerchandiseMethod != null)
                {
                    MethodInfo updateMerchandisePostfix = typeof(ShopUIPatches.UpdateMerchandisePatch).GetMethod(
                        "Postfix",
                        BindingFlags.Static | BindingFlags.Public);
                    if (updateMerchandisePostfix != null)
                    {
                        _harmony.Patch(updateMerchandiseMethod, null, new HarmonyMethod(updateMerchandisePostfix));
                        RPGLog.Debug("Harmony patch applied to Shop UpdateMerchandise (grid layout)");
                    }
                }
                
                // Patch Smoothie Merchant to add consumables
                MethodInfo smoothiePopulateMethod = typeof(PropEnt_Merchant_Smoothie).GetMethod(
                    "OnPopulateMerchandises",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (smoothiePopulateMethod != null)
                {
                    MethodInfo smoothiePostfix = typeof(SmoothieMerchantPatches.SmoothiePopulatePatch).GetMethod(
                        "Postfix",
                        BindingFlags.Static | BindingFlags.Public);
                    if (smoothiePostfix != null)
                    {
                        _harmony.Patch(smoothiePopulateMethod, null, new HarmonyMethod(smoothiePostfix));
                        RPGLog.Debug("Harmony patch applied to Smoothie Merchant (consumables)");
                    }
                }
                
                // NOTE: Elemental weapon attacks now use EntityEvent_OnAttackFiredBeforePrepare
                // instead of patching DoBasicAttackHit. This is handled in ElementalAttackPatches.SubscribeToHeroAttacks()
                // which is called from EquipmentManager.UpdateElementalEffects() when equipment changes.
                
                // Patch PingManager.Ping.IsValid to fix Smoothie merchant pings
                // The game's original code only checks for PropEnt_Merchant_Jonas, not PropEnt_Merchant_Base
                Type pingStructType = typeof(PingManager).GetNestedType("Ping", BindingFlags.Public);
                if (pingStructType != null)
                {
                    MethodInfo isValidMethod = pingStructType.GetMethod(
                        "IsValid",
                        BindingFlags.Instance | BindingFlags.Public);
                    if (isValidMethod != null)
                    {
                        MethodInfo isValidPostfix = typeof(PingIsValidPatch).GetMethod(
                            "Postfix",
                            BindingFlags.Static | BindingFlags.Public);
                        if (isValidPostfix != null)
                        {
                            _harmony.Patch(isValidMethod, null, new HarmonyMethod(isValidPostfix));
                            RPGLog.Debug("Harmony patch applied to PingManager.Ping.IsValid (Smoothie fix)");
                        }
                    }
                }
                
                // Patch PingManager.ShowPingChatMessage to handle custom RPG item pings in shop
                MethodInfo showPingChatMethod = typeof(PingManager).GetMethod(
                    "ShowPingChatMessage",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (showPingChatMethod != null)
                {
                    MethodInfo pingChatPrefix = typeof(PingManagerChatPatch).GetMethod(
                        "Prefix",
                        BindingFlags.Static | BindingFlags.Public);
                    if (pingChatPrefix != null)
                    {
                        _harmony.Patch(showPingChatMethod, new HarmonyMethod(pingChatPrefix));
                        RPGLog.Debug("Harmony patch applied to PingManager.ShowPingChatMessage");
                    }
                }
                else
                {
                    RPGLog.Warning(" Could not find PingManager.ShowPingChatMessage method");
                }
                
                // Patch Hidden Stash to also drop RPG items
                Type hiddenStashType = Type.GetType("Shrine_HiddenStash, Assembly-CSharp");
                if (hiddenStashType == null)
                {
                    // Try to find it in loaded assemblies
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        hiddenStashType = asm.GetType("Shrine_HiddenStash");
                        if (hiddenStashType != null) break;
                    }
                }
                
                if (hiddenStashType != null)
                {
                    MethodInfo hiddenStashOnUse = hiddenStashType.GetMethod(
                        "OnUse",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (hiddenStashOnUse != null)
                    {
                        MethodInfo stashPostfix = typeof(HiddenStashPatches.OnUsePatch).GetMethod(
                            "Postfix",
                            BindingFlags.Static | BindingFlags.Public);
                        if (stashPostfix != null)
                        {
                            _harmony.Patch(hiddenStashOnUse, null, new HarmonyMethod(stashPostfix));
                            RPGLog.Debug("Harmony patch applied to Shrine_HiddenStash (bonus item drops)");
                        }
                    }
                }
                else
                {
                    RPGLog.Warning(" Could not find Shrine_HiddenStash type");
                }
                
                // Patch ControlManager.SendPingPC to intercept dropped item sharing
                MethodInfo sendPingPCMethod = typeof(ControlManager).GetMethod(
                    "SendPingPC",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (sendPingPCMethod != null)
                {
                    MethodInfo pingPrefix = typeof(SendPingPCPatch).GetMethod(
                        "Prefix",
                        BindingFlags.Static | BindingFlags.Public);
                    if (pingPrefix != null)
                    {
                        _harmony.Patch(sendPingPCMethod, new HarmonyMethod(pingPrefix));
                        RPGLog.Debug("Harmony patch applied to ControlManager.SendPingPC (dropped item sharing)");
                    }
                }
                else
                {
                    RPGLog.Warning(" Could not find ControlManager.SendPingPC method");
                }
                
                // Patch scoring system for mastery bonus
                ApplyScoringPatches();
                
                // Patch mod config window to add scroll view
                ApplyConfigWindowPatch();
                
            }
            catch (Exception merchantEx)
            {
                RPGLog.Warning(" Failed to apply merchant patches: " + merchantEx.Message);
            }
            
            // Patch ControlManager.CastAbilityAtCursor for auto-aim
            try
            {
                AutoAimPatches.ApplyPatches(_harmony);
            }
            catch (Exception autoAimEx)
            {
                RPGLog.Warning(" Failed to apply auto-aim patches: " + autoAimEx.Message);
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to apply Harmony patches: " + e.Message);
        }
    }
    
    /// <summary>
    /// Patch the mod config window to add scrolling support for many settings
    /// </summary>
    private void ApplyConfigWindowPatch()
    {
        try
        {
            MethodInfo openConfigMethod = typeof(DewMod).GetMethod(
                "OpenConfigWindow",
                BindingFlags.Static | BindingFlags.Public);
            
            if (openConfigMethod != null)
            {
                MethodInfo configPrefix = typeof(ConfigWindowScrollPatch).GetMethod(
                    "Prefix",
                    BindingFlags.Static | BindingFlags.Public);
                
                if (configPrefix != null)
                {
                    _harmony.Patch(openConfigMethod, new HarmonyMethod(configPrefix));
                    RPGLog.Debug("Harmony patch applied to OpenConfigWindow (scroll support)");
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to apply config window patch: " + e.Message);
        }
    }
    
    /// <summary>
    /// Apply patches to enhance score and mastery points based on RPG mod activities
    /// </summary>
    private void ApplyScoringPatches()
    {
        try
        {
            // Patch Dew.GetRewardedMasteryPoints to add RPG bonus
            MethodInfo masteryMethod = typeof(Dew).GetMethod(
                "GetRewardedMasteryPoints",
                BindingFlags.Static | BindingFlags.Public);
            
            if (masteryMethod != null)
            {
                MethodInfo masteryPostfix = typeof(ScoringPatches.EnhancedMasteryPointsPatch).GetMethod(
                    "Postfix",
                    BindingFlags.Static | BindingFlags.Public);
                
                if (masteryPostfix != null)
                {
                    _harmony.Patch(masteryMethod, null, new HarmonyMethod(masteryPostfix));
                    RPGLog.Debug("Harmony patch applied to GetRewardedMasteryPoints (scoring bonus)");
                }
            }
            
            // Patch GameResultManager.UpdateGameResult to track RPG stats
            MethodInfo updateResultMethod = typeof(GameResultManager).GetMethod(
                "UpdateGameResult",
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (updateResultMethod != null)
            {
                MethodInfo updateResultPostfix = typeof(ScoringPatches.EnhancedGameResultPatch).GetMethod(
                    "Postfix",
                    BindingFlags.Static | BindingFlags.Public);
                
                if (updateResultPostfix != null)
                {
                    _harmony.Patch(updateResultMethod, null, new HarmonyMethod(updateResultPostfix));
                    RPGLog.Debug("Harmony patch applied to UpdateGameResult (scoring tracking)");
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to apply scoring patches: " + e.Message);
        }
        
        // Patch UIWindowManager.Update to block ESC when our windows are open
        try
        {
            // UIWindowManager.Update is "protected virtual void Update()"
            MethodInfo windowManagerUpdate = typeof(UIWindowManager).GetMethod(
                "Update",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (windowManagerUpdate == null)
            {
                RPGLog.Warning(" Could not find UIWindowManager.Update method!");
                // Try to list all methods for debugging
                var allMethods = typeof(UIWindowManager).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var m in allMethods)
                {
                    if (m.Name.Contains("Update") || m.DeclaringType == typeof(UIWindowManager))
                    {
                        RPGLog.Debug(" UIWindowManager method: " + m.Name + " (declaring: " + m.DeclaringType.Name + ")");
                    }
                }
            }
            else
            {
                RPGLog.Debug(" Found UIWindowManager.Update: " + windowManagerUpdate.ToString());
                
                MethodInfo blockEscPrefix = typeof(UIWindowManagerPatch).GetMethod(
                    "Prefix",
                    BindingFlags.Static | BindingFlags.Public);
                
                if (blockEscPrefix == null)
                {
                    RPGLog.Warning(" Could not find UIWindowManagerPatch.Prefix method!");
                }
                else
                {
                    RPGLog.Debug(" Found UIWindowManagerPatch.Prefix: " + blockEscPrefix.ToString());
                    _harmony.Patch(windowManagerUpdate, new HarmonyMethod(blockEscPrefix));
                    RPGLog.Debug("Harmony patch applied to UIWindowManager.Update (ESC key blocking)");
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to apply UIWindowManager patch: " + e.Message);
            RPGLog.Warning(" Stack: " + e.StackTrace);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RPGLog.Debug("Scene loaded: " + scene.name + " (mode: " + mode + ")");
        currentScene = scene.name;
        bool wasInGame = isInGame;
        isInGame = IsInGameScene(scene.name);
        RPGLog.Debug("isInGame: " + isInGame + " (was: " + wasInGame + ")");

        if (isInGame && !wasInGame)
        {
            // Just entered game - ALWAYS initialize fresh (we reset everything on exit)
            RPGLog.Debug("Entered game scene, initializing mod fresh...");
            InitializeMod();
            
            // Show consumable bar when entering game
            if (consumableBar != null)
            {
                consumableBar.SetVisible(true);
            }
        }
        else if (!isInGame && wasInGame)
        {
            // Left game - COMPLETE RESET
            RPGLog.Debug("Left game scene, performing complete mod reset...");
            
            // Save items before reset
            SaveItems();
            
            // Perform complete reset
            CompleteModReset();
        }
    }
    
    /// <summary>
    /// Completely reset the mod - destroy all instances and clear all state
    /// This ensures a fresh start for the next game
    /// </summary>
    private void CompleteModReset()
    {
        RPGLog.Debug("=== COMPLETE MOD RESET START ===");
        
        try
        {
            // 0. Remove ESC handler from GlobalUIManager
            if (_escBackHandler != null)
            {
                _escBackHandler.Remove();
                _escBackHandler = null;
                RPGLog.Debug(" Removed ESC BackHandler");
            }
            
            // 1. Reset scoring tracking for new run
            ScoringPatches.ResetTracking();
            
            // 1. Clean up networked systems first (these have event subscriptions)
            NetworkedItemSystem.Cleanup();
            DroppedItem.ClearAll();
            MonsterLootSystem.Cleanup();
            
            // 2. Clean up merchant caches
            MerchantShopUIPatches.ShopItemUpdateContentPatch.ClearCache();
            MerchantPatches.ClearCache();
            
            // 3. Clean up elemental attack system
            ElementalAttackPatches.ForceUnsubscribe();
            
            // 4. Clean up equipment manager (has event subscriptions and set bonus system)
            if (equipmentManager != null)
            {
                equipmentManager.Cleanup();
                equipmentManager = null;
            }
            
            // 5. Clean up stats system
            if (statsSystem != null)
            {
                statsSystem.Cleanup();
                statsSystem = null;
            }
            
            // 6. Clean up weapon mastery system
            if (masterySystem != null)
            {
                masterySystem.OnMasteryLevelUp -= OnMasteryLevelUp;
                masterySystem.Cleanup();
                masterySystem = null;
            }
            
            // 7. Clean up auto target system
            if (autoTargetSystem != null)
            {
                autoTargetSystem = null;
            }
            
            // 8. Clean up inventory manager
            if (inventoryManager != null)
            {
                inventoryManager.ClearInventory();
                inventoryManager = null;
            }
            
            // 9. Destroy UI objects
            if (inventoryUI != null)
            {
                inventoryUI.SetVisible(false);
                if (inventoryUI.gameObject != null)
                {
                    UnityEngine.Object.Destroy(inventoryUI.gameObject);
                }
                inventoryUI = null;
            }
            
            if (statsUI != null)
            {
                statsUI.SetVisible(false);
                if (statsUI.gameObject != null)
                {
                    UnityEngine.Object.Destroy(statsUI.gameObject);
                }
                statsUI = null;
            }
            
            if (masteryUI != null)
            {
                masteryUI.SetVisible(false);
                if (masteryUI.gameObject != null)
                {
                    UnityEngine.Object.Destroy(masteryUI.gameObject);
                }
                masteryUI = null;
            }
            
            if (resultsUI != null)
            {
                resultsUI.Cleanup();
                if (resultsUI.gameObject != null)
                {
                    UnityEngine.Object.Destroy(resultsUI.gameObject);
                }
                resultsUI = null;
            }
            
            if (consumableBar != null)
            {
                consumableBar.SetVisible(false);
                if (consumableBar.gameObject != null)
                {
                    UnityEngine.Object.Destroy(consumableBar.gameObject);
                }
                consumableBar = null;
            }
            
            if (settingsPanel != null)
            {
                if (settingsPanel.gameObject != null)
                {
                    UnityEngine.Object.Destroy(settingsPanel.gameObject);
                }
                settingsPanel = null;
            }
            
            // 10. Reset state flags
            isInventoryOpen = false;
            isStatsOpen = false;
            isMasteryOpen = false;
            _persistenceCheckPending = false;
            _persistenceCheckDone = false;
            _autoSaveTimer = 0f;
            
            // 11. Reset ItemPersistence state (but don't clear files)
            ItemPersistence.ResetState();
            
            RPGLog.Debug(" === COMPLETE MOD RESET DONE ===");
        }
        catch (Exception ex)
        {
            RPGLog.Error(" Error during mod reset: " + ex.Message);
            RPGLog.Error(" Stack trace: " + ex.StackTrace);
        }
    }
    
    /// <summary>
    /// Save all items (inventory, equipment, and stats)
    /// </summary>
    public static void SaveItems()
    {
        if (Instance == null) return;
        
        var inv = Instance.inventoryManager;
        var eq = Instance.equipmentManager;
        var stats = Instance.statsSystem;
        var mastery = Instance.masterySystem;
        
        if (inv != null && eq != null)
        {
            ItemPersistence.SaveAll(inv, eq);
            
            // Save stats
            if (stats != null)
            {
                ItemPersistence.SaveStats(stats.Serialize());
            }
            
            // Save weapon mastery (global, persists across runs)
            if (mastery != null)
            {
                ItemPersistence.SaveMastery(mastery.Serialize());
            }
            
            RPGLog.Debug(" Items and stats saved!");
        }
    }
    
    /// <summary>
    /// Get inventory manager (for external access)
    /// </summary>
    public InventoryManager GetInventoryManager()
    {
        return inventoryManager;
    }
    
    /// <summary>
    /// Get equipment manager (for external access)
    /// </summary>
    public EquipmentManager GetEquipmentManager()
    {
        return equipmentManager;
    }
    
    /// <summary>
    /// Get auto target system (for external access)
    /// </summary>
    public AutoTargetSystem GetAutoTargetSystem()
    {
        return autoTargetSystem;
    }
    
    /// <summary>
    /// Get weapon mastery system (for external access)
    /// </summary>
    public WeaponMasterySystem GetMasterySystem()
    {
        return masterySystem;
    }
    
    /// <summary>
    /// Notify that equipment has changed (updates auto target and elemental effects)
    /// </summary>
    public void OnEquipmentChanged()
    {
        if (autoTargetSystem != null)
        {
            autoTargetSystem.UpdateActiveEffects();
        }
        
        // Update elemental effects from equipped weapons
        if (equipmentManager != null)
        {
            equipmentManager.UpdateElementalEffects();
        }
        
        // Update weapon mastery bonus for new weapon
        if (masterySystem != null)
        {
            masterySystem.ApplyMasteryBonus();
        }
    }
    
    /// <summary>
    /// Get mod path (for loading item images)
    /// </summary>
    public string GetModPath()
    {
        return mod != null ? mod.path : "";
    }

    private void Start()
    {
        // Don't initialize in lobby - wait for PlayGame or Room scene
        currentScene = SceneManager.GetActiveScene().name;
        isInGame = IsInGameScene(currentScene);
        
        RPGLog.Debug(" Start() called, scene: " + currentScene + ", isInGame: " + isInGame);
        
        if (isInGame)
        {
            InitializeMod();
        }
    }

    private void InitializeMod()
    {
        try
        {
            RPGLog.Debug(" === INITIALIZING MOD (FRESH) ===");
            
            // Initialize persistence system first
            ItemPersistence.Initialize(mod.path);
            
            // Initialize item database (central source for all items)
            ItemDatabase.Initialize(mod.path);
            RPGLog.Debug(" ItemDatabase initialized with " + ItemDatabase.GetAllItems().Count + " items");
            
            // Always create fresh instances - no null checks!
            
            // Initialize inventory manager
            inventoryManager = new InventoryManager();
            inventoryManager.Initialize();
            inventoryManager.SetModPath(mod.path);
            
            // Initialize equipment manager
            equipmentManager = new EquipmentManager();
            equipmentManager.Initialize();
            equipmentManager.SetInventoryManager(inventoryManager);
            
            // Register with elemental attack patch
            ElementalAttackPatches.SetEquipmentManager(equipmentManager);
            
            // Initialize auto target system
            autoTargetSystem = new AutoTargetSystem();
            autoTargetSystem.Initialize(equipmentManager);
            
            // Set reference for auto-aim patches
            AutoAimPatches.SetAutoTargetSystem(autoTargetSystem);
            
            // Initialize stats system
            statsSystem = new StatsSystem();
            statsSystem.SetEquipmentManager(equipmentManager);
            
            // Initialize weapon mastery system
            masterySystem = new WeaponMasterySystem();
            masterySystem.SetEquipmentManager(equipmentManager);
            masterySystem.Initialize();
            
            // Subscribe to level up events
            masterySystem.OnMasteryLevelUp += OnMasteryLevelUp;
            
            // Initialize weapon mastery UI
            GameObject masteryUIObj = new GameObject("WeaponMasteryUI");
            masteryUIObj.transform.SetParent(transform);
            masteryUI = masteryUIObj.AddComponent<WeaponMasteryUI>();
            masteryUI.Initialize(masterySystem);
            
            // Initialize RPG results UI (shows stats on game result screen)
            GameObject resultsUIObj = new GameObject("RPGResultsUI");
            resultsUIObj.transform.SetParent(transform);
            resultsUI = resultsUIObj.AddComponent<RPGResultsUI>();
            resultsUI.Initialize();
            
            // NOTE: Persistence check is delayed until world seed is ready
            // See Update() for the delayed check
            _persistenceCheckPending = true;
            _persistenceCheckDone = false;
            
            // Initialize networked item system for multiplayer support
            InitializeNetworkedItemSystem();
            
            // Initialize monster loot system (server-side drops)
            InitializeMonsterLootSystem();

            // Create UI
            CreateInventoryUI();
            
            // Create stats UI
            CreateStatsUI();

            RPGLog.Debug(" === MOD INITIALIZATION COMPLETE ===");
        }
        catch (Exception ex)
        {
            RPGLog.Error(" Error initializing mod: " + ex.Message);
            RPGLog.Error(" Stack trace: " + ex.StackTrace);
        }
    }

    private bool IsInGameScene(string sceneName)
    {
        // PlayGame is the main game scene, Room_* are room sub-scenes within the game
        return sceneName == "PlayGame" || sceneName.StartsWith("Room_");
    }

    private void Update()
    {
        // Sync language setting
        Localization.CurrentLanguage = config.language;
        
        // Update scene check every frame in case scene changes
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName != currentScene)
        {
            currentScene = currentSceneName;
            bool wasInGame = isInGame;
            isInGame = IsInGameScene(currentScene);
            
            RPGLog.Debug(" Scene changed to: " + currentSceneName + ", isInGame: " + isInGame);
            
            if (isInGame && !wasInGame)
            {
                // Just entered game - ALWAYS initialize fresh (we reset everything on exit)
                RPGLog.Debug(" Entered game scene (via Update), initializing mod fresh...");
                InitializeMod();
                
                // Show consumable bar when entering game
                if (consumableBar != null)
                {
                    consumableBar.SetVisible(true);
                }
            }
            else if (!isInGame && wasInGame)
            {
                // Left game - COMPLETE RESET
                RPGLog.Debug(" Left game scene (via Update), performing complete mod reset...");
                
                // Save items before reset
                SaveItems();
                
                // Perform complete reset
                CompleteModReset();
            }
        }
        
        // Delayed persistence check - wait for world seed to be ready
        if (isInGame && _persistenceCheckPending && !_persistenceCheckDone)
        {
            TryDelayedPersistenceCheck();
        }
        
        // Process networked item sync (checks for new shared items)
        if (isInGame)
        {
            NetworkedItemSystem.ProcessIncomingMessages();
            
            // Check if monster loot system needs delayed subscription (server might not be ready on init)
            MonsterLootSystem.CheckDelayedSubscription();
        }
        
        // Only allow inventory toggle when in-game and not typing in chat
        if (isInGame)
        {
            // Ensure mod is initialized (safety check for clients joining late)
            if (inventoryManager == null || equipmentManager == null || statsSystem == null || masterySystem == null)
            {
                RPGLog.Debug(" Late initialization for client (missing systems)...");
                InitializeMod();
            }
            
            // ESC key handling is done via GlobalUIManager.AddBackHandler
            // which properly integrates with the game's ESC system
            
            // Don't toggle if player is typing in chat or any input field
            if (CheckInventoryKeybind() && !ControlManager.IsInputFieldFocused())
            {
                ToggleInventory();
            }
            
            // Toggle stats window with K key
            if (Input.GetKeyDown(KeyCode.K) && !ControlManager.IsInputFieldFocused())
            {
                ToggleStats();
            }
            
            // Toggle weapon mastery window with N key
            if (Input.GetKeyDown(KeyCode.N) && !ControlManager.IsInputFieldFocused())
            {
                ToggleMastery();
            }
            
            // Auto-save periodically while in-game
            if (_persistenceCheckDone)
            {
                _autoSaveTimer += Time.deltaTime;
                if (_autoSaveTimer >= AUTO_SAVE_INTERVAL)
                {
                    _autoSaveTimer = 0f;
                    SaveItems();
                    RPGLog.Debug(" Auto-saved items");
                }
            }
            
            // Update auto target system (auto attack, auto aim)
            if (autoTargetSystem != null)
            {
                autoTargetSystem.Update();
            }
            
            // Update stats system (check for level ups)
            if (statsSystem != null)
            {
                statsSystem.Update();
            }
            
            // Update weapon mastery system (ensure subscribed to attacks)
            if (masterySystem != null)
            {
                masterySystem.Update();
            }
            
            // Check if equipment stats need to be reapplied (hero wasn't ready during load)
            if (equipmentManager != null)
            {
                equipmentManager.CheckPendingStatReapply();
                // Update shield regeneration
                equipmentManager.UpdateShieldRegeneration(Time.deltaTime);
                
                // Update set bonus system (for buff durations like Wind Rush)
                SetBonusSystem setBonus = equipmentManager.SetBonusSystem;
                if (setBonus != null)
                {
                    setBonus.UpdateBuffs();
                }
            }
            
            // Update ChatPatches (for client Wind Rush buff expiration on server)
            ChatManagerPatch.Update();
            
            // Check for middle-click to share dropped items
            DroppedItem.CheckMiddleClickShare();
        }
    }
    
    private void ToggleStats()
    {
        if (statsUI == null)
        {
            RPGLog.Warning(" ToggleStats: statsUI is null! Attempting to create...");
            CreateStatsUI();
            if (statsUI == null)
            {
                RPGLog.Error(" ToggleStats: Failed to create statsUI!");
                return;
            }
        }
        
        isStatsOpen = !isStatsOpen;
        statsUI.SetVisible(isStatsOpen);
        
        RPGLog.Debug(" Stats window " + (isStatsOpen ? "opened" : "closed"));
    }
    
    /// <summary>
    /// Check if the inventory keybind is pressed (supports key combinations)
    /// </summary>
    private bool CheckInventoryKeybind()
    {
        // Get the configured key
        KeyCode inventoryKeyCode = config.GetInventoryKeyCode();
        
        // Check if primary key is pressed this frame
        if (!Input.GetKeyDown(inventoryKeyCode))
        {
            return false;
        }
        
        // Check modifier keys
        bool modifierSatisfied = true;
        
        // Check Left Shift
        if (config.requireShift)
        {
            modifierSatisfied = modifierSatisfied && Input.GetKey(KeyCode.LeftShift);
        }
        
        // Check Left Ctrl
        if (config.requireCtrl)
        {
            modifierSatisfied = modifierSatisfied && Input.GetKey(KeyCode.LeftControl);
        }
        
        // Check Left Alt
        if (config.requireAlt)
        {
            modifierSatisfied = modifierSatisfied && Input.GetKey(KeyCode.LeftAlt);
        }
        
        return modifierSatisfied;
    }
    
    /// <summary>
    /// Called when config is changed via the mod settings menu
    /// </summary>
    public override void OnConfigChanged()
    {
        base.OnConfigChanged();
        
        RPGLog.Debug(" ========== CONFIG APPLIED ==========");
        
        // Log keybind settings
        string keybindStr = config.inventoryKey;
        if (config.requireShift) keybindStr = "Shift+" + keybindStr;
        if (config.requireCtrl) keybindStr = "Ctrl+" + keybindStr;
        if (config.requireAlt) keybindStr = "Alt+" + keybindStr;
        RPGLog.Debug(" Inventory keybind: " + keybindStr);
        
        // Log stat system settings
        RPGLog.Debug(" Stat System - Points/Level: " + config.pointsPerLevel + 
            ", ATK/STR: " + config.atkPerStrength + 
            ", HP/VIT: " + config.hpPerVitality + 
            ", DEF/DEF: " + config.defPerDefense + 
            ", AP/INT: " + config.apPerIntelligence);
        
        // Log drop rate settings
        RPGLog.Debug(" Drop Rates - Equip: " + config.equipmentDropRate + "%" +
            ", Consumable: " + config.consumableDropRate + "%" +
            ", Boss Mult: " + config.bossDropMultiplier + "x");
        
        // Log rarity weights
        RPGLog.Debug(" Rarity Weights - Common: " + config.commonWeight + 
            ", Rare: " + config.rareWeight + 
            ", Epic: " + config.epicWeight + 
            ", Legendary: " + config.legendaryWeight);
        
        // Log set bonus settings
        RPGLog.Debug(" Set Bonus - Enabled: " + config.enableSetBonuses + 
            ", Common CD: " + config.setBonusProcCooldownCommon + "s" +
            ", Rare CD: " + config.setBonusProcCooldownRare + "s" +
            ", Epic CD: " + config.setBonusProcCooldownEpic + "s" +
            ", Legendary CD: " + config.setBonusProcCooldownLegendary + "s");
        
        // Log mastery settings
        RPGLog.Debug(" Mastery - XP/Hit: " + config.masteryXpPerHit + 
            ", Damage/Level: " + config.masteryDamagePerLevel + "%" +
            ", Crit/Level: " + config.masteryCritPerLevel + "%");
        
        // Log gold/dust settings
        RPGLog.Debug(" Gold/Dust - Gold: " + config.goldOnKillMinNormal + "-" + config.goldOnKillMaxNormal +
            ", Dust: " + config.dustOnKillMin + "-" + config.dustOnKillMax);
        
        RPGLog.Debug(" ====================================");
        
        // Refresh set bonus system if it exists
        if (equipmentManager != null && equipmentManager.SetBonusSystem != null)
        {
            // Refresh cooldowns from config
            SetBonusSystem.RefreshCooldownsFromConfig();
            RPGLog.Debug(" Set bonus cooldowns updated from config");
        }
        
        // Show in-game notification
        try
        {
            if (InGameUIManager.instance != null)
            {
                InGameUIManager.instance.ShowCenterMessage(CenterMessageType.General, "RPG Settings Applied!");
            }
        }
        catch { }
    }
    
    private void TryDelayedPersistenceCheck()
    {
        // Check if GameManager is ready with a valid runId
        // GameManager.runId is the game's official unique identifier for each run
        if (NetworkedManagerBase<GameManager>.instance == null) return;
        
        string runId = NetworkedManagerBase<GameManager>.instance.runId;
        if (string.IsNullOrEmpty(runId))
        {
            // runId not ready yet - wait
            return;
        }
        
        RPGLog.Debug(" GameManager.runId ready: " + runId + ", checking persistence...");
        
        bool isNewRun = ItemPersistence.CheckAndHandleNewRun(inventoryManager, equipmentManager);
        
        if (isNewRun)
        {
            // New run - CLEAR EVERYTHING first, then create example items
            RPGLog.Debug(" New run detected, clearing old data and creating fresh items...");
            
            // Reset inventory to initial slot count (removes expanded slots from previous run)
            inventoryManager.ResetToInitialSlots();
            
            // Clear equipment (removes equipped items from previous run)
            equipmentManager.UnequipAll();
            
            // Clear consumable bar
            if (consumableBar != null)
            {
                consumableBar.ClearAllSlots();
            }
            
            // Clear fast slots in UI
            if (inventoryUI != null)
            {
                inventoryUI.ClearAllFastSlots();
            }
            
            // Reset stats for new run
            if (statsSystem != null)
            {
                statsSystem.ResetForNewRun();
            }
            
            // Reset weapon mastery for new run
            if (masterySystem != null)
            {
                masterySystem.ResetForNewRun();
                RPGLog.Debug(" Weapon mastery reset for new run");
            }
            
            // Now create fresh example items
            inventoryManager.CreateExampleItems();
            RPGLog.Debug(" Fresh items created for new run!");
            
            // Equip starter potions to first fast slot
            RPGItem starterPotion = inventoryManager.GetItem(0); // First item is the starter potion
            if (starterPotion != null && starterPotion.type == ItemType.Consumable)
            {
                equipmentManager.SetConsumable(0, starterPotion);
                if (consumableBar != null)
                {
                    ConsumableSlot slot = consumableBar.GetSlot(0);
                    if (slot != null)
                    {
                        slot.SetItem(starterPotion);
                    }
                }
                // Remove from inventory (consumables disappear when equipped to fast slot)
                inventoryManager.RemoveItem(0);
                RPGLog.Debug(" Equipped starter potions to fast slot 1");
            }
        }
        else
        {
            RPGLog.Debug(" Continuing run, loaded saved items");
            
            // Sync consumable bar from loaded equipment
            if (consumableBar != null)
            {
                consumableBar.SyncFromEquipment();
            }
            
            // Load saved stats
            if (statsSystem != null)
            {
                string statsJson = ItemPersistence.LoadStats();
                if (!string.IsNullOrEmpty(statsJson))
                {
                    statsSystem.Deserialize(statsJson);
                    RPGLog.Debug(" Loaded saved stats");
                }
            }
            
            // Load weapon mastery for continued run
            if (masterySystem != null)
            {
                string masteryData = ItemPersistence.LoadMastery();
                if (!string.IsNullOrEmpty(masteryData))
                {
                    masterySystem.Deserialize(masteryData);
                    RPGLog.Debug(" Loaded weapon mastery data for continued run");
                }
            }
        }
        
        // Refresh UI to show loaded items
        if (inventoryUI != null)
        {
            inventoryUI.RefreshInventory();
            inventoryUI.RefreshFastSlots();
        }
        
        // Update elemental effects from loaded equipment and subscribe to hero attacks
        if (equipmentManager != null)
        {
            equipmentManager.UpdateElementalEffects();
        }
        
        // Subscribe to attacks for XP gain and apply mastery bonus
        // If hero isn't ready yet, mark for reapply (will be done in Update)
        if (masterySystem != null)
        {
            if (DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                masterySystem.SubscribeToAttacks(equipmentManager);
                masterySystem.ApplyMasteryBonus();
            }
            else
            {
                // Hero not ready yet, mark for reapply when available
                masterySystem.MarkForReapply();
                RPGLog.Debug(" Hero not ready, mastery will be applied when available");
            }
        }
        
        _persistenceCheckDone = true;
        _persistenceCheckPending = false;
    }
    
    /// <summary>
    /// Called when weapon mastery levels up
    /// </summary>
    private void OnMasteryLevelUp(string heroId, string weaponName, int newLevel)
    {
        // Track for scoring
        ScoringPatches.TrackWeaponMasteryLevelGained();
        
        // Show notification
        string message = string.Format("{0} {1} Lv{2}!", weaponName, Localization.MasteryLevelUp, newLevel);
        
        // Could add a visual notification here
        RPGLog.Debug(" " + heroId + ": " + message);
        
        // Save mastery progress
        if (masterySystem != null)
        {
            ItemPersistence.SaveMastery(masterySystem.Serialize());
        }
    }
    
    private void InitializeNetworkedItemSystem()
    {
        RPGLog.Debug(" Initializing networked item system...");
        
        NetworkedItemSystem.Initialize();
        NetworkedItemSystem.SetModPath(mod.path);
        NetworkedItemSystem.LocalInventoryManager = inventoryManager;
        
        // Register server handlers if we're the host
        if (Mirror.NetworkServer.active)
        {
            NetworkedItemSystem.RegisterServerHandlers();
            RPGLog.Debug(" Registered as HOST - server handlers active");
        }
        
        // Hook up callbacks
        NetworkedItemSystem.OnItemDroppedLocally = OnNetworkedItemDropped;
        NetworkedItemSystem.OnItemPickedUpLocally = OnNetworkedItemPickedUp;
        NetworkedItemSystem.OnDismantleProgressLocally = OnNetworkedDismantleProgress;
        NetworkedItemSystem.OnItemDismantledLocally = OnNetworkedItemDismantled;
        NetworkedItemSystem.OnItemReceivedLocally = OnNetworkedItemReceived;
        
        // Hook into chat messages for multiplayer sync
        HookChatMessages();
        
        RPGLog.Debug(" Networked item system callbacks registered");
    }
    
    private void InitializeMonsterLootSystem()
    {
        RPGLog.Debug(" Initializing monster loot system...");
        
        try
        {
            // Get all item templates from ItemDatabase (central source)
            List<RPGItem> itemTemplates = ItemDatabase.GetAllItems();
            
            if (itemTemplates == null || itemTemplates.Count == 0)
            {
                RPGLog.Warning(" No item templates available for monster loot!");
                return;
            }
            
            // Initialize the monster loot system with all item templates
            MonsterLootSystem.Initialize(itemTemplates);
            
            RPGLog.Debug(" Monster loot system initialized with " + itemTemplates.Count + " item templates");
        }
        catch (Exception e)
        {
            RPGLog.Error(" Failed to initialize monster loot system: " + e.Message);
        }
    }
    
    private bool _chatHooked = false;
    
    private void HookChatMessages()
    {
        if (_chatHooked) return;
        
        try
        {
            if (NetworkedManagerBase<ChatManager>.instance != null)
            {
                NetworkedManagerBase<ChatManager>.instance.ClientEvent_OnMessageReceived += OnChatMessageReceived;
                _chatHooked = true;
                RPGLog.Debug(" Hooked into ChatManager for multiplayer sync!");
            }
            else
            {
                RPGLog.Debug(" ChatManager not available yet for hook");
            }
        }
        catch (Exception e)
        {
            RPGLog.Error(" Failed to hook chat: " + e.Message);
        }
    }
    
    private void OnChatMessageReceived(ChatManager.Message message)
    {
        // Process our sync messages
        // - Client requests: [RPGREQ]... (visible, sent via CmdSendChatMessage)
        // - Host broadcasts: <size=0>RI... (hidden, sent via BroadcastChatMessage as Raw)
        if (!string.IsNullOrEmpty(message.content))
        {
            // Check for client request prefix OR host broadcast prefix
            if (message.content.Contains("[RPGREQ]") || message.content.Contains("<size=0>RI"))
            {
                NetworkedItemSystem.ProcessChatMessage(message.content);
            }
        }
    }
    
    private void OnNetworkedItemDropped(uint dropId, RPGItem item, Vector3 position)
    {
        // Create local visual for the dropped item
        RPGLog.Debug("[RPGItemsMod] Creating DroppedItem: " + item.name + " (dropId=" + dropId + ")");
        DroppedItem.CreateFromNetwork(dropId, item, position);
    }
    
    private void OnNetworkedItemPickedUp(uint dropId)
    {
        // Remove local visual
        DroppedItem dropped = DroppedItem.GetByNetworkId(dropId);
        if (dropped != null)
        {
            dropped.CleanupAndDestroy();
        }
    }
    
    private void OnNetworkedDismantleProgress(uint dropId, float progress)
    {
        // Update local visual's dismantle progress
        DroppedItem dropped = DroppedItem.GetByNetworkId(dropId);
        if (dropped != null)
        {
            dropped.SetDismantleProgress(progress);
        }
    }
    
    private void OnNetworkedItemDismantled(uint dropId, int dustAmount)
    {
        // Remove local visual (dust is spawned by NetworkedItemSystem)
        DroppedItem dropped = DroppedItem.GetByNetworkId(dropId);
        if (dropped != null)
        {
            dropped.CleanupAndDestroy();
        }
    }
    
    private void OnNetworkedItemReceived(RPGItem item)
    {
        // Add item to our inventory
        if (inventoryManager != null && item != null)
        {
            // Track item collection for scoring
            ScoringPatches.TrackItemCollected(item.rarity);
            
            // Special handling for consumables: try fast slots first with auto-stack
            if (item.type == ItemType.Consumable)
            {
                bool handled = TryAddConsumableToFastSlots(item);
                if (handled)
                {
                    RPGLog.Debug(" Added consumable to fast slot: " + item.name + " x" + item.currentStack);
                    SaveItems();
                    return;
                }
            }
            
            // Regular item or consumable that didn't fit in fast slots
            bool added = inventoryManager.AddItem(item);
            if (added)
            {
            }
            else
            {
                RPGLog.Debug(" Inventory full, couldn't receive: " + item.name);
            }
        }
    }
    
    /// <summary>
    /// Try to add a consumable to fast slots with auto-stacking
    /// Returns true if the item was fully handled (added or stacked)
    /// </summary>
    private bool TryAddConsumableToFastSlots(RPGItem item)
    {
        if (equipmentManager == null || consumableBar == null) return false;
        
        // First, try to stack with existing same-type consumable in fast slots
        for (int i = 0; i < 4; i++)
        {
            RPGItem existingItem = equipmentManager.GetConsumable(i);
            if (existingItem != null && existingItem.id == item.id)
            {
                // Same item type - stack them
                existingItem.currentStack += item.currentStack;
                
                // Sync the updated stack count to EquipmentManager and update display
                ConsumableSlot slot = consumableBar.GetSlot(i);
                if (slot != null)
                {
                    slot.SetItemAndSync(existingItem);
                }
                else
                {
                    // Fallback: update EquipmentManager directly if slot not found
                    equipmentManager.SetConsumable(i, existingItem);
                }
                
                // Update fast slot UI if inventory is open
                if (inventoryUI != null)
                {
                    inventoryUI.RefreshFastSlots();
                }
                
                RPGLog.Debug(" Stacked picked up consumable: " + existingItem.name + " x" + existingItem.currentStack);
                return true;
            }
        }
        
        // No existing stack found, try to find an empty fast slot
        for (int i = 0; i < 4; i++)
        {
            RPGItem existingItem = equipmentManager.GetConsumable(i);
            if (existingItem == null)
            {
                // Empty slot - put the consumable here
                equipmentManager.SetConsumable(i, item);
                
                // Sync to consumable bar display
                ConsumableSlot slot = consumableBar.GetSlot(i);
                if (slot != null)
                {
                    slot.SetItemAndSync(item);
                }
                
                // Update fast slot UI if inventory is open
                if (inventoryUI != null)
                {
                    inventoryUI.RefreshFastSlots();
                }
                
                RPGLog.Debug(" Added picked up consumable to empty slot: " + item.name + " x" + item.currentStack);
                return true;
            }
        }
        
        // No empty fast slot - check if we can stack with inventory consumables
        // and then move to fast slot
        return false; // Let it go to inventory
    }
    
    /// <summary>
    /// Called when a consumable is used from the HUD bar
    /// Refreshes inventory and fast slots to show updated stack counts
    /// </summary>
    private void OnConsumableUsed()
    {
        RPGLog.Debug(" Consumable used, refreshing UI and saving...");
        
        // Track for scoring
        ScoringPatches.TrackConsumableUsed();
        
        // Refresh inventory slots to show updated stack counts
        if (inventoryUI != null)
        {
            inventoryUI.RefreshInventory();
            inventoryUI.RefreshFastSlots();
        }
        
        // Save after using a consumable (important state change)
        SaveItems();
    }

    private void CreateInventoryUI()
    {
        RPGLog.Debug(" Creating inventory UI (all UGUI)...");
        
        // ALWAYS destroy existing UI objects first to ensure fresh creation
        // This is critical after CompleteModReset to prevent stale references
        GameObject existingUI = GameObject.Find("RPGItemsMod_UI");
        if (existingUI != null)
        {
            RPGLog.Debug(" Destroying existing UI object for fresh creation");
            Destroy(existingUI);
        }
        
        // Destroy any existing consumable bars
        ConsumableBar[] existingBars = UnityEngine.Object.FindObjectsByType<ConsumableBar>(FindObjectsSortMode.None);
        foreach (ConsumableBar bar in existingBars)
        {
            RPGLog.Debug(" Destroying existing consumable bar");
            Destroy(bar.gameObject);
        }
        
        // Create fresh UI
        GameObject uiObject = new GameObject("RPGItemsMod_UI");
        DontDestroyOnLoad(uiObject);
        
        // Create main inventory UI (opens/closes with I key)
        inventoryUI = uiObject.AddComponent<InventoryUI>();
        inventoryUI.Initialize(inventoryManager, equipmentManager, mod.path, config.uiStyle);
        inventoryUI.OnEquipmentChanged = OnEquipmentChanged;
        
        // Create always-visible consumable bar (HUD element)
        GameObject barObject = new GameObject("RPGItemsMod_ConsumableBar");
        DontDestroyOnLoad(barObject);
        consumableBar = barObject.AddComponent<ConsumableBar>();
        consumableBar.Initialize(equipmentManager, inventoryManager);
        
        // Hook up callback to refresh UI when consumables are used
        consumableBar.OnConsumableUsed = OnConsumableUsed;
        
        // Link inventory UI to consumable bar
        inventoryUI.SetConsumableBar(consumableBar);
        
        // Create settings panel for hotkey customization
        settingsPanel = uiObject.AddComponent<SettingsPanel>();
        settingsPanel.Initialize(inventoryUI.GetCanvas(), consumableBar);
        
        // IMPORTANT: Show consumable bar immediately after creation
        consumableBar.SetVisible(true);
        
        // Register ESC handler with GlobalUIManager to close mod windows
        // Use high priority (5000) so we handle ESC before game's menu (priority 0)
        RegisterEscapeHandler();
        
        RPGLog.Debug(" Inventory UI, consumable bar, and settings created!");
    }
    
    /// <summary>
    /// Register a BackHandler with GlobalUIManager to handle ESC for closing mod windows.
    /// This integrates properly with the game's ESC handling system.
    /// </summary>
    private void RegisterEscapeHandler()
    {
        try
        {
            var globalUI = ManagerBase<GlobalUIManager>.instance;
            if (globalUI == null)
            {
                RPGLog.Warning(" GlobalUIManager not available for ESC handler");
                return;
            }
            
            // Remove old handler if exists
            if (_escBackHandler != null)
            {
                _escBackHandler.Remove();
                _escBackHandler = null;
            }
            
            // Register with high priority (5000) to handle before menu (0)
            _escBackHandler = globalUI.AddBackHandler(this, 5000, () =>
            {
                // Check if any mod window is open
                bool anyWindowOpen = isInventoryOpen || isStatsOpen || isMasteryOpen;
                
                if (!anyWindowOpen)
                {
                    return false; // Don't consume ESC, let other handlers process it
                }
                
                RPGLog.Debug(" BackHandler: Closing mod windows via ESC");
                
                // Close all open mod windows
                if (isInventoryOpen && inventoryUI != null)
                {
                    isInventoryOpen = false;
                    inventoryUI.SetVisible(false);
                    if (settingsPanel != null) settingsPanel.SetVisible(false);
                    SaveItems();
                }
                
                if (isStatsOpen && statsUI != null)
                {
                    isStatsOpen = false;
                    statsUI.SetVisible(false);
                }
                
                if (isMasteryOpen && masteryUI != null)
                {
                    isMasteryOpen = false;
                    masteryUI.SetVisible(false);
                }
                
                return true; // Consumed ESC, don't let menu open
            });
            
            RPGLog.Debug(" Registered ESC BackHandler with GlobalUIManager (priority 5000)");
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to register ESC handler: " + e.Message);
        }
    }
    
    private void CreateStatsUI()
    {
        RPGLog.Debug(" Creating stats UI...");
        
        // Ensure stats system exists first
        if (statsSystem == null)
        {
            RPGLog.Debug(" Creating stats system for UI...");
            statsSystem = new StatsSystem();
            statsSystem.SetEquipmentManager(equipmentManager);
        }
        
        // Check if UI already exists
        GameObject existingStatsUI = GameObject.Find("RPGItemsMod_StatsUI");
        if (existingStatsUI != null)
        {
            RPGLog.Debug(" Stats UI already exists, reusing...");
            statsUI = existingStatsUI.GetComponent<StatsUI>();
            if (statsUI != null) return;
        }
        
        GameObject statsObject = new GameObject("RPGItemsMod_StatsUI");
        DontDestroyOnLoad(statsObject);
        
        statsUI = statsObject.AddComponent<StatsUI>();
        statsUI.Initialize(statsSystem);
        
        RPGLog.Debug(" Stats UI created!");
    }

    private void ToggleInventory()
    {
        RPGLog.Debug(" ToggleInventory called. inventoryUI is null: " + (inventoryUI == null));
        if (inventoryUI != null)
        {
            isInventoryOpen = !isInventoryOpen;
            RPGLog.Debug(" Setting inventory visible: " + isInventoryOpen);
            inventoryUI.SetVisible(isInventoryOpen);
            
            // Potion hotbar stays visible - don't hide it
            // Only close settings if inventory is closed
            if (!isInventoryOpen && settingsPanel != null)
            {
                settingsPanel.SetVisible(false);
            }
            
            // Auto-save when closing inventory
            if (!isInventoryOpen)
            {
                SaveItems();
            }
        }
        else
        {
            RPGLog.Warning(" Cannot toggle inventory - UI not initialized!");
        }
    }
    
    // Public method to drop an item at player's position
    public void DropItemAtPlayer(RPGItem item)
    {
        DroppedItem.DropItemAtPlayer(item);
    }
    
    private void ToggleMastery()
    {
        if (masteryUI == null)
        {
            RPGLog.Warning(" ToggleMastery: masteryUI is null! Attempting to create...");
            CreateMasteryUI();
            if (masteryUI == null)
            {
                RPGLog.Error(" ToggleMastery: Failed to create masteryUI!");
                return;
            }
        }
        
        isMasteryOpen = !isMasteryOpen;
        masteryUI.SetVisible(isMasteryOpen);
        
        RPGLog.Debug(" Mastery window " + (isMasteryOpen ? "opened" : "closed"));
    }
    
    private void CreateMasteryUI()
    {
        RPGLog.Debug(" Creating mastery UI...");
        
        // Check if UI already exists
        GameObject existingMasteryUI = GameObject.Find("WeaponMasteryUI");
        if (existingMasteryUI != null)
        {
            RPGLog.Debug(" Mastery UI already exists, reusing...");
            masteryUI = existingMasteryUI.GetComponent<WeaponMasteryUI>();
            if (masteryUI != null) return;
        }
        
        // Make sure mastery system exists
        if (masterySystem == null)
        {
            masterySystem = new WeaponMasterySystem();
            masterySystem.SetEquipmentManager(equipmentManager);
            masterySystem.Initialize();
            masterySystem.OnMasteryLevelUp += OnMasteryLevelUp;
        }
        
        GameObject masteryUIObj = new GameObject("WeaponMasteryUI");
        masteryUIObj.transform.SetParent(transform);
        masteryUI = masteryUIObj.AddComponent<WeaponMasteryUI>();
        masteryUI.Initialize(masterySystem);
        
        RPGLog.Debug(" Mastery UI created!");
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Unhook chat messages
        UnhookChatMessages();
        
        if (inventoryUI != null)
        {
            UnityEngine.Object.Destroy(inventoryUI.gameObject);
        }
        if (consumableBar != null)
        {
            UnityEngine.Object.Destroy(consumableBar.gameObject);
        }
    }
    
    private void UnhookChatMessages()
    {
        if (!_chatHooked) return;
        
        try
        {
            if (NetworkedManagerBase<ChatManager>.instance != null)
            {
                NetworkedManagerBase<ChatManager>.instance.ClientEvent_OnMessageReceived -= OnChatMessageReceived;
                _chatHooked = false;
                RPGLog.Debug(" Unhooked from ChatManager");
            }
        }
        catch (Exception e)
        {
            RPGLog.Error(" Failed to unhook chat: " + e.Message);
        }
    }
}

/// <summary>
/// Harmony patch to add scroll support to mod config windows
/// </summary>
public static class ConfigWindowScrollPatch
{
    public static bool Prefix(LoadedModInstance mod)
    {
        try
        {
            // Create window same as original
            UI_Window newWindow = UnityEngine.Object.Instantiate(DewGUI.widgetWindow, DewGUI.canvasTransform);
            newWindow.isDraggable = false;
            newWindow.enableBackdrop = true;
            // Scale window width based on screen resolution and UI scale
            float uiScale = RPGItemsMod.GetUIScaleWithScreenFactor();
            float baseWindowWidth = 1350f;
            float windowWidth = baseWindowWidth * uiScale;
            float screenWidth = Screen.width;
            windowWidth = Mathf.Clamp(windowWidth, 1000f, screenWidth * 0.9f);
            newWindow.SetWidth(windowWidth);
            
            // Add logic to close on mod reload
            CustomLogicBehavior customLogicBehavior = newWindow.gameObject.AddComponent<CustomLogicBehavior>();
            customLogicBehavior.onFrameUpdate = (Action)Delegate.Combine(customLogicBehavior.onFrameUpdate, (Action)delegate
            {
                if (DewMod.isLoadingMod)
                {
                    UnityEngine.Object.Destroy(newWindow.gameObject);
                }
            });
            
            // === KEY CHANGE: Create a scroll view to hold all settings ===
            // Get the window's content area (the VerticalLayoutGroup)
            VerticalLayoutGroup windowLayout = newWindow.GetComponentInChildren<VerticalLayoutGroup>();
            Transform contentParent = windowLayout != null ? windowLayout.transform : newWindow.transform;
            
            // Create scroll view container
            GameObject scrollContainer = new GameObject("ScrollContainer");
            scrollContainer.transform.SetParent(contentParent, false);
            
            RectTransform scrollContainerRect = scrollContainer.AddComponent<RectTransform>();
            scrollContainerRect.anchorMin = Vector2.zero;
            scrollContainerRect.anchorMax = Vector2.one;
            scrollContainerRect.sizeDelta = Vector2.zero;
            
            LayoutElement scrollLayoutElement = scrollContainer.AddComponent<LayoutElement>();
            scrollLayoutElement.preferredHeight = 800f; // Max height before scrolling
            scrollLayoutElement.flexibleWidth = 1f;
            
            // Create ScrollRect
            ScrollRect scrollRect = scrollContainer.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;
            
            // Create viewport with mask
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollContainer.transform, false);
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = new Vector2(0.5f, 1f);
            
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0.01f); // Nearly invisible but needed for mask
            
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            scrollRect.viewport = viewportRect;
            
            // Create content container for settings
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 0);
            
            // Add vertical layout group to content
            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = 5f;
            contentLayout.padding = new RectOffset(20, 20, 10, 10);
            
            // Add content size fitter to auto-size based on children
            ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollRect.content = contentRect;
            
            // Create vertical scrollbar
            GameObject scrollbarObj = new GameObject("Scrollbar");
            scrollbarObj.transform.SetParent(scrollContainer.transform, false);
            
            RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1, 0);
            scrollbarRect.anchorMax = new Vector2(1, 1);
            scrollbarRect.pivot = new Vector2(1, 0.5f);
            scrollbarRect.sizeDelta = new Vector2(20, 0);
            scrollbarRect.anchoredPosition = new Vector2(0, 0);
            
            Image scrollbarBg = scrollbarObj.AddComponent<Image>();
            scrollbarBg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            
            Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            
            // Create scrollbar handle
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(scrollbarObj.transform, false);
            
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.sizeDelta = new Vector2(-4, -4);
            handleAreaRect.anchoredPosition = Vector2.zero;
            
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.sizeDelta = Vector2.zero;
            handleRect.anchoredPosition = Vector2.zero;
            
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            
            // Link scrollbar to scroll rect
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarSpacing = -3;
            
            // Adjust viewport to make room for scrollbar
            viewportRect.anchorMax = new Vector2(1, 1);
            viewportRect.offsetMax = new Vector2(-22, 0);
            
            // Now build widgets into the scroll content instead of window directly
            ModBehaviour[] modBehaviours = mod.container.GetComponents<ModBehaviour>();
            SafeAction onReset = new SafeAction();
            SafeAction onRevert = new SafeAction();
            SafeAction onBeforeApply = new SafeAction();
            SafeAction onAfterApply = new SafeAction();
            SafeAction onDirty = new SafeAction();
            
            try
            {
                foreach (ModBehaviour m in modBehaviours)
                {
                    FieldInfo[] modConfigFields = m.modConfigFields;
                    foreach (FieldInfo f in modConfigFields)
                    {
                        ModConfig config = (ModConfig)f.GetValue(m);
                        ModConfig localState = config.Clone();
                        ModConfig defaultState = (ModConfig)Activator.CreateInstance(f.FieldType);
                        
                        // Build widgets into scroll content
                        SafeAction onChanged;
                        SafeAction requestUpdate;
                        localState.BuildWidgets(content.transform, out onChanged, out requestUpdate);
                        
                        onChanged.Add(delegate { onDirty.Invoke(); });
                        onReset.Add(delegate
                        {
                            defaultState.CopyTo(localState);
                            requestUpdate.Invoke();
                        });
                        onRevert.Add(delegate
                        {
                            config.CopyTo(localState);
                            requestUpdate.Invoke();
                        });
                        onBeforeApply.Add(delegate
                        {
                            localState.CopyTo(config);
                        });
                    }
                    onAfterApply.Add(delegate
                    {
                        m.OnConfigChanged();
                        m.SaveConfigsToDisk();
                    });
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            
            // Create buttons at bottom (outside scroll area)
            RefValue<bool> isDirty = new RefValue<bool>(v: false);
            HorizontalLayoutGroup buttonsGroup = DewGUI.CreateHorizontalLayoutGroup(contentParent, TextAnchor.MiddleCenter);
            buttonsGroup.spacing = 30f;
            buttonsGroup.padding.top = 35;
            
            Button backButton = UnityEngine.Object.Instantiate(DewGUI.widgetButton, buttonsGroup.transform).SetTextLocalized("Generic_Back");
            UI_BackButtonHandler backHandler = backButton.gameObject.AddComponent<UI_BackButtonHandler>();
            backHandler.priority = 200;
            backHandler.enabled = false;
            backHandler.enabled = true;
            
            DewGUI.CreateHorizontalFlexibleSpace(buttonsGroup.transform);
            
            Button resetButton = UnityEngine.Object.Instantiate(DewGUI.widgetButton, buttonsGroup.transform).SetTextLocalized("Settings_ResetToDefaults");
            Button revertButton = UnityEngine.Object.Instantiate(DewGUI.widgetButton, buttonsGroup.transform).SetTextLocalized("Settings_Undo");
            Button applyButton = UnityEngine.Object.Instantiate(DewGUI.widgetButton, buttonsGroup.transform).SetTextLocalized("Settings_Apply");
            
            // Create update action for button states (must be declared before use)
            Action updateButtonStates = delegate
            {
                revertButton.interactable = isDirty;
                applyButton.interactable = isDirty;
            };
            
            resetButton.onClick.AddListener(delegate
            {
                onReset.Invoke();
                isDirty.value = true;
                updateButtonStates();
            });
            
            revertButton.onClick.AddListener(delegate
            {
                onRevert.Invoke();
                isDirty.value = false;
                updateButtonStates();
            });
            
            applyButton.onClick.AddListener(delegate
            {
                onBeforeApply.Invoke();
                isDirty.value = false;
                updateButtonStates();
                onAfterApply.Invoke();
            });
            
            backButton.onClick.AddListener(delegate
            {
                if ((bool)isDirty)
                {
                    ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
                    {
                        rawContent = DewLocalization.GetUIValue("Settings_ConfirmUnsavedChanges"),
                        buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
                        defaultButton = DewMessageSettings.ButtonType.Cancel,
                        onClose = delegate(DewMessageSettings.ButtonType b)
                        {
                            if (b == DewMessageSettings.ButtonType.Yes)
                            {
                                UnityEngine.Object.Destroy(newWindow.gameObject);
                            }
                        }
                    });
                }
                else
                {
                    UnityEngine.Object.Destroy(newWindow.gameObject);
                }
            });
            
            onDirty.Add(delegate
            {
                isDirty.value = true;
                updateButtonStates();
            });
            
            updateButtonStates();
            
            // Skip original method
            return false;
        }
        catch (Exception e)
        {
            RPGLog.Warning(" ConfigWindowScrollPatch failed, falling back to original: " + e.Message);
            // Return true to run original method if our patch fails
            return true;
        }
    }
}

/// <summary>
/// Harmony patch to intercept SendPingPC and handle dropped item sharing
/// </summary>
public static class SendPingPCPatch
{
    /// <summary>
    /// Prefix that checks if we should share a dropped item instead of sending a normal ping
    /// </summary>
    /// <param name="__instance">The ControlManager instance (required for instance method patches)</param>
    public static bool Prefix(ControlManager __instance)
    {
        try
        {
            // Check if there's a dropped item in range that we can share
            if (DroppedItem.TryShareNearestItem())
            {
                // We handled the ping as an item share, skip the original method
                return false;
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" SendPingPCPatch error: " + e.Message);
        }
        
        // Let the original method run for normal pings
        return true;
    }
}

/// <summary>
/// Harmony patch to intercept UIWindowManager.Update and block ESC when mod windows are open
/// </summary>
public static class UIWindowManagerPatch
{
    /// <summary>
    /// Prefix for UIWindowManager.Update - this is kept as a fallback but main ESC handling
    /// is now done via GlobalUIManager.AddBackHandler which is the proper game integration.
    /// </summary>
    public static bool Prefix(UIWindowManager __instance)
    {
        // ESC is now handled via GlobalUIManager.AddBackHandler
        // This patch is kept as a fallback/no-op
        return true;
    }
}

