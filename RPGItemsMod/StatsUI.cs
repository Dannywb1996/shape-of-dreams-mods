using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

/// <summary>
/// Stats UI - DewGUI-style window for allocating stat points
/// Matches the inventory UI visual style with proper localization
/// </summary>
public class StatsUI : MonoBehaviour
{
    // References
    private StatsSystem statsSystem;
    private Canvas canvas;
    private GameObject mainPanel;
    private bool isVisible = false;
    
    // UI Elements
    private Text titleText;
    private Text availablePointsText;
    private Dictionary<StatsSystem.StatType, StatRow> statRows = new Dictionary<StatsSystem.StatType, StatRow>();
    private Button resetButton;
    private Button closeButton;
    
    // Stat row data structure
    private class StatRow
    {
        public GameObject rowObject;
        public Text nameText;
        public Text valueText;
        public Text bonusText;
        public Button addButton;
    }
    
    // Screen resolution tracking for dynamic UI scaling
    private Vector2 _lastScreenSize;
    private float _lastGameUIScale = 1f;
    private float _lastModUIScale = 1f;
    
    public void Initialize(StatsSystem system)
    {
        statsSystem = system;
        
        // Initialize screen size tracking
        _lastScreenSize = new Vector2(Screen.width, Screen.height);
        _lastGameUIScale = GetGameUIScale();
        _lastModUIScale = RPGItemsMod.GetModUIScaleOnly();
        
        // Subscribe to events
        statsSystem.OnStatsChanged += RefreshUI;
        statsSystem.OnPointsGained += OnPointsGained;
        
        CreateCanvas();
        CreateMainPanel();
        
        // Start hidden
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
        if (statsSystem != null)
        {
            statsSystem.OnStatsChanged -= RefreshUI;
            statsSystem.OnPointsGained -= OnPointsGained;
        }
    }
    
    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("StatsUI_Canvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5; // Above inventory but below dialogs
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
    }
    
    private void CreateMainPanel()
    {
        // Calculate UI scale (with screen factor for proper resolution support)
        float uiScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        
        // Scale panel size
        float baseWidth = 420f;
        float baseHeight = 550f;
        float panelWidth = baseWidth * uiScale;
        float panelHeight = baseHeight * uiScale;
        panelWidth = Mathf.Max(panelWidth, 336f);
        panelHeight = Mathf.Max(panelHeight, 440f);
        
        // Use UIHelper to create a game-styled window (same as inventory)
        mainPanel = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(panelWidth, panelHeight), "StatsPanel");
        
        RectTransform panelRect = mainPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        
        // Create header
        CreateHeader();
        
        // Create stat rows
        CreateStatRows();
        
        // Create footer with buttons
        CreateFooter();
        
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
    
    private void CreateHeader()
    {
        // Calculate UI scale for header elements
        float headerUIScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        
        // Header background
        GameObject header = new GameObject("Header");
        header.transform.SetParent(mainPanel.transform, false);
        
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        float headerWidth = -20f * headerUIScale;
        float headerHeight = Mathf.Max(70f * headerUIScale, 56f);
        headerRect.sizeDelta = new Vector2(headerWidth, headerHeight);
        float headerOffsetY = -10f * headerUIScale;
        headerRect.anchoredPosition = new Vector2(0, headerOffsetY);
        
        // Title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(header.transform, false);
        
        titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.StatsWindowTitle;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 22 to 26 for better readability
        float titleFontSize = Mathf.Max(26f * headerUIScale, 22f);
        titleText.fontSize = Mathf.RoundToInt(titleFontSize);
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.9f, 0.9f, 1f, 1f);
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.5f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = Vector2.zero;
        titleRect.anchoredPosition = Vector2.zero;
        
        // Available points text
        GameObject pointsObj = new GameObject("AvailablePoints");
        pointsObj.transform.SetParent(header.transform, false);
        
        availablePointsText = pointsObj.AddComponent<Text>();
        availablePointsText.text = string.Format(Localization.StatsAvailablePoints, 0);
        availablePointsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 14 to 16 for better readability
        float pointsFontSize = Mathf.Max(16f * headerUIScale, 14f);
        availablePointsText.fontSize = Mathf.RoundToInt(pointsFontSize);
        availablePointsText.color = new Color(0.7f, 0.8f, 0.9f, 1f);
        availablePointsText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform pointsRect = pointsObj.GetComponent<RectTransform>();
        pointsRect.anchorMin = new Vector2(0, 0);
        pointsRect.anchorMax = new Vector2(1, 0.5f);
        pointsRect.pivot = new Vector2(0.5f, 0.5f);
        pointsRect.sizeDelta = Vector2.zero;
        pointsRect.anchoredPosition = Vector2.zero;
        
