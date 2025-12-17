
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;

/// <summary>
/// Inventory UI - Creates and manages the inventory window using UGUI
/// Layout: CHARACTER (left) | BACKPACK (right)
/// Consumable bar is separate and always visible
/// </summary>
public class InventoryUI : MonoBehaviour
{
    private InventoryManager inventoryManager;
    private EquipmentManager equipmentManager;
    private ConsumableBar consumableBar;
    private string modPath;
    private UIStyle uiStyle = UIStyle.Classic;

    // UI Components
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform mainPanel;
    private RectTransform characterPanel;
    private RectTransform backpackPanel;
    private List<ItemSlot> itemSlots = new List<ItemSlot>();
    private List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();
    private GameObject tooltipPanel;
    private Text tooltipText;
    private Text statsText;
    private Text setBonusText;
    private RectTransform setBonusPanel;
    private GameObject sellButton;
    private GameObject upgradeButton;
    private int tooltipItemSlotIndex = -1; // Track which item slot the tooltip is showing
    private int hoveredSlotIndex = -1; // Track currently hovered slot for hotkey
    private EquipmentSlotType? tooltipEquipmentSlot = null; // Track which equipment slot the tooltip is showing
    
    // Hold-to-confirm progress (like dismantling)
    private float sellProgress = 0f;
    private float upgradeProgress = 0f;
    private float dismantleProgress = 0f;
    private float cleanseProgress = 0f;
    private float lastSellTapTime = 0f;
    private float lastUpgradeTapTime = 0f;
    private float lastDismantleTapTime = 0f;
    private float lastCleanseTapTime = 0f;
    private bool isSelling = false;
    private bool isUpgrading = false;
    private bool isDismantling = false;
    private bool isCleansing = false;
    private bool _ctrlDismantleTriggered = false; // Prevents multiple instant dismantles per key press
    private bool _ctrlSellTriggered = false; // Prevents multiple instant sells per key press
    private GameObject sellProgressBar;
    private GameObject upgradeProgressBar;
    private GameObject dismantleProgressBar;
    private GameObject cleanseProgressBar;
    private Image sellProgressFill;
    private Image upgradeProgressFill;
    private Image dismantleProgressFill;
    private Image cleanseProgressFill;
    private float _sellFillCv;
    private float _upgradeFillCv;
    private float _dismantleFillCv;
    private float _cleanseFillCv;
    
    // Constants (same as dismantling)
    private const float ActionTapMinInterval = 0.075f;
    private const float ActionDecayStartTime = 1f;
    private const float ActionTapStrength = 0.4f;
    
    // Sound effects (cached from game) - using dismantle tap for all sounds
    private GameObject fxDismantleTap = null;

    // Background overlay (for blocking game input and click-to-close)
    private Image backgroundOverlay;
    
    // Screen resolution tracking for dynamic UI scaling
    private Vector2 _lastScreenSize;
    private float _lastGameUIScale = 1f;
    private float _lastModUIScale = 1f;

    // Auto pickup and auto merge toggles
    private bool autoPickupEnabled = false;
    private bool autoMergeEnabled = false;
    private Button autoPickupButton;
    private Button autoMergeButton;
    private float lastAutoPickupCheck = 0f;
    private float lastAutoMergeCheck = 0f;
    private const float AUTO_PICKUP_CHECK_INTERVAL = 0.2f; // Check every 200ms
    private const float AUTO_MERGE_CHECK_INTERVAL = 0.5f; // Check every 500ms
    
    // Track pending pickups to prevent race conditions in multiplayer
    private HashSet<uint> _pendingPickups = new HashSet<uint>();
    private Dictionary<uint, float> _pickupCooldowns = new Dictionary<uint, float>();
    private const float PICKUP_COOLDOWN = 2f; // Prevent re-picking same item for 2 seconds

    // Grid settings
    private int backpackColumns = 5;
    private int initialRows = 6;
    private float slotSize = 50f;
    private float slotSpacing = 4f;
    private float equipSlotSize = 45f;
    
    // Scrollable backpack
    private ScrollRect backpackScrollRect;
    private RectTransform backpackGridRect;
    private GameObject backpackGridObj;

    // Drag & Drop
    private ItemSlot draggedSlot = null;
    private EquipmentSlot draggedEquipSlot = null;
    private FastSlotUI draggedFastSlot = null;
    private bool isDragging = false;
    private bool isDraggingFromFastSlot = false;
    private GameObject dragVisual = null;
    
    public ItemSlot DraggedSlot { get { return draggedSlot; } }
    public EquipmentSlot DraggedEquipSlot { get { return draggedEquipSlot; } }
    public FastSlotUI DraggedFastSlot { get { return draggedFastSlot; } }
    public bool IsDragging { get { return isDragging; } }
    public bool IsDraggingFromFastSlot { get { return isDraggingFromFastSlot; } }
    
    // Colors
    private Color accentColor = new Color(0.8f, 0.2f, 0.3f, 1f);

    public Canvas GetCanvas() { return canvas; }
    public EquipmentManager GetEquipmentManager() { return equipmentManager; }
    public InventoryManager GetInventoryManager() { return inventoryManager; }
    
    // Callback when equipment changes (for auto target system)
    public Action OnEquipmentChanged;
    
    public void SetConsumableBar(ConsumableBar bar)
    {
        consumableBar = bar;
    }
    
