using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Consumable Bar - Always visible HUD element showing consumable slots
/// Stays on screen even when inventory is closed
/// </summary>
public class ConsumableBar : MonoBehaviour
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform barPanel;
    private List<ConsumableSlot> slots = new List<ConsumableSlot>();
    private EquipmentManager equipmentManager;
    private InventoryManager inventoryManager;
    
    private int numSlots = 4;
    private KeyCode[] hotkeys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    
    // Callback when a consumable is used (for UI refresh)
    public System.Action OnConsumableUsed;
    
    // Screen resolution tracking for dynamic UI scaling
    private Vector2 _lastScreenSize;
    private float _lastGameUIScale = 1f;
    private float _lastModUIScale = 1f;

    public void Initialize(EquipmentManager eqManager, InventoryManager invManager)
    {
        equipmentManager = eqManager;
        inventoryManager = invManager;
        
        // Initialize screen size tracking
        _lastScreenSize = new Vector2(Screen.width, Screen.height);
        _lastGameUIScale = GetGameUIScale();
        _lastModUIScale = RPGItemsMod.GetModUIScaleOnly();
        
        CreateCanvas();
        CreateBar();
        
        // Consumable bar initialized
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

    private void CreateCanvas()
    {
        // Separate canvas for the HUD bar - always visible
        GameObject canvasObj = new GameObject("ConsumableBar_Canvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0; // Minimum, below inventory

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void CreateBar()
    {
        // Use game-styled window panel for the consumable bar
        float slotSize = 48f;
        float spacing = 8f;
        float padding = 38f; // Even more padding from border
        float totalWidth = (slotSize * numSlots) + (spacing * (numSlots - 1)) + (padding * 2);
        float totalHeight = slotSize + (padding * 2);
        
        Vector2 windowSize = new Vector2(totalWidth, totalHeight);
        GameObject barObj = UIHelper.CreateWindowPanel(canvas.transform, windowSize, "ConsumableBarWindow");
        barPanel = barObj.GetComponent<RectTransform>();
        barPanel.anchorMin = new Vector2(0.5f, 0);
        barPanel.anchorMax = new Vector2(0.5f, 0);
        barPanel.pivot = new Vector2(0.5f, 0);
        barPanel.anchoredPosition = new Vector2(0, 15);
        
        // Add drag functionality
        DragHandler dragHandler = barObj.AddComponent<DragHandler>();
        dragHandler.Initialize(barPanel);
        
        // Create 4 horizontal slots - centered with padding from borders
        float startX = -((slotSize * numSlots) + (spacing * (numSlots - 1))) / 2f + slotSize / 2f;
        
        for (int i = 0; i < numSlots; i++)
        {
            GameObject slotObj = new GameObject("Slot_" + i);
            slotObj.transform.SetParent(barPanel, false);
            
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(slotSize, slotSize);
            slotRect.anchoredPosition = new Vector2(startX + (i * (slotSize + spacing)), 0);
            
            ConsumableSlot slot = slotObj.AddComponent<ConsumableSlot>();
            slot.Initialize(i, hotkeys[i], equipmentManager, this);
            slots.Add(slot);
        }
        
        // Apply UI scaling from config
        ApplyUIScale();
    }
    
    /// <summary>
    /// Apply UI scale from config to the consumable bar
    /// </summary>
    private void ApplyUIScale()
    {
        float scale = RPGItemsMod.GetUIScale();
        
        if (barPanel != null)
        {
            barPanel.localScale = new Vector3(scale, scale, 1f);
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

    public void SetHotkey(int slotIndex, KeyCode key)
    {
        if (slotIndex >= 0 && slotIndex < hotkeys.Length)
        {
            hotkeys[slotIndex] = key;
            if (slotIndex < slots.Count)
            {
                slots[slotIndex].SetHotkey(key);
            }
        }
    }
    
    public KeyCode GetHotkey(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < hotkeys.Length)
        {
            return hotkeys[slotIndex];
        }
        return KeyCode.None;
    }
    
    public ConsumableSlot GetSlot(int index)
    {
        if (index >= 0 && index < slots.Count)
        {
            return slots[index];
        }
        return null;
    }
    
    public int SlotCount { get { return slots.Count; } }
    
    /// <summary>
    /// Show or hide the consumable bar
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        
        if (canvas != null)
        {
            canvas.enabled = visible;
        }
        
        // Visibility changed
    }
    
    /// <summary>
    /// Clear all consumable slots (for new runs)
    /// </summary>
    public void ClearAllSlots()
    {
        // Clearing slots
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null)
            {
                slots[i].SetItem(null);
            }
        }
    }
    
    /// <summary>
    /// Sync consumable bar slots from equipment manager (call after loading saved data)
    /// </summary>
    public void SyncFromEquipment()
    {
        if (equipmentManager == null)
        {
            RPGLog.Warning(" ConsumableBar.SyncFromEquipment: No equipment manager!");
            return;
        }
        
        // Syncing from equipment
        
        for (int i = 0; i < numSlots; i++)
        {
            RPGItem savedConsumable = equipmentManager.GetConsumable(i);
            if (savedConsumable != null)
            {
                // Restored slot
                if (i < slots.Count && slots[i] != null)
                {
                    slots[i].SetItem(savedConsumable);
                }
            }
            else
            {
                // Clear slot if nothing saved
                if (i < slots.Count && slots[i] != null)
                {
                    slots[i].SetItem(null);
                }
            }
        }
        
        // Sync complete
    }
}