        // Close button (X) in top-right corner
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(mainPanel.transform, false);
        
        closeButton = closeObj.AddComponent<Button>();
        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(0.6f, 0.2f, 0.2f, 0.9f);
        
        RectTransform closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.sizeDelta = new Vector2(28, 28);
        closeRect.anchoredPosition = new Vector2(-8, -8);
        
        GameObject closeTextObj = new GameObject("CloseText");
        closeTextObj.transform.SetParent(closeObj.transform, false);
        Text closeText = closeTextObj.AddComponent<Text>();
        closeText.text = "×";
        closeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 20 to 24 for better readability
        float closeFontSize = Mathf.Max(24f * headerUIScale, 20f);
        closeText.fontSize = Mathf.RoundToInt(closeFontSize);
        closeText.color = Color.white;
        closeText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;
        
        closeButton.onClick.AddListener(delegate { SetVisible(false); });
        
        // Hover effect for close button
        ColorBlock closeColors = closeButton.colors;
        closeColors.normalColor = new Color(0.6f, 0.2f, 0.2f, 0.9f);
        closeColors.highlightedColor = new Color(0.8f, 0.3f, 0.3f, 1f);
        closeColors.pressedColor = new Color(0.5f, 0.15f, 0.15f, 1f);
        closeButton.colors = closeColors;
    }
    
    private void CreateStatRows()
    {
        // Stats container with scroll view style background
        GameObject statsContainer = new GameObject("StatsContainer");
        statsContainer.transform.SetParent(mainPanel.transform, false);
        
        RectTransform containerRect = statsContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.offsetMin = new Vector2(15, 55); // Left, Bottom
        containerRect.offsetMax = new Vector2(-15, -85); // Right, Top
        
        // Container background (slightly darker)
        Image containerBg = statsContainer.AddComponent<Image>();
        containerBg.color = new Color(0.02f, 0.05f, 0.08f, 0.8f);
        
        // Add vertical layout
        VerticalLayoutGroup layout = statsContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        
        // Create a row for each stat
        foreach (StatsSystem.StatType stat in Enum.GetValues(typeof(StatsSystem.StatType)))
        {
            CreateStatRow(statsContainer.transform, stat);
        }
    }
    
    private void CreateStatRow(Transform parent, StatsSystem.StatType stat)
    {
        // Calculate UI scale for stat rows
        float rowUIScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        
        StatRow row = new StatRow();
        
        // Row container
        GameObject rowObj = new GameObject("StatRow_" + stat);
        rowObj.transform.SetParent(parent, false);
        row.rowObject = rowObj;
        
        RectTransform rowRect = rowObj.AddComponent<RectTransform>();
        float rowHeight = Mathf.Max(58f * rowUIScale, 46f);
        rowRect.sizeDelta = new Vector2(0, rowHeight);
        
        // Row background (slot-like appearance)
        Image rowBg = rowObj.AddComponent<Image>();
        rowBg.color = new Color(0.05f, 0.1f, 0.15f, 0.9f);
        
        // Stat name (left side)
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(rowObj.transform, false);
        
        row.nameText = nameObj.AddComponent<Text>();
        row.nameText.text = StatsSystem.GetStatName(stat);
        row.nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 15 to 17 for better readability
        float nameFontSize = Mathf.Max(17f * rowUIScale, 15f);
        row.nameText.fontSize = Mathf.RoundToInt(nameFontSize);
        row.nameText.fontStyle = FontStyle.Bold;
        row.nameText.color = new Color(0.9f, 0.9f, 1f, 1f);
        row.nameText.alignment = TextAnchor.MiddleLeft;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(0.4f, 1);
        nameRect.pivot = new Vector2(0, 0.5f);
        nameRect.offsetMin = new Vector2(12, 2);
        nameRect.offsetMax = new Vector2(0, -2);
        
        // Stat value (center) - points allocated
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(rowObj.transform, false);
        
        row.valueText = valueObj.AddComponent<Text>();
        row.valueText.text = "0";
        row.valueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 20 to 24 for better readability
        float valueFontSize = Mathf.Max(24f * rowUIScale, 20f);
        row.valueText.fontSize = Mathf.RoundToInt(valueFontSize);
        row.valueText.fontStyle = FontStyle.Bold;
        row.valueText.color = new Color(0.5f, 0.8f, 1f, 1f);
        row.valueText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform valueRect = valueObj.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.4f, 0.5f);
        valueRect.anchorMax = new Vector2(0.55f, 1);
        valueRect.pivot = new Vector2(0.5f, 0.5f);
        valueRect.offsetMin = new Vector2(0, 2);
        valueRect.offsetMax = new Vector2(0, -2);
        
        // Bonus description text (below name)
        GameObject bonusObj = new GameObject("Bonus");
        bonusObj.transform.SetParent(rowObj.transform, false);
        
        row.bonusText = bonusObj.AddComponent<Text>();
        row.bonusText.text = "";
        row.bonusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 11 to 13 for better readability
        float bonusFontSize = Mathf.Max(13f * rowUIScale, 11f);
        row.bonusText.fontSize = Mathf.RoundToInt(bonusFontSize);
        row.bonusText.color = new Color(0.5f, 0.6f, 0.7f, 1f);
        row.bonusText.alignment = TextAnchor.MiddleLeft;
        
        RectTransform bonusRect = bonusObj.GetComponent<RectTransform>();
        bonusRect.anchorMin = new Vector2(0, 0);
        bonusRect.anchorMax = new Vector2(0.75f, 0.5f);
        bonusRect.pivot = new Vector2(0, 0.5f);
        bonusRect.offsetMin = new Vector2(12, 2);
        bonusRect.offsetMax = new Vector2(0, -2);
        
        // Add button (+) - right side
        GameObject addObj = new GameObject("AddButton");
        addObj.transform.SetParent(rowObj.transform, false);
        
        row.addButton = addObj.AddComponent<Button>();
        Image addBg = addObj.AddComponent<Image>();
        addBg.color = UIHelper.ButtonColor;
        
        RectTransform addRect = addObj.GetComponent<RectTransform>();
        addRect.anchorMin = new Vector2(1, 0.5f);
        addRect.anchorMax = new Vector2(1, 0.5f);
        addRect.pivot = new Vector2(1, 0.5f);
        float addButtonSize = Mathf.Max(40f * rowUIScale, 32f);
        addRect.sizeDelta = new Vector2(addButtonSize, addButtonSize);
        float addButtonOffsetX = -10f * rowUIScale;
        addRect.anchoredPosition = new Vector2(addButtonOffsetX, 0);
        
        GameObject addTextObj = new GameObject("AddText");
        addTextObj.transform.SetParent(addObj.transform, false);
        Text addText = addTextObj.AddComponent<Text>();
        addText.text = "+";
        addText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 26 to 30 for better readability
        float addFontSize = Mathf.Max(30f * rowUIScale, 26f);
        addText.fontSize = Mathf.RoundToInt(addFontSize);
        addText.fontStyle = FontStyle.Bold;
        addText.color = Color.white;
        addText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform addTextRect = addTextObj.GetComponent<RectTransform>();
        addTextRect.anchorMin = Vector2.zero;
        addTextRect.anchorMax = Vector2.one;
        addTextRect.sizeDelta = Vector2.zero;
        
        // Button colors
        ColorBlock addColors = row.addButton.colors;
        addColors.normalColor = UIHelper.ButtonColor;
        addColors.highlightedColor = new Color(UIHelper.ButtonColor.r * 1.3f, UIHelper.ButtonColor.g * 1.3f, UIHelper.ButtonColor.b * 1.3f, 1f);
        addColors.pressedColor = new Color(UIHelper.ButtonColor.r * 0.7f, UIHelper.ButtonColor.g * 0.7f, UIHelper.ButtonColor.b * 0.7f, 1f);
        addColors.disabledColor = new Color(0.15f, 0.15f, 0.2f, 0.5f);
        row.addButton.colors = addColors;
        
        // Button click handler
        StatsSystem.StatType capturedStat = stat;
        row.addButton.onClick.AddListener(delegate { OnAddStatClicked(capturedStat); });
        
        statRows[stat] = row;
    }
    
    private void CreateFooter()
    {
        // Footer container
        GameObject footer = new GameObject("Footer");
        footer.transform.SetParent(mainPanel.transform, false);
        
        RectTransform footerRect = footer.AddComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0, 0);
        footerRect.anchorMax = new Vector2(1, 0);
        footerRect.pivot = new Vector2(0.5f, 0);
        footerRect.sizeDelta = new Vector2(-20, 45);
        footerRect.anchoredPosition = new Vector2(0, 8);
        
        // Reset button (left side)
        resetButton = UIHelper.CreateButton(footer.transform, Localization.StatsReset, new Vector2(130, 35));
        RectTransform resetRect = resetButton.GetComponent<RectTransform>();
        resetRect.anchorMin = new Vector2(0, 0.5f);
        resetRect.anchorMax = new Vector2(0, 0.5f);
        resetRect.pivot = new Vector2(0, 0.5f);
        resetRect.anchoredPosition = new Vector2(10, 0);
        
        // Change reset button color to reddish
        Image resetImg = resetButton.GetComponent<Image>();
        if (resetImg != null)
        {
            resetImg.color = new Color(0.45f, 0.25f, 0.25f, 1f);
        }
        ColorBlock resetColors = resetButton.colors;
        resetColors.normalColor = new Color(0.45f, 0.25f, 0.25f, 1f);
        resetColors.highlightedColor = new Color(0.55f, 0.3f, 0.3f, 1f);
        resetColors.pressedColor = new Color(0.35f, 0.2f, 0.2f, 1f);
        resetButton.colors = resetColors;
        
        resetButton.onClick.AddListener(OnResetClicked);
        
        // Hint text (right side)
        GameObject hintObj = new GameObject("Hint");
        hintObj.transform.SetParent(footer.transform, false);
        
        Text hintText = hintObj.AddComponent<Text>();
        hintText.text = Localization.StatsPressToClose;
        hintText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        hintText.fontSize = 11;
        hintText.color = new Color(0.5f, 0.6f, 0.7f, 1f);
        hintText.alignment = TextAnchor.MiddleRight;
        
        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0);
        hintRect.anchorMax = new Vector2(1, 1);
        hintRect.pivot = new Vector2(1, 0.5f);
        hintRect.offsetMin = new Vector2(0, 0);
        hintRect.offsetMax = new Vector2(-10, 0);
    }
    
    // ============================================================
    // UI LOGIC
    // ============================================================
    
    private void OnAddStatClicked(StatsSystem.StatType stat)
    {
        if (statsSystem == null) return;
        
        bool success = statsSystem.AllocatePoint(stat);
        if (success)
        {
            // Refresh immediately
            RefreshUI();
        }
    }
    
    private void OnResetClicked()
    {
        if (statsSystem == null) return;
        
        // Show confirmation dialog
        ShowResetConfirmation();
    }
    
    private void ShowResetConfirmation()
    {
        // Create confirmation dialog using game style
        GameObject dialogCanvas = new GameObject("ResetConfirmCanvas");
        Canvas dc = dialogCanvas.AddComponent<Canvas>();
        dc.renderMode = RenderMode.ScreenSpaceOverlay;
        dc.sortingOrder = 9999;
        
        CanvasScaler scaler = dialogCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        dialogCanvas.AddComponent<GraphicRaycaster>();
        
        // Background overlay
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(dialogCanvas.transform, false);
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.6f);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        
        // Dialog panel using game style
        GameObject dialog = UIHelper.CreateWindowPanel(dialogCanvas.transform, new Vector2(380, 160), "ConfirmDialog");
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Message
        GameObject msgObj = new GameObject("Message");
        msgObj.transform.SetParent(dialog.transform, false);
        Text msgText = msgObj.AddComponent<Text>();
        msgText.text = Localization.StatsResetConfirm;
        msgText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        msgText.fontSize = 15;
        msgText.color = new Color(0.9f, 0.9f, 1f, 1f);
        msgText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform msgRect = msgObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0, 0.5f);
        msgRect.anchorMax = new Vector2(1, 1);
        msgRect.offsetMin = new Vector2(20, 10);
        msgRect.offsetMax = new Vector2(-20, -15);
        
        // Buttons container
        GameObject buttonsObj = new GameObject("Buttons");
        buttonsObj.transform.SetParent(dialog.transform, false);
        HorizontalLayoutGroup buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
        buttonsLayout.spacing = 20;
        buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonsLayout.childForceExpandWidth = false;
        buttonsLayout.childForceExpandHeight = false;
        
        RectTransform buttonsRect = buttonsObj.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0, 0);
        buttonsRect.anchorMax = new Vector2(1, 0.5f);
        buttonsRect.offsetMin = new Vector2(20, 15);
        buttonsRect.offsetMax = new Vector2(-20, -5);
        
        // Confirm button - create manually to ensure proper sizing
        GameObject confirmObj = new GameObject("ConfirmButton");
        confirmObj.transform.SetParent(buttonsObj.transform, false);
        
        Button confirmBtn = confirmObj.AddComponent<Button>();
        Image confirmImg = confirmObj.AddComponent<Image>();
        confirmImg.color = new Color(0.2f, 0.45f, 0.3f, 1f);
        
        LayoutElement confirmLayout = confirmObj.AddComponent<LayoutElement>();
        confirmLayout.preferredWidth = 110;
        confirmLayout.preferredHeight = 35;
        confirmLayout.minWidth = 110;
        confirmLayout.minHeight = 35;
        
        GameObject confirmTextObj = new GameObject("Text");
        confirmTextObj.transform.SetParent(confirmObj.transform, false);
        Text confirmText = confirmTextObj.AddComponent<Text>();
        confirmText.text = Localization.Confirm;
        confirmText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        confirmText.fontSize = 14;
        confirmText.color = Color.white;
        confirmText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform confirmTextRect = confirmTextObj.GetComponent<RectTransform>();
        confirmTextRect.anchorMin = Vector2.zero;
        confirmTextRect.anchorMax = Vector2.one;
        confirmTextRect.sizeDelta = Vector2.zero;
        
        ColorBlock confirmColors = confirmBtn.colors;
        confirmColors.normalColor = new Color(0.2f, 0.45f, 0.3f, 1f);
        confirmColors.highlightedColor = new Color(0.25f, 0.55f, 0.35f, 1f);
        confirmColors.pressedColor = new Color(0.15f, 0.35f, 0.2f, 1f);
        confirmBtn.colors = confirmColors;
        
        confirmBtn.onClick.AddListener(delegate {
            statsSystem.ResetStats();
            Destroy(dialogCanvas);
            RefreshUI();
        });
        
        // Cancel button - create manually to ensure proper sizing
        GameObject cancelObj = new GameObject("CancelButton");
        cancelObj.transform.SetParent(buttonsObj.transform, false);
        
        Button cancelBtn = cancelObj.AddComponent<Button>();
        Image cancelImg = cancelObj.AddComponent<Image>();
        cancelImg.color = new Color(0.45f, 0.25f, 0.25f, 1f);
        
        LayoutElement cancelLayout = cancelObj.AddComponent<LayoutElement>();
        cancelLayout.preferredWidth = 110;
        cancelLayout.preferredHeight = 35;
        cancelLayout.minWidth = 110;
        cancelLayout.minHeight = 35;
        
        GameObject cancelTextObj = new GameObject("Text");
        cancelTextObj.transform.SetParent(cancelObj.transform, false);
        Text cancelText = cancelTextObj.AddComponent<Text>();
        cancelText.text = Localization.Cancel;
        cancelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cancelText.fontSize = 14;
        cancelText.color = Color.white;
        cancelText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform cancelTextRect = cancelTextObj.GetComponent<RectTransform>();
        cancelTextRect.anchorMin = Vector2.zero;
        cancelTextRect.anchorMax = Vector2.one;
        cancelTextRect.sizeDelta = Vector2.zero;
        
        ColorBlock cancelColors = cancelBtn.colors;
        cancelColors.normalColor = new Color(0.45f, 0.25f, 0.25f, 1f);
        cancelColors.highlightedColor = new Color(0.55f, 0.3f, 0.3f, 1f);
        cancelColors.pressedColor = new Color(0.35f, 0.15f, 0.15f, 1f);
        cancelBtn.colors = cancelColors;
        
        cancelBtn.onClick.AddListener(delegate {
            Destroy(dialogCanvas);
        });
    }
    
    private void OnPointsGained(int points)
    {
        // Could show a notification here
        RPGLog.Debug(string.Format(" Gained {0} stat points!", points));
    }
    
    public void RefreshUI()
    {
        if (statsSystem == null) return;
        
        // Update available points
        int available = statsSystem.GetAvailablePoints();
        availablePointsText.text = string.Format(Localization.StatsAvailablePoints, available);
        
        // Update each stat row
        foreach (var kvp in statRows)
        {
            StatsSystem.StatType stat = kvp.Key;
            StatRow row = kvp.Value;
            
            int points = statsSystem.GetStatPoints(stat);
            row.valueText.text = points.ToString();
            row.bonusText.text = statsSystem.GetStatBonusDescription(stat);
            
            // Enable/disable add button based on available points
            row.addButton.interactable = available > 0;
        }
        
        // Update reset button (only enable if points have been allocated)
        resetButton.interactable = statsSystem.GetTotalAllocatedPoints() > 0;
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
