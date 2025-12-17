using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles multiplayer sync for dropped items using the new RPC and persistentSyncedData API.
/// Uses ActorManager.instance.serverActor for global state and message handlers.
/// Items are private by default, shared via middle-click.
/// </summary>
public static class NetworkedItemSystem
{
    #region Message Types
    
    /// <summary>Message sent when a player shares an item with others</summary>
    [Serializable]
    public class ItemShareMessage
    {
        public uint dropId;
        public string itemJson;
        public float posX, posY, posZ;
        public uint ownerNetId;
    }
    
    /// <summary>Message sent when a player picks up an item</summary>
    [Serializable]
    public class ItemPickupMessage
    {
        public uint dropId;
        public uint pickerNetId;
    }
    
    /// <summary>Message sent when a player dismantles an item</summary>
    [Serializable]
    public class ItemDismantleMessage
    {
        public uint dropId;
        public int dustAmount;
    }
    
    /// <summary>Message sent when a player shows off an item in chat</summary>
    [Serializable]
    public class ItemShowoffMessage
    {
        public string itemJson;
        public string playerName;
        public uint ownerNetId;
    }
    
    /// <summary>Client request to share an item (sent to server)</summary>
    [Serializable]
    public class ShareItemRequest
    {
        public uint dropId;
        public string itemJson;
        public float posX, posY, posZ;
    }
    
    /// <summary>Client request to pick up an item (sent to server)</summary>
    [Serializable]
    public class PickupItemRequest
    {
        public uint dropId;
    }
    
    /// <summary>Client request to dismantle an item (sent to server)</summary>
    [Serializable]
    public class DismantleItemRequest
    {
        public uint dropId;
    }
    
    /// <summary>Client request to show off an item in chat (sent to server)</summary>
    [Serializable]
    public class ShowoffItemRequest
    {
        public string itemJson;
    }
    
    /// <summary>Server notification of a private drop for a specific player</summary>
    [Serializable]
    public class PrivateDropMessage
    {
        public uint dropId;
        public string itemJson;
        public float posX, posY, posZ;
    }
    
    #endregion
    
    #region Serializable Item Data
    
    /// <summary>Serializable version of RPGItem for network transfer</summary>
    [Serializable]
    public class SerializableItem
    {
        public int id;
        public string name;
        public string description;
        public int type;
        public int rarity;
        public int attackBonus;
        public int defenseBonus;
        public int healthBonus;
        public int abilityPowerBonus;
        public float criticalChance;
        public float criticalDamage;
        public float moveSpeedBonus;
        public int dodgeChargesBonus;
        public int goldOnKill;
        public int dustOnKill;
        public float lifeSteal;
        public float thorns;
        public float regeneration;
        public float attackSpeed;
        public float memoryHaste;
        public bool autoAttack;
        public bool autoAim;
        public int elementalType; // -1 for none
        public int elementalStacks;
        public string requiredHero;
        public float healPercentage;
        public float shieldPercentage;
        public int maxStack;
        public int currentStack;
        public int upgradeLevel;
        public string imagePath;
        
        public static SerializableItem FromRPGItem(RPGItem item)
        {
            SerializableItem s = new SerializableItem();
            s.id = item.id;
            s.name = item.name;
            s.description = item.description != null ? item.description : "";
            s.type = (int)item.type;
            s.rarity = (int)item.rarity;
            s.attackBonus = item.attackBonus;
            s.defenseBonus = item.defenseBonus;
            s.healthBonus = item.healthBonus;
            s.abilityPowerBonus = item.abilityPowerBonus;
            s.criticalChance = item.criticalChance;
            s.criticalDamage = item.criticalDamage;
            s.moveSpeedBonus = item.moveSpeedBonus;
            s.dodgeChargesBonus = item.dodgeChargesBonus;
            s.goldOnKill = item.goldOnKill;
            s.dustOnKill = item.dustOnKill;
            s.lifeSteal = item.lifeSteal;
            s.thorns = item.thorns;
            s.regeneration = item.regeneration;
            s.attackSpeed = item.attackSpeed;
            s.memoryHaste = item.memoryHaste;
            s.autoAttack = item.autoAttack;
            s.autoAim = item.autoAim;
            s.elementalType = item.elementalType.HasValue ? (int)item.elementalType.Value : -1;
            s.elementalStacks = item.elementalStacks;
            s.requiredHero = item.requiredHero != null ? item.requiredHero : "";
            s.healPercentage = item.healPercentage;
            s.shieldPercentage = item.shieldPercentage;
            s.maxStack = item.maxStack;
            s.currentStack = item.currentStack;
            s.upgradeLevel = item.upgradeLevel;
            s.imagePath = item.imagePath != null ? item.imagePath : "";
            return s;
        }
        
        public RPGItem ToRPGItem()
        {
            RPGItem item = new RPGItem(id, name, description, (ItemType)type, (ItemRarity)rarity);
            
            // Refresh localized fields so tooltips show translated text
            item.RefreshLocalizedFields();
            
            item.attackBonus = attackBonus;
            item.defenseBonus = defenseBonus;
            item.healthBonus = healthBonus;
            item.abilityPowerBonus = abilityPowerBonus;
            item.criticalChance = criticalChance;
            item.criticalDamage = criticalDamage;
            item.moveSpeedBonus = moveSpeedBonus;
            item.dodgeChargesBonus = dodgeChargesBonus;
            item.goldOnKill = goldOnKill;
            item.dustOnKill = dustOnKill;
            item.lifeSteal = lifeSteal;
            item.thorns = thorns;
            item.regeneration = regeneration;
            item.attackSpeed = attackSpeed;
            item.memoryHaste = memoryHaste;
            item.autoAttack = autoAttack;
            item.autoAim = autoAim;
            if (elementalType >= 0)
                item.elementalType = (ElementalType)elementalType;
            item.elementalStacks = elementalStacks;
            item.requiredHero = string.IsNullOrEmpty(requiredHero) ? null : requiredHero;
            item.healPercentage = healPercentage;
            item.shieldPercentage = shieldPercentage;
            item.maxStack = maxStack;
            item.currentStack = currentStack;
            item.upgradeLevel = upgradeLevel;
            item.imagePath = imagePath;
            
            // Load sprite if we have inventory manager
            if (!string.IsNullOrEmpty(imagePath) && LocalInventoryManager != null)
            {
                item.sprite = LocalInventoryManager.LoadItemSprite(imagePath);
            }
            
            return item;
        }
        
        // Manual JSON serialization for C# 5 compatibility
        public string ToJson()
        {
            return string.Format(
                "{{\"id\":{0},\"name\":\"{1}\",\"description\":\"{2}\",\"type\":{3},\"rarity\":{4}," +
                "\"attackBonus\":{5},\"defenseBonus\":{6},\"healthBonus\":{7},\"abilityPowerBonus\":{8}," +
                "\"criticalChance\":{9},\"criticalDamage\":{10},\"moveSpeedBonus\":{11},\"dodgeChargesBonus\":{12}," +
                "\"goldOnKill\":{13},\"dustOnKill\":{14},\"lifeSteal\":{15},\"thorns\":{16},\"regeneration\":{17}," +
                "\"attackSpeed\":{18},\"memoryHaste\":{19},\"autoAttack\":{20},\"autoAim\":{21}," +
                "\"elementalType\":{22},\"elementalStacks\":{23},\"requiredHero\":\"{24}\"," +
                "\"healPercentage\":{25},\"shieldPercentage\":{26},\"maxStack\":{27},\"currentStack\":{28}," +
                "\"upgradeLevel\":{29},\"imagePath\":\"{30}\"}}",
                id, EscapeJson(name), EscapeJson(description), type, rarity,
                attackBonus, defenseBonus, healthBonus, abilityPowerBonus,
                criticalChance.ToString(System.Globalization.CultureInfo.InvariantCulture),
                criticalDamage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                moveSpeedBonus.ToString(System.Globalization.CultureInfo.InvariantCulture),
                dodgeChargesBonus, goldOnKill, dustOnKill,
                lifeSteal.ToString(System.Globalization.CultureInfo.InvariantCulture),
                thorns.ToString(System.Globalization.CultureInfo.InvariantCulture),
                regeneration.ToString(System.Globalization.CultureInfo.InvariantCulture),
                attackSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture),
                memoryHaste.ToString(System.Globalization.CultureInfo.InvariantCulture),
                autoAttack.ToString().ToLower(), autoAim.ToString().ToLower(),
                elementalType, elementalStacks, EscapeJson(requiredHero),
                healPercentage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                shieldPercentage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                maxStack, currentStack, upgradeLevel, EscapeJson(imagePath));
        }
        
