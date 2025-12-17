using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Item Persistence System - Saves and loads inventory/equipment within a run
/// Items persist across disconnect/reconnect but reset on new run
/// </summary>
public static class ItemPersistence
{
    private static string _saveFolder;
    private static string _currentRunId = "";
    private static bool _isInitialized = false;
    
    // File names
    private const string INVENTORY_FILE = "inventory.dat";
    private const string EQUIPMENT_FILE = "equipment.dat";
    private const string RUN_ID_FILE = "current_run.dat";
    private const string STATS_FILE = "stats.dat";
    
    /// <summary>
    /// Check if persistence system is initialized
    /// </summary>
    public static bool IsInitialized()
    {
        return _isInitialized;
    }
    
    /// <summary>
    /// Reset the persistence state (but don't clear save files)
    /// Call this when completely resetting the mod between runs
    /// </summary>
    public static void ResetState()
    {
        _isInitialized = false;
        _currentRunId = "";
        RPGLog.Debug(" Persistence state reset (files preserved)");
    }
    
    /// <summary>
    /// Initialize the persistence system
    /// </summary>
    public static void Initialize(string modPath)
    {
        _saveFolder = Path.Combine(modPath, "saves");
        
        // Create saves folder if it doesn't exist
        if (!Directory.Exists(_saveFolder))
        {
            Directory.CreateDirectory(_saveFolder);
        }
        
        _isInitialized = true;
        RPGLog.Debug(" Persistence initialized: " + _saveFolder);
    }
    
    /// <summary>
    /// Check if this is a new run and handle accordingly
    /// Call this when entering a game
    /// </summary>
    public static bool CheckAndHandleNewRun(InventoryManager inventory, EquipmentManager equipment)
    {
        if (!_isInitialized)
        {
            RPGLog.Warning(" CheckAndHandleNewRun: Not initialized!");
            return false;
        }
        
        string currentRunId = GetCurrentGameRunId();
        string savedRunId = LoadSavedRunId();
        
        RPGLog.Debug(" === PERSISTENCE CHECK ===");
        RPGLog.Debug(" Current runId: '" + currentRunId + "'");
        RPGLog.Debug(" Saved runId: '" + savedRunId + "'");
        
        if (string.IsNullOrEmpty(currentRunId))
        {
            RPGLog.Debug(" No run ID yet - waiting...");
            return false;
        }
        
        bool runIdMatches = currentRunId == savedRunId;
        
        if (!runIdMatches)
        {
            RPGLog.Debug(" *** NEW RUN *** (old: '" + savedRunId + "')");
            ClearAllSavedData();
            SaveRunId(currentRunId);
            _currentRunId = currentRunId;
            return true; // Is new run
        }
        else
        {
            RPGLog.Debug(" *** CONTINUING RUN ***");
            _currentRunId = currentRunId;
            LoadInventory(inventory);
            LoadEquipment(equipment, inventory);
            return false; // Not new run
        }
    }
    
