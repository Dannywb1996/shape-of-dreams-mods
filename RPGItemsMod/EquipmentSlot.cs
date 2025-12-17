using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Equipment Slot UI Component - For weapons, armor, accessories (UGUI)
/// Right-click to unequip, drag to move items, middle-click to show off
/// Implements IPingableCustom to prevent ground pings when showing off
/// </summary>
public class EquipmentSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPingableCustom
{
    private EquipmentSlotType slotType;
    private InventoryUI inventoryUI;
    private EquipmentManager equipmentManager;
    private RPGItem equippedItem;

    // UI Components
    private Image slotBackground;
    private Image slotBorder;     // Border frame
    private Image hoverOverlay;   // Hover highlight
    private Image rarityImage;    // Animated rarity background
    private Image itemImage;
    private Text slotLabel;
    private Text upgradeText;     // Shows +1, +2, etc. for upgraded items
    private RectTransform rectTransform;

    public EquipmentSlotType SlotType { get { return slotType; } }
    public RPGItem EquippedItem { get { return equippedItem; } }

    public void Initialize(EquipmentSlotType type, InventoryUI ui, EquipmentManager manager)
    {
        slotType = type;
        inventoryUI = ui;
        equipmentManager = manager;

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        // Slot background - dark base
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
        rarityImage = RarityEffectHelper.CreateRarityImage(transform, (int)type);

        // Item image
        GameObject itemImgObj = new GameObject("ItemImage");
        itemImgObj.transform.SetParent(transform, false);
        itemImage = itemImgObj.AddComponent<Image>();
        itemImage.preserveAspect = true;
        itemImage.raycastTarget = false;
        itemImage.type = Image.Type.Simple;
        
        // Default white sprite
        Texture2D whiteTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        whiteTex.SetPixels(pixels);
        whiteTex.Apply();
        itemImage.sprite = Sprite.Create(whiteTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100f);
        
        RectTransform itemImgRect = itemImage.GetComponent<RectTransform>();
        itemImgRect.anchorMin = new Vector2(0, 0);
        itemImgRect.anchorMax = new Vector2(1, 1);
        itemImgRect.sizeDelta = new Vector2(-6, -6);
        itemImgRect.anchoredPosition = Vector2.zero;
        itemImage.gameObject.SetActive(false);

        // Hover overlay - brightens slot on hover
        GameObject hoverObj = new GameObject("HoverOverlay");
        hoverObj.transform.SetParent(transform, false);
        hoverOverlay = hoverObj.AddComponent<Image>();
        hoverOverlay.color = new Color(1f, 1f, 1f, 0f); // Transparent by default
        hoverOverlay.raycastTarget = false;
        RectTransform hoverRect = hoverOverlay.GetComponent<RectTransform>();
        hoverRect.anchorMin = Vector2.zero;
        hoverRect.anchorMax = Vector2.one;
        hoverRect.sizeDelta = Vector2.zero;
        hoverRect.anchoredPosition = Vector2.zero;
        
        // Upgrade level text - shows +1, +2, etc. in top-left corner
        GameObject upgradeTextObj = new GameObject("UpgradeText");
        upgradeTextObj.transform.SetParent(transform, false);
        upgradeText = upgradeTextObj.AddComponent<Text>();
        upgradeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        upgradeText.fontSize = 12;
        upgradeText.fontStyle = FontStyle.Bold;
        upgradeText.color = new Color(0.4f, 1f, 0.4f, 1f); // Green color for upgrades
        upgradeText.alignment = TextAnchor.UpperLeft;
        upgradeText.supportRichText = true;
        
        // Add outline for better visibility
        UnityEngine.UI.Outline upgradeOutline = upgradeTextObj.AddComponent<UnityEngine.UI.Outline>();
        upgradeOutline.effectColor = Color.black;
        upgradeOutline.effectDistance = new Vector2(1, -1);
        
        RectTransform upgradeTextRect = upgradeText.GetComponent<RectTransform>();
        upgradeTextRect.anchorMin = new Vector2(0, 0);
        upgradeTextRect.anchorMax = new Vector2(1, 1);
        upgradeTextRect.sizeDelta = new Vector2(-4, -4);
        upgradeTextRect.anchoredPosition = Vector2.zero;
        upgradeText.gameObject.SetActive(false);
    }
    