        public static SerializableItem FromJson(string json)
        {
            SerializableItem item = new SerializableItem();
            try
            {
                item.id = GetIntValue(json, "id");
                item.name = GetStringValue(json, "name");
                item.description = GetStringValue(json, "description");
                item.type = GetIntValue(json, "type");
                item.rarity = GetIntValue(json, "rarity");
                item.attackBonus = GetIntValue(json, "attackBonus");
                item.defenseBonus = GetIntValue(json, "defenseBonus");
                item.healthBonus = GetIntValue(json, "healthBonus");
                item.abilityPowerBonus = GetIntValue(json, "abilityPowerBonus");
                item.criticalChance = GetFloatValue(json, "criticalChance");
                item.criticalDamage = GetFloatValue(json, "criticalDamage");
                item.moveSpeedBonus = GetFloatValue(json, "moveSpeedBonus");
                item.dodgeChargesBonus = GetIntValue(json, "dodgeChargesBonus");
                item.goldOnKill = GetIntValue(json, "goldOnKill");
                item.dustOnKill = GetIntValue(json, "dustOnKill");
                item.lifeSteal = GetFloatValue(json, "lifeSteal");
                item.thorns = GetFloatValue(json, "thorns");
                item.regeneration = GetFloatValue(json, "regeneration");
                item.attackSpeed = GetFloatValue(json, "attackSpeed");
                item.memoryHaste = GetFloatValue(json, "memoryHaste");
                item.autoAttack = GetBoolValue(json, "autoAttack");
                item.autoAim = GetBoolValue(json, "autoAim");
                item.elementalType = GetIntValue(json, "elementalType");
                item.elementalStacks = GetIntValue(json, "elementalStacks");
                item.requiredHero = GetStringValue(json, "requiredHero");
                item.healPercentage = GetFloatValue(json, "healPercentage");
                item.shieldPercentage = GetFloatValue(json, "shieldPercentage");
                item.maxStack = GetIntValue(json, "maxStack");
                item.currentStack = GetIntValue(json, "currentStack");
                item.upgradeLevel = GetIntValue(json, "upgradeLevel");
                item.imagePath = GetStringValue(json, "imagePath");
            }
            catch (Exception e)
            {
                RPGLog.Warning(" Error parsing item JSON: " + e.Message);
            }
            return item;
        }
        
        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
        
        private static int GetIntValue(string json, string key)
        {
            string pattern = "\"" + key + "\":";
            int idx = json.IndexOf(pattern);
            if (idx < 0) return 0;
            idx += pattern.Length;
            int endIdx = json.IndexOfAny(new char[] { ',', '}' }, idx);
            if (endIdx < 0) return 0;
            string val = json.Substring(idx, endIdx - idx).Trim();
            int result;
            int.TryParse(val, out result);
            return result;
        }
        
        private static float GetFloatValue(string json, string key)
        {
            string pattern = "\"" + key + "\":";
            int idx = json.IndexOf(pattern);
            if (idx < 0) return 0f;
            idx += pattern.Length;
            int endIdx = json.IndexOfAny(new char[] { ',', '}' }, idx);
            if (endIdx < 0) return 0f;
            string val = json.Substring(idx, endIdx - idx).Trim();
            float result;
            float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
            return result;
        }
        
        private static bool GetBoolValue(string json, string key)
        {
            string pattern = "\"" + key + "\":";
            int idx = json.IndexOf(pattern);
            if (idx < 0) return false;
            idx += pattern.Length;
            int endIdx = json.IndexOfAny(new char[] { ',', '}' }, idx);
            if (endIdx < 0) return false;
            string val = json.Substring(idx, endIdx - idx).Trim().ToLower();
            return val == "true";
        }
        