    /// <summary>
    /// Get a unique identifier for the current game run
    /// Uses GameManager.runId (GUID) as identifier - this is the game's official run identifier
    /// </summary>
    private static string GetCurrentGameRunId()
    {
        try
        {
            if (NetworkedManagerBase<GameManager>.instance != null)
            {
                string runId = NetworkedManagerBase<GameManager>.instance.runId;
                if (!string.IsNullOrEmpty(runId))
                {
                    return runId;
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Could not get run ID: " + e.Message);
        }
        return "";
    }
    
    /// <summary>
    /// Save the current run ID
    /// </summary>
    private static void SaveRunId(string runId)
    {
        try
        {
            string path = Path.Combine(_saveFolder, RUN_ID_FILE);
            File.WriteAllText(path, runId);
            
            // Verify write
            string verify = File.ReadAllText(path).Trim();
            if (verify != runId)
            {
                RPGLog.Error(" RunID MISMATCH! Saved: " + runId + " Read: " + verify);
            }
            else
            {
                RPGLog.Debug(" Saved runId: " + runId);
            }
        }
        catch (Exception e)
        {
            RPGLog.Error(" Failed to save run ID: " + e.Message);
        }
    }
    
    /// <summary>
    /// Load the saved run ID
    /// </summary>
    private static string LoadSavedRunId()
    {
        try
        {
            string path = Path.Combine(_saveFolder, RUN_ID_FILE);
            if (File.Exists(path))
            {
                return File.ReadAllText(path).Trim();
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Could not load run ID: " + e.Message);
        }
        return "";
    }
    
    /// <summary>
    /// Clear all saved data (for new runs)
    /// </summary>
    public static void ClearAllSavedData()
    {
        try
        {
            string inventoryPath = Path.Combine(_saveFolder, INVENTORY_FILE);
            string equipmentPath = Path.Combine(_saveFolder, EQUIPMENT_FILE);
            string runIdPath = Path.Combine(_saveFolder, RUN_ID_FILE);
            
            if (File.Exists(inventoryPath)) File.Delete(inventoryPath);
            if (File.Exists(equipmentPath)) File.Delete(equipmentPath);
            if (File.Exists(runIdPath)) File.Delete(runIdPath);
            
            RPGLog.Debug(" Cleared saved data");
        }
        catch (Exception e)
        {
            RPGLog.Error(" Failed to clear saved data: " + e.Message);
        }
    }
    
    #region Inventory Save/Load
    
    /// <summary>
    /// Save inventory to file
    /// Thread-safe: Unity runs on single main thread, but add defensive checks
    /// </summary>
    public static void SaveInventory(InventoryManager inventory)
    {
        if (!_isInitialized || inventory == null) return;
        
        try
        {
            // Validate save folder exists
            if (string.IsNullOrEmpty(_saveFolder) || !Directory.Exists(_saveFolder))
            {
                RPGLog.Warning(" Save folder not initialized, cannot save inventory");
                return;
            }
            
            List<RPGItem> items = inventory.GetAllItems();
            if (items == null)
            {
                RPGLog.Warning(" Cannot save - inventory items list is null");
                return;
            }
            
            string data = SerializeItemList(items);
            if (string.IsNullOrEmpty(data))
            {
                RPGLog.Warning(" Serialized data is empty, skipping save");
                return;
            }
            
            string path = Path.Combine(_saveFolder, INVENTORY_FILE);
            
            // Write to temp file first, then rename (atomic operation)
            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, data);
            
            // Atomic replace (if file exists, delete it first)
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tempPath, path);
            
            RPGLog.Debug(" Saved inventory: " + items.Count + " items");
        }
        catch (Exception e)
        {
            RPGLog.Error(" Failed to save inventory: " + e.Message + "\n" + e.StackTrace);
        }
    }
    
    /// <summary>
    /// Load inventory from file
    /// </summary>
    public static void LoadInventory(InventoryManager inventory)
    {
        if (!_isInitialized || inventory == null) return;
        
        try
        {
            string path = Path.Combine(_saveFolder, INVENTORY_FILE);
            if (!File.Exists(path))
            {
                RPGLog.Debug(" No saved inventory found");
                return;
            }
            
            string data = File.ReadAllText(path);
            List<RPGItem> items = DeserializeItemList(data);
            
            inventory.ClearInventory();
            foreach (RPGItem item in items)
            {
                // Skip consumables with 0 stack
                if (item.type == ItemType.Consumable && item.currentStack <= 0)
                {
                    RPGLog.Debug(" Skipping inventory consumable with 0 stack: " + item.name);
                    continue;
                }
                
                if (!string.IsNullOrEmpty(item.imagePath))
                {
                    item.sprite = inventory.LoadItemSprite(item.imagePath);
                }
                inventory.AddItem(item);
            }
            
            RPGLog.Debug(" Loaded inventory: " + items.Count + " items");
        }
        catch (Exception e)
        {
            RPGLog.Error(" Failed to load inventory: " + e.Message);
        }
    }
    
    #endregion
    
    #region Equipment Save/Load
    
    /// <summary>
    /// Save equipment to file
    /// Thread-safe: Uses atomic file write pattern
    /// </summary>
    public static void SaveEquipment(EquipmentManager equipment)
    {
        if (!_isInitialized || equipment == null) return;
        
        try
        {
            // Validate save folder exists
            if (string.IsNullOrEmpty(_saveFolder) || !Directory.Exists(_saveFolder))
            {
                RPGLog.Warning(" Save folder not initialized, cannot save equipment");
                return;
            }
            
            List<string> lines = new List<string>();
            int equipCount = 0;
            int consumableCount = 0;
            
            // Save each equipment slot
            foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
            {
                RPGItem item = equipment.GetEquippedItem(slotType);
                if (item != null)
                {
                    lines.Add(string.Format("{0}:{1}", (int)slotType, SerializeItem(item)));
                    equipCount++;
                }
            }
            
            // Save consumable slots (0-3) - only if they have stacks remaining
            for (int i = 0; i < 4; i++)
            {
                RPGItem consumable = equipment.GetConsumable(i);
                if (consumable != null && consumable.currentStack > 0)
                {
                    lines.Add(string.Format("C{0}:{1}", i, SerializeItem(consumable)));
                    consumableCount++;
                }
            }
            
            string path = Path.Combine(_saveFolder, EQUIPMENT_FILE);
            
            // Write to temp file first, then rename (atomic operation)
            string tempPath = path + ".tmp";
            File.WriteAllLines(tempPath, lines.ToArray());
            
            // Atomic replace
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tempPath, path);
            
            RPGLog.Debug(" Saved equipment: " + equipCount + " equip, " + consumableCount + " consumables");
        }
        catch (Exception e)
        {
            RPGLog.Error(" Failed to save equipment: " + e.Message + "\n" + e.StackTrace);
        }
    }
    
