using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

/// <summary>
/// Custom UI panel that shows RPG mod statistics on the game result screen.
/// Uses DewGUI widgets to match the game's native style (like InfiniteDungeon mod).
/// Displays items collected, upgrades, set bonuses, mastery levels, and bonus mastery points earned.
/// Each player sees their own individual stats (no sync needed - tracking is local).
/// Supports both English and Chinese languages.
/// Positions itself next to InfiniteDungeon window if both mods are active, or centers if alone.
/// </summary>
public class RPGResultsUI : MonoBehaviour
{
    private static UI_Window _resultWindow;
    private bool _wasResultScreenActive = false;
    
    // ============================================================
    // LOCALIZATION - Uses Localization.Get() for all languages
    // ============================================================
    
    // Title
    private static string TitleText { get { return Localization.Get("ResultsTitle"); } }
    
    // Section headers
    private static string HeaderProgression { get { return Localization.Get("ResultsProgression"); } }
    
    // Labels
    private static string LabelConsumables { get { return Localization.Get("ResultsConsumables"); } }
    private static string LabelItemsCollected { get { return Localization.Get("ResultsItemsCollected"); } }
    private static string LabelItemsUpgraded { get { return Localization.Get("ResultsItemsUpgraded"); } }
    private static string LabelSetBonuses { get { return Localization.Get("ResultsSetBonuses"); } }
    private static string LabelStatPoints { get { return Localization.Get("ResultsStatPoints"); } }
    private static string LabelLegendaryItems { get { return Localization.Get("ResultsLegendaryItems"); } }
    private static string LabelEpicItems { get { return Localization.Get("ResultsEpicItems"); } }
    private static string LabelRareItems { get { return Localization.Get("ResultsRareItems"); } }
    
    // Bonus text
    private static string LabelScoreBonus { get { return Localization.Get("ResultsScoreBonus"); } }
    private static string LabelMasteryBonus { get { return Localization.Get("ResultsMasteryBonus"); } }
    
    // Button
    private static string ButtonContinue { get { return Localization.Get("ResultsContinue"); } }
    
    public void Initialize()
    {
        RPGLog.Debug(" RPG Results UI initialized");
    }
    
    private void Update()
    {
        // Check if we're on the result screen
        bool isResultScreen = IsResultScreenActive();
        
        if (isResultScreen && !_wasResultScreenActive)
        {
            // Just entered result screen
            ShowResults();
        }
        else if (!isResultScreen && _wasResultScreenActive)
        {
            // Just left result screen
            HideResults();
        }
        
        _wasResultScreenActive = isResultScreen;
    }
    
    private bool IsResultScreenActive()
    {
        try
        {
            // Check if InGameUIManager exists and is in "Result" state
            if (InGameUIManager.instance != null)
            {
                return InGameUIManager.instance.state == "Result";
            }
        }
        catch { }
        
        return false;
    }
    
    private void ShowResults()
    {
        // Only show if there were any RPG activities
        if (ScoringPatches.GetItemsCollected() == 0 && 
            ScoringPatches.GetItemsUpgraded() == 0 &&
            ScoringPatches.GetSetBonusesActivated() == 0 &&
            ScoringPatches.GetStatPointsSpent() == 0 &&
            ScoringPatches.GetConsumablesUsed() == 0)
        {
            return;
        }
        
        try
        {
            CreateResultWindow();
            RPGLog.Debug(" Showing RPG results panel (individual stats for this player)");
        }
        catch (Exception ex)
        {
            RPGLog.Error(" Error showing result window: " + ex.Message + "\n" + ex.StackTrace);
        }
    }
    
    private void HideResults()
    {
        if (_resultWindow != null)
        {
            UnityEngine.Object.Destroy(_resultWindow.gameObject);
            _resultWindow = null;
        }
    }
    
