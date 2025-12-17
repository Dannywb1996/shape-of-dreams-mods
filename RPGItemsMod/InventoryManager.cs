using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Inventory Manager - Handles item storage and operations
/// Now supports unlimited inventory with dynamic expansion
/// </summary>
public class InventoryManager
{
    private List<RPGItem> items = new List<RPGItem>();
    private int initialSlots = 30; // Starting slots (5 columns x 6 rows)
    private int columnsPerRow = 5; // Fixed columns, rows expand
    private string modPath;
    
    // Event for when inventory expands (UI needs to add slots)
    public Action<int> OnInventoryExpanded;
    
    // Event for when inventory resets (UI needs to rebuild)
    public Action OnInventoryReset;
    
    // Event for when an item is added (UI needs to refresh)
    public Action OnItemAdded;

    public int SlotCount { get { return items.Count; } }
    public int InitialSlotCount { get { return initialSlots; } }
    public int MaxSlots { get { return items.Count; } } // For backwards compatibility
    public List<RPGItem> Items { get { return items; } }
    public int ColumnsPerRow { get { return columnsPerRow; } }

    public void Initialize()
    {
        // Initialize with starting slots (empty)
        // Items will be loaded from save or created fresh in RPGItemsMod.CheckAndHandleNewRun()
        for (int i = 0; i < initialSlots; i++)
        {
            items.Add(null);
        }
        // NOTE: Do NOT call CreateExampleItems() here!
        // Starter items are only added for NEW runs in RPGItemsMod.cs after checking persistence
    }
    
    /// <summary>
    /// Expand inventory by one row (adds columnsPerRow slots)
    /// </summary>
    public void ExpandInventory()
    {
        int newSlots = columnsPerRow;
        int startIndex = items.Count;
        
        for (int i = 0; i < newSlots; i++)
        {
            items.Add(null);
        }
        
        RPGLog.Debug(string.Format(" Inventory expanded: {0} -> {1} slots", startIndex, items.Count));
        
        // Notify UI to add new slots
        if (OnInventoryExpanded != null)
        {
            OnInventoryExpanded.Invoke(newSlots);
        }
    }
    
