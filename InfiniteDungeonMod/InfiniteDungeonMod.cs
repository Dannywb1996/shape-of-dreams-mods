using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteDungeonMod
{
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
    /// Configuration class for the Infinite Dungeon Mod.
    /// These settings appear in the mod loader's configuration menu.
    /// Uses collapsible sections for better organization.
    /// </summary>
    [Serializable]
    public class InfiniteDungeonConfig : ModConfig
    {
        // Track which sections are expanded (persists during session)
        [NonSerialized] private static Dictionary<string, bool> _sectionStates = new Dictionary<string, bool>();
        
        // ==================== MONSTER SPEED ====================
        [ConfigSection("▶ MONSTER SPEED / 怪物速度", false)]
        [HideInInspector] public bool _sectionSpeed;
        
        [ModConfig.LabelText("Move Speed Cap (%) / 移速上限")]
        [ModConfig.Description("Max how fast monsters can run. 150 = 1.5x speed. 怪物最快能跑多快，150=1.5倍速。")]
        public int moveSpeedCap = 150;
        
        [ModConfig.LabelText("Attack Speed Cap (%) / 攻速上限")]
        [ModConfig.Description("Max how fast monsters can attack. 200 = 2x attack speed. 怪物最快能打多快，200=2倍攻速。")]
        public int attackSpeedCap = 200;
        
        [ModConfig.LabelText("Move Speed Start Depth / 移速开始深度")]
        [ModConfig.Description("When do monsters start running faster? Default: depth 10. 从第几层开始跑更快？默认第10层。")]
        public int moveSpeedStartDepth = 10;
        
        [ModConfig.LabelText("Attack Speed Start Depth / 攻速开始深度")]
        [ModConfig.Description("When do monsters start attacking faster? Default: depth 20. 从第几层开始打更快？默认第20层。")]
        public int attackSpeedStartDepth = 20;
        
        [ModConfig.LabelText("Move Speed Per Depth (%) / 每深度移速")]
        [ModConfig.Description("How much faster monsters run per depth. 2.0 = +2% per floor. 每层怪物快多少，2.0=每层+2%。")]
        public float moveSpeedPerDepth = 2.0f;
        
        [ModConfig.LabelText("Attack Speed Per Depth (%) / 每深度攻速")]
        [ModConfig.Description("How much faster monsters attack per depth. 2.5 = +2.5% per floor. 每层怪物攻速快多少，2.5=每层+2.5%。")]
        public float attackSpeedPerDepth = 2.5f;
        
        // ==================== MONSTER STATS ====================
        [ConfigSection("▶ MONSTER STATS / 怪物属性", false)]
        [HideInInspector] public bool _sectionStats;
        
        [ModConfig.LabelText("Damage Multiplier / 伤害倍率")]
        [ModConfig.Description("How hard monsters hit. 1.0 = normal, 2.0 = twice as hard. 怪物打你多疼，1.0正常，2.0双倍疼。")]
        public float monsterDamageMultiplier = 1.0f;
        
        [ModConfig.LabelText("Damage Per Depth (%) / 每深度伤害")]
        [ModConfig.Description("Monsters hit harder as you go deeper. Higher = scarier deep floors! 越深怪物越疼，数字越大后期越难！")]
        public float damagePerDepth = 3.0f;
        
        [ModConfig.LabelText("Damage Start Depth / 伤害开始深度")]
        [ModConfig.Description("When do monsters start hitting harder? Default: depth 1. 从第几层开始变疼？默认第1层。")]
        public int damageStartDepth = 1;
        
        [ModConfig.LabelText("Damage Late Game Boost / 后期伤害加速")]
        [ModConfig.Description("Makes deep floors MUCH harder! 1.0 = same growth all floors, 1.5 = late floors way harder, 2.0 = crazy hard! 让深层更难！1.0=每层一样，1.5=后期难很多，2.0=超级难！")]
        public float damageExponent = 1.2f;
        
        [ModConfig.LabelText("Health Multiplier / 血量倍率")]
        [ModConfig.Description("How tanky monsters are. 1.0 = normal, 2.0 = twice the health. 怪物多肉，1.0正常，2.0双倍血。")]
        public float monsterHealthMultiplier = 1.0f;
        
        [ModConfig.LabelText("Health Per Depth (%) / 每深度血量")]
        [ModConfig.Description("Monsters get tankier as you go deeper. Higher = beefier deep floors! 越深怪物越肉，数字越大后期越耐打！")]
        public float healthPerDepth = 4.0f;
        
        [ModConfig.LabelText("Health Start Depth / 血量开始深度")]
        [ModConfig.Description("When do monsters start getting tankier? Default: depth 3. 从第几层开始变肉？默认第3层。")]
        public int healthStartDepth = 3;
        
        [ModConfig.LabelText("Health Late Game Boost / 后期血量加速")]
        [ModConfig.Description("Makes deep floor monsters MUCH tankier! 1.0 = same growth all floors, 1.5 = late floors way tankier, 2.0 = super tanky! 让深层怪物更肉！1.0=每层一样，1.5=后期肉很多，2.0=超级肉！")]
        public float healthExponent = 1.3f;
        
        [ModConfig.LabelText("Armor Per Depth / 每深度护甲")]
        [ModConfig.Description("Monsters get more armor as you go deeper. More armor = harder to kill! 越深怪物护甲越高，护甲高=更难打死！")]
        public float armorPerDepth = 1.5f;
        
        [ModConfig.LabelText("Armor Start Depth / 护甲开始深度")]
        [ModConfig.Description("When do monsters start getting armor? Default: depth 5. 从第几层开始有护甲？默认第5层。")]
        public int armorStartDepth = 5;
        
        [ModConfig.LabelText("Armor Late Game Boost / 后期护甲加速")]
        [ModConfig.Description("Makes deep floor monsters have MORE armor! 1.0 = same growth all floors, 1.5 = late floors way more armor! 让深层怪物护甲更高！1.0=每层一样，1.5=后期护甲多很多！")]
        public float armorExponent = 1.1f;
        
        [ModConfig.LabelText("Armor On/Off / 护甲开关")]
        [ModConfig.Description("Turn armor scaling on or off. 1.0 = on, 0 = off (no armor). 护甲缩放开关，1.0开，0关（无护甲）。")]
        public float armorScaling = 1.0f;
        
        // ==================== MONSTER POPULATION ====================
        [ConfigSection("▶ POPULATION / 怪物数量", false)]
        [HideInInspector] public bool _sectionPopulation;
        
        [ModConfig.LabelText("Population Cap (x) / 数量上限")]
        [ModConfig.Description("Max monster population multiplier. Default: 3.0 (3x). 怪物数量上限倍率，默认3.0（3倍）。")]
        public float populationCap = 3.0f;
        
        [ModConfig.LabelText("Population Per Depth (%) / 每深度数量")]
        [ModConfig.Description("Population increase % per depth. Default: 2.0. 每深度怪物数量增加%，默认2.0。")]
        public float populationPerDepth = 2.0f;
        
        [ModConfig.LabelText("Population Start Depth / 数量开始深度")]
        [ModConfig.Description("Depth where population scaling starts. Default: 5. 怪物数量加成开始深度，默认5。")]
        public int populationStartDepth = 5;
        
        // ==================== EXP SYSTEM ====================
        [ConfigSection("▶ EXP SYSTEM / 经验系统", false)]
        [HideInInspector] public bool _sectionExp;
        
        [ModConfig.LabelText("EXP Multiplier / 经验倍率")]
        [ModConfig.Description("Monster EXP drop multiplier. 1.0 = normal. 怪物经验倍率，1.0正常。")]
        public float expMultiplier = 1.0f;
        
        // ==================== HUNTER SYSTEM ====================
        [ConfigSection("▶ HUNTER SYSTEM / 猎人系统", false)]
        [HideInInspector] public bool _sectionHunter;
        
        [ModConfig.LabelText("Infection Chance (%) / 感染几率")]
        [ModConfig.Description("Chance to get infected when entering infected node. Default: 35. 进入感染节点时被感染的几率，默认35。")]
        public int infectionChance = 35;
        
        [ModConfig.LabelText("Node Infection Chance (%) / 节点感染几率")]
        [ModConfig.Description("Chance for adjacent nodes to become infected. Default: 8. 相邻节点被感染的几率，默认8。")]
        public int nodeInfectionChance = 8;
        
        [ModConfig.LabelText("Min Curse Charges / 最小诅咒层数")]
        [ModConfig.Description("Minimum curse duration (charges). Default: 1. 最小诅咒持续时间（层数），默认1。")]
        public int minCurseCharges = 1;
        
        [ModConfig.LabelText("Max Curse Charges / 最大诅咒层数")]
        [ModConfig.Description("Maximum curse duration (charges). Default: 5. 最大诅咒持续时间（层数），默认5。")]
        public int maxCurseCharges = 5;
        
        [ModConfig.LabelText("Min Player Infection / 最小感染层数")]
        [ModConfig.Description("Min nodes player can infect when infected. Default: 1. 玩家感染时最少可感染节点数，默认1。")]
        public int minPlayerInfectionCharges = 1;
        
        [ModConfig.LabelText("Max Player Infection / 最大感染层数")]
        [ModConfig.Description("Max nodes player can infect when infected. Default: 5. 玩家感染时最多可感染节点数，默认5。")]
        public int maxPlayerInfectionCharges = 5;
        
        [ModConfig.LabelText("Obliviax Trigger Chance (%) / 遗忘者触发几率")]
        [ModConfig.Description("Chance to trigger Obliviax when entering hunted node. Default: 5. 进入猎杀节点时触发遗忘者的几率，默认5。")]
        public int obliviaxTriggerChance = 5;
        
        // ==================== WEAKENED BOSS ====================
        [ConfigSection("▶ WEAKENED BOSS / 弱化首领", false)]
        [HideInInspector] public bool _sectionWeakenedBoss;
        
        [ModConfig.LabelText("Spawn Chance (%) / 生成几率")]
        [ModConfig.Description("Chance for weakened boss per new combat node. Default: 3. 每个新战斗节点弱化首领生成几率，默认3。")]
        public int weakenedBossChance = 3;
        
        [ModConfig.LabelText("Size Scale (%) / 体型缩放")]
        [ModConfig.Description("Weakened boss size as % of normal. Default: 70. 弱化首领体型百分比，默认70。")]
        public int weakenedBossScale = 70;
        
        [ModConfig.LabelText("Health (%) / 血量")]
        [ModConfig.Description("Weakened boss health as % of normal. Default: 40. 弱化首领血量百分比，默认40。")]
        public int weakenedBossHealth = 40;
        
        [ModConfig.LabelText("Damage (%) / 伤害")]
        [ModConfig.Description("Weakened boss damage as % of normal. Default: 55. 弱化首领伤害百分比，默认55。")]
        public int weakenedBossDamage = 55;
        
        // ==================== DUNGEON GENERATION ====================
        [ConfigSection("▶ DUNGEON LAYOUT / 地牢布局", false)]
        [HideInInspector] public bool _sectionDungeon;
        
        [ModConfig.LabelText("Min Branches / 最小分支数")]
        [ModConfig.Description("Minimum branches per node. Default: 2. 每节点最小分支数，默认2。")]
        public int minBranches = 2;
        
        [ModConfig.LabelText("Max Branches / 最大分支数")]
        [ModConfig.Description("Maximum branches per node. Default: 4. 每节点最大分支数，默认4。")]
        public int maxBranches = 4;
        
        [ModConfig.LabelText("Min Nodes Per Branch / 最小链长")]
        [ModConfig.Description("Minimum nodes per branch chain. Default: 1. 每分支链最小节点数，默认1。")]
        public int minNodesPerBranch = 1;
        
        [ModConfig.LabelText("Max Nodes Per Branch / 最大链长")]
        [ModConfig.Description("Maximum nodes per branch chain. Default: 3. 每分支链最大节点数，默认3。")]
        public int maxNodesPerBranch = 3;
        
        [ModConfig.LabelText("Loop Chance (%) / 环路几率")]
        [ModConfig.Description("Chance to create shortcut loops. Default: 15. 创建捷径环路的几率，默认15。")]
        public int loopChance = 15;
        
        [ModConfig.LabelText("Chain Chance (%) / 链条几率")]
        [ModConfig.Description("Chance to create node chains. Default: 40. 创建节点链的几率，默认40。")]
        public int chainChance = 40;
        
        [ModConfig.LabelText("Dead End Chance (%) / 死路几率")]
        [ModConfig.Description("Chance for dead end nodes. Default: 8. 死路节点的几率，默认8。")]
        public int deadEndChance = 8;
        
        // ==================== NODE TYPE CHANCES ====================
        [ConfigSection("▶ NODE TYPES / 节点类型", false)]
        [HideInInspector] public bool _sectionNodeTypes;
        
        [ModConfig.LabelText("Combat Chance (%) / 战斗几率")]
        [ModConfig.Description("Chance for Combat nodes. Default: 67. 战斗节点几率，默认67。")]
        public int combatNodeChance = 67;
        
        [ModConfig.LabelText("Merchant Chance (%) / 商人几率")]
        [ModConfig.Description("Chance for Merchant nodes. Default: 15. 商人节点几率，默认15。")]
        public int merchantNodeChance = 15;
        
        [ModConfig.LabelText("Event Chance (%) / 事件几率")]
        [ModConfig.Description("Chance for Event nodes. Default: 15. 事件节点几率，默认15。")]
        public int eventNodeChance = 15;
        
        [ModConfig.LabelText("Boss Chance (%) / Boss几率")]
        [ModConfig.Description("Chance for Boss nodes. Default: 3. Boss节点几率，默认3。")]
        public int bossNodeChance = 3;
        
        // ==================== ROOM MODIFIERS ====================
        [ConfigSection("▶ ROOM MODIFIERS / 房间修饰", false)]
        [HideInInspector] public bool _sectionModifiers;
        
        [ModConfig.LabelText("Plain Room Weight / 空房权重")]
        [ModConfig.Description("Weight for no modifiers. Higher = more empty rooms. Default: 50. 无修饰权重，越高空房越多，默认50。")]
        public int plainRoomChance = 50;
        
        [ModConfig.LabelText("Well Weight / 升级井权重")]
        [ModConfig.Description("Weight for Upgrade Well. Set 20 = roughly 1 in 5 rooms. Default: 15. 升级井权重，20≈每5房1个，默认15。")]
        public int wellChance = 15;
        
        [ModConfig.LabelText("MiniBoss Weight / 小Boss权重")]
        [ModConfig.Description("Weight for MiniBoss spawn. Set 20 = roughly 1 in 5 rooms. Default: 20. 小Boss权重，20≈每5房1个，默认20。")]
        public int miniBossChance = 20;
        
        [ModConfig.LabelText("Hidden Stash Weight / 宝箱权重")]
        [ModConfig.Description("Weight for treasure chests. Set 20 = roughly 1 in 5 rooms. Default: 15. 宝箱权重，20≈每5房1个，默认15。")]
        public int hiddenStashChance = 15;
        
        [ModConfig.LabelText("Other Modifier Weight / 其他修饰权重")]
        [ModConfig.Description("Weight for each other modifier (gold, stardust, etc). Default: 5. 其他修饰权重，默认5。")]
        public int otherModifierChance = 5;
        
        // ==================== PURE WHITE RIFT ====================
        [ConfigSection("▶ PURE WHITE RIFT / 纯白裂隙", false)]
        [HideInInspector] public bool _sectionPureWhite;
        
        [ModConfig.LabelText("Min Depth / 最小深度")]
        [ModConfig.Description("Minimum depth to spawn pure white rift after boss. Default: 50. 首领后生成纯白裂隙的最小深度，默认50。")]
        public int pureWhiteRiftMinDepth = 50;
        
        // ==================== PLATINUM COIN ====================
        [ConfigSection("▶ PLATINUM COIN / 白金币", false)]
        [HideInInspector] public bool _sectionPlatinum;
        
        [ModConfig.LabelText("MiniBoss Platinum Chance (%) / 小Boss白金币几率")]
        [ModConfig.Description("Chance for MiniBoss to drop Platinum Coin. Default: 15. 小Boss掉落白金币的几率，默认15。")]
        public float miniBossPlatinumChance = 15f;
        
        [ModConfig.LabelText("Boss Platinum Chance (%) / Boss白金币几率")]
        [ModConfig.Description("Chance for Boss to drop Platinum Coin. Default: 50. Boss掉落白金币的几率，默认50。")]
        public float bossPlatinumChance = 50f;
        
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
                        // Get UI scale once for this section
                        float configUIScale = InfiniteDungeonMod.GetUIScale();
                        
                        TextMeshProUGUI headerText = headerButton.GetComponentInChildren<TextMeshProUGUI>();
                        if (headerText != null)
                        {
                            // Scale font size based on UI scale
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
                    
                    // Add description if present
                    ModConfig.DescriptionAttribute descAttr = f.GetCustomAttribute<ModConfig.DescriptionAttribute>();
                    if (descAttr != null)
                    {
                        TextMeshProUGUI descText = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, widgetParent);
                        descText.SetText(descAttr.text);
                        descText.color = Color.Lerp(descText.color, Color.white, 0.5f);
                        descText.margin = descText.margin.WithY(-15f).WithW(10f);
                        descText.fontSize *= 0.85f;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[InfiniteDungeon] Failed to create widget for field " + f.Name + ": " + ex.Message);
                }
            }
            
            _requestUpdate.Invoke();
        }
    }
    
    // Main mod class - split into partial classes for organization
    /// - No zone advancement or looping - endless exploration!
    /// </summary>
    public partial class InfiniteDungeonMod : ModBehaviour
    {
        // Public config field - shown in mod loader settings menu
        public InfiniteDungeonConfig config = new InfiniteDungeonConfig();
        
        // Mod configuration
        public static bool IsModActive = false;
        public static InfiniteDungeonMod Instance = null;
        
        // ==================== UI SCALING ====================
        
        /// <summary>
        /// Get the UI scale factor combining game settings and screen resolution.
        /// Respects game's UI scale setting from options menu.
        /// Also applies screen height factor for proper resolution support (reference: 1440p).
        /// </summary>
        public static float GetUIScale()
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
        
        // TRUE only when in an actual game session (not main menu)
        // Use this for patches that should only run during gameplay
        public static bool IsInGameSession { get { return IsModActive && NetworkedManagerBase<GameManager>.softInstance != null; } }
        
        // Dungeon state - depth only increases on NEW node visits
        private static int _totalDepth = 0;
        private static List<Zone> _allZones = new List<Zone>();
        private static DewRandom _dungeonRandom;
        
        // Node generation settings - SPREAD OUT DUNGEON LAYOUT
        // ==================== DEBUG MODE ====================
        // Set to true to enable detailed logging (causes FPS drops!)
        // Should be FALSE for release builds
        public const bool DEBUG_MODE = false;
        
        // These are now configurable via InfiniteDungeonConfig
        // Keeping const for non-configurable layout params
        private const float MIN_NODE_DISTANCE = 22f; // Good spacing
        private const float MAX_NODE_DISTANCE = 35f; // Not too far
        private const float NODE_SAFE_RADIUS = 18f; // Clearance from other nodes
        private const float PATH_CLEARANCE = 15f; // Clearance from paths
        private const float START_DIRECTION = 0f; // Initial direction (right)
        private const float CORRIDOR_ANGLE_VARIANCE = 60f; // Allow decent turns
        private const float MIN_ANGLE_BETWEEN_BRANCHES = 60f; // Good separation between branches
        
        // Config getters for dungeon generation
        public static int MIN_BRANCHES { get { return Instance != null ? Instance.config.minBranches : 2; } }
        public static int MAX_BRANCHES { get { return Instance != null ? Instance.config.maxBranches : 4; } }
        public static int MIN_NODES_PER_BRANCH { get { return Instance != null ? Instance.config.minNodesPerBranch : 1; } }
        public static int MAX_NODES_PER_BRANCH { get { return Instance != null ? Instance.config.maxNodesPerBranch : 3; } }
        public static float LOOP_CHANCE { get { return Instance != null ? Instance.config.loopChance / 100f : 0.15f; } }
        public static float CHAIN_CHANCE { get { return Instance != null ? Instance.config.chainChance / 100f : 0.40f; } }
        public static float DEAD_END_CHANCE { get { return Instance != null ? Instance.config.deadEndChance / 100f : 0.08f; } }
        
        // Track occupied positions for collision detection
        private static HashSet<string> _occupiedGridCells = new HashSet<string>(); // Legacy, kept for compatibility
        
        // Hunter infection settings - now configurable
        public static float HUNTER_INFECTION_CHANCE { get { return Instance != null ? Instance.config.nodeInfectionChance / 100f : 0.08f; } }
        public static float OBLIVIAX_TRIGGER_CHANCE { get { return Instance != null ? Instance.config.obliviaxTriggerChance / 100f : 0.05f; } }
        
        // ========== UI SCALE SYSTEM ==========
        // Track generated nodes and their zones
        private static Dictionary<int, Zone> _nodeZoneMap = new Dictionary<int, Zone>();
        
        // Track visited nodes (for depth counting - only new visits increase depth)
        private static HashSet<int> _visitedNodes = new HashSet<int>();
        
        // Track each node's world position (Vector2)
        private static Dictionary<int, Vector2> _nodePositions = new Dictionary<int, Vector2>();
        
        // Track node connections (parent-child only, bidirectional for travel)
        // Key = node index, Value = set of connected node indices
        private static Dictionary<int, HashSet<int>> _nodeConnections = new Dictionary<int, HashSet<int>>();
        
        // Track each node's "forward direction" for branching
        private static Dictionary<int, float> _nodeDirections = new Dictionary<int, float>();
        
        // Track which nodes have already generated children
        private static HashSet<int> _nodesWithChildren = new HashSet<int>();
        
        // Track if this is a continued run (loaded from save with existing map)
        private static bool _isContinuedRun = false;
        
        // Track if the dungeon has been initialized this session (prevents re-init on lobby return)
        private static bool _dungeonInitialized = false;
        
        // Flag to track when traveling to Zone_Primus (final boss zone) - mod should not interfere
        private static bool _isTravelingToPrimus = false;
        public static bool IsTravelingToPrimus() { return _isTravelingToPrimus; }
        public static void SetTravelingToPrimus(bool value) { _isTravelingToPrimus = value; }
        
        // Flag to allow our own modifier additions (blocks game's default modifier system)
        private static bool _allowModifierAddition = false;
        
        // Track modifier IDs that belong to our mod (so we can reset them properly)
        private static HashSet<int> _ourModifierIds = new HashSet<int>();
        
        // Track if welcome message has been shown this session
        private static bool _welcomeMessageShown = false;
        
        // Track last announced depth (to avoid spam)
        private static int _lastAnnouncedDepth = -1;
        
        // Introduction UI state
        private static bool _introShownThisSession = false;
        private static UI_Window _introWindow = null;
        
        // Custom Hunter infection tracking - we manage this ourselves instead of relying on the game
        private static HashSet<int> _hunterInfectedNodes = new HashSet<int>();
        
        // Player infection system - when player enters infected node, they have a chance to become a carrier
        private static bool _playerIsInfected = false;
        private static int _playerInfectionCharges = 0; // Number of nodes the player can still infect
        
        // Configurable player infection charges
        public static int MIN_PLAYER_INFECTION_CHARGES { get { return Instance != null ? Instance.config.minPlayerInfectionCharges : 1; } }
        public static int MAX_PLAYER_INFECTION_CHARGES { get { return Instance != null ? Instance.config.maxPlayerInfectionCharges : 5; } }
        
        // Track nodes the player infected THIS SESSION to prevent re-infection on revisit
        // When player spreads their last charge to a node, they shouldn't get re-infected
        // when the game does a revisit call to that same node
        private static HashSet<int> _playerInfectedNodesThisSession = new HashSet<int>();
        
        // Pot of Greed tracking - players who used it get a miniboss in the next combat room
        private static HashSet<string> _potOfGreedPlayers = new HashSet<string>();
        private static bool _pendingMiniBossSpawn = false;
        
        // ============================================================
        // WEAKENED BOSS SYSTEM - Real bosses in their proper rooms
        // Bosses spawn in their actual boss rooms with all abilities working
        // ============================================================
        // Configurable weakened boss settings
        public static float WEAKENED_BOSS_CHANCE { get { return Instance != null ? Instance.config.weakenedBossChance / 100f : 0.03f; } }
        public static float WEAKENED_BOSS_SCALE { get { return Instance != null ? Instance.config.weakenedBossScale / 100f : 0.70f; } }
        public static float WEAKENED_BOSS_HEALTH_MULT { get { return Instance != null ? Instance.config.weakenedBossHealth / 100f : 0.40f; } }
        public static float WEAKENED_BOSS_DAMAGE_MULT { get { return Instance != null ? Instance.config.weakenedBossDamage / 100f : 0.55f; } }
        
        // Boss data: Monster name -> Boss room name
        // Each boss spawns in their actual boss room so all abilities work correctly
        private static readonly Dictionary<string, string> BossToRoomMap = new Dictionary<string, string>
        {
            { "Mon_Primus_BossPrimusAeron", "Room_Primus_Boss_0" },
            { "Mon_Forest_BossDemon", "Room_Forest_Boss_0" },
            { "Mon_DarkCave_BossSeeker", "Room_DarkCave_Boss_0" },
            { "Mon_Despair_BossAzurak", "Room_Despair_Boss_0" },
            { "Mon_Ink_BossWhiteNight", "Room_Ink_Boss_0" },
            { "Mon_Ink_BossDarkMoon", "Room_Ink_Boss_0" },
            { "Mon_LavaLand_BossInfernus", "Room_LavaLand_Boss_0" },
            { "Mon_Sky_BossNyx", "Room_Sky_Boss_0" },
            { "Mon_SnowMountain_BossSkoll", "Room_SnowMountain_Boss_0" },
        };
        
        // List of bosses for random selection
        private static readonly string[] WeakenedBosses = new string[]
        {
            // Only bosses that work in normal combat rooms (no complex arena mechanics)
            "Mon_Forest_BossDemon",           // Forest Demon - works fine
            "Mon_Sky_BossNyx",                // Nyx - works fine
            "Mon_SnowMountain_BossSkoll",     // Skoll - works fine
            // Excluded: Primus/Aeron (first boss), Seeker, Azurak, WhiteNight, DarkMoon, Infernus (need arena)
        };
        
        // Track nodes that have been converted to boss encounters
        private static HashSet<int> _weakenedBossNodes = new HashSet<int>();
        
        // Track pending weakened boss spawn (boss name to spawn after room reload)
        private static string _pendingWeakenedBossName = null;
        
        // Lore-friendly prompts for weakened boss encounters - CLEARLY state boss is WEAKENED with reduced stats
        // Only includes bosses that work in normal combat rooms (no arena mechanics needed)
        private static readonly Dictionary<string, string[]> WeakenedBossPrompts = new Dictionary<string, string[]>
        {
            { "Mon_Forest_BossDemon", new string[] {
                "<color=#ff9900>* [WEAKENED BOSS] *</color>\n<color=#ffcc00>The Forest Demon emerges from the shadows!</color>\n<color=#88ff88>>>> This boss is WEAKENED! <<<</color>\n<color=#aaaaaa>Lost in the infinite dream, its power has greatly diminished. It deals less damage and has reduced health. A rare opportunity to face a legendary foe!</color>",
                "<color=#ff9900>* [虚弱首领] *</color>\n<color=#ffcc00>森林恶魔从阴影中浮现！</color>\n<color=#88ff88>>>> 此首领已被削弱！ <<<</color>\n<color=#aaaaaa>迷失在无尽梦境中，它的力量大大减弱。伤害降低，生命值减少。难得的机会面对传说中的敌人！</color>"
            }},
            { "Mon_Sky_BossNyx", new string[] {
                "<color=#ff9900>* [WEAKENED BOSS] *</color>\n<color=#ffcc00>Nyx descends from the void above!</color>\n<color=#88ff88>>>> This boss is WEAKENED! <<<</color>\n<color=#aaaaaa>Her wings broken by the fall through endless dreams. She deals less damage and has reduced health. Seize this chance to ground the nightmare!</color>",
                "<color=#ff9900>* [虚弱首领] *</color>\n<color=#ffcc00>尼克斯从虚空中降临！</color>\n<color=#88ff88>>>> 此首领已被削弱！ <<<</color>\n<color=#aaaaaa>坠落无尽梦境折断了她的双翼。伤害降低，生命值减少。抓住机会击落这噩梦！</color>"
            }},
            { "Mon_SnowMountain_BossSkoll", new string[] {
                "<color=#ff9900>* [WEAKENED BOSS] *</color>\n<color=#ffcc00>Skoll prowls into view, hunting through the dream!</color>\n<color=#88ff88>>>> This boss is WEAKENED! <<<</color>\n<color=#aaaaaa>The warmth of infinite dreams has sapped its strength. It deals less damage and has reduced health. Slay this diminished beast!</color>",
                "<color=#ff9900>* [虚弱首领] *</color>\n<color=#ffcc00>斯库尔潜行而来，在梦境中狩猎！</color>\n<color=#88ff88>>>> 此首领已被削弱！ <<<</color>\n<color=#aaaaaa>无尽梦境的温暖吸走了它的力量。伤害降低，生命值减少。斩杀这削弱的野兽！</color>"
            }},
        };
        
        // Generic prompts for unknown bosses - CLEARLY state WEAKENED
        private static readonly string[] GenericWeakenedBossPrompts = new string[]
        {
            "<color=#ff9900>* [WEAKENED BOSS] *</color>\n<color=#ffcc00>An ancient terror manifests before you!</color>\n<color=#88ff88>>>> This boss is WEAKENED! <<<</color>\n<color=#aaaaaa>Wandering through infinite dreams has sapped its power. It deals less damage and has reduced health. Destroy this diminished horror!</color>",
            "<color=#ff9900>* [虚弱首领] *</color>\n<color=#ffcc00>一个古老的恐惧在你面前显现！</color>\n<color=#88ff88>>>> 此首领已被削弱！ <<<</color>\n<color=#aaaaaa>在无尽梦境中游荡吸走了它的力量。伤害降低，生命值减少。摧毁这削弱的恐怖！</color>"
        };
        
        // ============================================================
        // HUNTER CURSES SYSTEM - Additional curses from infected nodes
        // ============================================================
        
        // Curse types that can be applied to the player
        // Categories:
        // - ENVIRONMENTAL DAMAGE: Room modifiers that spawn hazards (only hurt players)
        // - PLAYER DEBUFF: Custom stat penalties applied to players only
        // - MONSTER BUFF: Custom stat bonuses applied to monsters only
        public enum HunterCurseType
        {
            None,
            // === ENVIRONMENTAL DAMAGE (room modifiers - hurt players) ===
            MeteorFury,      // RoomMod_RiskOfMeteors - Meteors fall on players
            InkStorm,        // RoomMod_InkStrikeWarning - Ink strikes hit players
            FlameEngulf,     // RoomMod_EngulfedInFlame - Fire damage to players
            ToxicMiasma,     // RoomMod_ToxicArea - Toxic damage to players
            BlackRain,       // RoomMod_BlackRain - Black rain damage to players
            
            // === PLAYER DEBUFFS (custom stat penalties - players only) ===
            DarkVeil,        // Reduced crit/damage for players
            GravityCurse,    // Reduced movement speed for players
            WeakenedSpirit,  // Reduced max health for players
            SlowMind,        // Reduced ability haste for players
            Vulnerability,   // Reduced armor for players
            
            // === MONSTER BUFFS (custom stat bonuses - monsters only) ===
            TimewarpCurse,   // Monsters get increased speed
            MonsterRage,     // Monsters get increased damage
            IronHide,        // Monsters get increased armor
            Frenzy           // Monsters get increased attack speed
        }
        
        // Active curses on the player
        private static Dictionary<HunterCurseType, int> _playerCurses = new Dictionary<HunterCurseType, int>();
        
        // Configurable curse charges
        public static int MIN_CURSE_CHARGES { get { return Instance != null ? Instance.config.minCurseCharges : 1; } }
        public static int MAX_CURSE_CHARGES { get { return Instance != null ? Instance.config.maxCurseCharges : 5; } }
        
        // Track applied stat bonuses so we can remove them when leaving room
        private static Dictionary<Hero, List<StatBonus>> _appliedCurseDebuffs = new Dictionary<Hero, List<StatBonus>>();
        
        // Store shrine/savedObject data separately so we can restore it after clearing main save data
        private static Dictionary<int, List<DewPersistence.RoomSavedObjectData>> _preservedSavedObjects = new Dictionary<int, List<DewPersistence.RoomSavedObjectData>>();
        
        // Helper methods for accessing preserved saved objects from patch classes
        public static void StorePreservedSavedObjects(int nodeIndex, List<DewPersistence.RoomSavedObjectData> savedObjects)
        {
            _preservedSavedObjects[nodeIndex] = new List<DewPersistence.RoomSavedObjectData>(savedObjects);
        }
        
        public static List<DewPersistence.RoomSavedObjectData> GetPreservedSavedObjects(int nodeIndex)
        {
            if (_preservedSavedObjects.ContainsKey(nodeIndex))
            {
                return _preservedSavedObjects[nodeIndex];
            }
            return null;
        }
        
        public static void ClearPreservedSavedObjects(int nodeIndex)
        {
            if (_preservedSavedObjects.ContainsKey(nodeIndex))
            {
                _preservedSavedObjects.Remove(nodeIndex);
            }
        }
        
        // UI for showing infection/curse status
        private static GameObject _infectionUI = null;
        private static TextMeshProUGUI _infectionText = null;
        
        // Public setter for continued run flag (called from WorldGenerationPatch)
        public static void SetContinuedRun(bool value)
        {
            _isContinuedRun = value;
        }
        
        // Check if dungeon has been initialized
        public static bool IsDungeonInitialized()
        {
            return _dungeonInitialized;
        }
        
        // Mark dungeon as initialized
        public static void MarkDungeonInitialized()
        {
            _dungeonInitialized = true;
        }
        
        // Check if a node is infected by our custom Hunter system
        public static bool IsNodeHunterInfected(int nodeIndex)
        {
            return _hunterInfectedNodes.Contains(nodeIndex);
        }
        
        // Get all hunter infected nodes (for debugging)
        public static HashSet<int> GetHunterInfectedNodes()
        {
            return _hunterInfectedNodes;
        }
        
        /// <summary>
        /// Get the count of infected nodes (for scoring)
        /// </summary>
        public static int GetInfectedNodeCount()
        {
            return _hunterInfectedNodes.Count;
        }
        
        /// <summary>
        /// Get the total number of curses endured this run (for scoring)
        /// Tracks how many curse charges have been consumed
        /// </summary>
        public static int GetCursesEndured()
        {
            return _totalCursesEndured;
        }
        
        // Track total curses endured for scoring
        private static int _totalCursesEndured = 0;
        
        /// <summary>
        /// Increment curses endured counter (called when a curse charge is consumed)
        /// </summary>
        public static void IncrementCursesEndured()
        {
            _totalCursesEndured++;
        }
        
        /// <summary>
        /// Get the total number of nodes visited/explored
        /// </summary>
        public static int GetVisitedNodesCount()
        {
            return _visitedNodes.Count;
        }
        
        /// <summary>
        /// Get the total weakened bosses defeated
        /// </summary>
        public static int GetWeakenedBossesDefeated()
        {
            return _weakenedBossesDefeated;
        }
        
        // Track weakened bosses defeated
        private static int _weakenedBossesDefeated = 0;
        
        /// <summary>
        /// Increment weakened bosses defeated counter
        /// </summary>
        public static void IncrementWeakenedBossesDefeated()
        {
            _weakenedBossesDefeated++;
        }
        
        // ============================================================
        // CHAT MESSAGE SYSTEM
        // ============================================================
        
        /// <summary>
        /// Send a chat message to all players (broadcast)
        /// </summary>
        public static void SendChatMessage(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message)) return;
                if (NetworkedManagerBase<ChatManager>.instance == null) return;
                
                ChatManager.Message msg = new ChatManager.Message
                {
                    type = ChatManager.MessageType.Raw,
                    content = message
                };
                
                if (NetworkServer.active)
                {
                    // Server broadcasts to all
                    NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(msg);
                }
                else
                {
                    // Client shows locally
                    NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(msg);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error sending chat message: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Send a chat message only to the local player
        /// </summary>
        public static void SendLocalChatMessage(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message)) return;
                if (NetworkedManagerBase<ChatManager>.instance == null) return;
                
                ChatManager.Message msg = new ChatManager.Message
                {
                    type = ChatManager.MessageType.Raw,
                    content = message
                };
                
                NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(msg);
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error sending local chat message: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Announce dungeon events with atmospheric messages
        /// </summary>
        public static void AnnounceDungeonEvent(string zoneName, WorldNodeType nodeType, int depth, bool isNewNode, List<string> modifierNames)
        {
            try
            {
                // Welcome message (first time only) - uses message key for client-side localization
                if (!_welcomeMessageShown && depth <= 1)
                {
                    _welcomeMessageShown = true;
                    SendChatMessage(DungeonLocalization.CreateMessageKey("WELCOME"));
                }
                
                // Only announce for new nodes (not revisits)
                if (!isNewNode)
                {
                    return;
                }
                
                // Zone entry message - uses message key with zone name parameter
                if (!string.IsNullOrEmpty(zoneName))
                {
                    SendChatMessage(DungeonLocalization.CreateMessageKey("ZONE_ENTRY", zoneName));
                }
                
                // Node type specific messages - use message keys
                if (nodeType == WorldNodeType.ExitBoss)
                {
                    SendChatMessage(DungeonLocalization.CreateMessageKey("BOSS_ROOM"));
                }
                else if (nodeType == WorldNodeType.Merchant)
                {
                    SendChatMessage(DungeonLocalization.CreateMessageKey("MERCHANT_ROOM"));
                }
                else if (nodeType == WorldNodeType.Event)
                {
                    SendChatMessage(DungeonLocalization.CreateMessageKey("EVENT_ROOM"));
                }
                
                // Modifier messages (shrines, hazards, etc.) - use message keys
                if (modifierNames != null)
                {
                    foreach (string modName in modifierNames)
                    {
                        SendChatMessage(DungeonLocalization.CreateMessageKey("MOD_ANNOUNCE", modName));
                    }
                }
                
                // Depth milestone announcements - use message keys
                if (depth > _lastAnnouncedDepth)
                {
                    _lastAnnouncedDepth = depth;
                    SendChatMessage(DungeonLocalization.CreateMessageKey("MILESTONE", depth.ToString()));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error announcing dungeon event: " + ex.Message);
            }
        }
        
        // Harmony instance
        private Harmony _harmony;
        private static bool _harmonyPatchesApplied = false;
        
        private void Awake()
        {
            Debug.Log("[InfiniteDungeon] Infinite Dungeon Mode by Chino loaded!");
            
            // CRITICAL: Check if the mod instance is properly initialized
            // If not, the mod was likely disabled but the DLL is still loaded
            if (instance == null || mod == null)
            {
                Debug.LogWarning("[InfiniteDungeon] Mod instance not properly initialized - mod may be disabled. Skipping initialization.");
                IsModActive = false;
                return;
            }
            
            // Mark as gameplay-altering mod (required for server-side mods)
            instance.isAlteringGameplay = true;
            
            IsModActive = true;
            Instance = this;
            
            // Load config from disk
            LoadConfigsToDisk();
            
            // Initialize localization system - set mod path first (for workshop support), then detect language
            if (mod != null && !string.IsNullOrEmpty(mod.path))
            {
                DungeonLocalization.SetModPath(mod.path);
            }
            DungeonLocalization.DetectLanguage();
            
            // DELAY Harmony patches until game session starts to avoid FPS drops in main menu
            // Harmony intercepts method calls even if patches bail out early, causing overhead
            _harmony = new Harmony(mod.metadata.id);
            // NOTE: PatchAll() is now called in OnZoneManagerStartClient
            
            // Register for ZoneManager lifecycle - this ensures we initialize when a game session starts
            // This works for BOTH host and client!
            CallOnNetworkedManager<ZoneManager>(
                OnZoneManagerStartClient,
                OnZoneManagerStopClient
            );
            
            Debug.Log("[InfiniteDungeon] Mod loaded. Patches will be applied when game starts.");
        }
        
        /// <summary>
        /// Called when ZoneManager starts (game session begins) - works for BOTH host and client
        /// </summary>
        private void OnZoneManagerStartClient()
        {
            // Apply Harmony patches on first game session start (delayed from Awake to avoid menu FPS drops)
            if (!_harmonyPatchesApplied && _harmony != null)
            {
                _harmony.PatchAll();
                _harmonyPatchesApplied = true;
                Debug.Log("[InfiniteDungeon] Harmony patches applied.");
            }
            
            // Reset intro shown flag for this NEW game session
            _introShownThisSession = false;
            
            // Start initialization coroutine
            StartCoroutine(InitializeModForSession());
        }
        
        /// <summary>
        /// Called when ZoneManager stops (game session ends)
        /// </summary>
        private void OnZoneManagerStopClient()
        {
            // ResetDungeonState is already called via Update() when GameManager.softInstance becomes null
        }
        
        /// <summary>
        /// Called when mod config is changed in the mod loader settings
        /// </summary>
        public override void OnConfigChanged()
        {
            // Re-detect language in case game language changed
            DungeonLocalization.DetectLanguage();
            
            // Refresh intro window if it's open
            foreach (var callback in _introRefreshCallbacks)
            {
                try
                {
                    if (callback != null)
                        callback.Invoke();
                }
                catch { }
            }
        }
        
        private void Start()
        {
            // NOTE: We no longer start InitializeMod here!
            // Instead, we use CallOnNetworkedManager<ZoneManager> which properly detects
            // when a game session starts for BOTH host and client
        }
        
        // Track if we've applied late patches
        private static bool _appliedLatePatches = false;
        
        /// <summary>
        /// Apply patches that require types from Assembly-CSharp which may not be available during Awake()
        /// Called from InitializeMod when game is actually running
        /// NOTE: Only the HOST needs to apply these patches - clients don't need them
        /// </summary>
        private void ApplyLatePatches()
        {
            // Only apply late patches on the server/host
            // Clients don't need these patches and may have Harmony version mismatches
            if (!NetworkServer.active)
            {
                return;
            }
            
            if (_appliedLatePatches) return;
            _appliedLatePatches = true;
            
            // Start coroutine to destroy GameMod_Obliviax after GameManager spawns it
            // GameMods are spawned on OnStartServer of GameManager, so we need to wait a bit
            StartCoroutine(DestroyGameModObliviaxCoroutine());
        }
        
        /// <summary>
        /// Destroy the GameMod_Obliviax instance to disable the native Obliviax quest system.
        /// We have our own custom hunter system with 5% chance to teleport to Obliviax's boss room.
        /// Based on advice from Lizard Smoothie: iterate through ActorManager.instance.allActors
        /// and destroy any instances of GameMod_Obliviax after some time passes.
        /// </summary>
        private IEnumerator DestroyGameModObliviaxCoroutine()
        {
            // Wait for GameManager to spawn GameMods (they're spawned on OnStartServer)
            yield return new WaitForSeconds(2.0f);
            
            if (!NetworkServer.active) yield break;
            
            try
            {
                Type gameModObliviaxType = Type.GetType("GameMod_Obliviax, Assembly-CSharp");
                if (gameModObliviaxType == null)
                {
                    yield break;
                }
                
                ActorManager actorManager = NetworkedManagerBase<ActorManager>.instance;
                if (actorManager == null) yield break;
                
                // Iterate through all actors and destroy any GameMod_Obliviax instances
                // Use ToArray() to avoid modifying collection while iterating
                Actor[] allActors = actorManager.allActors.ToArray();
                int destroyedCount = 0;
                
                foreach (Actor actor in allActors)
                {
                    if (actor == null) continue;
                    
                    if (gameModObliviaxType.IsInstanceOfType(actor))
                    {
                        actor.Destroy();
                        destroyedCount++;
                    }
                }
                
                if (destroyedCount > 0)
                {
                    Debug.Log("[InfiniteDungeon] Disabled native Obliviax quest system (destroyed " + destroyedCount + " GameMod_Obliviax instance(s))");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error destroying GameMod_Obliviax (non-fatal): " + ex.Message);
            }
        }
        
        private void Update()
        {
            // ========================================
            // LOBBY/GAME END DETECTION
            // ========================================
            // Check if we're NOT in a game (returned to lobby or game ended)
            // GameManager.softInstance is null when not in a run
            bool isInGame = NetworkedManagerBase<GameManager>.softInstance != null;
            
            if (!isInGame && _dungeonInitialized)
            {
                // We left the game! Reset everything
                ResetDungeonState();
                return; // Don't process anything else this frame
            }
            
            // Hide infection UI when not in game
            if (!isInGame && _infectionUI != null)
            {
                _infectionUI.SetActive(false);
            }
            
            // ========================================
            // RE-SUBSCRIBE TO EVENTS
            // ========================================
            // Re-subscribe to ZoneManager events if needed (handles new game starts)
            if (_needsResubscribe && NetworkedManagerBase<ZoneManager>.instance != null)
            {
                _needsResubscribe = false;
                
                // IMPORTANT: Unsubscribe first to prevent double subscription!
                NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= OnRoomLoaded;
                NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += OnRoomLoaded;
                _isSubscribedToRoomLoaded = true;
                
                // Also ensure we have zones loaded and start node has children
                if (_allZones.Count == 0)
                {
                    LoadAllZones();
                }
                StartCoroutine(EnsureStartNodeHasAdjacent());
                
                // Mark as initialized (intro window is shown from InitializeModForSession)
                _dungeonInitialized = true;
                // NOTE: Don't call ShowIntroductionWindow() here - it's already called from InitializeModForSession
            }
        }
        
        private static bool _needsResubscribe = false;
        private static bool _isSubscribedToRoomLoaded = false;
        
        /// <summary>
        /// Initialize mod for a new game session. Called via CallOnNetworkedManager when ZoneManager starts.
        /// This works for BOTH host and client!
        /// </summary>
        private IEnumerator InitializeModForSession()
        {
            // ZoneManager should already be available since we're called from its lifecycle callback
            // But wait a frame to ensure everything is set up
            yield return null;
            
            if (NetworkedManagerBase<ZoneManager>.instance == null)
            {
                Debug.LogError("[InfiniteDungeon] ZoneManager is null even though we were called from its lifecycle!");
                // Still try to show intro window
                _dungeonInitialized = true;
                ShowIntroductionWindow();
                yield break;
            }
            
            // ========== STEP 1: Apply late patches (HOST ONLY, non-fatal) ==========
            try
            {
                ApplyLatePatches();
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Late patches failed (non-fatal): " + ex.Message);
            }
            
            // ========== STEP 2: Load zones (non-fatal) ==========
            try
            {
            LoadAllZones();
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] LoadAllZones failed (non-fatal): " + ex.Message);
            }
            
            // ========== STEP 3: Initialize random (non-fatal) ==========
            try
            {
            uint seed = NetworkedManagerBase<ZoneManager>.instance.worldSeed;
            if (seed == 0) seed = DewRandom.GetRandomSeed();
            _dungeonRandom = new DewRandom(seed);
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Random init failed (non-fatal): " + ex.Message);
                _dungeonRandom = new DewRandom(DewRandom.GetRandomSeed());
            }
            
            // ========== STEP 4: Subscribe to events (non-fatal) ==========
            try
            {
                NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= OnRoomLoaded;
            NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += OnRoomLoaded;
                _isSubscribedToRoomLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Event subscription failed (non-fatal): " + ex.Message);
            }
            
            // ========== STEP 5: Sync depth (non-fatal) ==========
            try
            {
                SyncDepthFromNodes();
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] SyncDepthFromNodes failed (non-fatal): " + ex.Message);
            }
            
            // Mark dungeon as initialized - this is used to detect when we return to lobby
            _dungeonInitialized = true;
            
            // ========== STEP 6: SHOW INTRO WINDOW (CRITICAL - must always run) ==========
            ShowIntroductionWindow();
            
            // ========== STEP 7: Generate adjacent nodes (non-fatal) ==========
            try
            {
            StartCoroutine(EnsureStartNodeHasAdjacent());
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] EnsureStartNodeHasAdjacent failed (non-fatal): " + ex.Message);
            }
            
            // ========== STEP 8: Client-only sync setup (non-fatal) ==========
            if (!NetworkServer.active)
            {
                try
                {
                    // Register callback for when synced data changes
                    RegisterConnectionSyncCallback();
                    
                    // IMPORTANT: Force immediate sync of connection data from server!
                    // Reset version to -1 to force reading the latest data
                    _lastSyncedVersion = -1;
                    UpdateConnectionCacheFromSync();
                    
                    // Start monitor coroutine as backup
                    StartCoroutine(MonitorConnectionSync());
                }
                catch (Exception ex)
                {
                    Debug.LogError("[InfiniteDungeon] Client sync setup failed (non-fatal): " + ex.Message);
                }
            }
        }
        
        // NOTE: Old MonitorMatrixSyncAndRefreshMap removed - replaced by MonitorConnectionSync
        // which uses the new persistentSyncedData API (see ConnectionSyncSystem.cs)
        
        /// <summary>
        /// Sync depth and visited nodes from ZoneManager's synced node list.
        /// This is crucial for clients joining mid-game to have the correct depth.
        /// The ZoneManager.nodes SyncList is automatically synced via Mirror.
        /// </summary>
        private void SyncDepthFromNodes()
        {
            try
            {
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null || zm.nodes.Count == 0) return;
                
                int calculatedDepth = 0;
                int visitedCount = 0;
                
                // Count visited combat/boss nodes to calculate depth
                for (int i = 0; i < zm.nodes.Count; i++)
                {
                    WorldNodeData node = zm.nodes[i];
                    
                    // Skip sidetrack nodes
                    if (node.IsSidetrackNode()) continue;
                    
                    if (node.status == WorldNodeStatus.HasVisited)
                    {
                        // Track as visited
                        if (!_visitedNodes.Contains(i))
                        {
                            _visitedNodes.Add(i);
                        }
                        visitedCount++;
                        
                        // Only combat and boss nodes increase depth
                        if (node.type == WorldNodeType.Combat || node.type == WorldNodeType.ExitBoss)
                        {
                            calculatedDepth++;
                        }
                    }
                }
                
                // Update depth if the calculated value is higher
                // (This handles clients joining mid-game)
                if (calculatedDepth > _totalDepth)
                {
                    _totalDepth = calculatedDepth;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error syncing depth from nodes: " + ex.Message);
            }
        }
        
        private IEnumerator EnsureStartNodeHasAdjacent()
        {
            // Wait a bit for everything to settle
            yield return new WaitForSeconds(1.0f);
            
            if (!NetworkServer.active) yield break;
            
            ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
            if (zm == null || zm.nodes.Count == 0) yield break;
            
            int currentNode = zm.currentNodeIndex;
            if (currentNode < 0) yield break;
            
            // IMPORTANT: Check if this is a CONTINUED RUN (loaded from save)
            // If there are more than 1 node and we didn't create them, this is a continued run
            // In that case, we should NOT generate new nodes - the save has its own map
            bool isContinuedRun = zm.nodes.Count > 1 && _nodePositions.Count == 0;
            
            if (isContinuedRun)
            {
                _isContinuedRun = true;
                
                // Import all existing nodes into our tracking system
                ImportExistingNodes(zm);
                yield break;
            }
            
            // Initialize start node if not tracked (fresh game)
            if (!_nodePositions.ContainsKey(currentNode))
            {
                _nodePositions[currentNode] = Vector2.zero;
                _nodeDirections[currentNode] = START_DIRECTION; // Start facing up-right
                if (!_nodeConnections.ContainsKey(currentNode))
                {
                    _nodeConnections[currentNode] = new HashSet<int>();
                }
            }
            
            // Check if already has children
            if (!_nodesWithChildren.Contains(currentNode))
            {
                
                // Mark as visited if not already
                if (!_visitedNodes.Contains(currentNode))
                {
                    _visitedNodes.Add(currentNode);
                    _totalDepth++;
                }
                
                yield return StartCoroutine(GenerateChildNodes(currentNode));
            }
        }
        
        /// <summary>
        /// Import existing nodes from a continued run into our tracking system
        /// </summary>
        private void ImportExistingNodes(ZoneManager zm)
        {
            try
            {
                // First pass: Import all node positions and mark visited nodes
                for (int i = 0; i < zm.nodes.Count; i++)
                {
                    WorldNodeData node = zm.nodes[i];
                    
                    // Skip sidetrack nodes
                    if (node.IsSidetrackNode()) continue;
                    
                    _nodePositions[i] = node.position;
                    _nodeDirections[i] = START_DIRECTION; // Default direction
                    
                    // Mark visited nodes
                    if (node.status == WorldNodeStatus.HasVisited)
                    {
                        _visitedNodes.Add(i);
                    }
                }
                
                // Import connections from the distance matrix
                int nodeCount = zm.nodes.Count;
                for (int i = 0; i < nodeCount; i++)
                {
                    if (zm.nodes[i].IsSidetrackNode()) continue;
                    
                    if (!_nodeConnections.ContainsKey(i))
                    {
                        _nodeConnections[i] = new HashSet<int>();
                    }
                    
                    for (int j = 0; j < nodeCount; j++)
                    {
                        if (i == j) continue;
                        if (zm.nodes[j].IsSidetrackNode()) continue;
                        
                        if (zm.IsNodeConnected(i, j))
                        {
                            _nodeConnections[i].Add(j);
                        }
                    }
                }
                
                // Second pass: Mark nodes as having children ONLY if they have connections to higher-index nodes
                // This ensures frontier nodes (unvisited nodes at the edge) can still generate children
                for (int i = 0; i < zm.nodes.Count; i++)
                {
                    if (zm.nodes[i].IsSidetrackNode()) continue;
                    
                    // A node has children if it has connections to nodes with higher indices
                    // (in our generation, children always have higher indices than parents)
                    if (_nodeConnections.ContainsKey(i))
                    {
                        bool hasChildren = false;
                        foreach (int connectedNode in _nodeConnections[i])
                        {
                            if (connectedNode > i)
                            {
                                hasChildren = true;
                                break;
                            }
                        }
                        
                        if (hasChildren)
                        {
                            _nodesWithChildren.Add(i);
                        }
                    }
                }
                
                // Set depth based on visited nodes count
                _totalDepth = _visitedNodes.Count;
                
                // Restore hunter infected nodes from game's hunterStatuses
                // IMPORTANT: Merchant, Start, and Boss nodes should NEVER be infected!
                _hunterInfectedNodes.Clear();
                for (int i = 0; i < zm.hunterStatuses.Count && i < zm.nodes.Count; i++)
                {
                    if (zm.hunterStatuses[i] != HunterStatus.None)
                    {
                        // Check if this is a safe/special node - if so, clear the infection!
                        WorldNodeType nodeType = zm.nodes[i].type;
                        if (nodeType == WorldNodeType.Start || 
                            nodeType == WorldNodeType.Merchant ||
                            nodeType == WorldNodeType.ExitBoss)
                        {
                            // Clear the infection from safe/boss node
                            zm.hunterStatuses[i] = HunterStatus.None;
                            continue;
                        }
                        
                        _hunterInfectedNodes.Add(i);
                    }
                }
                
                // Sync connections to clients (for continued runs)
                if (NetworkServer.active)
                {
                    SyncConnectionsToClients();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error importing existing nodes: " + ex.Message);
            }
        }
        
        private void LoadAllZones()
        {
            try
            {
                // Get all zones in the game
                Zone[] zones = DewResources.FindAllByNameSubstring<Zone>("Zone_").ToArray();
                zones = DewBuildProfile.current.content.FilterZones(zones);
                
                _allZones.Clear();
                foreach (Zone z in zones)
                {
                    // Skip tutorial zones
                    if (z.name.Contains("Tutorial")) continue;
                    
                    // Skip Primus - it's the final zone that ends the game when boss is defeated
                    if (z.name == "Zone_Primus") continue;
                    
                    _allZones.Add(z);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error loading zones: " + ex.Message);
            }
        }
        
        // ============================================================
        // INTRODUCTION UI SYSTEM
        // ============================================================
        
        /// <summary>
        /// Check if EssenceNerfMod is currently loaded
        /// </summary>
        private static bool IsEssenceNerfModLoaded()
        {
            try
            {
                // Check DewMod.loadedInstances for EssenceNerfMod
                foreach (var instance in DewMod.loadedInstances)
                {
                    if (instance.mod != null && instance.mod.metadata != null)
                    {
                        string modId = instance.mod.metadata.id;
                        if (modId != null && (modId.ToLower().Contains("essencenerf") || modId.ToLower().Contains("essence_nerf")))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[InfiniteDungeon] Error checking for EssenceNerfMod: " + ex.Message);
            }
            return false;
        }
        
        /// <summary>
        /// Show the introduction window (only once per game session)
        /// Shows for BOTH host and clients INDEPENDENTLY
        /// Each player must close their own window
        /// </summary>
        public void ShowIntroductionWindow()
        {
            // Must be in a game to show intro
            if (NetworkedManagerBase<GameManager>.softInstance == null)
            {
                return;
            }
            
            // Check if we should show the intro (only once per game session)
            if (_introShownThisSession)
            {
                return;
            }
            
            // Check if window already exists - destroy it first to prevent duplicates
            if (_introWindow != null)
            {
                UnityEngine.Object.Destroy(_introWindow.gameObject);
                _introWindow = null;
            }
            
            _introShownThisSession = true;
            
            // Start coroutine to show the window after UI is ready
            StartCoroutine(ShowIntroWindowDelayed());
        }
        
        private IEnumerator ShowIntroWindowDelayed()
        {
            // Wait for DewGUI to be available
            int waitCount = 0;
            while (DewGUI.canvasTransform == null)
            {
                waitCount++;
                if (waitCount > 20) // 10 seconds max wait
                {
                    Debug.LogError("[InfiniteDungeon] DewGUI.canvasTransform never became available!");
                    yield break;
                }
                yield return new WaitForSeconds(0.5f);
            }
            
            // Also wait for widgetWindow to be available
            waitCount = 0;
            while (DewGUI.widgetWindow == null)
            {
                waitCount++;
                if (waitCount > 20) // 10 seconds max wait
                {
                    Debug.LogError("[InfiniteDungeon] DewGUI.widgetWindow never became available!");
                    yield break;
                }
                yield return new WaitForSeconds(0.5f);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            try
            {
                CreateIntroWindow();
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error creating intro window: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
        
        // Store references for language switching
        private Transform _introMainLayout;
        private List<System.Action> _introRefreshCallbacks = new List<System.Action>();
        
        private void CreateIntroWindow()
        {
            // Verify DewGUI is ready
            if (DewGUI.canvasTransform == null || DewGUI.widgetWindow == null)
            {
                Debug.LogError("[InfiniteDungeon] CreateIntroWindow: DewGUI not ready!");
                return;
            }
            
            // Language is automatically detected from game settings
            
            // Check if EssenceNerfMod is loaded
            bool essenceNerfActive = IsEssenceNerfModLoaded();
            
            // ========== UI SIZING ==========
            // All sizes are in "reference pixels" for 1440p
            // The game's CanvasScaler automatically scales everything based on:
            // - Screen resolution (height / 1440)
            // - User's UI scale setting
            // Use a WIDER window so content doesn't need as many lines
            
            float windowWidth = 1000f; // Wide window = less text wrapping = shorter height
            
            // Create window using DewGUI
            _introWindow = UnityEngine.Object.Instantiate(DewGUI.widgetWindow, DewGUI.canvasTransform);
            
            if (_introWindow == null)
            {
                Debug.LogError("[InfiniteDungeon] Failed to instantiate intro window!");
                return;
            }
            _introWindow.isDraggable = false;
            _introWindow.enableBackdrop = true;
            _introWindow.SetWidth(windowWidth);
            
            // Get the window's RectTransform - center it
            RectTransform windowRect = _introWindow.GetComponent<RectTransform>();
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.anchoredPosition = Vector2.zero;
            
            // Compact padding and spacing
            int basePadding = 30;
            int topBottomPadding = 20;
            float baseSpacing = 4f; // Minimal spacing between elements
            
            // Create vertical layout for content
            VerticalLayoutGroup mainLayout = DewGUI.CreateVerticalLayoutGroup(_introWindow.transform);
            mainLayout.padding = new RectOffset(basePadding, basePadding, topBottomPadding, topBottomPadding);
            mainLayout.spacing = baseSpacing;
            mainLayout.childAlignment = TextAnchor.UpperCenter;
            _introMainLayout = mainLayout.transform;
            
            // Add LayoutElement to make it fit content
            float contentWidth = windowWidth - (basePadding * 2);
            UnityEngine.UI.LayoutElement mainLayoutElement = mainLayout.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            mainLayoutElement.preferredWidth = contentWidth;
            
            // ========== FONT SIZES ==========
            // Use prefab defaults and let canvas scaler handle resolution!
            // Only adjust title to be larger for emphasis
            // widgetTextHeader default is ~30, widgetTextBody default is ~22
            
            // Title
            TMPro.TextMeshProUGUI title = UnityEngine.Object.Instantiate(DewGUI.widgetTextHeader, mainLayout.transform);
            title.text = DungeonLocalization.IntroTitle;
            title.fontSize *= 1.1f; // Slight emphasis
            title.alignment = TMPro.TextAlignmentOptions.Center;
            title.color = new Color(1f, 0.7f, 0.3f);
            _introRefreshCallbacks.Add(() => { title.text = DungeonLocalization.IntroTitle; });
            
            // Subtitle
            TMPro.TextMeshProUGUI subtitle = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
            subtitle.text = DungeonLocalization.IntroSubtitle;
            subtitle.fontSize *= 0.85f;
            subtitle.alignment = TMPro.TextAlignmentOptions.Center;
            subtitle.color = new Color(0.8f, 0.8f, 0.9f);
            _introRefreshCallbacks.Add(() => { subtitle.text = DungeonLocalization.IntroSubtitle; });
            
            // Lore text 1
            TMPro.TextMeshProUGUI lore1 = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
            lore1.text = DungeonLocalization.IntroLore1;
            lore1.fontSize *= 0.85f;
            lore1.alignment = TMPro.TextAlignmentOptions.Center;
            lore1.fontStyle = TMPro.FontStyles.Italic;
            lore1.color = new Color(0.7f, 0.75f, 0.85f);
            _introRefreshCallbacks.Add(() => { lore1.text = DungeonLocalization.IntroLore1; });
            
            // Lore text 2
            TMPro.TextMeshProUGUI lore2 = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
            lore2.text = DungeonLocalization.IntroLore2;
            lore2.fontSize *= 0.8f;
            lore2.alignment = TMPro.TextAlignmentOptions.Center;
            lore2.color = new Color(0.65f, 0.65f, 0.7f);
            _introRefreshCallbacks.Add(() => { lore2.text = DungeonLocalization.IntroLore2; });
            
            // Lore text 3
            TMPro.TextMeshProUGUI lore3 = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
            lore3.text = DungeonLocalization.IntroLore3;
            lore3.fontSize *= 0.8f;
            lore3.alignment = TMPro.TextAlignmentOptions.Center;
            lore3.color = new Color(0.65f, 0.65f, 0.7f);
            _introRefreshCallbacks.Add(() => { lore3.text = DungeonLocalization.IntroLore3; });
            
            // Small spacer before features
            CreateSpacer(mainLayout.transform, 6f);
            
            // Features title - smaller header
            TMPro.TextMeshProUGUI featuresTitle = UnityEngine.Object.Instantiate(DewGUI.widgetTextHeader, mainLayout.transform);
            featuresTitle.text = DungeonLocalization.IntroFeatureTitle;
            featuresTitle.fontSize *= 0.8f;
            featuresTitle.alignment = TMPro.TextAlignmentOptions.Center;
            featuresTitle.color = new Color(0.6f, 0.8f, 1f);
            _introRefreshCallbacks.Add(() => { featuresTitle.text = DungeonLocalization.IntroFeatureTitle; });
            
            // Features list - compact text
            TMPro.TextMeshProUGUI[] featureTexts = new TMPro.TextMeshProUGUI[6];
            for (int i = 0; i < 6; i++)
            {
                featureTexts[i] = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
                featureTexts[i].fontSize *= 0.75f; // Smaller for compact list
                featureTexts[i].alignment = TMPro.TextAlignmentOptions.Left;
                featureTexts[i].color = new Color(0.75f, 0.75f, 0.8f);
            }
            // Set initial text
            featureTexts[0].text = DungeonLocalization.IntroFeature1;
            featureTexts[1].text = DungeonLocalization.IntroFeature2;
            featureTexts[2].text = DungeonLocalization.IntroFeature3;
            featureTexts[3].text = DungeonLocalization.IntroFeature4;
            featureTexts[4].text = DungeonLocalization.IntroFeature5;
            featureTexts[5].text = DungeonLocalization.IntroFeature6;
            // Add refresh callbacks
            _introRefreshCallbacks.Add(() => { featureTexts[0].text = DungeonLocalization.IntroFeature1; });
            _introRefreshCallbacks.Add(() => { featureTexts[1].text = DungeonLocalization.IntroFeature2; });
            _introRefreshCallbacks.Add(() => { featureTexts[2].text = DungeonLocalization.IntroFeature3; });
            _introRefreshCallbacks.Add(() => { featureTexts[3].text = DungeonLocalization.IntroFeature4; });
            _introRefreshCallbacks.Add(() => { featureTexts[4].text = DungeonLocalization.IntroFeature5; });
            _introRefreshCallbacks.Add(() => { featureTexts[5].text = DungeonLocalization.IntroFeature6; });
            
            // EssenceNerfMod section (only if loaded)
            TMPro.TextMeshProUGUI essenceTitle = null;
            TMPro.TextMeshProUGUI essenceDesc = null;
            if (essenceNerfActive)
            {
                essenceTitle = UnityEngine.Object.Instantiate(DewGUI.widgetTextHeader, mainLayout.transform);
                essenceTitle.text = DungeonLocalization.IntroEssenceNerfTitle;
                essenceTitle.fontSize *= 0.65f;
                essenceTitle.alignment = TMPro.TextAlignmentOptions.Center;
                essenceTitle.color = new Color(1f, 0.9f, 0.4f);
                var et = essenceTitle;
                _introRefreshCallbacks.Add(() => { et.text = DungeonLocalization.IntroEssenceNerfTitle; });
                
                essenceDesc = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
                essenceDesc.text = DungeonLocalization.IntroEssenceNerfDesc;
                essenceDesc.fontSize *= 0.75f;
                essenceDesc.alignment = TMPro.TextAlignmentOptions.Center;
                essenceDesc.color = new Color(0.7f, 0.7f, 0.6f);
                var ed = essenceDesc;
                _introRefreshCallbacks.Add(() => { ed.text = DungeonLocalization.IntroEssenceNerfDesc; });
            }
            
            // Small spacer before separator
            CreateSpacer(mainLayout.transform, 6f);
            
            // Decorative separator
            TMPro.TextMeshProUGUI separator = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
            separator.text = "~ ~ ~ ~ ~ ~ ~ ~ ~";
            separator.fontSize *= 0.7f;
            separator.alignment = TMPro.TextAlignmentOptions.Center;
            separator.color = new Color(0.4f, 0.45f, 0.55f, 0.5f);
            
            // Personal message - compact
            TMPro.TextMeshProUGUI personalMsg = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
            personalMsg.text = DungeonLocalization.IntroPersonalMessage;
            personalMsg.fontSize *= 0.75f;
            personalMsg.alignment = TMPro.TextAlignmentOptions.Center;
            personalMsg.fontStyle = TMPro.FontStyles.Italic;
            personalMsg.color = new Color(0.75f, 0.7f, 0.85f);
            _introRefreshCallbacks.Add(() => { personalMsg.text = DungeonLocalization.IntroPersonalMessage; });
            
            // Credits - signature (compact)
            TMPro.TextMeshProUGUI credits = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
            credits.text = DungeonLocalization.IntroCredits;
            credits.fontSize *= 0.75f;
            credits.alignment = TMPro.TextAlignmentOptions.Center;
            credits.fontStyle = TMPro.FontStyles.Italic;
            credits.color = new Color(1f, 0.85f, 0.6f, 0.9f);
            _introRefreshCallbacks.Add(() => { credits.text = DungeonLocalization.IntroCredits; });
            
            // Spacer before button
            CreateSpacer(mainLayout.transform, 10f);
            
            // Button - use default size, just set reasonable dimensions
            UnityEngine.UI.Button okButton = UnityEngine.Object.Instantiate(DewGUI.widgetButton, mainLayout.transform);
            okButton.SetText(DungeonLocalization.IntroButtonOk);
            // Don't override size - let the prefab use its default size which scales properly
            okButton.interactable = true;
            
            // Ensure button has proper raycast target
            var buttonImage = okButton.GetComponent<UnityEngine.UI.Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
            }
            
            // Store reference to close window (avoid closure issues)
            var windowToClose = _introWindow;
            okButton.onClick.AddListener(() => {
                if (windowToClose != null)
                {
                    UnityEngine.Object.Destroy(windowToClose.gameObject);
                }
                _introWindow = null;
                _introRefreshCallbacks.Clear();
            });
            _introRefreshCallbacks.Add(() => { okButton.SetText(DungeonLocalization.IntroButtonOk); });
            
            // Add ContentSizeFitter to window
            UnityEngine.UI.ContentSizeFitter fitter = _introWindow.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            
            // Ensure window is on top and can receive input
            _introWindow.transform.SetAsLastSibling();
            
        }
        
        private void CreateSpacer(Transform parent, float height)
        {
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);
            UnityEngine.UI.LayoutElement le = spacer.AddComponent<UnityEngine.UI.LayoutElement>();
            le.preferredHeight = height;
        }
        
        private void OnIntroOkClicked()
        {
            CloseIntroWindow();
        }
        
        private void CloseIntroWindow()
        {
            if (_introWindow != null)
            {
                UnityEngine.Object.Destroy(_introWindow.gameObject);
                _introWindow = null;
            }
            _introRefreshCallbacks.Clear();
        }
        
        // ============================================================
        // END INTRODUCTION UI SYSTEM
        // ============================================================
        
        private void OnRoomLoaded(EventInfoLoadRoom info)
        {
            // If we're traveling to Zone_Primus (final boss), skip all our custom logic
            if (_isTravelingToPrimus) return;
            
            try
            {
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null) return;
                
                int currentNode = zm.currentNodeIndex;
                if (currentNode < 0 || currentNode >= zm.nodes.Count) return;
                
                // Auto-center the map view on current node (panning system) - ALL PLAYERS
                MapPanningSystem.CenterOnCurrentNode();
                
                // IMPORTANT: For clients joining mid-game, sync depth from nodes if we haven't yet
                // This ensures the depth is correct even if the client joins while in a room
                if (!NetworkServer.active && _totalDepth == 0 && zm.nodes.Count > 1)
                {
                    SyncDepthFromNodes();
                }
                
                // ========================================
                // CLIENT + SERVER: Track depth and visited nodes for UI
                // ========================================
                WorldNodeType nodeType = zm.nodes[currentNode].type;
                bool isNewNode = !_visitedNodes.Contains(currentNode);
                
                if (isNewNode)
                {
                    _visitedNodes.Add(currentNode);
                    
                    // Only Combat and Boss nodes increase depth
                    bool increasesDepth = (nodeType == WorldNodeType.Combat || nodeType == WorldNodeType.ExitBoss);
                    if (increasesDepth)
                    {
                        _totalDepth++;
                    }
                }
                else
                {
                    // REVISITING a node - teleport players to starting position after a frame delay
                    // Dev note: "teleport happens right after ClientEvent_OnRoomLoaded, 
                    // so you could teleport the players after a frame delay to override the teleport position"
                    if (NetworkServer.active)
                    {
                        StartCoroutine(TeleportPlayersToStartPosition());
                    }
                }
                
                // ========================================
                // SERVER ONLY: Node generation, hunter system, announcements
                // ========================================
                if (!NetworkServer.active) return;
                
                // NOTE: We do NOT reset didCreateInstance for modifiers!
                // One-time modifiers like Pot of Greed should only be usable once.
                // The game's modifier system handles this correctly.
                
                // Initialize node position if not tracked (especially important for start node)
                if (!_nodePositions.ContainsKey(currentNode))
                {
                    Vector2 nodePos = zm.nodes[currentNode].position;
                    // Ensure positive X
                    if (nodePos.x < 0f) nodePos.x = 1f;
                    _nodePositions[currentNode] = nodePos;
                    _nodeDirections[currentNode] = START_DIRECTION;
                    if (!_nodeConnections.ContainsKey(currentNode))
                    {
                        _nodeConnections[currentNode] = new HashSet<int>();
                    }
                }
                
                // Get node info for announcements
                string zoneName = "";
                if (_nodeZoneMap.ContainsKey(currentNode) && _nodeZoneMap[currentNode] != null)
                {
                    zoneName = _nodeZoneMap[currentNode].name;
                }
                else if (zm.currentZone != null)
                {
                    zoneName = zm.currentZone.name;
                }
                
                // Collect modifier names for announcements
                List<string> modifierNames = new List<string>();
                foreach (var modData in zm.nodes[currentNode].modifiers)
                {
                    // ModifierData is a struct, check if type is set
                    if (!string.IsNullOrEmpty(modData.type))
                    {
                        modifierNames.Add(modData.type);
                    }
                }
                
                if (isNewNode)
                {
                    // Server-only: Announce and handle game logic
                    bool increasesDepth = (nodeType == WorldNodeType.Combat || nodeType == WorldNodeType.ExitBoss);
                    
                    // ANNOUNCE the new room with atmospheric messages!
                    AnnounceDungeonEvent(zoneName, nodeType, _totalDepth, true, modifierNames);
                    
                    // ========================================
                    // POT OF GREED MINIBOSS SPAWN
                    // ========================================
                    // Check if any player used Pot of Greed and this is a combat room
                    if (_pendingMiniBossSpawn && nodeType == WorldNodeType.Combat)
                    {
                        _pendingMiniBossSpawn = false;
                        StartCoroutine(SpawnPotOfGreedMiniBoss());
                        SendChatMessage(DungeonLocalization.CreateMessageKey("POT_GREED"));
                    }
                    
                    // ========================================
                    // WEAKENED BOSS SPAWN (Rare event!)
                    // ========================================
                    // Small chance for a weakened boss to appear in combat nodes
                    if (nodeType == WorldNodeType.Combat && !_pendingMiniBossSpawn)
                    {
                        TrySpawnWeakenedBoss();
                    }
                    
                    // Generate child nodes only for new nodes that don't have children yet
                    // This MUST happen BEFORE hunter infection so we have nodes to infect!
                    if (!_nodesWithChildren.Contains(currentNode))
                    {
                        StartCoroutine(GenerateChildNodesAndTryInfect(currentNode));
                    }
                    else
                    {
                        // Node already has children, just try infection
                        // Custom Hunter Infection System: 50% chance to infect adjacent nodes (TESTING)
                        if (_dungeonRandom != null && _totalDepth > 0)
                        {
                            TryInfectAdjacentNodes(zm, currentNode);
                        }
                    }
                }
                else
                {
                    // Revisiting - monsters will respawn but depth stays the same
                    // DO NOT generate new nodes on revisit - this is intentional!
                }
                
                // ========================================
                // HUNTER INFECTION SYSTEM
                // ========================================
                
                // Check if this node was ALREADY infected before we entered
                // IMPORTANT: Also check if player JUST infected this node in this session
                // to prevent re-infection when the game does a revisit call
                bool nodeWasAlreadyInfected = _hunterInfectedNodes.Contains(currentNode);
                bool playerJustSpreadInfection = false;

                // Track nodes the player infected THIS SESSION (to prevent re-infection on revisit)
                bool playerInfectedThisNodeThisSession = _playerInfectedNodesThisSession.Contains(currentNode);
                
                // If player is infected and this node is NOT already infected, spread the infection!
                // Only consume charge if we're infecting a NEW node
                if (_playerIsInfected && _playerInfectionCharges > 0 && !nodeWasAlreadyInfected)
                {
                    // Don't infect safe nodes (Merchant, Start) or boss nodes
                    // Boss nodes should NEVER be infected - they're special encounters!
                    if (nodeType != WorldNodeType.Start && nodeType != WorldNodeType.Merchant && nodeType != WorldNodeType.ExitBoss)
                    {
                        _hunterInfectedNodes.Add(currentNode);
                        _playerInfectedNodesThisSession.Add(currentNode); // Track that WE infected this node
                        _playerInfectionCharges--;
                        playerJustSpreadInfection = true;
                        
                        // Also set the game's hunterStatuses so the map shows the visual indicator!
                        if (currentNode < zm.hunterStatuses.Count)
                        {
                            zm.hunterStatuses[currentNode] = HunterStatus.Level1;
                        }
                        
                        SendChatMessage(DungeonLocalization.CreateMessageKey("PLAYER_SPREAD"));
                        
                        // Check if player is cured (no more charges)
                        if (_playerInfectionCharges <= 0)
                        {
                            _playerIsInfected = false;
                            _playerInfectedNodesThisSession.Clear(); // Clear tracking when cured
                            SendChatMessage(DungeonLocalization.CreateMessageKey("PLAYER_CURED"));
                        }
                    }
                }
                
                // Check if node is NOW infected (either was already or just got infected by player)
                bool nodeIsInfected = _hunterInfectedNodes.Contains(currentNode);
                
                // CRITICAL: Safe rooms should NEVER be hunted, even if somehow infected!
                // This is a defensive check - safe nodes should never be in _hunterInfectedNodes
                // Safe nodes: Merchant, Start, ExitBoss
                bool isSafeNodeType = (nodeType == WorldNodeType.ExitBoss || 
                                       nodeType == WorldNodeType.Merchant || 
                                       nodeType == WorldNodeType.Start);
                
                if (isSafeNodeType)
                {
                    if (nodeIsInfected)
                    {
                        Debug.LogWarning("[InfiniteDungeon] BUG DETECTED: Safe node " + currentNode + " (" + nodeType + ") was in _hunterInfectedNodes! Removing...");
                        _hunterInfectedNodes.Remove(currentNode);
                        
                        // Also clear the game's hunter status
                        if (currentNode < zm.hunterStatuses.Count)
                        {
                            zm.hunterStatuses[currentNode] = HunterStatus.None;
                        }
                    }
                    nodeIsInfected = false; // Safe rooms are NEVER infected
                }
                
                // If this node is infected (either pre-existing OR just spread by player)
                // Apply hunter effects IMMEDIATELY - the node is dangerous!
                if (nodeIsInfected)
                {
                    if (nodeWasAlreadyInfected && !playerInfectedThisNodeThisSession)
                    {
                        SendChatMessage(DungeonLocalization.CreateMessageKey("HUNTER_ENCOUNTER"));
                    }
                    
                    // Apply hunter effects to the room via coroutine (wait for monsters to spawn)
                    // This buffs monsters and makes the room harder!
                    StartCoroutine(ApplyHunterEffectsToRoom());
                    
                    // If player is NOT already infected AND this was a pre-existing infected node
                    // (NOT one the player infected themselves), they have a CHANCE to become infected!
                    // CRITICAL: Don't re-infect if player just spread their last charge to this node
                    if (!_playerIsInfected && nodeWasAlreadyInfected && !playerInfectedThisNodeThisSession)
                    {
                        // Roll for infection chance - use config value (default 35%)
                        float infectionChance = config != null ? config.infectionChance / 100f : 0.35f;
                        float roll = _dungeonRandom != null ? _dungeonRandom.Value() : UnityEngine.Random.value;
                        if (roll < infectionChance)
                        {
                            _playerIsInfected = true;
                            _playerInfectionCharges = _dungeonRandom != null 
                                ? _dungeonRandom.Range(MIN_PLAYER_INFECTION_CHARGES, MAX_PLAYER_INFECTION_CHARGES + 1)
                                : 3;
                            SendChatMessage(DungeonLocalization.CreateMessageKey("PLAYER_INFECTED"));
                        
                            // Roll for a random curse when first getting infected (only once!)
                            TryApplyRandomCurse();
                        
                            // Apply curse effects but DON'T tick down charges on FIRST infection room
                            // (the room where you GET the curse shouldn't count)
                            ApplyCurseEffectsOnly();
                        }
                    }
                    else if (_playerIsInfected)
                    {
                        // Already infected - roll for additional curse when entering infected nodes
                        // 25% chance to get an additional curse
                        TryApplyAdditionalCurse();
                        
                        // Process existing curses normally (apply effects and tick down)
                        ProcessPlayerCurses();
                    }
                    
                    // Check for Obliviax trigger (5% chance) - only on pre-existing infected nodes
                    // (not ones the player infected themselves)
                    if (nodeWasAlreadyInfected && !playerInfectedThisNodeThisSession)
                    {
                    TryTriggerObliviax(zm);
                    }
                }
                else
                {
                    // Node is NOT infected
                    // ========================================
                    // SAFE NODE CHECK - Only Merchant and Start are truly safe!
                    // Event nodes CAN have combat, so they're not safe
                    // ========================================
                    bool isSafeNode = (nodeType == WorldNodeType.Merchant || 
                                       nodeType == WorldNodeType.Start);
                    
                    if (isSafeNode)
                    {
                        // Safe nodes - no curse effects, no tick down
                    }
                    else
                    {
                        // All other nodes - process curses normally (tick down charges)
                    // ========================================
                    // CURSE SYSTEM - Apply active curses and tick down
                    // ========================================
                    ProcessPlayerCurses();
                    }
                }
                
                // Register for room clear if node is infected (to clear infection when cleared)
                // Use nodeIsInfected here since we want to clear any infected node
                
                // Update the infection/curse UI
                UpdateInfectionUI();
                
                // ========================================
                // ROOM CLEAR LISTENER - Clear infection when room is cleared
                // ========================================
                Room room = SingletonDewNetworkBehaviour<Room>.instance;
                if (room != null && nodeIsInfected)
                {
                    // Capture the node index for the closure
                    int infectedNode = currentNode;
                    room.onRoomClear.AddListener(delegate
                    {
                        ClearNodeInfection(infectedNode);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in OnRoomLoaded: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
        
        /// <summary>
        /// Try to trigger Obliviax kidnap when entering a hunted node.
        /// This creates the Ge_Obliviax_InterruptTravelAndKidnap effect which:
        /// 1. Shows a warning message ("Obliviax is watching...")
        /// 2. Registers a travel interrupt
        /// 3. When the player tries to LEAVE the room, the kidnap sequence triggers
        /// 4. Player is teleported to Obliviax's Nest
        /// </summary>
        private void TryTriggerObliviax(ZoneManager zm)
        {
            try
            {
                // 5% chance to trigger Obliviax
                if (_dungeonRandom == null || _dungeonRandom.Value() >= OBLIVIAX_TRIGGER_CHANCE) return;
                
                // Check if already being kidnapped or in Obliviax room
                if (zm.isInAnyTransition) return;
                
                // Check if Ge_Obliviax_InterruptTravelAndKidnap already exists (don't stack)
                System.Type kidnapType = System.Type.GetType("Ge_Obliviax_InterruptTravelAndKidnap, Assembly-CSharp");
                if (kidnapType != null)
                {
                    var findMethod = typeof(Dew).GetMethods()
                        .FirstOrDefault(m => m.Name == "FindActorOfType" && m.IsGenericMethod && m.GetParameters().Length == 0);
                    if (findMethod != null)
                    {
                        var genericFind = findMethod.MakeGenericMethod(kidnapType);
                        var existingEffect = genericFind.Invoke(null, null);
                        if (existingEffect != null)
                {
                    return;
                        }
                    }
                }
                
                
                // Create the kidnap effect - this registers a travel interrupt
                // The actual kidnap happens when the player tries to leave (travel to another node)
                StartCoroutine(TriggerObliviaxCoroutine());
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in TryTriggerObliviax: " + ex.Message);
            }
        }
        
        private IEnumerator TriggerObliviaxCoroutine()
        {
            // Wait a moment for the room to fully load before showing the warning
            yield return new WaitForSeconds(1.5f);
            
            try
            {
                // Create the kidnap effect using Dew.CreateActor
                // This effect:
                // 1. Shows warning FX and broadcasts "Chat_Notice_ObliviaxWarning"
                // 2. Adds a TravelToNodeInterrupt
                // 3. When player tries to travel, TravelInterrupt is called
                // 4. TravelInterrupt starts the kidnap sequence (effects, then teleport to Obliviax's Nest)
                System.Type kidnapType = System.Type.GetType("Ge_Obliviax_InterruptTravelAndKidnap, Assembly-CSharp");
                if (kidnapType != null)
                {
                    var createActorMethod = typeof(Dew).GetMethods()
                        .FirstOrDefault(m => m.Name == "CreateActor" && m.IsGenericMethod && m.GetParameters().Length == 0);
                    
                    if (createActorMethod != null)
                    {
                        var genericMethod = createActorMethod.MakeGenericMethod(kidnapType);
                        genericMethod.Invoke(null, null);
                    }
                    else
                    {
                        Debug.LogError("[InfiniteDungeon] Could not find Dew.CreateActor method");
                    }
                }
                else
                {
                    Debug.LogError("[InfiniteDungeon] Could not find Ge_Obliviax_InterruptTravelAndKidnap type");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error creating Obliviax kidnap effect: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Teleport all players to the room's starting position when revisiting a node.
        /// Called with a frame delay to override the game's default teleport to saved position.
        /// </summary>
        private IEnumerator TeleportPlayersToStartPosition()
        {
            // Wait one frame to let the game's teleport happen first
            yield return null;
            
            try
            {
                Room room = SingletonDewNetworkBehaviour<Room>.instance;
                if (room == null)
                {
                    Debug.LogWarning("[InfiniteDungeon] No room instance found for teleport");
                    yield break;
                }
                
                Vector3 startPos = room.GetHeroSpawnPosition();
                
                // Teleport all players to the start position
                foreach (DewPlayer player in DewPlayer.gamePlayers)
                {
                    if (player != null && player.hero != null && !player.hero.IsNullInactiveDeadOrKnockedOut())
                    {
                        // Spread players slightly to avoid stacking
                        Vector3 offset = UnityEngine.Random.insideUnitSphere * 2f;
                        offset.y = 0f;
                        Vector3 teleportPos = Dew.GetValidAgentDestination_Closest(startPos, startPos + offset);
                        
                        player.hero.Control.Teleport(teleportPos);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error teleporting players to start: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Mark a node as visited (for depth tracking)
        /// </summary>
        public static void MarkNodeVisited(int nodeIndex)
        {
            _visitedNodes.Add(nodeIndex);
        }
        
        /// <summary>
        /// Check if a node has been visited
        /// </summary>
        public static bool IsNodeVisited(int nodeIndex)
        {
            return _visitedNodes.Contains(nodeIndex);
        }
        
        /// <summary>
        /// Get the current dungeon depth (used by other mods like RPGItemsMod for scaling)
        /// </summary>
        public static int GetTotalDepth()
        {
            return _totalDepth;
        }
        
        /// <summary>
        /// Set the total depth directly (used for initialization)
        /// </summary>
        public static void SetTotalDepth(int depth)
        {
            _totalDepth = depth;
        }
        
        /// <summary>
        /// Reset dungeon state (called when new game starts)
        /// </summary>
        public static void ResetDungeonState()
        {
            _totalDepth = 0;
            _nodeZoneMap.Clear();
            _visitedNodes.Clear();
            _nodePositions.Clear();
            _nodeConnections.Clear();
            _nodeDirections.Clear();
            _nodesWithChildren.Clear();
            _occupiedGridCells.Clear(); // Clear grid for new dungeon
            _isContinuedRun = false;
            _dungeonInitialized = false; // Reset this too for actual new games
            _needsResubscribe = true; // Re-subscribe to events on next Update
            _isSubscribedToRoomLoaded = false; // Reset subscription flag
            _allowModifierAddition = false;
            _ourModifierIds.Clear();
            _hunterInfectedNodes.Clear(); // Clear hunter tracking
            _playerIsInfected = false; // Clear player infection
            _playerInfectionCharges = 0;
            _playerInfectedNodesThisSession.Clear(); // Clear session infection tracking
            _playerCurses.Clear(); // Clear all curses
            _totalCursesEndured = 0; // Reset curses endured counter
            _weakenedBossesDefeated = 0; // Reset weakened bosses counter
            _potOfGreedPlayers.Clear(); // Clear pot of greed tracking
            _pendingMiniBossSpawn = false; // Clear pending miniboss spawn
            _weakenedBossNodes.Clear(); // Clear weakened boss tracking
            _pendingWeakenedBossName = null; // Clear pending boss spawn
            _appliedCurseDebuffs.Clear(); // Clear tracked curse debuffs
            ClearSyncedConnections(); // Clear connection sync data
            
            // Destroy infection UI if it exists
            if (_infectionUI != null)
            {
                UnityEngine.Object.Destroy(_infectionUI);
                _infectionUI = null;
                _infectionText = null;
            }
            
            // Reset announcement tracking
            _welcomeMessageShown = false;
            _lastAnnouncedDepth = -1;
            
            // Reset intro window state so it shows again on new game
            // BUT don't destroy the window if it's currently showing - let the player close it
            // Only reset the flag so a NEW game will show the intro again
            // The window will be destroyed when player clicks OK or when leaving the game
            if (_introWindow == null)
            {
                // Only reset the flag if window is not currently showing
                // This prevents the intro from being re-shown during the same session
                _introShownThisSession = false;
            }
            
            // Reset result UI
            InfiniteDungeonResultUI.Reset();
            
            // Clear zones so they get reloaded fresh (Zone objects can be destroyed when leaving scene)
            _allZones.Clear();
            
            // Re-initialize random number generator with a fresh seed
            _dungeonRandom = new DewRandom(DewRandom.GetRandomSeed());
            
        }
        
        private void OnDestroy()
        {
            IsModActive = false;
            Instance = null;
            
            if (NetworkedManagerBase<ZoneManager>.instance != null)
            {
                NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= OnRoomLoaded;
            }
            
            if (_harmony != null)
            {
                _harmony.UnpatchAll(_harmony.Id);
                _harmonyPatchesApplied = false;
            }
            
            ResetDungeonState();
            
        }
    }
    
    // ==================== HARMONY PATCHES ====================
}