    private void CreateResultWindow()
    {
        if (_resultWindow != null) return;
        
        // ========== AUTO-ADAPT WINDOW SIZE TO SCREEN ==========
        // Use RPGItemsMod's GetUIScaleWithScreenFactor() for consistent scaling with screen resolution
        float scaleFactor = RPGItemsMod.GetUIScaleWithScreenFactor();
        float screenWidth = Screen.width;
        
        float maxWindowWidth = 550f * scaleFactor;
        maxWindowWidth = Mathf.Min(maxWindowWidth, screenWidth * 0.45f);
        maxWindowWidth = Mathf.Max(maxWindowWidth, 400f);
        
        // Create window using DewGUI
        _resultWindow = UnityEngine.Object.Instantiate(DewGUI.widgetWindow, DewGUI.canvasTransform);
        _resultWindow.isDraggable = false;
        _resultWindow.enableBackdrop = false; // No backdrop - let InfiniteDungeon handle that
        _resultWindow.SetWidth(maxWindowWidth);
        
        // Position - check if InfiniteDungeon window exists
        RectTransform windowRect = _resultWindow.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Check if InfiniteDungeon result window is present
        bool infiniteDungeonActive = IsInfiniteDungeonResultActive();
        
        if (infiniteDungeonActive)
        {
            // Position to the left of center (InfiniteDungeon is in center)
            float offsetX = -350f * scaleFactor;
            windowRect.anchoredPosition = new Vector2(offsetX, 0);
        }
        else
        {
            // Center if alone
            windowRect.anchoredPosition = Vector2.zero;
        }
        
        // Scale padding and spacing
        int basePadding = (int)(40 * scaleFactor);
        int topBottomPadding = (int)(25 * scaleFactor);
        float baseSpacing = 10f * scaleFactor;
        
        basePadding = Mathf.Max(basePadding, 15);
        topBottomPadding = Mathf.Max(topBottomPadding, 12);
        baseSpacing = Mathf.Max(baseSpacing, 5f);
        
        // Create vertical layout for content
        VerticalLayoutGroup mainLayout = DewGUI.CreateVerticalLayoutGroup(_resultWindow.transform);
        mainLayout.padding = new RectOffset(basePadding, basePadding, topBottomPadding, topBottomPadding);
        mainLayout.spacing = baseSpacing;
        mainLayout.childAlignment = TextAnchor.UpperCenter;
        
        float contentWidth = maxWindowWidth - (basePadding * 2);
        LayoutElement mainLayoutElement = mainLayout.gameObject.AddComponent<LayoutElement>();
        mainLayoutElement.preferredWidth = contentWidth;
        
        // ========== SCALE FONT SIZES (bigger for readability) ==========
        float titleSize = 48f * scaleFactor;
        float sectionSize = 26f * scaleFactor;
        float statLabelSize = 22f * scaleFactor;
        float statValueSize = 30f * scaleFactor;
        
        titleSize = Mathf.Max(titleSize, 36f);
        sectionSize = Mathf.Max(sectionSize, 20f);
        statLabelSize = Mathf.Max(statLabelSize, 18f);
        statValueSize = Mathf.Max(statValueSize, 24f);
        
        // ========== TITLE ==========
        TextMeshProUGUI title = UnityEngine.Object.Instantiate(DewGUI.widgetTextHeader, mainLayout.transform);
        title.text = TitleText;
        title.fontSize = titleSize;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(1f, 0.75f, 0.3f); // Orange-gold like InfiniteDungeon
        title.enableWordWrapping = false; // Keep title on one line
        title.overflowMode = TextOverflowModes.Overflow;
        
        // Spacer
        CreateSpacer(mainLayout.transform, Mathf.Max(12f * scaleFactor, 6f));
        
        // ========== SECTION HEADER ==========
        TextMeshProUGUI sectionHeader = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
        sectionHeader.text = "◆ " + HeaderProgression;
        sectionHeader.fontSize = sectionSize;
        sectionHeader.alignment = TextAlignmentOptions.Center;
        sectionHeader.color = new Color(1f, 0.84f, 0f); // Gold
        sectionHeader.fontStyle = FontStyles.Bold;
        
        // Spacer
        CreateSpacer(mainLayout.transform, Mathf.Max(8f * scaleFactor, 4f));
        
        // ========== STATS GRID ==========
        // Create a horizontal layout for stats (like InfiniteDungeon)
        GameObject statsContainer = new GameObject("StatsContainer");
        statsContainer.transform.SetParent(mainLayout.transform, false);
        HorizontalLayoutGroup statsLayout = statsContainer.AddComponent<HorizontalLayoutGroup>();
        statsLayout.spacing = 30f * scaleFactor;
        statsLayout.childAlignment = TextAnchor.MiddleCenter;
        statsLayout.childForceExpandWidth = false;
        statsLayout.childForceExpandHeight = false;
        
        // Left column (labels)
        GameObject leftColumn = new GameObject("LeftColumn");
        leftColumn.transform.SetParent(statsContainer.transform, false);
        VerticalLayoutGroup leftLayout = leftColumn.AddComponent<VerticalLayoutGroup>();
        leftLayout.spacing = 8f * scaleFactor;
        leftLayout.childAlignment = TextAnchor.MiddleRight;
        leftLayout.childControlWidth = true;
        leftLayout.childControlHeight = true;
        leftLayout.childForceExpandWidth = false;
        leftLayout.childForceExpandHeight = false;
        
        // Right column (values)
        GameObject rightColumn = new GameObject("RightColumn");
        rightColumn.transform.SetParent(statsContainer.transform, false);
        VerticalLayoutGroup rightLayout = rightColumn.AddComponent<VerticalLayoutGroup>();
        rightLayout.spacing = 8f * scaleFactor;
        rightLayout.childAlignment = TextAnchor.MiddleLeft;
        rightLayout.childControlWidth = true;
        rightLayout.childControlHeight = true;
        rightLayout.childForceExpandWidth = false;
        rightLayout.childForceExpandHeight = false;
        
        // Get stats
        int consumables = ScoringPatches.GetConsumablesUsed();
        int itemsCollected = ScoringPatches.GetItemsCollected();
        int itemsUpgraded = ScoringPatches.GetItemsUpgraded();
        int setBonuses = ScoringPatches.GetSetBonusesActivated();
        int statPoints = ScoringPatches.GetStatPointsSpent();
        int legendaryItems = ScoringPatches.GetLegendaryItemsFound();
        int epicItems = ScoringPatches.GetEpicItemsFound();
        int rareItems = ScoringPatches.GetRareItemsFound();
        long scoreBonus = ScoringPatches.CalculateBonusScore();
        long masteryBonus = ScoringPatches.CalculateBonusMasteryPoints();
        
        // Add stats - Labels on left, values on right
        if (itemsCollected > 0)
        {
            CreateStatLabel(leftLayout.transform, LabelItemsCollected + ":", statLabelSize, new Color(0.7f, 0.7f, 0.8f));
            CreateStatValue(rightLayout.transform, itemsCollected.ToString(), statValueSize, Color.white);
        }
        
        if (legendaryItems > 0 || epicItems > 0 || rareItems > 0)
        {
            string rarityBreakdown = "";
            if (legendaryItems > 0) rarityBreakdown += "L:" + legendaryItems;
            if (epicItems > 0) rarityBreakdown += (rarityBreakdown.Length > 0 ? " " : "") + "E:" + epicItems;
            if (rareItems > 0) rarityBreakdown += (rarityBreakdown.Length > 0 ? " " : "") + "R:" + rareItems;
            
            if (!string.IsNullOrEmpty(rarityBreakdown))
            {
                CreateStatLabel(leftLayout.transform, "", statLabelSize, new Color(0.7f, 0.7f, 0.8f));
                CreateStatValue(rightLayout.transform, "(" + rarityBreakdown + ")", statLabelSize * 0.85f, new Color(0.9f, 0.7f, 0.3f));
            }
        }
        
        if (itemsUpgraded > 0)
        {
            CreateStatLabel(leftLayout.transform, LabelItemsUpgraded + ":", statLabelSize, new Color(0.7f, 0.7f, 0.8f));
            CreateStatValue(rightLayout.transform, itemsUpgraded.ToString(), statValueSize, Color.white);
        }
        
        if (setBonuses > 0)
        {
            CreateStatLabel(leftLayout.transform, LabelSetBonuses + ":", statLabelSize, new Color(0.7f, 0.7f, 0.8f));
            CreateStatValue(rightLayout.transform, setBonuses.ToString(), statValueSize, new Color(0.8f, 0.5f, 0.9f));
        }
        
        if (statPoints > 0)
        {
            CreateStatLabel(leftLayout.transform, LabelStatPoints + ":", statLabelSize, new Color(0.7f, 0.7f, 0.8f));
            CreateStatValue(rightLayout.transform, statPoints.ToString(), statValueSize, Color.white);
        }
        
        if (consumables > 0)
        {
            CreateStatLabel(leftLayout.transform, LabelConsumables + ":", statLabelSize, new Color(0.7f, 0.7f, 0.8f));
            CreateStatValue(rightLayout.transform, consumables.ToString(), statValueSize, new Color(0.8f, 0.5f, 0.9f));
        }
        
        // Spacer before bonuses
        CreateSpacer(mainLayout.transform, Mathf.Max(15f * scaleFactor, 8f));
        
        // ========== SCORE BONUS ROW ==========
        CreateBonusRow(mainLayout.transform, LabelScoreBonus, "+" + scoreBonus.ToString("N0"), 
                       statLabelSize, statValueSize, new Color(1f, 0.85f, 0.4f)); // Gold
        
        // ========== MASTERY BONUS ROW ==========
        CreateBonusRow(mainLayout.transform, LabelMasteryBonus, "+" + masteryBonus.ToString("N0"), 
                       statLabelSize, statValueSize, new Color(0.33f, 0.94f, 0.77f)); // Green
        
        // Spacer
        CreateSpacer(mainLayout.transform, Mathf.Max(20f * scaleFactor, 10f));
        
        // ========== CLOSE BUTTON ==========
        float buttonWidth = Mathf.Max(180f * scaleFactor, 120f);
        float buttonHeight = Mathf.Max(40f * scaleFactor, 32f);
        
        Button closeButton = UnityEngine.Object.Instantiate(DewGUI.widgetButton, mainLayout.transform);
        closeButton.SetText(ButtonContinue);
        closeButton.SetWidth(buttonWidth);
        closeButton.SetHeight(buttonHeight);
        closeButton.interactable = true;
        
        var buttonImage = closeButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.raycastTarget = true;
        }
        
