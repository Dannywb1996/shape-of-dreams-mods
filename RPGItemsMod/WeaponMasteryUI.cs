using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Weapon Mastery UI - Displays mastery progress for the CURRENTLY EQUIPPED custom weapon only
/// Updates dynamically when weapon is changed
/// </summary>
public class WeaponMasteryUI : MonoBehaviour
{
    // References
    private WeaponMasterySystem masterySystem;
    private Canvas canvas;
    private GameObject mainPanel;
    private bool isVisible = false;
    
    // UI Elements
    private Text titleText;
    private Text currentWeaponText;
    private GameObject rowContainer;
    private List<MasteryRowUI> masteryRows = new List<MasteryRowUI>();
    private GameObject noWeaponsMessage;
    
    // Cached hero ID and weapon name for change detection
    private string currentHeroId = null;
    private string lastEquippedWeapon = null;
    
    // Row structure
    private class MasteryRowUI
    {
        public GameObject rowObject;
        public Text weaponNameText;
        public Text levelText;
        public Image xpBarBackground;
        public Image xpBarFill;
        public Text xpText;
        public Text bonusText;
        public string weaponName;
    }
    
    // Screen resolution tracking for dynamic UI scaling
    private Vector2 _lastScreenSize;
    private float _lastGameUIScale = 1f;
    private float _lastModUIScale = 1f;
    
    // Colors
    private static readonly Color PANEL_BG = new Color(0.08f, 0.08f, 0.12f, 0.95f);
    private static readonly Color ROW_BG = new Color(0.12f, 0.12f, 0.18f, 1f);
    private static readonly Color ROW_BG_EQUIPPED = new Color(0.15f, 0.18f, 0.12f, 1f); // Greenish for equipped
    private static readonly Color XP_BAR_BG = new Color(0.2f, 0.2f, 0.25f, 1f);
    private static readonly Color XP_BAR_FILL = new Color(0.4f, 0.7f, 0.3f, 1f);
    private static readonly Color XP_BAR_MAX = new Color(1f, 0.85f, 0.2f, 1f);
    private static readonly Color TITLE_COLOR = new Color(0.9f, 0.8f, 0.5f, 1f);
    private static readonly Color TEXT_COLOR = new Color(0.9f, 0.9f, 0.95f, 1f);
    private static readonly Color TEXT_DIM = new Color(0.6f, 0.6f, 0.65f, 1f);
    private static readonly Color BONUS_COLOR = new Color(0.5f, 0.9f, 0.5f, 1f);
    private static readonly Color EQUIPPED_COLOR = new Color(0.4f, 1f, 0.4f, 1f);
    
    public void Initialize(WeaponMasterySystem system)
    {
        masterySystem = system;
        
        // Initialize screen size tracking
        _lastScreenSize = new Vector2(Screen.width, Screen.height);
        _lastGameUIScale = GetGameUIScale();
        _lastModUIScale = RPGItemsMod.GetModUIScaleOnly();
        
        if (masterySystem != null)
        {
            masterySystem.OnMasteryXPGained += RefreshUI;
            masterySystem.OnMasteryLevelUp += OnLevelUp;
        }
        
        CreateCanvas();
        CreateMainPanel();
        
        SetVisible(false);
    }
    
    /// <summary>
    /// Get the game's UI scale setting safely
    /// </summary>
    private float GetGameUIScale()
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
    
    private void OnDestroy()
    {
        if (masterySystem != null)
        {
            masterySystem.OnMasteryXPGained -= RefreshUI;
            masterySystem.OnMasteryLevelUp -= OnLevelUp;
        }
    }
    
    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("WeaponMasteryUI_Canvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 6; // Above inventory
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
    }
    
