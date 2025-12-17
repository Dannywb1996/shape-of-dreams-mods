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
    public partial class InfiniteDungeonMod
    {
        // ==================== DUNGEON GENERATION ====================
        
        /// <summary>
        /// Wrapper that generates child nodes and THEN tries hunter infection.
        /// This ensures we have nodes to infect!
        /// </summary>
        private IEnumerator GenerateChildNodesAndTryInfect(int parentNodeIndex)
        {
            // First, generate the child nodes
            yield return StartCoroutine(GenerateChildNodes(parentNodeIndex));
            
            // Now try to infect the newly generated adjacent nodes
            ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
            if (zm != null && _dungeonRandom != null && _totalDepth > 0)
            {
                TryInfectAdjacentNodes(zm, parentNodeIndex);
            }
        }
        
        /// <summary>
        /// Generate child nodes branching from the current node in a clean tree pattern
        /// </summary>
        private IEnumerator GenerateChildNodes(int parentNodeIndex)
        {
            // IMPORTANT: Capture the parent node BEFORE any yield!
            // The parentNodeIndex parameter is the correct value at call time
            int parentNode = parentNodeIndex;
            
            // Debug logging removed for performance
            
            // Wait for zones to be loaded if needed
            float waitTime = 0f;
            while (_allZones.Count == 0 && waitTime < 5f)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
                if (waitTime >= 1f && _allZones.Count == 0)
                {
                    // Try to force load zones
                    LoadAllZones();
                }
            }
            
            if (_allZones.Count == 0)
            {
                Debug.LogError("[InfiniteDungeon] No zones loaded after waiting 5s! Attempting emergency load...");
                LoadAllZones();
                if (_allZones.Count == 0)
                {
                    Debug.LogError("[InfiniteDungeon] CRITICAL: Emergency zone load failed!");
                    yield break;
                }
            }
            
            yield return new WaitForSeconds(0.3f); // Small delay to let room settle
            
            if (!NetworkServer.active) yield break;
            
            try
            {
                ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
                if (zm == null)
                {
                    Debug.LogError("[InfiniteDungeon] ZoneManager is null!");
                    yield break;
                }
                
                // Verify the parent node is valid (don't re-read currentNodeIndex!)
                if (parentNode < 0 || parentNode >= zm.nodes.Count)
                {
                    Debug.LogWarning("[InfiniteDungeon] Invalid parent node index: " + parentNode + " (nodes.Count=" + zm.nodes.Count + ")");
                    yield break;
                }
                
                // Don't generate if already has children
                if (_nodesWithChildren.Contains(parentNode))
                {
                    yield break;
                }
                
                // Mark as having children
                _nodesWithChildren.Add(parentNode);
                
                // Get parent position
                Vector2 parentPos;
                if (!_nodePositions.TryGetValue(parentNode, out parentPos))
                {
                    parentPos = zm.nodes[parentNode].position;
                    _nodePositions[parentNode] = parentPos;
                }
                
                // Ensure parent has connection set
                if (!_nodeConnections.ContainsKey(parentNode))
                {
                    _nodeConnections[parentNode] = new HashSet<int>();
                }
                
                // Get parent's "forward direction" (direction we came from, or random for start)
                float parentDirection;
                if (!_nodeDirections.TryGetValue(parentNode, out parentDirection))
                {
                    parentDirection = _dungeonRandom.Range(0f, 360f);
                    _nodeDirections[parentNode] = parentDirection;
                }
                
                // DUNGEON GENERATION - Spread in multiple directions
                // Create branches that go different ways, not all in one direction
                int numBranches;
                if (_dungeonRandom.Value() < DEAD_END_CHANCE && _totalDepth > 5)
                {
                    // Dead end - rare, only after good depth
                    numBranches = 0;
                    
                    // Announce the dead end to players!
                    SendChatMessage(DungeonLocalization.CreateMessageKey("DEAD_END"));
                }
                else
                {
                    // Random number of branches between MIN and MAX
                    numBranches = _dungeonRandom.Range(MIN_BRANCHES, MAX_BRANCHES + 1);
                }
                
                // Debug logging removed for performance
                
                int successfulBranches = 0;
                List<float> usedAngles = new List<float>(); // Track angles already used for branches
                List<int> createdNodes = new List<int>(); // Track created nodes for chaining
                
                // DUNGEON GENERATION - Spread branches in DIFFERENT directions
                // Don't just follow parent direction - go all directions!
                List<float> branchAngles = new List<float>();
                
                // Find which directions are already blocked (by the path we came from)
                float blockedDirection = parentDirection + 180f; // Direction back to parent
                
                if (numBranches == 1)
                {
                    // Single branch - pick a direction that's NOT back where we came from
                    float[] possibleDirections = new float[] { 0f, 90f, -90f, 45f, -45f };
                    float chosen = possibleDirections[_dungeonRandom.Range(0, possibleDirections.Length)];
                    float finalAngle = parentDirection + chosen + _dungeonRandom.Range(-30f, 30f);
                    branchAngles.Add(finalAngle);
                }
                else if (numBranches == 2)
                {
                    // Fork - two paths going in clearly different directions
                    float angle1 = parentDirection + _dungeonRandom.Range(-40f, 40f); // Roughly forward
                    float angle2 = parentDirection + (_dungeonRandom.Value() < 0.5f ? 90f : -90f) + _dungeonRandom.Range(-30f, 30f);
                    branchAngles.Add(angle1);
                    branchAngles.Add(angle2);
                }
                else if (numBranches == 3)
                {
                    // Three-way intersection - spread in 3 different directions
                    branchAngles.Add(parentDirection + _dungeonRandom.Range(-30f, 30f)); // Forward
                    branchAngles.Add(parentDirection + 90f + _dungeonRandom.Range(-25f, 25f)); // Right
                    branchAngles.Add(parentDirection - 90f + _dungeonRandom.Range(-25f, 25f)); // Left
                }
                else if (numBranches >= 4)
                {
                    // Four-way intersection - spread in 4 different directions
                    branchAngles.Add(parentDirection + _dungeonRandom.Range(-20f, 20f)); // Forward
                    branchAngles.Add(parentDirection + 90f + _dungeonRandom.Range(-20f, 20f)); // Right
                    branchAngles.Add(parentDirection - 90f + _dungeonRandom.Range(-20f, 20f)); // Left
                    branchAngles.Add(parentDirection + 45f + _dungeonRandom.Range(-15f, 15f)); // Forward-Right
                }
                
                // Try to create nodes at each pre-calculated angle
                foreach (float targetAngle in branchAngles)
                {
                    Vector2 branchPos = FindValidNodePosition(zm, parentPos, targetAngle, parentNode);
                    
                    if (branchPos != Vector2.zero)
                    {
                        int newNodeIndex = CreateNodeAtPosition(zm, parentNode, branchPos, targetAngle);
                        if (newNodeIndex >= 0)
                        {
                            successfulBranches++;
                            usedAngles.Add(targetAngle);
                            createdNodes.Add(newNodeIndex);
                        }
                    }
                }
                
                // If we couldn't create all branches, try additional angles with proper separation
                int maxAttempts = 10;
                int attempts = 0;
                while (successfulBranches < numBranches && attempts < maxAttempts)
                {
                    attempts++;
                    
                    // Pick a random angle that's well-separated from existing branches
                    float candidateAngle = _dungeonRandom.Range(0f, 360f);
                    
                    // Check if this angle is far enough from all used angles
                    bool angleOk = true;
                    foreach (float usedAngle in usedAngles)
                    {
                        float diff = Mathf.Abs(Mathf.DeltaAngle(candidateAngle, usedAngle));
                        if (diff < MIN_ANGLE_BETWEEN_BRANCHES)
                        {
                            angleOk = false;
                            break;
                        }
                    }
                    
                    if (!angleOk) continue;
                    
                    Vector2 branchPos = FindValidNodePosition(zm, parentPos, candidateAngle, parentNode);
                    
                    if (branchPos != Vector2.zero)
                    {
                        int newNodeIndex = CreateNodeAtPosition(zm, parentNode, branchPos, candidateAngle);
                        if (newNodeIndex >= 0)
                        {
                            successfulBranches++;
                            usedAngles.Add(candidateAngle);
                            createdNodes.Add(newNodeIndex);
                        }
                    }
                }
                
                // ========================================
                // DUNGEON CHAIN GENERATION - ORGANIC WINDING CORRIDORS
                // High chance to extend branches into winding chains
                // This creates dungeon-like corridors that curve and twist!
                // ========================================
                if (createdNodes.Count > 0 && _dungeonRandom.Value() < CHAIN_CHANCE)
                {
                    // Pick 1-2 branches to extend into chains (or all if few branches)
                    int chainsToCreate = _dungeonRandom.Range(1, Mathf.Min(3, createdNodes.Count + 1));
                    
                    // Shuffle created nodes to pick random ones for chaining
                    List<int> shuffledNodes = new List<int>(createdNodes);
                    for (int i = shuffledNodes.Count - 1; i > 0; i--)
                    {
                        int j = _dungeonRandom.Range(0, i + 1);
                        int temp = shuffledNodes[i];
                        shuffledNodes[i] = shuffledNodes[j];
                        shuffledNodes[j] = temp;
                    }
                    
                    for (int c = 0; c < chainsToCreate && c < shuffledNodes.Count; c++)
                    {
                        int chainStartNode = shuffledNodes[c];
                        // Each branch gets its own random node count
                        int chainLength = _dungeonRandom.Range(MIN_NODES_PER_BRANCH, MAX_NODES_PER_BRANCH + 1);
                        
                        // Get the direction this branch was going
                        float chainDirection;
                        if (!_nodeDirections.TryGetValue(chainStartNode, out chainDirection))
                        {
                            chainDirection = _dungeonRandom.Range(0f, 360f);
                        }
                        
                        int currentChainNode = chainStartNode;
                        Vector2 currentChainPos;
                        if (!_nodePositions.TryGetValue(currentChainNode, out currentChainPos))
                        {
                            continue;
                        }
                        
                        // Track cumulative turn to create winding effect
                        float cumulativeTurn = 0f;
                        float turnBias = _dungeonRandom.Value() < 0.5f ? 1f : -1f; // Tend to curve one direction
                        
                        for (int chainIdx = 0; chainIdx < chainLength; chainIdx++)
                        {
                            // WINDING PATH: Each node turns more, creating curves not straight lines
                            float turnAmount = _dungeonRandom.Range(20f, 50f) * turnBias;
                            
                            // Occasionally reverse the turn direction for S-curves
                            if (_dungeonRandom.Value() < 0.3f)
                            {
                                turnBias *= -1f;
                            }
                            
                            cumulativeTurn += turnAmount;
                            float newAngle = chainDirection + cumulativeTurn + _dungeonRandom.Range(-15f, 15f);
                            
                            Vector2 chainNodePos = FindValidNodePosition(zm, currentChainPos, newAngle, currentChainNode);
                            
                            if (chainNodePos == Vector2.zero)
                            {
                                // Try other angles - more attempts for organic paths
                                bool found = false;
                                for (int retry = 0; retry < 8; retry++)
                                {
                                    newAngle = chainDirection + _dungeonRandom.Range(-90f, 90f);
                                    chainNodePos = FindValidNodePosition(zm, currentChainPos, newAngle, currentChainNode);
                                    if (chainNodePos != Vector2.zero)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    break;
                                }
                            }
                            
                            // Create the chain node connected to the PREVIOUS chain node (not parent!)
                            int chainNodeIndex = CreateNodeAtPosition(zm, currentChainNode, chainNodePos, newAngle);
                            if (chainNodeIndex >= 0)
                            {
                                // Mark the previous chain node as having children so it doesn't regenerate
                                _nodesWithChildren.Add(currentChainNode);
                                
                                // Move to next in chain
                                currentChainNode = chainNodeIndex;
                                currentChainPos = chainNodePos;
                                chainDirection = newAngle; // Update base direction
                                successfulBranches++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                
                // Try to create loops/shortcuts to nearby existing nodes
                if (_dungeonRandom.Value() < LOOP_CHANCE && successfulBranches > 0)
                {
                    TryCreateLoopConnection(zm, parentNode, parentPos);
                }
                
                // Emergency fallback if no nodes created
                if (successfulBranches == 0 && numBranches > 0)
                {
                    Debug.LogWarning("[InfiniteDungeon] Failed to create branches, trying emergency generation...");
                    
                    // Try all 8 directions at increasing distances - NO X constraint!
                    for (float dist = MIN_NODE_DISTANCE; dist <= MAX_NODE_DISTANCE * 2; dist += 8f)
                    {
                        for (int angle = 0; angle < 360; angle += 30) // More angles (12 instead of 8)
                        {
                            float rad = angle * Mathf.Deg2Rad;
                            Vector2 targetPos = parentPos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * dist;
                            
                            // Mirror negative X to positive before validation
                            if (targetPos.x < 0f)
                            {
                                targetPos.x = Mathf.Abs(targetPos.x);
                            }
                            
                            // After mirroring, re-check distance from parent
                            float distFromParent = Vector2.Distance(targetPos, parentPos);
                            if (distFromParent < MIN_NODE_DISTANCE) continue;
                            
                            if (!IsPositionValid(zm, targetPos, parentNode)) continue;
                            
                            int newNodeIndex = CreateNodeAtPosition(zm, parentNode, targetPos, angle);
                            if (newNodeIndex >= 0)
                            {
                                successfulBranches++;
                                break;
                            }
                        }
                        if (successfulBranches > 0) break;
                    }
                    
                    // LAST RESORT: If still no nodes, force create at a fixed position
                    if (successfulBranches == 0)
                    {
                        Debug.LogWarning("[InfiniteDungeon] Emergency fallback failed, forcing node creation...");
                        Vector2 forcedPos = parentPos + new Vector2(MIN_NODE_DISTANCE + 5f, 0f); // Force to the right
                        int forcedNode = CreateNodeAtPosition(zm, parentNode, forcedPos, 0f, skipValidation: true);
                        if (forcedNode >= 0)
                        {
                            successfulBranches++;
                        }
                    }
                }
                
                if (successfulBranches == 0 && numBranches > 0)
                {
                    Debug.LogError("[InfiniteDungeon] CRITICAL: Could not create ANY child nodes from node " + parentNode + " at pos " + parentPos + "!");
                    _nodesWithChildren.Remove(parentNode);
                    
                    // Announce dead end
                    SendChatMessage(DungeonLocalization.CreateMessageKey("DEAD_END"));
                }
                else if (successfulBranches > 0)
                {
                    // Force refresh the map UI to show the new nodes immediately
                    MapPanningSystem.ForceRefreshMap();
                }
                // Note: We don't announce new paths - only shortcuts are announced
                // This keeps the chat cleaner and shortcuts feel more special
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error generating child nodes: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Convert world position to grid coordinates (legacy, uses MIN_NODE_DISTANCE as cell size)
        /// </summary>
        private Vector2Int WorldToGrid(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPos.x / MIN_NODE_DISTANCE),
                Mathf.RoundToInt(worldPos.y / MIN_NODE_DISTANCE)
            );
        }
        
        /// <summary>
        /// Convert grid coordinates to world position (legacy, uses MIN_NODE_DISTANCE as cell size)
        /// </summary>
        private Vector2 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = _dungeonRandom != null ? _dungeonRandom.Range(-3f, 3f) : 0f;
            float offsetY = _dungeonRandom != null ? _dungeonRandom.Range(-3f, 3f) : 0f;
            return new Vector2(
                gridPos.x * MIN_NODE_DISTANCE + offsetX,
                gridPos.y * MIN_NODE_DISTANCE + offsetY
            );
        }
        
        /// <summary>
        /// Shuffle an array in place (Fisher-Yates)
        /// </summary>
        private void ShuffleArray(int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = _dungeonRandom.Range(0, i + 1);
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
        
        /// <summary>
        /// Find a valid position for a new node at the given angle from parent
        /// Returns Vector2.zero if no valid position found
        /// </summary>
        private Vector2 FindValidNodePosition(ZoneManager zm, Vector2 parentPos, float angle, int parentNodeIndex)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            
            // Try multiple distances
            for (int attempt = 0; attempt < 5; attempt++)
            {
                float distance = _dungeonRandom.Range(MIN_NODE_DISTANCE, MAX_NODE_DISTANCE);
                Vector2 targetPos = parentPos + direction * distance;
                
                // Add some perpendicular variance for organic feel
                float perpOffset = _dungeonRandom.Range(-5f, 5f);
                Vector2 perpDir = new Vector2(-direction.y, direction.x);
                targetPos += perpDir * perpOffset;
                
                // CRITICAL: Mirror negative X to positive BEFORE validation!
                // This prevents the mirroring in CreateNodeAtPosition from placing nodes on top of each other
                if (targetPos.x < 0f)
                {
                    targetPos.x = Mathf.Abs(targetPos.x);
                }
                
                // CRITICAL: After mirroring, re-check distance from parent!
                // Mirroring can cause the position to end up much closer to the parent
                float distFromParent = Vector2.Distance(targetPos, parentPos);
                if (distFromParent < MIN_NODE_DISTANCE)
                {
                    continue; // Too close to parent after mirroring, try again
                }
                
                // Check if position is valid (with the final X coordinate)
                if (IsPositionValid(zm, targetPos, parentNodeIndex))
                {
                    return targetPos;
                }
            }
            
            return Vector2.zero;
        }
        
        /// <summary>
        /// Check if a position is valid for placing a new node
        /// </summary>
        private bool IsPositionValid(ZoneManager zm, Vector2 pos, int parentNodeIndex)
        {
            // Check distance from all existing nodes
            foreach (var kvp in _nodePositions)
            {
                if (kvp.Key == parentNodeIndex) continue;
                
                float dist = Vector2.Distance(pos, kvp.Value);
                if (dist < NODE_SAFE_RADIUS)
                {
                    return false;
                }
            }
            
            // Also check against nodes in ZoneManager that we might not have tracked
            for (int i = 0; i < zm.nodes.Count; i++)
            {
                if (i == parentNodeIndex) continue;
                float dist = Vector2.Distance(pos, zm.nodes[i].position);
                if (dist < NODE_SAFE_RADIUS)
                {
                    return false;
                }
            }
            
            // CRITICAL: Check if this position is too close to ANY existing path
            // This prevents nodes from being placed on top of connection lines
            if (IsPositionTooCloseToExistingPaths(pos, parentNodeIndex))
            {
                return false;
            }
            
            // Check if the path to this node would cross existing paths
            Vector2 parentPos = _nodePositions.ContainsKey(parentNodeIndex) ? _nodePositions[parentNodeIndex] : zm.nodes[parentNodeIndex].position;
            if (DoesPathCrossExistingPaths(zm, parentPos, pos, parentNodeIndex))
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if a position is too close to any existing connection path
        /// </summary>
        private bool IsPositionTooCloseToExistingPaths(Vector2 pos, int excludeParentNode)
        {
            float pathClearance = PATH_CLEARANCE; // Use the constant defined at the top
            
            foreach (var kvp in _nodeConnections)
            {
                int nodeA = kvp.Key;
                
                // Get position of nodeA
                Vector2 posA;
                if (!_nodePositions.TryGetValue(nodeA, out posA)) continue;
                
                foreach (int nodeB in kvp.Value)
                {
                    // Only check each connection once (nodeA < nodeB)
                    if (nodeB <= nodeA) continue;
                    
                    // Skip connections involving the parent node (we're connecting to it)
                    if (nodeA == excludeParentNode || nodeB == excludeParentNode) continue;
                    
                    // Get position of nodeB
                    Vector2 posB;
                    if (!_nodePositions.TryGetValue(nodeB, out posB)) continue;
                    
                    // Calculate distance from pos to the line segment (posA -> posB)
                    float distToPath = DistancePointToLineSegment(pos, posA, posB);
                    
                    if (distToPath < pathClearance)
                    {
                        return true; // Too close to this path
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a new path would cross any existing paths
        /// </summary>
        private bool DoesPathCrossExistingPaths(ZoneManager zm, Vector2 start, Vector2 end, int parentNodeIndex)
        {
            foreach (var kvp in _nodeConnections)
            {
                int nodeA = kvp.Key;
                foreach (int nodeB in kvp.Value)
                {
                    // Skip if this involves the parent node
                    if (nodeA == parentNodeIndex || nodeB == parentNodeIndex) continue;
                    // Only check each pair once
                    if (nodeB < nodeA) continue;
                    
                    Vector2 posA, posB;
                    if (!_nodePositions.TryGetValue(nodeA, out posA)) continue;
                    if (!_nodePositions.TryGetValue(nodeB, out posB)) continue;
                    
                    if (DoLinesIntersect(start, end, posA, posB))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// Create a node at a specific world position (organic generation)
        /// </summary>
        private int CreateNodeAtPosition(ZoneManager zm, int parentNodeIndex, Vector2 worldPos, float direction, bool skipValidation = false)
        {
            try
            {
                // Safety checks
                if (_dungeonRandom == null)
                {
                    _dungeonRandom = new DewRandom(DewRandom.GetRandomSeed());
                }
                
                if (_allZones == null || _allZones.Count == 0)
                {
                    LoadAllZones();
                }
                
                if (_allZones.Count == 0)
                {
                    Debug.LogError("[InfiniteDungeon] No zones available!");
                    return -1;
                }
                
                // CRITICAL: Ensure X >= 0 because IsSidetrackNode() returns true for negative X!
                // This was causing nodes to be filtered out of the map display
                if (worldPos.x < 0f)
                {
                    worldPos.x = Mathf.Abs(worldPos.x); // Mirror to positive side
                }
                
                // Final validation (skip if forced creation)
                if (!skipValidation && !IsPositionValid(zm, worldPos, parentNodeIndex))
                {
                    return -1;
                }
                
                // Determine node type FIRST
                WorldNodeType actualNodeType = DetermineNodeType(zm);
                
                // Pick zone based on node type
                Zone randomZone;
                if (actualNodeType == WorldNodeType.ExitBoss)
                {
                    // For boss nodes, only pick from zones that CAN have bosses
                    // This ensures equal chance for all bosses!
                    List<Zone> bossZones = new List<Zone>();
                    foreach (Zone z in _allZones)
                    {
                        if (CanZoneHaveBossNode(z) && z.bossRooms != null && z.bossRooms.Count > 0)
                        {
                            bossZones.Add(z);
                        }
                    }
                    
                    if (bossZones.Count > 0)
                    {
                        randomZone = bossZones[_dungeonRandom.Range(0, bossZones.Count)];
                    }
                    else
                    {
                        // Fallback to any zone if no boss zones available
                        randomZone = _allZones[_dungeonRandom.Range(0, _allZones.Count)];
                        Debug.LogWarning("[InfiniteDungeon] No boss zones available, falling back to random zone");
                    }
                }
                else
                {
                    // For non-boss nodes, pick any zone
                    randomZone = _allZones[_dungeonRandom.Range(0, _allZones.Count)];
                }
                
                // Get a room from the selected zone
                WorldNodeType roomNodeType;
                string roomName = GetRandomRoomFromZone(randomZone, actualNodeType, out roomNodeType);
                if (string.IsNullOrEmpty(roomName))
                {
                    Debug.LogWarning("[InfiniteDungeon] Could not get room from zone: " + randomZone.name);
                    return -1;
                }
                
                // Generate modifiers for this node
                List<ModifierData> modifiers = GenerateRandomModifiers(roomNodeType);
                
                // Create the node with the room already set
                int newNodeIndex = zm.nodes.Count;
                
                WorldNodeData newNode = new WorldNodeData
                {
                    room = roomName, // SET THE ROOM from the node's zone!
                    type = roomNodeType,
                    status = WorldNodeStatus.Revealed,
                    position = worldPos,
                    roomRotValue = _dungeonRandom.Range(0f, 360f),
                    roomRotIndex = 0,
                    modifiers = modifiers
                };
                
                // Add node to ZoneManager lists
                zm.nodes.Add(newNode);
                zm.hunterStatuses.Add(HunterStatus.None);
                zm.visitedNodesSaveData.Add(null);
                
                // If Limbo mode is active, add the Limbo decorator to non-boss nodes
                // This matches the game's behavior in GameMod_Limbo.OnWorldGenerated()
                if (roomNodeType != WorldNodeType.ExitBoss && IsLimboModeActive())
                {
                    zm.AddModifier<RoomMod_Limbo_Decorator>(newNodeIndex);
                }
                
                // Track in our data structures
                _nodePositions[newNodeIndex] = worldPos;
                _nodeDirections[newNodeIndex] = direction;
                _nodeZoneMap[newNodeIndex] = randomZone;
                
                // Create bidirectional connection
                if (!_nodeConnections.ContainsKey(newNodeIndex))
                {
                    _nodeConnections[newNodeIndex] = new HashSet<int>();
                }
                if (!_nodeConnections.ContainsKey(parentNodeIndex))
                {
                    _nodeConnections[parentNodeIndex] = new HashSet<int>();
                }
                
                _nodeConnections[newNodeIndex].Add(parentNodeIndex);
                _nodeConnections[parentNodeIndex].Add(newNodeIndex);
                
                // Debug logging removed for performance
                
                // Rebuild distance matrix
                RebuildDistanceMatrix(zm);
                
                return newNodeIndex;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error creating node: " + ex.Message);
                return -1;
            }
        }
        
        /// <summary>
        /// Try to create a loop connection to a nearby existing node
        /// </summary>
        private void TryCreateLoopConnection(ZoneManager zm, int nodeIndex, Vector2 nodePos)
        {
            try
            {
                float loopMaxDist = MAX_NODE_DISTANCE * 1.5f;
                
                foreach (var kvp in _nodePositions)
                {
                    int otherNode = kvp.Key;
                    
                    // Skip self and already connected nodes
                    if (otherNode == nodeIndex) continue;
                    if (_nodeConnections.ContainsKey(nodeIndex) && _nodeConnections[nodeIndex].Contains(otherNode)) continue;
                    
                    float dist = Vector2.Distance(nodePos, kvp.Value);
                    
                    // Only connect to nearby nodes
                    if (dist > loopMaxDist) continue;
                    if (dist < NODE_SAFE_RADIUS) continue;
                    
                    // Check if path would cross existing paths
                    if (DoesPathCrossExistingPaths(zm, nodePos, kvp.Value, nodeIndex)) continue;
                    
                    // CRITICAL: Check if path would pass through any other node
                    if (DoesPathPassThroughNode(zm, nodePos, kvp.Value, nodeIndex, otherNode)) continue;
                    
                    // Create the loop connection
                    if (!_nodeConnections.ContainsKey(otherNode))
                    {
                        _nodeConnections[otherNode] = new HashSet<int>();
                    }
                    
                    _nodeConnections[nodeIndex].Add(otherNode);
                    _nodeConnections[otherNode].Add(nodeIndex);
                    
                    RebuildDistanceMatrix(zm);
                    
                    // Announce the shortcut with dream lore
                    SendChatMessage(DungeonLocalization.CreateMessageKey("SHORTCUT"));
                    
                    break; // Only one loop per node
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error creating loop connection: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Check if a path between two points would pass too close to any other node
        /// This prevents loop connections from visually crossing over nodes
        /// </summary>
        private bool DoesPathPassThroughNode(ZoneManager zm, Vector2 start, Vector2 end, int startNode, int endNode)
        {
            float nodeAvoidanceRadius = 12f; // Path must be at least this far from any node
            
            foreach (var kvp in _nodePositions)
            {
                int checkNode = kvp.Key;
                
                // Skip the start and end nodes of the path
                if (checkNode == startNode || checkNode == endNode) continue;
                
                Vector2 nodePos = kvp.Value;
                
                // Calculate distance from this node to the line segment (start -> end)
                float distToLine = DistancePointToLineSegment(nodePos, start, end);
                
                if (distToLine < nodeAvoidanceRadius)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Determine what type of node to create based on depth and randomness
        /// Uses configurable chances from mod settings
        /// </summary>
        private WorldNodeType DetermineNodeType(ZoneManager zm)
        {
            // Get config values (percentages)
            var config = InfiniteDungeonMod.Instance != null ? InfiniteDungeonMod.Instance.config : null;
            float bossChance = config != null ? config.bossNodeChance / 100f : 0.03f;
            float merchantChance = config != null ? config.merchantNodeChance / 100f : 0.15f;
            float eventChance = config != null ? config.eventNodeChance / 100f : 0.15f;
            // combatChance is the remainder (default case)
            
            // Random roll for node type
            float roll = _dungeonRandom != null ? _dungeonRandom.Value() : 0.5f;
            
            // Check in order: Boss -> Merchant -> Event -> Combat (default)
            float cumulative = 0f;
            
            // Boss chance
            cumulative += bossChance;
            if (roll < cumulative)
            {
                return WorldNodeType.ExitBoss;
            }
            
            // Merchant chance
            cumulative += merchantChance;
            if (roll < cumulative)
            {
                return WorldNodeType.Merchant;
            }
            
            // Event chance
            cumulative += eventChance;
            if (roll < cumulative)
            {
                return WorldNodeType.Event;
            }
            
            // Default to combat
            return WorldNodeType.Combat;
        }
        
        /// <summary>
        /// Create a node at a specific grid position (legacy method, kept for compatibility)
        /// </summary>
        private int CreateNodeAtGrid(ZoneManager zm, int parentNodeIndex, Vector2Int gridPos, Vector2 worldPos)
        {
            try
            {
                // Safety checks
                if (_dungeonRandom == null)
                {
                    _dungeonRandom = new DewRandom(DewRandom.GetRandomSeed());
                }
                
                if (_allZones == null || _allZones.Count == 0)
                {
                    LoadAllZones();
                }
                
                if (_allZones.Count == 0)
                {
                    Debug.LogError("[InfiniteDungeon] No zones available!");
                    return -1;
                }
                
                // CRITICAL: Ensure X >= 0 because IsSidetrackNode() returns true for negative X!
                if (worldPos.x < 0f)
                {
                    worldPos.x = Mathf.Abs(worldPos.x);
                }
                
                // BULLETPROOF CHECK: Verify no existing node is too close to this position
                // This catches cases where grid tracking got out of sync
                float minDistSq = 20f * 20f; // Minimum 20 units between nodes
                for (int i = 0; i < zm.nodes.Count; i++)
                {
                    Vector2 existingPos = zm.nodes[i].position;
                    float distSq = (existingPos - worldPos).sqrMagnitude;
                    if (distSq < minDistSq)
                    {
                        return -1;
                    }
                }
                
                // BULLETPROOF CHECK: Verify this position doesn't intersect any existing connection line
                if (DoesPositionIntersectAnyPath(zm, worldPos, parentNodeIndex))
                {
                    return -1;
                }
                
                // BULLETPROOF CHECK: Verify the NEW connection line won't cross any existing paths
                Vector2 parentPos = zm.nodes[parentNodeIndex].position;
                if (DoesNewLineCrossExistingPaths(zm, parentPos, worldPos, parentNodeIndex))
                {
                    return -1;
                }
                
                // Mark grid cell as occupied
                string gridKey = gridPos.x + "," + gridPos.y;
                _occupiedGridCells.Add(gridKey);
                
                // Pick a random zone
                Zone randomZone = _allZones[_dungeonRandom.Range(0, _allZones.Count)];
                if (randomZone == null)
                {
                    LoadAllZones();
                    randomZone = _allZones[_dungeonRandom.Range(0, _allZones.Count)];
                }
                
                // Determine node type
                WorldNodeType desiredNodeType = DetermineNodeType();
                WorldNodeType actualNodeType;
                string roomName = GetRandomRoomFromZone(randomZone, desiredNodeType, out actualNodeType);
                
                if (string.IsNullOrEmpty(roomName))
                {
                    Debug.LogWarning("[InfiniteDungeon] Could not get room from zone: " + randomZone.name);
                    _occupiedGridCells.Remove(gridKey); // Undo occupation
                    return -1;
                }
                
                int newNodeIndex = zm.nodes.Count;
                
                // Generate modifiers
                List<ModifierData> modifiers = GenerateRandomModifiers(actualNodeType);
                
                WorldNodeData newNode = new WorldNodeData
                {
                    room = roomName,
                    type = actualNodeType,
                    status = WorldNodeStatus.Revealed,
                    position = worldPos,
                    roomRotValue = _dungeonRandom.Range(0f, 360f),
                    roomRotIndex = 0,
                    modifiers = modifiers
                };
                
                // Add to ZoneManager
                zm.nodes.Add(newNode);
                zm.hunterStatuses.Add(HunterStatus.None);
                zm.visitedNodesSaveData.Add(null);
                
                // Track in our data structures
                _nodePositions[newNodeIndex] = worldPos;
                _nodeDirections[newNodeIndex] = 0f;
                _nodeZoneMap[newNodeIndex] = randomZone;
                
                // Create bidirectional connection
                if (!_nodeConnections.ContainsKey(newNodeIndex))
                {
                    _nodeConnections[newNodeIndex] = new HashSet<int>();
                }
                if (!_nodeConnections.ContainsKey(parentNodeIndex))
                {
                    _nodeConnections[parentNodeIndex] = new HashSet<int>();
                }
                
                _nodeConnections[newNodeIndex].Add(parentNodeIndex);
                _nodeConnections[parentNodeIndex].Add(newNodeIndex);
                
                // Rebuild distance matrix
                RebuildDistanceMatrix(zm);
                
                return newNodeIndex;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error creating node at grid: " + ex.Message);
                return -1;
            }
        }
        
        // Old methods removed - now using grid-based generation
        
        /// <summary>
        /// Create a single node at the specified position, connected only to its parent
        /// (Legacy method kept for compatibility)
        /// </summary>
        private int CreateNode(ZoneManager zm, int parentNodeIndex, Vector2 worldPos, float direction)
        {
            try
            {
                // Safety: ensure zones are loaded AND valid (not destroyed Unity objects)
                bool needsReload = _allZones.Count == 0;
                if (!needsReload && _allZones.Count > 0 && _allZones[0] == null)
                {
                    needsReload = true;
                }
                
                if (needsReload)
                {
                    Debug.LogWarning("[InfiniteDungeon] Zones invalid or empty, reloading...");
                    LoadAllZones();
                    if (_allZones.Count == 0)
                    {
                        Debug.LogError("[InfiniteDungeon] Still no zones after reload!");
                        return -1;
                    }
                }
                
                if (_dungeonRandom == null)
                {
                    Debug.LogWarning("[InfiniteDungeon] _dungeonRandom is null, initializing...");
                    _dungeonRandom = new DewRandom(DewRandom.GetRandomSeed());
                }
                
                // CRITICAL: Ensure X >= 0 because IsSidetrackNode() returns true for negative X!
                if (worldPos.x < 0f)
                {
                    worldPos.x = Mathf.Abs(worldPos.x);
                }
                
                // Pick a random zone for this node
                Zone randomZone = _allZones[_dungeonRandom.Range(0, _allZones.Count)];
                
                // Double-check the zone is valid (Unity object not destroyed)
                if (randomZone == null)
                {
                    Debug.LogWarning("[InfiniteDungeon] Selected zone was null, reloading zones...");
                    LoadAllZones();
                    if (_allZones.Count == 0)
                    {
                        Debug.LogError("[InfiniteDungeon] No zones available after reload!");
                        return -1;
                    }
                    randomZone = _allZones[_dungeonRandom.Range(0, _allZones.Count)];
                }
                
                // Determine node type
                WorldNodeType desiredNodeType = DetermineNodeType();
                
                // Pick a random room from that zone based on node type
                WorldNodeType actualNodeType;
                string roomName = GetRandomRoomFromZone(randomZone, desiredNodeType, out actualNodeType);
                if (string.IsNullOrEmpty(roomName))
                {
                    Debug.LogWarning("[InfiniteDungeon] Could not get room from zone: " + randomZone.name);
                    return -1;
                }
                
                // Create the new node
                int newNodeIndex = zm.nodes.Count;
                
                // Generate random room modifiers for variety (wells, shrines, smoothie merchant, etc.)
                List<ModifierData> modifiers = GenerateRandomModifiers(actualNodeType);
                
                WorldNodeData newNode = new WorldNodeData
                {
                    room = roomName,
                    type = actualNodeType,
                    status = WorldNodeStatus.Revealed,
                    position = worldPos,
                    roomRotValue = _dungeonRandom.Range(0f, 360f),
                    roomRotIndex = 0,
                    modifiers = modifiers
                };
                
                // Add node to ZoneManager lists
                zm.nodes.Add(newNode);
                zm.hunterStatuses.Add(HunterStatus.None);
                zm.visitedNodesSaveData.Add(null);
                
                // Track in our data structures
                _nodePositions[newNodeIndex] = worldPos;
                _nodeDirections[newNodeIndex] = direction;
                _nodeZoneMap[newNodeIndex] = randomZone;
                
                // Create bidirectional connection (ONLY to parent)
                if (!_nodeConnections.ContainsKey(newNodeIndex))
                {
                    _nodeConnections[newNodeIndex] = new HashSet<int>();
                }
                if (!_nodeConnections.ContainsKey(parentNodeIndex))
                {
                    _nodeConnections[parentNodeIndex] = new HashSet<int>();
                }
                
                _nodeConnections[newNodeIndex].Add(parentNodeIndex);
                _nodeConnections[parentNodeIndex].Add(newNodeIndex);
                
                // Debug logging removed for performance
                
                // Check for proximity connections - connect to nearby existing nodes
                AddProximityConnections(zm, newNodeIndex, worldPos, parentNodeIndex);
                
                // Rebuild distance matrix
                RebuildDistanceMatrix(zm);
                
                return newNodeIndex;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error creating node: " + ex.Message);
                return -1;
            }
        }
        
        /// <summary>
        /// Add connections to nearby existing nodes (proximity connections)
        /// Now handled by TryCreateLoopConnection in the organic generation system
        /// This method is kept for legacy compatibility but does nothing
        /// </summary>
        private void AddProximityConnections(ZoneManager zm, int newNodeIndex, Vector2 newNodePos, int parentNodeIndex)
        {
            // Proximity connections are now handled by TryCreateLoopConnection
            // This method is kept for compatibility with the legacy CreateNodeAtGrid method
        }
        
        /// <summary>
        /// Static wrapper for GenerateRandomModifiers - used by patches
        /// </summary>
        public static List<ModifierData> GenerateRandomModifiersStatic(WorldNodeType nodeType)
        {
            if (Instance != null)
            {
                return Instance.GenerateRandomModifiers(nodeType);
            }
            return new List<ModifierData>();
        }
        
        /// <summary>
        /// Generate random room modifiers for a node (wells, shrines, smoothie merchant, etc.)
        /// Combat and Boss rooms get modifiers. Start and Merchant are safe (no modifiers).
        /// 50% chance for plain room, 50% chance for modifiers. Each modifier has 5% chance and can stack.
        /// </summary>
        private List<ModifierData> GenerateRandomModifiers(WorldNodeType nodeType)
        {
            List<ModifierData> modifiers = new List<ModifierData>();
            
            // SAFE ROOMS - No modifiers at all!
            // BOSS ROOMS: No random modifiers - bosses are placed in the room scene
            // This MUST be checked first to prevent modifiers like RoomMod_Anitya from spawning
            if (nodeType == WorldNodeType.ExitBoss)
            {
                return modifiers;
            }
            
            // Start and Merchant rooms are safe
            if (nodeType == WorldNodeType.Start || nodeType == WorldNodeType.Merchant)
            {
                return modifiers; // No modifiers for safe rooms
            }
            
            // Only Combat rooms get modifiers (Boss already handled above)
            if (nodeType != WorldNodeType.Combat)
            {
                return modifiers; // Event, Quest, Special rooms - no modifiers
            }
            
            // Null check for random
            if (_dungeonRandom == null)
            {
                Debug.LogWarning("[InfiniteDungeon] _dungeonRandom is null in GenerateRandomModifiers");
                return modifiers;
            }
            
            // Get config values for modifier weights (used as relative weights, not independent chances)
            var config = Instance != null ? Instance.config : null;
            int plainRoomWeight = config != null ? config.plainRoomChance : 50;
            int wellWeight = config != null ? config.wellChance : 15;
            int miniBossWeight = config != null ? config.miniBossChance : 20;
            int hiddenStashWeight = config != null ? config.hiddenStashChance : 15;
            int otherModifierWeight = config != null ? config.otherModifierChance : 5;
            
            // Get ZoneManager for registering modifiers
            ZoneManager zm = NetworkedManagerBase<ZoneManager>.instance;
            
            // ========== WEIGHTED SELECTION SYSTEM ==========
            // This ensures the configured percentages are ACTUAL spawn rates!
            // Example: If well=20, miniboss=20, plain=50, hiddenStash=10
            // Total = 100, so well has exactly 20% chance, etc.
            
            // Build weighted modifier pool
            List<WeightedModifier> modifierPool = new List<WeightedModifier>();
            
            // Plain room (no modifiers)
            modifierPool.Add(new WeightedModifier(null, plainRoomWeight));
            
            // High priority modifiers with their configured weights
            modifierPool.Add(new WeightedModifier("RoomMod_SpawnWell", wellWeight));
            modifierPool.Add(new WeightedModifier("RoomMod_SpawnMiniBoss", miniBossWeight));
            modifierPool.Add(new WeightedModifier("RoomMod_SpawnHiddenStash", hiddenStashWeight));
            
            // Beneficial modifiers - share the "other" weight
            string[] beneficialModifiers = new string[]
            {
                "RoomMod_SpawnAltarOfCleansing",
                "RoomMod_SpawnBlessedGuidance",
                "RoomMod_SpawnAscension",
                "RoomMod_SpawnGuidance",
                "RoomMod_SpawnParadox",
                "RoomMod_SpawnEntanglement",
                "RoomMod_SpawnMirrorOfRemorse",
                "RoomMod_GiftMerchant",
                "RoomMod_Artifact",
                "RoomMod_GoldEverywhere",
                "RoomMod_StardustEverywhere",
                "RoomMod_PureDream",
                "RoomMod_DistantMemories",
                "RoomMod_LingeringAuraOfGuidance",
                "RoomMod_StarCookie",
                "RoomMod_FragmentOfRadiance_StartProp",
            };
            
            // Each beneficial modifier gets equal share of otherModifierWeight
            // e.g., if other=5 and there are 16 beneficial modifiers, each gets ~0.3 weight
            foreach (string modName in beneficialModifiers)
            {
                modifierPool.Add(new WeightedModifier(modName, otherModifierWeight));
            }
            
            // Challenging modifiers - 60% of the other weight
            string[] challengingModifiers = new string[]
            {
                "RoomMod_SpawnMawOfDoom",
                "RoomMod_SpawnPotOfGreed",
                "RoomMod_SpawnDisintegration",
                "RoomMod_SpawnHatred",
                "RoomMod_HarderFightBetterReward",
                "RoomMod_AcceleratedTime",
                "RoomMod_Ambush",
                "RoomMod_InversionSigil",
                "RoomMod_UnstableVeilOfTime",
                "RoomMod_WarpingField",
            };
            
            int challengingWeight = (int)(otherModifierWeight * 0.6f);
            foreach (string modName in challengingModifiers)
            {
                modifierPool.Add(new WeightedModifier(modName, challengingWeight));
            }
            
            // Zone-specific modifiers - 40% of other weight
            string[] zoneModifiers = new string[]
            {
                "RoomMod_VeilOfDark",
                "RoomMod_DarkCondensationZone",
                "RoomMod_SymbioteHabitat",
                "RoomMod_ToxicArea",
                "RoomMod_UnstableRatSwarm",
                "RoomMod_EngulfedInFlame",
                "RoomMod_LeafPuppies",
                "RoomMod_BlackRain",
                "RoomMod_InkStrikeWarning",
                "RoomMod_SmallSoul",
                "RoomMod_FireDevil",
                "RoomMod_MeteoricLife",
                "RoomMod_RiskOfMeteors",
                "RoomMod_GazeOfErebos",
                "RoomMod_GravityTraining",
                "RoomMod_ArcticTerritory",
            };
            
            // Zone modifiers have 40% of the base chance
            // ========== WEIGHTED SELECTION FOR PRIMARY MODIFIER ==========
            // This ensures high-priority modifiers (Well, MiniBoss, HiddenStash) have fair distribution
            // Calculate total weight for primary selection
            int totalWeight = 0;
            foreach (var wm in modifierPool)
            {
                totalWeight += wm.weight;
            }
            
            if (totalWeight <= 0)
            {
                return modifiers; // No valid modifiers
            }
            
            // Roll for primary modifier using cumulative ranges
            int roll = _dungeonRandom.Range(0, totalWeight);
            int cumulative = 0;
            WeightedModifier selected = modifierPool[0];
            foreach (var wm in modifierPool)
            {
                cumulative += wm.weight;
                if (roll < cumulative)
                {
                    selected = wm;
                    break;
                }
            }
            
            // Add the primary modifier (null = plain room base, but can still get extras!)
            if (selected.modName != null)
            {
                TryAddModifier(zm, selected.modName, modifiers);
            }
            
            // ========== ADDITIONAL MODIFIERS (STACKING) ==========
            // After the primary modifier, roll for additional modifiers independently
            // This allows multiple modifiers per room for more variety!
            
            // Skip additional rolls if we got a plain room AND want to keep it plain sometimes
            bool gotPrimaryModifier = selected.modName != null;
            
            // Zone-specific modifiers - independent rolls for extra variety
            int zoneWeight = (int)(otherModifierWeight * 0.4f);
            float zoneChance = zoneWeight / 100f;
            foreach (string modName in zoneModifiers)
            {
                if (_dungeonRandom.Value() < zoneChance)
                {
                    TryAddModifier(zm, modName, modifiers);
                }
            }
            
            // If we got a primary modifier, also roll for bonus modifiers from other categories
            if (gotPrimaryModifier)
            {
                // Small chance for bonus beneficial modifiers
                float bonusBeneficialChance = otherModifierWeight / 200f; // Half the normal rate
                foreach (string modName in beneficialModifiers)
                {
                    // Skip if already added as primary
                    if (modName == selected.modName) continue;
                    if (_dungeonRandom.Value() < bonusBeneficialChance)
                    {
                        TryAddModifier(zm, modName, modifiers);
                    }
                }
                
                // Small chance for bonus challenging modifiers
                float bonusChallengeChance = (otherModifierWeight * 0.6f) / 200f;
                foreach (string modName in challengingModifiers)
                {
                    if (modName == selected.modName) continue;
                    if (_dungeonRandom.Value() < bonusChallengeChance)
                    {
                        TryAddModifier(zm, modName, modifiers);
                    }
                }
            }
            
            return modifiers;
        }
        
        /// <summary>
        /// Helper struct for weighted modifier selection
        /// </summary>
        private struct WeightedModifier
        {
            public string modName; // null = plain room (no modifier)
            public int weight;
            
            public WeightedModifier(string name, int w)
            {
                modName = name;
                weight = w;
            }
        }
        
        /// <summary>
        /// Try to add a modifier to the list and register it with ZoneManager.
        /// Uses a simplified approach: just create an empty ModifierServerData entry.
        /// The game will instantiate the modifier fresh when the room loads.
        /// </summary>
        private bool TryAddModifier(ZoneManager zm, string modName, List<ModifierData> modifiers)
        {
            if (zm == null) return false;
            
            try
            {
                // Generate a unique modifier ID
                int modId = GenerateUniqueModifierId(zm);
                
                // Track this as OUR modifier
                _ourModifierIds.Add(modId);
                
                // Create an empty server data entry - the game will create a fresh instance
                // when the room loads. No need to serialize the prefab!
                EnsureModifierServerDataExists(zm, modId);
                
                modifiers.Add(new ModifierData 
                { 
                    type = modName, 
                    isForceRevealed = false,
                    id = modId
                });
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error registering modifier " + modName + ": " + ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Generate a unique modifier ID that doesn't conflict with existing ones.
        /// Uses GUID-based random IDs for uniqueness.
        /// </summary>
        private static int GenerateUniqueModifierId(ZoneManager zm)
        {
            // Try to generate a unique random ID
            for (int tries = 0; tries < 16; tries++)
            {
                // Generate a positive 30-bit integer from GUID
                int candidate = Mathf.Abs(Guid.NewGuid().GetHashCode()) & 0x3FFFFFFF;
                
                // Make sure it doesn't conflict with existing IDs
                if (zm == null || !zm.modifierServerData.ContainsKey(candidate))
                {
                    return candidate;
                }
            }
            
            // Fallback: use timestamp-based ID
            int fallback = (int)(DateTime.UtcNow.Ticks & 0x3FFFFFFF);
            if (zm == null || !zm.modifierServerData.ContainsKey(fallback))
            {
                return fallback;
            }
            
            // Last resort: XOR with magic number
            return fallback ^ 0x1A2B3C4D;
        }
        
        /// <summary>
        /// Ensure a ModifierServerData entry exists for the given ID.
        /// Creates an empty entry if it doesn't exist - the game will create
        /// a fresh modifier instance when the room loads.
        /// </summary>
        private static void EnsureModifierServerDataExists(ZoneManager zm, int id)
        {
            if (zm == null) return;
            
            if (!zm.modifierServerData.ContainsKey(id))
            {
                zm.modifierServerData[id] = new ModifierServerData
                {
                    didCreateInstance = false,
                    persistentData = null  // No saved state - game creates fresh instance
                };
            }
        }
        
        /// <summary>
        /// Called by BlockAddModifierPatch to check if we should allow the modifier addition
        /// </summary>
        public static bool ShouldAllowModifierAddition()
        {
            return _allowModifierAddition;
        }
        
        /// <summary>
        /// Check if a modifier ID belongs to our mod
        /// </summary>
        public static bool IsOurModifier(int modId)
        {
            return _ourModifierIds.Contains(modId);
        }
        
        /// <summary>
        /// Try to get the connections for a specific node.
        /// Used by LimitSoulNodeDistancePatch to find nearby nodes.
        /// </summary>
        public static bool TryGetNodeConnections(int nodeIndex, out HashSet<int> connections)
        {
            return _nodeConnections.TryGetValue(nodeIndex, out connections);
        }
        
        /// <summary>
        /// Check if a zone can have boss nodes generated.
        /// Excludes: Primus (final boss - accessed via Pure White Rift only)
        /// Zone_Sky is allowed - Nyx is a valid boss encounter!
        /// </summary>
        private bool CanZoneHaveBossNode(Zone zone)
        {
            if (zone == null) return false;
            if (zone.name == "Zone_Primus") return false;
            // Zone_Sky is allowed - Nyx boss encounter!
            return true;
        }
        
        /// <summary>
        /// Get a random room from a zone based on desired node type.
        /// </summary>
        private string GetRandomRoomFromZone(Zone zone, WorldNodeType desiredNodeType, out WorldNodeType actualNodeType)
        {
            List<string> availableRooms = new List<string>();
            actualNodeType = desiredNodeType;
            
            switch (desiredNodeType)
            {
                case WorldNodeType.ExitBoss:
                    if (CanZoneHaveBossNode(zone) && zone.bossRooms != null && zone.bossRooms.Count > 0)
                    {
                        availableRooms.AddRange(zone.bossRooms);
                        actualNodeType = WorldNodeType.ExitBoss;
                    }
                    else
                    {
                        // Zone doesn't have boss rooms - fall back to Combat type
                        actualNodeType = WorldNodeType.Combat;
                    }
                    break;
                    
                case WorldNodeType.Merchant:
                    if (zone.shopRooms != null && zone.shopRooms.Count > 0)
                    {
                        availableRooms.AddRange(zone.shopRooms);
                        actualNodeType = WorldNodeType.Merchant;
                    }
                    break;
                    
                case WorldNodeType.Event:
                    if (zone.eventRooms != null && zone.eventRooms.Count > 0)
                    {
                        availableRooms.AddRange(zone.eventRooms);
                        actualNodeType = WorldNodeType.Event;
                    }
                    break;
                    
                case WorldNodeType.Combat:
                default:
                    if (zone.combatRooms != null && zone.combatRooms.Count > 0)
                    {
                        availableRooms.AddRange(zone.combatRooms);
                        actualNodeType = WorldNodeType.Combat;
                    }
                    break;
            }
            
            // Fallback to combat rooms
            if (availableRooms.Count == 0 && zone.combatRooms != null && zone.combatRooms.Count > 0)
            {
                availableRooms.AddRange(zone.combatRooms);
                actualNodeType = WorldNodeType.Combat;
            }
            
            // Fallback to general rooms
            if (availableRooms.Count == 0 && zone.rooms != null && zone.rooms.Count > 0)
            {
                availableRooms.AddRange(zone.rooms);
                actualNodeType = WorldNodeType.Combat;
            }
            
            if (availableRooms.Count == 0)
            {
                Debug.LogWarning("[InfiniteDungeon] No available rooms for zone: " + zone.name + ", nodeType: " + desiredNodeType);
                return null;
            }
            
            string selectedRoom = availableRooms[_dungeonRandom.Range(0, availableRooms.Count)];
            return selectedRoom;
        }
        
        /// <summary>
        /// Determine a random node type for the infinite dungeon.
        /// Uses configurable chances from mod settings.
        /// </summary>
        private WorldNodeType DetermineNodeType()
        {
            // Get config values
            var config = Instance != null ? Instance.config : null;
            float combatChance = config != null ? config.combatNodeChance / 100f : 0.67f;
            float merchantChance = config != null ? config.merchantNodeChance / 100f : 0.15f;
            float eventChance = config != null ? config.eventNodeChance / 100f : 0.15f;
            // Boss chance is the remainder
            
            float roll = _dungeonRandom.Value();
            
            float threshold1 = combatChance;
            float threshold2 = threshold1 + merchantChance;
            float threshold3 = threshold2 + eventChance;
            
            if (roll < threshold1) return WorldNodeType.Combat;
            if (roll < threshold2) return WorldNodeType.Merchant;
            if (roll < threshold3) return WorldNodeType.Event;
            return WorldNodeType.ExitBoss;
        }
        
        /// <summary>
        /// Rebuild the entire distance matrix from our tracked connections.
        /// This ensures the map UI draws the correct path lines between nodes.
        /// CRITICAL: Matrix must be exactly size*size elements for IsNodeConnected to work!
        /// </summary>
        private void RebuildDistanceMatrix(ZoneManager zm)
        {
            int size = zm.nodes.Count;
            int expectedMatrixSize = size * size;
            
            // Clear and rebuild from scratch to ensure correct size
            zm.nodeDistanceMatrix.Clear();
            
            int connectionCount = 0;
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (row == col)
                    {
                        zm.nodeDistanceMatrix.Add(0);
                    }
                    else
                    {
                        // Check if connected (check both directions)
                        bool connected = false;
                        
                        HashSet<int> rowConnections;
                        if (_nodeConnections.TryGetValue(row, out rowConnections))
                        {
                            connected = rowConnections.Contains(col);
                        }
                        
                        // Also check reverse direction
                        if (!connected)
                        {
                            HashSet<int> colConnections;
                            if (_nodeConnections.TryGetValue(col, out colConnections))
                            {
                                connected = colConnections.Contains(row);
                            }
                        }
                        
                        zm.nodeDistanceMatrix.Add(connected ? 1 : 10000);
                        
                        if (connected && row < col) connectionCount++;
                    }
                }
            }
            
            // Verify matrix size is correct
            if (zm.nodeDistanceMatrix.Count != expectedMatrixSize)
            {
                Debug.LogError("[InfiniteDungeon] MATRIX SIZE MISMATCH! Expected " + expectedMatrixSize + 
                              " but got " + zm.nodeDistanceMatrix.Count + ". Fixing...");
                
                // Pad with 10000 if too small
                while (zm.nodeDistanceMatrix.Count < expectedMatrixSize)
                {
                    zm.nodeDistanceMatrix.Add(10000);
                }
                
                // Truncate if too large (shouldn't happen but just in case)
                while (zm.nodeDistanceMatrix.Count > expectedMatrixSize)
                {
                    zm.nodeDistanceMatrix.RemoveAt(zm.nodeDistanceMatrix.Count - 1);
                }
            }
            
            // Sync connections to clients using the new persistentSyncedData API
            // This is more reliable than the n² matrix for large node counts
            SyncConnectionsToClients();
        }
        
        /// <summary>
        /// Get the zone associated with a node for monster spawning
        /// </summary>
        public static Zone GetNodeZone(int nodeIndex)
        {
            Zone zone;
            if (_nodeZoneMap.TryGetValue(nodeIndex, out zone))
            {
                return zone;
            }
            return null;
        }
        
        /// <summary>
        /// Check if Limbo mode is currently active
        /// </summary>
        private static bool IsLimboModeActive()
        {
            try
            {
                // Check if GameMod_Limbo is active
                GameMod_Limbo limboMod = Dew.FindActorOfType<GameMod_Limbo>();
                return limboMod != null && !limboMod.IsNullOrInactive();
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Register a zone for a node (called from patches)
    }
}
