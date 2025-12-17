using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Result of creating a progress bar
/// </summary>
public struct ProgressBarResult
{
    public GameObject container;
    public Image fill;
    
    public ProgressBarResult(GameObject container, Image fill)
    {
        this.container = container;
        this.fill = fill;
    }
}

/// <summary>
/// Helper class to create UI elements using the game's DewGUI widget system
/// Provides a consistent look and feel with the game's native UI
/// </summary>
public static class UIHelper
{
    // Cached widget prefabs
    private static GameObject _widgetWindowPrefab;
    private static GameObject _widgetButtonPrefab;
    private static GameObject _widgetTextHeaderPrefab;
    private static GameObject _widgetTextBodyPrefab;
    private static GameObject _widgetTextLabelPrefab;
    
    // UI Colors from game
    public static readonly Color WindowBackgroundColor = new Color(0.027f, 0.075f, 0.114f, 0.97f);
    public static readonly Color BorderColor = new Color(0.105f, 0.196f, 0.263f, 1f);
    public static readonly Color SharpBorderColor = new Color(0.384f, 0.424f, 0.529f, 0.31f);
    public static readonly Color AccentColor = new Color(0.482f, 0.612f, 0.843f, 0.57f);
    public static readonly Color ShadowColor = new Color(0f, 0f, 0f, 0.72f);
    public static readonly Color ButtonColor = new Color(0.286f, 0.427f, 0.565f, 1f);
    
    // Cached sprites from game
    private static Sprite _borderSprite;
    private static Sprite _shadowSprite;
    private static Sprite _buttonSprite;
    private static Sprite _tooltipBackgroundSprite;
    
    // Tooltip colors - darker background for better readability
    public static readonly Color TooltipBackgroundColor = new Color(0.08f, 0.09f, 0.11f, 0.98f);
    
    /// <summary>
    /// Load game UI assets for reuse
    /// </summary>
    public static void Initialize()
    {
        try
        {
            // Load widget prefabs from DewGUI
            _widgetWindowPrefab = Resources.Load<GameObject>("DewGUI/Widget Window");
            _widgetButtonPrefab = Resources.Load<GameObject>("DewGUI/Widget Button");
            _widgetTextHeaderPrefab = Resources.Load<GameObject>("DewGUI/Widget Text Header");
            _widgetTextBodyPrefab = Resources.Load<GameObject>("DewGUI/Widget Text Body");
            _widgetTextLabelPrefab = Resources.Load<GameObject>("DewGUI/Widget Text Label");
            
            // Load sprites from game assets
            LoadSprites();
            
            RPGLog.Debug(" UIHelper initialized with game assets");
        }
        catch (Exception e)
        {
            RPGLog.Warning(" UIHelper initialization failed: " + e.Message);
        }
    }
    