    private void CreateMainPanel()
    {
        // Calculate UI scale (with screen factor for proper resolution support)
        float uiScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        
        // Main panel - positioned on the right side of screen
        mainPanel = new GameObject("MasteryPanel");
        mainPanel.transform.SetParent(canvas.transform, false);
        
        Image panelBg = mainPanel.AddComponent<Image>();
        panelBg.color = PANEL_BG;
        
        RectTransform panelRect = mainPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.pivot = new Vector2(1, 0.5f);
        
        // Scale panel size
        float baseWidth = 350f;
        float baseHeight = 450f;
        float panelWidth = baseWidth * uiScale;
        float panelHeight = baseHeight * uiScale;
        panelWidth = Mathf.Max(panelWidth, 280f);
        panelHeight = Mathf.Max(panelHeight, 360f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        
        // Scale position offset
        float baseOffsetX = -20f;
        float baseOffsetY = 50f;
        panelRect.anchoredPosition = new Vector2(baseOffsetX * uiScale, baseOffsetY * uiScale);
        
        // Add outline
        Outline outline = mainPanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.3f, 0.4f, 1f);
        outline.effectDistance = new Vector2(2, -2);
        
        // Title bar
        GameObject titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(mainPanel.transform, false);
        
        Image titleBg = titleBar.AddComponent<Image>();
        titleBg.color = new Color(0.15f, 0.12f, 0.08f, 1f);
        
        RectTransform titleRect = titleBar.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 40);
        titleRect.anchoredPosition = Vector2.zero;
        
        // Title text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(titleBar.transform, false);
        
        titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.WeaponMastery;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 18 to 22 for better readability
        float titleFontSize = Mathf.Max(22f * uiScale, 18f);
        titleText.fontSize = Mathf.RoundToInt(titleFontSize);
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = TITLE_COLOR;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleTextRect = titleObj.GetComponent<RectTransform>();
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = Vector2.one;
        titleTextRect.sizeDelta = Vector2.zero;
        
        // Current weapon indicator
        GameObject currentWeaponObj = new GameObject("CurrentWeapon");
        currentWeaponObj.transform.SetParent(mainPanel.transform, false);
        
        currentWeaponText = currentWeaponObj.AddComponent<Text>();
        currentWeaponText.text = "";
        currentWeaponText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 13 to 15 for better readability
        float currentWeaponFontSize = Mathf.Max(15f * uiScale, 13f);
        currentWeaponText.fontSize = Mathf.RoundToInt(currentWeaponFontSize);
        currentWeaponText.color = EQUIPPED_COLOR;
        currentWeaponText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform currentWeaponRect = currentWeaponObj.GetComponent<RectTransform>();
        currentWeaponRect.anchorMin = new Vector2(0, 1);
        currentWeaponRect.anchorMax = new Vector2(1, 1);
        currentWeaponRect.pivot = new Vector2(0.5f, 1);
        currentWeaponRect.sizeDelta = new Vector2(-20, 25);
        currentWeaponRect.anchoredPosition = new Vector2(0, -45);
        
        // Row container with vertical layout (scrollable area)
        rowContainer = new GameObject("RowContainer");
        rowContainer.transform.SetParent(mainPanel.transform, false);
        
        RectTransform containerRect = rowContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        float containerPadding = 10f * uiScale;
        float containerBottomOffset = Mathf.Max(75f * uiScale, 60f);
        containerRect.offsetMin = new Vector2(containerPadding, containerPadding);
        containerRect.offsetMax = new Vector2(-containerPadding, -containerBottomOffset);
        
        VerticalLayoutGroup layout = rowContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6;
        layout.padding = new RectOffset(0, 0, 5, 5);
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        
        // No weapons message (hidden by default)
        noWeaponsMessage = new GameObject("NoWeaponsMessage");
        noWeaponsMessage.transform.SetParent(rowContainer.transform, false);
        
        Text noWeaponsText = noWeaponsMessage.AddComponent<Text>();
        noWeaponsText.text = Localization.Get("NoCustomWeaponsUsed");
        noWeaponsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 12 to 14 for better readability
        float noWeaponsFontSize = Mathf.Max(14f * uiScale, 12f);
        noWeaponsText.fontSize = Mathf.RoundToInt(noWeaponsFontSize);
        noWeaponsText.color = TEXT_DIM;
        noWeaponsText.alignment = TextAnchor.UpperCenter;
        