        private static string GetStringValue(string json, string key)
        {
            string pattern = "\"" + key + "\":\"";
            int idx = json.IndexOf(pattern);
            if (idx < 0) return "";
            idx += pattern.Length;
            int endIdx = idx;
            while (endIdx < json.Length)
            {
                if (json[endIdx] == '"' && (endIdx == 0 || json[endIdx - 1] != '\\'))
                    break;
                endIdx++;
            }
            if (endIdx >= json.Length) return "";
            string val = json.Substring(idx, endIdx - idx);
            return val.Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("\\n", "\n").Replace("\\r", "\r");
        }
    }
    
    #endregion
    
    private const string DATA_KEY_PREFIX = "RPGItems";
    
    private static uint _nextDropId = 1;
    private static Dictionary<uint, NetworkedDropData> _activeDrops = new Dictionary<uint, NetworkedDropData>();
    private static bool _isInitialized = false;
    private static bool _handlersRegistered = false;
    private static bool _serverActorWarningShown = false;
    private static string _modPath = "";
    
    // Callbacks
    public static Action<uint, RPGItem, Vector3> OnItemDroppedLocally;
    public static Action<uint> OnItemPickedUpLocally;
    public static Action<uint, float> OnDismantleProgressLocally;
    public static Action<uint, int> OnItemDismantledLocally;
    public static Action<RPGItem> OnItemReceivedLocally;
    
    public static InventoryManager LocalInventoryManager;
    
    public class NetworkedDropData
    {
        public uint dropId;
        public RPGItem item;
        public Vector3 position;
        public uint ownerNetId;
        public bool isShared;
        public float sharedTime;
        public float dismantleProgress;
        public float lastDismantleTapTime;
        public DroppedItem visualInstance;
        public bool isMonsterDrop; // True if dropped by monster, false if dropped by player
    }
    
    #region Initialization
    
    public static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        
        RegisterMessageHandlers();
        LoadPersistedItems();
        
        RPGLog.Debug(" NetworkedItemSystem initialized with new RPC API");
    }
    
    public static void SetModPath(string path)
    {
        _modPath = path;
    }
    
    private static void RegisterMessageHandlers()
    {
        if (_handlersRegistered) return;
        
        try
        {
            Actor serverActor = GetServerActor();
            if (serverActor == null)
            {
                if (!_serverActorWarningShown)
                {
                    RPGLog.Debug(" serverActor not available yet, will retry later");
                    _serverActorWarningShown = true;
                }
                return;
            }
            
            // Reset warning flag since we found the serverActor
            _serverActorWarningShown = false;
            
            // ALWAYS clear existing handlers first to prevent duplicates
            // This is important because the game ADDS handlers to a list, not replaces them
            // NOTE: The game's CustomRpc_UnregisterServerMessageHandler<T>() has a bug - it removes from
            // _clientRpcHandlers instead of _serverRpcHandlers. We must use the overload with handler delegate.
            try
            {
                serverActor.CustomRpc_UnregisterClientMessageHandler<ItemShareMessage>();
                serverActor.CustomRpc_UnregisterClientMessageHandler<ItemPickupMessage>();
                serverActor.CustomRpc_UnregisterClientMessageHandler<ItemDismantleMessage>();
                serverActor.CustomRpc_UnregisterClientMessageHandler<ItemShowoffMessage>();
                serverActor.CustomRpc_UnregisterClientMessageHandler<PrivateDropMessage>();
                
                // For server handlers, use the overload with handler delegate (game bug workaround)
                if (NetworkServer.active)
                {
                    serverActor.CustomRpc_UnregisterServerMessageHandler<ShareItemRequest>(OnShareItemRequestReceived);
                    serverActor.CustomRpc_UnregisterServerMessageHandler<PickupItemRequest>(OnPickupItemRequestReceived);
                    serverActor.CustomRpc_UnregisterServerMessageHandler<DismantleItemRequest>(OnDismantleItemRequestReceived);
                    serverActor.CustomRpc_UnregisterServerMessageHandler<ShowoffItemRequest>(OnShowoffItemRequestReceived);
                }
            }
            catch { }
            
            // Register client-side handlers (receive from server)
            serverActor.CustomRpc_RegisterClientMessageHandler<ItemShareMessage>(OnItemShareReceived);
            serverActor.CustomRpc_RegisterClientMessageHandler<ItemPickupMessage>(OnItemPickupReceived);
            serverActor.CustomRpc_RegisterClientMessageHandler<ItemDismantleMessage>(OnItemDismantleReceived);
            serverActor.CustomRpc_RegisterClientMessageHandler<ItemShowoffMessage>(OnItemShowoffReceived);
            serverActor.CustomRpc_RegisterClientMessageHandler<PrivateDropMessage>(OnPrivateDropReceived);
            
            // Register server-side handlers (receive from clients)
            // Use the overload with DewPlayer to get the actual sender
            if (NetworkServer.active)
            {
                serverActor.CustomRpc_RegisterServerMessageHandler<ShareItemRequest>("ShareItemRequest", OnShareItemRequestReceived);
                serverActor.CustomRpc_RegisterServerMessageHandler<PickupItemRequest>("PickupItemRequest", OnPickupItemRequestReceived);
                serverActor.CustomRpc_RegisterServerMessageHandler<DismantleItemRequest>("DismantleItemRequest", OnDismantleItemRequestReceived);
                serverActor.CustomRpc_RegisterServerMessageHandler<ShowoffItemRequest>("ShowoffItemRequest", OnShowoffItemRequestReceived);
            }
            
            // Subscribe to persistentSyncedData changes for late-join sync
            try { serverActor.persistentSyncedData.Callback -= OnPersistentDataChanged; } catch { }
            serverActor.persistentSyncedData.Callback += OnPersistentDataChanged;
            
            _handlersRegistered = true;
            RPGLog.Debug(" Registered RPC message handlers on serverActor");
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to register message handlers: " + e.Message);
        }
    }
    
    private static void LoadPersistedItems()
    {
        try
        {
            Actor serverActor = GetServerActor();
            if (serverActor == null) return;
            
            // Load all shared items from persistentSyncedData
            // Create snapshot to avoid modification during iteration (defensive programming)
            List<KeyValuePair<string, string>> snapshot = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, string> kvp in serverActor.persistentSyncedData)
            {
                if (kvp.Key != null && kvp.Key.StartsWith(DATA_KEY_PREFIX + "::SharedItem::"))
                {
                    snapshot.Add(kvp);
                }
            }
            
            // Process snapshot (safe to modify dictionary during this)
            foreach (KeyValuePair<string, string> kvp in snapshot)
                {
                    ProcessPersistedItem(kvp.Key, kvp.Value);
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Failed to load persisted items: " + e.Message);
        }
    }
    
    private static void ProcessPersistedItem(string key, string value)
    {
        try
        {
            // Key format: RPGItems::SharedItem::<dropId>
            string[] parts = key.Split(new string[] { "::" }, StringSplitOptions.None);
            if (parts.Length < 3) return;
            
            uint dropId;
            if (!uint.TryParse(parts[2], out dropId)) return;
            
            // Don't duplicate (thread-safe check)
            if (_activeDrops.ContainsKey(dropId)) return;
            
            // Parse the stored data - value is the JSON of ItemShareMessage
            ItemShareMessage msg = ParseItemShareMessage(value);
            if (msg == null) return;
            
            // Don't create our own items
            if (msg.ownerNetId == GetLocalPlayerNetId()) return;
            
            SerializableItem sItem = SerializableItem.FromJson(msg.itemJson);
            RPGItem item = sItem.ToRPGItem();
            Vector3 position = new Vector3(msg.posX, msg.posY, msg.posZ);
            
            // Shared items should NOT auto-pickup (player should choose to pick them up)
            CreateLocalDrop(item, position, msg.ownerNetId, dropId, true, false); // isMonsterDrop = false
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing persisted item: " + e.Message);
        }
    }
    
    private static ItemShareMessage ParseItemShareMessage(string json)
    {
        try
        {
            ItemShareMessage msg = new ItemShareMessage();
            msg.dropId = (uint)GetIntValueFromJson(json, "dropId");
            msg.itemJson = GetStringValueFromJson(json, "itemJson");
            msg.posX = GetFloatValueFromJson(json, "posX");
            msg.posY = GetFloatValueFromJson(json, "posY");
            msg.posZ = GetFloatValueFromJson(json, "posZ");
            msg.ownerNetId = (uint)GetIntValueFromJson(json, "ownerNetId");
            return msg;
        }
        catch
        {
            return null;
        }
    }
    
    private static string SerializeItemShareMessage(ItemShareMessage msg)
    {
        return string.Format("{{\"dropId\":{0},\"itemJson\":\"{1}\",\"posX\":{2},\"posY\":{3},\"posZ\":{4},\"ownerNetId\":{5}}}",
            msg.dropId,
            EscapeJsonString(msg.itemJson),
            msg.posX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            msg.posY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            msg.posZ.ToString(System.Globalization.CultureInfo.InvariantCulture),
            msg.ownerNetId);
    }
    
    private static string EscapeJsonString(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
    
    private static int GetIntValueFromJson(string json, string key)
    {
        string pattern = "\"" + key + "\":";
        int idx = json.IndexOf(pattern);
        if (idx < 0) return 0;
        idx += pattern.Length;
        int endIdx = json.IndexOfAny(new char[] { ',', '}' }, idx);
        if (endIdx < 0) return 0;
        string val = json.Substring(idx, endIdx - idx).Trim();
        int result;
        int.TryParse(val, out result);
        return result;
    }
    
    private static float GetFloatValueFromJson(string json, string key)
    {
        string pattern = "\"" + key + "\":";
        int idx = json.IndexOf(pattern);
        if (idx < 0) return 0f;
        idx += pattern.Length;
        int endIdx = json.IndexOfAny(new char[] { ',', '}' }, idx);
        if (endIdx < 0) return 0f;
        string val = json.Substring(idx, endIdx - idx).Trim();
        float result;
        float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
        return result;
    }
    
    private static string GetStringValueFromJson(string json, string key)
    {
        string pattern = "\"" + key + "\":\"";
        int idx = json.IndexOf(pattern);
        if (idx < 0) return "";
        idx += pattern.Length;
        int endIdx = idx;
        while (endIdx < json.Length)
        {
            if (json[endIdx] == '"' && (endIdx == 0 || json[endIdx - 1] != '\\'))
                break;
            endIdx++;
        }
        if (endIdx >= json.Length) return "";
        string val = json.Substring(idx, endIdx - idx);
        return val.Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("\\n", "\n").Replace("\\r", "\r");
    }
    
    public static void Cleanup()
    {
        try
        {
            UnregisterMessageHandlers();
        }
        catch { }
        
        _activeDrops.Clear();
        _nextDropId = 1;
        _isInitialized = false;
        _serverActorWarningShown = false;
    }
    
    /// <summary>
    /// Called when ActorManager starts (new game session)
    /// This ensures we register handlers on the NEW serverActor
    /// </summary>
    public static void OnActorManagerStarted()
    {
        RPGLog.Debug(" NetworkedItemSystem: ActorManager started, registering handlers");
        
        // Reset handler state - we need to register on the NEW serverActor
        _handlersRegistered = false;
        
        // Register handlers on the new serverActor
        RegisterMessageHandlers();
        LoadPersistedItems();
    }
    
    /// <summary>
    /// Called when ActorManager stops (game session ends)
    /// </summary>
    public static void OnActorManagerStopped()
    {
        RPGLog.Debug(" NetworkedItemSystem: ActorManager stopped, unregistering handlers");
        UnregisterMessageHandlers();
    }
    
    private static void UnregisterMessageHandlers()
    {
        if (!_handlersRegistered) return;
        
        try
        {
            Actor serverActor = GetServerActor();
            if (serverActor != null)
            {
                serverActor.CustomRpc_UnregisterClientMessageHandler<ItemShareMessage>();
                serverActor.CustomRpc_UnregisterClientMessageHandler<ItemPickupMessage>();
                serverActor.CustomRpc_UnregisterClientMessageHandler<ItemDismantleMessage>();
                serverActor.CustomRpc_UnregisterClientMessageHandler<ItemShowoffMessage>();
                serverActor.CustomRpc_UnregisterClientMessageHandler<PrivateDropMessage>();
                
                if (NetworkServer.active)
                {
                    serverActor.CustomRpc_UnregisterServerMessageHandler<ShareItemRequest>();
                    serverActor.CustomRpc_UnregisterServerMessageHandler<PickupItemRequest>();
                    serverActor.CustomRpc_UnregisterServerMessageHandler<DismantleItemRequest>();
                    serverActor.CustomRpc_UnregisterServerMessageHandler<ShowoffItemRequest>();
                }
                
                try { serverActor.persistentSyncedData.Callback -= OnPersistentDataChanged; } catch { }
            }
        }
        catch { }
        
        _handlersRegistered = false;
    }
    
    public static void ProcessIncomingMessages()
    {
        // Re-register handlers if needed (in case serverActor was recreated)
        if (!_handlersRegistered && _isInitialized)
        {
            RegisterMessageHandlers();
            LoadPersistedItems();
        }
    }
    
    // Legacy method for compatibility
    public static void RegisterServerHandlers() { }
    
    #endregion
    
    #region Server-Side Message Handlers
    
    private static void OnShareItemRequestReceived(ShareItemRequest request, DewPlayer sender)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Get the actual sender's netId (not the host's!)
            uint ownerNetId = 0;
            string senderName = "Unknown";
            if (sender != null && sender.hero != null)
            {
                ownerNetId = sender.hero.netId;
                senderName = sender.playerName;
            }
            
            // Create the share message
            ItemShareMessage msg = new ItemShareMessage();
            msg.dropId = request.dropId;
            msg.itemJson = request.itemJson;
            msg.posX = request.posX;
            msg.posY = request.posY;
            msg.posZ = request.posZ;
            msg.ownerNetId = ownerNetId;
            
            // Store in persistentSyncedData for late-joiners
            Actor serverActor = GetServerActor();
            if (serverActor != null)
            {
                string dataKey = DATA_KEY_PREFIX + "::SharedItem::" + request.dropId.ToString();
                serverActor.persistentSyncedData[dataKey] = SerializeItemShareMessage(msg);
                
                // Broadcast to all clients EXCEPT the sender (they already showed the message locally)
                foreach (DewPlayer player in DewPlayer.allHumanPlayers)
                {
                    if (player != sender)
                    {
                        // For remote clients, send via RPC
                        if (player != DewPlayer.local)
                        {
                            serverActor.CustomRpc_SendMessageToClient(player, msg);
                        }
                        else
                        {
                            // For the HOST, process locally since we ARE the server
                            // The RPC won't trigger our own client handler
                            OnItemShareReceived(msg);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Server error handling share request: " + e.Message);
        }
    }
    
    private static void OnPickupItemRequestReceived(PickupItemRequest request, DewPlayer sender)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Get the actual sender's netId
            uint pickerNetId = 0;
            if (sender != null && sender.hero != null)
            {
                pickerNetId = sender.hero.netId;
            }
            
            // Remove from persistentSyncedData
            Actor serverActor = GetServerActor();
            if (serverActor != null)
            {
                string dataKey = DATA_KEY_PREFIX + "::SharedItem::" + request.dropId.ToString();
                if (serverActor.persistentSyncedData.ContainsKey(dataKey))
                {
                    serverActor.persistentSyncedData.Remove(dataKey);
                }
                
                // Broadcast pickup to all clients
                ItemPickupMessage msg = new ItemPickupMessage();
                msg.dropId = request.dropId;
                msg.pickerNetId = pickerNetId;
                serverActor.CustomRpc_SendMessageToAllClients(msg);
            }
            
            RPGLog.Debug(" Server: Item " + request.dropId + " picked up by " + (sender != null ? sender.playerName : "unknown"));
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Server error handling pickup request: " + e.Message);
        }
    }
    
    private static void OnDismantleItemRequestReceived(DismantleItemRequest request, DewPlayer sender)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Remove from persistentSyncedData
            Actor serverActor = GetServerActor();
            if (serverActor != null)
            {
                string dataKey = DATA_KEY_PREFIX + "::SharedItem::" + request.dropId.ToString();
                if (serverActor.persistentSyncedData.ContainsKey(dataKey))
                {
                    serverActor.persistentSyncedData.Remove(dataKey);
                }
                
                // Calculate dust amount (we need item data for this)
                int dustAmount = 10; // Default
                
                // Broadcast dismantle to all clients
                ItemDismantleMessage msg = new ItemDismantleMessage();
                msg.dropId = request.dropId;
                msg.dustAmount = dustAmount;
                serverActor.CustomRpc_SendMessageToAllClients(msg);
            }
            
            RPGLog.Debug(" Server: Item " + request.dropId + " dismantled by " + (sender != null ? sender.playerName : "unknown"));
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Server error handling dismantle request: " + e.Message);
        }
    }
    
    private static void OnShowoffItemRequestReceived(ShowoffItemRequest request, DewPlayer sender)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Get the actual sender's info
            uint ownerNetId = 0;
            string playerName = Localization.Unknown;
            if (sender != null)
            {
                if (sender.hero != null)
                {
                    ownerNetId = sender.hero.netId;
                }
                playerName = sender.playerName;
            }
            
            // Broadcast showoff to all clients EXCEPT the sender (they already showed locally)
            ItemShowoffMessage msg = new ItemShowoffMessage();
            msg.itemJson = request.itemJson;
            msg.playerName = playerName;
            msg.ownerNetId = ownerNetId;
            
            Actor serverActor = GetServerActor();
            if (serverActor != null)
            {
                // Send to each player except the original sender
                foreach (DewPlayer player in DewPlayer.allHumanPlayers)
                {
                    if (player != sender)
                    {
                        serverActor.CustomRpc_SendMessageToClient(player, msg);
                    }
                }
            }
            
            RPGLog.Debug(" Server: Item showoff from " + playerName);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Server error handling showoff request: " + e.Message);
        }
    }
    
    #endregion
    
    #region Client-Side Message Handlers
    
    private static void OnItemShareReceived(ItemShareMessage msg)
    {
        try
        {
            // Don't process our own shares
            if (msg.ownerNetId == GetLocalPlayerNetId()) return;
            
            // Don't duplicate (thread-safe check)
            if (_activeDrops.ContainsKey(msg.dropId)) return;
            
            SerializableItem sItem = SerializableItem.FromJson(msg.itemJson);
            RPGItem item = sItem.ToRPGItem();
            Vector3 position = new Vector3(msg.posX, msg.posY, msg.posZ);
            
            // Shared items should NOT auto-pickup (player should choose to pick them up)
            // This includes both monster shared drops (ownerNetId = 0) and player-shared items
            CreateLocalDrop(item, position, msg.ownerNetId, msg.dropId, true, false); // isMonsterDrop = false
            
            // Show ping message for player-shared items (not monster world drops)
            if (msg.ownerNetId != 0)
            {
                ShowReceivedSharePingMessage(item, msg.ownerNetId);
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Client error handling share message: " + e.Message);
        }
    }
    
    private static void OnItemPickupReceived(ItemPickupMessage msg)
    {
        try
        {
            // Thread-safe removal (already uses ContainsKey check)
            RemoveLocalDropSilent(msg.dropId);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Client error handling pickup message: " + e.Message);
        }
    }
    
    private static void OnItemDismantleReceived(ItemDismantleMessage msg)
    {
        try
        {
            // Thread-safe removal (already uses ContainsKey check)
            RemoveLocalDropSilent(msg.dropId);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Client error handling dismantle message: " + e.Message);
        }
    }
    
    private static void OnItemShowoffReceived(ItemShowoffMessage msg)
    {
        try
        {
            // Don't show our own showoffs
            if (msg.ownerNetId == GetLocalPlayerNetId()) return;
            
            SerializableItem sItem = SerializableItem.FromJson(msg.itemJson);
            RPGItem item = sItem.ToRPGItem();
            
            ShowReceivedShowoffMessage(item, msg.ownerNetId, msg.playerName);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Client error handling showoff message: " + e.Message);
        }
    }
    
    private static void OnPrivateDropReceived(PrivateDropMessage msg)
    {
        try
        {
            // Special case: dropId = 0xFFFFFFFF means "add directly to inventory"
            // This is used for merchant purchases
            if (msg.dropId == 0xFFFFFFFF)
            {
                if (string.IsNullOrEmpty(msg.itemJson))
                {
                    RPGLog.Error(" Received empty itemJson for purchase!");
                    return;
                }
                
                SerializableItem sItem = SerializableItem.FromJson(msg.itemJson);
                if (sItem == null)
                {
                    RPGLog.Error(" Failed to deserialize purchased item!");
                    return;
                }
                
                RPGItem item = sItem.ToRPGItem();
                if (item == null)
                {
                    RPGLog.Error(" Failed to convert to RPGItem!");
                    return;
                }
                
                // Load sprite
                ItemDatabase.EnsureSprite(item);
                
                // Add directly to local player's inventory
                // For consumables, use the OnItemReceivedLocally callback which handles fast slot stacking
                bool added = false;
                
                if (item.type == ItemType.Consumable && OnItemReceivedLocally != null)
                {
                    // Use callback which handles consumable auto-stacking to fast slots
                    OnItemReceivedLocally(item);
                    added = true;
                }
                else if (LocalInventoryManager != null)
                {
                    LocalInventoryManager.AddItem(item);
                    added = true;
                }
                else
                {
                    // Fallback: try to find inventory manager
                    RPGItemsMod mod = UnityEngine.Object.FindFirstObjectByType<RPGItemsMod>();
                    if (mod != null)
                    {
                        InventoryManager invManager = mod.GetInventoryManager();
                        if (invManager != null)
                        {
                            invManager.AddItem(item);
                            added = true;
                        }
                    }
                }
                
                if (!added)
                {
                    RPGLog.Error(" Could not find inventory manager to add purchased item!");
                }
                
                return;
            }
            
            // Normal private drop - create in world (this is a monster drop)
            // Don't duplicate (thread-safe check)
            if (_activeDrops.ContainsKey(msg.dropId)) return;
            
            SerializableItem sItem2 = SerializableItem.FromJson(msg.itemJson);
            RPGItem item2 = sItem2.ToRPGItem();
            Vector3 position = new Vector3(msg.posX, msg.posY, msg.posZ);
            
            uint localNetId = GetLocalPlayerNetId();
            CreateLocalDrop(item2, position, localNetId, msg.dropId, false, true); // isMonsterDrop = true
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Client error handling private drop: " + e.Message);
        }
    }
    
    private static void OnPersistentDataChanged(SyncIDictionary<string, string>.Operation op, string key, string value)
    {
        try
        {
            // Handle OP_CLEAR - key and value may be null
            if (op == SyncIDictionary<string, string>.Operation.OP_CLEAR)
            {
                // Clear all our drops when dictionary is cleared (save/load)
                RPGLog.Debug(" persistentSyncedData cleared, cleaning up drops");
                return;
            }
            
            // For other operations, key must be valid
            if (string.IsNullOrEmpty(key)) return;
            if (!key.StartsWith(DATA_KEY_PREFIX + "::SharedItem::")) return;
            
            if (op == SyncIDictionary<string, string>.Operation.OP_ADD ||
                op == SyncIDictionary<string, string>.Operation.OP_SET)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    ProcessPersistedItem(key, value);
                }
            }
            else if (op == SyncIDictionary<string, string>.Operation.OP_REMOVE)
            {
                // Extract dropId and remove
                string[] parts = key.Split(new string[] { "::" }, StringSplitOptions.None);
                if (parts.Length >= 3)
                {
                    uint dropId;
                    if (uint.TryParse(parts[2], out dropId))
                    {
                        RemoveLocalDropSilent(dropId);
                    }
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error in OnPersistentDataChanged: " + e.Message);
        }
    }
    
    #endregion
    
    #region Public API
    
    public static void DropItem(RPGItem item, Vector3 position)
    {
        if (item == null) return;
        
        uint ownerNetId = GetLocalPlayerNetId();
        uint dropId = GenerateDropId();
        
        CreateLocalDrop(item, position, ownerNetId, dropId, false);
        RPGLog.Debug(" Dropped item (private): " + item.name + " (ID: " + dropId + ")");
    }
    
    private static uint GenerateDropId()
    {
        uint localNetId = GetLocalPlayerNetId();
        // NOTE: _nextDropId++ is not thread-safe, but Unity runs on single main thread
        // For true thread safety, would need Interlocked.Increment, but not necessary here
        // The localNetId prefix ensures uniqueness across different players
        uint dropId = (_nextDropId++) | ((localNetId & 0xFF) << 24);
        
        // Safety check: prevent overflow (unlikely but possible)
        if (_nextDropId >= uint.MaxValue - 1000)
        {
            _nextDropId = 1; // Reset to prevent overflow
            RPGLog.Warning(" Drop ID counter reset to prevent overflow");
        }
        
        return dropId;
    }
    
    public static void ShareItem(uint dropId)
    {
        // Use TryGetValue to avoid race condition
        NetworkedDropData dropData;
        if (!_activeDrops.TryGetValue(dropId, out dropData)) return;
        
        uint localNetId = GetLocalPlayerNetId();
        
        if (dropData.ownerNetId != localNetId) return;
        if (dropData.isShared) return;
        
        dropData.isShared = true;
        dropData.sharedTime = Time.time;
        
        if (dropData.visualInstance != null)
        {
            dropData.visualInstance.ShowSharedMessage();
        }
        
        ShowSharePingMessage(dropData.item);
        
        // Create share request
        SerializableItem sItem = SerializableItem.FromRPGItem(dropData.item);
        ShareItemRequest request = new ShareItemRequest();
        request.dropId = dropId;
        request.itemJson = sItem.ToJson();
        request.posX = dropData.position.x;
        request.posY = dropData.position.y;
        request.posZ = dropData.position.z;
        
        Actor serverActor = GetServerActor();
        if (serverActor != null)
        {
            // Store reference locally to avoid race condition
            Actor localServerActor = serverActor;
            
            if (NetworkServer.active)
            {
                // Host: Handle directly
                ItemShareMessage msg = new ItemShareMessage();
                msg.dropId = dropId;
                msg.itemJson = request.itemJson;
                msg.posX = request.posX;
                msg.posY = request.posY;
                msg.posZ = request.posZ;
                msg.ownerNetId = localNetId;
                
                // Store in persistentSyncedData (use local reference)
                string dataKey = DATA_KEY_PREFIX + "::SharedItem::" + dropId.ToString();
                localServerActor.persistentSyncedData[dataKey] = SerializeItemShareMessage(msg);
                
                // Broadcast to OTHER clients only (we already showed locally)
                // Use local reference to avoid race condition
                foreach (DewPlayer player in DewPlayer.allHumanPlayers)
                {
                    if (player != DewPlayer.local)
                    {
                        localServerActor.CustomRpc_SendMessageToClient(player, msg);
                    }
                }
            }
            else
            {
                // Client: Send request to server
                serverActor.CustomRpc_SendMessageToServer(request);
            }
        }
    }
    
    public static void RequestPickup(uint dropId)
    {
        // Use TryGetValue to avoid race condition
        NetworkedDropData dropData;
        if (!_activeDrops.TryGetValue(dropId, out dropData)) return;
        
        if (OnItemReceivedLocally != null)
        {
            OnItemReceivedLocally(dropData.item);
        }
        
        // Send pickup request
        Actor serverActor = GetServerActor();
        if (serverActor != null)
        {
            if (NetworkServer.active)
            {
                // Host: Handle directly
                string dataKey = DATA_KEY_PREFIX + "::SharedItem::" + dropId.ToString();
                if (serverActor.persistentSyncedData.ContainsKey(dataKey))
                {
                    serverActor.persistentSyncedData.Remove(dataKey);
                }
                
                ItemPickupMessage msg = new ItemPickupMessage();
                msg.dropId = dropId;
                msg.pickerNetId = GetLocalPlayerNetId();
                serverActor.CustomRpc_SendMessageToAllClients(msg);
            }
            else
            {
                // Client: Send request
                PickupItemRequest request = new PickupItemRequest();
                request.dropId = dropId;
                serverActor.CustomRpc_SendMessageToServer(request);
            }
        }
        
        RemoveLocalDrop(dropId);
        
        if (OnItemPickedUpLocally != null)
        {
            OnItemPickedUpLocally(dropId);
        }
    }
    
    public static void RequestDismantleTap(uint dropId)
    {
        // Use TryGetValue to avoid race condition
        NetworkedDropData dropData;
        if (!_activeDrops.TryGetValue(dropId, out dropData)) return;
        
        float now = Time.time;
        if (now - dropData.lastDismantleTapTime < 0.075f) return;
        
        dropData.lastDismantleTapTime = now;
        dropData.dismantleProgress += 0.4f;
        
        if (OnDismantleProgressLocally != null)
        {
            OnDismantleProgressLocally(dropId, dropData.dismantleProgress);
        }
        
        if (dropData.dismantleProgress >= 1f)
        {
            int dustAmount = CalculateDustAmount(dropData.item);
            SpawnDustReward(dustAmount);
            
            // Send dismantle request
            Actor serverActor = GetServerActor();
            if (serverActor != null)
            {
                if (NetworkServer.active)
                {
                    // Host: Handle directly
                    string dataKey = DATA_KEY_PREFIX + "::SharedItem::" + dropId.ToString();
                    if (serverActor.persistentSyncedData.ContainsKey(dataKey))
                    {
                        serverActor.persistentSyncedData.Remove(dataKey);
                    }
                    
                    ItemDismantleMessage msg = new ItemDismantleMessage();
                    msg.dropId = dropId;
                    msg.dustAmount = dustAmount;
                    serverActor.CustomRpc_SendMessageToAllClients(msg);
                }
                else
                {
                    // Client: Send request
                    DismantleItemRequest request = new DismantleItemRequest();
                    request.dropId = dropId;
                    serverActor.CustomRpc_SendMessageToServer(request);
                }
            }
            
            RemoveLocalDrop(dropId);
            
            if (OnItemDismantledLocally != null)
            {
                OnItemDismantledLocally(dropId, dustAmount);
            }
        }
    }
    
    public static void CompleteDismantle(uint dropId, int dustAmount)
    {
        _activeDrops.Remove(dropId);
    }
    
    /// <summary>
    /// Instantly dismantle an item (Ctrl+key shortcut - skips hold-to-confirm)
    /// </summary>
    public static void RequestInstantDismantle(uint dropId)
    {
        // Use TryGetValue to avoid race condition
        NetworkedDropData dropData;
        if (!_activeDrops.TryGetValue(dropId, out dropData)) return;
        
        // Calculate dust and award it
        int dustAmount = CalculateDustAmount(dropData.item);
        SpawnDustReward(dustAmount);
        
        // Send dismantle request
        Actor serverActor = GetServerActor();
        if (serverActor != null)
        {
            if (NetworkServer.active)
            {
                // Host: Handle directly
                string dataKey = DATA_KEY_PREFIX + "::SharedItem::" + dropId.ToString();
                if (serverActor.persistentSyncedData.ContainsKey(dataKey))
                {
                    serverActor.persistentSyncedData.Remove(dataKey);
                }
                
                ItemDismantleMessage msg = new ItemDismantleMessage();
                msg.dropId = dropId;
                msg.dustAmount = dustAmount;
                serverActor.CustomRpc_SendMessageToAllClients(msg);
            }
            else
            {
                // Client: Send request
                DismantleItemRequest request = new DismantleItemRequest();
                request.dropId = dropId;
                serverActor.CustomRpc_SendMessageToServer(request);
            }
        }
        
        RemoveLocalDrop(dropId);
        
        if (OnItemDismantledLocally != null)
        {
            OnItemDismantledLocally(dropId, dustAmount);
        }
        
        RPGLog.Debug(" Instant dismantle completed for drop " + dropId);
    }
    
    public static NetworkedDropData GetDrop(uint dropId)
    {
        // Thread-safe access using TryGetValue
        NetworkedDropData data;
        _activeDrops.TryGetValue(dropId, out data);
        return data;
    }
    
    public static Dictionary<uint, NetworkedDropData> GetAllDrops()
    {
        // Return a copy to prevent external modification (thread safety)
        // NOTE: In Unity, this is usually safe since main thread only, but defensive programming
        return new Dictionary<uint, NetworkedDropData>(_activeDrops);
    }
    
    /// <summary>
    /// Create a shared drop (server-side, for monster loot).
    /// This immediately shares the item with all players.
    /// </summary>
    public static void CreateSharedDrop(RPGItem item, Vector3 position, uint dropId)
    {
        if (item == null) return;
        
        uint ownerNetId = 0; // World drop
        
        CreateLocalDrop(item, position, ownerNetId, dropId, true, false); // isMonsterDrop = false (shared drops should NOT auto-pickup)
        
        // Use TryGetValue for thread safety
        NetworkedDropData dropData;
        if (_activeDrops.TryGetValue(dropId, out dropData))
        {
            dropData.isShared = true;
            dropData.sharedTime = Time.time;
        }
        
        // Store and broadcast
        SerializableItem sItem = SerializableItem.FromRPGItem(item);
        ItemShareMessage msg = new ItemShareMessage();
        msg.dropId = dropId;
        msg.itemJson = sItem.ToJson();
        msg.posX = position.x;
        msg.posY = position.y;
        msg.posZ = position.z;
        msg.ownerNetId = ownerNetId;
        
        Actor serverActor = GetServerActor();
        if (serverActor != null)
        {
            string dataKey = DATA_KEY_PREFIX + "::SharedItem::" + dropId.ToString();
            serverActor.persistentSyncedData[dataKey] = SerializeItemShareMessage(msg);
            serverActor.CustomRpc_SendMessageToAllClients(msg);
        }
        
        RPGLog.Debug(" Created shared monster drop: " + item.name + " (ID: " + dropId + ")");
    }
    
    /// <summary>
    /// Create a private drop for a specific player (server-side, for monster loot).
    /// Only the target player will see this drop until they share it.
    /// </summary>
    public static void CreatePrivateDropForPlayer(RPGItem item, Vector3 position, uint dropId, uint targetPlayerNetId)
    {
        if (item == null) return;
        
        SerializableItem sItem = SerializableItem.FromRPGItem(item);
        PrivateDropMessage msg = new PrivateDropMessage();
        msg.dropId = dropId;
        msg.itemJson = sItem.ToJson();
        msg.posX = position.x;
        msg.posY = position.y;
        msg.posZ = position.z;
        
        // Find the target player and send directly to them
        Actor serverActor = GetServerActor();
        if (serverActor != null)
        {
            DewPlayer targetPlayer = GetPlayerByHeroNetId(targetPlayerNetId);
            if (targetPlayer != null)
            {
                serverActor.CustomRpc_SendMessageToClient(targetPlayer, msg);
            }
            else
            {
                RPGLog.Warning(" Could not find player with netId " + targetPlayerNetId);
            }
        }
    }
    
    // Legacy method for compatibility
    public static void ProcessPrivateDrops() { }
    
    /// <summary>
    /// Show off item in chat (from inventory middle-click)
    /// </summary>
    public static void ShareItemInChat(RPGItem item)
    {
        if (item == null) return;
        
        // Show local message immediately (only for the sender)
        ShowShowoffPingMessage(item);
        
        SerializableItem sItem = SerializableItem.FromRPGItem(item);
        ShowoffItemRequest request = new ShowoffItemRequest();
        request.itemJson = sItem.ToJson();
        
        Actor serverActor = GetServerActor();
        if (serverActor != null)
        {
            if (NetworkServer.active)
            {
                // Host: Broadcast to OTHER clients only (we already showed locally)
                ItemShowoffMessage msg = new ItemShowoffMessage();
                msg.itemJson = request.itemJson;
                msg.playerName = DewPlayer.local != null ? DewPlayer.local.playerName : "Unknown";
                msg.ownerNetId = GetLocalPlayerNetId();
                
                // Send to each client EXCEPT ourselves
                foreach (DewPlayer player in DewPlayer.allHumanPlayers)
                {
                    if (player != DewPlayer.local)
                    {
                        serverActor.CustomRpc_SendMessageToClient(player, msg);
                    }
                }
            }
            else
            {
                // Client: Send request to server (server will broadcast to others)
                serverActor.CustomRpc_SendMessageToServer(request);
            }
        }
    }
    
    // Legacy method for chat message processing (no longer needed with RPC)
    public static void ProcessChatMessage(string content) { }
    
    #endregion
    
    #region Chat Ping Messages
    
    private static void ShowSharePingMessage(RPGItem item)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null || DewPlayer.local == null) return;
            
            string rarityColor = GetRarityColorHex(item.rarity);
            string playerName = DewPlayer.local.playerName;
            string playerColor = ChatManager.GetPlayerColorHex(DewPlayer.local);
            
            string sharedText = Localization.CurrentLanguage == ModLanguage.Chinese ? "分享了" : "shared";
            string content = string.Format("<color=#e4edf0><color={0}>{1}</color> {2} <color={3}>[{4}]</color></color>",
                playerColor, playerName, sharedText, rarityColor, item.GetDisplayName());
            
            ChatManager.Message msg = new ChatManager.Message();
            msg.type = ChatManager.MessageType.Raw;
            msg.content = content;
            msg.itemType = "RPGItem_" + item.type.ToString();
            msg.itemCustomData = SerializeItemForTooltip(item);
            
            chatManager.ShowMessageLocally(msg);
        }
        catch { }
    }
    
    private static void ShowReceivedSharePingMessage(RPGItem item, uint ownerNetId)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            string playerName = "Unknown";
            string playerColor = "#70d4ff";
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == ownerNetId)
                {
                    playerName = player.playerName;
                    playerColor = ChatManager.GetPlayerColorHex(player);
                    break;
                }
            }
            
            string rarityColor = GetRarityColorHex(item.rarity);
            
            string sharedText = Localization.CurrentLanguage == ModLanguage.Chinese ? "分享了" : "shared";
            string content = string.Format("<color=#e4edf0><color={0}>{1}</color> {2} <color={3}>[{4}]</color></color>",
                playerColor, playerName, sharedText, rarityColor, item.GetDisplayName());
            
            ChatManager.Message msg = new ChatManager.Message();
            msg.type = ChatManager.MessageType.Raw;
            msg.content = content;
            msg.itemType = "RPGItem_" + item.type.ToString();
            msg.itemCustomData = SerializeItemForTooltip(item);
            
            chatManager.ShowMessageLocally(msg);
        }
        catch { }
    }
    
    private static void ShowShowoffPingMessage(RPGItem item)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null || DewPlayer.local == null) return;
            
            string rarityColor = GetRarityColorHex(item.rarity);
            string playerName = DewPlayer.local.playerName;
            string playerColor = ChatManager.GetPlayerColorHex(DewPlayer.local);
            
            string showsOffText = Localization.CurrentLanguage == ModLanguage.Chinese ? "展示了" : "shows off";
            string content = string.Format("<color=#e4edf0><color={0}>{1}</color> {2} <color={3}>[{4}]</color></color>",
                playerColor, playerName, showsOffText, rarityColor, item.GetDisplayName());
            
            ChatManager.Message msg = new ChatManager.Message();
            msg.type = ChatManager.MessageType.Raw;
            msg.content = content;
            msg.itemType = "RPGItem_" + item.type.ToString();
            msg.itemCustomData = SerializeItemForTooltip(item);
            
            chatManager.ShowMessageLocally(msg);
        }
        catch { }
    }
    
    private static void ShowReceivedShowoffMessage(RPGItem item, uint ownerNetId, string fallbackPlayerName)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            if (ownerNetId == GetLocalPlayerNetId()) return;
            
            string playerName = fallbackPlayerName != null ? fallbackPlayerName : "Unknown";
            string playerColor = "#70d4ff";
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == ownerNetId)
                {
                    playerName = player.playerName;
                    playerColor = ChatManager.GetPlayerColorHex(player);
                    break;
                }
            }
            
            if (fallbackPlayerName != null && playerName == fallbackPlayerName)
            {
                foreach (DewPlayer player in DewPlayer.allHumanPlayers)
                {
                    if (player != null && player.playerName == fallbackPlayerName)
                    {
                        playerColor = ChatManager.GetPlayerColorHex(player);
                        break;
                    }
                }
            }
            
            string rarityColor = GetRarityColorHex(item.rarity);
            
            string showsOffText = Localization.CurrentLanguage == ModLanguage.Chinese ? "展示了" : "shows off";
            string content = string.Format("<color=#e4edf0><color={0}>{1}</color> {2} <color={3}>[{4}]</color></color>",
                playerColor, playerName, showsOffText, rarityColor, item.GetDisplayName());
            
            ChatManager.Message msg = new ChatManager.Message();
            msg.type = ChatManager.MessageType.Raw;
            msg.content = content;
            msg.itemType = "RPGItem_" + item.type.ToString();
            msg.itemCustomData = SerializeItemForTooltip(item);
            
            chatManager.ShowMessageLocally(msg);
        }
        catch { }
    }
    
    public static string SerializeItemForTooltip(RPGItem item)
    {
        int elemType = item.elementalType.HasValue ? ((int)item.elementalType.Value + 1) : 0;
        string desc = item.description != null ? item.description.Replace("|", "/") : "";
        string reqHero = item.requiredHero != null ? item.requiredHero.Replace("|", "/") : "";
        return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}",
            item.name,
            (int)item.type,
            (int)item.rarity,
            item.attackBonus,
            item.defenseBonus,
            item.healthBonus,
            item.healPercentage,
            item.moveSpeedBonus,
            item.dodgeChargesBonus,
            item.goldOnKill,
            item.upgradeLevel,
            desc,
            item.abilityPowerBonus,
            item.criticalChance,
            item.criticalDamage,
            item.thorns,
            item.lifeSteal,
            item.dustOnKill,
            item.regeneration,
            item.autoAttack ? 1 : 0,
            item.autoAim ? 1 : 0,
            item.attackSpeed,
            item.memoryHaste,
            elemType,
            item.elementalStacks,
            item.shieldPercentage,
            reqHero);
    }
    
    public static RPGItem DeserializeItemFromTooltip(string data)
    {
        try
        {
            string[] parts = data.Split('|');
            if (parts.Length < 9) return null;
            
            string name = parts[0];
            ItemType type = (ItemType)int.Parse(parts[1]);
            ItemRarity rarity = (ItemRarity)int.Parse(parts[2]);
            
            string description = "";
            if (parts.Length >= 12) description = parts[11].Replace("/", "|");
            
            RPGItem item = new RPGItem(0, name, description, type, rarity, 1);
            item.attackBonus = int.Parse(parts[3]);
            item.defenseBonus = int.Parse(parts[4]);
            item.healthBonus = int.Parse(parts[5]);
            item.healPercentage = float.Parse(parts[6]);
            if (parts.Length >= 8) item.moveSpeedBonus = float.Parse(parts[7]);
            if (parts.Length >= 9) item.dodgeChargesBonus = int.Parse(parts[8]);
            if (parts.Length >= 10) item.goldOnKill = int.Parse(parts[9]);
            if (parts.Length >= 11) item.upgradeLevel = int.Parse(parts[10]);
            if (parts.Length >= 13) item.abilityPowerBonus = int.Parse(parts[12]);
            if (parts.Length >= 14) item.criticalChance = float.Parse(parts[13]);
            if (parts.Length >= 15) item.criticalDamage = float.Parse(parts[14]);
            if (parts.Length >= 16) item.thorns = float.Parse(parts[15]);
            if (parts.Length >= 17) item.lifeSteal = float.Parse(parts[16]);
            if (parts.Length >= 18) item.dustOnKill = int.Parse(parts[17]);
            if (parts.Length >= 19) item.regeneration = float.Parse(parts[18]);
            if (parts.Length >= 20) item.autoAttack = int.Parse(parts[19]) == 1;
            if (parts.Length >= 21) item.autoAim = int.Parse(parts[20]) == 1;
            if (parts.Length >= 22) item.attackSpeed = float.Parse(parts[21]);
            if (parts.Length >= 23) item.memoryHaste = float.Parse(parts[22]);
            if (parts.Length >= 24)
            {
                int elemType = int.Parse(parts[23]);
                if (elemType > 0)
                {
                    item.elementalType = (ElementalType)(elemType - 1);
                }
            }
            if (parts.Length >= 25) item.elementalStacks = int.Parse(parts[24]);
            if (parts.Length >= 26) item.shieldPercentage = float.Parse(parts[25]);
            if (parts.Length >= 27) item.requiredHero = parts[26].Replace("/", "|");
            
            return item;
        }
        catch
        {
            return null;
        }
    }
    
    private static string GetRarityColorHex(ItemRarity rarity)
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
            switch (rarity)
            {
                case ItemRarity.Common: return "#E4F0F2";
                case ItemRarity.Rare: return "#34FCFF";
                case ItemRarity.Epic: return "#C75CF7";
                case ItemRarity.Legendary: return "#FF3E34";
                default: return "#E4F0F2";
            }
        }
    }
    
    #endregion
    
    #region Local Processing
    
    private static void CreateLocalDrop(RPGItem item, Vector3 position, uint ownerNetId, uint dropId, bool isShared, bool isMonsterDrop = false)
    {
        // Prevent duplicate drops (race condition protection)
        if (_activeDrops.ContainsKey(dropId))
        {
            RPGLog.Warning(" Drop ID " + dropId + " already exists, skipping duplicate");
            return;
        }
        
        NetworkedDropData dropData = new NetworkedDropData();
        dropData.dropId = dropId;
        dropData.item = item;
        dropData.position = position;
        dropData.ownerNetId = ownerNetId;
        dropData.isShared = isShared;
        dropData.dismantleProgress = 0f;
        dropData.lastDismantleTapTime = 0f;
        dropData.isMonsterDrop = isMonsterDrop;
        
        _activeDrops[dropId] = dropData;
        
        if (OnItemDroppedLocally != null)
        {
            OnItemDroppedLocally(dropId, item, position);
        }
    }
    
    private static void RemoveLocalDrop(uint dropId)
    {
        if (_activeDrops.ContainsKey(dropId))
        {
            NetworkedDropData dropData = _activeDrops[dropId];
            
            if (dropData.visualInstance != null)
            {
                UnityEngine.Object.Destroy(dropData.visualInstance.gameObject);
            }
            
            _activeDrops.Remove(dropId);
        }
    }
    
    private static void RemoveLocalDropSilent(uint dropId)
    {
        if (_activeDrops.ContainsKey(dropId))
        {
            NetworkedDropData dropData = _activeDrops[dropId];
            
            if (dropData.visualInstance != null)
            {
                UnityEngine.Object.Destroy(dropData.visualInstance.gameObject);
            }
            
            _activeDrops.Remove(dropId);
        }
    }
    
    #endregion
    
    #region Utility
    
    private static Actor GetServerActor()
    {
        try
        {
            ActorManager am = NetworkedManagerBase<ActorManager>.instance;
            if (am != null)
            {
                return am.serverActor;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
    
    public static uint GetLocalPlayerNetId()
    {
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            return DewPlayer.local.hero.netId;
        }
        return 0;
    }
    
    private static DewPlayer GetPlayerByHeroNetId(uint netId)
    {
        foreach (DewPlayer player in DewPlayer.allHumanPlayers)
        {
            if (player != null && player.hero != null && player.hero.netId == netId)
            {
                return player;
            }
        }
        return null;
    }
    
    private static string GetPlayerNameByNetId(uint netId)
    {
        DewPlayer player = GetPlayerByHeroNetId(netId);
        if (player != null)
        {
            return player.playerName;
        }
        return Localization.Unknown;
    }
    
    private static int CalculateDustAmount(RPGItem item)
    {
        int baseAmount = 10;
        
        switch (item.rarity)
        {
            case ItemRarity.Common: baseAmount = 10; break;
            case ItemRarity.Rare: baseAmount = 25; break;
            case ItemRarity.Epic: baseAmount = 100; break;
            case ItemRarity.Legendary: baseAmount = 200; break;
        }
        
        baseAmount += (item.attackBonus + item.defenseBonus + item.healthBonus / 10) * 2;
        
        return baseAmount;
    }
    
    private static void SpawnDustReward(int amount)
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        try
        {
            if (NetworkServer.active)
            {
                // HOST: Directly add dust
                DewPlayer.local.AddDreamDust(amount);
                RPGLog.Debug(" Host received " + amount + " dust from dismantle");
            }
            else
            {
                // CLIENT: Request host to give us dust via chat message
                ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
                if (chatManager != null)
                {
                    string content = string.Format("[RPGDUST]{0}|{1}", DewPlayer.local.hero.netId, amount);
                    chatManager.CmdSendChatMessage(content, null);
                    RPGLog.Debug(" Client requested " + amount + " dust from dismantle");
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error spawning dust reward: " + e.Message);
        }
    }
    
    #endregion
}