    private static void LoadSprites()
    {
        // Try to find border sprites from loaded UI elements
        if (_widgetWindowPrefab != null)
        {
            Image[] images = _widgetWindowPrefab.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img.sprite != null)
                {
                    string spriteName = img.sprite.name.ToLower();
                    if (spriteName.Contains("border") && _borderSprite == null)
                    {
                        _borderSprite = img.sprite;
                    }
                    else if (spriteName.Contains("shadow") && _shadowSprite == null)
                    {
                        _shadowSprite = img.sprite;
                    }
                }
            }
        }
        
        if (_widgetButtonPrefab != null)
        {
            Image btnImg = _widgetButtonPrefab.GetComponent<Image>();
            if (btnImg != null && btnImg.sprite != null)
            {
                _buttonSprite = btnImg.sprite;
            }
        }
        
        // Try to get tooltip background sprite from the game's tooltip manager
        TryLoadTooltipSprite();
    }
    
    private static void TryLoadTooltipSprite()
    {
        try
        {
            // Method 1: Try loading directly from Resources
            Sprite tooltipSprite = Resources.Load<Sprite>("Tooltip_Background");
            if (tooltipSprite != null)
            {
                _tooltipBackgroundSprite = tooltipSprite;
                RPGLog.Debug(" Got tooltip sprite from Resources: " + _tooltipBackgroundSprite.name);
                return;
            }
            
            // Method 2: Try to find the tooltip manager and get its background sprite
            if (SingletonBehaviour<UI_TooltipManager>.instance != null)
            {
                var tooltipManager = SingletonBehaviour<UI_TooltipManager>.instance;
                
                // Try multiple approaches to find the tooltip image
                Transform[] allChildren = tooltipManager.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    Image img = child.GetComponent<Image>();
                    if (img != null && img.sprite != null)
                    {
                        string spriteName = img.sprite.name.ToLower();
                        if (spriteName.Contains("tooltip") || spriteName.Contains("background"))
                        {
                            _tooltipBackgroundSprite = img.sprite;
                            RPGLog.Debug(" Got tooltip sprite from TooltipManager: " + _tooltipBackgroundSprite.name);
                            return;
                        }
                    }
                }
                
                // Try getting from the main tooltip GameObject itself
                if (tooltipManager.rawTextTooltip != null)
                {
                    // Go up to find the tooltip root
                    Transform root = tooltipManager.rawTextTooltip.transform;
                    while (root.parent != null && root.parent != tooltipManager.transform)
                    {
                        root = root.parent;
                    }
                    
                    Image rootImg = root.GetComponent<Image>();
                    if (rootImg != null && rootImg.sprite != null)
                    {
                        _tooltipBackgroundSprite = rootImg.sprite;
                        RPGLog.Debug(" Got tooltip sprite from tooltip root: " + _tooltipBackgroundSprite.name);
                        return;
                    }
                }
            }
            
            // Method 3: Try to find any sprite with "tooltip" in the name from loaded assets
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite sprite in allSprites)
            {
                if (sprite.name.ToLower().Contains("tooltip"))
                {
                    _tooltipBackgroundSprite = sprite;
                    RPGLog.Debug(" Found tooltip sprite in loaded assets: " + sprite.name);
                    return;
                }
            }
            
            RPGLog.Debug(" Could not find tooltip sprite, will use fallback styling");
        }
        catch (System.Exception e)
        {
            RPGLog.Warning(" Failed to load tooltip sprite: " + e.Message);
        }
    }
    
    /// <summary>
    /// Get the tooltip background sprite (tries to load if not already loaded)
    /// </summary>
    public static Sprite GetTooltipBackgroundSprite()
    {
        if (_tooltipBackgroundSprite == null)
        {
            TryLoadTooltipSprite();
        }
        return _tooltipBackgroundSprite;
    }
    
    /// <summary>
    /// Create a game-styled window panel using the game's Widget Window prefab
    /// </summary>
    public static GameObject CreateWindowPanel(Transform parent, Vector2 size, string name = "Window")
    {
        // Try to instantiate the actual game's Widget Window prefab
        if (_widgetWindowPrefab != null)
        {
            try
            {
                GameObject windowObj = UnityEngine.Object.Instantiate(_widgetWindowPrefab, parent);
                windowObj.name = name;
                
                RectTransform rect = windowObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = size;
                }
                
                // Disable drag functionality for our windows
                UI_Window windowComp = windowObj.GetComponent<UI_Window>();
                if (windowComp != null)
                {
                    windowComp.isDraggable = false;
                }
                
                // Clear any existing layout groups that might interfere
                // Use DestroyImmediate to ensure they're gone before we add new ones
                VerticalLayoutGroup vlg = windowObj.GetComponent<VerticalLayoutGroup>();
                if (vlg != null)
                {
                    UnityEngine.Object.DestroyImmediate(vlg);
                }
                
                HorizontalLayoutGroup hlg = windowObj.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    UnityEngine.Object.DestroyImmediate(hlg);
                }
                
                ContentSizeFitter csf = windowObj.GetComponent<ContentSizeFitter>();
                if (csf != null)
                {
                    UnityEngine.Object.DestroyImmediate(csf);
                }
                
                RPGLog.Debug(" Created window using game's Widget Window prefab");
                return windowObj;
            }
            catch (System.Exception e)
            {
                RPGLog.Warning(" Failed to instantiate Widget Window: " + e.Message);
            }
        }
        
        // Fallback: Create manually
        GameObject fallbackObj = new GameObject(name);
        fallbackObj.transform.SetParent(parent, false);
        
        RectTransform fallbackRect = fallbackObj.AddComponent<RectTransform>();
        fallbackRect.sizeDelta = size;
        
        // Background
        Image bg = fallbackObj.AddComponent<Image>();
        bg.color = WindowBackgroundColor;
        bg.raycastTarget = true;
        
        // If we have the border sprite, use it
        if (_borderSprite != null)
        {
            bg.sprite = _borderSprite;
            bg.type = Image.Type.Sliced;
        }
        
        // Add shadow
        CreateShadow(fallbackObj.transform, size);
        
        // Add border decoration
        CreateBorder(fallbackObj.transform, size);
        
        return fallbackObj;
    }
    
    /// <summary>
    /// Create shadow effect behind a window
    /// </summary>
    private static void CreateShadow(Transform parent, Vector2 size)
    {
        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(parent, false);
        shadowObj.transform.SetAsFirstSibling();
        
        RectTransform rect = shadowObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = new Vector2(80, 80); // Extend beyond parent
        rect.anchoredPosition = Vector2.zero;
        
        Image shadow = shadowObj.AddComponent<Image>();
        shadow.color = ShadowColor;
        shadow.raycastTarget = false;
        
        if (_shadowSprite != null)
        {
            shadow.sprite = _shadowSprite;
            shadow.type = Image.Type.Sliced;
        }
        
        LayoutElement le = shadowObj.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }
    
    /// <summary>
    /// Create border decoration
    /// </summary>
    private static void CreateBorder(Transform parent, Vector2 size)
    {
        // Big border
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(parent, false);
        
        RectTransform rect = borderObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        Image border = borderObj.AddComponent<Image>();
        border.color = BorderColor;
        border.raycastTarget = false;
        
        if (_borderSprite != null)
        {
            border.sprite = _borderSprite;
            border.type = Image.Type.Sliced;
        }
        
        LayoutElement le = borderObj.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
        
        // Sharp inner border
        GameObject sharpObj = new GameObject("SharpBorder");
        sharpObj.transform.SetParent(parent, false);
        
        RectTransform sharpRect = sharpObj.AddComponent<RectTransform>();
        sharpRect.anchorMin = Vector2.zero;
        sharpRect.anchorMax = Vector2.one;
        sharpRect.sizeDelta = new Vector2(-2, -2);
        sharpRect.anchoredPosition = Vector2.zero;
        
        Image sharpBorder = sharpObj.AddComponent<Image>();
        sharpBorder.color = SharpBorderColor;
        sharpBorder.raycastTarget = false;
        
        if (_borderSprite != null)
        {
            sharpBorder.sprite = _borderSprite;
            sharpBorder.type = Image.Type.Sliced;
        }
        
        LayoutElement sharpLe = sharpObj.AddComponent<LayoutElement>();
        sharpLe.ignoreLayout = true;
        
        // Accent glow
        GameObject accentObj = new GameObject("AccentGlow");
        accentObj.transform.SetParent(parent, false);
        
        RectTransform accentRect = accentObj.AddComponent<RectTransform>();
        accentRect.anchorMin = Vector2.zero;
        accentRect.anchorMax = Vector2.one;
        accentRect.sizeDelta = new Vector2(4, 4);
        accentRect.anchoredPosition = Vector2.zero;
        
        Image accent = accentObj.AddComponent<Image>();
        accent.color = AccentColor;
        accent.raycastTarget = false;
        
        if (_borderSprite != null)
        {
            accent.sprite = _borderSprite;
            accent.type = Image.Type.Sliced;
            accent.pixelsPerUnitMultiplier = 2f;
        }
        
        LayoutElement accentLe = accentObj.AddComponent<LayoutElement>();
        accentLe.ignoreLayout = true;
    }
    
    /// <summary>
    /// Create a styled button - always creates manually to avoid localization issues
    /// </summary>
    public static Button CreateButton(Transform parent, string text, Vector2 size)
    {
        // Always create our own button manually to avoid localization issues
        // The game's button prefab has DewLocalizedText that overrides text even after removal
        GameObject btnObj = new GameObject("Button_" + text);
        btnObj.transform.SetParent(parent, false);
        
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        
        Image img = btnObj.AddComponent<Image>();
        img.color = ButtonColor;
        if (_buttonSprite != null)
        {
            img.sprite = _buttonSprite;
            img.type = Image.Type.Sliced;
        }
        
        Button button = btnObj.AddComponent<Button>();
        button.targetGraphic = img;
        
        // Set up button colors for hover/press feedback
        ColorBlock colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = new Color(ButtonColor.r * 1.2f, ButtonColor.g * 1.2f, ButtonColor.b * 1.2f, ButtonColor.a);
        colors.pressedColor = new Color(ButtonColor.r * 0.8f, ButtonColor.g * 0.8f, ButtonColor.b * 0.8f, ButtonColor.a);
        button.colors = colors;
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        Text btnText = textObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        btnText.fontSize = 14;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return button;
    }
    
    /// <summary>
    /// Create a header text element
    /// </summary>
    public static Text CreateHeaderText(Transform parent, string text, int fontSize = 18)
    {
        GameObject textObj = new GameObject("HeaderText");
        textObj.transform.SetParent(parent, false);
        
        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComp.fontSize = fontSize;
        textComp.fontStyle = FontStyle.Bold;
        textComp.color = Color.white;
        textComp.alignment = TextAnchor.MiddleCenter;
        
        return textComp;
    }
    
    /// <summary>
    /// Create a body text element
    /// </summary>
    public static Text CreateBodyText(Transform parent, string text, int fontSize = 14)
    {
        GameObject textObj = new GameObject("BodyText");
        textObj.transform.SetParent(parent, false);
        
        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComp.fontSize = fontSize;
        textComp.color = new Color(0.8f, 0.8f, 0.8f);
        textComp.alignment = TextAnchor.MiddleLeft;
        
        return textComp;
    }
    
    /// <summary>
    /// Create a styled item slot background
    /// </summary>
    public static Image CreateSlotBackground(Transform parent, Vector2 size)
    {
        GameObject slotObj = new GameObject("SlotBackground");
        slotObj.transform.SetParent(parent, false);
        
        RectTransform rect = slotObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        
        Image img = slotObj.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.12f, 0.9f);
        img.raycastTarget = true;
        
        // Add subtle border
        GameObject borderObj = new GameObject("SlotBorder");
        borderObj.transform.SetParent(slotObj.transform, false);
        
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0.3f, 0.3f, 0.35f, 0.5f);
        borderImg.raycastTarget = false;
        
        // Make it an outline only
        Outline outline = borderObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.4f, 0.45f, 0.3f);
        outline.effectDistance = new Vector2(1, -1);
        
        return img;
    }
    
    /// <summary>
    /// Create a progress bar
    /// </summary>
    public static ProgressBarResult CreateProgressBar(Transform parent, Vector2 size, Color fillColor)
    {
        GameObject containerObj = new GameObject("ProgressBar");
        containerObj.transform.SetParent(parent, false);
        
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.sizeDelta = size;
        
        // Background
        Image bgImg = containerObj.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(containerObj.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.sizeDelta = new Vector2(0, 0);
        fillRect.anchoredPosition = Vector2.zero;
        
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = fillColor;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = 0;
        fillImg.fillAmount = 0f;
        
        return new ProgressBarResult(containerObj, fillImg);
    }
    
    /// <summary>
    /// Create a tooltip panel styled like the game's tooltips
    /// </summary>
    public static GameObject CreateTooltipPanel(Transform parent)
    {
        GameObject tooltipObj = new GameObject("Tooltip");
        tooltipObj.transform.SetParent(parent, false);
        
        RectTransform rect = tooltipObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 150);
        rect.pivot = new Vector2(0, 1);
        
        // Main background - dark color like game tooltips
        Image bg = tooltipObj.AddComponent<Image>();
        bg.raycastTarget = false;
        bg.color = TooltipBackgroundColor;
        
        // Always create visible border frame for consistent look
        CreateTooltipBorder(tooltipObj.transform);
        
        // Add content layout - game uses padding 36,36,34,30
        VerticalLayoutGroup layout = tooltipObj.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 18, 16);
        layout.spacing = 2;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        ContentSizeFitter fitter = tooltipObj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        return tooltipObj;
    }
    
    /// <summary>
    /// Create visible border for tooltip - matches game's tooltip style
    /// </summary>
    private static void CreateTooltipBorder(Transform parent)
    {
        // Outer glow/shadow - subtle
        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(parent, false);
        shadowObj.transform.SetAsFirstSibling();
        
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.sizeDelta = new Vector2(4, 4);
        shadowRect.anchoredPosition = Vector2.zero;
        
        Image shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.4f);
        shadowImg.raycastTarget = false;
        
        LayoutElement shadowLayout = shadowObj.AddComponent<LayoutElement>();
        shadowLayout.ignoreLayout = true;
        
        // Subtle border line - same color but slightly lighter for edge definition
        GameObject borderLine = new GameObject("BorderLine");
        borderLine.transform.SetParent(parent, false);
        
        RectTransform borderRect = borderLine.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        borderRect.anchoredPosition = Vector2.zero;
        
        Image borderImg = borderLine.AddComponent<Image>();
        // Slightly lighter than background for subtle edge
        borderImg.color = new Color(0.25f, 0.28f, 0.35f, 1f);
        borderImg.raycastTarget = false;
        
        LayoutElement borderLayout = borderLine.AddComponent<LayoutElement>();
        borderLayout.ignoreLayout = true;
        
        // Inner background (same as game tooltip color) - inset like game's -14,-14
        GameObject innerBg = new GameObject("InnerBg");
        innerBg.transform.SetParent(borderLine.transform, false);
        
        RectTransform innerRect = innerBg.AddComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.sizeDelta = new Vector2(-2, -2);
        innerRect.anchoredPosition = Vector2.zero;
        
        Image innerImg = innerBg.AddComponent<Image>();
        innerImg.color = TooltipBackgroundColor;
        innerImg.raycastTarget = false;
    }
    
    /// <summary>
    /// Get a color based on item rarity
    /// </summary>
    public static Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return new Color(0.7f, 0.7f, 0.7f);
            case ItemRarity.Rare:
                return new Color(0.2f, 0.8f, 0.2f);
            case ItemRarity.Epic:
                return new Color(0.7f, 0.3f, 0.9f);
            case ItemRarity.Legendary:
                return new Color(1f, 0.6f, 0.1f);
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Get a hex color string based on item rarity (for rich text)
    /// </summary>
    public static string GetRarityColorHex(ItemRarity rarity)
    {
        try
        {
            Rarity gameRarity;
            switch (rarity)
            {
                case ItemRarity.Common: gameRarity = Rarity.Common; break;
                case ItemRarity.Rare: gameRarity = Rarity.Rare; break;
                case ItemRarity.Epic: gameRarity = Rarity.Epic; break;
                case ItemRarity.Legendary: gameRarity = Rarity.Legendary; break;
                default: gameRarity = Rarity.Common; break;
            }
            return Dew.GetRarityColorHex(gameRarity);
        }
        catch
        {
            // Fallback colors if game API not available
            switch (rarity)
            {
                case ItemRarity.Common: return "#B0B0B0";
                case ItemRarity.Rare: return "#33CC33";
                case ItemRarity.Epic: return "#B34DE6";
                case ItemRarity.Legendary: return "#FF9919";
                default: return "#FFFFFF";
            }
        }
    }
}