        RectTransform noWeaponsRect = noWeaponsMessage.GetComponent<RectTransform>();
        noWeaponsRect.sizeDelta = new Vector2(0, 100);
        
        LayoutElement noWeaponsLayout = noWeaponsMessage.AddComponent<LayoutElement>();
        noWeaponsLayout.preferredHeight = 100;
        noWeaponsLayout.minHeight = 100;
        
        // Close button
        CreateCloseButton();
        
        // Apply UI scaling from config
        ApplyUIScale();
    }
    
    /// <summary>
    /// Apply UI scale from config
    /// </summary>
    private void ApplyUIScale()
    {
        float scale = RPGItemsMod.GetUIScale();
        
        if (mainPanel != null)
        {
            RectTransform panelRect = mainPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }
    
    private void Update()
    {
        // Check for screen resolution or UI scale changes
        CheckForScaleChanges();
    }
    
    /// <summary>
    /// Check for screen resolution or UI scale changes and reapply scaling if needed
    /// </summary>
    private void CheckForScaleChanges()
    {
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        float currentGameUIScale = GetGameUIScale();
        float currentModUIScale = RPGItemsMod.GetModUIScaleOnly();
        
        bool needsUpdate = false;
        
        // Check if screen size changed
        if (_lastScreenSize != currentScreenSize)
        {
            _lastScreenSize = currentScreenSize;
            needsUpdate = true;
        }
        
        // Check if game UI scale changed
        if (Mathf.Abs(_lastGameUIScale - currentGameUIScale) > 0.001f)
        {
            _lastGameUIScale = currentGameUIScale;
            needsUpdate = true;
        }
        
        // Check if mod UI scale changed
        if (Mathf.Abs(_lastModUIScale - currentModUIScale) > 0.001f)
        {
            _lastModUIScale = currentModUIScale;
            needsUpdate = true;
        }
        
        if (needsUpdate)
        {
            ApplyUIScale();
        }
    }
    
    private void CreateCloseButton()
    {
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(mainPanel.transform, false);
        
        Image closeBg = closeBtn.AddComponent<Image>();
        closeBg.color = new Color(0.5f, 0.2f, 0.2f, 1f);
        
        Button btn = closeBtn.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.5f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.6f, 0.25f, 0.25f, 1f);
        colors.pressedColor = new Color(0.4f, 0.15f, 0.15f, 1f);
        btn.colors = colors;
        
        btn.onClick.AddListener(delegate { SetVisible(false); });
        
        RectTransform closeRect = closeBtn.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.sizeDelta = new Vector2(30, 30);
        closeRect.anchoredPosition = new Vector2(-5, -5);
        
        // X text
        GameObject xText = new GameObject("X");
        xText.transform.SetParent(closeBtn.transform, false);
        
        Text x = xText.AddComponent<Text>();
        x.text = "X";
        x.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        x.fontSize = 16;
        x.fontStyle = FontStyle.Bold;
        x.color = Color.white;
        x.alignment = TextAnchor.MiddleCenter;
        
        RectTransform xRect = xText.GetComponent<RectTransform>();
        xRect.anchorMin = Vector2.zero;
        xRect.anchorMax = Vector2.one;
        xRect.sizeDelta = Vector2.zero;
    }
    
    private MasteryRowUI CreateMasteryRow(WeaponMasterySystem.MasteryData data, bool isEquipped)
    {
        // Calculate UI scale for weapon row elements
        float uiScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        
        MasteryRowUI row = new MasteryRowUI();
        row.weaponName = data.weaponName;
        
        // Row container
        GameObject rowObj = new GameObject("MasteryRow_" + data.weaponId);
        rowObj.transform.SetParent(rowContainer.transform, false);
        row.rowObject = rowObj;
        
        Image rowBg = rowObj.AddComponent<Image>();
        rowBg.color = isEquipped ? ROW_BG_EQUIPPED : ROW_BG;
        
        RectTransform rowRect = rowObj.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 65);
        
        LayoutElement layoutElem = rowObj.AddComponent<LayoutElement>();
        layoutElem.preferredHeight = 65;
        layoutElem.minHeight = 65;
        
        // Weapon name and level (top row)
        GameObject topRow = new GameObject("TopRow");
        topRow.transform.SetParent(rowObj.transform, false);
        
        RectTransform topRect = topRow.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 0.6f);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.offsetMin = new Vector2(10, 0);
        topRect.offsetMax = new Vector2(-10, -3);
        
        // Weapon name (with equipped indicator) - use translated name
        GameObject nameObj = new GameObject("WeaponName");
        nameObj.transform.SetParent(topRow.transform, false);
        
        string displayName = data.GetDisplayName();
        row.weaponNameText = nameObj.AddComponent<Text>();
        row.weaponNameText.text = isEquipped ? "► " + displayName : displayName;
        row.weaponNameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 14 to 16 for better readability
        float weaponNameFontSize = Mathf.Max(16f * uiScale, 14f);
        row.weaponNameText.fontSize = Mathf.RoundToInt(weaponNameFontSize);
        row.weaponNameText.fontStyle = FontStyle.Bold;
        row.weaponNameText.color = isEquipped ? EQUIPPED_COLOR : TEXT_COLOR;
        row.weaponNameText.alignment = TextAnchor.MiddleLeft;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(0.65f, 1);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        // Level text
        GameObject levelObj = new GameObject("Level");
        levelObj.transform.SetParent(topRow.transform, false);
        
        row.levelText = levelObj.AddComponent<Text>();
        row.levelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 13 to 15 for better readability
        float levelFontSize = Mathf.Max(15f * uiScale, 13f);
        row.levelText.fontSize = Mathf.RoundToInt(levelFontSize);
        row.levelText.fontStyle = FontStyle.Bold;
        row.levelText.alignment = TextAnchor.MiddleRight;
        
        RectTransform levelRect = levelObj.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.65f, 0);
        levelRect.anchorMax = new Vector2(1, 1);
        levelRect.offsetMin = Vector2.zero;
        levelRect.offsetMax = Vector2.zero;
        
        // XP bar (middle)
        GameObject xpBarObj = new GameObject("XPBar");
        xpBarObj.transform.SetParent(rowObj.transform, false);
        
        row.xpBarBackground = xpBarObj.AddComponent<Image>();
        row.xpBarBackground.color = XP_BAR_BG;
        
        RectTransform xpBarRect = xpBarObj.GetComponent<RectTransform>();
        xpBarRect.anchorMin = new Vector2(0, 0.35f);
        xpBarRect.anchorMax = new Vector2(1, 0.55f);
        xpBarRect.offsetMin = new Vector2(10, 0);
        xpBarRect.offsetMax = new Vector2(-10, 0);
        
        // XP bar fill
        GameObject xpFillObj = new GameObject("XPFill");
        xpFillObj.transform.SetParent(xpBarObj.transform, false);
        
        row.xpBarFill = xpFillObj.AddComponent<Image>();
        row.xpBarFill.color = XP_BAR_FILL;
        
        RectTransform xpFillRect = xpFillObj.GetComponent<RectTransform>();
        xpFillRect.anchorMin = new Vector2(0, 0);
        xpFillRect.anchorMax = new Vector2(0.5f, 1);
        xpFillRect.offsetMin = Vector2.zero;
        xpFillRect.offsetMax = Vector2.zero;
        
        // XP text (on bar)
        GameObject xpTextObj = new GameObject("XPText");
        xpTextObj.transform.SetParent(xpBarObj.transform, false);
        
        row.xpText = xpTextObj.AddComponent<Text>();
        row.xpText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 10 to 12 for better readability
        float xpFontSize = Mathf.Max(12f * uiScale, 10f);
        row.xpText.fontSize = Mathf.RoundToInt(xpFontSize);
        row.xpText.color = TEXT_COLOR;
        row.xpText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform xpTextRect = xpTextObj.GetComponent<RectTransform>();
        xpTextRect.anchorMin = Vector2.zero;
        xpTextRect.anchorMax = Vector2.one;
        xpTextRect.sizeDelta = Vector2.zero;
        
        // Bonus text (bottom)
        GameObject bonusObj = new GameObject("Bonus");
        bonusObj.transform.SetParent(rowObj.transform, false);
        
        row.bonusText = bonusObj.AddComponent<Text>();
        row.bonusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 10 to 12 for better readability
        float bonusFontSize = Mathf.Max(12f * uiScale, 10f);
        row.bonusText.fontSize = Mathf.RoundToInt(bonusFontSize);
        row.bonusText.color = BONUS_COLOR;
        row.bonusText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform bonusRect = bonusObj.GetComponent<RectTransform>();
        bonusRect.anchorMin = new Vector2(0, 0);
        bonusRect.anchorMax = new Vector2(1, 0.35f);
        bonusRect.offsetMin = new Vector2(10, 3);
        bonusRect.offsetMax = new Vector2(-10, 0);
        
        return row;
    }
    
    private void UpdateMasteryRow(MasteryRowUI row, WeaponMasterySystem.MasteryData data, bool isEquipped)
    {
        if (row == null || data == null || masterySystem == null) return;
        
        int level = data.level;
        int currentXP = data.currentXP;
        int nextLevelXP = masterySystem.GetXPForNextLevel(level);
        int currentLevelXP = masterySystem.GetXPForLevel(level);
        
        // Update weapon name with equipped indicator
        row.weaponNameText.text = isEquipped ? "► " + data.weaponName : data.weaponName;
        row.weaponNameText.color = isEquipped ? EQUIPPED_COLOR : TEXT_COLOR;
        
        // Update row background
        Image rowBg = row.rowObject.GetComponent<Image>();
        if (rowBg != null)
        {
            rowBg.color = isEquipped ? ROW_BG_EQUIPPED : ROW_BG;
        }
        
        // Update level text - show special color for high levels
        if (level >= WeaponMasterySystem.SOFT_CAP_LEVEL)
        {
            row.levelText.text = string.Format("Lv {0} ★", level);
            row.levelText.color = XP_BAR_MAX; // Gold color for soft cap+
        }
        else
        {
            row.levelText.text = string.Format("Lv {0}", level);
            row.levelText.color = TITLE_COLOR;
        }
        
        // Update XP bar
        int xpInLevel = currentXP - currentLevelXP;
        int xpNeeded = nextLevelXP - currentLevelXP;
        float progress = xpNeeded > 0 ? (float)xpInLevel / xpNeeded : 0f;
        
        row.xpBarFill.GetComponent<RectTransform>().anchorMax = new Vector2(Mathf.Clamp01(progress), 1);
        row.xpBarFill.color = level >= WeaponMasterySystem.SOFT_CAP_LEVEL ? XP_BAR_MAX : XP_BAR_FILL;
        
        // Format XP text
        if (nextLevelXP >= 10000)
        {
            row.xpText.text = string.Format("{0:N0} / {1:N0}", currentXP, nextLevelXP);
        }
        else
        {
            row.xpText.text = string.Format("{0} / {1}", currentXP, nextLevelXP);
        }
        
        // Update bonus text
        if (level > 0)
        {
            row.bonusText.text = masterySystem.GetMasteryBonusDescription(level);
            row.bonusText.color = isEquipped ? EQUIPPED_COLOR : BONUS_COLOR;
        }
        else
        {
            row.bonusText.text = Localization.Get("KeepUsingToLevelUp");
            row.bonusText.color = TEXT_DIM;
        }
    }
    
    public void RefreshUI()
    {
        if (masterySystem == null) return;
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        string heroId = DewPlayer.local.hero.GetType().Name;
        string currentWeapon = masterySystem.GetEquippedCustomWeaponName();
        
        // Check if hero or weapon changed
        bool heroChanged = (heroId != currentHeroId);
        bool weaponChanged = (currentWeapon != lastEquippedWeapon);
        
        if (heroChanged)
        {
            currentHeroId = heroId;
        }
        
        if (weaponChanged)
        {
            lastEquippedWeapon = currentWeapon;
        }
        
        // Rebuild UI (always rebuild to show current state)
        RebuildUI(heroId);
    }
    
    private void RebuildUI(string heroId)
    {
        // Clear existing rows
        foreach (var existingRow in masteryRows)
        {
            if (existingRow.rowObject != null)
            {
                Destroy(existingRow.rowObject);
            }
        }
        masteryRows.Clear();
        
        // Get currently equipped weapon
        RPGItem equippedWeaponItem = masterySystem.GetEquippedCustomWeapon();
        string equippedWeapon = equippedWeaponItem != null ? equippedWeaponItem.name : null;
        
        // Update current weapon text - use translated name
        if (equippedWeaponItem != null)
        {
            string displayName = equippedWeaponItem.GetDisplayName();
            string equippedLabel = Localization.Get("Equipped");
            currentWeaponText.text = equippedLabel + ": " + displayName;
            currentWeaponText.color = EQUIPPED_COLOR;
        }
        else
        {
            currentWeaponText.text = Localization.Get("NoCustomWeaponEquipped");
            currentWeaponText.color = TEXT_DIM;
        }
        
        // Only show mastery for the currently equipped weapon
        if (equippedWeaponItem == null)
        {
            // No weapon equipped - show message
            if (noWeaponsMessage != null)
            {
                noWeaponsMessage.SetActive(true);
            }
            // Resize panel to fit the message (title + current weapon text + message)
            mainPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 200);
            return;
        }
        
        // Get mastery data for equipped weapon only
        WeaponMasterySystem.MasteryData equippedMastery = masterySystem.GetMastery(heroId, equippedWeapon);
        
        if (equippedMastery == null)
        {
            // Weapon has no mastery data yet - create a placeholder display
            // This happens when a weapon is first equipped before any XP is gained
            string weaponId = equippedWeapon.ToLower().Replace(" ", "_");
            equippedMastery = new WeaponMasterySystem.MasteryData(equippedWeapon, weaponId, equippedWeaponItem.id);
        }
        
        // Hide no weapons message
        if (noWeaponsMessage != null)
        {
            noWeaponsMessage.SetActive(false);
        }
        
        // Create single row for equipped weapon
        MasteryRowUI row = CreateMasteryRow(equippedMastery, true);
        UpdateMasteryRow(row, equippedMastery, true);
        masteryRows.Add(row);
        
        // Resize panel for single weapon display
        float panelHeight = 85 + 71; // Title + current weapon + 1 row
        mainPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(350, panelHeight);
    }
    
    private void OnLevelUp(string heroId, string weaponName, int newLevel)
    {
        // Find and highlight the row
        foreach (var row in masteryRows)
        {
            if (row.weaponName == weaponName)
            {
                row.levelText.color = Color.yellow;
                break;
            }
        }
        
        RefreshUI();
    }
    
    public void SetVisible(bool visible)
    {
        isVisible = visible;
        if (mainPanel != null)
        {
            mainPanel.SetActive(visible);
        }
        
        if (visible)
        {
            RefreshUI();
        }
    }
    
    public bool IsVisible()
    {
        return isVisible;
    }
    
    public void Toggle()
    {
        SetVisible(!isVisible);
    }
    
    // ESC key handling is now centralized in RPGItemsMod.Update()
}
