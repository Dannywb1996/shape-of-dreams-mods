using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteDungeonMod
{
    // ==================== MAP UI & PANNING ====================
    /// Static class to manage map panning state
    /// </summary>
    public static class MapPanningSystem
    {
        // Additional pan offset from user input (on top of auto-centering)
        public static Vector2 UserPanOffset = Vector2.zero;
        
        // Pan speed for keyboard
        public const float PAN_SPEED = 0.5f;
        
        // Is the user currently dragging the map?
        public static bool IsDragging = false;
        
        // Last mouse position for drag calculation
        public static Vector2 LastMousePosition;
        
        // Reference to the world map
        public static UI_InGame_WorldMap CurrentWorldMap;
        
        // Pan limits (how far the user can pan from center)
        public const float MAX_PAN = 2.0f;
        
        /// <summary>
        /// Reset user pan offset (center on current node)
        /// </summary>
        public static void CenterOnCurrentNode()
        {
            UserPanOffset = Vector2.zero;
        }
        
        /// <summary>
        /// Force refresh the world map to show newly created nodes
        /// Call this after generating new nodes
        /// </summary>
        public static void ForceRefreshMap()
        {
            try
            {
                // Find the world map if we don't have a reference
                if (CurrentWorldMap == null)
                {
                    CurrentWorldMap = UnityEngine.Object.FindObjectOfType<UI_InGame_WorldMap>();
                }
                
                if (CurrentWorldMap != null && CurrentWorldMap.gameObject.activeInHierarchy)
                {
                    // RefreshNodes is private, so use reflection to call it
                    var refreshMethod = typeof(UI_InGame_WorldMap).GetMethod("RefreshNodes", 
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    
                    if (refreshMethod != null)
                    {
                        refreshMethod.Invoke(CurrentWorldMap, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[InfiniteDungeon] Could not refresh map: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Apply user pan offset to move the view
        /// </summary>
        public static void Pan(Vector2 delta)
        {
            UserPanOffset += delta;
            UserPanOffset.x = Mathf.Clamp(UserPanOffset.x, -MAX_PAN, MAX_PAN);
            UserPanOffset.y = Mathf.Clamp(UserPanOffset.y, -MAX_PAN, MAX_PAN);
        }
        
        /// <summary>
        /// Get the panned position for a node - ALWAYS centered on current node + user offset
        /// </summary>
        public static Vector2 GetPannedPosition(Vector2 originalNormalizedPos)
        {
            // First, calculate the offset needed to center the CURRENT node
            ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
            
            // Safety checks - if anything is invalid, return center position
            if (zm == null || zm.nodes == null || zm.nodes.Count == 0)
            {
                return new Vector2(0.5f, 0.5f); // Center of screen
            }
            
            if (zm.currentNodeIndex < 0 || zm.currentNodeIndex >= zm.nodes.Count)
            {
                return new Vector2(0.5f, 0.5f); // Center of screen
            }
            
            // Check for valid world dimensions to prevent division by zero
            float worldWidth = ZoneManager.WorldWidth;
            float worldHeight = ZoneManager.WorldHeight;
            if (worldWidth <= 0f) worldWidth = 1f;
            if (worldHeight <= 0f) worldHeight = 1f;
            
            Vector2 currentNodePos = zm.nodes[zm.currentNodeIndex].position;
            float currentNormX = currentNodePos.x / worldWidth;
            float currentNormY = currentNodePos.y / worldHeight;
            
            // Clamp to prevent NaN/Infinity
            currentNormX = Mathf.Clamp(currentNormX, -10f, 10f);
            currentNormY = Mathf.Clamp(currentNormY, -10f, 10f);
            
            // Calculate auto-center offset (to put current node at 0.5, 0.5)
            Vector2 autoCenterOffset = new Vector2(0.5f - currentNormX, 0.5f - currentNormY);
            
            // Apply auto-center + user pan offset
            Vector2 result = originalNormalizedPos + autoCenterOffset + UserPanOffset;
            
            // Final safety clamp to prevent runaway positions
            result.x = Mathf.Clamp(result.x, -5f, 5f);
            result.y = Mathf.Clamp(result.y, -5f, 5f);
            
            return result;
        }
    }
    
    /// <summary>
    /// Patch to initialize map panning when the world map is opened
    /// </summary>
    [HarmonyPatch(typeof(UI_InGame_WorldMap), "OnEnable")]
    public static class WorldMapPanningInitPatch
    {
        static void Postfix(UI_InGame_WorldMap __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            
            try
            {
                // Only apply to the main (full) world map, not the minimap
                if (!__instance.isMain) return;
                
                // Store reference
                MapPanningSystem.CurrentWorldMap = __instance;
                
                // Center on current node when map opens
                MapPanningSystem.CenterOnCurrentNode();
                
                // Add the panning controller component if not present
                MapPanController controller = __instance.GetComponent<MapPanController>();
                if (controller == null)
                {
                    controller = __instance.gameObject.AddComponent<MapPanController>();
                }
                controller.worldMap = __instance;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in WorldMapPanningInitPatch: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Patch to apply pan offset to node positions during Setup - makes nodes appear centered immediately
    /// </summary>
    [HarmonyPatch(typeof(UI_InGame_World_NodeItem), "Setup")]
    public static class NodeItemSetupPatch
    {
        static void Postfix(UI_InGame_World_NodeItem __instance, int i, UI_InGame_WorldMap parent)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            
            try
            {
                // Only apply to the main world map, not minimap
                if (__instance.isMiniMapVariant) return;
                
                // Safety check for valid node index
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null || zm.nodes == null || i < 0 || i >= zm.nodes.Count)
                {
                    return; // Skip if invalid
                }
                
                // Check for valid world dimensions to prevent division by zero
                float worldWidth = ZoneManager.WorldWidth;
                float worldHeight = ZoneManager.WorldHeight;
                if (worldWidth <= 0f) worldWidth = 1f;
                if (worldHeight <= 0f) worldHeight = 1f;
                
                // Get the node's original normalized position
                WorldNodeData node = zm.nodes[i];
                Vector2 originalPos = new Vector2(
                    node.position.x / worldWidth,
                    node.position.y / worldHeight
                );
                
                // Clamp to prevent NaN/Infinity
                originalPos.x = Mathf.Clamp(originalPos.x, -10f, 10f);
                originalPos.y = Mathf.Clamp(originalPos.y, -10f, 10f);
                
                // Apply pan offset IMMEDIATELY so nodes appear centered from the start
                Vector2 pannedPos = MapPanningSystem.GetPannedPosition(originalPos);
                
                // Set the anchor immediately (no animation)
                RectTransform rt = (RectTransform)__instance.transform;
                rt.anchorMin = pannedPos;
                rt.anchorMax = pannedPos;
                
                // Also update the private _originalPos field so the Update method uses the correct position
                System.Reflection.FieldInfo originalPosField = typeof(UI_InGame_World_NodeItem).GetField("_originalPos", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (originalPosField != null)
                {
                    originalPosField.SetValue(__instance, pannedPos);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in NodeItemSetupPatch: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Patch the Update method to use the panned position we set in Setup
    /// The _originalPos is already set to the correct panned position, so we just need to
    /// recalculate it each frame to handle dynamic panning
    /// </summary>
    [HarmonyPatch(typeof(UI_InGame_World_NodeItem), "Update")]
    public static class NodeItemUpdatePatch
    {
        // CACHED REFLECTION FIELDS - GetField is expensive, only do it once!
        private static System.Reflection.FieldInfo _cvField;
        private static System.Reflection.FieldInfo _currentOffsetField;
        private static System.Reflection.FieldInfo _nextOffsetSetTimeField;
        private static System.Reflection.FieldInfo _startTimeField;
        private static bool _fieldsCached = false;
        
        private static void CacheFields()
        {
            if (_fieldsCached) return;
            
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            _cvField = typeof(UI_InGame_World_NodeItem).GetField("_cv", flags);
            _currentOffsetField = typeof(UI_InGame_World_NodeItem).GetField("_currentOffset", flags);
            _nextOffsetSetTimeField = typeof(UI_InGame_World_NodeItem).GetField("_nextOffsetSetTime", flags);
            _startTimeField = typeof(UI_InGame_World_NodeItem).GetField("_startTime", flags);
            _fieldsCached = true;
        }
        
        static bool Prefix(UI_InGame_World_NodeItem __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            try
            {
                // Only intercept for main world map, not minimap
                if (__instance.isMiniMapVariant) return true;
                
                // Cache reflection fields (only done once)
                CacheFields();
                
                if (_cvField == null || _currentOffsetField == null) return true;
                
                Vector2 cv = (Vector2)_cvField.GetValue(__instance);
                Vector2 currentOffset = (Vector2)_currentOffsetField.GetValue(__instance);
                float nextOffsetSetTime = (float)_nextOffsetSetTimeField.GetValue(__instance);
                float startTime = (float)_startTimeField.GetValue(__instance);
                
                // Handle "you are here" animation (copied from original)
                GameObject youAreHereObject = __instance.youAreHereObject;
                if (youAreHereObject != null && youAreHereObject.activeSelf)
                {
                    youAreHereObject.transform.localScale = Vector3.one * (1.35f + Mathf.Sin((Time.time - startTime) * 5f + Mathf.PI / 2f) * 0.3f);
                }
                
                // Get the node's RAW position from ZoneManager (not the cached _originalPos)
                int nodeIndex = __instance.index;
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null || zm.nodes == null || zm.nodes.Count == 0) return true;
                if (nodeIndex < 0 || nodeIndex >= zm.nodes.Count) return true;
                
                // Check for valid world dimensions to prevent division by zero
                float worldWidth = ZoneManager.WorldWidth;
                float worldHeight = ZoneManager.WorldHeight;
                if (worldWidth <= 0f) worldWidth = 1f;
                if (worldHeight <= 0f) worldHeight = 1f;
                
                WorldNodeData node = zm.nodes[nodeIndex];
                Vector2 rawNormalizedPos = new Vector2(
                    node.position.x / worldWidth,
                    node.position.y / worldHeight
                );
                
                // Clamp to prevent NaN/Infinity
                rawNormalizedPos.x = Mathf.Clamp(rawNormalizedPos.x, -10f, 10f);
                rawNormalizedPos.y = Mathf.Clamp(rawNormalizedPos.y, -10f, 10f);
                
                // Apply pan offset to get the target position
                Vector2 targetPos = MapPanningSystem.GetPannedPosition(rawNormalizedPos);
                
                // Use SmoothDamp to the target position
                RectTransform rt = (RectTransform)__instance.transform;
                Vector2 newAnchor = Vector2.SmoothDamp(rt.anchorMin, targetPos + currentOffset, ref cv, 0.6f);
                rt.anchorMin = newAnchor;
                rt.anchorMax = newAnchor;
                
                // Save cv back
                _cvField.SetValue(__instance, cv);
                
                // Handle random offset (copied from original)
                if (Time.time > nextOffsetSetTime)
                {
                    _nextOffsetSetTimeField.SetValue(__instance, Time.time + UnityEngine.Random.Range(0.25f, 0.7f));
                    _currentOffsetField.SetValue(__instance, UnityEngine.Random.insideUnitCircle * 0.0075f);
                }
                
                return false; // Skip original Update
            }
            catch
            {
                return true; // If anything fails, run original
            }
        }
    }
    
    /// <summary>
    /// MonoBehaviour component that handles map panning input
    /// </summary>
    public class MapPanController : MonoBehaviour
    {
        public UI_InGame_WorldMap worldMap;
        
        private float _lastRefreshTime = 0f;
        
        void Update()
        {
            if (worldMap == null || !worldMap.isMain) return;
            
            // Handle keyboard panning (WASD or Arrow keys while holding Shift)
            HandleKeyboardPan();
            
            // Handle mouse drag panning (middle mouse button or right mouse button)
            HandleMousePan();
            
            // Handle scroll wheel for quick center
            HandleScrollWheel();
        }
        
        void HandleKeyboardPan()
        {
            float panAmount = MapPanningSystem.PAN_SPEED * Time.unscaledDeltaTime;
            bool panned = false;
            
            // Use Shift + WASD or Shift + Arrow keys for panning
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
                    MapPanningSystem.Pan(new Vector2(0, -panAmount));
                    panned = true;
                }
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                {
                    MapPanningSystem.Pan(new Vector2(0, panAmount));
                    panned = true;
                }
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                {
                    MapPanningSystem.Pan(new Vector2(panAmount, 0));
                    panned = true;
                }
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                {
                    MapPanningSystem.Pan(new Vector2(-panAmount, 0));
                    panned = true;
                }
            }
            
            // Press Home or H to center on current node
            if (Input.GetKeyDown(KeyCode.Home) || Input.GetKeyDown(KeyCode.H))
            {
                MapPanningSystem.CenterOnCurrentNode();
                panned = true;
            }
            
            if (panned)
            {
                RefreshMapIfNeeded();
            }
        }
        
        void HandleMousePan()
        {
            // Mouse dragging disabled - game closes map on any mouse button
            // Use keyboard controls instead (Shift + WASD)
        }
        
        void HandleScrollWheel()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // Scroll to zoom/pan - for now just use it as quick pan
                // Scroll up = move view up (pan down)
                MapPanningSystem.Pan(new Vector2(0, -scroll * 0.2f));
                RefreshMapIfNeeded();
            }
        }
        
        void RefreshMapIfNeeded()
        {
            // Throttle refreshes to avoid performance issues
            if (Time.unscaledTime - _lastRefreshTime < 0.05f) return;
            _lastRefreshTime = Time.unscaledTime;
            
            // Refresh the node positions
            if (worldMap != null && worldMap.nodeParent != null)
            {
                // Update all node positions
                foreach (Transform child in worldMap.nodeParent)
                {
                    UI_InGame_World_NodeItem nodeItem = child.GetComponent<UI_InGame_World_NodeItem>();
                    if (nodeItem != null && !nodeItem.isMiniMapVariant)
                    {
                        UpdateNodePosition(nodeItem);
                    }
                    
                    // Also update edges
                    UI_InGame_World_Edge edge = child.GetComponent<UI_InGame_World_Edge>();
                    if (edge != null)
                    {
                        // Edges will update automatically based on their connected nodes
                    }
                }
            }
        }
        
        void UpdateNodePosition(UI_InGame_World_NodeItem nodeItem)
        {
            try
            {
                WorldNodeData node = nodeItem.node;
                if (node.room == null && node.type == WorldNodeType.Start)
                {
                    // Start node might not have room set
                }
                
                // Check for valid world dimensions to prevent division by zero
                float worldWidth = ZoneManager.WorldWidth;
                float worldHeight = ZoneManager.WorldHeight;
                if (worldWidth <= 0f) worldWidth = 1f;
                if (worldHeight <= 0f) worldHeight = 1f;
                
                Vector2 originalPos = new Vector2(
                    node.position.x / worldWidth,
                    node.position.y / worldHeight
                );
                
                // Clamp to prevent NaN/Infinity
                originalPos.x = Mathf.Clamp(originalPos.x, -10f, 10f);
                originalPos.y = Mathf.Clamp(originalPos.y, -10f, 10f);
                
                Vector2 pannedPos = MapPanningSystem.GetPannedPosition(originalPos);
                pannedPos = new Vector2(0.5f, 0.5f) * -0.12f + pannedPos * 1.12f;
                
                RectTransform rt = (RectTransform)nodeItem.transform;
                rt.anchorMin = pannedPos;
                rt.anchorMax = pannedPos;
            }
            catch { }
        }
        
        private GameObject _hintUI;
        private TextMeshProUGUI _hintText;
        
        void OnEnable()
        {
            // Create the hint UI when map is opened
            if (worldMap != null && worldMap.isMain)
            {
                CreateHintUI();
            }
        }
        
        void OnDisable()
        {
            // Destroy hint UI when map closes
            if (_hintUI != null)
            {
                UnityEngine.Object.Destroy(_hintUI);
                _hintUI = null;
            }
        }
        
        void OnDestroy()
        {
            if (_hintUI != null)
            {
                UnityEngine.Object.Destroy(_hintUI);
                _hintUI = null;
            }
        }
        
        void CreateHintUI()
        {
            try
            {
                if (_hintUI != null) return;
                
                // Find the canvas
                Canvas canvas = worldMap.GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
                }
                if (canvas == null) return;
                
                // Create container - positioned at TOP of screen
                // Use DewGUI.canvasTransform for proper CanvasScaler integration
                Transform canvasParent = DewGUI.canvasTransform ?? canvas.transform;
                _hintUI = new GameObject("MapPanHint");
                _hintUI.transform.SetParent(canvasParent, false);
                
                RectTransform rt = _hintUI.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);  // Top center
                rt.anchorMax = new Vector2(0.5f, 1f);  // Top center
                rt.pivot = new Vector2(0.5f, 1f);      // Pivot at top
                
                // Fixed sizes for 1440p reference - CanvasScaler handles scaling
                rt.anchoredPosition = new Vector2(0, -25f);
                rt.sizeDelta = new Vector2(700f, 55f);
                
                // Add background
                UnityEngine.UI.Image bg = _hintUI.AddComponent<UnityEngine.UI.Image>();
                bg.color = new Color(0, 0, 0, 0.75f);
                
                // Create text using game's widget for proper font support (Chinese characters)
                _hintText = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, _hintUI.transform);
                _hintText.text = DungeonLocalization.IsChinese ?
                    "地图控制:  Shift + WASD 平移  |  H 回到当前位置" :
                    "Map Controls:  Shift + WASD to pan  |  H to center";
                // Use prefab default font size - CanvasScaler handles resolution scaling
                _hintText.alignment = TextAlignmentOptions.Center;
                _hintText.color = new Color(0.9f, 0.9f, 0.9f, 1f);  // Brighter text
                
                // Position the text to fill the container (fixed padding for 1440p reference)
                RectTransform textRt = _hintText.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = new Vector2(15f, 8f);
                textRt.offsetMax = new Vector2(-15f, -8f);
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error creating hint UI: " + ex.Message);
            }
        }
    }
    
    [HarmonyPatch(typeof(ZoneManager), "GenerateWorld_Imp")]
    public static class WorldGenerationPatch
    {
        static bool Prefix(ZoneManager __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            if (!NetworkServer.active) return true;
            
            // CRITICAL: If we're traveling to Zone_Primus (final boss), let the game handle it!
            if (InfiniteDungeonMod.IsTravelingToPrimus())
                return true; // Let the game's normal world generation happen
            
            try
            {
                // First, check if nodes exist - this is the primary indicator of whether we need generation
                bool hasExistingNodes = __instance.nodes != null && __instance.nodes.Count > 0;
                
                // If dungeon was initialized BUT there are no nodes, this is a NEW game (player died and restarted)
                // We MUST reset and regenerate in this case!
                if (InfiniteDungeonMod.IsDungeonInitialized() && !hasExistingNodes)
                {
                    InfiniteDungeonMod.ResetDungeonState();
                    // Fall through to normal generation below
                }
                // If dungeon is initialized AND nodes exist, this is returning from lobby mid-run
                else if (InfiniteDungeonMod.IsDungeonInitialized() && hasExistingNodes)
                {
                    
                    // Mark as continued run
                    InfiniteDungeonMod.SetContinuedRun(true);
                    
                    // Still invoke onWorldGenerated so the game continues properly
                    if (__instance.onWorldGenerated != null)
                    {
                        __instance.onWorldGenerated.Invoke();
                    }
                    
                    return false; // Skip original method
                }
                // If dungeon NOT initialized but nodes exist, this is loading a saved game
                else if (!InfiniteDungeonMod.IsDungeonInitialized() && hasExistingNodes)
                {
                    // Validate the nodes - if any are invalid, reset and regenerate
                    bool nodesValid = true;
                    try
                    {
                        foreach (var node in __instance.nodes)
                        {
                            if (string.IsNullOrEmpty(node.room))
                            {
                                nodesValid = false;
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        nodesValid = false;
                    }
                    
                    if (!nodesValid)
                    {
                        InfiniteDungeonMod.ResetDungeonState();
                        __instance.nodes.Clear();
                        // Fall through to normal generation
                    }
                    else
                    {
                        // Mark as continued run so EnsureStartNodeHasAdjacent can import the data
                        InfiniteDungeonMod.SetContinuedRun(true);
                        InfiniteDungeonMod.MarkDungeonInitialized();
                        
                        // Still invoke onWorldGenerated so the game continues properly
                        if (__instance.onWorldGenerated != null)
                        {
                            __instance.onWorldGenerated.Invoke();
                        }
                        
                        return false; // Skip original method
                    }
                }
                
                // If we get here, it's a fresh new game - generate the start node
                InfiniteDungeonMod.ResetDungeonState();
                
                Zone currentZone = __instance.currentZone;
                if (currentZone == null)
                {
                    Debug.LogError("[InfiniteDungeon] No current zone set!");
                    return true;
                }
                
                string startRoom = null;
                if (currentZone.startRooms != null && currentZone.startRooms.Count > 0)
                {
                    startRoom = currentZone.startRooms[UnityEngine.Random.Range(0, currentZone.startRooms.Count)];
                }
                else if (currentZone.combatRooms != null && currentZone.combatRooms.Count > 0)
                {
                    startRoom = currentZone.combatRooms[UnityEngine.Random.Range(0, currentZone.combatRooms.Count)];
                }
                
                if (string.IsNullOrEmpty(startRoom))
                {
                    Debug.LogError("[InfiniteDungeon] Could not find start room");
                    return true;
                }
                
                WorldNodeData startNode = new WorldNodeData
                {
                    room = startRoom,
                    type = WorldNodeType.Start,
                    status = WorldNodeStatus.Revealed,
                    position = Vector2.zero,
                    roomRotValue = 0f,
                    roomRotIndex = 0,
                    modifiers = new List<ModifierData>()
                };
                
                __instance.nodes.Clear();
                __instance.nodes.Add(startNode);
                
                __instance.hunterStatuses.Clear();
                __instance.hunterStatuses.Add(HunterStatus.None);
                
                __instance.visitedNodesSaveData = new List<DewPersistence.RoomData>();
                __instance.visitedNodesSaveData.Add(null);
                
                __instance.nodeDistanceMatrix.Clear();
                __instance.nodeDistanceMatrix.Add(0);
                
                // Use the correct backing field names (without Network_ prefix)
                var currentNodeField = AccessTools.Field(typeof(ZoneManager), "_003CcurrentNodeIndex_003Ek__BackingField");
                if (currentNodeField != null)
                {
                    currentNodeField.SetValue(__instance, -1);
                }
                else
                {
                    // Try property setter directly
                    var prop = typeof(ZoneManager).GetProperty("currentNodeIndex");
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(__instance, -1, null);
                    }
                }
                
                var hunterStartField = AccessTools.Field(typeof(ZoneManager), "_003ChunterStartNodeIndex_003Ek__BackingField");
                if (hunterStartField != null)
                {
                    hunterStartField.SetValue(__instance, 0);
                }
                
                var hunterSkippedField = AccessTools.Field(typeof(ZoneManager), "_003ChunterSkippedTurns_003Ek__BackingField");
                if (hunterSkippedField != null)
                {
                    hunterSkippedField.SetValue(__instance, 2);
                }
                
                // currentTurnIndex has a private setter, try to find the backing field
                var turnIndexField = AccessTools.Field(typeof(ZoneManager), "<currentTurnIndex>k__BackingField");
                if (turnIndexField != null)
                {
                    turnIndexField.SetValue(__instance, 0);
                }
                
                InfiniteDungeonMod.RegisterNodeZone(0, currentZone);
                
                // Mark the start node as already visited (so it doesn't count as "new" when loaded)
                // Start node is depth 0 (the dungeon entrance)
                InfiniteDungeonMod.MarkNodeVisited(0);
                InfiniteDungeonMod.SetTotalDepth(0);
                
                // Mark dungeon as initialized (prevents regeneration on lobby return)
                InfiniteDungeonMod.MarkDungeonInitialized();
                
                if (__instance.onWorldGenerated != null)
                {
                    __instance.onWorldGenerated.Invoke();
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in world generation: " + ex.Message);
                return true;
            }
        }
    }
    
    /// <summary>
    /// Block the game from adding modifiers through ZoneManager.AddModifier
    /// This prevents double modifiers from the game's default systems
    /// Our modifiers are pre-generated when nodes are created
    /// EXCEPTION: Critical game modifiers like HeroSoul (for reviving teammates) are always allowed!
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), "AddModifier", new Type[] { typeof(int), typeof(ModifierData), typeof(Action<RoomModifierBase>) })]
    public static class BlockAddModifierPatch
    {
        // Critical modifiers that must ALWAYS be allowed (game functionality)
        private static readonly HashSet<string> CriticalModifiers = new HashSet<string>
        {
            "RoomMod_HeroSoul",           // Reviving dead teammates - CRITICAL!
            "RoomMod_Hunted",             // Hunter system (game's native)
            "RoomMod_QuestTarget",        // Quest objectives
            "RoomMod_Limbo_Decorator",    // Visual decorator - cosmetic, should be allowed
            // Fragment of Radiance (Light Boss) quest modifiers
            "RoomMod_FragmentOfRadiance_StartProp",       // Starting shrine that spawns the quest
            "RoomMod_FragmentOfRadiance_MysteriousPlace", // Quest destination room modifier
        };
        
        static bool Prefix(ZoneManager __instance, int nodeIndex, ModifierData mod)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            // Don't interfere when traveling to Zone_Primus
            if (InfiniteDungeonMod.IsTravelingToPrimus()) return true;
            
            // ALWAYS allow critical game modifiers (like HeroSoul for reviving teammates)
            if (!string.IsNullOrEmpty(mod.type) && CriticalModifiers.Contains(mod.type))
            {
                return true; // Silent allow - no spam logging for decorators
            }
            
            // Also allow any modifier with "HeroSoul" in the name (safety net)
            if (!string.IsNullOrEmpty(mod.type) && mod.type.Contains("HeroSoul"))
            {
                return true;
            }
            
            // Allow decorators (cosmetic modifiers)
            if (!string.IsNullOrEmpty(mod.type) && mod.type.Contains("Decorator"))
            {
                return true; // Silent allow - decorators are cosmetic
            }
            
            // Allow quest-related modifiers (FragmentOfRadiance, etc.)
            if (!string.IsNullOrEmpty(mod.type) && mod.type.Contains("FragmentOfRadiance"))
            {
                return true;
            }
            
            // Allow our own modifier additions (when _allowModifierAddition is true)
            if (InfiniteDungeonMod.ShouldAllowModifierAddition()) return true;
            
            // Check if this is one of our modifiers being re-added (shouldn't happen but just in case)
            if (InfiniteDungeonMod.IsOurModifier(mod.id)) return true;
            
            // Block all other modifier additions from the game
            return false;
        }
    }
    
    // NOTE: We don't block generic AddModifier<T> because:
    // 1. It's needed for RoomMod_HeroSoul (reviving teammates)
    // 2. The TargetMethod() approach with generics can cause Harmony issues
    // The non-generic AddModifier patch above handles most cases with whitelist
    
    /// <summary>
    /// Prevent KeyNotFoundException and NullReferenceException when modifiers try to save their state.
    /// This happens when a modifier with an unregistered ID tries to access modifierServerData,
    /// or when the modifier's gameObject has already been destroyed.
    /// </summary>
    [HarmonyPatch(typeof(RoomModifierBase), "ClientEventOnRoomLoadStarted")]
    public static class SafeModifierSavePatch
    {
        static bool Prefix(RoomModifierBase __instance)
        {
            if (!InfiniteDungeonMod.IsModActive) return true;
            
            try
            {
                // Check if the modifier or its gameObject is destroyed
                if (__instance == null || __instance.gameObject == null)
                {
                    return false; // Skip - modifier is destroyed
                }
                
                // Check if the modifier's ID exists in modifierServerData
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null) return false; // Skip if no ZoneManager
                
                int modId = __instance.id;
                
                // If the ID doesn't exist in modifierServerData, add a dummy entry
                if (!zm.modifierServerData.ContainsKey(modId))
                {
                    zm.modifierServerData[modId] = new ModifierServerData
                    {
                        didCreateInstance = true,
                        persistentData = null
                    };
                }
            }
            catch (Exception)
            {
                // If any error occurs, skip the original method to prevent crashes
                return false;
            }
            
            return true; // Allow original method
        }
    }
    
    /// <summary>
    /// Detect when Pot of Greed shrine is used and schedule a miniboss for the next combat room.
    /// Pot of Greed gives gold but spawns a miniboss in the next combat room.
    /// </summary>
    [HarmonyPatch(typeof(Shrine), "DoPostUseRoutines")]
    public static class PotOfGreedDetectionPatch
    {
        static void Postfix(Shrine __instance, Entity entity)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            
            try
            {
                // Check if this is a Pot of Greed shrine
                string shrineName = __instance.GetType().Name;
                if (shrineName == "Shrine_PotOfGreed" || __instance.name.Contains("PotOfGreed"))
                {
                    // Get the player who used it
                    if (entity != null && entity.owner != null)
                    {
                        InfiniteDungeonMod.OnPotOfGreedUsed(entity.owner.guid);
                    }
                    else
                    {
                        // Fallback - just schedule the miniboss
                        InfiniteDungeonMod.OnPotOfGreedUsed("unknown");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in PotOfGreedDetectionPatch: " + ex.Message);
            }
        }
    }
    
    // ==================== GAME RESULT WINDOW ====================
    /// <summary>
    /// Custom end-of-run result window showing Infinite Dungeon stats
    /// </summary>
    public static class InfiniteDungeonResultUI
    {
        private static UI_Window _resultWindow = null;
        private static bool _resultShown = false;
        
        /// <summary>
        /// Show the custom result window with dungeon stats
        /// </summary>
        public static void ShowResultWindow(DewGameResult.ResultType resultType)
        {
            if (_resultWindow != null || _resultShown) return;
            
            try
            {
                // Detect language for this client before creating window
                DungeonLocalization.DetectLanguage();
                
                CreateResultWindow(resultType);
                _resultShown = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error showing result window: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Reset result window state (for new runs)
        /// </summary>
        public static void Reset()
        {
            _resultShown = false;
            if (_resultWindow != null)
            {
                UnityEngine.Object.Destroy(_resultWindow.gameObject);
                _resultWindow = null;
            }
        }
        
        private static void CreateResultWindow(DewGameResult.ResultType resultType)
        {
            // ========== UI SIZING ==========
            // All sizes are in "reference pixels" for 1440p
            // The game's CanvasScaler automatically scales everything based on:
            // - Screen resolution (height / 1440)
            // - User's UI scale setting
            // So we just use FIXED values here - no manual scaling needed!
            
            float windowWidth = 700f; // Reference width for 1440p
            
            // Create window using DewGUI
            _resultWindow = UnityEngine.Object.Instantiate(DewGUI.widgetWindow, DewGUI.canvasTransform);
            _resultWindow.isDraggable = false;
            _resultWindow.enableBackdrop = true;
            _resultWindow.SetWidth(windowWidth);
            
            // Position in center
            RectTransform windowRect = _resultWindow.GetComponent<RectTransform>();
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.anchoredPosition = Vector2.zero;
            // DO NOT set localScale - CanvasScaler handles all scaling
            
            // Padding and spacing (fixed reference values for 1440p)
            int basePadding = 35;
            int topBottomPadding = 25;
            float baseSpacing = 10f;
            
            // Create vertical layout for content
            VerticalLayoutGroup mainLayout = DewGUI.CreateVerticalLayoutGroup(_resultWindow.transform);
            mainLayout.padding = new RectOffset(basePadding, basePadding, topBottomPadding, topBottomPadding);
            mainLayout.spacing = baseSpacing;
            mainLayout.childAlignment = TextAnchor.UpperCenter;
            
            float contentWidth = windowWidth - (basePadding * 2);
            UnityEngine.UI.LayoutElement mainLayoutElement = mainLayout.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            mainLayoutElement.preferredWidth = contentWidth;
            
            // Get stats from local mod tracking (only mod-specific stats, not game stats)
            int depth = InfiniteDungeonMod.GetTotalDepth();
            int nodesExplored = InfiniteDungeonMod.GetVisitedNodesCount();
            int infectedNodes = InfiniteDungeonMod.GetInfectedNodeCount();
            int cursesEndured = InfiniteDungeonMod.GetCursesEndured();
            int weakenedBosses = InfiniteDungeonMod.GetWeakenedBossesDefeated();
            
            // ========== TITLE ==========
            // Use prefab defaults, apply multipliers for hierarchy
            TMPro.TextMeshProUGUI title = UnityEngine.Object.Instantiate(DewGUI.widgetTextHeader, mainLayout.transform);
            title.text = DungeonLocalization.ResultTitle;
            title.fontSize *= 1.3f; // Larger for title emphasis
            title.alignment = TMPro.TextAlignmentOptions.Center;
            title.color = new Color(1f, 0.75f, 0.3f);
            
            // Subtitle with rank
            string rank = DungeonLocalization.ResultDepthRank(depth);
            TMPro.TextMeshProUGUI subtitle = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
            subtitle.text = "<color=#aaddff>" + rank + "</color>";
            // Use prefab default font size
            subtitle.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Spacer
            CreateSpacer(mainLayout.transform, 10f);
            
            // ========== STATS GRID ==========
            // Create a horizontal layout for stats
            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(mainLayout.transform, false);
            HorizontalLayoutGroup statsLayout = statsContainer.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 30f; // Fixed spacing for 1440p
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = false;
            
            // Left column stats
            GameObject leftColumn = new GameObject("LeftColumn");
            leftColumn.transform.SetParent(statsContainer.transform, false);
            VerticalLayoutGroup leftLayout = leftColumn.AddComponent<VerticalLayoutGroup>();
            leftLayout.spacing = 6f;
            leftLayout.childAlignment = TextAnchor.MiddleRight;
            
            // Right column stats  
            GameObject rightColumn = new GameObject("RightColumn");
            rightColumn.transform.SetParent(statsContainer.transform, false);
            VerticalLayoutGroup rightLayout = rightColumn.AddComponent<VerticalLayoutGroup>();
            rightLayout.spacing = 6f;
            rightLayout.childAlignment = TextAnchor.MiddleLeft;
            
            // Add stats - Left side (labels) - Only mod-specific stats
            // Use null for fontSize to use prefab default
            CreateStatLabel(leftLayout.transform, DungeonLocalization.ResultDepthReached + ":", 0f, new Color(0.7f, 0.7f, 0.8f));
            CreateStatLabel(leftLayout.transform, DungeonLocalization.ResultNodesExplored + ":", 0f, new Color(0.7f, 0.7f, 0.8f));
            CreateStatLabel(leftLayout.transform, DungeonLocalization.ResultInfectedNodes + ":", 0f, new Color(0.7f, 0.7f, 0.8f));
            CreateStatLabel(leftLayout.transform, DungeonLocalization.ResultCursesEndured + ":", 0f, new Color(0.7f, 0.7f, 0.8f));
            CreateStatLabel(leftLayout.transform, DungeonLocalization.ResultWeakenedBosses + ":", 0f, new Color(0.7f, 0.7f, 0.8f));
            
            // Add stats - Right side (values)
            CreateStatValue(rightLayout.transform, depth.ToString(), 0f, new Color(1f, 0.85f, 0.4f));
            CreateStatValue(rightLayout.transform, nodesExplored.ToString(), 0f, new Color(0.6f, 0.9f, 1f));
            CreateStatValue(rightLayout.transform, infectedNodes.ToString(), 0f, new Color(1f, 0.4f, 0.4f));
            CreateStatValue(rightLayout.transform, cursesEndured.ToString(), 0f, new Color(0.8f, 0.4f, 1f));
            CreateStatValue(rightLayout.transform, weakenedBosses.ToString(), 0f, new Color(0.9f, 0.5f, 0.5f));
            
            // ========== SCORE & MASTERY BONUSES ==========
            // Calculate bonuses
            long scoreBonus = CalculateScoreBonus(depth, infectedNodes, cursesEndured, weakenedBosses);
            long masteryBonus = CalculateMasteryBonus(depth, infectedNodes, cursesEndured);
            
            // Score Bonus row
            CreateBonusRow(mainLayout.transform, DungeonLocalization.ResultScoreBonus, "+" + scoreBonus.ToString("N0"), 
                           0f, 0f, new Color(1f, 0.85f, 0.4f)); // Gold
            
            // Mastery Bonus row
            CreateBonusRow(mainLayout.transform, DungeonLocalization.ResultMasteryBonus, "+" + masteryBonus.ToString("N0"), 
                           0f, 0f, new Color(0.33f, 0.94f, 0.77f)); // Green
            
            // Spacer
            CreateSpacer(mainLayout.transform, 15f);
            
            // ========== SUMMARY MESSAGE ==========
            string summaryText = GetSummaryText(resultType);
            TMPro.TextMeshProUGUI summary = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, mainLayout.transform);
            summary.text = summaryText;
            summary.fontSize *= 0.85f; // Slightly smaller for summary
            summary.alignment = TMPro.TextAlignmentOptions.Center;
            summary.fontStyle = TMPro.FontStyles.Italic;
            summary.color = new Color(0.75f, 0.75f, 0.85f);
            
            // Spacer
            CreateSpacer(mainLayout.transform, 15f);
            
            // ========== CLOSE BUTTON ==========
            float buttonWidth = 200f;  // Fixed size for 1440p reference
            float buttonHeight = 45f;  // Fixed size for 1440p reference
            
            UnityEngine.UI.Button closeButton = UnityEngine.Object.Instantiate(DewGUI.widgetButton, mainLayout.transform);
            closeButton.SetText(DungeonLocalization.ResultButtonClose);
            closeButton.SetWidth(buttonWidth);
            closeButton.SetHeight(buttonHeight);
            closeButton.interactable = true;
            
            var buttonImage = closeButton.GetComponent<UnityEngine.UI.Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
            }
            
            var windowToClose = _resultWindow;
            closeButton.onClick.AddListener(() => {
                if (windowToClose != null)
                {
                    UnityEngine.Object.Destroy(windowToClose.gameObject);
                }
                _resultWindow = null;
            });
            
            // Add ContentSizeFitter
            UnityEngine.UI.ContentSizeFitter fitter = _resultWindow.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            
            // Ensure window is on top
            _resultWindow.transform.SetAsLastSibling();
        }
        
        private static void CreateSpacer(Transform parent, float height)
        {
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);
            UnityEngine.UI.LayoutElement le = spacer.AddComponent<UnityEngine.UI.LayoutElement>();
            le.preferredHeight = height;
        }
        
        private static void CreateStatLabel(Transform parent, string text, float fontSize, Color color)
        {
            TMPro.TextMeshProUGUI label = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, parent);
            label.text = text;
            // If fontSize is 0 or less, use prefab default (CanvasScaler handles resolution)
            if (fontSize > 0f) label.fontSize = fontSize;
            label.alignment = TMPro.TextAlignmentOptions.Right;
            label.color = color;
        }
        
        private static void CreateStatValue(Transform parent, string text, float fontSize, Color color)
        {
            TMPro.TextMeshProUGUI value = UnityEngine.Object.Instantiate(DewGUI.widgetTextHeader, parent);
            value.text = text;
            // If fontSize is 0 or less, use prefab default (CanvasScaler handles resolution)
            if (fontSize > 0f) value.fontSize = fontSize;
            value.alignment = TMPro.TextAlignmentOptions.Left;
            value.color = color;
        }
        
        private static void CreateBonusRow(Transform parent, string label, string value, float labelSize, float valueSize, Color valueColor)
        {
            // Create horizontal layout for bonus row
            GameObject bonusContainer = new GameObject("BonusRow");
            bonusContainer.transform.SetParent(parent, false);
            HorizontalLayoutGroup bonusLayout = bonusContainer.AddComponent<HorizontalLayoutGroup>();
            bonusLayout.spacing = 15f; // Fixed spacing for 1440p reference
            bonusLayout.childAlignment = TextAnchor.MiddleCenter;
            bonusLayout.childForceExpandWidth = false;
            bonusLayout.childForceExpandHeight = false;
            
            // Label
            TMPro.TextMeshProUGUI bonusLabel = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, bonusLayout.transform);
            bonusLabel.text = label + ":";
            // If labelSize is 0 or less, use prefab default
            if (labelSize > 0f) bonusLabel.fontSize = labelSize;
            bonusLabel.alignment = TMPro.TextAlignmentOptions.Right;
            bonusLabel.color = new Color(0.7f, 0.7f, 0.8f);
            
            // Value
            TMPro.TextMeshProUGUI bonusValue = UnityEngine.Object.Instantiate(DewGUI.widgetTextHeader, bonusLayout.transform);
            bonusValue.text = value;
            // If valueSize is 0 or less, use prefab default
            if (valueSize > 0f) bonusValue.fontSize = valueSize;
            bonusValue.alignment = TMPro.TextAlignmentOptions.Left;
            bonusValue.color = valueColor;
        }
        
        /// <summary>
        /// Calculate score bonus from Infinite Dungeon stats.
        /// This is added to the game's Total Score display.
        /// </summary>
        private static long CalculateScoreBonus(int depth, int infectedNodes, int cursesEndured, int weakenedBosses)
        {
            long score = 0;
            
            // Depth bonus (main contribution)
            score += depth * 1000L;                    // 1000 per depth
            score += (depth / 10) * 5000L;             // 5000 bonus every 10 depths
            
            // Deep run bonus
            if (depth > 50) score += (depth - 50) * 500L;
            if (depth > 100) score += (depth - 100) * 1000L;
            
            // Hunter system bonuses
            score += infectedNodes * 2000L;            // 2000 per infected node survived
            score += cursesEndured * 500L;             // 500 per curse charge endured
            
            // Weakened boss bonus
            score += weakenedBosses * 10000L;          // 10000 per weakened boss killed
            
            return score;
        }
        
        /// <summary>
        /// Calculate mastery bonus from Infinite Dungeon stats.
        /// This is added to the game's Mastery Points.
        /// KEPT MODEST - should not overshadow base game mastery (typical run = ~100k-150k points)
        /// Our bonus is a small percentage addition, not a replacement.
        /// </summary>
        private static long CalculateMasteryBonus(int depth, int infectedNodes, int cursesEndured)
        {
            long bonus = 0;
            
            // Small bonus per depth (50 points each - depth 50 = 2,500 bonus)
            bonus += depth * 50L;
            
            // Small milestone bonus every 10 depths (200 points)
            bonus += (depth / 10) * 200L;
            
            // Tiny bonus for hunter encounters (25 per infected node)
            bonus += infectedNodes * 25L;
            
            // Tiny bonus for curse survival (10 per curse charge)
            bonus += cursesEndured * 10L;
            
            return bonus;
        }
        
        /// <summary>
        /// Get the score bonus for external use (e.g., by the score patch)
        /// </summary>
        public static long GetScoreBonus()
        {
            int depth = InfiniteDungeonMod.GetTotalDepth();
            int infectedNodes = InfiniteDungeonMod.GetInfectedNodeCount();
            int cursesEndured = InfiniteDungeonMod.GetCursesEndured();
            int weakenedBosses = InfiniteDungeonMod.GetWeakenedBossesDefeated();
            return CalculateScoreBonus(depth, infectedNodes, cursesEndured, weakenedBosses);
        }
        
        private static string GetSummaryText(DewGameResult.ResultType resultType)
        {
            switch (resultType)
            {
                case DewGameResult.ResultType.PureWhiteDream:
                    return DungeonLocalization.ResultSummaryVictory;
                case DewGameResult.ResultType.GameOver:
                    return DungeonLocalization.ResultSummaryGameOver;
                case DewGameResult.ResultType.Conceded:
                case DewGameResult.ResultType.UnknownFate:
                default:
                    return DungeonLocalization.ResultSummaryConcede;
            }
        }
        
    }
    
    /// <summary>
    /// Patch to show our custom result window when game ends
    /// This runs on ALL clients (host and clients) because UserCode_RpcRegisterResult is the client-side RPC handler
    /// </summary>
    [HarmonyPatch(typeof(GameResultManager), "UserCode_RpcRegisterResult__DewGameResult__Boolean")]
    public static class GameResultShowCustomWindowPatch
    {
        static void Postfix(DewGameResult result, bool didGameEnd)
        {
            if (!InfiniteDungeonMod.IsModActive) return;
            if (!didGameEnd) return;
            if (result == null) return;
            
            try
            {
                // Show our custom result window after a short delay to let the game's result screen appear first
                // This runs independently on each client
                NetworkedManagerBase<GameManager>.instance.StartCoroutine(ShowResultDelayed(result.result));
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in GameResultShowCustomWindowPatch: " + ex.Message);
            }
        }
        
        private static System.Collections.IEnumerator ShowResultDelayed(DewGameResult.ResultType resultType)
        {
            // Wait a moment for the game's result screen to appear
            yield return new WaitForSeconds(0.5f);
            
            InfiniteDungeonResultUI.ShowResultWindow(resultType);
        }
    }
    
    /// <summary>
    /// Patch DewMod.OpenConfigWindow to add scrolling support for mod config windows.
    /// This allows mods with many settings (like Infinite Dungeon) to be scrollable.
    /// </summary>
    [HarmonyPatch(typeof(DewMod), "OpenConfigWindow")]
    public static class ModConfigScrollPatch
    {
        static void Postfix(LoadedModInstance mod)
        {
            // Find the config window that was just created (most recent UI_Window)
            UI_Window[] windows = UnityEngine.Object.FindObjectsOfType<UI_Window>();
            if (windows == null || windows.Length == 0) return;
            
            // Get the most recently created window (should be the config window)
            UI_Window configWindow = null;
            foreach (var w in windows)
            {
                if (w.gameObject.activeInHierarchy && configWindow == null)
                {
                    configWindow = w;
                }
                else if (w.gameObject.activeInHierarchy && w.transform.GetSiblingIndex() > configWindow.transform.GetSiblingIndex())
                {
                    configWindow = w;
                }
            }
            
            if (configWindow == null) return;
            
            try
            {
                // Find the VerticalLayoutGroup in the window (where config widgets are placed)
                VerticalLayoutGroup vlg = configWindow.GetComponentInChildren<VerticalLayoutGroup>();
                if (vlg == null) return;
                
                Transform contentParent = vlg.transform;
                
                // Count how many children (settings) there are
                int childCount = contentParent.childCount;
                
                // Only add scrolling if there are many settings (more than ~15)
                if (childCount < 30) return;
                
                // Create scroll view structure
                // 1. Create viewport
                GameObject viewportObj = new GameObject("Viewport");
                viewportObj.transform.SetParent(configWindow.transform, false);
                
                RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
                viewportRect.anchorMin = new Vector2(0, 0);
                viewportRect.anchorMax = new Vector2(1, 1);
                viewportRect.offsetMin = new Vector2(20, 80); // Leave room for buttons at bottom
                viewportRect.offsetMax = new Vector2(-20, -20);
                
                Image viewportImage = viewportObj.AddComponent<Image>();
                viewportImage.color = new Color(0, 0, 0, 0.01f); // Nearly invisible but needed for masking
                
                Mask viewportMask = viewportObj.AddComponent<Mask>();
                viewportMask.showMaskGraphic = false;
                
                // 2. Create content container
                GameObject contentObj = new GameObject("Content");
                contentObj.transform.SetParent(viewportObj.transform, false);
                
                RectTransform contentRect = contentObj.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
                contentRect.anchoredPosition = Vector2.zero;
                
                // Move the VLG to the content
                vlg.transform.SetParent(contentObj.transform, false);
                RectTransform vlgRect = vlg.GetComponent<RectTransform>();
                if (vlgRect != null)
                {
                    vlgRect.anchorMin = new Vector2(0, 1);
                    vlgRect.anchorMax = new Vector2(1, 1);
                    vlgRect.pivot = new Vector2(0.5f, 1);
                    vlgRect.anchoredPosition = Vector2.zero;
                    vlgRect.sizeDelta = new Vector2(0, vlgRect.sizeDelta.y);
                }
                
                // Add ContentSizeFitter to content
                ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                
                // Also add to VLG
                ContentSizeFitter vlgCsf = vlg.gameObject.GetComponent<ContentSizeFitter>();
                if (vlgCsf == null)
                {
                    vlgCsf = vlg.gameObject.AddComponent<ContentSizeFitter>();
                }
                vlgCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                vlgCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                
                // 3. Add ScrollRect to viewport
                ScrollRect scrollRect = viewportObj.AddComponent<ScrollRect>();
                scrollRect.content = contentRect;
                scrollRect.viewport = viewportRect;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
                scrollRect.scrollSensitivity = 30f;
                
                // 4. Create vertical scrollbar
                GameObject scrollbarObj = new GameObject("Scrollbar Vertical");
                scrollbarObj.transform.SetParent(viewportObj.transform, false);
                
                RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
                scrollbarRect.anchorMin = new Vector2(1, 0);
                scrollbarRect.anchorMax = new Vector2(1, 1);
                scrollbarRect.pivot = new Vector2(1, 0.5f);
                scrollbarRect.sizeDelta = new Vector2(20, 0);
                scrollbarRect.anchoredPosition = new Vector2(10, 0);
                
                Image scrollbarBg = scrollbarObj.AddComponent<Image>();
                scrollbarBg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                
                // Create sliding area
                GameObject slidingAreaObj = new GameObject("Sliding Area");
                slidingAreaObj.transform.SetParent(scrollbarObj.transform, false);
                
                RectTransform slidingAreaRect = slidingAreaObj.AddComponent<RectTransform>();
                slidingAreaRect.anchorMin = Vector2.zero;
                slidingAreaRect.anchorMax = Vector2.one;
                slidingAreaRect.offsetMin = new Vector2(2, 2);
                slidingAreaRect.offsetMax = new Vector2(-2, -2);
                
                // Create handle
                GameObject handleObj = new GameObject("Handle");
                handleObj.transform.SetParent(slidingAreaObj.transform, false);
                
                RectTransform handleRect = handleObj.AddComponent<RectTransform>();
                handleRect.anchorMin = new Vector2(0, 0);
                handleRect.anchorMax = new Vector2(1, 1);
                handleRect.offsetMin = Vector2.zero;
                handleRect.offsetMax = Vector2.zero;
                
                Image handleImage = handleObj.AddComponent<Image>();
                handleImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                
                // Setup Scrollbar component
                Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
                scrollbar.handleRect = handleRect;
                scrollbar.direction = Scrollbar.Direction.BottomToTop;
                scrollbar.targetGraphic = handleImage;
                
                // Set scrollbar colors
                ColorBlock colors = scrollbar.colors;
                colors.normalColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                colors.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                colors.selectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                scrollbar.colors = colors;
                
                // Connect scrollbar to scroll rect
                scrollRect.verticalScrollbar = scrollbar;
                scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                scrollRect.verticalScrollbarSpacing = 5f;
                
                // Move buttons to be after viewport (so they stay at bottom)
                HorizontalLayoutGroup buttonsGroup = configWindow.GetComponentInChildren<HorizontalLayoutGroup>();
                if (buttonsGroup != null)
                {
                    buttonsGroup.transform.SetAsLastSibling();
                }
                
                // Force layout rebuild
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                LayoutRebuilder.ForceRebuildLayoutImmediate(viewportRect);
            }
            catch (Exception)
            {
                // Silently ignore scroll setup errors - not critical
            }
        }
    }
}
