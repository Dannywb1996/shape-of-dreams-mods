using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Settings Panel - Customize hotkeys for consumable slots (UGUI)
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform settingsPanel;
    private ConsumableBar consumableBar;
    
    private List<HotkeyButton> hotkeyButtons = new List<HotkeyButton>();
    private int waitingForKeySlot = -1;
    
    // Screen resolution tracking for dynamic UI scaling
    private Vector2 _lastScreenSize;
    private float _lastGameUIScale = 1f;
    private float _lastModUIScale = 1f;

    public void Initialize(Canvas parentCanvas, ConsumableBar bar)
    {
        canvas = parentCanvas;
        consumableBar = bar;
        
        // Initialize screen size tracking
        _lastScreenSize = new Vector2(Screen.width, Screen.height);
        _lastGameUIScale = GetGameUIScale();
        _lastModUIScale = RPGItemsMod.GetModUIScaleOnly();
        
        CreateSettingsWindow();
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

    private void CreateSettingsWindow()
    {
        // Use game-styled window panel
        Vector2 windowSize = new Vector2(320, 260);
        GameObject panelObj = UIHelper.CreateWindowPanel(canvas.transform, windowSize, "SettingsWindow");
        settingsPanel = panelObj.GetComponent<RectTransform>();
        settingsPanel.anchorMin = new Vector2(0.5f, 0.5f);
        settingsPanel.anchorMax = new Vector2(0.5f, 0.5f);
        settingsPanel.pivot = new Vector2(0.5f, 0.5f);
        settingsPanel.anchoredPosition = Vector2.zero;

        canvasGroup = panelObj.AddComponent<CanvasGroup>();

        // Title with accent color
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(settingsPanel, false);
        Text title = titleObj.AddComponent<Text>();
        title.text = Localization.ConsumableHotkeys;
        title.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        title.fontSize = 14;
        title.fontStyle = FontStyle.Bold;
        title.color = UIHelper.AccentColor;
        title.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 30);
        titleRect.anchoredPosition = new Vector2(0, -15);

        // Hotkey settings
        CreateHotkeySettings();
        
        // Close button with game styling
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(settingsPanel, false);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        Image closeBtnImg = closeBtnObj.AddComponent<Image>();
        closeBtnImg.color = UIHelper.ButtonColor;
        
        // Add border to button
        GameObject btnBorderObj = new GameObject("Border");
        btnBorderObj.transform.SetParent(closeBtnObj.transform, false);
        Image btnBorder = btnBorderObj.AddComponent<Image>();
        btnBorder.color = new Color(0.3f, 0.35f, 0.45f, 0.8f);
        btnBorder.raycastTarget = false;
        RectTransform btnBorderRect = btnBorder.GetComponent<RectTransform>();
        btnBorderRect.anchorMin = Vector2.zero;
        btnBorderRect.anchorMax = Vector2.one;
        btnBorderRect.sizeDelta = Vector2.zero;
        btnBorderRect.anchoredPosition = Vector2.zero;
        GameObject btnInnerObj = new GameObject("Inner");
        btnInnerObj.transform.SetParent(btnBorderObj.transform, false);
        Image btnInner = btnInnerObj.AddComponent<Image>();
        btnInner.color = UIHelper.ButtonColor;
        btnInner.raycastTarget = false;
        RectTransform btnInnerRect = btnInner.GetComponent<RectTransform>();
        btnInnerRect.anchorMin = Vector2.zero;
        btnInnerRect.anchorMax = Vector2.one;
        btnInnerRect.sizeDelta = new Vector2(-2, -2);
        btnInnerRect.anchoredPosition = Vector2.zero;
        
        RectTransform closeBtnRect = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.5f, 0);
        closeBtnRect.anchorMax = new Vector2(0.5f, 0);
        closeBtnRect.pivot = new Vector2(0.5f, 0);
        closeBtnRect.sizeDelta = new Vector2(90, 28);
        closeBtnRect.anchoredPosition = new Vector2(0, 25);
        
        GameObject closeBtnTextObj = new GameObject("Text");
        closeBtnTextObj.transform.SetParent(closeBtnObj.transform, false);
        Text closeBtnText = closeBtnTextObj.AddComponent<Text>();
        closeBtnText.text = Localization.Close;
        closeBtnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        closeBtnText.fontSize = 12;
        closeBtnText.fontStyle = FontStyle.Bold;
        closeBtnText.color = Color.white;
        closeBtnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform closeBtnTextRect = closeBtnText.GetComponent<RectTransform>();
        closeBtnTextRect.anchorMin = Vector2.zero;
        closeBtnTextRect.anchorMax = Vector2.one;
        closeBtnTextRect.sizeDelta = Vector2.zero;
        closeBtnTextRect.anchoredPosition = Vector2.zero;
        
        closeBtn.onClick.AddListener(() => SetVisible(false));
        
        // Apply UI scaling from config
        ApplyUIScale();
    }
    
    /// <summary>
    /// Apply UI scale from config
    /// </summary>
    private void ApplyUIScale()
    {
        float scale = RPGItemsMod.GetUIScale();
        
        if (settingsPanel != null)
        {
            settingsPanel.localScale = new Vector3(scale, scale, 1f);
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

    private void CreateHotkeySettings()
    {
        string[] labels = new string[] { 
            Localization.Slot + " 1:", 
            Localization.Slot + " 2:", 
            Localization.Slot + " 3:", 
            Localization.Slot + " 4:" 
        };
        KeyCode[] defaultKeys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
        
        for (int i = 0; i < 4; i++)
        {
            float yPos = -55 - (i * 35);
            
            // Label
            GameObject labelObj = new GameObject("Label_" + i);
            labelObj.transform.SetParent(settingsPanel, false);
            Text label = labelObj.AddComponent<Text>();
            label.text = labels[i];
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 12;
            label.color = new Color(0.7f, 0.7f, 0.75f, 1f);
            label.alignment = TextAnchor.MiddleLeft;
            
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(0, 1);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.sizeDelta = new Vector2(80, 26);
            labelRect.anchoredPosition = new Vector2(50, yPos);

            // Button with border styling
            GameObject btnObj = new GameObject("HotkeyButton_" + i);
            btnObj.transform.SetParent(settingsPanel, false);
            Button btn = btnObj.AddComponent<Button>();
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.18f, 0.20f, 0.25f, 1f);
            
            // Add border
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(btnObj.transform, false);
            borderObj.transform.SetAsFirstSibling();
            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = new Color(0.3f, 0.35f, 0.45f, 0.8f);
            borderImg.raycastTarget = false;
            RectTransform borderRect = borderImg.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = new Vector2(2, 2);
            borderRect.anchoredPosition = Vector2.zero;
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0, 1);
            btnRect.anchorMax = new Vector2(0, 1);
            btnRect.pivot = new Vector2(0, 0.5f);
            btnRect.sizeDelta = new Vector2(100, 26);
            btnRect.anchoredPosition = new Vector2(140, yPos);

            GameObject btnTextObj = new GameObject("ButtonText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            Text btnText = btnTextObj.AddComponent<Text>();
            
            // Get current hotkey from bar if available
            KeyCode currentKey = defaultKeys[i];
            if (consumableBar != null)
            {
                currentKey = consumableBar.GetHotkey(i);
            }
            btnText.text = GetKeyLabel(currentKey);
            
            btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            btnText.fontSize = 12;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform btnTextRect = btnText.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;
            btnTextRect.anchoredPosition = Vector2.zero;

            HotkeyButton hotkeyBtn = new HotkeyButton();
            hotkeyBtn.slotIndex = i;
            hotkeyBtn.button = btn;
            hotkeyBtn.text = btnText;
            hotkeyButtons.Add(hotkeyBtn);
            
            int index = i;
            btn.onClick.AddListener(() => StartWaitingForKey(index));
        }
        
        // Instructions
        GameObject instrObj = new GameObject("Instructions");
        instrObj.transform.SetParent(settingsPanel, false);
        Text instr = instrObj.AddComponent<Text>();
        instr.text = Localization.ClickToRebind;
        instr.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        instr.fontSize = 10;
        instr.color = new Color(0.5f, 0.5f, 0.55f, 1f);
        instr.alignment = TextAnchor.MiddleCenter;
        
        RectTransform instrRect = instr.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0, 0);
        instrRect.anchorMax = new Vector2(1, 0);
        instrRect.pivot = new Vector2(0.5f, 0);
        instrRect.sizeDelta = new Vector2(-40, 20);
        instrRect.anchoredPosition = new Vector2(0, 58);
    }
    
    private string GetKeyLabel(KeyCode key)
    {
        if (key >= KeyCode.Alpha1 && key <= KeyCode.Alpha9)
        {
            return (key - KeyCode.Alpha1 + 1).ToString();
        }
        return key.ToString();
    }

    private void StartWaitingForKey(int slotIndex)
    {
        waitingForKeySlot = slotIndex;
        hotkeyButtons[slotIndex].text.text = Localization.PressKey;
        hotkeyButtons[slotIndex].text.color = Color.yellow;
    }

    private void Update()
    {
        // Check for screen resolution or UI scale changes
        CheckForScaleChanges();
        
        if (waitingForKeySlot >= 0)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key) && key != KeyCode.None && key != KeyCode.Mouse0 && key != KeyCode.Escape)
                {
                    // Update button text
                    hotkeyButtons[waitingForKeySlot].text.text = GetKeyLabel(key);
                    hotkeyButtons[waitingForKeySlot].text.color = Color.white;
                    
                    // Update consumable bar
                    if (consumableBar != null)
                    {
                        consumableBar.SetHotkey(waitingForKeySlot, key);
                    }
                    
                    waitingForKeySlot = -1;
                    break;
                }
            }
        }
    }

    public void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }

    public bool IsVisible()
    {
        return canvasGroup != null && canvasGroup.alpha > 0;
    }
}

public class HotkeyButton
{
    public int slotIndex;
    public Button button;
    public Text text;
}
