using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Shared helper for rarity background effects across all slot types
/// </summary>
public static class RarityEffectHelper
{
    private static Material sharedRarityMaterial = null;
    private static Sprite sharedRaritySprite = null;
    private static bool materialSearched = false;
    
    public static Material SharedMaterial { get { return sharedRarityMaterial; } }
    public static Sprite SharedSprite { get { return sharedRaritySprite; } }
    
    /// <summary>
    /// Try to find and cache the rarity material AND sprite from game's UI
    /// </summary>
    public static void TryGetRarityMaterial()
    {
        if (sharedRarityMaterial != null && sharedRaritySprite != null) return;
        if (materialSearched) return;
        
        try
        {
            // BEST METHOD: Get directly from shop item's rarityImage
            UI_InGame_FloatingWindow_Shop_Item[] shopItems = UnityEngine.Object.FindObjectsByType<UI_InGame_FloatingWindow_Shop_Item>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (UI_InGame_FloatingWindow_Shop_Item item in shopItems)
            {
                if (item != null && item.rarityImage != null)
                {
                    if (item.rarityImage.material != null && sharedRarityMaterial == null)
                    {
                        sharedRarityMaterial = item.rarityImage.material;
                        RPGLog.Debug(" Got rarity material from shop item: " + sharedRarityMaterial.name);
                    }
                    if (item.rarityImage.sprite != null && sharedRaritySprite == null)
                    {
                        sharedRaritySprite = item.rarityImage.sprite;
                        RPGLog.Debug(" Got rarity sprite from shop item: " + sharedRaritySprite.name);
                    }
                    if (sharedRarityMaterial != null && sharedRaritySprite != null) return;
                }
            }
            
            // Try shop prefab
            UI_InGame_FloatingWindow_Shop shopUI = UnityEngine.Object.FindFirstObjectByType<UI_InGame_FloatingWindow_Shop>();
            if (shopUI != null && shopUI.itemPrefab != null && shopUI.itemPrefab.rarityImage != null)
            {
                if (shopUI.itemPrefab.rarityImage.material != null && sharedRarityMaterial == null)
                {
                    sharedRarityMaterial = shopUI.itemPrefab.rarityImage.material;
                    RPGLog.Debug(" Got rarity material from shop prefab: " + sharedRarityMaterial.name);
                }
                if (shopUI.itemPrefab.rarityImage.sprite != null && sharedRaritySprite == null)
                {
                    sharedRaritySprite = shopUI.itemPrefab.rarityImage.sprite;
                    RPGLog.Debug(" Got rarity sprite from shop prefab: " + sharedRaritySprite.name);
                }
                if (sharedRarityMaterial != null && sharedRaritySprite != null) return;
            }
            
            // Search all loaded materials for matBackdrop
            if (sharedRarityMaterial == null)
            {
                Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();
                foreach (Material mat in allMaterials)
                {
                    if (mat != null && mat.name == "matBackdrop")
                    {
                        sharedRarityMaterial = mat;
                        RPGLog.Debug(" Got rarity material by name: matBackdrop");
                        break;
                    }
                }
            }
            
            // Search all loaded sprites for texBackdrop
            if (sharedRaritySprite == null)
            {
                Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (Sprite spr in allSprites)
                {
                    if (spr != null && (spr.name == "texBackdrop" || spr.name == "texBackdrop_0"))
                    {
                        sharedRaritySprite = spr;
                        RPGLog.Debug(" Got rarity sprite by name: " + spr.name);
                        break;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            RPGLog.Warning(" Failed to get rarity material/sprite: " + e.Message);
        }
        
        materialSearched = true;
    }
    
    /// <summary>
    /// Create a rarity background Image as a child of the given transform
    /// </summary>
    public static Image CreateRarityImage(Transform parent, int slotIndex)
    {
        TryGetRarityMaterial();
        
        GameObject rarityObj = new GameObject("RarityBackground");
        rarityObj.transform.SetParent(parent, false);
        Image rarityImage = rarityObj.AddComponent<Image>();
        rarityImage.raycastTarget = false;
        
        RectTransform rarityRect = rarityImage.GetComponent<RectTransform>();
        rarityRect.anchorMin = new Vector2(0.1f, 0.1f);   // Inset 10% from edges
        rarityRect.anchorMax = new Vector2(0.9f, 0.9f);   // Inset 10% from edges
        rarityRect.sizeDelta = Vector2.zero;
        rarityRect.anchoredPosition = Vector2.zero;
        rarityRect.localScale = new Vector3(2.5f, 2.5f, 1f); // Larger scale for more visible animation
        rarityImage.color = Color.clear;
        
        // Apply sprite if we have it
        if (sharedRaritySprite != null)
        {
            rarityImage.sprite = sharedRaritySprite;
        }
        
        // Apply material if we have it (instantiated with unique offset)
        if (sharedRarityMaterial != null)
        {
            Material mat = UnityEngine.Object.Instantiate(sharedRarityMaterial);
            mat.SetTextureOffset("_DistortTex", new Vector2(UnityEngine.Random.value, (float)slotIndex * 0.12371f));
            rarityImage.material = mat;
        }
        
        return rarityImage;
    }
    
    /// <summary>
    /// Update rarity image color based on item rarity
    /// </summary>
    public static void UpdateRarityImage(Image rarityImage, RPGItem item, int slotIndex)
    {
        if (rarityImage == null) return;
        
        if (item == null || item.currentStack <= 0)
        {
            rarityImage.color = Color.clear;
            return;
        }
        
        // Try to get material/sprite if we don't have them yet
        if (sharedRarityMaterial == null || sharedRaritySprite == null)
        {
            TryGetRarityMaterial();
            
            // Apply sprite if we just got it
            if (sharedRaritySprite != null && rarityImage.sprite == null)
            {
                rarityImage.sprite = sharedRaritySprite;
            }
            
            // Apply material if we just got it
            if (sharedRarityMaterial != null && (rarityImage.material == null || rarityImage.material.name == "Default UI Material"))
            {
                Material mat = UnityEngine.Object.Instantiate(sharedRarityMaterial);
                mat.SetTextureOffset("_DistortTex", new Vector2(UnityEngine.Random.value, (float)slotIndex * 0.12371f));
                rarityImage.material = mat;
            }
        }
        
        // Set color based on rarity
        rarityImage.color = GetGameRarityColor(item.rarity);
    }
    
    /// <summary>
    /// Get rarity color using game's Dew.GetRarityColor system
    /// </summary>
    public static Color GetGameRarityColor(ItemRarity rarity)
    {
        try
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return Dew.GetRarityColor(Rarity.Common);
                case ItemRarity.Rare:
                    return Dew.GetRarityColor(Rarity.Rare);
                case ItemRarity.Epic:
                    return Dew.GetRarityColor(Rarity.Epic);
                case ItemRarity.Legendary:
                    return Dew.GetRarityColor(Rarity.Legendary);
                default:
                    return Dew.GetRarityColor(Rarity.Common);
            }
        }
        catch
        {
            // Fallback colors if game API fails
            switch (rarity)
            {
                case ItemRarity.Common: return new Color(0.89f, 0.94f, 0.95f);
                case ItemRarity.Rare: return new Color(0.2f, 0.99f, 1f);
                case ItemRarity.Epic: return new Color(0.78f, 0.36f, 0.97f);
                case ItemRarity.Legendary: return new Color(1f, 0.84f, 0f);
                default: return Color.white;
            }
        }
    }
}

/// <summary>
/// Item Slot UI Component - Represents a single inventory slot
/// Middle-click to share item info in chat
/// Implements IPingableCustom to prevent ground pings when showing off items
/// </summary>
public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPingableCustom
{
    private int slotIndex;
    private InventoryUI inventoryUI;
    private InventoryManager inventoryManager;
    private RPGItem currentItem;

    // UI Components
    private Image slotBackground;
    private Image slotBorder;     // Border frame
    private Image hoverOverlay;   // Hover highlight
    private Image rarityImage;    // Rarity glow layer with game's animated material
    private Image itemImage;
    private Text stackText;
    private Text upgradeText;     // Shows +1, +2, etc. for upgraded items
    private RectTransform rectTransform;

    public int SlotIndex { get { return slotIndex; } }

    public void Initialize(int index, InventoryUI ui, InventoryManager manager)
    {
        slotIndex = index;
        inventoryUI = ui;
        inventoryManager = manager;

        rectTransform = GetComponent<RectTransform>();
        
        // Slot background - dark base layer with slight transparency
        slotBackground = gameObject.AddComponent<Image>();
        slotBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
        slotBackground.raycastTarget = true; // Enable raycasts for tooltips

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
        // Make it a frame (outline) by using a child for the inner part
        GameObject innerObj = new GameObject("Inner");
        innerObj.transform.SetParent(borderObj.transform, false);
        Image innerImg = innerObj.AddComponent<Image>();
        innerImg.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
        innerImg.raycastTarget = false;
        RectTransform innerRect = innerImg.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.sizeDelta = new Vector2(-3, -3); // 1.5px border on each side
        innerRect.anchoredPosition = Vector2.zero;

        // Rarity background layer (with animated material from game)
        rarityImage = RarityEffectHelper.CreateRarityImage(transform, index);

        // Item image
        GameObject itemImgObj = new GameObject("ItemImage");
        itemImgObj.transform.SetParent(transform, false);
        itemImage = itemImgObj.AddComponent<Image>();
        itemImage.preserveAspect = true;
        itemImage.raycastTarget = false; // Don't block raycasts
        itemImage.type = Image.Type.Simple; // Ensure type is set
        // Create a default white sprite so the Image component can render colors
        Texture2D whiteTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        whiteTex.SetPixels(pixels);
        whiteTex.Apply();
        Sprite defaultSprite = Sprite.Create(whiteTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100f);
        itemImage.sprite = defaultSprite;
        itemImage.color = Color.white; // Initialize color
        RectTransform itemImgRect = itemImage.GetComponent<RectTransform>();
        itemImgRect.anchorMin = Vector2.zero;
        itemImgRect.anchorMax = Vector2.one;
        itemImgRect.sizeDelta = new Vector2(-6, -6); // Padding
        itemImgRect.anchoredPosition = Vector2.zero;
        itemImage.gameObject.SetActive(false);
        itemImage.enabled = false;

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

        // Stack text - make it more visible with outline/shadow
        GameObject stackTextObj = new GameObject("StackText");
        stackTextObj.transform.SetParent(transform, false);
        stackText = stackTextObj.AddComponent<Text>();
        stackText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        stackText.fontSize = 14;
        stackText.fontStyle = FontStyle.Bold;
        stackText.color = Color.white;
        stackText.alignment = TextAnchor.LowerRight;
        stackText.supportRichText = true;
        
        // Add outline for better visibility
        UnityEngine.UI.Outline outline = stackTextObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
        
        RectTransform stackTextRect = stackText.GetComponent<RectTransform>();
        stackTextRect.anchorMin = new Vector2(0, 0);
        stackTextRect.anchorMax = new Vector2(1, 1);
        stackTextRect.sizeDelta = new Vector2(-4, -4);
        stackTextRect.anchoredPosition = Vector2.zero;
        stackText.gameObject.SetActive(false);
        
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
    
    /// <summary>
    /// Check if the current player's hero can use this item
    /// </summary>
    private bool CanPlayerUseItem(RPGItem item)
    {
        if (item == null) return true;
        if (string.IsNullOrEmpty(item.requiredHero)) return true; // No requirement
        
        // Check current hero
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return false;
        
        string heroTypeName = DewPlayer.local.hero.GetType().Name;
        return RPGItem.HeroMatchesRequirement(heroTypeName, item.requiredHero);
    }

    public void SetItem(RPGItem item)
    {
        currentItem = item;

        if (item == null)
        {
            itemImage.gameObject.SetActive(false);
            itemImage.enabled = false;
            stackText.gameObject.SetActive(false);
            upgradeText.gameObject.SetActive(false);
            slotBackground.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            RarityEffectHelper.UpdateRarityImage(rarityImage, null, slotIndex);
            return;
        }
        
        // Check if item was depleted (stack <= 0)
        if (item.currentStack <= 0)
        {
            currentItem = null;
            itemImage.gameObject.SetActive(false);
            itemImage.enabled = false;
            stackText.gameObject.SetActive(false);
            upgradeText.gameObject.SetActive(false);
            slotBackground.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            RarityEffectHelper.UpdateRarityImage(rarityImage, null, slotIndex);
            return;
        }
        
        // Update rarity background with animated effect
        RarityEffectHelper.UpdateRarityImage(rarityImage, item, slotIndex);
        
        // Keep slot background dark
        slotBackground.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        // Check if player can use this item (hero requirement)
        bool canUse = CanPlayerUseItem(item);
        Color itemTint = canUse ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f); // Darken if can't use
        
        // Lazy load sprite if needed
        ItemDatabase.EnsureSprite(item);
        
        // Set item image
        bool spriteSet = false;
        
        if (item.sprite != null)
        {
            itemImage.sprite = item.sprite;
            itemImage.color = itemTint;
            spriteSet = true;
            itemImage.enabled = true;
            itemImage.gameObject.SetActive(true);
        }
        
        if (!spriteSet)
        {
            // Use colored placeholder only when no image exists
            Color placeholderColor = RarityEffectHelper.GetGameRarityColor(item.rarity);
            if (!canUse) placeholderColor *= 0.4f; // Darken placeholder too
            itemImage.color = placeholderColor;
            itemImage.type = Image.Type.Simple;
            itemImage.gameObject.SetActive(true);
            itemImage.enabled = true;
        }
        
        // Dim rarity effect if can't use
        if (!canUse && rarityImage != null)
        {
            rarityImage.color = new Color(rarityImage.color.r * 0.5f, rarityImage.color.g * 0.5f, rarityImage.color.b * 0.5f, rarityImage.color.a * 0.5f);
        }

        // Set stack text - show if stackable (maxStack > 1 or unlimited stacks with -1)
        if (item.maxStack > 1 || item.maxStack < 0 || item.type == ItemType.Consumable)
        {
            stackText.text = item.currentStack.ToString();
            stackText.gameObject.SetActive(true);
        }
        else
        {
            stackText.gameObject.SetActive(false);
        }
        
        // Set upgrade level text - show +1, +2, etc. for upgraded items
        if (item.upgradeLevel > 0 && item.type != ItemType.Consumable)
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
        
        // RPGLog.Debug(" OnPointerEnter slot " + slotIndex);
        if (currentItem != null)
        {
            if (inventoryUI == null || rectTransform == null) return;
            // For ScreenSpaceOverlay, use screen position directly
            Vector3 tooltipPosition = rectTransform.position + new Vector3(rectTransform.rect.width + 10f, 0, 0);
            inventoryUI.ShowTooltip(currentItem, tooltipPosition, slotIndex);
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem != null && inventoryUI != null)
        {
            inventoryUI.StartDrag(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventoryUI != null && inventoryUI.IsDragging)
        {
            inventoryUI.UpdateDragPosition(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventoryUI != null)
        {
            // Find target slot under cursor
            ItemSlot targetSlot = null;
            EquipmentSlot equipSlot = null;
            ConsumableSlot consumableSlot = null;
            FastSlotUI fastSlotUI = null;
            bool isOverInventoryUI = false;
            
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
                if (eqSlot != null)
                {
                    equipSlot = eqSlot;
                    break;
                }
                
                ConsumableSlot cSlot = result.gameObject.GetComponent<ConsumableSlot>();
                if (cSlot != null)
                {
                    consumableSlot = cSlot;
                    break;
                }
                
                FastSlotUI fSlot = result.gameObject.GetComponent<FastSlotUI>();
                if (fSlot != null)
                {
                    fastSlotUI = fSlot;
                    break;
                }
                
                // Check if we're over any part of the inventory UI panels (not the background overlay)
                Image uiImage = result.gameObject.GetComponent<Image>();
                if (uiImage != null)
                {
                    // Skip the background overlay - it's meant to block game input, not item drops
                    if (result.gameObject.name == "Background")
                    {
                        continue;
                    }
                    
                    // Check if this is part of the inventory UI by looking for InventoryUI in parents
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

            if (equipSlot != null)
            {
                inventoryUI.OnEquipmentSlotDrop(equipSlot);
            }
            else if (consumableSlot != null)
            {
                inventoryUI.OnConsumableSlotDrop(consumableSlot);
            }
            else if (fastSlotUI != null)
            {
                inventoryUI.OnFastSlotUIDrop(fastSlotUI);
            }
            else if (targetSlot != null)
            {
                inventoryUI.EndDrag(targetSlot);
            }
            else if (isOverInventoryUI)
            {
                // Dropped inside inventory UI panel but not on a valid slot - cancel drag, don't drop
                inventoryUI.CancelDrag();
            }
            else
            {
                // Dropped outside inventory UI panels (on background or outside) - drop to game world
                inventoryUI.DropItemToWorld(slotIndex);
            }
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;
        
        // Right-click to equip or use item
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (inventoryUI != null)
            {
                // Shift + Right-click on stackable items = drop partial stack
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (currentItem.currentStack > 1)
                    {
                        inventoryUI.ShowDropAmountDialog(slotIndex);
                        return;
                    }
                }
                
                inventoryUI.RightClickItem(slotIndex);
            }
        }
        // Middle-click is handled by OnPing (IPingableCustom)
    }
    
    /// <summary>
    /// IPingableCustom implementation - handles middle-click ping to show off items
    /// Returns true to indicate we handled the ping (prevents ground ping)
    /// </summary>
    public bool OnPing()
    {
        if (currentItem != null)
        {
            NetworkedItemSystem.ShareItemInChat(currentItem);
            return true;
        }
        return false;
    }
}