/// <summary>
/// Individual consumable slot in the HUD bar
/// Supports dragging to swap slots and dropping to ground
/// </summary>
public class ConsumableSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Drag state
    private static ConsumableSlot dragSourceSlot = null;
    private static GameObject dragIcon = null;
    private static Canvas dragCanvas = null;
    
    private int slotIndex;
    private KeyCode hotkey;
    private EquipmentManager equipmentManager;
    private RPGItem currentItem;
    private ConsumableBar parentBar;
    
    private Image slotBackground;
    private Image slotBorder;     // Border frame
    private Image hoverOverlay;   // Hover highlight
    private Image rarityImage;    // Animated rarity background
    private Image itemImage;
    private Text hotkeyText;
    private Text stackText;
    private RectTransform rectTransform;

    public int SlotIndex { get { return slotIndex; } }
    public RPGItem CurrentItem { get { return currentItem; } }

    public void Initialize(int index, KeyCode key, EquipmentManager eqManager, ConsumableBar bar)
    {
        slotIndex = index;
        hotkey = key;
        equipmentManager = eqManager;
        parentBar = bar;
        
        rectTransform = GetComponent<RectTransform>();
        
        // Background - dark base
        slotBackground = gameObject.AddComponent<Image>();
        slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
        slotBackground.raycastTarget = true;
        
        // Border frame - styled like game slots
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(transform, false);
        slotBorder = borderObj.AddComponent<Image>();
        slotBorder.color = new Color(0.25f, 0.30f, 0.38f, 0.8f);
        slotBorder.raycastTarget = false;
        RectTransform borderRect = slotBorder.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        borderRect.anchoredPosition = Vector2.zero;
        // Inner part for frame effect
        GameObject innerObj = new GameObject("Inner");
        innerObj.transform.SetParent(borderObj.transform, false);
        Image innerImg = innerObj.AddComponent<Image>();
        innerImg.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
        innerImg.raycastTarget = false;
        RectTransform innerRect = innerImg.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.sizeDelta = new Vector2(-3, -3);
        innerRect.anchoredPosition = Vector2.zero;
        
        // Rarity background layer (with animated material from game)
        rarityImage = RarityEffectHelper.CreateRarityImage(transform, index + 200);
        
        // Item image
        GameObject itemImgObj = new GameObject("ItemImage");
        itemImgObj.transform.SetParent(transform, false);
        itemImage = itemImgObj.AddComponent<Image>();
        itemImage.preserveAspect = true;
        itemImage.raycastTarget = false;
        
        RectTransform itemImgRect = itemImage.GetComponent<RectTransform>();
        itemImgRect.anchorMin = Vector2.zero;
        itemImgRect.anchorMax = Vector2.one;
        itemImgRect.sizeDelta = new Vector2(-10, -10);
        itemImgRect.anchoredPosition = Vector2.zero;
        itemImage.gameObject.SetActive(false);
        
        // Hover overlay - brightens slot on hover
        GameObject hoverObj = new GameObject("HoverOverlay");
        hoverObj.transform.SetParent(transform, false);
        hoverOverlay = hoverObj.AddComponent<Image>();
        hoverOverlay.color = new Color(1f, 1f, 1f, 0f);
        hoverOverlay.raycastTarget = false;
        RectTransform hoverRect = hoverOverlay.GetComponent<RectTransform>();
        hoverRect.anchorMin = Vector2.zero;
        hoverRect.anchorMax = Vector2.one;
        hoverRect.sizeDelta = Vector2.zero;
        hoverRect.anchoredPosition = Vector2.zero;
        
        // Hotkey label (top-right)
        GameObject hotkeyObj = new GameObject("Hotkey");
        hotkeyObj.transform.SetParent(transform, false);
        hotkeyText = hotkeyObj.AddComponent<Text>();
        hotkeyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        hotkeyText.fontSize = 12;
        hotkeyText.fontStyle = FontStyle.Bold;
        hotkeyText.color = new Color(1f, 0.9f, 0.3f);
        hotkeyText.alignment = TextAnchor.UpperRight;
        hotkeyText.text = GetHotkeyLabel(key);
        
        Outline hotkeyOutline = hotkeyObj.AddComponent<Outline>();
        hotkeyOutline.effectColor = Color.black;
        hotkeyOutline.effectDistance = new Vector2(1, -1);
        
        RectTransform hotkeyRect = hotkeyText.GetComponent<RectTransform>();
        hotkeyRect.anchorMin = Vector2.zero;
        hotkeyRect.anchorMax = Vector2.one;
        hotkeyRect.sizeDelta = new Vector2(-4, -4);
        hotkeyRect.anchoredPosition = Vector2.zero;
        
        // Stack count (bottom-right)
        GameObject stackObj = new GameObject("Stack");
        stackObj.transform.SetParent(transform, false);
        stackText = stackObj.AddComponent<Text>();
        stackText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        stackText.fontSize = 14;
        stackText.fontStyle = FontStyle.Bold;
        stackText.color = Color.white;
        stackText.alignment = TextAnchor.LowerRight;
        
        Outline stackOutline = stackObj.AddComponent<Outline>();
        stackOutline.effectColor = Color.black;
        stackOutline.effectDistance = new Vector2(1, -1);
        
        RectTransform stackRect = stackText.GetComponent<RectTransform>();
        stackRect.anchorMin = Vector2.zero;
        stackRect.anchorMax = Vector2.one;
        stackRect.sizeDelta = new Vector2(-4, -4);
        stackRect.anchoredPosition = Vector2.zero;
        stackText.gameObject.SetActive(false);
    }
    
    private string GetHotkeyLabel(KeyCode key)
    {
        if (key >= KeyCode.Alpha1 && key <= KeyCode.Alpha9)
        {
            return (key - KeyCode.Alpha1 + 1).ToString();
        }
        return key.ToString();
    }

    public void SetHotkey(KeyCode key)
    {
        hotkey = key;
        hotkeyText.text = GetHotkeyLabel(key);
    }

    /// <summary>
    /// Set the item displayed in this HUD slot (visual only)
    /// Call SetItemAndSync() if you also want to update EquipmentManager
    /// </summary>
    public void SetItem(RPGItem item)
    {
        currentItem = item;
        
        if (item == null)
        {
            itemImage.gameObject.SetActive(false);
            stackText.gameObject.SetActive(false);
            slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            RarityEffectHelper.UpdateRarityImage(rarityImage, null, slotIndex + 200);
            return;
        }
        
        // Update rarity background with animated effect
        RarityEffectHelper.UpdateRarityImage(rarityImage, item, slotIndex + 200);
        
        // Keep slot background dark
        slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
        
        // Lazy load sprite if needed
        ItemDatabase.EnsureSprite(item);
        
        // Show item
        if (item.sprite != null)
        {
            itemImage.sprite = item.sprite;
            itemImage.color = Color.white;
            itemImage.gameObject.SetActive(true);
        }
        else
        {
            itemImage.color = RarityEffectHelper.GetGameRarityColor(item.rarity);
            itemImage.gameObject.SetActive(true);
        }
        
        // Show stack count
        stackText.text = item.currentStack.ToString();
        stackText.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Set item AND sync to EquipmentManager (for saving)
    /// Call this from FastSlotUI when assigning consumables
    /// </summary>
    public void SetItemAndSync(RPGItem item)
    {
        SetItem(item);
        
        // Sync to EquipmentManager so it gets saved
        if (equipmentManager != null)
        {
            equipmentManager.SetConsumable(slotIndex, item);
            // Slot synced
        }
    }
    
    /// <summary>
    /// Refresh the display without changing the item (e.g., after stack count changes)
    /// </summary>
    public void RefreshDisplay()
    {
        // Re-call SetItem with the same item to refresh visuals
        SetItem(currentItem);
    }

    private void Update()
    {
        // Check hotkey press - only allow when in-game with a hero and not typing in chat
        if (Input.GetKeyDown(hotkey) && currentItem != null && CanUseConsumables() && !ControlManager.IsInputFieldFocused())
        {
            UseItem();
        }
    }
    
    private bool CanUseConsumables()
    {
        // Only allow using consumables when there's a local player with a hero
        return DewPlayer.local != null && DewPlayer.local.hero != null;
    }

    public void UseItem()
    {
        if (currentItem == null) return;
        
        // Double-check we can use consumables
        if (!CanUseConsumables())
        {
            // Not in game
            return;
        }
        
        // Using consumable
        
        // Use the potion
        if (equipmentManager != null)
        {
            equipmentManager.UsePotion(currentItem);
        }
        
        // Decrease stack
        currentItem.currentStack--;
        
        if (currentItem.currentStack <= 0)
        {
            SetItem(null);
        }
        else
        {
            stackText.text = currentItem.currentStack.ToString();
        }
        
        // Notify parent bar that a consumable was used (to refresh inventory/fast slots)
        if (parentBar != null && parentBar.OnConsumableUsed != null)
        {
            parentBar.OnConsumableUsed();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // HUD bar - right-click to use consumables
        if (!CanUseConsumables()) return;
        
        if (eventData.button == PointerEventData.InputButton.Right && currentItem != null)
        {
            UseItem();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show hover effect
        if (hoverOverlay != null)
        {
            hoverOverlay.color = new Color(1f, 1f, 1f, 0.15f);
        }
        if (slotBorder != null)
        {
            slotBorder.color = new Color(0.5f, 0.6f, 0.8f, 1f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide hover effect
        if (hoverOverlay != null)
        {
            hoverOverlay.color = new Color(1f, 1f, 1f, 0f);
        }
        if (slotBorder != null)
        {
            slotBorder.color = new Color(0.25f, 0.30f, 0.38f, 0.8f);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Accept drops from other consumable slots
        if (dragSourceSlot != null && dragSourceSlot != this)
        {
            RPGItem sourceItem = dragSourceSlot.currentItem;
            RPGItem targetItem = this.currentItem;
            
            // Check if we should STACK instead of swap (same item type)
            if (sourceItem != null && targetItem != null && sourceItem.id == targetItem.id)
            {
                // Same consumable type - STACK them!
                targetItem.currentStack += sourceItem.currentStack;
                
                // Clear the source slot (item has been merged)
                dragSourceSlot.SetItemAndSync(null);
                
                // Sync this slot to show new stack count (also updates EquipmentManager)
                this.SetItemAndSync(targetItem);
                
                RPGLog.Debug(" Stacked consumable in slot " + slotIndex + ": " + targetItem.name + " x" + targetItem.currentStack);
            }
            else
            {
                // Different items or one is empty - swap them
            dragSourceSlot.SetItemAndSync(targetItem);
            this.SetItemAndSync(sourceItem);
            
            RPGLog.Debug(" Swapped consumable slots " + dragSourceSlot.slotIndex + " <-> " + slotIndex);
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        
        dragSourceSlot = this;
        
        // Create drag icon
        if (dragCanvas == null)
        {
            GameObject canvasObj = new GameObject("DragCanvas");
            dragCanvas = canvasObj.AddComponent<Canvas>();
            dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dragCanvas.sortingOrder = 9999;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(dragCanvas.transform, false);
        
        // Lazy load sprite if needed
        ItemDatabase.EnsureSprite(currentItem);
        
        Image iconImg = dragIcon.AddComponent<Image>();
        if (currentItem.sprite != null)
        {
            iconImg.sprite = currentItem.sprite;
        }
        iconImg.color = new Color(1f, 1f, 1f, 0.8f);
        iconImg.raycastTarget = false;
        
        RectTransform iconRect = dragIcon.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(48, 48);
        
        // Add stack text
        if (currentItem.currentStack > 1)
        {
            GameObject stackObj = new GameObject("Stack");
            stackObj.transform.SetParent(dragIcon.transform, false);
            Text stackTxt = stackObj.AddComponent<Text>();
            stackTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            stackTxt.fontSize = 14;
            stackTxt.fontStyle = FontStyle.Bold;
            stackTxt.color = Color.white;
            stackTxt.alignment = TextAnchor.LowerRight;
            stackTxt.text = currentItem.currentStack.ToString();
            stackTxt.raycastTarget = false;
            
            Outline outline = stackObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);
            
            RectTransform stackRect = stackTxt.GetComponent<RectTransform>();
            stackRect.anchorMin = Vector2.zero;
            stackRect.anchorMax = Vector2.one;
            stackRect.sizeDelta = new Vector2(-4, -4);
            stackRect.anchoredPosition = Vector2.zero;
        }
        
        // Dim the source slot
        if (itemImage != null)
        {
            itemImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore source slot appearance
        if (itemImage != null && currentItem != null)
        {
            itemImage.color = Color.white;
        }
        
        // Destroy drag icon
        if (dragIcon != null)
        {
            Object.Destroy(dragIcon);
            dragIcon = null;
        }
        
        // Check if dropped on another ConsumableSlot (handled by OnDrop)
        // or dropped outside UI (drop to ground)
        bool droppedOnConsumableSlot = false;
        if (eventData.pointerCurrentRaycast.isValid && eventData.pointerCurrentRaycast.gameObject != null)
        {
            // Check if we dropped on a ConsumableSlot
            ConsumableSlot targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<ConsumableSlot>();
            if (targetSlot == null)
            {
                targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<ConsumableSlot>();
            }
            droppedOnConsumableSlot = (targetSlot != null);
        }
        
        // If not dropped on another slot, treat as drop to ground
        if (!droppedOnConsumableSlot)
        {
            // Dropped outside consumable slots - drop item to ground
            if (currentItem != null && DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                RPGLog.Debug(" Dropping consumable to ground: " + currentItem.name);
                
                // Show stack split dialog if stack > 1
                if (currentItem.currentStack > 1)
                {
                    ShowDropStackDialog();
                }
                else
                {
                    DropItemToGround(currentItem.currentStack);
                }
            }
        }
        
        dragSourceSlot = null;
    }
    
    // Stack drop dialog (standalone, doesn't need inventory open)
    private static GameObject dropDialog = null;
    private static ConsumableSlot dropDialogSlot = null;
    private static RPGItem dropDialogItem = null;
    private static int dropDialogAmount = 1;
    private static Text dropDialogAmountText = null;
    
    private void ShowDropStackDialog()
    {
        // Create standalone drop dialog on the drag canvas (always visible)
        dropDialogSlot = this;
        dropDialogItem = currentItem;
        dropDialogAmount = 1;
        
        if (dropDialog == null)
        {
            CreateDropDialog();
        }
        
        // Update display
        if (dropDialogAmountText != null && dropDialogItem != null)
        {
            dropDialogAmountText.text = string.Format("{0} / {1}", dropDialogAmount, dropDialogItem.currentStack);
        }
        
        dropDialog.SetActive(true);
    }
    
    private void CreateDropDialog()
    {
        // Ensure we have a canvas
        if (dragCanvas == null)
        {
            GameObject canvasObj = new GameObject("DragCanvas");
            dragCanvas = canvasObj.AddComponent<Canvas>();
            dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dragCanvas.sortingOrder = 9999;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create dialog panel
        dropDialog = UIHelper.CreateWindowPanel(dragCanvas.transform, new Vector2(200, 120), "ConsumableDropDialog");
        RectTransform dialogRect = dropDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(dropDialog.transform, false);
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
        amountObj.transform.SetParent(dropDialog.transform, false);
        dropDialogAmountText = amountObj.AddComponent<Text>();
        dropDialogAmountText.text = "1 / 1";
        dropDialogAmountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        dropDialogAmountText.fontSize = 18;
        dropDialogAmountText.alignment = TextAnchor.MiddleCenter;
        dropDialogAmountText.color = Color.white;
        RectTransform amountRect = amountObj.GetComponent<RectTransform>();
        amountRect.anchorMin = new Vector2(0, 0.5f);
        amountRect.anchorMax = new Vector2(1, 0.5f);
        amountRect.sizeDelta = new Vector2(0, 30);
        amountRect.anchoredPosition = new Vector2(0, 5);
        
        // Minus button
        Button minusBtn = UIHelper.CreateButton(dropDialog.transform, "-", new Vector2(30, 30));
        minusBtn.onClick.AddListener(OnDropDialogMinus);
        RectTransform minusRect = minusBtn.GetComponent<RectTransform>();
        minusRect.anchorMin = new Vector2(0, 0.5f);
        minusRect.anchorMax = new Vector2(0, 0.5f);
        minusRect.anchoredPosition = new Vector2(25, 5);
        
        // Plus button
        Button plusBtn = UIHelper.CreateButton(dropDialog.transform, "+", new Vector2(30, 30));
        plusBtn.onClick.AddListener(OnDropDialogPlus);
        RectTransform plusRect = plusBtn.GetComponent<RectTransform>();
        plusRect.anchorMin = new Vector2(1, 0.5f);
        plusRect.anchorMax = new Vector2(1, 0.5f);
        plusRect.anchoredPosition = new Vector2(-25, 5);
        
        // Confirm button
        Button confirmBtn = UIHelper.CreateButton(dropDialog.transform, Localization.Drop, new Vector2(80, 25));
        confirmBtn.onClick.AddListener(OnDropDialogConfirm);
        RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(-45, 20);
        
        // Cancel button
        Button cancelBtn = UIHelper.CreateButton(dropDialog.transform, Localization.Cancel, new Vector2(80, 25));
        cancelBtn.onClick.AddListener(OnDropDialogCancel);
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.anchoredPosition = new Vector2(45, 20);
        
        dropDialog.SetActive(false);
    }
    
    private static void OnDropDialogMinus()
    {
        if (dropDialogAmount > 1)
        {
            dropDialogAmount--;
            UpdateDropDialogDisplay();
        }
    }
    
    private static void OnDropDialogPlus()
    {
        if (dropDialogItem != null && dropDialogAmount < dropDialogItem.currentStack)
        {
            dropDialogAmount++;
            UpdateDropDialogDisplay();
        }
    }
    
    private static void UpdateDropDialogDisplay()
    {
        if (dropDialogItem != null && dropDialogAmountText != null)
        {
            dropDialogAmountText.text = string.Format("{0} / {1}", dropDialogAmount, dropDialogItem.currentStack);
        }
    }
    
    private static void OnDropDialogConfirm()
    {
        if (dropDialogSlot != null && dropDialogAmount > 0)
        {
            dropDialogSlot.DropItemToGround(dropDialogAmount);
        }
        CloseDropDialog();
    }
    
    private static void OnDropDialogCancel()
    {
        CloseDropDialog();
    }
    
    private static void CloseDropDialog()
    {
        if (dropDialog != null)
        {
            dropDialog.SetActive(false);
        }
        dropDialogSlot = null;
        dropDialogItem = null;
    }
    
    public void DropItemToGround(int amount)
    {
        if (currentItem == null || amount <= 0) return;
        
        // Create a clone with the specified amount
        RPGItem dropItem = currentItem.Clone();
        dropItem.currentStack = amount;
        
        // Drop at player position
        Vector3 dropPos = DewPlayer.local.hero.position;
        dropPos += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        
        DroppedItem.CreateNetworked(dropItem, dropPos);
        
        // Update current item
        currentItem.currentStack -= amount;
        if (currentItem.currentStack <= 0)
        {
            SetItemAndSync(null);
        }
        else
        {
            stackText.text = currentItem.currentStack.ToString();
            // Also sync to equipment manager
            if (equipmentManager != null)
            {
                equipmentManager.SetConsumable(slotIndex, currentItem);
            }
        }
        
        RPGLog.Debug(" Dropped " + amount + "x " + dropItem.name + " from consumable slot " + slotIndex);
    }
}

/// <summary>
/// Makes a panel draggable
/// </summary>
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 dragOffset;

    public void Initialize(RectTransform rect)
    {
        rectTransform = rect;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );
        dragOffset = rectTransform.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );
        rectTransform.anchoredPosition = localPoint + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Position saved automatically
    }
}