    /// <summary>
    /// Notify that equipment has changed (triggers auto target update)
    /// </summary>
    private void NotifyEquipmentChanged()
    {
        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged();
        }
    }

    public void Initialize(InventoryManager manager, EquipmentManager eqManager, string path, UIStyle style = UIStyle.Classic)
    {
        inventoryManager = manager;
        equipmentManager = eqManager;
        modPath = path;
        uiStyle = style;

        // Initialize screen size tracking
        _lastScreenSize = new Vector2(Screen.width, Screen.height);
        _lastGameUIScale = GetGameUIScale();
        _lastModUIScale = RPGItemsMod.GetModUIScaleOnly();
        
        // Initialize UI helper with game assets
        UIHelper.Initialize();
        
        CreateCanvas();
        CreateMainLayout();
        CreateTooltip();
        SetVisible(false);
        
        // Load sound effects
        LoadSoundEffects();
        
        RefreshInventory();
        // UI initialized
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
        // Unsubscribe from inventory events
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryExpanded -= OnInventoryExpanded;
            inventoryManager.OnInventoryReset -= OnInventoryReset;
            inventoryManager.OnItemAdded -= OnItemAdded;
        }
    }

    private void CreateCanvas()
    {
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("RPGItemsMod_EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(eventSystemObj);
        }

        GameObject canvasObj = new GameObject("RPGItemsMod_Canvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1; // Minimum so game tooltips appear on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
    }

    private void CreateMainLayout()
    {
        // Dark overlay background - can block game input when inventory is open (configurable)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvas.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        backgroundOverlay = bgObj.AddComponent<Image>();
        backgroundOverlay.color = new Color(0f, 0f, 0f, 0.6f);
        
        // Check config for whether to block movement when inventory is open
        bool blockMovement = false; // Default to NOT blocking (allows movement while inventory open)
        if (RPGItemsMod.Instance != null && RPGItemsMod.Instance.config != null)
        {
            blockMovement = RPGItemsMod.Instance.config.blockMovementWhenInventoryOpen;
        }
        backgroundOverlay.raycastTarget = blockMovement;
        
        // Add click handler to close inventory when clicking background (only if blocking is enabled)
        if (blockMovement)
        {
            // Use EventTrigger to handle both left and right click
            EventTrigger trigger = bgObj.AddComponent<EventTrigger>();
            
            // Left click to close
            EventTrigger.Entry leftClickEntry = new EventTrigger.Entry();
            leftClickEntry.eventID = EventTriggerType.PointerClick;
            leftClickEntry.callback.AddListener((data) => {
                PointerEventData pointerData = (PointerEventData)data;
                if (pointerData.button == PointerEventData.InputButton.Left || 
                    pointerData.button == PointerEventData.InputButton.Right)
                {
                    SetVisible(false);
                }
            });
            trigger.triggers.Add(leftClickEntry);
        }

        // Create a container for all windows (not visible, just for organization)
        GameObject containerObj = new GameObject("WindowContainer");
        containerObj.transform.SetParent(canvas.transform, false);
        mainPanel = containerObj.AddComponent<RectTransform>();
        mainPanel.anchorMin = new Vector2(0.5f, 0.5f);
        mainPanel.anchorMax = new Vector2(0.5f, 0.5f);
        mainPanel.pivot = new Vector2(0.5f, 0.5f);
        mainPanel.anchoredPosition = Vector2.zero;
        
        // Choose layout based on UI style
        if (uiStyle == UIStyle.DiabloStyle)
        {
            // Full screen width for Diablo-style layout - anchor to screen edges
            mainPanel.sizeDelta = Vector2.zero; // Use full screen
            mainPanel.anchorMin = Vector2.zero;
            mainPanel.anchorMax = Vector2.one;
            mainPanel.pivot = new Vector2(0.5f, 0.5f);
            mainPanel.anchoredPosition = Vector2.zero;
            CreateDiabloStyleLayout();
        }
        else
        {
            mainPanel.sizeDelta = new Vector2(900, 650); // Classic layout size
            CreateClassicLayout();
        }
        
        CreateSetBonusPanel(); // Set bonus panel works for both layouts
        
        // Apply UI scaling from config
        ApplyUIScale();
    }
    
    /// <summary>
    /// Apply UI scale from config to all window panels
    /// </summary>
    private void ApplyUIScale()
    {
        float scale = RPGItemsMod.GetUIScale();
        
        // Apply scale to main container
        if (mainPanel != null)
        {
            mainPanel.localScale = new Vector3(scale, scale, 1f);
        }
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
            RPGLog.Debug("Screen resolution changed to " + Screen.width + "x" + Screen.height);
        }
        
        // Check if game UI scale changed
        if (Mathf.Abs(_lastGameUIScale - currentGameUIScale) > 0.001f)
        {
            _lastGameUIScale = currentGameUIScale;
            needsUpdate = true;
            RPGLog.Debug("Game UI scale changed to " + currentGameUIScale);
        }
        
        // Check if mod UI scale changed
        if (Mathf.Abs(_lastModUIScale - currentModUIScale) > 0.001f)
        {
            _lastModUIScale = currentModUIScale;
            needsUpdate = true;
            RPGLog.Debug("Mod UI scale changed to " + currentModUIScale);
        }
        
        if (needsUpdate)
        {
            ApplyUIScale();
        }
    }
    
    /// <summary>
    /// Create classic layout: Equipment left, Inventory right
    /// </summary>
    private void CreateClassicLayout()
    {
        CreateCharacterWindow();
        CreateBackpackWindow();
        CreateFastSlotsWindow();
    }
    
    /// <summary>
    /// Create Diablo-style layout: Single panel with Stats+Equipment at top, Inventory below, Consumables at bottom
    /// </summary>
    private void CreateDiabloStyleLayout()
    {
        CreateDiabloMainPanel();
    }
    
    /// <summary>
    /// Create main panel for Diablo-style layout
    /// </summary>
    private void CreateDiabloMainPanel()
    {
        // Main panel - positioned at right side of screen
        Vector2 windowSize = new Vector2(600, 780);
        GameObject windowObj = UIHelper.CreateWindowPanel(canvas.transform, windowSize, "DiabloMainPanel");
        backpackPanel = windowObj.GetComponent<RectTransform>();
        
        // Position at right side of screen
        backpackPanel.anchorMin = new Vector2(1, 0.5f);
        backpackPanel.anchorMax = new Vector2(1, 0.5f);
        backpackPanel.pivot = new Vector2(1, 0.5f);
        backpackPanel.anchoredPosition = new Vector2(-20, 0);
        
        // Set backpack columns for Diablo style (10 columns)
        backpackColumns = 10;
        
        // Window title
        CreateWindowTitle(backpackPanel, Localization.Backpack, new Vector2(0, -15));
        
        // TOP SECTION: Stats (left) and Equipment (right) side by side
        float topSectionHeight = 380f;
        float topSectionY = -50f;
        
        // Stats panel (left half of top section) - INSIDE the main panel
        CreateDiabloStatsPanel(backpackPanel, topSectionY, topSectionHeight);
        
        // Equipment panel (right half of top section)
        CreateDiabloEquipmentPanel(backpackPanel, topSectionY, topSectionHeight);
        
        // BUTTON ROW: Between equipment and inventory (in its own panel)
        float buttonRowHeight = 25f;
        float buttonRowY = topSectionY - topSectionHeight - 5f; // 5px below equipment panel
        CreateDiabloButtonRow(backpackPanel, buttonRowY, buttonRowHeight);
        
        // MIDDLE SECTION: Inventory grid (4 rows, no scrollbar until needed)
        float inventoryY = buttonRowY - buttonRowHeight - 5f; // 5px below button row
        CreateDiabloInventoryGrid(backpackPanel, inventoryY);
        
        // BOTTOM SECTION: Consumables (centered) with Settings button
        CreateDiabloConsumableSlots(backpackPanel);
    }
    
    /// <summary>
    /// Create set bonuses panel for Diablo-style (left side of top section)
    /// Shows set bonuses content with a STATS button at the bottom
    /// </summary>
    private void CreateDiabloStatsPanel(RectTransform parentPanel, float yPos, float height)
    {
        GameObject panelObj = new GameObject("SetBonusesPanel");
        panelObj.transform.SetParent(parentPanel, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        
        // Anchor to left 40% of parent, below title
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0.4f, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.offsetMin = new Vector2(25, yPos - height);
        panelRect.offsetMax = new Vector2(-5, yPos);
        
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
        
        // SET BONUSES title
        GameObject titleObj = new GameObject("SetBonusesTitle");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.SetBonuses;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 12 to 14 for better readability
        float uiScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        float titleFontSize = Mathf.Max(14f * uiScale, 12f);
        titleText.fontSize = Mathf.RoundToInt(titleFontSize);
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = accentColor;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 20);
        titleRect.anchoredPosition = new Vector2(0, -5);
        
        // Scroll view for set bonuses (in case player has many different tier bonuses)
        GameObject scrollViewObj = new GameObject("SetBonusesScrollView");
        scrollViewObj.transform.SetParent(panelObj.transform, false);
        RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0, 0);
        scrollViewRect.anchorMax = new Vector2(1, 1);
        scrollViewRect.offsetMin = new Vector2(10, 40); // More left padding
        scrollViewRect.offsetMax = new Vector2(-10, -30); // More right padding and top for title
        
        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        
        Image viewportMask = viewportObj.AddComponent<Image>();
        viewportMask.color = new Color(0, 0, 0, 0.01f);
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        scrollRect.viewport = viewportRect;
        
        // Content container
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);
        
        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.content = contentRect;
        
        // Set bonuses text - full width, no horizontal truncation
        GameObject bonusTextObj = new GameObject("SetBonusesText");
        bonusTextObj.transform.SetParent(contentObj.transform, false);
        diabloSetBonusText = bonusTextObj.AddComponent<Text>();
        diabloSetBonusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 10 to 12 for better readability
        float bonusTextFontSize = Mathf.Max(12f * uiScale, 10f);
        diabloSetBonusText.fontSize = Mathf.RoundToInt(bonusTextFontSize);
        diabloSetBonusText.color = Color.white;
        diabloSetBonusText.alignment = TextAnchor.UpperLeft;
        diabloSetBonusText.supportRichText = true;
        diabloSetBonusText.text = Localization.NoSetBonuses;
        diabloSetBonusText.horizontalOverflow = HorizontalWrapMode.Wrap; // Wrap text instead of cutting off
        diabloSetBonusText.verticalOverflow = VerticalWrapMode.Overflow;
        
        RectTransform bonusTextRect = diabloSetBonusText.GetComponent<RectTransform>();
        bonusTextRect.anchorMin = new Vector2(0, 1);
        bonusTextRect.anchorMax = new Vector2(1, 1);
        bonusTextRect.pivot = new Vector2(0, 1);
        bonusTextRect.anchoredPosition = Vector2.zero;
        bonusTextRect.sizeDelta = new Vector2(0, 0);
        
        // Add layout element for proper sizing
        LayoutElement textLayout = bonusTextObj.AddComponent<LayoutElement>();
        textLayout.flexibleWidth = 1;
        
        // STATS button at bottom of panel
        GameObject statsButtonObj = new GameObject("StatsButton");
        statsButtonObj.transform.SetParent(panelObj.transform, false);
        RectTransform buttonRect = statsButtonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 0);
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.offsetMin = new Vector2(10, 5);
        buttonRect.offsetMax = new Vector2(-10, 30);
        
        Image buttonBg = statsButtonObj.AddComponent<Image>();
        buttonBg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
        
        Button button = statsButtonObj.AddComponent<Button>();
        button.targetGraphic = buttonBg;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        button.colors = colors;
        
        // Button text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(statsButtonObj.transform, false);
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = Localization.Stats.ToUpper();
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 10;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform buttonTextRect = buttonText.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        // Button click handler - toggle stats popup
        button.onClick.AddListener(() => {
            if (diabloStatsPopup != null)
            {
                diabloStatsPopup.SetActive(!diabloStatsPopup.activeSelf);
            }
        });
        
        // Create the stats popup (hidden by default)
        CreateDiabloStatsPopup(parentPanel);
    }
    
    // Reference to Diablo-style set bonus text in the main panel
    private Text diabloSetBonusText;
    // Reference to Diablo-style stats popup
    private GameObject diabloStatsPopup;
    
    /// <summary>
    /// Create stats popup for Diablo-style layout (auto-adapts to content size)
    /// </summary>
    private void CreateDiabloStatsPopup(RectTransform parentPanel)
    {
        // Calculate UI scale for popup elements
        float popupUIScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        
        // Create popup panel with auto-sizing
        GameObject popupObj = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(250, 100), "StatsPopup");
        diabloStatsPopup = popupObj;
        
        RectTransform popupRect = popupObj.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(1, 0.5f);
        popupRect.anchorMax = new Vector2(1, 0.5f);
        popupRect.pivot = new Vector2(1, 0.5f);
        // Position to the left of the main inventory panel
        popupRect.anchoredPosition = new Vector2(-640, 0);
        
        // Add ContentSizeFitter for auto-sizing
        ContentSizeFitter popupFitter = popupObj.AddComponent<ContentSizeFitter>();
        popupFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        popupFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Add VerticalLayoutGroup for proper layout with larger padding for window borders
        VerticalLayoutGroup popupLayout = popupObj.AddComponent<VerticalLayoutGroup>();
        popupLayout.padding = new RectOffset(25, 25, 30, 25); // More padding for window border style
        popupLayout.spacing = 5;
        popupLayout.childAlignment = TextAnchor.UpperCenter;
        popupLayout.childControlWidth = true;
        popupLayout.childControlHeight = true;
        popupLayout.childForceExpandWidth = true;
        popupLayout.childForceExpandHeight = false;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(popupObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.Stats;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 14;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = accentColor;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.minHeight = 25;
        titleLayout.preferredHeight = 25;
        titleLayout.preferredWidth = 200;
        
        // Stats text
        GameObject statsTextObj = new GameObject("StatsText");
        statsTextObj.transform.SetParent(popupObj.transform, false);
        statsText = statsTextObj.AddComponent<Text>();
        statsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 11 to 13 for better readability
        float statsTextFontSize = Mathf.Max(13f * popupUIScale, 11f);
        statsText.fontSize = Mathf.RoundToInt(statsTextFontSize);
        statsText.color = Color.white;
        statsText.alignment = TextAnchor.UpperLeft;
        statsText.supportRichText = true;
        statsText.text = Localization.Loading;
        statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        statsText.verticalOverflow = VerticalWrapMode.Overflow;
        
        LayoutElement statsLayout = statsTextObj.AddComponent<LayoutElement>();
        statsLayout.preferredWidth = 200;
        statsLayout.flexibleWidth = 0;
        
        // Hidden by default
        diabloStatsPopup.SetActive(false);
    }
    
    /// <summary>
    /// Create equipment panel for Diablo-style (right side of top section)
    /// </summary>
    private void CreateDiabloEquipmentPanel(RectTransform parentPanel, float yPos, float height)
    {
        GameObject equipmentObj = new GameObject("EquipmentPanel");
        equipmentObj.transform.SetParent(parentPanel, false);
        RectTransform equipmentRect = equipmentObj.AddComponent<RectTransform>();
        
        // Anchor to right 60% of parent, below title
        equipmentRect.anchorMin = new Vector2(0.4f, 1);
        equipmentRect.anchorMax = new Vector2(1, 1);
        equipmentRect.pivot = new Vector2(0, 1);
        equipmentRect.offsetMin = new Vector2(5, yPos - height);
        equipmentRect.offsetMax = new Vector2(-25, yPos);
        
        Image equipmentBg = equipmentObj.AddComponent<Image>();
        equipmentBg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
        
        // Equipment layout - humanoid shape
        float centerX = 165f;
        float sideOffset = 55f;
        float startY = -35f;
        float rowSpacing = 58f; // Increased spacing to prevent label overlap
        
        // Row 0: HEAD (center)
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.Head, new Vector2(centerX, startY), Localization.HeadSlotLabel);
        
        // Row 1: AMULET (center)
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.Amulet, new Vector2(centerX, startY - rowSpacing), Localization.AmuletSlotLabel);
        
        // Row 2: L.HAND, CHEST, R.HAND
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.LeftHand, new Vector2(centerX - sideOffset, startY - rowSpacing * 2), Localization.LeftHandLabel);
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.Chest, new Vector2(centerX, startY - rowSpacing * 2), Localization.ChestSlotLabel);
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.RightHand, new Vector2(centerX + sideOffset, startY - rowSpacing * 2), Localization.RightHandLabel);
        
        // Row 3: L.RING, BELT, R.RING
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.LeftRing, new Vector2(centerX - sideOffset, startY - rowSpacing * 3), Localization.LeftRingLabel);
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.Belt, new Vector2(centerX, startY - rowSpacing * 3), Localization.BeltSlotLabel);
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.RightRing, new Vector2(centerX + sideOffset, startY - rowSpacing * 3), Localization.RightRingLabel);
        
        // Row 4: LEGS (center)
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.Legs, new Vector2(centerX, startY - rowSpacing * 4), Localization.LegsSlotLabel);
        
        // Row 5: BOOTS (center)
        CreateEquipmentSlotAt(equipmentRect, EquipmentSlotType.Boots, new Vector2(centerX, startY - rowSpacing * 5), Localization.BootsSlotLabel);
    }
    
    /// <summary>
    /// Create inventory grid for Diablo-style (middle section) - 4 rows, no scrollbar until full
    /// </summary>
    private RectTransform CreateDiabloInventoryGrid(RectTransform parentPanel, float yPos)
    {
        // Calculate height for 4 rows
        float inventoryHeight = 4 * (slotSize + slotSpacing) + 20;
        
        GameObject scrollViewObj = new GameObject("InventoryScrollView");
        scrollViewObj.transform.SetParent(parentPanel, false);
        RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0, 1);
        scrollViewRect.anchorMax = new Vector2(1, 1);
        scrollViewRect.pivot = new Vector2(0.5f, 1);
        scrollViewRect.offsetMin = new Vector2(25, yPos - inventoryHeight);
        scrollViewRect.offsetMax = new Vector2(-25, yPos);
        
        Image scrollBg = scrollViewObj.AddComponent<Image>();
        scrollBg.color = new Color(0.03f, 0.03f, 0.05f, 0.95f);
        
        backpackScrollRect = scrollViewObj.AddComponent<ScrollRect>();
        backpackScrollRect.horizontal = false;
        backpackScrollRect.vertical = true;
        backpackScrollRect.movementType = ScrollRect.MovementType.Clamped;
        backpackScrollRect.scrollSensitivity = 30f;
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(5, 5);
        viewportRect.offsetMax = new Vector2(-5, -5);
        
        Image viewportMask = viewportObj.AddComponent<Image>();
        viewportMask.color = new Color(0, 0, 0, 0.01f);
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        backpackScrollRect.viewport = viewportRect;
        
        // Content container
        backpackGridObj = new GameObject("BackpackGrid");
        backpackGridObj.transform.SetParent(viewportObj.transform, false);
        backpackGridRect = backpackGridObj.AddComponent<RectTransform>();
        backpackGridRect.anchorMin = new Vector2(0, 1);
        backpackGridRect.anchorMax = new Vector2(1, 1);
        backpackGridRect.pivot = new Vector2(0.5f, 1);
        backpackGridRect.anchoredPosition = Vector2.zero;
        
        ContentSizeFitter contentFitter = backpackGridObj.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GridLayoutGroup gridLayout = backpackGridObj.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(slotSize, slotSize);
        gridLayout.spacing = new Vector2(slotSpacing, slotSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = backpackColumns;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.padding = new RectOffset(5, 5, 5, 5);
        
        backpackScrollRect.content = backpackGridRect;
        
        // Create slots for 4 rows minimum (4 rows * 10 columns = 40 slots)
        // All slots are inventory slots now (sort button is in button row above)
        int minSlots = 4 * backpackColumns; // 40 slots for 4 rows
        
        RPGLog.Debug(string.Format(" Diablo Grid: Creating {0} inventory slots, current count: {1}", 
            minSlots, inventoryManager.SlotCount));
        
        // Ensure inventory manager has enough slots for Diablo layout
        while (inventoryManager.SlotCount < minSlots)
        {
            inventoryManager.Items.Add(null);
            RPGLog.Debug(string.Format(" Expanded inventory to {0} slots", inventoryManager.SlotCount));
        }
        
        RPGLog.Debug(string.Format(" Final inventory slot count: {0}", inventoryManager.SlotCount));
        
        // Create ALL inventory slots for 4 rows
        for (int i = 0; i < minSlots; i++)
        {
            CreateItemSlot(i);
        }
        
        RPGLog.Debug(string.Format(" Created {0} item slots", minSlots));
        
        // No scrollbar initially - it will appear when content exceeds viewport
        backpackScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        
        // Subscribe to inventory events (for Diablo layout)
        inventoryManager.OnInventoryExpanded += OnInventoryExpanded;
        inventoryManager.OnInventoryReset += OnInventoryReset;
        inventoryManager.OnItemAdded += OnItemAdded;
        
        return scrollViewRect;
    }
    
    /// <summary>
    /// Create consumable slots for Diablo-style (bottom section, centered) with Settings button
    /// </summary>
    private void CreateDiabloConsumableSlots(RectTransform parentPanel)
    {
        // Calculate UI scale for consumable slots
        float uiScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        
        // Container at bottom of panel
        GameObject consumableContainer = new GameObject("ConsumableSlots");
        consumableContainer.transform.SetParent(parentPanel, false);
        RectTransform containerRect = consumableContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.offsetMin = new Vector2(25, 25);
        containerRect.offsetMax = new Vector2(-25, 85);
        
        // 4 horizontal consumable slots centered (no extra labels - FastSlotUI already shows hotkey numbers)
        float slotSizeLocal = 50f;
        float spacing = 8f;
        float totalWidth = 4 * slotSizeLocal + 3 * spacing;
        float startX = -totalWidth / 2 + slotSizeLocal / 2;
        
        for (int i = 0; i < 4; i++)
        {
            GameObject slotObj = new GameObject("ConsumableSlot_" + i);
            slotObj.transform.SetParent(consumableContainer.transform, false);
            
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(slotSizeLocal, slotSizeLocal);
            slotRect.anchoredPosition = new Vector2(startX + i * (slotSizeLocal + spacing), 0);
            
            // FastSlotUI component (already has hotkey number inside)
            FastSlotUI slot = slotObj.AddComponent<FastSlotUI>();
            slot.Initialize(i, this, consumableBar);
            
            // Ensure the slot GameObject can receive drops (FastSlotUI's Image should handle this, but ensure it's set up)
            // The Initialize() method adds an Image with raycastTarget = true, so this should work
        }
        
        // Settings button to the right of consumables
        GameObject btnObj = new GameObject("SettingsButton");
        btnObj.transform.SetParent(consumableContainer.transform, false);
        
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(70, 28);
        btnRect.anchoredPosition = new Vector2(totalWidth / 2 + 50, 0);
        
        Button btn = btnObj.AddComponent<Button>();
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = UIHelper.ButtonColor;
        
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.text = Localization.Settings;
        btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 10 to 12 for better readability
        float settingsBtnFontSize = Mathf.Max(12f * uiScale, 10f);
        btnText.fontSize = Mathf.RoundToInt(settingsBtnFontSize);
        btnText.fontStyle = FontStyle.Bold;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform btnTextRect = btnText.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        btnTextRect.anchoredPosition = Vector2.zero;
        
        btn.onClick.AddListener(() => {
            // Open settings window
            SettingsPanel settings = GetComponent<SettingsPanel>();
            if (settings == null)
            {
                settings = transform.parent.GetComponentInChildren<SettingsPanel>();
            }
            if (settings != null)
            {
                settings.SetVisible(!settings.IsVisible());
            }
        });
    }

    /// <summary>
    /// Create the CHARACTER window (left side) - separate game-styled window
    /// </summary>
    private void CreateCharacterWindow()
    {
        // Create window using game's Widget Window prefab
        Vector2 windowSize = new Vector2(250, 580);  // Taller to fit stats
        GameObject windowObj = UIHelper.CreateWindowPanel(mainPanel, windowSize, "CharacterWindow");
        characterPanel = windowObj.GetComponent<RectTransform>();
        characterPanel.anchorMin = new Vector2(0, 0.5f);
        characterPanel.anchorMax = new Vector2(0, 0.5f);
        characterPanel.pivot = new Vector2(0, 0.5f);
        characterPanel.anchoredPosition = new Vector2(0, 0);
        
        // Window title - positioned higher with more padding
        CreateWindowTitle(characterPanel, Localization.Character, new Vector2(0, -15));
        
        // Equipment layout - vertical flow with hands on sides
        // Layout:
        //        HEAD
        //       AMULET
        // L.HAND CHEST R.HAND
        // L.RING BELT  R.RING
        //        LEGS
        //       BOOTS
        
        float centerX = 125f;  // Center of panel
        float sideOffset = 55f; // Distance from center for side slots
        float startY = -90f;   // Start much further below title for HEAD label separation
        float rowSpacing = 58f; // More spacing to account for labels above slots
        
        // Row 0: HEAD (center)
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.Head, new Vector2(centerX, startY), Localization.HeadSlotLabel);
        
        // Row 1: AMULET (center)
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.Amulet, new Vector2(centerX, startY - rowSpacing), Localization.AmuletSlotLabel);
        
        // Row 2: L.HAND, CHEST, R.HAND
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.LeftHand, new Vector2(centerX - sideOffset, startY - rowSpacing * 2), Localization.LeftHandLabel);
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.Chest, new Vector2(centerX, startY - rowSpacing * 2), Localization.ChestSlotLabel);
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.RightHand, new Vector2(centerX + sideOffset, startY - rowSpacing * 2), Localization.RightHandLabel);
        
        // Row 3: L.RING, BELT, R.RING
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.LeftRing, new Vector2(centerX - sideOffset, startY - rowSpacing * 3), Localization.LeftRingLabel);
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.Belt, new Vector2(centerX, startY - rowSpacing * 3), Localization.BeltSlotLabel);
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.RightRing, new Vector2(centerX + sideOffset, startY - rowSpacing * 3), Localization.RightRingLabel);
        
        // Row 4: LEGS (center)
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.Legs, new Vector2(centerX, startY - rowSpacing * 4), Localization.LegsSlotLabel);
        
        // Row 5: BOOTS (center)
        CreateEquipmentSlotAt(characterPanel, EquipmentSlotType.Boots, new Vector2(centerX, startY - rowSpacing * 5), Localization.BootsSlotLabel);
        
        // Stats display at bottom
        CreateStatsDisplay();
    }
    
    /// <summary>
    /// Create a window title with game styling
    /// </summary>
    private void CreateWindowTitle(RectTransform parent, string title, Vector2 position)
    {
        GameObject titleObj = new GameObject("WindowTitle");
        titleObj.transform.SetParent(parent, false);
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = title;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 16 to 18 for better readability
        float windowTitleScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        float windowTitleFontSize = Mathf.Max(18f * windowTitleScale, 16f);
        titleText.fontSize = Mathf.RoundToInt(windowTitleFontSize);
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = UIHelper.AccentColor;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(200, 30);
        titleRect.anchoredPosition = position;
    }

    private void CreateEquipmentSlotAt(RectTransform parent, EquipmentSlotType slotType, Vector2 position, string label)
    {
        GameObject slotObj = new GameObject("EquipmentSlot_" + slotType);
        slotObj.transform.SetParent(parent, false);
        
        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0, 1);
        slotRect.anchorMax = new Vector2(0, 1);
        slotRect.pivot = new Vector2(0.5f, 0.5f);
        slotRect.sizeDelta = new Vector2(equipSlotSize, equipSlotSize);
        slotRect.anchoredPosition = position;
        
        EquipmentSlot slot = slotObj.AddComponent<EquipmentSlot>();
        slot.Initialize(slotType, this, equipmentManager);
        slot.SetLabel(label);
        equipmentSlots.Add(slot);
    }

    /// <summary>
    /// Create the BACKPACK window (center) - separate game-styled window with scrolling
    /// </summary>
    private void CreateBackpackWindow()
    {
        // Calculate UI scale for backpack elements
        float windowTitleScale = RPGItemsMod.GetUIScaleWithScreenFactor();
        
        // Create window using game's Widget Window prefab
        Vector2 windowSize = new Vector2(340, 420);
        GameObject windowObj = UIHelper.CreateWindowPanel(mainPanel, windowSize, "BackpackWindow");
        backpackPanel = windowObj.GetComponent<RectTransform>();
        backpackPanel.anchorMin = new Vector2(0.5f, 0.5f);
        backpackPanel.anchorMax = new Vector2(0.5f, 0.5f);
        backpackPanel.pivot = new Vector2(0.5f, 0.5f);
        backpackPanel.anchoredPosition = new Vector2(30, 50);
        
        // Window title
        CreateWindowTitle(backpackPanel, Localization.Backpack, new Vector2(0, -15));
        
        // Create scroll view container (viewport area)
        float viewportHeight = initialRows * (slotSize + slotSpacing) + 10;
        float viewportWidth = backpackColumns * (slotSize + slotSpacing) + 20; // Extra for scrollbar
        
        // Calculate where the scroll view will be positioned
        float scrollViewY = -10f; // This is the anchoredPosition.y of the scroll view
        float scrollViewTop = scrollViewY + viewportHeight / 2; // Top edge of scroll view
        
        // Create buttons in a row just above the inventory grid (inside the window)
        CreateClassicButtonRow(backpackPanel, scrollViewTop);
        
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(backpackPanel, false);
        RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollViewRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollViewRect.pivot = new Vector2(0.5f, 0.5f);
        scrollViewRect.sizeDelta = new Vector2(viewportWidth, viewportHeight);
        scrollViewRect.anchoredPosition = new Vector2(0, -10);
        
        // Add ScrollRect component
        backpackScrollRect = scrollViewObj.AddComponent<ScrollRect>();
        backpackScrollRect.horizontal = false;
        backpackScrollRect.vertical = true;
        backpackScrollRect.movementType = ScrollRect.MovementType.Clamped;
        backpackScrollRect.scrollSensitivity = 30f;
        
        // Create viewport (mask area)
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = new Vector2(-12, 0); // Leave space for scrollbar
        
        Image viewportMask = viewportObj.AddComponent<Image>();
        viewportMask.color = new Color(0, 0, 0, 0.01f); // Nearly invisible but needed for mask
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        backpackScrollRect.viewport = viewportRect;
        
        // Create content container (this will grow as inventory expands)
        backpackGridObj = new GameObject("BackpackGrid");
        backpackGridObj.transform.SetParent(viewportObj.transform, false);
        backpackGridRect = backpackGridObj.AddComponent<RectTransform>();
        backpackGridRect.anchorMin = new Vector2(0, 1);
        backpackGridRect.anchorMax = new Vector2(1, 1);
        backpackGridRect.pivot = new Vector2(0.5f, 1);
        backpackGridRect.anchoredPosition = Vector2.zero;
        
        // Content size will be set by ContentSizeFitter
        ContentSizeFitter contentFitter = backpackGridObj.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GridLayoutGroup gridLayout = backpackGridObj.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(slotSize, slotSize);
        gridLayout.spacing = new Vector2(slotSpacing, slotSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = backpackColumns;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.padding = new RectOffset(5, 5, 5, 5);
        
        backpackScrollRect.content = backpackGridRect;
        
        // Create initial slots based on inventory size
        int totalSlots = inventoryManager.SlotCount;
        for (int i = 0; i < totalSlots; i++)
        {
            CreateItemSlot(i);
        }
        
        // Create vertical scrollbar
        GameObject scrollbarObj = new GameObject("Scrollbar");
        scrollbarObj.transform.SetParent(scrollViewObj.transform, false);
        RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.pivot = new Vector2(1, 0.5f);
        scrollbarRect.sizeDelta = new Vector2(10, 0);
        scrollbarRect.anchoredPosition = Vector2.zero;
        
        Image scrollbarBg = scrollbarObj.AddComponent<Image>();
        scrollbarBg.color = new Color(0.15f, 0.15f, 0.18f, 0.8f);
        
        Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        
        // Scrollbar handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(scrollbarObj.transform, false);
        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.sizeDelta = Vector2.zero;
        
        Image handleImg = handleObj.AddComponent<Image>();
        handleImg.color = new Color(0.4f, 0.4f, 0.45f, 1f);
        
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImg;
        backpackScrollRect.verticalScrollbar = scrollbar;
        backpackScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        
        // Subscribe to inventory events
        inventoryManager.OnInventoryExpanded += OnInventoryExpanded;
        inventoryManager.OnInventoryReset += OnInventoryReset;
        inventoryManager.OnItemAdded += OnItemAdded;
        
        // Add hint text at bottom
        GameObject hintObj = new GameObject("Hint");
        hintObj.transform.SetParent(backpackPanel, false);
        Text hint = hintObj.AddComponent<Text>();
        hint.text = Localization.PressToClose;
        hint.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 12 to 14 for better readability
        float hintFontSize = Mathf.Max(14f * windowTitleScale, 12f);
        hint.fontSize = Mathf.RoundToInt(hintFontSize);
        hint.color = new Color(0.4f, 0.4f, 0.4f);
        hint.alignment = TextAnchor.MiddleCenter;
        
        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0);
        hintRect.anchorMax = new Vector2(0.5f, 0);
        hintRect.pivot = new Vector2(0.5f, 0);
        hintRect.sizeDelta = new Vector2(200, 25);
        hintRect.anchoredPosition = new Vector2(0, 10);
    }
    
    /// <summary>
    /// Create button row for Classic layout (Sort, Pick, Merge buttons right-aligned just above inventory grid)
    /// </summary>
    private void CreateClassicButtonRow(RectTransform parent, float aboveGridY)
    {
        float sortWidth = 40f;
        float pickWidth = 50f;  // Wider for "Pick ✓"
        float mergeWidth = 60f; // Wider for "Merge ✓"
        float buttonHeight = 20f;
        float spacing = 4f;
        float rightMargin = 10f;
        
        // Position buttons just above the grid, anchored to top-right of parent
        // aboveGridY is relative to center, convert to top anchor
        float yFromTop = -40f; // Fixed position below the title "BACKPACK"
        
        // Sort button (rightmost)
        Button sortBtn = UIHelper.CreateButton(parent, Localization.Get("Sort"), new Vector2(sortWidth, buttonHeight));
        RectTransform sortRect = sortBtn.GetComponent<RectTransform>();
        sortRect.anchorMin = new Vector2(1, 1);
        sortRect.anchorMax = new Vector2(1, 1);
        sortRect.pivot = new Vector2(1, 1);
        sortRect.anchoredPosition = new Vector2(-rightMargin, yFromTop);
        
        Image sortBg = sortBtn.GetComponent<Image>();
        if (sortBg != null)
        {
            sortBg.color = new Color(0.2f, 0.35f, 0.5f, 0.9f);
        }
        sortBtn.onClick.AddListener(OnSortButtonClicked);
        
        // Auto Pickup button (middle)
        Button pickBtn = UIHelper.CreateButton(parent, Localization.Get("Pick"), new Vector2(pickWidth, buttonHeight));
        RectTransform pickRect = pickBtn.GetComponent<RectTransform>();
        pickRect.anchorMin = new Vector2(1, 1);
        pickRect.anchorMax = new Vector2(1, 1);
        pickRect.pivot = new Vector2(1, 1);
        pickRect.anchoredPosition = new Vector2(-rightMargin - sortWidth - spacing, yFromTop);
        
        Image pickBg = pickBtn.GetComponent<Image>();
        if (pickBg != null)
        {
            pickBg.color = autoPickupEnabled ? new Color(0.2f, 0.5f, 0.35f, 0.9f) : new Color(0.2f, 0.35f, 0.5f, 0.9f);
        }
        autoPickupButton = pickBtn;
        UpdateAutoPickupButtonVisual();
        pickBtn.onClick.AddListener(OnAutoPickupButtonClicked);
        
        // Auto Merge button (leftmost)
        Button mergeBtn = UIHelper.CreateButton(parent, Localization.Get("Merge"), new Vector2(mergeWidth, buttonHeight));
        RectTransform mergeRect = mergeBtn.GetComponent<RectTransform>();
        mergeRect.anchorMin = new Vector2(1, 1);
        mergeRect.anchorMax = new Vector2(1, 1);
        mergeRect.pivot = new Vector2(1, 1);
        mergeRect.anchoredPosition = new Vector2(-rightMargin - sortWidth - spacing - pickWidth - spacing, yFromTop);
        
        Image mergeBg = mergeBtn.GetComponent<Image>();
        if (mergeBg != null)
        {
            mergeBg.color = autoMergeEnabled ? new Color(0.2f, 0.5f, 0.35f, 0.9f) : new Color(0.2f, 0.35f, 0.5f, 0.9f);
        }
        autoMergeButton = mergeBtn;
        UpdateAutoMergeButtonVisual();
        mergeBtn.onClick.AddListener(OnAutoMergeButtonClicked);
    }
    
    /// <summary>
    /// Create button row for Diablo layout (Sort, Pick, Merge buttons in their own panel between equipment and inventory)
    /// </summary>
    private void CreateDiabloButtonRow(RectTransform parent, float yPos, float height)
    {
        // Create an invisible panel to hold the buttons
        GameObject panelObj = new GameObject("ButtonRowPanel");
        panelObj.transform.SetParent(parent, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        
        // Position the panel to span the full width, at the specified Y position
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(0.5f, 1);
        panelRect.anchoredPosition = new Vector2(0, yPos);
        panelRect.sizeDelta = new Vector2(0, height);
        
        // Button dimensions
        float sortWidth = 40f;
        float pickWidth = 50f;  // Wider for "Pick ✓"
        float mergeWidth = 60f; // Wider for "Merge ✓"
        float buttonHeight = 20f;
        float spacing = 4f;
        float rightMargin = 25f;
        
        // Sort button (rightmost) - positioned inside the panel
        Button sortBtn = UIHelper.CreateButton(panelRect, Localization.Get("Sort"), new Vector2(sortWidth, buttonHeight));
        RectTransform sortRect = sortBtn.GetComponent<RectTransform>();
        sortRect.anchorMin = new Vector2(1, 0.5f);
        sortRect.anchorMax = new Vector2(1, 0.5f);
        sortRect.pivot = new Vector2(1, 0.5f);
        sortRect.anchoredPosition = new Vector2(-rightMargin, 0);
        
        Image sortBg = sortBtn.GetComponent<Image>();
        if (sortBg != null)
        {
            sortBg.color = new Color(0.2f, 0.35f, 0.5f, 0.9f);
        }
        sortBtn.onClick.AddListener(OnSortButtonClicked);
        
        // Auto Pickup button (middle)
        Button pickBtn = UIHelper.CreateButton(panelRect, Localization.Get("Pick"), new Vector2(pickWidth, buttonHeight));
        RectTransform pickRect = pickBtn.GetComponent<RectTransform>();
        pickRect.anchorMin = new Vector2(1, 0.5f);
        pickRect.anchorMax = new Vector2(1, 0.5f);
        pickRect.pivot = new Vector2(1, 0.5f);
        pickRect.anchoredPosition = new Vector2(-rightMargin - sortWidth - spacing, 0);
        
        Image pickBg = pickBtn.GetComponent<Image>();
        if (pickBg != null)
        {
            pickBg.color = autoPickupEnabled ? new Color(0.2f, 0.5f, 0.35f, 0.9f) : new Color(0.2f, 0.35f, 0.5f, 0.9f);
        }
        autoPickupButton = pickBtn;
        UpdateAutoPickupButtonVisual();
        pickBtn.onClick.AddListener(OnAutoPickupButtonClicked);
        
        // Auto Merge button (leftmost)
        Button mergeBtn = UIHelper.CreateButton(panelRect, Localization.Get("Merge"), new Vector2(mergeWidth, buttonHeight));
        RectTransform mergeRect = mergeBtn.GetComponent<RectTransform>();
        mergeRect.anchorMin = new Vector2(1, 0.5f);
        mergeRect.anchorMax = new Vector2(1, 0.5f);
        mergeRect.pivot = new Vector2(1, 0.5f);
        mergeRect.anchoredPosition = new Vector2(-rightMargin - sortWidth - spacing - pickWidth - spacing, 0);
        
        Image mergeBg = mergeBtn.GetComponent<Image>();
        if (mergeBg != null)
        {
            mergeBg.color = autoMergeEnabled ? new Color(0.2f, 0.5f, 0.35f, 0.9f) : new Color(0.2f, 0.35f, 0.5f, 0.9f);
        }
        autoMergeButton = mergeBtn;
        UpdateAutoMergeButtonVisual();
        mergeBtn.onClick.AddListener(OnAutoMergeButtonClicked);
    }
    
    /// <summary>
    /// Update auto pickup button visual state
    /// </summary>
    private void UpdateAutoPickupButtonVisual()
    {
        if (autoPickupButton == null) return;
        
        Image btnBg = autoPickupButton.GetComponent<Image>();
        if (btnBg != null)
        {
            btnBg.color = autoPickupEnabled ? new Color(0.2f, 0.5f, 0.35f, 0.9f) : new Color(0.2f, 0.35f, 0.5f, 0.9f);
        }
        
        Text btnText = autoPickupButton.GetComponentInChildren<Text>();
        if (btnText != null)
        {
            // Use localized text for buttons
            btnText.text = autoPickupEnabled ? Localization.Get("Pick") + " ✓" : Localization.Get("Pick");
        }
    }
    
    /// <summary>
    /// Update auto merge button visual state
    /// </summary>
    private void UpdateAutoMergeButtonVisual()
    {
        if (autoMergeButton == null) return;
        
        Image btnBg = autoMergeButton.GetComponent<Image>();
        if (btnBg != null)
        {
            btnBg.color = autoMergeEnabled ? new Color(0.2f, 0.5f, 0.35f, 0.9f) : new Color(0.2f, 0.35f, 0.5f, 0.9f);
        }
        
        Text btnText = autoMergeButton.GetComponentInChildren<Text>();
        if (btnText != null)
        {
            // Use localized text for buttons
            btnText.text = autoMergeEnabled ? Localization.Get("Merge") + " ✓" : Localization.Get("Merge");
        }
    }
    
    /// <summary>
    /// Handle auto pickup button click - toggle auto pickup
    /// </summary>
    private void OnAutoPickupButtonClicked()
    {
        autoPickupEnabled = !autoPickupEnabled;
        UpdateAutoPickupButtonVisual();
        PlaySortSound();
        RPGLog.Debug("[AutoPickup] Auto Pickup " + (autoPickupEnabled ? "ENABLED" : "DISABLED"));
    }
    
    /// <summary>
    /// Handle auto merge button click - toggle auto merge
    /// </summary>
    private void OnAutoMergeButtonClicked()
    {
        autoMergeEnabled = !autoMergeEnabled;
        UpdateAutoMergeButtonVisual();
        PlaySortSound();
        RPGLog.Debug("[AutoMerge] Auto Merge " + (autoMergeEnabled ? "ENABLED" : "DISABLED"));
        
        // If just enabled, do an immediate merge check
        if (autoMergeEnabled)
        {
            CheckAndAutoMergeInventory();
        }
    }
    
    /// <summary>
    /// Check and auto merge one pair of duplicate items in inventory or equipped
    /// Called periodically when auto merge is enabled
    /// </summary>
    private void CheckAndAutoMergeInventory()
    {
        if (inventoryManager == null) return;
        if (equipmentManager == null) return;
        
        // Build a list of all items with their locations
        // Location: -1 to -10 = equipment slots, 0+ = inventory slots
        List<AutoMergeCandidate> candidates = new List<AutoMergeCandidate>();
        
        // Add inventory items
        for (int i = 0; i < inventoryManager.SlotCount; i++)
        {
            RPGItem item = inventoryManager.GetItem(i);
            if (item == null) continue;
            if (item.type == ItemType.Consumable) continue; // Skip consumables
            candidates.Add(new AutoMergeCandidate { item = item, inventorySlot = i, equipmentSlot = null });
        }
        
        // Add equipped items
        EquipmentSlotType[] equipSlots = (EquipmentSlotType[])System.Enum.GetValues(typeof(EquipmentSlotType));
        foreach (EquipmentSlotType slot in equipSlots)
        {
            RPGItem item = equipmentManager.GetEquippedItem(slot);
            if (item == null) continue;
            if (item.type == ItemType.Consumable) continue; // Skip consumables
            candidates.Add(new AutoMergeCandidate { item = item, inventorySlot = -1, equipmentSlot = slot });
        }
        
        // Look for matching pairs
        for (int i = 0; i < candidates.Count; i++)
        {
            AutoMergeCandidate c1 = candidates[i];
            
            for (int j = i + 1; j < candidates.Count; j++)
            {
                AutoMergeCandidate c2 = candidates[j];
                
                // Must be same item ID AND same upgrade level
                if (c1.item.id == c2.item.id && c1.item.upgradeLevel == c2.item.upgradeLevel)
                {
                    // IMPORTANT: Don't merge if BOTH items are equipped!
                    // Player intentionally equipped two of the same item (dual-wield weapons, matching rings)
                    if (c1.equipmentSlot.HasValue && c2.equipmentSlot.HasValue)
                    {
                        continue; // Skip this pair - both are equipped intentionally
                    }
                    
                    // Merge c2 into c1
                    int oldLevel = c1.item.upgradeLevel;
                    
                    // Remember if c2 was equipped (we'll need to re-equip c1 to that slot)
                    EquipmentSlotType? c2EquipSlot = c2.equipmentSlot;
                    
                    // Use UpgradeItem which handles level increment AND stat recalculation
                    InventoryMerchantHelper.UpgradeItem(c1.item);
                    
                    // Remove c2 (consumed in merge)
                    if (c2.equipmentSlot.HasValue)
                    {
                        // Remove from equipment
                        equipmentManager.Unequip(c2.equipmentSlot.Value);
                        RPGLog.Debug(string.Format("[AutoMerge] Merged {0} (equipped {1}) + (equipped {2}) -> +{3}", 
                            c1.item.name, c1.equipmentSlot.HasValue ? c1.equipmentSlot.Value.ToString() : "inv:" + c1.inventorySlot, 
                            c2.equipmentSlot.Value, c1.item.upgradeLevel));
                    }
                    else
                    {
                        // Remove from inventory
                        inventoryManager.Items[c2.inventorySlot] = null;
                        RPGLog.Debug(string.Format("[AutoMerge] Merged {0} ({1}) + (inv slot {2}) -> +{3}", 
                            c1.item.name, c1.equipmentSlot.HasValue ? "equipped " + c1.equipmentSlot.Value : "inv:" + c1.inventorySlot, 
                            c2.inventorySlot, c1.item.upgradeLevel));
                    }
                    
                    ScoringPatches.TrackItemMerged();
                    
                    // Show RPG lore chat message
                    ShowAutoMergeChatMessage(c1.item, oldLevel, c1.item.upgradeLevel);
                    
                    // If the consumed item (c2) was equipped, re-equip the upgraded item (c1) to that slot
                    if (c2EquipSlot.HasValue && !c1.equipmentSlot.HasValue)
                    {
                        // c1 is in inventory, c2 was equipped - move upgraded c1 to the equipment slot
                        inventoryManager.Items[c1.inventorySlot] = null; // Remove from inventory
                        equipmentManager.Equip(c1.item, c2EquipSlot.Value);
                        RPGLog.Debug(string.Format("[AutoMerge] Re-equipped upgraded {0} to slot {1}", 
                            c1.item.name, c2EquipSlot.Value));
                        
                        // Update the equipment slot UI
                        foreach (EquipmentSlot slot in equipmentSlots)
                        {
                            if (slot.SlotType == c2EquipSlot.Value)
                            {
                                slot.SetItem(c1.item);
                                break;
                            }
                        }
                    }
                    
                    // Recalculate stats if any equipped item was involved
                    if (c1.equipmentSlot.HasValue || c2EquipSlot.HasValue)
                    {
                        equipmentManager.ReapplyAllStats();
                    }
                    
                    RefreshInventory();
                    MarkDirty();
                    
                    return; // Only merge one pair per check to avoid issues
                }
            }
        }
    }
    
    /// <summary>
    /// Helper struct for auto merge candidate tracking
    /// </summary>
    private struct AutoMergeCandidate
    {
        public RPGItem item;
        public int inventorySlot; // -1 if equipped
        public EquipmentSlotType? equipmentSlot; // null if in inventory
    }
    
    /// <summary>
    /// Show a local RPG-style chat message when auto-merge happens
    /// </summary>
    private void ShowAutoMergeChatMessage(RPGItem mergedItem, int oldLevel, int newLevel)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            // Pick a random lore message using the new JSON-based localization
            string loreText = Localization.GetRandomMergeLore();
            
            // Get rarity color
            string rarityColor = GetRarityColorHex(mergedItem.rarity);
            
            // Format: [Lore message] [Item Name] +old → +new
            string upgradeText = string.Format("+{0} → +{1}", oldLevel, newLevel);
            string content = string.Format("<color=#b8a0d4><i>{0}</i></color>\n<color={1}>[{2}]</color> <color=#90EE90>{3}</color>",
                loreText, rarityColor, mergedItem.GetDisplayName(), upgradeText);
            
            ChatManager.Message msg = new ChatManager.Message();
            msg.type = ChatManager.MessageType.Raw;
            msg.content = content;
            
            chatManager.ShowMessageLocally(msg);
        }
        catch (System.Exception e)
        {
            RPGLog.Warning("[AutoMerge] Failed to show chat message: " + e.Message);
        }
    }
    
    /// <summary>
    /// Get color hex for item rarity
    /// </summary>
    private static string GetRarityColorHex(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "#b0b0b0";
            case ItemRarity.Rare: return "#4169E1";
            case ItemRarity.Epic: return "#9932CC";
            case ItemRarity.Legendary: return "#FFD700";
            default: return "#ffffff";
    }
    }
    
    /// <summary>
    /// <summary>
    /// Handle sort button click - organize inventory
    /// </summary>
    private void OnSortButtonClicked()
    {
        if (inventoryManager == null) return;
        
        // Sort the inventory
        inventoryManager.SortInventory();
        
        // Refresh UI
        RefreshInventory();
        MarkDirty();
        
        // Play a sound effect (use dismantle tap sound)
        PlaySortSound();
    }
    
    /// <summary>
    /// Play sort sound effect
    /// </summary>
    private void PlaySortSound()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        if (fxDismantleTap == null)
        {
            LoadSoundEffects();
        }
        
        if (fxDismantleTap != null)
        {
            DewEffect.PlayNew(fxDismantleTap, DewPlayer.local.hero.position, null, null);
        }
    }
    
    /// <summary>
    /// Check for items in range and auto pickup if enabled
    /// </summary>
    private void CheckAndAutoPickupItems()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        if (inventoryManager == null) return;
        
        Hero hero = DewPlayer.local.hero;
        if (hero.IsNullInactiveDeadOrKnockedOut()) return;
        
        Vector3 heroPos = hero.position;
        const float pickupRange = 4.0f; // Slightly larger than DroppedItem focusDistance (3.5)
        
        // Get all dropped items
        List<DroppedItem> allItems = DroppedItem.GetAllDroppedItems();
        if (allItems == null || allItems.Count == 0) return;
        
        // Log item count periodically (every 30 seconds) - only for debug
        if (Time.time - _lastAutoPickupLogTime > 30f)
        {
            _lastAutoPickupLogTime = Time.time;
            RPGLog.Debug("[AutoPickup] Found " + allItems.Count + " dropped items on ground");
        }
        
        // Create a copy to avoid modification during iteration
        List<DroppedItem> itemsToCheck = new List<DroppedItem>(allItems);
        
        foreach (DroppedItem droppedItem in itemsToCheck)
        {
            if (droppedItem == null) continue;
            if (droppedItem.item == null) continue;
            
            // Check distance first (cheaper than CanInteract)
            Vector3 itemPos = droppedItem.transform.position;
            float distance = Vector2.Distance(new Vector2(itemPos.x, itemPos.z), new Vector2(heroPos.x, heroPos.z));
            
            if (distance <= pickupRange)
            {
                // Check if item can be interacted with (checks spawn delay, ownership, etc.)
                if (!droppedItem.CanInteract(hero)) continue;
                
                // Only auto-pickup monster drops, NOT player-dropped items
                if (droppedItem.networkDropId != 0)
                {
                    NetworkedItemSystem.NetworkedDropData netData = NetworkedItemSystem.GetDrop(droppedItem.networkDropId);
                    if (netData != null && !netData.isMonsterDrop)
                    {
                        // This is a player-dropped item, skip it
                        continue;
                    }
                }
                
                // Try to pick up
                TryAutoPickupItem(droppedItem);
                return; // Only pick up one item per check to avoid issues
            }
        }
    }
    
    private float _lastAutoPickupLogTime = 0f;
    
    /// <summary>
    /// Try to auto pickup a dropped item
    /// </summary>
    private void TryAutoPickupItem(DroppedItem droppedItem)
    {
        if (droppedItem == null || droppedItem.item == null) return;
        if (inventoryManager == null) return;
        
        // For networked items, check if we're already trying to pick this up (prevents race conditions)
        if (droppedItem.networkDropId != 0)
        {
            uint dropId = droppedItem.networkDropId;
            
            // Check if already pending pickup
            if (_pendingPickups.Contains(dropId))
            {
                return;
            }
            
            // Check cooldown to prevent spam
            float currentTime = Time.realtimeSinceStartup;
            float cooldownExpiry;
            if (_pickupCooldowns.TryGetValue(dropId, out cooldownExpiry))
            {
                if (currentTime < cooldownExpiry)
                {
                    return;
                }
                _pickupCooldowns.Remove(dropId);
            }
            
            // Mark as pending and set cooldown
            _pendingPickups.Add(dropId);
            _pickupCooldowns[dropId] = currentTime + PICKUP_COOLDOWN;
            
            RPGLog.Debug("[AutoPickup] Picking up (networked): " + droppedItem.item.name);
            NetworkedItemSystem.RequestPickup(dropId);
            
            // Remove from pending after a short delay (in case server doesn't respond)
            // The item will be removed from _activeDrops by NetworkedItemSystem anyway
            StartCoroutine(RemovePendingPickupAfterDelay(dropId, 1f));
        }
        else
        {
            // Local-only item - add manually
            RPGItem itemToAdd = droppedItem.item.Clone();
            bool added = inventoryManager.AddItem(itemToAdd);
            if (added)
            {
                RPGLog.Debug("[AutoPickup] Picked up (local): " + itemToAdd.name);
                droppedItem.CleanupAndDestroy();
            }
            else
            {
                RPGLog.Warning("[AutoPickup] Failed to add to inventory: " + itemToAdd.name);
                return;
            }
        }
        
        RefreshInventory();
    }
    
    /// <summary>
    /// Coroutine to remove pending pickup after delay
    /// </summary>
    private System.Collections.IEnumerator RemovePendingPickupAfterDelay(uint dropId, float delay)
    {
        yield return new WaitForSeconds(delay);
        _pendingPickups.Remove(dropId);
    }
    
    /// <summary>
    /// Clean up expired pickup cooldowns to prevent memory buildup
    /// </summary>
    private void CleanupExpiredPickupCooldowns()
    {
        if (_pickupCooldowns.Count == 0) return;
        
        float currentTime = Time.realtimeSinceStartup;
        List<uint> expiredKeys = null;
        
        foreach (var kvp in _pickupCooldowns)
        {
            if (currentTime >= kvp.Value)
            {
                if (expiredKeys == null) expiredKeys = new List<uint>();
                expiredKeys.Add(kvp.Key);
            }
        }
        
        if (expiredKeys != null)
        {
            foreach (uint key in expiredKeys)
            {
                _pickupCooldowns.Remove(key);
            }
        }
    }
    
    /// <summary>
    /// Create a single item slot and add it to the grid
    /// </summary>
    private void CreateItemSlot(int index)
    {
        GameObject slotObj = new GameObject("ItemSlot_" + index);
        slotObj.transform.SetParent(backpackGridObj.transform, false);
        slotObj.AddComponent<RectTransform>();
        
        ItemSlot slot = slotObj.AddComponent<ItemSlot>();
        slot.Initialize(index, this, inventoryManager);
        itemSlots.Add(slot);
    }
    
    /// <summary>
    /// Called when inventory expands - add new slots to UI
    /// </summary>
    private void OnInventoryExpanded(int newSlotCount)
    {
        int startIndex = itemSlots.Count;
        for (int i = 0; i < newSlotCount; i++)
        {
            CreateItemSlot(startIndex + i);
        }
        
        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(backpackGridRect);
        
        // Scroll to bottom to show new slots
        if (backpackScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            backpackScrollRect.verticalNormalizedPosition = 0f;
        }
        
        RPGLog.Debug(string.Format(" UI expanded: added {0} slots, total {1}", newSlotCount, itemSlots.Count));
    }
    
    /// <summary>
    /// Called when inventory resets (new run) - rebuild slots to initial count
    /// For Diablo layout, maintain minimum 40 slots (4 rows x 10 columns)
    /// </summary>
    private void OnInventoryReset()
    {
        RPGLog.Debug(" Inventory reset - rebuilding UI slots...");
        
        // For Diablo layout, we need minimum 40 slots (4 rows)
        // For Classic layout, use initialSlots (30)
        int minSlots = inventoryManager.InitialSlotCount;
        if (uiStyle == UIStyle.DiabloStyle)
        {
            minSlots = 4 * backpackColumns; // 40 slots for 4 rows (sort button is now in button row, not grid)
        }
        
        // Destroy excess slots (keep only minSlots)
        while (itemSlots.Count > minSlots)
        {
            int lastIndex = itemSlots.Count - 1;
            if (itemSlots[lastIndex] != null && itemSlots[lastIndex].gameObject != null)
            {
                GameObject.Destroy(itemSlots[lastIndex].gameObject);
            }
            itemSlots.RemoveAt(lastIndex);
        }
        
        // For Diablo layout, ensure we have enough slots
        if (uiStyle == UIStyle.DiabloStyle)
        {
            // Expand inventory manager if needed
            while (inventoryManager.SlotCount < minSlots)
            {
                inventoryManager.Items.Add(null);
            }
            
            // Create missing UI slots
            while (itemSlots.Count < minSlots)
            {
                CreateItemSlot(itemSlots.Count);
            }
        }
        
        // Force layout rebuild
        if (backpackGridRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(backpackGridRect);
        }
        
        // Scroll to top
        if (backpackScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            backpackScrollRect.verticalNormalizedPosition = 1f;
        }
        
        // Refresh all slots
        RefreshInventory();
        
        RPGLog.Debug(string.Format(" UI reset: now {0} slots (min required: {1})", itemSlots.Count, minSlots));
    }
    
    /// <summary>
    /// Called when an item is added to inventory - refresh display if visible
    /// </summary>
    private void OnItemAdded()
    {
        // Refresh inventory display if it's currently visible
        if (canvasGroup != null && canvasGroup.alpha > 0)
        {
            RefreshInventory();
        }
        else
        {
            // Mark as needing refresh for when it becomes visible
            MarkDirty();
        }
    }

    /// <summary>
    /// Create the FAST SLOTS window (right side) - separate game-styled window
    /// </summary>
    private void CreateFastSlotsWindow()
    {
        // Create window using game's Widget Window prefab
        Vector2 windowSize = new Vector2(140, 420);  // Taller for button padding
        GameObject windowObj = UIHelper.CreateWindowPanel(mainPanel, windowSize, "FastSlotsWindow");
        RectTransform fastPanel = windowObj.GetComponent<RectTransform>();
        fastPanel.anchorMin = new Vector2(1, 0.5f);
        fastPanel.anchorMax = new Vector2(1, 0.5f);
        fastPanel.pivot = new Vector2(1, 0.5f);
        fastPanel.anchoredPosition = new Vector2(0, 50);
        
        // Window title
        CreateWindowTitle(fastPanel, Localization.Hotbar, new Vector2(0, -20));
        
        // Instructions
        GameObject instrObj = new GameObject("Instructions");
        instrObj.transform.SetParent(fastPanel, false);
        Text instr = instrObj.AddComponent<Text>();
        instr.text = Localization.DragToAssign;
        instr.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 10 to 12 for better readability
        instr.fontSize = 12;
        instr.color = new Color(0.5f, 0.5f, 0.5f);
        instr.alignment = TextAnchor.UpperCenter;
        
        RectTransform instrRect = instr.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0, 1);
        instrRect.anchorMax = new Vector2(1, 1);
        instrRect.pivot = new Vector2(0.5f, 1);
        instrRect.sizeDelta = new Vector2(-10, 35);
        instrRect.anchoredPosition = new Vector2(0, -40);
        
        // 4 fast slots vertically
        float startY = -90f;
        float slotHeight = 65f;
        
        for (int i = 0; i < 4; i++)
        {
            // Slot label (hotkey number)
            GameObject labelObj = new GameObject("SlotLabel_" + i);
            labelObj.transform.SetParent(fastPanel, false);
            Text label = labelObj.AddComponent<Text>();
            label.text = "[" + (i + 1) + "]";
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 11;
            label.color = new Color(0.6f, 0.6f, 0.6f);
            label.alignment = TextAnchor.MiddleCenter;
            
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 1);
            labelRect.anchorMax = new Vector2(0.5f, 1);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(80, 15);
            labelRect.anchoredPosition = new Vector2(0, startY - (i * slotHeight) + 20);
            
            // The actual slot
            GameObject slotObj = new GameObject("FastSlot_" + i);
            slotObj.transform.SetParent(fastPanel, false);
            
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 1);
            slotRect.anchorMax = new Vector2(0.5f, 1);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(50, 50);
            slotRect.anchoredPosition = new Vector2(0, startY - (i * slotHeight));
            
            FastSlotUI slot = slotObj.AddComponent<FastSlotUI>();
            slot.Initialize(i, this, consumableBar);
        }
        
        // Settings button at bottom with much more padding to avoid borders
        GameObject btnObj = new GameObject("SettingsButton");
        btnObj.transform.SetParent(fastPanel, false);
        
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0);
        btnRect.anchorMax = new Vector2(0.5f, 0);
        btnRect.pivot = new Vector2(0.5f, 0);
        btnRect.sizeDelta = new Vector2(90, 26);
        btnRect.anchoredPosition = new Vector2(0, 35); // Much more padding from bottom
        
        Button btn = btnObj.AddComponent<Button>();
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = UIHelper.ButtonColor;
        
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.text = Localization.Settings;
        btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 11 to 13 for better readability
        btnText.fontSize = 13;
        btnText.fontStyle = FontStyle.Bold;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform btnTextRect = btnText.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        btnTextRect.anchoredPosition = Vector2.zero;
        
        btn.onClick.AddListener(() => {
            SettingsPanel settings = GetComponent<SettingsPanel>();
            if (settings == null)
            {
                settings = transform.parent.GetComponentInChildren<SettingsPanel>();
            }
            if (settings != null)
            {
                settings.SetVisible(!settings.IsVisible());
            }
        });
    }

    private void CreateStatsDisplay()
    {
        GameObject statsObj = new GameObject("StatsDisplay");
        statsObj.transform.SetParent(characterPanel, false);
        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0);
        statsRect.anchorMax = new Vector2(1, 0);
        statsRect.pivot = new Vector2(0.5f, 0);
        statsRect.sizeDelta = new Vector2(-50, 135); // Much more side padding to not cover borders
        statsRect.anchoredPosition = new Vector2(0, 30); // Much more bottom padding
        
        Image statsBg = statsObj.AddComponent<Image>();
        statsBg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
        
        // Title
        GameObject titleObj = new GameObject("StatsTitle");
        titleObj.transform.SetParent(statsObj.transform, false);
        Text titleTxt = titleObj.AddComponent<Text>();
        titleTxt.text = Localization.Stats;
        titleTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 11 to 13 for better readability
        titleTxt.fontSize = 13;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.color = accentColor;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleTxtRect = titleTxt.GetComponent<RectTransform>();
        titleTxtRect.anchorMin = new Vector2(0, 1);
        titleTxtRect.anchorMax = new Vector2(1, 1);
        titleTxtRect.pivot = new Vector2(0.5f, 1);
        titleTxtRect.sizeDelta = new Vector2(0, 18);
        titleTxtRect.anchoredPosition = Vector2.zero;
        
        // Stats text - with more padding
        GameObject statsTextObj = new GameObject("StatsText");
        statsTextObj.transform.SetParent(statsObj.transform, false);
        statsText = statsTextObj.AddComponent<Text>();
        statsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 10 to 12 for better readability
        statsText.fontSize = 12;
        statsText.color = Color.white;
        statsText.alignment = TextAnchor.UpperLeft;
        statsText.supportRichText = true;
        statsText.text = Localization.Loading;
        statsText.verticalOverflow = VerticalWrapMode.Overflow;
        
        RectTransform statsTextRect = statsText.GetComponent<RectTransform>();
        statsTextRect.anchorMin = new Vector2(0, 0);
        statsTextRect.anchorMax = new Vector2(1, 1);
        statsTextRect.sizeDelta = new Vector2(-12, -20);
        statsTextRect.anchoredPosition = new Vector2(6, -2);
    }
    
    /// <summary>
    /// Create the SET BONUS panel - shows active set bonuses from equipped items
    /// Positioned to the LEFT of the character panel, vertically centered
    /// Panel auto-adapts height based on content
    /// </summary>
    private void CreateSetBonusPanel()
    {
        // Create a window - wider to accommodate longer descriptions
        Vector2 windowSize = new Vector2(280, 100); // Initial size, will auto-adapt (increased from 220 to 280)
        GameObject windowObj = UIHelper.CreateWindowPanel(mainPanel, windowSize, "SetBonusWindow");
        setBonusPanel = windowObj.GetComponent<RectTransform>();
        
        // Position based on UI style
        if (uiStyle == UIStyle.DiabloStyle)
        {
            // For Diablo-style: Position centered on screen, hidden by default (shown via button)
            // Position relative to canvas, not mainPanel
            setBonusPanel.SetParent(canvas.transform, false);
            setBonusPanel.anchorMin = new Vector2(0.5f, 0.5f);
            setBonusPanel.anchorMax = new Vector2(0.5f, 0.5f);
            setBonusPanel.pivot = new Vector2(0.5f, 0.5f);
            setBonusPanel.anchoredPosition = new Vector2(0, 0); // Centered on screen
            setBonusPanel.gameObject.SetActive(false); // Hidden by default, shown via SET BONUSES button
        }
        else
        {
            // For Classic: Position to the left of character panel
        setBonusPanel.anchorMin = new Vector2(0, 0.5f);
        setBonusPanel.anchorMax = new Vector2(0, 0.5f);
        setBonusPanel.pivot = new Vector2(1, 0.5f); // Pivot on right side so it extends left
        setBonusPanel.anchoredPosition = new Vector2(-10, 0); // 10px gap to the left of character panel
        }
        
        // Get or add vertical layout group for auto-sizing
        VerticalLayoutGroup layout = windowObj.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = windowObj.AddComponent<VerticalLayoutGroup>();
        }
        // Large padding to keep content inside the styled border area
        layout.padding = new RectOffset(25, 25, 30, 35); // left, right, top, bottom - extra for decorative borders
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        // Get or add content size fitter to auto-adapt panel height
        ContentSizeFitter panelFitter = windowObj.GetComponent<ContentSizeFitter>();
        if (panelFitter == null)
        {
            panelFitter = windowObj.AddComponent<ContentSizeFitter>();
        }
        panelFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Window title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(setBonusPanel, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 14;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(1f, 0.84f, 0f); // Gold color
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = Localization.Get("SetBonuses");
        
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 25;
        titleLayout.flexibleWidth = 1;
        
        // Set bonus content text
        GameObject contentObj = new GameObject("SetBonusContent");
        contentObj.transform.SetParent(setBonusPanel, false);
        setBonusText = contentObj.AddComponent<Text>();
        setBonusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        setBonusText.fontSize = 10;
        setBonusText.color = Color.white;
        setBonusText.alignment = TextAnchor.UpperLeft;
        setBonusText.supportRichText = true;
        setBonusText.text = Localization.Get("NoSetBonuses");
        setBonusText.horizontalOverflow = HorizontalWrapMode.Wrap; // Wrap text instead of cutting off
        setBonusText.verticalOverflow = VerticalWrapMode.Overflow;
        
        // Add RectTransform setup for proper text sizing
        RectTransform textRect = contentObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        
        // Layout element for text to work with content size fitter
        LayoutElement textLayout = contentObj.AddComponent<LayoutElement>();
        textLayout.flexibleWidth = 1;
        textLayout.preferredWidth = 230; // 280 - 50 padding (25 left + 25 right) - increased from 170
        textLayout.minHeight = 50; // Minimum height to ensure text is visible
    }
    
    /// <summary>
    /// Update the set bonus display text
    /// </summary>
    private void UpdateSetBonusDisplay()
    {
        if (equipmentManager == null) return;
        
        SetBonusSystem setSystem = equipmentManager.SetBonusSystem;
        string displayText = "";
        
        if (setSystem == null)
        {
            displayText = Localization.IsChinese() ? "套装系统未初始化" : "Set system not initialized";
        }
        else
        {
        List<SetBonusSystem.SetBonusUIData> activeBonus = setSystem.GetActiveSetBonusesForUI();
        
        if (activeBonus.Count == 0)
        {
                displayText = Localization.Get("NoSetBonusesDetailed");
        }
            else
            {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        bool isChinese = Localization.IsChinese();
        
        foreach (var data in activeBonus)
        {
            if (data.definition == null) continue;
            
            // Get rarity color
            string rarityColor = UIHelper.GetRarityColorHex(data.rarity);
            string setName = isChinese ? data.definition.setNameChinese : data.definition.setName;
            
            // Set name and count
            sb.AppendFormat("<color={0}><b>{1}</b></color> ({2}/6)\n", rarityColor, setName, data.equippedCount);
            
            // 2-piece bonus
            if (data.definition.twoSetBonus != null)
            {
                string desc = isChinese ? data.definition.twoSetBonus.descriptionChinese : data.definition.twoSetBonus.description;
                string checkmark = data.hasTwoSet ? "<color=#00FF00>✓</color>" : "<color=#666666>○</color>";
                sb.AppendFormat("  {0} 2pc: {1}\n", checkmark, desc);
            }
            
            // 4-piece bonus
            if (data.definition.fourSetBonus != null)
            {
                string desc = isChinese ? data.definition.fourSetBonus.descriptionChinese : data.definition.fourSetBonus.description;
                string checkmark = data.hasFourSet ? "<color=#00FF00>✓</color>" : "<color=#666666>○</color>";
                sb.AppendFormat("  {0} 4pc: {1}\n", checkmark, desc);
            }
            
            // 6-piece bonus
            if (data.definition.sixSetBonus != null)
            {
                string desc = isChinese ? data.definition.sixSetBonus.descriptionChinese : data.definition.sixSetBonus.description;
                string checkmark = data.hasSixSet ? "<color=#FFD700>✓</color>" : "<color=#666666>○</color>";
                sb.AppendFormat("  {0} 6pc: {1}\n", checkmark, desc);
                
                // Show cooldown if 6-piece is active
                if (data.hasSixSet)
                {
                    float cooldown = setSystem.GetProcCooldownRemaining(data.rarity);
                    if (cooldown > 0)
                    {
                        sb.AppendFormat("     <color=#FF6600>CD: {0:F1}s</color>\n", cooldown);
                    }
                }
            }
            
            sb.Append("\n");
        }
        
                displayText = sb.ToString().TrimEnd('\n');
            }
        }
        
        // Update classic layout set bonus text
        if (setBonusText != null)
        {
            setBonusText.text = displayText;
        }
        
        // Update Diablo layout set bonus text (in the main panel)
        if (diabloSetBonusText != null)
        {
            diabloSetBonusText.text = displayText;
        }
    }

    private void CreateTooltip()
    {
        // Use UIHelper to create a game-styled tooltip panel
        tooltipPanel = UIHelper.CreateTooltipPanel(canvas.transform);
        
        // IMPORTANT: Add CanvasGroup to prevent tooltip from blocking raycasts
        // This fixes the flickering issue when tooltip appears under mouse
        CanvasGroup tooltipCanvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
        tooltipCanvasGroup.blocksRaycasts = false;
        tooltipCanvasGroup.interactable = false;

        // Tooltip text
        GameObject tooltipTextObj = new GameObject("TooltipText");
        tooltipTextObj.transform.SetParent(tooltipPanel.transform, false);
        tooltipText = tooltipTextObj.AddComponent<Text>();
        tooltipText.text = "";
        tooltipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 12 to 14 for better readability
        tooltipText.fontSize = 14;
        tooltipText.color = new Color(0.85f, 0.85f, 0.85f, 1f); // Brighter text for readability
        tooltipText.alignment = TextAnchor.UpperLeft;
        tooltipText.supportRichText = true;
        tooltipText.raycastTarget = false; // Also disable raycast on text
        
        // Add shadow for text like game
        Shadow textShadow = tooltipTextObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        textShadow.effectDistance = new Vector2(1, -1);
        
        // Add LayoutElement for minimum width
        LayoutElement textLayout = tooltipTextObj.AddComponent<LayoutElement>();
        textLayout.minWidth = 240;
        textLayout.preferredWidth = 240;

        tooltipPanel.SetActive(false);
    }

    public void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            if (visible)
            {
                needsRefresh = true;
                RefreshInventory();
                RefreshFastSlots(); // Sync fast slots with ConsumableBar
                UpdateStatsDisplay();
                UpdateSetBonusDisplay(); // Also update set bonus display
            }
        }
    }

    public void ShowTooltip(RPGItem item, Vector2 position)
    {
        ShowTooltip(item, position, -1, false);
    }
    
    /// <summary>
    /// Show tooltip for an equipped item (no comparison stats)
    /// </summary>
    public void ShowEquippedTooltip(RPGItem item, Vector2 position)
    {
        ShowEquippedTooltip(item, position, null);
    }
    
    /// <summary>
    /// Show tooltip for an equipped item with slot type (for upgrade support)
    /// </summary>
    public void ShowEquippedTooltip(RPGItem item, Vector2 position, EquipmentSlotType? slotType)
    {
        tooltipEquipmentSlot = slotType;
        ShowTooltip(item, position, -1, true);
    }
    
    public void ShowTooltip(RPGItem item, Vector2 position, int slotIndex)
    {
        ShowTooltip(item, position, slotIndex, false);
    }
    
    public void ShowTooltip(RPGItem item, Vector2 position, int slotIndex, bool isEquipped)
    {
        if (item == null) return;
        
        // Use our custom tooltip
        if (tooltipPanel == null || tooltipText == null)
        {
            return;
        }
        
        // Reset progress if hovering over a different item
        if (tooltipItemSlotIndex != slotIndex)
        {
            sellProgress = 0f;
            upgradeProgress = 0f;
            isSelling = false;
            isUpgrading = false;
        }
        
        tooltipItemSlotIndex = slotIndex;
        hoveredSlotIndex = slotIndex; // Track for hotkey
        
        // Build tooltip content - use equipped tooltip (no comparison) for equipped items
        string tooltipContent = isEquipped ? item.GetEquippedTooltip() : BuildTooltipContent(item);
        
        // Add hints for hotkeys
        List<string> hints = new List<string>();
        
        // Combine hint - check if dragging a combinable item
        if (isDragging && draggedSlot != null && slotIndex >= 0)
        {
            RPGItem draggedItem = inventoryManager.GetItem(draggedSlot.SlotIndex);
            if (CanCombineItems(draggedItem, item))
            {
                hints.Add("<color=#55efc4>" + Localization.DropToCombine + " -> +" + (item.upgradeLevel + 1) + "</color>");
            }
        }
        
        // Combine info for non-consumable items
        if (!isDragging && item.type != ItemType.Consumable && item.type != ItemType.Quest && item.type != ItemType.Material)
        {
            hints.Add("<color=#b2bec3>" + Localization.DragSameItemToCombine + " (+" + item.upgradeLevel + ")</color>");
        }
        
        // Sell hint if near merchant
        PropEnt_Merchant_Base merchant = FindNearbyMerchant();
        if (merchant != null && item.type != ItemType.Quest)
        {
            int sellPrice = InventoryMerchantHelper.GetSellPrice(item);
            hints.Add(Localization.FormatSellHint(sellPrice));
        }
        
        // Upgrade hint if near upgrade well
        Shrine_UpgradeWell upgradeWell = FindNearbyUpgradeWell();
        bool canUpgrade = item.type != ItemType.Consumable && 
                          item.type != ItemType.Material && 
                          item.type != ItemType.Quest;
        
        if (upgradeWell != null && canUpgrade)
        {
            int upgradeCost = InventoryMerchantHelper.GetUpgradeCost(item);
            hints.Add(Localization.FormatUpgradeHint(upgradeCost));
        }
        
        // Dismantle hint (works anywhere) - equipment only
        bool canDismantle = item.type != ItemType.Consumable && 
                            item.type != ItemType.Material && 
                            item.type != ItemType.Quest;
        if (canDismantle)
        {
            int dismantleValue = InventoryMerchantHelper.GetDismantleValue(item);
            hints.Add(Localization.FormatDismantleHint(dismantleValue));
        }
        
        // Cleanse hint if near cleanse altar
        Shrine_AltarOfCleansing cleanseAltar = FindNearbyCleanseAltar();
        if (cleanseAltar != null && InventoryMerchantHelper.CanCleanse(item))
        {
            int cleanseCost = InventoryMerchantHelper.GetCleanseCost(item);
            int cleanseRefund = InventoryMerchantHelper.GetCleanseRefund(item);
            hints.Add(Localization.FormatCleanseHint(cleanseCost, cleanseRefund));
        }
        
        if (hints.Count > 0)
        {
            tooltipContent += "\n\n" + string.Join("\n", hints.ToArray());
        }
        
        tooltipText.text = tooltipContent;
        
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        
        // Force layout rebuild to get accurate size
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        
        // Get tooltip size
        float tooltipWidth = tooltipRect.rect.width;
        float tooltipHeight = tooltipRect.rect.height;
        
        // Calculate position with screen boundary checking
        Vector2 adjustedPos = position;
        
        // Check right edge - if tooltip would go off screen, show it to the left of cursor
        if (position.x + tooltipWidth > Screen.width)
        {
            adjustedPos.x = position.x - tooltipWidth - 20;
        }
        
        // Check left edge
        if (adjustedPos.x < 0)
        {
            adjustedPos.x = 10;
        }
        
        // Check top edge
        if (position.y + tooltipHeight > Screen.height)
        {
            adjustedPos.y = Screen.height - tooltipHeight - 10;
        }
        
        // Check bottom edge
        if (adjustedPos.y < tooltipHeight)
        {
            adjustedPos.y = tooltipHeight + 10;
        }
        
        tooltipRect.position = adjustedPos;
        
        // Hide buttons - using hotkeys instead
        if (sellButton != null) sellButton.SetActive(false);
        if (upgradeButton != null) upgradeButton.SetActive(false);
        
        // Create progress bars if needed
        CreateProgressBarsIfNeeded();
        
        // Only hide progress bars if we're NOT currently in an action (selling/upgrading)
        // Otherwise the bar would flicker when tooltip is refreshed during hold action
        if (!isSelling && sellProgressBar != null)
        {
            sellProgressBar.SetActive(false);
        }
        if (!isUpgrading && upgradeProgressBar != null)
        {
            upgradeProgressBar.SetActive(false);
        }
        
        tooltipPanel.transform.SetAsLastSibling();
        tooltipPanel.SetActive(true);
    }
    
    private void UpdateTooltipButtons(RPGItem item, int slotIndex)
    {
        if (slotIndex < 0) return; // No slot index, can't sell
        
        // Check if near merchant (upgrade uses hotkey 'U', no button)
        PropEnt_Merchant_Base merchant = FindNearbyMerchant();
        
        bool canSell = merchant != null && item.type != ItemType.Quest;
        
        // Create sell button if needed
        if (canSell)
        {
            if (sellButton == null)
            {
                CreateSellButton();
            }
            if (sellButton != null)
            {
                sellButton.SetActive(true);
                int sellPrice = InventoryMerchantHelper.GetSellPrice(item);
                Text buttonText = sellButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = Localization.FormatSellPrice(sellPrice);
                }
            }
        }
        else if (sellButton != null)
        {
            sellButton.SetActive(false);
        }
        
        // Hide upgrade button - use hotkey 'U' instead
        if (upgradeButton != null)
        {
            upgradeButton.SetActive(false);
        }
    }
    
    private void CreateSellButton()
    {
        if (tooltipPanel == null) return;
        
        sellButton = new GameObject("SellButton");
        sellButton.transform.SetParent(tooltipPanel.transform, false);
        
        RectTransform buttonRect = sellButton.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 0);
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.sizeDelta = new Vector2(-10, 25);
        buttonRect.anchoredPosition = new Vector2(0, 5);
        
        Image buttonBg = sellButton.AddComponent<Image>();
        buttonBg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(sellButton.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = Localization.Sell;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 11 to 13 for better readability
        buttonText.fontSize = 13;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Button button = sellButton.AddComponent<Button>();
        button.onClick.AddListener(() => {
            if (tooltipItemSlotIndex >= 0)
            {
                SellItem(tooltipItemSlotIndex);
                HideTooltip();
            }
        });
    }
    
    private void CreateUpgradeButton()
    {
        if (tooltipPanel == null) return;
        
        upgradeButton = new GameObject("UpgradeButton");
        upgradeButton.transform.SetParent(tooltipPanel.transform, false);
        
        RectTransform buttonRect = upgradeButton.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 0);
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.sizeDelta = new Vector2(-10, 25);
        
        // Position below sell button if it exists
        float yOffset = sellButton != null ? 30f : 5f;
        buttonRect.anchoredPosition = new Vector2(0, yOffset);
        
        Image buttonBg = upgradeButton.AddComponent<Image>();
        buttonBg.color = new Color(0.2f, 0.6f, 0.8f, 0.9f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(upgradeButton.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = Localization.Upgrade;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 11 to 13 for better readability
        buttonText.fontSize = 13;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Button button = upgradeButton.AddComponent<Button>();
        button.onClick.AddListener(() => {
            if (tooltipItemSlotIndex >= 0)
            {
                UpgradeItem(tooltipItemSlotIndex);
                HideTooltip();
            }
            else if (tooltipEquipmentSlot.HasValue)
            {
                UpgradeEquippedItem(tooltipEquipmentSlot.Value);
                HideTooltip();
            }
        });
    }
    
    private void CreateProgressBarsIfNeeded()
    {
        if (tooltipPanel == null) return;
        
        // Create sell progress bar if needed - as a layout child of tooltip
        if (sellProgressBar == null)
        {
            sellProgressBar = new GameObject("SellProgressBar");
            sellProgressBar.transform.SetParent(tooltipPanel.transform, false);
            
            // Use LayoutElement to participate in VerticalLayoutGroup
            LayoutElement sellLayout = sellProgressBar.AddComponent<LayoutElement>();
            sellLayout.minHeight = 10;
            sellLayout.preferredHeight = 10;
            sellLayout.flexibleWidth = 1;
            
            Image progressBg = sellProgressBar.AddComponent<Image>();
            progressBg.color = new Color(0.1f, 0.1f, 0.12f, 1f);
            
            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(sellProgressBar.transform, false);
            sellProgressFill = fillObj.AddComponent<Image>();
            sellProgressFill.color = new Color(1f, 0.5f, 0.2f, 1f); // Orange for sell
            
            RectTransform fillRect = sellProgressFill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            
            sellProgressBar.SetActive(false);
        }
        
        // Create upgrade progress bar if needed - as a layout child
        if (upgradeProgressBar == null)
        {
            upgradeProgressBar = new GameObject("UpgradeProgressBar");
            upgradeProgressBar.transform.SetParent(tooltipPanel.transform, false);
            
            // Use LayoutElement to participate in VerticalLayoutGroup
            LayoutElement upgradeLayout = upgradeProgressBar.AddComponent<LayoutElement>();
            upgradeLayout.minHeight = 10;
            upgradeLayout.preferredHeight = 10;
            upgradeLayout.flexibleWidth = 1;
            
            Image progressBg = upgradeProgressBar.AddComponent<Image>();
            progressBg.color = new Color(0.1f, 0.1f, 0.12f, 1f);
            
            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(upgradeProgressBar.transform, false);
            upgradeProgressFill = fillObj.AddComponent<Image>();
            upgradeProgressFill.color = new Color(0.3f, 0.8f, 1f, 1f); // Cyan for upgrade
            
            RectTransform fillRect = upgradeProgressFill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            
            upgradeProgressBar.SetActive(false);
        }
        
        // Create dismantle progress bar if needed - as a layout child
        if (dismantleProgressBar == null)
        {
            dismantleProgressBar = new GameObject("DismantleProgressBar");
            dismantleProgressBar.transform.SetParent(tooltipPanel.transform, false);
            
            LayoutElement dismantleLayout = dismantleProgressBar.AddComponent<LayoutElement>();
            dismantleLayout.minHeight = 10;
            dismantleLayout.preferredHeight = 10;
            dismantleLayout.flexibleWidth = 1;
            
            Image progressBg = dismantleProgressBar.AddComponent<Image>();
            progressBg.color = new Color(0.1f, 0.1f, 0.12f, 1f);
            
            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(dismantleProgressBar.transform, false);
            dismantleProgressFill = fillObj.AddComponent<Image>();
            dismantleProgressFill.color = new Color(0.8f, 0.3f, 0.3f, 1f); // Red for dismantle
            
            RectTransform fillRect = dismantleProgressFill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            
            dismantleProgressBar.SetActive(false);
        }
        
        // Create cleanse progress bar if needed - as a layout child
        if (cleanseProgressBar == null)
        {
            cleanseProgressBar = new GameObject("CleanseProgressBar");
            cleanseProgressBar.transform.SetParent(tooltipPanel.transform, false);
            
            LayoutElement cleanseLayout = cleanseProgressBar.AddComponent<LayoutElement>();
            cleanseLayout.minHeight = 10;
            cleanseLayout.preferredHeight = 10;
            cleanseLayout.flexibleWidth = 1;
            
            Image progressBg = cleanseProgressBar.AddComponent<Image>();
            progressBg.color = new Color(0.1f, 0.1f, 0.12f, 1f);
            
            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(cleanseProgressBar.transform, false);
            cleanseProgressFill = fillObj.AddComponent<Image>();
            cleanseProgressFill.color = new Color(0.6f, 0.3f, 0.8f, 1f); // Purple for cleanse
            
            RectTransform fillRect = cleanseProgressFill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            
            cleanseProgressBar.SetActive(false);
        }
    }
    
    private string BuildTooltipContent(RPGItem item)
    {
        // Use inventory-specific tooltip with large title
        return item.GetInventoryTooltip();
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
        if (sellButton != null)
            sellButton.SetActive(false);
        if (upgradeButton != null)
            upgradeButton.SetActive(false);
        tooltipItemSlotIndex = -1;
        hoveredSlotIndex = -1; // Clear hovered slot
        tooltipEquipmentSlot = null; // Clear equipment slot
        
        // Reset progress when tooltip is hidden
        sellProgress = 0f;
        upgradeProgress = 0f;
        isSelling = false;
        isUpgrading = false;
        if (sellProgressBar != null) sellProgressBar.SetActive(false);
        if (upgradeProgressBar != null) upgradeProgressBar.SetActive(false);
    }

    private bool needsRefresh = true;
    
    public void RefreshInventory()
    {
        if (inventoryManager == null || itemSlots == null) return;
        
        // Refresh backpack items
        for (int i = 0; i < itemSlots.Count && i < inventoryManager.MaxSlots; i++)
        {
            RPGItem item = inventoryManager.GetItem(i);
            if (itemSlots[i] != null)
            {
                itemSlots[i].SetItem(item);
            }
        }
        
        // Refresh equipment slots from EquipmentManager
        RefreshEquipmentSlots();
        
        // Refresh set bonus display
        UpdateSetBonusDisplay();
        
        needsRefresh = false;
    }
    
    /// <summary>
    /// Refresh fast slots to show current consumable states
    /// </summary>
    public void RefreshFastSlots()
    {
        // Find all FastSlotUI components and update them
        FastSlotUI[] fastSlots = GetComponentsInChildren<FastSlotUI>(true);
        foreach (FastSlotUI slot in fastSlots)
        {
            slot.UpdateVisuals();
        }
    }
    
    /// <summary>
    /// Clear all fast slots (for new runs)
    /// </summary>
    public void ClearAllFastSlots()
    {
        // Clearing fast slots
        FastSlotUI[] allFastSlots = GetComponentsInChildren<FastSlotUI>(true);
        foreach (FastSlotUI slot in allFastSlots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
            }
        }
    }
    
    private void RefreshEquipmentSlots()
    {
        if (equipmentManager == null) return;
        
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot != null)
            {
                RPGItem equipped = equipmentManager.GetEquipped(slot.SlotType);
                slot.SetItem(equipped);
            }
        }
    }
    
    public void MarkDirty()
    {
        needsRefresh = true;
    }
    
    /// <summary>
    /// Return a consumable from fast slot back to inventory
    /// </summary>
    public void ReturnConsumableToInventory(RPGItem item)
    {
        if (item == null || item.type != ItemType.Consumable) return;
        
        // Add back to inventory
        inventoryManager.AddItem(item);
        
        MarkDirty();
        RefreshInventory();
        
        RPGLog.Debug(" Returned consumable to inventory: " + item.name);
    }
    
    public void UpdateStatsDisplay()
    {
        if (equipmentManager != null && statsText != null)
        {
            // Use full detailed stats for Diablo layout (more space available)
            if (uiStyle == UIStyle.DiabloStyle)
            {
                statsText.text = equipmentManager.GetFullDetailedStats();
            }
            else
        {
            statsText.text = equipmentManager.GetDetailedStats();
            }
        }
    }

    private void Update()
    {
        // Check for screen resolution or UI scale changes
        CheckForScaleChanges();
        
        if (needsRefresh && canvasGroup != null && canvasGroup.interactable && canvasGroup.alpha > 0)
        {
            RefreshInventory();
        }
        
        // Update stats display continuously while visible (like game UI does)
        // This ensures finalStats sync from server is reflected
        if (canvasGroup != null && canvasGroup.alpha > 0)
        {
            UpdateStatsDisplay();
        }
        
        // Auto pickup check (every AUTO_PICKUP_CHECK_INTERVAL seconds) - runs even when inventory is closed
        if (autoPickupEnabled)
        {
            if (Time.time - lastAutoPickupCheck >= AUTO_PICKUP_CHECK_INTERVAL)
            {
                lastAutoPickupCheck = Time.time;
                CheckAndAutoPickupItems();
                
                // Clean up expired cooldowns periodically
                CleanupExpiredPickupCooldowns();
            }
        }
        
        // Auto merge check (every AUTO_MERGE_CHECK_INTERVAL seconds) - continuously merges inventory items
        if (autoMergeEnabled)
        {
            if (Time.time - lastAutoMergeCheck >= AUTO_MERGE_CHECK_INTERVAL)
            {
                lastAutoMergeCheck = Time.time;
                CheckAndAutoMergeInventory();
            }
        }
        
        // Check for upgrade hotkey 'U' (hold-to-confirm)
        if (Input.GetKey(KeyCode.U) && !ControlManager.IsInputFieldFocused())
        {
            // Use hoveredSlotIndex or tooltipItemSlotIndex as fallback
            int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
            
            // Try inventory slot first, then equipment slot
            RPGItem item = null;
            if (targetSlot >= 0)
            {
                item = inventoryManager.GetItem(targetSlot);
            }
            else if (tooltipEquipmentSlot.HasValue)
            {
                item = equipmentManager.GetEquippedItem(tooltipEquipmentSlot.Value);
            }
            
            if (item != null)
            {
                // Check if near upgrade well
                Shrine_UpgradeWell upgradeWell = FindNearbyUpgradeWell();
                if (upgradeWell != null)
                {
                    // Check if item can be upgraded
                    if (item.type != ItemType.Consumable && 
                        item.type != ItemType.Material && 
                        item.type != ItemType.Quest)
                    {
                        // Check if player can afford the upgrade
                        int upgradeCost = InventoryMerchantHelper.GetUpgradeCost(item);
                        if (DewPlayer.local != null && DewPlayer.local.dreamDust >= upgradeCost)
                        {
                            // Accumulate progress (hold-to-confirm)
                            float currentTime = Time.time;
                            if (currentTime - lastUpgradeTapTime >= ActionTapMinInterval)
                            {
                                lastUpgradeTapTime = currentTime;
                                upgradeProgress += ActionTapStrength;
                                isUpgrading = true;
                                
                                // Play upgrade tap sound effect
                                PlayUpgradeTapSound(upgradeWell);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // Key released - stop accumulating
            if (isUpgrading)
            {
                isUpgrading = false;
            }
        }
        
        // Check for sell hotkey (configurable, default: S) (hold-to-confirm)
        // Ctrl + key = INSTANT sell (skip confirmation)
        bool sellKeyHeld = false;
        if (RPGItemsMod.Instance != null && RPGItemsMod.Instance.config != null)
        {
            string sellKeyStr = RPGItemsMod.Instance.config.sellKey;
            if (!string.IsNullOrEmpty(sellKeyStr))
            {
                try
                {
                    KeyCode sellKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), sellKeyStr, true);
                    sellKeyHeld = Input.GetKey(sellKeyCode);
                }
                catch
                {
                    // Fallback to S if invalid key string
                    sellKeyHeld = Input.GetKey(KeyCode.S);
                }
            }
            else
            {
                sellKeyHeld = Input.GetKey(KeyCode.S);
            }
        }
        else
        {
            sellKeyHeld = Input.GetKey(KeyCode.S);
        }
        bool ctrlHeldForSell = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        
        if (sellKeyHeld && !ControlManager.IsInputFieldFocused())
        {
            // Use hoveredSlotIndex or tooltipItemSlotIndex as fallback
            int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
            
            if (targetSlot >= 0)
            {
                RPGItem item = inventoryManager.GetItem(targetSlot);
                if (item != null)
                {
                    // Check if near merchant
                    PropEnt_Merchant_Base merchant = FindNearbyMerchant();
                    if (merchant != null && item.type != ItemType.Quest)
                    {
                        // INSTANT sell with Ctrl held - skip confirmation!
                        if (ctrlHeldForSell)
                        {
                            // Only trigger once per key press
                            if (!_ctrlSellTriggered)
                            {
                                _ctrlSellTriggered = true;
                                ExecuteSellItem(targetSlot);
                                PlaySellTapSound(merchant);
                                string sellKeyName = (RPGItemsMod.Instance != null && RPGItemsMod.Instance.config != null) ? RPGItemsMod.Instance.config.sellKey : "S";
                                RPGLog.Debug(" Instant sell (Ctrl+" + sellKeyName + "): " + item.name);
                            }
                        }
                        else
                        {
                            // Normal hold-to-confirm behavior
                            _ctrlSellTriggered = false;
                            
                            // Accumulate progress (hold-to-confirm)
                            float currentTime = Time.time;
                            if (currentTime - lastSellTapTime >= ActionTapMinInterval)
                            {
                                lastSellTapTime = currentTime;
                                sellProgress += ActionTapStrength;
                                isSelling = true;
                                
                                // Play sell tap sound effect
                                PlaySellTapSound(merchant);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // Key released - stop accumulating
            if (isSelling)
            {
                isSelling = false;
            }
            _ctrlSellTriggered = false;
        }
        
        // Check for dismantle hotkey (hold-to-confirm) - uses player's configured interactAlt key
        // Ctrl + dismantle key = INSTANT dismantle (skip confirmation)
        bool dismantleKeyHeld = DewInput.GetButton(DewSave.profileMain.controls.interactAlt, checkGameAreaForMouse: false);
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        
        if (dismantleKeyHeld && !ControlManager.IsInputFieldFocused())
        {
            int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
            
            if (targetSlot >= 0)
            {
                RPGItem item = inventoryManager.GetItem(targetSlot);
                if (item != null)
                {
                    // Dismantle works anywhere - check item type only
                    if (item.type != ItemType.Quest && 
                        item.type != ItemType.Consumable && item.type != ItemType.Material)
                    {
                        // INSTANT dismantle with Ctrl held - skip confirmation!
                        if (ctrlHeld)
                        {
                            // Only trigger once per key press
                            if (!_ctrlDismantleTriggered)
                            {
                                _ctrlDismantleTriggered = true;
                                ExecuteDismantleItem(targetSlot);
                                PlayDismantleTapSound();
                                RPGLog.Debug(" Instant dismantle (Ctrl+key): " + item.name);
                            }
                        }
                        else
                        {
                            // Normal hold-to-confirm behavior
                            _ctrlDismantleTriggered = false;
                            
                            // Accumulate progress (hold-to-confirm)
                            float currentTime = Time.time;
                            if (currentTime - lastDismantleTapTime >= ActionTapMinInterval)
                            {
                                lastDismantleTapTime = currentTime;
                                dismantleProgress += ActionTapStrength;
                                isDismantling = true;
                                
                                // Play dismantle tap sound effect
                                PlayDismantleTapSound();
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (isDismantling)
            {
                isDismantling = false;
            }
            _ctrlDismantleTriggered = false;
        }
        
        // Check for cleanse hotkey 'C' (hold-to-confirm) - near cleanse sink
        if (Input.GetKey(KeyCode.C) && !ControlManager.IsInputFieldFocused())
        {
            int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
            
            if (targetSlot >= 0)
            {
                RPGItem item = inventoryManager.GetItem(targetSlot);
                if (item != null)
                {
                    // Check if near cleanse altar
                    Shrine_AltarOfCleansing cleanseAltar = FindNearbyCleanseAltar();
                    if (cleanseAltar != null && InventoryMerchantHelper.CanCleanse(item))
                    {
                        // Check if player can afford the cleanse
                        int cleanseCost = InventoryMerchantHelper.GetCleanseCost(item);
                        if (DewPlayer.local != null && DewPlayer.local.gold >= cleanseCost)
                        {
                            // Accumulate progress (hold-to-confirm)
                            float currentTime = Time.time;
                            if (currentTime - lastCleanseTapTime >= ActionTapMinInterval)
                            {
                                lastCleanseTapTime = currentTime;
                                cleanseProgress += ActionTapStrength;
                                isCleansing = true;
                                
                                // Play cleanse tap sound effect
                                PlayCleanseTapSound();
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (isCleansing)
            {
                isCleansing = false;
            }
        }
        
        // Update progress bars and check for completion
        UpdateActionProgress();
    }
    
    private void UpdateActionProgress()
    {
        float currentTime = Time.time;
        
        // Update sell progress
        if (sellProgress > 0f && currentTime - lastSellTapTime > ActionDecayStartTime)
        {
            sellProgress = 0f;
            isSelling = false;
        }
        
        // Ensure progress bars exist
        if (tooltipPanel != null && tooltipPanel.activeSelf && sellProgressBar == null)
        {
            CreateProgressBarsIfNeeded();
        }
        
        if (sellProgressFill != null)
        {
            // Only show if actively selling and conditions are met
            bool shouldShow = sellProgress > 0f && isSelling;
            if (shouldShow)
            {
                // Verify conditions are still met
                int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
                if (targetSlot >= 0)
                {
                    RPGItem item = inventoryManager.GetItem(targetSlot);
                    if (item != null)
                    {
                        PropEnt_Merchant_Base merchant = FindNearbyMerchant();
                        shouldShow = merchant != null && item.type != ItemType.Quest;
                    }
                    else
                    {
                        shouldShow = false;
                    }
                }
                else
                {
                    shouldShow = false;
                }
            }
            
            if (shouldShow)
            {
                float targetFill = Mathf.Clamp01(sellProgress / 0.8f); // Fills at 0.8 (~2-3 taps)
                float currentFill = sellProgressFill.GetComponent<RectTransform>().anchorMax.x;
                float smoothFill = Mathf.SmoothDamp(currentFill, targetFill, ref _sellFillCv, 0.1f);
                
                RectTransform fillRect = sellProgressFill.GetComponent<RectTransform>();
                fillRect.anchorMax = new Vector2(smoothFill, 1);
            }
            
            if (sellProgressBar != null)
            {
                sellProgressBar.SetActive(shouldShow);
            }
        }
        else if (isSelling && sellProgress > 0f)
        {
            // Progress bar not created yet but we're selling - create it now
            CreateProgressBarsIfNeeded();
        }
        
        // Complete sell when progress >= 1.0
        if (sellProgress >= 1f)
        {
            int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
            if (targetSlot >= 0)
            {
                SellItem(targetSlot);
                sellProgress = 0f;
                isSelling = false;
            }
        }
        
        // Update upgrade progress
        if (upgradeProgress > 0f && currentTime - lastUpgradeTapTime > ActionDecayStartTime)
        {
            upgradeProgress = 0f;
            isUpgrading = false;
        }
        
        // Ensure progress bars exist
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            CreateProgressBarsIfNeeded();
        }
        
        if (upgradeProgressFill != null)
        {
            // Only show if actively upgrading and conditions are met
            bool shouldShow = upgradeProgress > 0f && isUpgrading;
            
            if (shouldShow)
            {
                // Verify conditions are still met
                int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
                RPGItem item = null;
                
                if (targetSlot >= 0)
                {
                    item = inventoryManager.GetItem(targetSlot);
                }
                else if (tooltipEquipmentSlot.HasValue)
                {
                    item = equipmentManager.GetEquippedItem(tooltipEquipmentSlot.Value);
                }
                
                if (item != null)
                {
                    Shrine_UpgradeWell upgradeWell = FindNearbyUpgradeWell();
                    shouldShow = upgradeWell != null && 
                        item.type != ItemType.Consumable && 
                        item.type != ItemType.Material && 
                        item.type != ItemType.Quest;
                }
                else
                {
                    shouldShow = false;
                }
            }
            
            if (shouldShow)
            {
                float targetFill = Mathf.Clamp01(upgradeProgress / 0.8f); // Fills at 0.8 (~2-3 taps)
                float currentFill = upgradeProgressFill.GetComponent<RectTransform>().anchorMax.x;
                float smoothFill = Mathf.SmoothDamp(currentFill, targetFill, ref _upgradeFillCv, 0.1f);
                
                RectTransform fillRect = upgradeProgressFill.GetComponent<RectTransform>();
                fillRect.anchorMax = new Vector2(smoothFill, 1);
            }
            
            if (upgradeProgressBar != null)
            {
                upgradeProgressBar.SetActive(shouldShow);
            }
        }
        else if (isUpgrading && upgradeProgress > 0f)
        {
            // Progress bar not created yet but we're upgrading - create it now
            CreateProgressBarsIfNeeded();
        }
        
        // Complete upgrade when progress >= 1.0
        if (upgradeProgress >= 1f)
        {
            int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
            if (targetSlot >= 0)
            {
                UpgradeItem(targetSlot);
                upgradeProgress = 0f;
                isUpgrading = false;
            }
            else if (tooltipEquipmentSlot.HasValue)
            {
                UpgradeEquippedItem(tooltipEquipmentSlot.Value);
                upgradeProgress = 0f;
                isUpgrading = false;
            }
        }
        
        // Update dismantle progress
        if (dismantleProgress > 0f && currentTime - lastDismantleTapTime > ActionDecayStartTime)
        {
            dismantleProgress = 0f;
            isDismantling = false;
        }
        
        if (dismantleProgressFill != null)
        {
            bool shouldShow = dismantleProgress > 0f && isDismantling;
            
            if (shouldShow)
            {
                int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
                if (targetSlot >= 0)
                {
                    RPGItem item = inventoryManager.GetItem(targetSlot);
                    if (item != null)
                    {
                        // Dismantle works anywhere - just check item type
                        shouldShow = item.type != ItemType.Quest && 
                            item.type != ItemType.Consumable && 
                            item.type != ItemType.Material;
                    }
                    else
                    {
                        shouldShow = false;
                    }
                }
                else
                {
                    shouldShow = false;
                }
            }
            
            if (shouldShow)
            {
                float targetFill = Mathf.Clamp01(dismantleProgress / 0.8f);
                float currentFill = dismantleProgressFill.GetComponent<RectTransform>().anchorMax.x;
                float smoothFill = Mathf.SmoothDamp(currentFill, targetFill, ref _dismantleFillCv, 0.1f);
                
                RectTransform fillRect = dismantleProgressFill.GetComponent<RectTransform>();
                fillRect.anchorMax = new Vector2(smoothFill, 1);
            }
            
            if (dismantleProgressBar != null)
            {
                dismantleProgressBar.SetActive(shouldShow);
            }
        }
        else if (isDismantling && dismantleProgress > 0f)
        {
            CreateProgressBarsIfNeeded();
        }
        
        // Complete dismantle when progress >= 1.0
        if (dismantleProgress >= 1f)
        {
            int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
            if (targetSlot >= 0)
            {
                DismantleItem(targetSlot);
                dismantleProgress = 0f;
                isDismantling = false;
            }
        }
        
        // Update cleanse progress
        if (cleanseProgress > 0f && currentTime - lastCleanseTapTime > ActionDecayStartTime)
        {
            cleanseProgress = 0f;
            isCleansing = false;
        }
        
        if (cleanseProgressFill != null)
        {
            bool shouldShow = cleanseProgress > 0f && isCleansing;
            
            if (shouldShow)
            {
                int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
                if (targetSlot >= 0)
                {
                    RPGItem item = inventoryManager.GetItem(targetSlot);
                    if (item != null)
                    {
                        Shrine_AltarOfCleansing cleanseAltar = FindNearbyCleanseAltar();
                        shouldShow = cleanseAltar != null && InventoryMerchantHelper.CanCleanse(item);
                    }
                    else
                    {
                        shouldShow = false;
                    }
                }
                else
                {
                    shouldShow = false;
                }
            }
            
            if (shouldShow)
            {
                float targetFill = Mathf.Clamp01(cleanseProgress / 0.8f);
                float currentFill = cleanseProgressFill.GetComponent<RectTransform>().anchorMax.x;
                float smoothFill = Mathf.SmoothDamp(currentFill, targetFill, ref _cleanseFillCv, 0.1f);
                
                RectTransform fillRect = cleanseProgressFill.GetComponent<RectTransform>();
                fillRect.anchorMax = new Vector2(smoothFill, 1);
            }
            
            if (cleanseProgressBar != null)
            {
                cleanseProgressBar.SetActive(shouldShow);
            }
        }
        else if (isCleansing && cleanseProgress > 0f)
        {
            CreateProgressBarsIfNeeded();
        }
        
        // Complete cleanse when progress >= 1.0
        if (cleanseProgress >= 1f)
        {
            int targetSlot = hoveredSlotIndex >= 0 ? hoveredSlotIndex : tooltipItemSlotIndex;
            if (targetSlot >= 0)
            {
                CleanseItem(targetSlot);
                cleanseProgress = 0f;
                isCleansing = false;
            }
        }
        
        if (dragVisual != null && isDragging)
        {
            RectTransform dragRect = dragVisual.GetComponent<RectTransform>();
            if (dragRect != null)
            {
                dragRect.position = Input.mousePosition;
            }
        }
    }

    public void StartDrag(ItemSlot slot)
    {
        draggedSlot = slot;
        draggedEquipSlot = null;
        isDragging = true;
        
        RPGItem item = inventoryManager.GetItem(slot.SlotIndex);
        CreateDragVisual(item);
    }
    
    public void StartDragFromEquipment(EquipmentSlot equipSlot)
    {
        draggedEquipSlot = equipSlot;
        draggedSlot = null;
        isDragging = true;
        
        RPGItem item = equipSlot.EquippedItem;
        CreateDragVisual(item);
    }
    
    private void CreateDragVisual(RPGItem item)
    {
        if (item == null) return;
        
        // Lazy load sprite if needed
        ItemDatabase.EnsureSprite(item);
        
        if (item.sprite != null)
        {
            dragVisual = new GameObject("DragVisual");
            dragVisual.transform.SetParent(canvas.transform, false);
            
            RectTransform dragRect = dragVisual.AddComponent<RectTransform>();
            dragRect.sizeDelta = new Vector2(50, 50);
            
            Image dragImage = dragVisual.AddComponent<Image>();
            dragImage.sprite = item.sprite;
            dragImage.color = new Color(1f, 1f, 1f, 0.8f);
            dragImage.raycastTarget = false;
            
            CanvasGroup dragCG = dragVisual.AddComponent<CanvasGroup>();
            dragCG.blocksRaycasts = false;
        }
    }

    public void EndDrag(ItemSlot targetSlot)
    {
        if (!isDragging || draggedSlot == null)
        {
            CleanupDragVisual();
            return;
        }

        if (targetSlot != null && targetSlot != draggedSlot)
        {
            RPGItem draggedItem = inventoryManager.GetItem(draggedSlot.SlotIndex);
            RPGItem targetItem = inventoryManager.GetItem(targetSlot.SlotIndex);
            
            // Check if consumables can be stacked (same ID, both are consumables)
            if (CanStackConsumables(draggedItem, targetItem))
            {
                // Stack consumables: add dragged stack to target, remove dragged item
                targetItem.currentStack += draggedItem.currentStack;
                inventoryManager.Items[draggedSlot.SlotIndex] = null;
                RPGLog.Debug(" Stacked consumables: " + targetItem.name + " x" + targetItem.currentStack);
                MarkDirty();
                RefreshInventory();
            }
            // Check if items can be combined (same item, same upgrade level, not consumables)
            else if (CanCombineItems(draggedItem, targetItem))
            {
                // Show merge confirmation dialog instead of combining directly
                ShowMergeConfirmDialog(draggedSlot.SlotIndex, targetSlot.SlotIndex);
                // Don't refresh yet - wait for dialog confirmation
            }
            else
            {
                // Normal swap
                inventoryManager.SwapItems(draggedSlot.SlotIndex, targetSlot.SlotIndex);
                MarkDirty();
                RefreshInventory();
            }
        }

        draggedSlot = null;
        draggedEquipSlot = null;
        isDragging = false;
        CleanupDragVisual();
    }
    
    /// <summary>
    /// Check if two consumable items can be stacked
    /// </summary>
    private bool CanStackConsumables(RPGItem item1, RPGItem item2)
    {
        if (item1 == null || item2 == null) return false;
        if (item1.type != ItemType.Consumable || item2.type != ItemType.Consumable) return false;
        
        // Must be same consumable (same ID)
        return item1.id == item2.id;
    }
    
    /// <summary>
    /// Check if two items can be combined to upgrade
    /// Requirements: Same item ID, same upgrade level, not consumables
    /// </summary>
    private bool CanCombineItems(RPGItem item1, RPGItem item2)
    {
        if (item1 == null || item2 == null) return false;
        if (item1.type == ItemType.Consumable || item2.type == ItemType.Consumable) return false;
        
        // Must be same item (same ID) and same upgrade level
        return item1.id == item2.id && item1.upgradeLevel == item2.upgradeLevel;
    }
    
    /// <summary>
    /// Combine two items: removes source item, upgrades target item by +1
    /// </summary>
    private void CombineItems(int sourceSlotIndex, int targetSlotIndex)
    {
        RPGItem sourceItem = inventoryManager.GetItem(sourceSlotIndex);
        RPGItem targetItem = inventoryManager.GetItem(targetSlotIndex);
        
        if (sourceItem == null || targetItem == null) return;
        
        int oldLevel = targetItem.upgradeLevel;
        
        // Track merge for scoring
        ScoringPatches.TrackItemMerged();
        
        // Upgrade the target item using the helper
        InventoryMerchantHelper.UpgradeItem(targetItem);
        
        // Remove the source item (consumed in combining)
        inventoryManager.Items[sourceSlotIndex] = null;
        
        // Play upgrade sound
        PlayUpgradeCompleteSound();
        
        RPGLog.Debug(" Combined items: " + targetItem.name + " +" + oldLevel + " -> +" + targetItem.upgradeLevel);
    }
    
    /// <summary>
    /// End drag with raycast detection (for equipment slot dragging)
    /// </summary>
    public void EndDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            CleanupDragVisual();
            return;
        }
        
        // Find target under cursor
        ItemSlot targetSlot = null;
        EquipmentSlot targetEquipSlot = null;
        
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            ItemSlot itemSlot = result.gameObject.GetComponent<ItemSlot>();
            if (itemSlot != null)
            {
                targetSlot = itemSlot;
                break;
            }
            
            EquipmentSlot eqSlot = result.gameObject.GetComponent<EquipmentSlot>();
            if (eqSlot != null && eqSlot != draggedEquipSlot)
            {
                targetEquipSlot = eqSlot;
                break;
            }
        }
        
        // Handle equipment slot being dragged
        if (draggedEquipSlot != null)
        {
            RPGItem draggedItem = draggedEquipSlot.EquippedItem;
            
            if (targetSlot != null)
            {
                // Dragged to inventory slot - unequip and place in inventory
                RPGItem existingItem = inventoryManager.GetItem(targetSlot.SlotIndex);
                
                // Unequip the item first
                equipmentManager.Unequip(draggedEquipSlot.SlotType);
                draggedEquipSlot.SetItem(null);
                
                if (existingItem != null && equipmentManager.CanEquip(existingItem, draggedEquipSlot.SlotType))
                {
                    // Swap: equip the existing item and put dragged in its place
                    equipmentManager.Equip(existingItem, draggedEquipSlot.SlotType);
                    draggedEquipSlot.SetItem(existingItem);
                    inventoryManager.Items[targetSlot.SlotIndex] = draggedItem;
                }
                else
                {
                    // Just put the dragged item in the inventory slot
                    inventoryManager.Items[targetSlot.SlotIndex] = draggedItem;
                }
                
                MarkDirty();
                RefreshInventory();
                UpdateStatsDisplay();
                NotifyEquipmentChanged();
            }
            else if (targetEquipSlot != null)
            {
                // Dragged to another equipment slot
                RPGItem targetItem = targetEquipSlot.EquippedItem;
                
                // Check if we can merge (same item, same upgrade level)
                if (CanCombineItems(draggedItem, targetItem))
                {
                    // Show merge confirmation dialog for equipment-to-equipment
                    ShowMergeEquipmentToEquipmentDialog(draggedEquipSlot, targetEquipSlot);
                    return; // Don't clean up yet
                }
                
                // Normal swap if compatible
                if (equipmentManager.CanEquip(draggedItem, targetEquipSlot.SlotType))
                {
                    // Unequip both
                    equipmentManager.Unequip(draggedEquipSlot.SlotType);
                    if (targetItem != null) equipmentManager.Unequip(targetEquipSlot.SlotType);
                    
                    // Equip swapped
                    equipmentManager.Equip(draggedItem, targetEquipSlot.SlotType);
                    targetEquipSlot.SetItem(draggedItem);
                    
                    if (targetItem != null && equipmentManager.CanEquip(targetItem, draggedEquipSlot.SlotType))
                    {
                        equipmentManager.Equip(targetItem, draggedEquipSlot.SlotType);
                        draggedEquipSlot.SetItem(targetItem);
                    }
                    else
                    {
                        draggedEquipSlot.SetItem(null);
                        if (targetItem != null) inventoryManager.AddItem(targetItem);
                    }
                    
                    MarkDirty();
                    RefreshInventory();
                    UpdateStatsDisplay();
                    NotifyEquipmentChanged();
                }
            }
            // Dropped outside - do nothing (don't drop equipped items to world)
        }
        
        draggedSlot = null;
        draggedEquipSlot = null;
        isDragging = false;
        CleanupDragVisual();
    }
    
    public void UpdateDragPosition(Vector2 screenPosition)
    {
        if (dragVisual != null)
        {
            dragVisual.transform.position = screenPosition;
        }
    }
    
    /// <summary>
    /// Cancel the current drag operation without dropping the item
    /// </summary>
    public void CancelDrag()
    {
        draggedSlot = null;
        draggedEquipSlot = null;
        draggedFastSlot = null;
        isDragging = false;
        isDraggingFromFastSlot = false;
        CleanupDragVisual();
    }
    
    // ==================== FAST SLOT DRAG METHODS ====================
    
    /// <summary>
    /// Start dragging from a fast slot (consumable hotbar)
    /// </summary>
    public void StartDragFromFastSlot(FastSlotUI fastSlot)
    {
        draggedFastSlot = fastSlot;
        draggedSlot = null;
        draggedEquipSlot = null;
        isDraggingFromFastSlot = true;
        isDragging = false; // This is specifically for fast slot drag
        
        RPGItem item = fastSlot.CurrentItem;
        CreateDragVisual(item);
    }
    
    /// <summary>
    /// Cancel drag from fast slot
    /// </summary>
    public void CancelDragFromFastSlot()
    {
        draggedFastSlot = null;
        isDraggingFromFastSlot = false;
        CleanupDragVisual();
    }
    
    /// <summary>
    /// Handle dropping fast slot item onto inventory slot
    /// </summary>
    public void OnFastSlotDropToInventory(FastSlotUI sourceSlot, ItemSlot targetSlot)
    {
        if (sourceSlot == null || sourceSlot.CurrentItem == null)
        {
            CancelDragFromFastSlot();
            return;
        }
        
        RPGItem itemToMove = sourceSlot.CurrentItem;
        RPGItem targetItem = inventoryManager.GetItem(targetSlot.SlotIndex);
        
        // If target slot has a consumable, swap them
        if (targetItem != null && targetItem.type == ItemType.Consumable)
        {
            // Check if same consumable - stack them
            if (targetItem.id == itemToMove.id)
            {
                targetItem.currentStack += itemToMove.currentStack;
                
                // Clear the fast slot
                sourceSlot.SetItem(null);
                if (equipmentManager != null)
                {
                    equipmentManager.SetConsumable(sourceSlot.SlotIndex, null);
                }
                
                // Update inventory display
                MarkDirty();
                RefreshInventory();
            }
            else
            {
                // Different consumable - swap
                sourceSlot.SetItem(targetItem);
                if (equipmentManager != null)
                {
                    equipmentManager.SetConsumable(sourceSlot.SlotIndex, targetItem);
                }
                
                inventoryManager.Items[targetSlot.SlotIndex] = itemToMove;
                MarkDirty();
                RefreshInventory();
            }
        }
        else if (targetItem == null)
        {
            // Empty slot - move item there
            inventoryManager.Items[targetSlot.SlotIndex] = itemToMove;
            
            // Clear the fast slot
            sourceSlot.SetItem(null);
            if (equipmentManager != null)
            {
                equipmentManager.SetConsumable(sourceSlot.SlotIndex, null);
            }
            
            MarkDirty();
            RefreshInventory();
        }
        else
        {
            // Target has non-consumable - can't swap, just return to inventory
            sourceSlot.ReturnItemToInventory();
        }
        
        CancelDragFromFastSlot();
    }
    
    /// <summary>
    /// Handle swapping items between two fast slots
    /// </summary>
    public void OnFastSlotSwap(FastSlotUI sourceSlot, FastSlotUI targetSlot)
    {
        if (sourceSlot == null || targetSlot == null)
        {
            CancelDragFromFastSlot();
            return;
        }
        
        RPGItem sourceItem = sourceSlot.CurrentItem;
        RPGItem targetItem = targetSlot.CurrentItem;
        
        // Check if same consumable - stack them
        if (sourceItem != null && targetItem != null && sourceItem.id == targetItem.id)
        {
            targetItem.currentStack += sourceItem.currentStack;
            
            // Clear source slot
            sourceSlot.SetItem(null);
            if (equipmentManager != null)
            {
                equipmentManager.SetConsumable(sourceSlot.SlotIndex, null);
            }
            
            // Update target slot display
            targetSlot.RefreshDisplay();
            if (equipmentManager != null)
            {
                equipmentManager.SetConsumable(targetSlot.SlotIndex, targetItem);
            }
        }
        else
        {
            // Swap items
            sourceSlot.SetItem(targetItem);
            targetSlot.SetItem(sourceItem);
            
            if (equipmentManager != null)
            {
                equipmentManager.SetConsumable(sourceSlot.SlotIndex, targetItem);
                equipmentManager.SetConsumable(targetSlot.SlotIndex, sourceItem);
            }
        }
        
        CancelDragFromFastSlot();
    }
    
    private void CleanupDragVisual()
    {
        if (dragVisual != null)
        {
            UnityEngine.Object.Destroy(dragVisual);
            dragVisual = null;
        }
    }
    
    public void OnEquipmentSlotDrop(EquipmentSlot equipSlot)
    {
        if (draggedSlot == null)
        {
            CleanupDragVisual();
            return;
        }
        
        RPGItem draggedItem = inventoryManager.GetItem(draggedSlot.SlotIndex);
        RPGItem equippedItem = equipSlot.EquippedItem;
        
        // Check if we can merge items (same item, same upgrade level)
        if (CanCombineItems(draggedItem, equippedItem))
        {
            // Show merge confirmation dialog
            ShowMergeWithEquipmentDialog(draggedSlot.SlotIndex, equipSlot);
            // Don't clean up yet - wait for dialog
            return;
        }
        
        if (draggedItem != null && equipmentManager.CanEquip(draggedItem, equipSlot.SlotType))
        {
            RPGItem previousItem = equipmentManager.Equip(draggedItem, equipSlot.SlotType);
            
            if (previousItem != null)
            {
                inventoryManager.Items[draggedSlot.SlotIndex] = previousItem;
            }
            else
            {
                inventoryManager.Items[draggedSlot.SlotIndex] = null;
            }
            
            equipSlot.SetItem(draggedItem);
            MarkDirty();
            RefreshInventory();
            UpdateStatsDisplay();
            NotifyEquipmentChanged();
        }
        else
        {
            // This is expected validation - UI should prevent invalid equips
            RPGLog.Debug(" Cannot equip " + (draggedItem != null ? draggedItem.name + " (" + draggedItem.type + ")" : "null") + " to " + equipSlot.SlotType);
        }
        
        draggedSlot = null;
        isDragging = false;
        CleanupDragVisual();
    }
    
    /// <summary>
    /// Show merge confirmation dialog for merging inventory item with equipped item
    /// </summary>
    private void ShowMergeWithEquipmentDialog(int inventorySlotIndex, EquipmentSlot equipSlot)
    {
        // Clean up drag visual immediately when showing dialog
        CleanupDragVisual();
        isDragging = false;
        
        RPGItem inventoryItem = inventoryManager.GetItem(inventorySlotIndex);
        RPGItem equippedItem = equipSlot.EquippedItem;
        
        if (inventoryItem == null || equippedItem == null)
        {
            draggedSlot = null;
            return;
        }
        
        int currentLevel = equippedItem.upgradeLevel;
        int newLevel = currentLevel + 1;
        
        // Store references for callback
        _mergeEquipInventorySlot = inventorySlotIndex;
        _mergeEquipSlot = equipSlot;
        
        // Create dialog using existing pattern
        if (_mergeEquipDialog == null)
        {
            CreateMergeEquipDialog();
        }
        
        // Update the confirmation text
        if (_mergeEquipText != null)
        {
            string itemName = equippedItem.GetLocalizedName();
            string statsComparison = BuildStatsComparisonString(equippedItem);
            
            string message = string.Format("<b><color={0}>{1}</color></b>\n", 
                equippedItem.GetRarityColorHex(), itemName);
            message += string.Format("<color=#aaaaaa>+{0} → <color=#55efc4>+{1}</color></color>\n\n", 
                currentLevel, newLevel);
            message += statsComparison;
            message += "\n<color=#ff6b6b><size=11>" + Localization.MergeWarning + "</size></color>";
            
            _mergeEquipText.text = message;
        }
        
        _mergeEquipDialog.SetActive(true);
    }
    
    // Merge with equipment dialog state
    private GameObject _mergeEquipDialog;
    private int _mergeEquipInventorySlot = -1;
    private EquipmentSlot _mergeEquipSlot = null;
    private Text _mergeEquipText;
    
    private void CreateMergeEquipDialog()
    {
        _mergeEquipDialog = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(300, 220), "MergeEquipDialog");
        RectTransform dialogRect = _mergeEquipDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(_mergeEquipDialog.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.MergeItems;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 16 to 18 for better readability
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.9f, 0.85f, 0.7f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleRect.anchoredPosition = new Vector2(0, -15);
        
        // Message
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(_mergeEquipDialog.transform, false);
        _mergeEquipText = messageObj.AddComponent<Text>();
        _mergeEquipText.text = "";
        _mergeEquipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 13 to 15 for better readability
        _mergeEquipText.fontSize = 15;
        _mergeEquipText.alignment = TextAnchor.MiddleCenter;
        _mergeEquipText.color = Color.white;
        _mergeEquipText.supportRichText = true;
        _mergeEquipText.lineSpacing = 1.1f;
        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.2f);
        messageRect.anchorMax = new Vector2(1, 0.88f);
        messageRect.sizeDelta = new Vector2(-20, 0);
        messageRect.anchoredPosition = Vector2.zero;
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(_mergeEquipDialog.transform, Localization.Confirm, new Vector2(80, 28));
        confirmBtn.onClick.AddListener(OnMergeEquipConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-50, 22);
        Image confirmImage = confirmBtn.GetComponent<Image>();
        if (confirmImage != null) confirmImage.color = new Color(0.2f, 0.6f, 0.3f);
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(_mergeEquipDialog.transform, Localization.Cancel, new Vector2(80, 28));
        cancelBtn.onClick.AddListener(OnMergeEquipCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(50, 22);
        
        _mergeEquipDialog.SetActive(false);
    }
    
    private void OnMergeEquipConfirm()
    {
        if (_mergeEquipInventorySlot >= 0 && _mergeEquipSlot != null)
        {
            RPGItem equippedItem = _mergeEquipSlot.EquippedItem;
            RPGItem sourceItem = inventoryManager.GetItem(_mergeEquipInventorySlot);
            
            // Validate items still exist and are valid for merge
            if (equippedItem != null && sourceItem != null && CanCombineItems(sourceItem, equippedItem))
            {
                int oldLevel = equippedItem.upgradeLevel;
                EquipmentSlotType slotType = _mergeEquipSlot.SlotType;
                
                // Unequip first to remove old stats
                equipmentManager.Unequip(slotType);
                
                // Properly upgrade the item (updates stats, not just level)
                InventoryMerchantHelper.UpgradeItem(equippedItem);
                
                // Remove the source item from inventory (consumed)
                inventoryManager.Items[_mergeEquipInventorySlot] = null;
                
                // Re-equip the upgraded item to apply new stats
                equipmentManager.Equip(equippedItem, slotType);
                _mergeEquipSlot.SetItem(equippedItem);
                
                // Play upgrade sound
                PlayUpgradeCompleteSound();
                
                RPGLog.Debug(" Merged to equipment: " + equippedItem.name + " +" + oldLevel + " -> +" + equippedItem.upgradeLevel);
                
                MarkDirty();
                RefreshInventory();
                UpdateStatsDisplay();
                NotifyEquipmentChanged();
            }
            else
            {
                RPGLog.Debug(" Merge cancelled - items no longer valid for merge");
            }
        }
        CloseMergeEquipDialog();
    }
    
    private void OnMergeEquipCancel()
    {
        CloseMergeEquipDialog();
    }
    
    private void CloseMergeEquipDialog()
    {
        if (_mergeEquipDialog != null)
        {
            _mergeEquipDialog.SetActive(false);
        }
        _mergeEquipInventorySlot = -1;
        _mergeEquipSlot = null;
        
        // Clean up drag state
        draggedSlot = null;
        isDragging = false;
        CleanupDragVisual();
    }
    
    /// <summary>
    /// Show merge confirmation dialog for merging two equipped items
    /// </summary>
    private void ShowMergeEquipmentToEquipmentDialog(EquipmentSlot sourceSlot, EquipmentSlot targetSlot)
    {
        // Clean up drag visual immediately when showing dialog
        CleanupDragVisual();
        isDragging = false;
        
        RPGItem sourceItem = sourceSlot.EquippedItem;
        RPGItem targetItem = targetSlot.EquippedItem;
        
        if (sourceItem == null || targetItem == null)
        {
            draggedEquipSlot = null;
            return;
        }
        
        int currentLevel = targetItem.upgradeLevel;
        int newLevel = currentLevel + 1;
        
        // Store references for callback
        _mergeEquipSourceSlot = sourceSlot;
        _mergeEquipTargetSlot = targetSlot;
        
        // Create dialog using existing pattern
        if (_mergeEquipToEquipDialog == null)
        {
            CreateMergeEquipToEquipDialog();
        }
        
        // Update the confirmation text
        if (_mergeEquipToEquipText != null)
        {
            string itemName = targetItem.GetLocalizedName();
            string statsComparison = BuildStatsComparisonString(targetItem);
            
            string message = string.Format("<b><color={0}>{1}</color></b>\n", 
                targetItem.GetRarityColorHex(), itemName);
            message += string.Format("<color=#aaaaaa>+{0} → <color=#55efc4>+{1}</color></color>\n\n", 
                currentLevel, newLevel);
            message += statsComparison;
            message += "\n<color=#ff6b6b><size=11>" + Localization.MergeWarning + "</size></color>";
            
            _mergeEquipToEquipText.text = message;
        }
        
        _mergeEquipToEquipDialog.SetActive(true);
    }
    
    // Merge equipment to equipment dialog state
    private GameObject _mergeEquipToEquipDialog;
    private EquipmentSlot _mergeEquipSourceSlot = null;
    private EquipmentSlot _mergeEquipTargetSlot = null;
    private Text _mergeEquipToEquipText;
    
    private void CreateMergeEquipToEquipDialog()
    {
        _mergeEquipToEquipDialog = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(300, 220), "MergeEquipToEquipDialog");
        RectTransform dialogRect = _mergeEquipToEquipDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(_mergeEquipToEquipDialog.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.MergeItems;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 16 to 18 for better readability
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.9f, 0.85f, 0.7f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleRect.anchoredPosition = new Vector2(0, -15);
        
        // Message
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(_mergeEquipToEquipDialog.transform, false);
        _mergeEquipToEquipText = messageObj.AddComponent<Text>();
        _mergeEquipToEquipText.text = "";
        _mergeEquipToEquipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _mergeEquipToEquipText.fontSize = 13;
        _mergeEquipToEquipText.alignment = TextAnchor.MiddleCenter;
        _mergeEquipToEquipText.color = Color.white;
        _mergeEquipToEquipText.supportRichText = true;
        _mergeEquipToEquipText.lineSpacing = 1.1f;
        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.2f);
        messageRect.anchorMax = new Vector2(1, 0.88f);
        messageRect.sizeDelta = new Vector2(-20, 0);
        messageRect.anchoredPosition = Vector2.zero;
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(_mergeEquipToEquipDialog.transform, Localization.Confirm, new Vector2(80, 28));
        confirmBtn.onClick.AddListener(OnMergeEquipToEquipConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-50, 22);
        Image confirmImage = confirmBtn.GetComponent<Image>();
        if (confirmImage != null) confirmImage.color = new Color(0.2f, 0.6f, 0.3f);
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(_mergeEquipToEquipDialog.transform, Localization.Cancel, new Vector2(80, 28));
        cancelBtn.onClick.AddListener(OnMergeEquipToEquipCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(50, 22);
        
        _mergeEquipToEquipDialog.SetActive(false);
    }
    
    private void OnMergeEquipToEquipConfirm()
    {
        if (_mergeEquipSourceSlot != null && _mergeEquipTargetSlot != null)
        {
            RPGItem sourceItem = _mergeEquipSourceSlot.EquippedItem;
            RPGItem targetItem = _mergeEquipTargetSlot.EquippedItem;
            
            // Validate items still exist and are valid for merge
            if (sourceItem != null && targetItem != null && CanCombineItems(sourceItem, targetItem))
            {
                int oldLevel = targetItem.upgradeLevel;
                EquipmentSlotType sourceSlotType = _mergeEquipSourceSlot.SlotType;
                EquipmentSlotType targetSlotType = _mergeEquipTargetSlot.SlotType;
                
                // Unequip both to remove stats
                equipmentManager.Unequip(sourceSlotType);
                equipmentManager.Unequip(targetSlotType);
                
                // Properly upgrade the target item (updates stats, not just level)
                InventoryMerchantHelper.UpgradeItem(targetItem);
                
                // Source item is consumed - don't re-equip it
                _mergeEquipSourceSlot.SetItem(null);
                
                // Re-equip the upgraded target item
                equipmentManager.Equip(targetItem, targetSlotType);
                _mergeEquipTargetSlot.SetItem(targetItem);
                
                // Play upgrade sound
                PlayUpgradeCompleteSound();
                
                RPGLog.Debug(" Merged equipment: " + targetItem.name + " +" + oldLevel + " -> +" + targetItem.upgradeLevel);
                
                MarkDirty();
                RefreshInventory();
                UpdateStatsDisplay();
                NotifyEquipmentChanged();
            }
            else
            {
                RPGLog.Debug(" Merge cancelled - items no longer valid for merge");
            }
        }
        CloseMergeEquipToEquipDialog();
    }
    
    private void OnMergeEquipToEquipCancel()
    {
        CloseMergeEquipToEquipDialog();
    }
    
    private void CloseMergeEquipToEquipDialog()
    {
        if (_mergeEquipToEquipDialog != null)
        {
            _mergeEquipToEquipDialog.SetActive(false);
        }
        _mergeEquipSourceSlot = null;
        _mergeEquipTargetSlot = null;
        
        // Clean up drag state
        draggedSlot = null;
        draggedEquipSlot = null;
        isDragging = false;
        CleanupDragVisual();
    }
    
    public void OnConsumableSlotDrop(ConsumableSlot slot)
    {
        if (draggedSlot == null)
        {
            CleanupDragVisual();
            return;
        }
        
        int sourceSlotIndex = draggedSlot.SlotIndex;
        RPGItem draggedItem = inventoryManager.GetItem(sourceSlotIndex);
        
        if (draggedItem != null && draggedItem.type == ItemType.Consumable)
        {
            // Get existing item from EquipmentManager (the source of truth)
            RPGItem existingItem = null;
            if (equipmentManager != null)
            {
                existingItem = equipmentManager.GetConsumable(slot.SlotIndex);
            }
            
            // Fallback to slot if EquipmentManager doesn't have it
            if (existingItem == null)
            {
                existingItem = slot.CurrentItem;
            }
            
            RPGLog.Debug(" ConsumableSlotDrop: dragged=" + draggedItem.name + " x" + draggedItem.currentStack + 
                      ", existing=" + (existingItem != null ? existingItem.name + " x" + existingItem.currentStack : "null"));
            
            // Check if we should STACK instead of replace
            if (existingItem != null && existingItem.id == draggedItem.id)
            {
                // Same consumable type - STACK them!
                existingItem.currentStack += draggedItem.currentStack;
                
                // Remove dragged item from inventory (it's been merged)
                inventoryManager.RemoveItem(sourceSlotIndex);
                
                // Sync the updated stack count to EquipmentManager and refresh display
                slot.SetItemAndSync(existingItem);
                
                MarkDirty();
                RefreshInventory();
                RefreshFastSlots();
                
                RPGLog.Debug(" Stacked consumable in HUD slot: " + existingItem.name + " x" + existingItem.currentStack);
            }
            else if (existingItem != null)
            {
                // Different consumable type - SWAP them
                // Return existing item to inventory first
                inventoryManager.Items[sourceSlotIndex] = existingItem;
                
                // Put dragged item in consumable slot
                slot.SetItemAndSync(draggedItem);
                
                MarkDirty();
                RefreshInventory();
                RefreshFastSlots();
                
                RPGLog.Debug(" Swapped consumable in HUD slot: " + draggedItem.name + " (returned " + existingItem.name + " to inventory)");
            }
            else
            {
                // Empty slot - move the item there (remove from inventory)
                slot.SetItemAndSync(draggedItem);
                
                // Remove from inventory
                inventoryManager.RemoveItem(sourceSlotIndex);
                
                MarkDirty();
                RefreshInventory();
                RefreshFastSlots();
                
                RPGLog.Debug(" Moved consumable to HUD slot, removed from inventory");
            }
        }
        
        draggedSlot = null;
        isDragging = false;
        CleanupDragVisual();
    }
    
    public void UnequipItem(EquipmentSlot equipSlot)
    {
        RPGItem item = equipmentManager.Unequip(equipSlot.SlotType);
        if (item != null)
        {
            inventoryManager.AddItem(item);
            equipSlot.SetItem(null);
            MarkDirty();
            RefreshInventory();
            UpdateStatsDisplay();
            NotifyEquipmentChanged();
        }
    }
    
    /// <summary>
    /// Right-click on inventory slot to equip or use item
    /// </summary>
    public void RightClickItem(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        // Consumables - use them
        if (item.type == ItemType.Consumable)
        {
            if (equipmentManager != null)
            {
                equipmentManager.UsePotion(item);
                item.currentStack--;
                
                if (item.currentStack <= 0)
                {
                    inventoryManager.Items[slotIndex] = null;
                }
                
                MarkDirty();
                RefreshInventory();
            }
            return;
        }
        
        // Equipment - try to equip to the appropriate slot
        EquipmentSlotType? targetSlot = GetSlotForItemType(item.type);
        if (targetSlot.HasValue)
        {
            // Check if the slot is empty or can swap
            RPGItem previousItem = equipmentManager.Equip(item, targetSlot.Value);
            
            if (previousItem != null)
            {
                // Swap - put old item back in inventory
                inventoryManager.Items[slotIndex] = previousItem;
            }
            else
            {
                // Slot was empty - remove from inventory
                inventoryManager.Items[slotIndex] = null;
            }
            
            // Update the equipment slot UI
            foreach (EquipmentSlot slot in equipmentSlots)
            {
                if (slot.SlotType == targetSlot.Value)
                {
                    slot.SetItem(item);
                    break;
                }
            }
            
            MarkDirty();
            RefreshInventory();
            UpdateStatsDisplay();
            NotifyEquipmentChanged();
        }
    }
    
    /// <summary>
    /// Get the primary equipment slot for an item type
    /// For items with multiple possible slots (weapons, rings), returns the first available or primary slot
    /// </summary>
    private EquipmentSlotType? GetSlotForItemType(ItemType type)
    {
        switch (type)
        {
            case ItemType.Helmet: return EquipmentSlotType.Head;
            case ItemType.ChestArmor: return EquipmentSlotType.Chest;
            case ItemType.Pants: return EquipmentSlotType.Legs;
            case ItemType.Boots: return EquipmentSlotType.Boots;
            case ItemType.Belt: return EquipmentSlotType.Belt;
            case ItemType.Amulet: return EquipmentSlotType.Amulet;
            case ItemType.Weapon:
                // Prefer right hand, then left hand
                if (equipmentManager.GetEquipped(EquipmentSlotType.RightHand) == null)
                    return EquipmentSlotType.RightHand;
                if (equipmentManager.GetEquipped(EquipmentSlotType.LeftHand) == null)
                    return EquipmentSlotType.LeftHand;
                return EquipmentSlotType.RightHand; // Default swap right hand
            case ItemType.TwoHandedWeapon:
                // Two-handed weapons can go in either hand slot
                // Prefer right hand, then left hand
                if (equipmentManager.GetEquipped(EquipmentSlotType.RightHand) == null)
                    return EquipmentSlotType.RightHand;
                if (equipmentManager.GetEquipped(EquipmentSlotType.LeftHand) == null)
                    return EquipmentSlotType.LeftHand;
                return EquipmentSlotType.RightHand; // Default swap right hand
            case ItemType.Ring:
                // Prefer left ring, then right ring
                if (equipmentManager.GetEquipped(EquipmentSlotType.LeftRing) == null)
                    return EquipmentSlotType.LeftRing;
                if (equipmentManager.GetEquipped(EquipmentSlotType.RightRing) == null)
                    return EquipmentSlotType.RightRing;
                return EquipmentSlotType.LeftRing; // Default swap left ring
            default:
                return null; // Not equippable
        }
    }
    
    public void OnFastSlotUIDrop(FastSlotUI fastSlot)
    {
        if (draggedSlot == null)
        {
            CleanupDragVisual();
            return;
        }
        
        int sourceSlotIndex = draggedSlot.SlotIndex;
        RPGItem draggedItem = inventoryManager.GetItem(sourceSlotIndex);
        
        if (draggedItem != null && draggedItem.type == ItemType.Consumable)
        {
            // IMPORTANT: Get existing item from EquipmentManager (the source of truth)
            // FastSlotUI.CurrentItem might be out of sync
            RPGItem existingItem = null;
            if (equipmentManager != null)
            {
                existingItem = equipmentManager.GetConsumable(fastSlot.SlotIndex);
            }
            
            // Fallback to FastSlotUI if EquipmentManager doesn't have it
            if (existingItem == null)
            {
                existingItem = fastSlot.CurrentItem;
            }
            
            RPGLog.Debug(" FastSlotUIDrop: dragged=" + draggedItem.name + " x" + draggedItem.currentStack + 
                      ", existing=" + (existingItem != null ? existingItem.name + " x" + existingItem.currentStack : "null"));
            
            // Check if we should STACK instead of replace
            if (existingItem != null && existingItem.id == draggedItem.id)
            {
                // Same consumable type - STACK them!
                existingItem.currentStack += draggedItem.currentStack;
                
                // Remove dragged item from inventory (it's been merged)
                inventoryManager.RemoveItem(sourceSlotIndex);
                
                // Sync the updated stack count to EquipmentManager FIRST
                if (equipmentManager != null)
                {
                    equipmentManager.SetConsumable(fastSlot.SlotIndex, existingItem);
                }
                
                // Update FastSlotUI display
                fastSlot.SetItem(existingItem);
                
                // Also sync with ConsumableBar
                if (consumableBar != null)
                {
                    ConsumableSlot barSlot = consumableBar.GetSlot(fastSlot.SlotIndex);
                    if (barSlot != null)
                    {
                        barSlot.SetItemAndSync(existingItem);
                    }
                }
                
                MarkDirty();
                RefreshInventory();
                RefreshFastSlots();
                
                RPGLog.Debug(" Stacked consumable in fast slot: " + existingItem.name + " x" + existingItem.currentStack);
            }
            else if (existingItem != null)
            {
                // Different consumable type - SWAP them
                // Return existing item to inventory first
                inventoryManager.Items[sourceSlotIndex] = existingItem;
                
                // Clear the dragged item from any other fast slots
                ClearItemFromAllFastSlots(draggedItem);
                
                // Update EquipmentManager
                if (equipmentManager != null)
                {
                    equipmentManager.SetConsumable(fastSlot.SlotIndex, draggedItem);
                }
                
                // Put dragged item in fast slot
                fastSlot.SetItem(draggedItem);
                
                // Sync with ConsumableBar
                if (consumableBar != null)
                {
                    ConsumableSlot barSlot = consumableBar.GetSlot(fastSlot.SlotIndex);
                    if (barSlot != null)
                    {
                        barSlot.SetItemAndSync(draggedItem);
                    }
                }
                
                MarkDirty();
                RefreshInventory();
                RefreshFastSlots();
                
                RPGLog.Debug(" Swapped consumable in fast slot: " + draggedItem.name + " (returned " + existingItem.name + " to inventory)");
            }
            else
            {
                // Empty slot - just move the item there
                // IMPORTANT: Clear this item from any other fast slots first!
                ClearItemFromAllFastSlots(draggedItem);
                
                // Update EquipmentManager
                if (equipmentManager != null)
                {
                    equipmentManager.SetConsumable(fastSlot.SlotIndex, draggedItem);
                }
                
                // Move item to fast slot
                fastSlot.SetItem(draggedItem);
                
                // Remove from inventory
                inventoryManager.RemoveItem(sourceSlotIndex);
                
                // Sync with ConsumableBar
                if (consumableBar != null)
                {
                    ConsumableSlot barSlot = consumableBar.GetSlot(fastSlot.SlotIndex);
                    if (barSlot != null)
                    {
                        barSlot.SetItemAndSync(draggedItem);
                    }
                }
                
                MarkDirty();
                RefreshInventory();
                RefreshFastSlots();
                
                RPGLog.Debug(" Moved consumable to fast slot, removed from inventory");
            }
        }
        else
        {
            RPGLog.Debug(" Only consumables can be assigned to fast slots");
        }
        
        draggedSlot = null;
        isDragging = false;
        CleanupDragVisual();
    }
    
    /// <summary>
    /// Clear an item from all fast slots (both FastSlotUI and ConsumableBar)
    /// Call this before assigning to a new slot to prevent duplicates
    /// </summary>
    private void ClearItemFromAllFastSlots(RPGItem item)
    {
        if (item == null) return;
        
        // Clear from ConsumableBar (HUD)
        if (consumableBar != null)
        {
            for (int i = 0; i < consumableBar.SlotCount; i++)
            {
                ConsumableSlot barSlot = consumableBar.GetSlot(i);
                if (barSlot != null && barSlot.CurrentItem == item)
                {
                    barSlot.SetItemAndSync(null); // Use sync version to update EquipmentManager
                    // Cleared from bar
                }
            }
        }
        
        // Clear from FastSlotUI (inventory panel)
        FastSlotUI[] fastSlots = GetComponentsInChildren<FastSlotUI>(true);
        foreach (FastSlotUI slot in fastSlots)
        {
            if (slot.GetCurrentItem() == item)
            {
                slot.ClearSlot();
                // Cleared from fast slot
            }
        }
    }
    
    public void DropItemToWorld(int slotIndex)
    {
        DropItemToWorld(slotIndex, -1); // -1 means drop all
    }
    
    /// <summary>
    /// Drop a specific amount of an item to the world.
    /// For stackable items (like potions), this allows sharing partial stacks.
    /// </summary>
    /// <param name="slotIndex">Inventory slot index</param>
    /// <param name="amount">Amount to drop (-1 for all, or specific count)</param>
    public void DropItemToWorld(int slotIndex, int amount)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null)
        {
            CleanupDragVisual();
            draggedSlot = null;
            isDragging = false;
            return;
        }
        
        // Don't allow dropping quest items
        if (item.type == ItemType.Quest)
        {
            // Cannot drop quest items
            CleanupDragVisual();
            draggedSlot = null;
            isDragging = false;
            return;
        }
        
        // Determine how much to drop
        int dropAmount = (amount <= 0 || amount >= item.currentStack) ? item.currentStack : amount;
        
        // Create a clone for the dropped item if we're only dropping part of the stack
        RPGItem droppedItem;
        if (dropAmount < item.currentStack)
        {
            // Drop partial stack
            droppedItem = item.Clone();
            droppedItem.currentStack = dropAmount;
            
            // Reduce original stack
            item.currentStack -= dropAmount;
        }
        else
        {
            // Drop entire stack
            droppedItem = item;
            
            // Clear this item from ALL fast slots and consumable bar before dropping
            ClearItemFromAllFastSlots(item);
            
            // Remove from inventory
            inventoryManager.RemoveItem(slotIndex);
        }
        
        // Drop the item to the game world
        DroppedItem.DropItemAtPlayer(droppedItem);
        
        MarkDirty();
        RefreshInventory();
        RefreshFastSlots();
        
        draggedSlot = null;
        isDragging = false;
        CleanupDragVisual();
    }
    
    /// <summary>
    /// Shows a dialog to choose how many items to drop (for stackable items)
    /// </summary>
    public void ShowDropAmountDialog(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        // If only 1 in stack or not stackable, just drop directly
        if (item.currentStack <= 1)
        {
            DropItemToWorld(slotIndex);
            return;
        }
        
        // Show amount selection UI
        ShowStackSplitDialog(item, slotIndex, true);
    }
    
    // Stack split dialog state
    private GameObject stackSplitDialog;
    private int stackSplitSlotIndex = -1;
    private int stackSplitAmount = 1;
    private bool stackSplitIsForDrop = false;
    private Text stackSplitAmountText;
    
    // Merge confirmation dialog state
    private GameObject mergeConfirmDialog;
    private int mergeSourceSlotIndex = -1;
    private int mergeTargetSlotIndex = -1;
    private Text mergeConfirmText;
    
    // Sell confirmation dialog state
    private GameObject sellConfirmDialog;
    private int sellSlotIndex = -1;
    private Text sellConfirmText;
    
    // Dismantle confirmation dialog state
    private GameObject dismantleConfirmDialog;
    private int dismantleSlotIndex = -1;
    private Text dismantleConfirmText;
    
    // Cleanse confirmation dialog state
    private GameObject cleanseConfirmDialog;
    private int cleanseSlotIndex = -1;
    private Text cleanseConfirmText;
    
    // Consumable drop dialog state (for dropping from consumable bar)
    private GameObject consumableDropDialog;
    private ConsumableSlot consumableDropSlot = null;
    private RPGItem consumableDropItem = null;
    private int consumableDropAmount = 1;
    private Text consumableDropAmountText;
    
    // Upgrade confirmation dialog state
    private GameObject upgradeConfirmDialog;
    private int upgradeSlotIndex = -1;
    private EquipmentSlotType? upgradeEquipmentSlot = null;
    private Text upgradeConfirmText;
    
    /// <summary>
    /// Shows a drop dialog for consumables from the consumable bar
    /// </summary>
    public void ShowConsumableDropDialog(ConsumableSlot slot, RPGItem item)
    {
        if (slot == null || item == null) return;
        
        consumableDropSlot = slot;
        consumableDropItem = item;
        consumableDropAmount = 1;
        
        if (consumableDropDialog == null)
        {
            CreateConsumableDropDialog();
        }
        
        // Update max amount display
        if (consumableDropAmountText != null)
        {
            consumableDropAmountText.text = string.Format("{0} / {1}", consumableDropAmount, item.currentStack);
        }
        
        consumableDropDialog.SetActive(true);
    }
    
    private void CreateConsumableDropDialog()
    {
        // Create dialog panel
        consumableDropDialog = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(200, 120), "ConsumableDropDialog");
        RectTransform dialogRect = consumableDropDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(consumableDropDialog.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.DropAmount;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 14;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = UIHelper.AccentColor;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleRect.anchoredPosition = new Vector2(0, -15);
        
        // Amount display
        GameObject amountObj = new GameObject("Amount");
        amountObj.transform.SetParent(consumableDropDialog.transform, false);
        consumableDropAmountText = amountObj.AddComponent<Text>();
        consumableDropAmountText.text = "1 / 1";
        consumableDropAmountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        consumableDropAmountText.fontSize = 18;
        consumableDropAmountText.alignment = TextAnchor.MiddleCenter;
        consumableDropAmountText.color = Color.white;
        RectTransform amountRect = amountObj.GetComponent<RectTransform>();
        amountRect.anchorMin = new Vector2(0, 0.5f);
        amountRect.anchorMax = new Vector2(1, 0.5f);
        amountRect.sizeDelta = new Vector2(0, 30);
        amountRect.anchoredPosition = new Vector2(0, 5);
        
        // Minus button
        Button minusBtn = UIHelper.CreateButton(consumableDropDialog.transform, "-", new Vector2(30, 30));
        minusBtn.onClick.AddListener(OnConsumableDropMinus);
        RectTransform minusRect = minusBtn.GetComponent<RectTransform>();
        minusRect.anchorMin = new Vector2(0, 0.5f);
        minusRect.anchorMax = new Vector2(0, 0.5f);
        minusRect.anchoredPosition = new Vector2(25, 5);
        
        // Plus button
        Button plusBtn = UIHelper.CreateButton(consumableDropDialog.transform, "+", new Vector2(30, 30));
        plusBtn.onClick.AddListener(OnConsumableDropPlus);
        RectTransform plusRect = plusBtn.GetComponent<RectTransform>();
        plusRect.anchorMin = new Vector2(1, 0.5f);
        plusRect.anchorMax = new Vector2(1, 0.5f);
        plusRect.anchoredPosition = new Vector2(-25, 5);
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(consumableDropDialog.transform, Localization.Drop, new Vector2(80, 25));
        confirmBtn.onClick.AddListener(OnConsumableDropConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-45, 20);
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(consumableDropDialog.transform, Localization.Cancel, new Vector2(80, 25));
        cancelBtn.onClick.AddListener(OnConsumableDropCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(45, 20);
        
        consumableDropDialog.SetActive(false);
    }
    
    private void OnConsumableDropMinus()
    {
        if (consumableDropAmount > 1)
        {
            consumableDropAmount--;
            UpdateConsumableDropDisplay();
        }
    }
    
    private void OnConsumableDropPlus()
    {
        if (consumableDropItem != null && consumableDropAmount < consumableDropItem.currentStack)
        {
            consumableDropAmount++;
            UpdateConsumableDropDisplay();
        }
    }
    
    private void UpdateConsumableDropDisplay()
    {
        if (consumableDropItem != null && consumableDropAmountText != null)
        {
            consumableDropAmountText.text = string.Format("{0} / {1}", consumableDropAmount, consumableDropItem.currentStack);
        }
    }
    
    private void OnConsumableDropConfirm()
    {
        if (consumableDropSlot != null && consumableDropAmount > 0)
        {
            consumableDropSlot.DropItemToGround(consumableDropAmount);
        }
        
        CloseConsumableDropDialog();
    }
    
    private void OnConsumableDropCancel()
    {
        CloseConsumableDropDialog();
    }
    
    private void CloseConsumableDropDialog()
    {
        if (consumableDropDialog != null)
        {
            consumableDropDialog.SetActive(false);
        }
        consumableDropSlot = null;
        consumableDropItem = null;
    }
    
    private void ShowStackSplitDialog(RPGItem item, int slotIndex, bool isForDrop)
    {
        stackSplitSlotIndex = slotIndex;
        stackSplitAmount = 1;
        stackSplitIsForDrop = isForDrop;
        
        if (stackSplitDialog == null)
        {
            CreateStackSplitDialog();
        }
        
        // Update max amount
        if (stackSplitAmountText != null)
        {
            stackSplitAmountText.text = string.Format("{0} / {1}", stackSplitAmount, item.currentStack);
        }
        
        stackSplitDialog.SetActive(true);
    }
    
    private void CreateStackSplitDialog()
    {
        // Create dialog panel
        stackSplitDialog = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(200, 120), "StackSplitDialog");
        RectTransform dialogRect = stackSplitDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(stackSplitDialog.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.DropAmount;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 14;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = UIHelper.AccentColor;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleRect.anchoredPosition = new Vector2(0, -15);
        
        // Amount display
        GameObject amountObj = new GameObject("Amount");
        amountObj.transform.SetParent(stackSplitDialog.transform, false);
        stackSplitAmountText = amountObj.AddComponent<Text>();
        stackSplitAmountText.text = "1 / 1";
        stackSplitAmountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 18 to 22 for better readability
        stackSplitAmountText.fontSize = 22;
        stackSplitAmountText.alignment = TextAnchor.MiddleCenter;
        stackSplitAmountText.color = Color.white;
        RectTransform amountRect = amountObj.GetComponent<RectTransform>();
        amountRect.anchorMin = new Vector2(0, 0.5f);
        amountRect.anchorMax = new Vector2(1, 0.5f);
        amountRect.sizeDelta = new Vector2(0, 30);
        amountRect.anchoredPosition = new Vector2(0, 5);
        
        // Minus button
        Button minusBtn = UIHelper.CreateButton(stackSplitDialog.transform, "-", new Vector2(30, 30));
        minusBtn.onClick.AddListener(OnStackSplitMinus);
        RectTransform minusRect = minusBtn.GetComponent<RectTransform>();
        minusRect.anchorMin = new Vector2(0, 0.5f);
        minusRect.anchorMax = new Vector2(0, 0.5f);
        minusRect.anchoredPosition = new Vector2(25, 5);
        
        // Plus button
        Button plusBtn = UIHelper.CreateButton(stackSplitDialog.transform, "+", new Vector2(30, 30));
        plusBtn.onClick.AddListener(OnStackSplitPlus);
        RectTransform plusRect = plusBtn.GetComponent<RectTransform>();
        plusRect.anchorMin = new Vector2(1, 0.5f);
        plusRect.anchorMax = new Vector2(1, 0.5f);
        plusRect.anchoredPosition = new Vector2(-25, 5);
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(stackSplitDialog.transform, Localization.Drop, new Vector2(80, 25));
        confirmBtn.onClick.AddListener(OnStackSplitConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-45, 20);
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(stackSplitDialog.transform, Localization.Cancel, new Vector2(80, 25));
        cancelBtn.onClick.AddListener(OnStackSplitCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(45, 20);
        
        stackSplitDialog.SetActive(false);
    }
    
    private void OnStackSplitMinus()
    {
        if (stackSplitAmount > 1)
        {
            stackSplitAmount--;
            UpdateStackSplitDisplay();
        }
    }
    
    private void OnStackSplitPlus()
    {
        RPGItem item = inventoryManager.GetItem(stackSplitSlotIndex);
        if (item != null && stackSplitAmount < item.currentStack)
        {
            stackSplitAmount++;
            UpdateStackSplitDisplay();
        }
    }
    
    private void UpdateStackSplitDisplay()
    {
        RPGItem item = inventoryManager.GetItem(stackSplitSlotIndex);
        if (item != null && stackSplitAmountText != null)
        {
            stackSplitAmountText.text = string.Format("{0} / {1}", stackSplitAmount, item.currentStack);
        }
    }
    
    private void OnStackSplitConfirm()
    {
        if (stackSplitSlotIndex >= 0)
        {
            if (stackSplitIsForDrop)
            {
                DropItemToWorld(stackSplitSlotIndex, stackSplitAmount);
            }
        }
        
        if (stackSplitDialog != null)
        {
            stackSplitDialog.SetActive(false);
        }
        stackSplitSlotIndex = -1;
    }
    
    private void OnStackSplitCancel()
    {
        if (stackSplitDialog != null)
        {
            stackSplitDialog.SetActive(false);
        }
        stackSplitSlotIndex = -1;
    }
    
    // ============================================================
    // MERGE CONFIRMATION DIALOG
    // ============================================================
    
    /// <summary>
    /// Shows a confirmation dialog before merging two items with stats comparison
    /// </summary>
    private void ShowMergeConfirmDialog(int sourceSlotIndex, int targetSlotIndex)
    {
        RPGItem sourceItem = inventoryManager.GetItem(sourceSlotIndex);
        RPGItem targetItem = inventoryManager.GetItem(targetSlotIndex);
        
        if (sourceItem == null || targetItem == null) return;
        
        mergeSourceSlotIndex = sourceSlotIndex;
        mergeTargetSlotIndex = targetSlotIndex;
        
        if (mergeConfirmDialog == null)
        {
            CreateMergeConfirmDialog();
        }
        
        // Update the confirmation text with stats comparison
        if (mergeConfirmText != null)
        {
            string itemName = targetItem.GetLocalizedName();
            int currentLevel = targetItem.upgradeLevel;
            int newLevel = currentLevel + 1;
            
            // Build the stats comparison string
            string statsComparison = BuildStatsComparisonString(targetItem);
            
            // Build full message
            string message = string.Format("<b><color={0}>{1}</color></b>\n", 
                targetItem.GetRarityColorHex(), itemName);
            message += string.Format("<color=#aaaaaa>+{0} → <color=#55efc4>+{1}</color></color>\n\n", 
                currentLevel, newLevel);
            message += statsComparison;
            message += "\n<color=#ff6b6b><size=11>" + Localization.MergeWarning + "</size></color>";
            
            mergeConfirmText.text = message;
        }
        
        mergeConfirmDialog.SetActive(true);
    }
    
    /// <summary>
    /// Build a stats comparison string showing current → upgraded values
    /// </summary>
    private string BuildStatsComparisonString(RPGItem item)
    {
        if (item == null) return "";
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        int currentLevel = item.upgradeLevel;
        int nextLevel = currentLevel + 1;
        float currentMult = InventoryMerchantHelper.GetUpgradeMultiplier(currentLevel);
        float nextMult = InventoryMerchantHelper.GetUpgradeMultiplier(nextLevel);
        
        // Attack - use CeilToInt and ensure minimum increase of 1
        if (item.attackBonus > 0)
        {
            int baseAtk = Mathf.RoundToInt(item.attackBonus / currentMult);
            int nextAtk = Mathf.CeilToInt(baseAtk * nextMult);
            int increase = nextAtk - item.attackBonus;
            if (increase < 1) { increase = 1; nextAtk = item.attackBonus + 1; }
            sb.AppendFormat("<color=#ff6b6b>{0}: {1} → {2} (+{3})</color>\n", 
                Localization.ATK, item.attackBonus, nextAtk, increase);
        }
        
        // Defense - use CeilToInt and ensure minimum increase of 1
        if (item.defenseBonus > 0)
        {
            int baseDef = Mathf.RoundToInt(item.defenseBonus / currentMult);
            int nextDef = Mathf.CeilToInt(baseDef * nextMult);
            int increase = nextDef - item.defenseBonus;
            if (increase < 1) { increase = 1; nextDef = item.defenseBonus + 1; }
            sb.AppendFormat("<color=#4ecdc4>{0}: {1} → {2} (+{3})</color>\n", 
                Localization.DEF, item.defenseBonus, nextDef, increase);
        }
        
        // Health - use CeilToInt and ensure minimum increase of 5
        if (item.healthBonus > 0)
        {
            int baseHp = Mathf.RoundToInt(item.healthBonus / currentMult);
            int nextHp = Mathf.CeilToInt(baseHp * nextMult);
            int increase = nextHp - item.healthBonus;
            if (increase < 5) { increase = 5; nextHp = item.healthBonus + 5; }
            sb.AppendFormat("<color=#95e66b>{0}: {1} → {2} (+{3})</color>\n", 
                Localization.HP, item.healthBonus, nextHp, increase);
        }
        
        // Ability Power - use CeilToInt and ensure minimum increase of 1
        if (item.abilityPowerBonus > 0)
        {
            int baseAp = Mathf.RoundToInt(item.abilityPowerBonus / currentMult);
            int nextAp = Mathf.CeilToInt(baseAp * nextMult);
            int increase = nextAp - item.abilityPowerBonus;
            if (increase < 1) { increase = 1; nextAp = item.abilityPowerBonus + 1; }
            sb.AppendFormat("<color=#CC66FF>{0}: {1} → {2} (+{3})</color>\n", 
                Localization.AP, item.abilityPowerBonus, nextAp, increase);
        }
        
        // Critical Chance (flat increase)
        if (item.criticalChance > 0)
        {
            float nextCrit = item.criticalChance + 0.5f;
            sb.AppendFormat("<color=#ff7675>{0}: {1}% → {2}% (+0.5%)</color>\n", 
                Localization.Crit, item.criticalChance.ToString("F1"), nextCrit.ToString("F1"));
        }
        
        // Critical Damage (flat increase)
        if (item.criticalDamage > 0)
        {
            float nextCritDmg = item.criticalDamage + 2f;
            sb.AppendFormat("<color=#e17055>{0}: {1}% → {2}% (+2%)</color>\n", 
                Localization.CritDamage, item.criticalDamage.ToString("F0"), nextCritDmg.ToString("F0"));
        }
        
        string result = sb.ToString();
        if (string.IsNullOrEmpty(result))
        {
            // No upgradeable stats, show a generic message
            result = "<color=#888888>" + (Localization.CurrentLanguage == ModLanguage.Chinese ? 
                "无可升级属性" : "No upgradeable stats") + "</color>\n";
        }
        
        return result.TrimEnd('\n');
    }
    
    private void CreateMergeConfirmDialog()
    {
        // Create dialog panel - larger to fit stats comparison
        mergeConfirmDialog = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(300, 220), "MergeConfirmDialog");
        RectTransform dialogRect = mergeConfirmDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mergeConfirmDialog.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.MergeItems;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 16 to 18 for better readability
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.9f, 0.85f, 0.7f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleRect.anchoredPosition = new Vector2(0, -15);
        
        // Stats comparison message
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(mergeConfirmDialog.transform, false);
        mergeConfirmText = messageObj.AddComponent<Text>();
        mergeConfirmText.text = "";
        mergeConfirmText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 13 to 15 for better readability
        mergeConfirmText.fontSize = 15;
        mergeConfirmText.alignment = TextAnchor.MiddleCenter;
        mergeConfirmText.color = Color.white;
        mergeConfirmText.supportRichText = true;
        mergeConfirmText.lineSpacing = 1.1f;
        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.2f);
        messageRect.anchorMax = new Vector2(1, 0.88f);
        messageRect.sizeDelta = new Vector2(-20, 0);
        messageRect.anchoredPosition = Vector2.zero;
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(mergeConfirmDialog.transform, Localization.Confirm, new Vector2(80, 28));
        confirmBtn.onClick.AddListener(OnMergeConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-50, 22);
        
        // Change confirm button color to green
        Image confirmImage = confirmBtn.GetComponent<Image>();
        if (confirmImage != null)
        {
            confirmImage.color = new Color(0.2f, 0.6f, 0.3f);
        }
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(mergeConfirmDialog.transform, Localization.Cancel, new Vector2(80, 28));
        cancelBtn.onClick.AddListener(OnMergeCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(50, 22);
        
        mergeConfirmDialog.SetActive(false);
    }
    
    private void OnMergeConfirm()
    {
        if (mergeSourceSlotIndex >= 0 && mergeTargetSlotIndex >= 0)
        {
            CombineItems(mergeSourceSlotIndex, mergeTargetSlotIndex);
            MarkDirty();
            RefreshInventory();
        }
        
        CloseMergeDialog();
    }
    
    private void OnMergeCancel()
    {
        CloseMergeDialog();
    }
    
    private void CloseMergeDialog()
    {
        if (mergeConfirmDialog != null)
        {
            mergeConfirmDialog.SetActive(false);
        }
        mergeSourceSlotIndex = -1;
        mergeTargetSlotIndex = -1;
    }
    
    // ============================================================
    // SELL CONFIRMATION DIALOG
    // ============================================================
    
    /// <summary>
    /// Shows sell confirmation dialog
    /// </summary>
    public void ShowSellConfirmDialog(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        // Check if we're near a merchant
        PropEnt_Merchant_Base merchant = FindNearbyMerchant();
        if (merchant == null)
        {
            RPGLog.Debug(" No merchant nearby to sell to");
            return;
        }
        
        sellSlotIndex = slotIndex;
        
        if (sellConfirmDialog == null)
        {
            CreateSellConfirmDialog();
        }
        
        // Update the confirmation text
        if (sellConfirmText != null)
        {
            int sellPrice = InventoryMerchantHelper.GetSellPrice(item);
            
            string message = string.Format("<b><color={0}>{1}</color></b>\n", 
                item.GetRarityColorHex(), item.GetDisplayName());
            message += string.Format("<color=#aaaaaa>{0} {1}</color>\n\n", 
                Localization.GetRarityName(item.rarity), item.GetTypeName());
            message += string.Format("<color=#ffd700>{0}: {1} {2}</color>", 
                Localization.YouWillReceive, sellPrice, Localization.Gold);
            
            sellConfirmText.text = message;
        }
        
        sellConfirmDialog.SetActive(true);
    }
    
    private void CreateSellConfirmDialog()
    {
        sellConfirmDialog = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(260, 140), "SellConfirmDialog");
        RectTransform dialogRect = sellConfirmDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(sellConfirmDialog.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.SellItem;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 16 to 18 for better readability
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.9f, 0.85f, 0.7f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleRect.anchoredPosition = new Vector2(0, -15);
        
        // Message
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(sellConfirmDialog.transform, false);
        sellConfirmText = messageObj.AddComponent<Text>();
        sellConfirmText.text = "";
        sellConfirmText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        sellConfirmText.fontSize = 13;
        sellConfirmText.alignment = TextAnchor.MiddleCenter;
        sellConfirmText.color = Color.white;
        sellConfirmText.supportRichText = true;
        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.25f);
        messageRect.anchorMax = new Vector2(1, 0.85f);
        messageRect.sizeDelta = new Vector2(-20, 0);
        messageRect.anchoredPosition = Vector2.zero;
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(sellConfirmDialog.transform, Localization.Sell, new Vector2(80, 28));
        confirmBtn.onClick.AddListener(OnSellConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-50, 22);
        
        Image confirmImage = confirmBtn.GetComponent<Image>();
        if (confirmImage != null) confirmImage.color = new Color(0.7f, 0.6f, 0.2f); // Gold color
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(sellConfirmDialog.transform, Localization.Cancel, new Vector2(80, 28));
        cancelBtn.onClick.AddListener(OnSellCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(50, 22);
        
        sellConfirmDialog.SetActive(false);
    }
    
    private void OnSellConfirm()
    {
        if (sellSlotIndex >= 0)
        {
            ExecuteSellItem(sellSlotIndex);
        }
        CloseSellDialog();
    }
    
    private void OnSellCancel()
    {
        CloseSellDialog();
    }
    
    private void CloseSellDialog()
    {
        if (sellConfirmDialog != null)
        {
            sellConfirmDialog.SetActive(false);
        }
        sellSlotIndex = -1;
    }
    
    /// <summary>
    /// Sell an item to merchant (if near merchant) - shows confirmation dialog
    /// </summary>
    public void SellItem(int slotIndex)
    {
        ShowSellConfirmDialog(slotIndex);
    }
    
    /// <summary>
    /// Actually execute the sell (called after confirmation)
    /// </summary>
    private void ExecuteSellItem(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        PropEnt_Merchant_Base merchant = FindNearbyMerchant();
        if (merchant == null) return;
        
        int sellPrice = InventoryMerchantHelper.GetSellPrice(item);
        
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            if (NetworkServer.active)
            {
                DewPlayer.local.AddGold(sellPrice);
            }
            else
            {
                RequestSellGold(DewPlayer.local.hero.netId, sellPrice);
            }
            
            PlaySellSound(merchant);
        }
        
        // Track for scoring
        ScoringPatches.TrackItemSold();
        
        inventoryManager.RemoveItem(slotIndex);
        
        MarkDirty();
        RefreshInventory();
        UpdateStatsDisplay();
    }
    
    // ============================================================
    // DISMANTLE CONFIRMATION DIALOG
    // ============================================================
    
    /// <summary>
    /// Shows dismantle confirmation dialog
    /// </summary>
    public void ShowDismantleConfirmDialog(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        dismantleSlotIndex = slotIndex;
        
        if (dismantleConfirmDialog == null)
        {
            CreateDismantleConfirmDialog();
        }
        
        // Update the confirmation text
        if (dismantleConfirmText != null)
        {
            int dismantleValue = InventoryMerchantHelper.GetDismantleValue(item);
            
            string message = string.Format("<b><color={0}>{1}</color></b>\n", 
                item.GetRarityColorHex(), item.GetDisplayName());
            message += string.Format("<color=#aaaaaa>{0} {1}</color>\n\n", 
                Localization.GetRarityName(item.rarity), item.GetTypeName());
            message += string.Format("<color=#74b9ff>{0}: {1} {2}</color>\n", 
                Localization.YouWillReceive, dismantleValue, Localization.DreamDust);
            message += string.Format("<color=#ff6b6b><size=11>{0}</size></color>", 
                Localization.DismantleWarning);
            
            dismantleConfirmText.text = message;
        }
        
        dismantleConfirmDialog.SetActive(true);
    }
    
    private void CreateDismantleConfirmDialog()
    {
        dismantleConfirmDialog = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(260, 150), "DismantleConfirmDialog");
        RectTransform dialogRect = dismantleConfirmDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(dismantleConfirmDialog.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.DismantleItem;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 16 to 18 for better readability
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.9f, 0.85f, 0.7f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleRect.anchoredPosition = new Vector2(0, -15);
        
        // Message
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(dismantleConfirmDialog.transform, false);
        dismantleConfirmText = messageObj.AddComponent<Text>();
        dismantleConfirmText.text = "";
        dismantleConfirmText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 13 to 15 for better readability
        dismantleConfirmText.fontSize = 15;
        dismantleConfirmText.alignment = TextAnchor.MiddleCenter;
        dismantleConfirmText.color = Color.white;
        dismantleConfirmText.supportRichText = true;
        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.22f);
        messageRect.anchorMax = new Vector2(1, 0.85f);
        messageRect.sizeDelta = new Vector2(-20, 0);
        messageRect.anchoredPosition = Vector2.zero;
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(dismantleConfirmDialog.transform, Localization.Dismantle, new Vector2(90, 28));
        confirmBtn.onClick.AddListener(OnDismantleConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-55, 22);
        
        Image confirmImage = confirmBtn.GetComponent<Image>();
        if (confirmImage != null) confirmImage.color = new Color(0.6f, 0.3f, 0.3f); // Red color
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(dismantleConfirmDialog.transform, Localization.Cancel, new Vector2(80, 28));
        cancelBtn.onClick.AddListener(OnDismantleCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(50, 22);
        
        dismantleConfirmDialog.SetActive(false);
    }
    
    private void OnDismantleConfirm()
    {
        if (dismantleSlotIndex >= 0)
        {
            ExecuteDismantleItem(dismantleSlotIndex);
        }
        CloseDismantleDialog();
    }
    
    private void OnDismantleCancel()
    {
        CloseDismantleDialog();
    }
    
    private void CloseDismantleDialog()
    {
        if (dismantleConfirmDialog != null)
        {
            dismantleConfirmDialog.SetActive(false);
        }
        dismantleSlotIndex = -1;
    }
    
    /// <summary>
    /// Dismantle an item for Dream Dust
    /// </summary>
    public void DismantleItem(int slotIndex)
    {
        ShowDismantleConfirmDialog(slotIndex);
    }
    
    /// <summary>
    /// Actually execute the dismantle (called after confirmation)
    /// </summary>
    private void ExecuteDismantleItem(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        int dismantleValue = InventoryMerchantHelper.GetDismantleValue(item);
        
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            if (NetworkServer.active)
            {
                DewPlayer.local.AddDreamDust(dismantleValue);
            }
            else
            {
                // Client: Request host to GIVE us dust (not spend!)
                RequestGiveDust(DewPlayer.local.hero.netId, dismantleValue);
            }
        }
        
        // Track for scoring
        ScoringPatches.TrackItemDismantled();
        ScoringPatches.TrackDustFromMod(dismantleValue);
        
        inventoryManager.RemoveItem(slotIndex);
        
        MarkDirty();
        RefreshInventory();
        UpdateStatsDisplay();
        
        RPGLog.Debug(" Dismantled item: " + item.name + " for " + dismantleValue + " Dream Dust");
    }
    
    // ============================================================
    // CLEANSE CONFIRMATION DIALOG
    // ============================================================
    
    /// <summary>
    /// Shows cleanse confirmation dialog with stats comparison
    /// </summary>
    public void ShowCleanseConfirmDialog(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        if (!InventoryMerchantHelper.CanCleanse(item))
        {
            RPGLog.Debug(" Item cannot be cleansed (no upgrades)");
            return;
        }
        
        cleanseSlotIndex = slotIndex;
        
        if (cleanseConfirmDialog == null)
        {
            CreateCleanseConfirmDialog();
        }
        
        // Update the confirmation text with stats comparison
        if (cleanseConfirmText != null)
        {
            int cleanseCost = InventoryMerchantHelper.GetCleanseCost(item);
            int cleanseRefund = InventoryMerchantHelper.GetCleanseRefund(item);
            int currentLevel = item.upgradeLevel;
            
            string message = string.Format("<b><color={0}>{1}</color></b>\n", 
                item.GetRarityColorHex(), item.GetDisplayName());
            message += string.Format("<color=#aaaaaa>+{0} → <color=#49ffd7>+0</color></color>\n\n", currentLevel);
            
            // Add stats comparison (showing reduction)
            message += BuildCleanseStatsComparisonString(item);
            
            message += string.Format("\n\n<color=#ffd700>{0}: {1} {2}</color>", 
                Localization.Cost, cleanseCost, Localization.Gold);
            message += string.Format("\n<color=#74b9ff>{0}: {1} {2}</color>", 
                Localization.Refund, cleanseRefund, Localization.DreamDust);
            
            cleanseConfirmText.text = message;
        }
        
        cleanseConfirmDialog.SetActive(true);
    }
    
    /// <summary>
    /// Build a stats comparison string showing current → cleansed values (reduction)
    /// </summary>
    private string BuildCleanseStatsComparisonString(RPGItem item)
    {
        if (item == null) return "";
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        int currentLevel = item.upgradeLevel;
        float currentMult = InventoryMerchantHelper.GetUpgradeMultiplier(currentLevel);
        
        // Attack
        if (item.attackBonus > 0)
        {
            int baseAtk = Mathf.RoundToInt(item.attackBonus / currentMult);
            int decrease = item.attackBonus - baseAtk;
            sb.AppendFormat("<color=#ff6b6b>{0}: {1} → {2} (-{3})</color>\n", 
                Localization.ATK, item.attackBonus, baseAtk, decrease);
        }
        
        // Defense
        if (item.defenseBonus > 0)
        {
            int baseDef = Mathf.RoundToInt(item.defenseBonus / currentMult);
            int decrease = item.defenseBonus - baseDef;
            sb.AppendFormat("<color=#4ecdc4>{0}: {1} → {2} (-{3})</color>\n", 
                Localization.DEF, item.defenseBonus, baseDef, decrease);
        }
        
        // Health
        if (item.healthBonus > 0)
        {
            int baseHp = Mathf.RoundToInt(item.healthBonus / currentMult);
            int decrease = item.healthBonus - baseHp;
            sb.AppendFormat("<color=#95e66b>{0}: {1} → {2} (-{3})</color>\n", 
                Localization.HP, item.healthBonus, baseHp, decrease);
        }
        
        // Ability Power
        if (item.abilityPowerBonus > 0)
        {
            int baseAp = Mathf.RoundToInt(item.abilityPowerBonus / currentMult);
            int decrease = item.abilityPowerBonus - baseAp;
            sb.AppendFormat("<color=#CC66FF>{0}: {1} → {2} (-{3})</color>\n", 
                Localization.AP, item.abilityPowerBonus, baseAp, decrease);
        }
        
        // Critical Chance (flat decrease)
        if (item.criticalChance > 0)
        {
            float baseCrit = item.criticalChance - (0.5f * currentLevel);
            baseCrit = Mathf.Max(0f, baseCrit);
            float decrease = item.criticalChance - baseCrit;
            sb.AppendFormat("<color=#ff7675>{0}: {1}% → {2}% (-{3}%)</color>\n", 
                Localization.Crit, item.criticalChance.ToString("F1"), baseCrit.ToString("F1"), decrease.ToString("F1"));
        }
        
        // Critical Damage (flat decrease)
        if (item.criticalDamage > 0)
        {
            float baseCritDmg = item.criticalDamage - (2f * currentLevel);
            baseCritDmg = Mathf.Max(0f, baseCritDmg);
            float decrease = item.criticalDamage - baseCritDmg;
            sb.AppendFormat("<color=#e17055>{0}: {1}% → {2}% (-{3}%)</color>\n", 
                Localization.CritDamage, item.criticalDamage.ToString("F0"), baseCritDmg.ToString("F0"), decrease.ToString("F0"));
        }
        
        return sb.ToString().TrimEnd('\n');
    }
    
    private void CreateCleanseConfirmDialog()
    {
        cleanseConfirmDialog = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(300, 260), "CleanseConfirmDialog");
        RectTransform dialogRect = cleanseConfirmDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(cleanseConfirmDialog.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.CleanseItem;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 16 to 18 for better readability
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.29f, 1f, 0.84f); // Cyan/teal color
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleRect.anchoredPosition = new Vector2(0, -15);
        
        // Message with stats comparison
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(cleanseConfirmDialog.transform, false);
        cleanseConfirmText = messageObj.AddComponent<Text>();
        cleanseConfirmText.text = "";
        cleanseConfirmText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cleanseConfirmText.fontSize = 12;
        cleanseConfirmText.alignment = TextAnchor.MiddleCenter;
        cleanseConfirmText.color = Color.white;
        cleanseConfirmText.supportRichText = true;
        cleanseConfirmText.lineSpacing = 1.1f;
        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.18f);
        messageRect.anchorMax = new Vector2(1, 0.9f);
        messageRect.sizeDelta = new Vector2(-20, 0);
        messageRect.anchoredPosition = Vector2.zero;
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(cleanseConfirmDialog.transform, Localization.Cleanse, new Vector2(85, 28));
        confirmBtn.onClick.AddListener(OnCleanseConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-52, 22);
        
        Image confirmImage = confirmBtn.GetComponent<Image>();
        if (confirmImage != null) confirmImage.color = new Color(0.18f, 0.55f, 0.52f); // Teal color
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(cleanseConfirmDialog.transform, Localization.Cancel, new Vector2(80, 28));
        cancelBtn.onClick.AddListener(OnCleanseCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(50, 22);
        
        cleanseConfirmDialog.SetActive(false);
    }
    
    private void OnCleanseConfirm()
    {
        if (cleanseSlotIndex >= 0)
        {
            ExecuteCleanseItem(cleanseSlotIndex);
        }
        CloseCleanseDialog();
    }
    
    private void OnCleanseCancel()
    {
        CloseCleanseDialog();
    }
    
    private void CloseCleanseDialog()
    {
        if (cleanseConfirmDialog != null)
        {
            cleanseConfirmDialog.SetActive(false);
        }
        cleanseSlotIndex = -1;
    }
    
    /// <summary>
    /// Cleanse an item - shows confirmation dialog
    /// </summary>
    public void CleanseItem(int slotIndex)
    {
        ShowCleanseConfirmDialog(slotIndex);
    }
    
    /// <summary>
    /// Actually execute the cleanse (called after confirmation)
    /// </summary>
    private void ExecuteCleanseItem(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        if (!InventoryMerchantHelper.CanCleanse(item)) return;
        
        int cleanseCost = InventoryMerchantHelper.GetCleanseCost(item);
        
        // Ensure player exists
        if (DewPlayer.local == null)
        {
            RPGLog.Debug(" Cannot cleanse: player is null");
            return;
        }
        
        // Check if player has enough gold
        if (DewPlayer.local.gold < cleanseCost)
        {
            RPGLog.Debug(" Not enough gold to cleanse");
            return;
        }
        
        // Deduct gold cost - MUST use SpendGold, not AddGold (AddGold only works with positive values)
        if (DewPlayer.local.hero != null)
        {
            if (NetworkServer.active)
            {
                DewPlayer.local.SpendGold(cleanseCost);
                RPGLog.Debug(" Spent " + cleanseCost + " gold for cleanse (server)");
            }
            else
            {
                // Client: Request host to spend our gold
                RequestSpendGoldForCleanse(DewPlayer.local.hero.netId, cleanseCost);
                RPGLog.Debug(" Requested spend " + cleanseCost + " gold for cleanse (client)");
            }
        }
        else
        {
            RPGLog.Debug(" Cannot cleanse: hero is null");
            return;
        }
        
        // Cleanse the item and get refund
        int refund = InventoryMerchantHelper.CleanseItem(item);
        
        // Track for scoring
        ScoringPatches.TrackItemCleansed();
        ScoringPatches.TrackDustFromMod(refund);
        
        // Give Dream Dust refund
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            if (NetworkServer.active)
            {
                DewPlayer.local.AddDreamDust(refund);
            }
            else
            {
                // Client: Request host to GIVE us dust (refund)
                RequestGiveDust(DewPlayer.local.hero.netId, refund);
            }
        }
        
        MarkDirty();
        RefreshInventory();
        UpdateStatsDisplay();
        
        RPGLog.Debug(" Cleansed item: " + item.name + " - Cost: " + cleanseCost + "g, Refund: " + refund + " dust");
    }
    
    /// <summary>
    /// Send sell request to host (for clients)
    /// </summary>
    private void RequestSellGold(uint heroNetId, int goldAmount)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            string content = string.Format("[RPGSELL]{0}|{1}", heroNetId, goldAmount);
            chatManager.CmdSendChatMessage(content, null);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to send sell request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Send upgrade dust request to host (for clients) - SPENDS dust
    /// </summary>
    private void RequestSpendDust(uint heroNetId, int dustAmount)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            string content = string.Format("[RPGSPEND]{0}|{1}", heroNetId, dustAmount);
            chatManager.CmdSendChatMessage(content, null);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to send spend dust request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Send dismantle dust request to host (for clients) - GIVES dust
    /// </summary>
    private void RequestGiveDust(uint heroNetId, int dustAmount)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            string content = string.Format("[RPGDUST]{0}|{1}", heroNetId, dustAmount);
            chatManager.CmdSendChatMessage(content, null);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to send give dust request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Send cleanse gold spend request to host (for clients) - SPENDS gold
    /// </summary>
    private void RequestSpendGoldForCleanse(uint heroNetId, int goldAmount)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            // Use a new message type for spending gold (cleanse cost)
            string content = string.Format("[RPGCLEANSE]{0}|{1}", heroNetId, goldAmount);
            chatManager.CmdSendChatMessage(content, null);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to send cleanse gold request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Show upgrade confirmation dialog for an inventory item
    /// </summary>
    public void UpgradeItem(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        // Don't allow upgrading consumables, materials, or quest items
        if (item.type == ItemType.Consumable || item.type == ItemType.Material || item.type == ItemType.Quest)
        {
            return;
        }
        
        // Check if we're near an upgrade well
        Shrine_UpgradeWell upgradeWell = FindNearbyUpgradeWell();
        if (upgradeWell == null) return;
        
        // Calculate upgrade cost (dream dust)
        int upgradeCost = InventoryMerchantHelper.GetUpgradeCost(item);
        
        // Check if player can afford
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Cost cost = Cost.DreamDust(upgradeCost);
        AffordType affordType = cost.CanAfford(DewPlayer.local.hero);
        
        if (affordType != AffordType.Yes)
        {
            return;
        }
        
        // Show confirmation dialog
        ShowUpgradeConfirmDialog(item, slotIndex, null, upgradeCost);
    }
    
    /// <summary>
    /// Show upgrade confirmation dialog with stats comparison
    /// </summary>
    private void ShowUpgradeConfirmDialog(RPGItem item, int slotIndex, EquipmentSlotType? equipSlot, int upgradeCost)
    {
        upgradeSlotIndex = slotIndex;
        upgradeEquipmentSlot = equipSlot;
        
        if (upgradeConfirmDialog == null)
        {
            CreateUpgradeConfirmDialog();
        }
        
        // Build confirmation message with stats comparison
        if (upgradeConfirmText != null)
        {
            string itemName = item.GetLocalizedName();
            int currentLevel = item.upgradeLevel;
            int newLevel = currentLevel + 1;
            string statsComparison = BuildStatsComparisonString(item);
            
            string message = string.Format("<b><color={0}>{1}</color></b>\n", 
                item.GetRarityColorHex(), itemName);
            message += string.Format("<color=#aaaaaa>+{0} → <color=#55efc4>+{1}</color></color>\n\n", 
                currentLevel, newLevel);
            message += statsComparison;
            message += string.Format("\n<color=#b2bec3>{0}: <color=#74b9ff>{1}</color></color>", 
                Localization.DreamDust, upgradeCost);
            
            upgradeConfirmText.text = message;
        }
        
        upgradeConfirmDialog.SetActive(true);
    }
    
    /// <summary>
    /// Create the upgrade confirmation dialog
    /// </summary>
    private void CreateUpgradeConfirmDialog()
    {
        upgradeConfirmDialog = UIHelper.CreateWindowPanel(canvas.transform, new Vector2(300, 260), "UpgradeConfirmDialog");
        RectTransform dialogRect = upgradeConfirmDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(upgradeConfirmDialog.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = Localization.Upgrade;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 16 to 18 for better readability
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.9f, 0.85f, 0.7f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 25);
        titleRect.anchoredPosition = new Vector2(0, -15);
        
        // Message
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(upgradeConfirmDialog.transform, false);
        upgradeConfirmText = messageObj.AddComponent<Text>();
        upgradeConfirmText.text = "";
        upgradeConfirmText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Increased base size from 13 to 15 for better readability
        upgradeConfirmText.fontSize = 15;
        upgradeConfirmText.alignment = TextAnchor.MiddleCenter;
        upgradeConfirmText.color = Color.white;
        upgradeConfirmText.supportRichText = true;
        upgradeConfirmText.lineSpacing = 1.1f;
        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.15f);
        messageRect.anchorMax = new Vector2(1, 0.88f);
        messageRect.sizeDelta = new Vector2(-20, 0);
        messageRect.anchoredPosition = Vector2.zero;
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(upgradeConfirmDialog.transform, Localization.Upgrade, new Vector2(85, 28));
        confirmBtn.onClick.AddListener(OnUpgradeConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-50, 22);
        Image confirmImage = confirmBtn.GetComponent<Image>();
        if (confirmImage != null) confirmImage.color = new Color(0.2f, 0.5f, 0.7f);
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(upgradeConfirmDialog.transform, Localization.Cancel, new Vector2(80, 28));
        cancelBtn.onClick.AddListener(OnUpgradeCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(50, 22);
        
        upgradeConfirmDialog.SetActive(false);
    }
    
    /// <summary>
    /// Handle upgrade confirmation
    /// </summary>
    private void OnUpgradeConfirm()
    {
        if (upgradeSlotIndex >= 0)
        {
            PerformUpgrade(upgradeSlotIndex);
        }
        else if (upgradeEquipmentSlot.HasValue)
        {
            PerformEquippedUpgrade(upgradeEquipmentSlot.Value);
        }
        
        upgradeSlotIndex = -1;
        upgradeEquipmentSlot = null;
        
        if (upgradeConfirmDialog != null)
        {
            upgradeConfirmDialog.SetActive(false);
        }
    }
    
    /// <summary>
    /// Handle upgrade cancellation
    /// </summary>
    private void OnUpgradeCancel()
    {
        upgradeSlotIndex = -1;
        upgradeEquipmentSlot = null;
        
        if (upgradeConfirmDialog != null)
        {
            upgradeConfirmDialog.SetActive(false);
        }
    }
    
    /// <summary>
    /// Actually perform the upgrade on an inventory item (after confirmation)
    /// </summary>
    private void PerformUpgrade(int slotIndex)
    {
        RPGItem item = inventoryManager.GetItem(slotIndex);
        if (item == null) return;
        
        // Calculate upgrade cost (dream dust)
        int upgradeCost = InventoryMerchantHelper.GetUpgradeCost(item);
        
        // Spend dream dust (via network if client)
        if (NetworkServer.active)
        {
            // HOST: Directly spend dust
            DewPlayer.local.SpendDreamDust(upgradeCost);
        }
        else
        {
            // CLIENT: Send request to host to SPEND dust
            RequestSpendDust(DewPlayer.local.hero.netId, upgradeCost);
        }
        
        // Upgrade the item (pass dust cost to track for cleanse refund)
        InventoryMerchantHelper.UpgradeItem(item, upgradeCost);
        
        // Play upgrade completion sound effect (dismantle sound)
        PlayUpgradeCompleteSound();
        
        // If item is equipped, reapply stats
        EquipmentSlotType? equippedSlot = GetEquippedSlotForItem(item);
        if (equippedSlot.HasValue)
        {
            // Unequip and re-equip to refresh stats
            RPGItem unequipped = equipmentManager.Unequip(equippedSlot.Value);
            if (unequipped != null && unequipped.id == item.id)
            {
                equipmentManager.Equip(item, equippedSlot.Value);
                foreach (EquipmentSlot slot in equipmentSlots)
                {
                    if (slot.SlotType == equippedSlot.Value)
                    {
                        slot.SetItem(item);
                        break;
                    }
                }
            }
        }
        
        MarkDirty();
        RefreshInventory();
        UpdateStatsDisplay();
        
        // Refresh tooltip to show updated stats and upgrade hint
        // Always refresh if tooltip is visible for this slot
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            RPGItem freshItem = inventoryManager.GetItem(slotIndex);
            if (freshItem != null)
            {
                // Force update the tooltip slot index and refresh
                tooltipItemSlotIndex = slotIndex;
                hoveredSlotIndex = slotIndex;
                Vector3 tooltipPos = tooltipPanel.GetComponent<RectTransform>().position;
                ShowTooltip(freshItem, tooltipPos, slotIndex);
            }
        }
    }
    
    /// <summary>
    /// Show upgrade confirmation dialog for an equipped item
    /// </summary>
    public void UpgradeEquippedItem(EquipmentSlotType slotType)
    {
        RPGItem item = equipmentManager.GetEquippedItem(slotType);
        if (item == null) return;
        
        // Don't allow upgrading consumables, materials, or quest items
        if (item.type == ItemType.Consumable || item.type == ItemType.Material || item.type == ItemType.Quest)
        {
            return;
        }
        
        // Check if we're near an upgrade well
        Shrine_UpgradeWell upgradeWell = FindNearbyUpgradeWell();
        if (upgradeWell == null) return;
        
        // Calculate upgrade cost (dream dust)
        int upgradeCost = InventoryMerchantHelper.GetUpgradeCost(item);
        
        // Check if player can afford
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Cost cost = Cost.DreamDust(upgradeCost);
        AffordType affordType = cost.CanAfford(DewPlayer.local.hero);
        
        if (affordType != AffordType.Yes)
        {
            return;
        }
        
        // Show confirmation dialog
        ShowUpgradeConfirmDialog(item, -1, slotType, upgradeCost);
    }
    
    /// <summary>
    /// Actually perform the upgrade on an equipped item (after confirmation)
    /// </summary>
    private void PerformEquippedUpgrade(EquipmentSlotType slotType)
    {
        RPGItem item = equipmentManager.GetEquippedItem(slotType);
        if (item == null) return;
        
        // Calculate upgrade cost (dream dust)
        int upgradeCost = InventoryMerchantHelper.GetUpgradeCost(item);
        
        // Spend dream dust (via network if client)
        if (NetworkServer.active)
        {
            // HOST: Directly spend dust
            DewPlayer.local.SpendDreamDust(upgradeCost);
        }
        else
        {
            // CLIENT: Send request to host to SPEND dust
            RequestSpendDust(DewPlayer.local.hero.netId, upgradeCost);
        }
        
        // Upgrade the item (pass dust cost to track for cleanse refund)
        InventoryMerchantHelper.UpgradeItem(item, upgradeCost);
        
        // Play upgrade completion sound effect
        PlayUpgradeCompleteSound();
        
        // Unequip and re-equip to refresh stats
        RPGItem unequipped = equipmentManager.Unequip(slotType);
        if (unequipped != null && unequipped.id == item.id)
        {
            equipmentManager.Equip(item, slotType);
            foreach (EquipmentSlot slot in equipmentSlots)
            {
                if (slot.SlotType == slotType)
                {
                    slot.SetItem(item);
                    break;
                }
            }
        }
        
        MarkDirty();
        RefreshInventory();
        UpdateStatsDisplay();
        
        // Refresh tooltip to show updated stats
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            Vector3 tooltipPos = tooltipPanel.GetComponent<RectTransform>().position;
            ShowEquippedTooltip(item, tooltipPos, slotType);
        }
    }
    
    /// <summary>
    /// Play upgrade completion sound (uses dismantle tap sound)
    /// </summary>
    private void PlayUpgradeCompleteSound()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        // Use dismantle tap sound for completion (simple and reliable)
        if (fxDismantleTap == null)
        {
            LoadSoundEffects();
        }
        
        if (fxDismantleTap != null)
        {
            DewEffect.PlayNew(fxDismantleTap, DewPlayer.local.hero.position, null, null);
        }
    }
    
    private PropEnt_Merchant_Base FindNearbyMerchant()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return null;
        
        PropEnt_Merchant_Base[] merchants = UnityEngine.Object.FindObjectsByType<PropEnt_Merchant_Base>(FindObjectsSortMode.None);
        float maxDistance = 10f;
        
        foreach (PropEnt_Merchant_Base merchant in merchants)
        {
            if (merchant == null || !merchant.isActive) continue;
            
            float distance = Vector3.Distance(DewPlayer.local.hero.position, merchant.position);
            if (distance <= maxDistance)
            {
                return merchant;
            }
        }
        
        return null;
    }
    
    private Shrine_UpgradeWell FindNearbyUpgradeWell()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null)
        {
            return null;
        }
        
        Shrine_UpgradeWell[] wells = UnityEngine.Object.FindObjectsByType<Shrine_UpgradeWell>(FindObjectsSortMode.None);
        
        float maxDistance = 10f;
        Vector3 heroPos = DewPlayer.local.hero.position;
        
        foreach (Shrine_UpgradeWell well in wells)
        {
            if (well == null || !well.isActive) continue;
            
            float distance = Vector3.Distance(heroPos, well.position);
            if (distance <= maxDistance)
            {
                return well;
            }
        }
        
        return null;
    }
    
    private Shrine_AltarOfCleansing FindNearbyCleanseAltar()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null)
        {
            return null;
        }
        
        Shrine_AltarOfCleansing[] altars = UnityEngine.Object.FindObjectsByType<Shrine_AltarOfCleansing>(FindObjectsSortMode.None);
        
        float maxDistance = 10f;
        Vector3 heroPos = DewPlayer.local.hero.position;
        
        foreach (Shrine_AltarOfCleansing altar in altars)
        {
            if (altar == null || !altar.isActive) continue;
            
            float distance = Vector3.Distance(heroPos, altar.position);
            if (distance <= maxDistance)
            {
                return altar;
            }
        }
        
        return null;
    }
    
    private EquipmentSlotType? GetEquippedSlotForItem(RPGItem item)
    {
        if (item == null || equipmentManager == null) return null;
        
        foreach (EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentSlotType)))
        {
            RPGItem equipped = equipmentManager.GetEquipped(slotType);
            if (equipped != null && equipped.id == item.id)
            {
                return slotType;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Load sound effects from game (same approach as DroppedItem)
    /// Only loads fxDismantleTap which is used for all sounds
    /// </summary>
    private void LoadSoundEffects()
    {
        if (fxDismantleTap != null) return; // Already loaded
        
        try
        {
            // Load dismantle tap effect from Gem World Model prefab (same as DroppedItem)
            GameObject prefab = Resources.Load<GameObject>("WorldModels/Gem World Model");
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("WorldModels/Skill World Model");
            }
            
            if (prefab != null)
            {
                ItemWorldModel prefabWorldModel = prefab.GetComponent<ItemWorldModel>();
                if (prefabWorldModel != null)
                {
                    fxDismantleTap = prefabWorldModel.fxDismantleTap;
                    RPGLog.Debug(" Loaded dismantle tap sound effect from world model");
                }
                else
                {
                    RPGLog.Warning(" World model prefab has no ItemWorldModel component");
                }
            }
            else
            {
                RPGLog.Warning(" Could not load world model prefab for sound effects");
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to load sound effects: " + e.Message);
        }
    }
    
    /// <summary>
    /// Play sell tap sound (when holding sell key)
    /// </summary>
    private void PlaySellTapSound(PropEnt_Merchant_Base merchant)
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        // Use dismantle tap sound for tap feedback
        if (fxDismantleTap == null)
        {
            LoadSoundEffects();
        }
        
        if (fxDismantleTap != null)
        {
            DewEffect.PlayNew(fxDismantleTap, DewPlayer.local.hero.position, null, null);
        }
    }
    
    /// <summary>
    /// Play sell sound (when completing sell) - uses dismantle tap sound
    /// </summary>
    private void PlaySellSound(PropEnt_Merchant_Base merchant)
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        // Use dismantle tap sound for completion (simple and reliable)
        if (fxDismantleTap == null)
        {
            LoadSoundEffects();
        }
        
        if (fxDismantleTap != null)
        {
            DewEffect.PlayNew(fxDismantleTap, DewPlayer.local.hero.position, null, null);
        }
    }
    
    /// <summary>
    /// Play upgrade tap sound (when holding U key)
    /// Uses dismantle tap sound for feedback
    /// </summary>
    private void PlayUpgradeTapSound(Shrine_UpgradeWell well)
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        // Use dismantle tap sound for tap feedback (consistent with sell tap)
        if (fxDismantleTap == null)
        {
            LoadSoundEffects();
        }
        
        if (fxDismantleTap != null)
        {
            DewEffect.PlayNew(fxDismantleTap, DewPlayer.local.hero.position, null, null);
        }
    }
    
    /// <summary>
    /// Play dismantle tap sound (when holding D key)
    /// </summary>
    private void PlayDismantleTapSound()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        if (fxDismantleTap == null)
        {
            LoadSoundEffects();
        }
        
        if (fxDismantleTap != null)
        {
            DewEffect.PlayNew(fxDismantleTap, DewPlayer.local.hero.position, null, null);
        }
    }
    
    /// <summary>
    /// Play cleanse tap sound (when holding C key)
    /// </summary>
    private void PlayCleanseTapSound()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        if (fxDismantleTap == null)
        {
            LoadSoundEffects();
        }
        
        if (fxDismantleTap != null)
        {
            DewEffect.PlayNew(fxDismantleTap, DewPlayer.local.hero.position, null, null);
        }
    }
    
}