    public void SetLabel(string label)
    {
        // Slot label ABOVE the slot with proper padding
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(transform, false);
        slotLabel = labelObj.AddComponent<Text>();
        slotLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        slotLabel.fontSize = 8;
        slotLabel.color = new Color(0.5f, 0.5f, 0.55f, 1f);
        slotLabel.alignment = TextAnchor.LowerCenter;
        slotLabel.text = label;
        
        RectTransform labelRect = slotLabel.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0.5f, 0);
        labelRect.sizeDelta = new Vector2(50, 12);
        labelRect.anchoredPosition = new Vector2(0, 4); // Above the slot with padding
    }

    public void SetItem(RPGItem item)
    {
        equippedItem = item;

        if (item == null)
        {
            itemImage.gameObject.SetActive(false);
            upgradeText.gameObject.SetActive(false);
            slotBackground.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            RarityEffectHelper.UpdateRarityImage(rarityImage, null, (int)slotType);
            return;
        }

        // Update rarity background with animated effect
        RarityEffectHelper.UpdateRarityImage(rarityImage, item, (int)slotType);
        
        // Keep slot background dark
        slotBackground.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Lazy load sprite if needed
        ItemDatabase.EnsureSprite(item);

        // Set item image - keep original colors (no tint)
        if (item.sprite != null)
        {
            itemImage.sprite = item.sprite;
            itemImage.color = Color.white; // No tint - show original image
            itemImage.gameObject.SetActive(true);
        }
        else
        {
            // Only tint if no image (placeholder)
            itemImage.color = RarityEffectHelper.GetGameRarityColor(item.rarity);
            itemImage.gameObject.SetActive(true);
        }
        
        // Set upgrade level text - show +1, +2, etc. for upgraded items
        if (item.upgradeLevel > 0)
        {
            upgradeText.text = "+" + item.upgradeLevel;
            upgradeText.gameObject.SetActive(true);
        }
        else
        {
            upgradeText.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show hover effect
        if (hoverOverlay != null)
        {
            hoverOverlay.color = new Color(1f, 1f, 1f, 0.15f); // Light white overlay
        }
        if (slotBorder != null)
        {
            slotBorder.color = new Color(0.5f, 0.6f, 0.8f, 1f); // Brighter border on hover
        }
        
        if (equippedItem != null && inventoryUI != null && rectTransform != null)
        {
            Vector3 tooltipPosition = rectTransform.position + new Vector3(rectTransform.rect.width + 10f, 0, 0);
            // Use equipped tooltip (no comparison stats - item is already equipped)
            // Pass slot type so upgrade works for equipped items
            inventoryUI.ShowEquippedTooltip(equippedItem, tooltipPosition, slotType);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide hover effect
        if (hoverOverlay != null)
        {
            hoverOverlay.color = new Color(1f, 1f, 1f, 0f); // Transparent
        }
        if (slotBorder != null)
        {
            slotBorder.color = new Color(0.25f, 0.30f, 0.38f, 0.8f); // Normal border
        }
        
        if (inventoryUI != null)
        {
            inventoryUI.HideTooltip();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        RPGLog.Debug(" EquipmentSlot OnDrop called for " + slotType);
        if (inventoryUI != null && inventoryUI.IsDragging && inventoryUI.DraggedSlot != null)
        {
            inventoryUI.OnEquipmentSlotDrop(this);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Right-click to unequip (only if not dragging and has item)
        if (eventData.button == PointerEventData.InputButton.Right && equippedItem != null && !inventoryUI.IsDragging)
        {
            inventoryUI.UnequipItem(this);
        }
        // Middle-click is handled by OnPing (IPingableCustom)
    }
    
    // Drag handlers for dragging equipped items
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (equippedItem != null && eventData.button == PointerEventData.InputButton.Left)
        {
            inventoryUI.StartDragFromEquipment(this);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (inventoryUI.IsDragging)
        {
            inventoryUI.UpdateDragPosition(eventData.position);
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventoryUI.IsDragging)
        {
            inventoryUI.EndDrag(eventData);
        }
    }
    
    /// <summary>
    /// IPingableCustom implementation - handles middle-click ping to show off items
    /// </summary>
    public bool OnPing()
    {
        if (equippedItem != null)
        {
            NetworkedItemSystem.ShareItemInChat(equippedItem);
            return true;
        }
        return false;
    }
}

// Note: EquipmentSlotType enum is now defined in ItemTypes.cs