    /// <summary>
    /// Load equipment from file
    /// </summary>
    public static void LoadEquipment(EquipmentManager equipment, InventoryManager inventory = null)
    {
        if (!_isInitialized || equipment == null) return;
        
        try
        {
            string path = Path.Combine(_saveFolder, EQUIPMENT_FILE);
            if (!File.Exists(path))
            {
                RPGLog.Debug(" No saved equipment found");
                return;
            }
            
            string[] lines = File.ReadAllLines(path);
            equipment.UnequipAll();
            
            int equipCount = 0;
            int consumableCount = 0;
            
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                
                int colonIndex = line.IndexOf(':');
                if (colonIndex < 0) continue;
                
                string slotPart = line.Substring(0, colonIndex);
                string itemData = line.Substring(colonIndex + 1);
                
                RPGItem item = DeserializeItem(itemData);
                if (item == null) continue;
                
                if (!string.IsNullOrEmpty(item.imagePath) && inventory != null)
                {
                    item.sprite = inventory.LoadItemSprite(item.imagePath);
                }
                
                if (slotPart.StartsWith("C"))
                {
                    // Skip consumables with 0 stack
                    if (item.currentStack <= 0)
                    {
                        RPGLog.Debug(" Skipping consumable with 0 stack: " + item.name);
                        continue;
                    }
                    int slotIndex = int.Parse(slotPart.Substring(1));
                    equipment.SetConsumable(slotIndex, item);
                    consumableCount++;
                }
                else
                {
                    EquipmentSlotType slotType = (EquipmentSlotType)int.Parse(slotPart);
                    equipment.EquipToSlot(item, slotType, false);
                    equipCount++;
                }
            }
            
            equipment.ReapplyAllStats();
            RPGLog.Debug(" Loaded equipment: " + equipCount + " equip, " + consumableCount + " consumables");
        }
        catch (Exception e)
        {
            RPGLog.Error(" Failed to load equipment: " + e.Message);
        }
    }
    
    #endregion
    
    #region Serialization Helpers
    
    private static string SerializeItemList(List<RPGItem> items)
    {
        List<string> lines = new List<string>();
        foreach (RPGItem item in items)
        {
            if (item != null)
            {
                lines.Add(SerializeItem(item));
            }
        }
        return string.Join("\n", lines.ToArray());
    }
    
    private static List<RPGItem> DeserializeItemList(string data)
    {
        List<RPGItem> items = new List<RPGItem>();
        if (string.IsNullOrEmpty(data)) return items;
        
        string[] lines = data.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line.Trim())) continue;
            RPGItem item = DeserializeItem(line);
            if (item != null) items.Add(item);
        }
        return items;
    }
    
    private static string SerializeItem(RPGItem item)
    {
        // Format: id|name|desc|type|rarity|atk|def|hp|heal%|imagePath|currentStack|maxStack|
        //         moveSpd|dodge|goldOnHit|upgradeLevel|abilityPower|critChance|critDmg|
        //         thorns|lifeSteal|dustOnHit|regen|autoAttack|autoAim|atkSpeed|memoryHaste|
        //         elementalType|elementalStacks|shieldPercentage|requiredHero|dustSpentUpgrading
        int elemType = item.elementalType.HasValue ? ((int)item.elementalType.Value + 1) : 0; // 0 = none, 1-4 = Fire/Cold/Light/Dark
        return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}|{31}",
            item.id,                              // 0
            EscapeString(item.name),              // 1
            EscapeString(item.description ?? ""), // 2
            (int)item.type,                       // 3
            (int)item.rarity,                     // 4
            item.attackBonus,                     // 5
            item.defenseBonus,                    // 6
            item.healthBonus,                     // 7
            item.healPercentage,                  // 8
            EscapeString(item.imagePath ?? ""),   // 9
            item.currentStack,                    // 10
            item.maxStack,                        // 11
            item.moveSpeedBonus,                  // 12
            item.dodgeChargesBonus,               // 13
            item.goldOnKill,                       // 14
            item.upgradeLevel,                    // 15
            item.abilityPowerBonus,               // 16
            item.criticalChance,                  // 17
            item.criticalDamage,                  // 18
            item.thorns,                          // 19
            item.lifeSteal,                       // 20
            item.dustOnKill,                       // 21
            item.regeneration,                    // 22
            item.autoAttack ? 1 : 0,              // 23
            item.autoAim ? 1 : 0,                 // 24
            item.attackSpeed,                     // 25
            item.memoryHaste,                     // 26
            elemType,                             // 27
            item.elementalStacks,                 // 28
            item.shieldPercentage,                // 29
            EscapeString(item.requiredHero ?? ""),// 30
            item.dustSpentUpgrading);             // 31
    }
    
    private static RPGItem DeserializeItem(string data)
    {
        try
        {
            string[] parts = data.Split('|');
            if (parts.Length < 11) return null;
            
            int id = int.Parse(parts[0]);
            string name = UnescapeString(parts[1]);
            string description = UnescapeString(parts[2]);
            ItemType type = (ItemType)int.Parse(parts[3]);
            ItemRarity rarity = (ItemRarity)int.Parse(parts[4]);
            
            // Serialization order: index 10 = currentStack, index 11 = maxStack
            int maxStack = 1;
            if (parts.Length >= 12) maxStack = int.Parse(parts[11]);
            
            // Get fresh item from database to ensure name/description are current (for translation support)
            RPGItem dbItem = ItemDatabase.GetItemById(id);
            if (dbItem != null)
            {
                name = dbItem.name;
                description = dbItem.description;
            }
            
            RPGItem item = new RPGItem(id, name, description, type, rarity, maxStack);
            
            // Refresh localized fields so game tooltips show translated text
            item.RefreshLocalizedFields();
            
            item.attackBonus = int.Parse(parts[5]);
            item.defenseBonus = int.Parse(parts[6]);
            item.healthBonus = int.Parse(parts[7]);
            item.healPercentage = float.Parse(parts[8]);
            item.imagePath = UnescapeString(parts[9]);
            if (parts.Length >= 11) item.currentStack = int.Parse(parts[10]);
            
            // New stats (backwards compatible)
            if (parts.Length >= 13) item.moveSpeedBonus = float.Parse(parts[12]);
            if (parts.Length >= 14) item.dodgeChargesBonus = int.Parse(parts[13]);
            if (parts.Length >= 15) item.goldOnKill = int.Parse(parts[14]);
            if (parts.Length >= 16) item.upgradeLevel = int.Parse(parts[15]);
            if (parts.Length >= 17) item.abilityPowerBonus = int.Parse(parts[16]);
            if (parts.Length >= 18) item.criticalChance = float.Parse(parts[17]);
            if (parts.Length >= 19) item.criticalDamage = float.Parse(parts[18]);
            if (parts.Length >= 20) item.thorns = float.Parse(parts[19]);
            if (parts.Length >= 21) item.lifeSteal = float.Parse(parts[20]);
            if (parts.Length >= 22) item.dustOnKill = int.Parse(parts[21]);
            if (parts.Length >= 23) item.regeneration = float.Parse(parts[22]);
            if (parts.Length >= 24) item.autoAttack = int.Parse(parts[23]) == 1;
            if (parts.Length >= 25) item.autoAim = int.Parse(parts[24]) == 1;
            if (parts.Length >= 26) item.attackSpeed = float.Parse(parts[25]);
            if (parts.Length >= 27) item.memoryHaste = float.Parse(parts[26]);
            
            // Elemental data
            if (parts.Length >= 28)
            {
                int elemType = int.Parse(parts[27]);
                if (elemType > 0)
                {
                    item.elementalType = (ElementalType)(elemType - 1); // 1-4 -> 0-3 (Fire/Cold/Light/Dark)
                }
            }
            if (parts.Length >= 29) item.elementalStacks = int.Parse(parts[28]);
            if (parts.Length >= 30) item.shieldPercentage = float.Parse(parts[29]);
            if (parts.Length >= 31) item.requiredHero = UnescapeString(parts[30]);
            if (parts.Length >= 32) item.dustSpentUpgrading = int.Parse(parts[31]);
            
            return item;
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to deserialize item: " + e.Message);
            return null;
        }
    }
    
    private static string EscapeString(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        return str.Replace("|", "\\|").Replace("\n", "\\n");
    }
    
    private static string UnescapeString(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        return str.Replace("\\|", "|").Replace("\\n", "\n");
    }
    
    #endregion
    
    /// <summary>
    /// Save all data (call periodically or on important events)
    /// </summary>
    public static void SaveAll(InventoryManager inventory, EquipmentManager equipment)
    {
        SaveInventory(inventory);
        SaveEquipment(equipment);
    }
    
    /// <summary>
    /// Save stats data
    /// </summary>
    public static void SaveStats(string statsJson)
    {
        if (!_isInitialized) return;
        
        try
        {
            string path = Path.Combine(_saveFolder, STATS_FILE);
            File.WriteAllText(path, statsJson);
            RPGLog.Debug(" Saved stats");
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to save stats: " + e.Message);
        }
    }
    
    /// <summary>
    /// Load stats data
    /// </summary>
    public static string LoadStats()
    {
        if (!_isInitialized) return null;
        
        try
        {
            string path = Path.Combine(_saveFolder, STATS_FILE);
            if (!File.Exists(path)) return null;
            
            string json = File.ReadAllText(path);
            RPGLog.Debug(" Loaded stats");
            return json;
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to load stats: " + e.Message);
            return null;
        }
    }
    
    // ============================================================
    // WEAPON MASTERY PERSISTENCE
    // ============================================================
    
    private const string MASTERY_FILE = "mastery.dat";
    
    /// <summary>
    /// Save weapon mastery data (persists across all runs!)
    /// </summary>
    public static void SaveMastery(string data)
    {
        if (!_isInitialized) return;
        if (string.IsNullOrEmpty(data)) return;
        
        try
        {
            // Mastery saves to a global location, not per-run
            string globalFolder = Path.Combine(Application.persistentDataPath, "RPGItemsMod");
            if (!Directory.Exists(globalFolder))
            {
                Directory.CreateDirectory(globalFolder);
            }
            
            string path = Path.Combine(globalFolder, MASTERY_FILE);
            File.WriteAllText(path, data);
            RPGLog.Debug(" Saved weapon mastery data");
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to save mastery: " + e.Message);
        }
    }
    
    /// <summary>
    /// Load weapon mastery data (global, persists across all runs!)
    /// </summary>
    public static string LoadMastery()
    {
        try
        {
            string globalFolder = Path.Combine(Application.persistentDataPath, "RPGItemsMod");
            string path = Path.Combine(globalFolder, MASTERY_FILE);
            
            if (!File.Exists(path)) return null;
            
            string data = File.ReadAllText(path);
            RPGLog.Debug(" Loaded weapon mastery data");
            return data;
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to load mastery: " + e.Message);
            return null;
        }
    }
}
