using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Fast Slot UI - Inventory slot for assigning consumables to the HUD bar
/// Drag consumables here to assign them to the always-visible hotbar
/// Now supports dragging items OUT of fast slots back to inventory
/// </summary>
public class FastSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private int slotIndex;
    private InventoryUI inventoryUI;
    private ConsumableBar consumableBar;
    private RPGItem currentItem;

    private Image slotBackground;
    private Image slotBorder;     // Border frame
    private Image hoverOverlay;   // Hover highlight
    private Image rarityImage;    // Animated rarity background
    private Image itemImage;
    private Text hotkeyText;
    private Text stackText;
    private RectTransform rectTransform;

    public int SlotIndex { get { return slotIndex; } }

    public void Initialize(int index, InventoryUI ui, ConsumableBar bar)
    {
        slotIndex = index;
        inventoryUI = ui;
        consumableBar = bar;
        
        rectTransform = GetComponent<RectTransform>();

        // Background - dark base (must be on root GameObject to receive drops)
        slotBackground = gameObject.AddComponent<Image>();
        slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
        slotBackground.raycastTarget = true; // CRITICAL: Must be true for IDropHandler to work
        slotBackground.type = Image.Type.Simple; // Ensure it fills the entire RectTransform

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
        rarityImage = RarityEffectHelper.CreateRarityImage(transform, index + 100);

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

        // Hotkey display (top-right corner)
        GameObject hotkeyObj = new GameObject("Hotkey");
        hotkeyObj.transform.SetParent(transform, false);
        hotkeyText = hotkeyObj.AddComponent<Text>();
        hotkeyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        hotkeyText.fontSize = 12;
        hotkeyText.fontStyle = FontStyle.Bold;
        hotkeyText.color = new Color(1f, 0.9f, 0.4f);
        hotkeyText.alignment = TextAnchor.UpperRight;
        hotkeyText.text = (index + 1).ToString();
        
        Outline hotkeyOutline = hotkeyObj.AddComponent<Outline>();
        hotkeyOutline.effectColor = Color.black;
        hotkeyOutline.effectDistance = new Vector2(1, -1);
        
        RectTransform hotkeyRect = hotkeyText.GetComponent<RectTransform>();
        hotkeyRect.anchorMin = Vector2.zero;
        hotkeyRect.anchorMax = Vector2.one;
        hotkeyRect.sizeDelta = new Vector2(-4, -4);
        hotkeyRect.anchoredPosition = Vector2.zero;

        // Stack text (bottom-right corner)
        GameObject stackTextObj = new GameObject("StackText");
        stackTextObj.transform.SetParent(transform, false);
        stackText = stackTextObj.AddComponent<Text>();
        stackText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        stackText.fontSize = 12;
        stackText.fontStyle = FontStyle.Bold;
        stackText.color = Color.white;
        stackText.alignment = TextAnchor.LowerRight;
        
        Outline stackOutline = stackTextObj.AddComponent<Outline>();
        stackOutline.effectColor = Color.black;
        stackOutline.effectDistance = new Vector2(1, -1);
        
        RectTransform stackTextRect = stackText.GetComponent<RectTransform>();
        stackTextRect.anchorMin = Vector2.zero;
        stackTextRect.anchorMax = Vector2.one;
        stackTextRect.sizeDelta = new Vector2(-4, -4);
        stackTextRect.anchoredPosition = Vector2.zero;
        stackText.gameObject.SetActive(false);
        
    }
    
    private void Start()
    {
        // Sync with consumable bar after everything is initialized
        SyncFromBar();
    }
    
    public void SyncFromBar()
    {
        if (consumableBar == null)
        {
            // Try to find consumable bar if not set
            consumableBar = UnityEngine.Object.FindFirstObjectByType<ConsumableBar>();
        }
        
        if (consumableBar != null)
        {
            ConsumableSlot barSlot = consumableBar.GetSlot(slotIndex);
            if (barSlot != null && barSlot.CurrentItem != null)
            {
                // Update visuals without triggering SetItem (to avoid loop)
                currentItem = barSlot.CurrentItem;
                UpdateVisuals();
            }
        }
    }
    
    public void UpdateVisuals()
    {
        // First, sync with the consumable bar to check if item was cleared there
        if (consumableBar == null)
        {
            consumableBar = UnityEngine.Object.FindFirstObjectByType<ConsumableBar>();
        }
        
        if (consumableBar != null)
        {
            ConsumableSlot barSlot = consumableBar.GetSlot(slotIndex);
            if (barSlot != null)
            {
                // If bar slot is null/empty but we have an item, clear it
                if (barSlot.CurrentItem == null && currentItem != null)
                {
                    currentItem = null;
                }
                // If bar slot has a different item, sync to it
                else if (barSlot.CurrentItem != currentItem)
                {
                    currentItem = barSlot.CurrentItem;
                }
            }
        }
        
        if (currentItem == null)
        {
            itemImage.gameObject.SetActive(false);
            stackText.gameObject.SetActive(false);
            slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            RarityEffectHelper.UpdateRarityImage(rarityImage, null, slotIndex + 100);
            return;
        }
        
        // Check if item was depleted (stack <= 0)
        if (currentItem.currentStack <= 0)
        {
            currentItem = null;
            itemImage.gameObject.SetActive(false);
            stackText.gameObject.SetActive(false);
            slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            RarityEffectHelper.UpdateRarityImage(rarityImage, null, slotIndex + 100);
            return;
        }

        // Update rarity background with animated effect
        RarityEffectHelper.UpdateRarityImage(rarityImage, currentItem, slotIndex + 100);
        
        // Keep slot background dark
        slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

        // Lazy load sprite if needed
        ItemDatabase.EnsureSprite(currentItem);

        if (currentItem.sprite != null)
        {
            itemImage.sprite = currentItem.sprite;
            itemImage.color = Color.white;
            itemImage.gameObject.SetActive(true);
        }
        else
        {
            itemImage.color = RarityEffectHelper.GetGameRarityColor(currentItem.rarity);
            itemImage.gameObject.SetActive(true);
        }

        stackText.text = currentItem.currentStack.ToString();
        stackText.gameObject.SetActive(true);
    }

    public void SetItem(RPGItem item)
    {
        currentItem = item;
        
        // Sync to HUD bar FIRST (before updating visuals)
        if (consumableBar == null)
        {
            consumableBar = UnityEngine.Object.FindFirstObjectByType<ConsumableBar>();
        }
        
        if (consumableBar != null)
        {
            ConsumableSlot barSlot = consumableBar.GetSlot(slotIndex);
            if (barSlot != null)
            {
                // Use SetItemAndSync to also update EquipmentManager for saving
                barSlot.SetItemAndSync(item);
            }
        }
        
        // Now update visuals (after bar is updated)
        UpdateVisualsDirectly();
    }
    
    /// <summary>
    /// Get the current item in this slot
    /// </summary>
    public RPGItem GetCurrentItem()
    {
        return currentItem;
    }
    
    /// <summary>
    /// Property to get current item (for consistency with other slot types)
    /// </summary>
    public RPGItem CurrentItem { get { return currentItem; } }
    
    /// <summary>
    /// Refresh the display without changing the item (e.g., after stack count changes)
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateVisualsDirectly();
    }
    
    /// <summary>
    /// Clear this slot (set item to null and update visuals)
    /// </summary>
    public void ClearSlot()
    {
        currentItem = null;
        UpdateVisualsDirectly();
    }
    
    // Update visuals directly without syncing from bar (to avoid overwriting)
    private void UpdateVisualsDirectly()
    {
        if (currentItem == null || currentItem.currentStack <= 0)
        {
            itemImage.gameObject.SetActive(false);
            stackText.gameObject.SetActive(false);
            slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            RarityEffectHelper.UpdateRarityImage(rarityImage, null, slotIndex + 100);
            return;
        }

        // Update rarity background with animated effect
        RarityEffectHelper.UpdateRarityImage(rarityImage, currentItem, slotIndex + 100);
        
        // Keep slot background dark
        slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

        // Lazy load sprite if needed
        ItemDatabase.EnsureSprite(currentItem);

        if (currentItem.sprite != null)
        {
            itemImage.sprite = currentItem.sprite;
            itemImage.color = Color.white;
            itemImage.gameObject.SetActive(true);
        }
        else
        {
            itemImage.color = RarityEffectHelper.GetGameRarityColor(currentItem.rarity);
            itemImage.gameObject.SetActive(true);
        }

        stackText.text = currentItem.currentStack.ToString();
        stackText.gameObject.SetActive(true);
        
        RPGLog.Debug(" FastSlotUI " + slotIndex + " updated visuals for: " + currentItem.name);
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
        
        if (currentItem != null && inventoryUI != null && rectTransform != null)
        {
            // Position tooltip to the LEFT of the slot
            // Tooltip is ~250px wide, slot is ~50px, so offset by -(tooltip width + gap)
            // Negative X = left of slot, positive Y = above slot
            Vector3 tooltipPosition = rectTransform.position + new Vector3(-280f, 0f, 0);
            inventoryUI.ShowTooltip(currentItem, tooltipPosition);
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
        
        if (inventoryUI != null)
        {
            inventoryUI.HideTooltip();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        RPGLog.Debug(" FastSlotUI OnDrop called for slot " + slotIndex + ", IsDragging=" + (inventoryUI != null ? inventoryUI.IsDragging.ToString() : "null") + ", DraggedSlot=" + (inventoryUI != null && inventoryUI.DraggedSlot != null ? inventoryUI.DraggedSlot.SlotIndex.ToString() : "null"));
        
        if (inventoryUI != null && inventoryUI.IsDragging && inventoryUI.DraggedSlot != null)
        {
            inventoryUI.OnFastSlotUIDrop(this);
        }
        else
        {
            RPGLog.Debug(" FastSlotUI OnDrop: Drag not properly initialized - IsDragging=" + (inventoryUI != null ? inventoryUI.IsDragging.ToString() : "null"));
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right-click to remove item and return to inventory
        if (eventData.button == PointerEventData.InputButton.Right && currentItem != null)
        {
            ReturnItemToInventory();
        }
    }
    
    /// <summary>
    /// Return the current item to inventory and clear this slot
    /// </summary>
    public void ReturnItemToInventory()
    {
        if (currentItem == null) return;
        
        RPGItem itemToReturn = currentItem;
        
        // Clear this slot first
        SetItem(null);
        
        // Return to inventory
        if (inventoryUI != null)
        {
            inventoryUI.ReturnConsumableToInventory(itemToReturn);
        }
    }
    
    // ==================== DRAG HANDLERS ====================
    // Allow dragging items OUT of fast slots back to inventory
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem != null && inventoryUI != null)
        {
            RPGLog.Debug(" FastSlotUI: Starting drag from slot " + slotIndex + " with item " + currentItem.name);
            inventoryUI.StartDragFromFastSlot(this);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (inventoryUI != null && inventoryUI.IsDraggingFromFastSlot)
        {
            inventoryUI.UpdateDragPosition(eventData.position);
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventoryUI != null)
        {
            // Find what we're dropping on
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            ItemSlot targetInventorySlot = null;
            FastSlotUI targetFastSlot = null;
            bool isOverInventoryUI = false;
            
            foreach (var result in results)
            {
                // Check for inventory slot
                ItemSlot itemSlot = result.gameObject.GetComponent<ItemSlot>();
                if (itemSlot != null)
                {
                    targetInventorySlot = itemSlot;
                    break;
                }
                
                // Check for another fast slot
                FastSlotUI fSlot = result.gameObject.GetComponent<FastSlotUI>();
                if (fSlot != null && fSlot != this)
                {
                    targetFastSlot = fSlot;
                    break;
                }
                
                // Check if over inventory UI
                Image uiImage = result.gameObject.GetComponent<Image>();
                if (uiImage != null && result.gameObject.name != "Background")
                {
                    Transform parent = result.gameObject.transform;
                    while (parent != null)
                    {
                        if (parent.GetComponent<InventoryUI>() != null)
                        {
                            isOverInventoryUI = true;
                            break;
                        }
                        parent = parent.parent;
                    }
                }
            }
            
            if (targetInventorySlot != null)
            {
                // Dropped on inventory slot - move item there
                inventoryUI.OnFastSlotDropToInventory(this, targetInventorySlot);
            }
            else if (targetFastSlot != null)
            {
                // Dropped on another fast slot - swap
                inventoryUI.OnFastSlotSwap(this, targetFastSlot);
            }
            else if (isOverInventoryUI)
            {
                // Dropped inside inventory UI but not on valid slot - return to first empty inventory slot
                ReturnItemToInventory();
                inventoryUI.CancelDragFromFastSlot();
            }
            else
            {
                // Dropped outside - cancel (don't drop consumables to world)
                inventoryUI.CancelDragFromFastSlot();
            }
        }
    }
}