    /// <summary>
    /// Check if inventory needs expansion (all slots full)
    /// </summary>
    public bool NeedsExpansion()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) return false;
        }
        return true;
    }

    public void SetModPath(string path)
    {
        modPath = path;
    }

    public void CreateExampleItems()
    {
        // Players start with empty inventory + 20 common potions
        // Use the SAME ID as ItemDatabase (444) so they stack with dropped potions!
        // Common: 15% heal - Unlimited stacks for consumables
        RPGItem starterPotion = ItemDatabase.GetItemById(444); // Dreamer's Tonic (Common Health Potion)
        if (starterPotion != null)
        {
            // Clone the item so we don't modify the database original
            starterPotion = starterPotion.Clone();
            starterPotion.currentStack = 20;
            
            // Load the sprite
            if (!string.IsNullOrEmpty(modPath))
            {
                starterPotion.sprite = LoadItemSprite(starterPotion.imagePath);
            }
            
            AddItem(starterPotion);
            RPGLog.Debug(" Created starter potion with ID " + starterPotion.id + " x" + starterPotion.currentStack);
        }
        else
        {
            RPGLog.Error(" Could not find Dreamer's Tonic (ID 444) in ItemDatabase!");
        }
        
        // Items created - monsters will drop loot later
    }
    
    /// <summary>
    /// Get the current hero type name (e.g., "Hero_Aurena", "Hero_Lacerta")
    /// </summary>
    public static string GetCurrentHeroTypeName()
    {
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            return DewPlayer.local.hero.GetType().Name;
        }
        return "";
    }
    
    /// <summary>
    /// Check if current hero is Aurena
    /// </summary>
    public static bool IsCurrentHeroAurena()
    {
        return GetCurrentHeroTypeName() == "Hero_Aurena";
    }
    
    private int GetItemCount()
    {
        int count = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null) count++;
        }
        return count;
    }

    public bool AddItem(RPGItem item)
    {
        if (item == null) return false;

        // Try to stack with existing items first (consumables have unlimited stacks)
        if (item.HasUnlimitedStacks || item.maxStack > 1)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].id == item.id)
                {
                    // For unlimited stacks, always add
                    if (items[i].HasUnlimitedStacks)
                    {
                        items[i].currentStack += item.currentStack;
                        NotifyItemAdded();
                        return true;
                    }
                    // For limited stacks, check space
                    else if (items[i].currentStack < items[i].maxStack)
                    {
                        int spaceLeft = items[i].maxStack - items[i].currentStack;
                        if (item.currentStack <= spaceLeft)
                        {
                            items[i].currentStack += item.currentStack;
                            NotifyItemAdded();
                            return true;
                        }
                        else
                        {
                            items[i].currentStack = items[i].maxStack;
                            item.currentStack -= spaceLeft;
                        }
                    }
                }
            }
        }

        // Find empty slot
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                NotifyItemAdded();
                return true;
            }
        }

        // No empty slot found - expand inventory and add
        ExpandInventory();
        
        // Now add to the first new slot
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                NotifyItemAdded();
                return true;
            }
        }
        
        return false; // Should never reach here
    }
    
    /// <summary>
    /// Notify listeners that an item was added
    /// </summary>
    private void NotifyItemAdded()
    {
        if (OnItemAdded != null)
        {
            OnItemAdded.Invoke();
        }
    }

    public bool RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) return false;
        if (items[slotIndex] == null) return false;

        items[slotIndex] = null;
        return true;
    }

    public bool RemoveItem(int slotIndex, int amount)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) return false;
        if (items[slotIndex] == null) return false;

        if (items[slotIndex].currentStack <= amount)
        {
            items[slotIndex] = null;
        }
        else
        {
            items[slotIndex].currentStack -= amount;
        }
        return true;
    }

    public RPGItem GetItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) return null;
        return items[slotIndex];
    }

    public bool SwapItems(int slotIndex1, int slotIndex2)
    {
        if (slotIndex1 < 0 || slotIndex1 >= items.Count) return false;
        if (slotIndex2 < 0 || slotIndex2 >= items.Count) return false;

        RPGItem temp = items[slotIndex1];
        items[slotIndex1] = items[slotIndex2];
        items[slotIndex2] = temp;
        return true;
    }
    
    /// <summary>
    /// Get all non-null items
    /// </summary>
    public List<RPGItem> GetAllItems()
    {
        List<RPGItem> result = new List<RPGItem>();
        foreach (RPGItem item in items)
        {
            if (item != null)
            {
                result.Add(item);
            }
        }
        return result;
    }
    
    /// <summary>
    /// Get all items including nulls (for slot-based operations)
    /// </summary>
    public List<RPGItem> GetAllItemsWithNulls()
    {
        return new List<RPGItem>(items);
    }
    
    /// <summary>
    /// Clear all items from inventory
    /// </summary>
    public void ClearInventory()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i] = null;
        }
        // Cleared
    }
    
    /// <summary>
    /// Reset inventory to initial slot count (for new runs)
    /// </summary>
    public void ResetToInitialSlots()
    {
        items.Clear();
        for (int i = 0; i < initialSlots; i++)
        {
            items.Add(null);
        }
        RPGLog.Debug(" Inventory reset to " + initialSlots + " slots");
        
        // Notify UI to rebuild
        if (OnInventoryReset != null)
        {
            OnInventoryReset();
        }
    }
    
    /// <summary>
    /// Sort and organize inventory items
    /// Order: Rarity (Legendary > Epic > Rare > Common) -> Type -> Name (groups same items) -> Upgrade Level
    /// </summary>
    public void SortInventory()
    {
        // Collect all non-null items
        List<RPGItem> sortedItems = new List<RPGItem>();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
            {
                sortedItems.Add(items[i]);
            }
        }
        
        // Sort using custom comparer
        sortedItems.Sort(CompareItems);
        
        // Clear inventory and refill with sorted items
        for (int i = 0; i < items.Count; i++)
        {
            items[i] = null;
        }
        
        for (int i = 0; i < sortedItems.Count && i < items.Count; i++)
        {
            items[i] = sortedItems[i];
        }
        
        RPGLog.Debug(" Inventory sorted: " + sortedItems.Count + " items organized");
    }
    
    /// <summary>
    /// Compare two items for sorting
    /// Priority: Usable by hero (first) -> Rarity (desc) -> Type -> Name -> Upgrade Level (desc)
    /// </summary>
    private int CompareItems(RPGItem a, RPGItem b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;
        
        // Get current hero type for usability check
        string currentHero = GetCurrentHeroTypeName();
        
        // 0. Sort by usability (items current hero can use come first)
        bool aUsable = CanHeroUseItem(a, currentHero);
        bool bUsable = CanHeroUseItem(b, currentHero);
        
        if (aUsable && !bUsable) return -1; // a comes first
        if (!aUsable && bUsable) return 1;  // b comes first
        
        // 1. Sort by rarity (Legendary first, Common last)
        int rarityCompare = ((int)b.rarity).CompareTo((int)a.rarity);
        if (rarityCompare != 0) return rarityCompare;
        
        // 2. Sort by item type (equipment types grouped together)
        int typeCompare = GetTypeSortOrder(a.type).CompareTo(GetTypeSortOrder(b.type));
        if (typeCompare != 0) return typeCompare;
        
        // 3. Sort by name (groups same items together)
        int nameCompare = string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        if (nameCompare != 0) return nameCompare;
        
        // 4. Sort by upgrade level (higher first)
        return b.upgradeLevel.CompareTo(a.upgradeLevel);
    }
    
    /// <summary>
    /// Check if a hero can use/equip an item
    /// </summary>
    private bool CanHeroUseItem(RPGItem item, string heroType)
    {
        if (item == null) return false;
        if (string.IsNullOrEmpty(heroType)) return true; // No hero = all items usable
        
        // Consumables, materials, and quest items are usable by all
        if (item.type == ItemType.Consumable || 
            item.type == ItemType.Material || 
            item.type == ItemType.Quest)
        {
            return true;
        }
        
        // If item has no hero restriction, it's usable by all
        if (string.IsNullOrEmpty(item.requiredHero))
        {
            return true;
        }
        
        // Check if item is for this hero (handles Shell/Husk equivalence)
        return RPGItem.HeroMatchesRequirement(heroType, item.requiredHero);
    }
    
    /// <summary>
    /// Get sort order for item types (lower = earlier in inventory)
    /// </summary>
    private int GetTypeSortOrder(ItemType type)
    {
        switch (type)
        {
            // Weapons first
            case ItemType.TwoHandedWeapon: return 0;
            case ItemType.Weapon: return 1;
            case ItemType.OffHand: return 2;
            
            // Armor next
            case ItemType.Helmet: return 10;
            case ItemType.ChestArmor: return 11;
            case ItemType.Pants: return 12;
            case ItemType.Belt: return 13;
            case ItemType.Boots: return 14;
            
            // Accessories
            case ItemType.Amulet: return 20;
            case ItemType.Ring: return 21;
            
            // Consumables and misc last
            case ItemType.Consumable: return 30;
            case ItemType.Material: return 40;
            case ItemType.Quest: return 50;
            
            default: return 99;
        }
    }

    public Sprite LoadItemSprite(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath)) return null;

        try
        {
            string fullPath = Path.Combine(modPath, "images", imagePath);
            if (!File.Exists(fullPath)) return null;

            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2);
            
            // LoadImage is in ImageConversion class in Unity 6
            Assembly imageConversionAssembly = null;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == "UnityEngine.ImageConversionModule")
                {
                    imageConversionAssembly = asm;
                    break;
                }
            }
            
            if (imageConversionAssembly != null)
            {
                Type imageConversionType = imageConversionAssembly.GetType("UnityEngine.ImageConversion");
                if (imageConversionType != null)
                {
                    MethodInfo loadImageMethod = imageConversionType.GetMethod("LoadImage", 
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new Type[] { typeof(Texture2D), typeof(byte[]) },
                        null);
                    
                    if (loadImageMethod != null)
                    {
                        loadImageMethod.Invoke(null, new object[] { texture, fileData });
                        
                        if (texture.width > 2 && texture.height > 2)
                        {
                            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                        }
                    }
                }
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