        var windowToClose = _resultWindow;
        closeButton.onClick.AddListener(delegate {
            if (windowToClose != null)
            {
                UnityEngine.Object.Destroy(windowToClose.gameObject);
            }
            _resultWindow = null;
        });
        
        // Ensure window is on top
        _resultWindow.transform.SetAsLastSibling();
        
        RPGLog.Debug(" Result window created - Consumables: " + consumables + 
                  ", Score: +" + scoreBonus + ", Mastery: +" + masteryBonus);
    }
    
    private bool IsInfiniteDungeonResultActive()
    {
        try
        {
            // Look for InfiniteDungeon result window by searching for its specific window
            var allWindows = FindObjectsOfType<UI_Window>();
            foreach (var window in allWindows)
            {
                if (window == _resultWindow) continue;
                if (window.gameObject.activeInHierarchy)
                {
                    // Check if this is likely the InfiniteDungeon window
                    var textComponents = window.GetComponentsInChildren<TextMeshProUGUI>();
                    foreach (var text in textComponents)
                    {
                        if (text.text.Contains("Endless Dream") || text.text.Contains("无尽梦境") ||
                            text.text.Contains("Journey's End") || text.text.Contains("旅途终结"))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch { }
        
        return false;
    }
    
    // ============================================================
    // HELPER METHODS (matching InfiniteDungeon style)
    // ============================================================
    
    private static void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent, false);
        LayoutElement le = spacer.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
    }
    
    private static void CreateStatLabel(Transform parent, string text, float fontSize, Color color)
    {
        TextMeshProUGUI label = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, parent);
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.MidlineRight;
        label.color = color;
        label.enableAutoSizing = false;
        label.overflowMode = TextOverflowModes.Overflow;
        
        // Add layout element for consistent sizing
        LayoutElement le = label.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize * 1.5f;
    }
    
    private static void CreateStatValue(Transform parent, string text, float fontSize, Color color)
    {
        TextMeshProUGUI value = UnityEngine.Object.Instantiate(DewGUI.widgetTextHeader, parent);
        value.text = text;
        value.fontSize = fontSize;
        value.alignment = TextAlignmentOptions.MidlineLeft;
        value.color = color;
        value.enableAutoSizing = false;
        value.overflowMode = TextOverflowModes.Overflow;
        
        // Add layout element for consistent sizing
        LayoutElement le = value.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize * 1.5f;
    }
    
    private static void CreateBonusRow(Transform parent, string labelText, string valueText, 
                                       float labelSize, float valueSize, Color valueColor)
    {
        // Create horizontal container for the bonus row
        GameObject rowContainer = new GameObject("BonusRow");
        rowContainer.transform.SetParent(parent, false);
        
        HorizontalLayoutGroup rowLayout = rowContainer.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 15f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        
        // Label
        TextMeshProUGUI label = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, rowLayout.transform);
        label.text = labelText;
        label.fontSize = labelSize;
        label.alignment = TextAlignmentOptions.MidlineRight;
        label.color = new Color(0.7f, 0.7f, 0.8f);
        
        // Value
        TextMeshProUGUI value = UnityEngine.Object.Instantiate(DewGUI.widgetTextHeader, rowLayout.transform);
        value.text = valueText;
        value.fontSize = valueSize;
        value.alignment = TextAlignmentOptions.MidlineLeft;
        value.color = valueColor;
    }
    
    public void Cleanup()
    {
        HideResults();
    }
    
    private void OnDestroy()
    {
        Cleanup();
    }
}
