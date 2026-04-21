using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;


public class MapGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject mapContainer;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject linePrefab;
    
    [Header("Settings")]
    [SerializeField] private NodeConfigLibrary nodeConfigLibrary;
    [SerializeField] private int horizontalOffset;
    [SerializeField] private int verticalOffset;
    [SerializeField] private int nodesPerFloor;
    [SerializeField] private int floorPerAct;
    
    [Header("Debug")]
    [SerializeField] private int runSeed;
    [SerializeField] private float mapContainerHeightMult;
    [SerializeField] private bool generateOnPlay = false;
    
    private RectTransform _mapContainerRect;
    private float _mapContainerWidth;
    private float _mapContainerHeight;
    private float _nodeIconWidth;
    private float _nodeIconHeight;
    
    [Header("Test")]
    
    
    private MapNodeData[][] nodeGrid;
    private Random mapRng;
    private List<NodeView> _cacheNodeViews = new List<NodeView>();

    private void Awake()
    {
        RunManager.Instance.RegisterMapGenerator(this);
        RunManager.Instance.OnRunStateChange += OnEnterMapState;
    }

    private void Start()
    {
        if (!Application.isPlaying || !generateOnPlay)
            return;

        // GenerateNewMapWithRandomSeed();
    }

    private void OnEnterMapState(RunState runState)
    {
        if (runState == RunState.MapState)
        {
            for (int i = 0; i < _cacheNodeViews.Count; i++)
            {
                var nv =  _cacheNodeViews[i];
                nv.RefreshNodeState();
            }
        }
    }

    public void GenerateNewMapWithRandomSeed()
    {
        runSeed = Guid.NewGuid().GetHashCode();
        GenerateMap();
    }

    public void GenerateNewMapWithSeed(int seed)
    {
        runSeed = seed;
        GenerateMap();
    }

    public void GenerateMap()
    {
        // clear before generate
        ClearGeneratedMap();
        PrepareGenerationState();

        InitGrid();
        ConnectNodes();
        LogOrphanNodes();
        AssignNodeTypes(false);
        BuildRuntimeNodes();
        
        DrawVisualMap();
    }

    private void PrepareGenerationState()
    {
        if (mapContainer == null || nodePrefab == null || linePrefab == null)
        {
            Debug.LogError("MapGenerator is missing one or more references.");
            return;
        }

        _mapContainerRect = mapContainer.GetComponent<RectTransform>();
        if (_mapContainerRect == null)
        {
            Debug.LogError("mapContainer does not have a RectTransform.");
            return;
        }

        mapRng = new Random(SeedUtility.DeriveSeed(runSeed, RngCategory.Map));

        _mapContainerWidth = _mapContainerRect.rect.width;
        _mapContainerHeight = Screen.height * mapContainerHeightMult;
        _mapContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _mapContainerHeight);

        RectTransform nodeRect = nodePrefab.GetComponent<RectTransform>();
        if (nodeRect == null)
        {
            Debug.LogError("nodePrefab does not have a RectTransform.");
            return;
        }

        _nodeIconWidth = nodeRect.rect.width;
        _nodeIconHeight = nodeRect.rect.height;
    }

    public void ClearGeneratedMap()
    {
        if (mapContainer == null)
            return;

        List<GameObject> childrenToDelete = new List<GameObject>();

        foreach (Transform child in mapContainer.transform)
        {
            childrenToDelete.Add(child.gameObject);
        }

        for (int i = 0; i < childrenToDelete.Count; i++)
        {
            if (Application.isPlaying)
            {
                Destroy(childrenToDelete[i]);
            }
            else
            {
                DestroyImmediate(childrenToDelete[i]);
            }
        }

        nodeGrid = null;
    }
    

    private void InitGrid()
    {
        if (nodesPerFloor <= 0 || floorPerAct <= 0) return;
        
        nodeGrid = new MapNodeData[floorPerAct][];
        
        float horizontalGap = (_mapContainerWidth - nodesPerFloor * _nodeIconWidth - 2 * horizontalOffset) / (nodesPerFloor + 1);
        float verticalGap = (_mapContainerHeight - floorPerAct * _nodeIconHeight - 2 * verticalOffset) / (floorPerAct + 1);

        float stepX = horizontalGap + _nodeIconWidth;
        float stepY = verticalGap + _nodeIconHeight;

        float startX = 0 - _mapContainerWidth / 2 + horizontalOffset + _nodeIconWidth / 2 + horizontalGap;
        float startY = 0 - _mapContainerHeight / 2 + verticalOffset  + _nodeIconHeight / 2 + verticalGap;
        
        float currentX = startX;
        float currentY = startY;
        
        // Debug.Log($"mapContainerHeight: {_mapContainerHeight}," +
        //           $"mapContainerWidth: {_mapContainerWidth}" +
        //           $"startX: {startX}," +
        //           $"startY: {startY}," +
        //           $"horizontal Gap: {horizontalGap}," +
        //           $"vertical Gap: {verticalGap}");
        for (int floor = 0; floor < floorPerAct; floor++)
        {
            nodeGrid[floor] = new MapNodeData[nodesPerFloor];
            for (int index = 0; index < nodesPerFloor; index++)
            {
                // GameObject node = Instantiate(nodePrefab, mapContainer.transform);
                // // Debug.Log($"Spawned node {node.name}, activeSelf={node.activeSelf}, activeInHierarchy={node.activeInHierarchy}, parent={node.transform.parent.name}");
                //
                // node.name =  $"Node{floor}_{index}: [{currentX}_{currentY}]";
                //
                // var nodeRectTransform = node.GetComponent<RectTransform>();
                // if (nodeRectTransform != null) nodeRectTransform.anchoredPosition = new Vector2(currentX, currentY);
                //
                // mapnodedata
                nodeGrid[floor][index] = new MapNodeData(0,
                    floor, index, new Vector2(currentX, currentY));
                
                currentX += stepX;
            }
            currentX = startX;
            currentY += stepY;
        }
    }

    #region Connect Nodes
    private void ConnectNodes()
    {
        int pathToDraw = 6;
        int firstStartIndex = -1; // dummy
        

        for (int p = 0; p < pathToDraw; p++)
        {
            // pick starting index on floor 0
            int currentIndex = RngUtility.Range(mapRng, 0, nodesPerFloor);
            
            // Rule: First two paths cannot start on the same node
            if (p == 1)
            {
                while (currentIndex == firstStartIndex)
                {
                    currentIndex = RngUtility.Range(mapRng, 0, nodesPerFloor);
                }
            }
            
            if (p == 0) firstStartIndex = currentIndex;
            
            // !!! Mark node
            var currentMapNode = nodeGrid[0][currentIndex];
            currentMapNode.IsReachable = true;
            // Debug.Log($"Mark start reachable: floor 0 index {currentIndex}");

            for (int currentFloor = 0; currentFloor < floorPerAct - 1; currentFloor++)
            {
                int nextFloor = currentFloor + 1;

                // Get valid candidates: X-1, X, X+1 (clamped to 0 and nodesPerFloor-1)
                int minX = Mathf.Max(0, currentIndex - 1);
                int maxX = Mathf.Min(nodesPerFloor - 1, currentIndex + 1);
                
                List<int> validCandidates = new  List<int>();

                for (int targetX = minX; targetX <= maxX; targetX++)
                {
                    bool crossed = false;

                    if (targetX < currentIndex)
                    {
                        var leftNode = nodeGrid[currentFloor][currentIndex - 1];
                        if (leftNode.Children.Contains(nodeGrid[nextFloor][currentIndex]))
                        {
                            crossed = true;
                        }
                    }
                    else if (targetX > currentIndex)
                    {
                        var rightNode = nodeGrid[currentFloor][currentIndex + 1];
                        if (rightNode.Children.Contains(nodeGrid[nextFloor][currentIndex]))
                        {
                            crossed = true;
                        }
                    }
                    
                    if (!crossed) validCandidates.Add(targetX);
                }
                
                // fallback
                if (validCandidates.Count == 0) validCandidates.Add(currentIndex);
                
                int chosenNextIndex = validCandidates[RngUtility.Range(mapRng, 0, validCandidates.Count)];

                var from = nodeGrid[currentFloor][currentIndex];
                var to = nodeGrid[nextFloor][chosenNextIndex];

                if (!from.Children.Contains(to))
                {
                    from.Children.Add(to);
                    to.Parents.Add(from);
                }
                
                // !!! Mark destination node active
                to.IsReachable = true;
                // Debug.Log($"Mark reachable: floor {nextFloor} index {chosenNextIndex}");
                // move up
                currentIndex = chosenNextIndex;
            }
        }
        // DrawVisualLines();
    }
    
    private void DrawVisualMap()
    {
        for (int floor = 0; floor < floorPerAct; floor++)
        {
            for (int index = 0; index < nodesPerFloor; index++)
            {
                MapNodeData mapNode = nodeGrid[floor][index];

                // Only instantiate views for nodes that are part of the path
                if (!mapNode.IsReachable)
                    continue;

                GameObject nodeObj = Instantiate(nodePrefab, mapContainer.transform);
                NodeView view = nodeObj.GetComponent<NodeView>();
                
                if (view != null)
                {
                    view.Initialize(mapNode);
                    // Add to cache
                    _cacheNodeViews.Add(view);
                }
            }
        }
        
        // Draw lines after nodes are created
        DrawVisualLines(); 
    }
    private void DrawVisualLines()
    {
        for (int floor = 0; floor < floorPerAct - 1; floor++)
        {
            for (int index = 0; index < nodesPerFloor; index++)
            {
                MapNodeData node = nodeGrid[floor][index];
                
                if (!node.IsReachable) continue; 

                foreach (MapNodeData child in node.Children)
                {
                    CreateUINodeConnection(node.Position, child.Position);
                }
            }
        }
    }
    /// <summary>
    /// Debug
    /// </summary>
    private void LogOrphanNodes()
    {
        int prunedCount = 0;

        for (int floor = 0; floor < floorPerAct; floor++)
        {
            for (int index = 0; index < nodesPerFloor; index++)
            {
                MapNodeData node = nodeGrid[floor][index];
                
                if (!node.IsReachable)
                {
                    prunedCount++;
                }
            }
        }
        Debug.Log($"Data Generation Phase: Identified {prunedCount} unreachable nodes.");
    }
    private void CreateUINodeConnection(Vector2 startPos, Vector2 endPos)
    {
        // 1. Instantiate the line prefab inside the map container
        GameObject lineObj = Instantiate(linePrefab, mapContainer.transform);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        // 2. Push lines behind the nodes (so the icons render on top of the lines)
        lineObj.transform.SetAsFirstSibling();

        // 3. Set the starting position
        lineRect.anchoredPosition = startPos;

        // 4. Calculate the distance and direction
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        // 5. Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 6. Apply the math
        // width = distance to stretch to the target, height = line thickness (e.g., 5 pixels)
        lineRect.sizeDelta = new Vector2(distance, 5f); 
    
        // Rotate the RectTransform to point at the target
        lineRect.localEulerAngles = new Vector3(0, 0, angle); 
    }
    #endregion

    #region Assign Nodes

    private void AssignNodeTypes(bool ascensionOnePlus = true)
    {
        ResetAssignedTypes();
        ApplyFixedFloorAssignments();

        List<MapNodeData> nodesToAssign = GetReachableUnassignedNodes();

        bool success = TryAssignRemainingNodes(nodesToAssign, ascensionOnePlus);

        if (!success)
        {
            throw new InvalidOperationException(
                $"Failed to assign valid node types for runSeed={runSeed}. " +
                $"No complete valid assignment exists for the generated graph."
            );
        }
    }

    private void ResetAssignedTypes()
    {
        for (int floor = 0; floor < floorPerAct; floor++)
        {
            for (int index = 0; index < nodesPerFloor; index++)
            {
                nodeGrid[floor][index].AssignedType = NodeType.None;
            }
        }
    }

    private void ApplyFixedFloorAssignments()
    {
        for (int floor = 0; floor < floorPerAct; floor++)
        {
            for (int index = 0; index < nodesPerFloor; index++)
            {
                MapNodeData node = nodeGrid[floor][index];

                if (!node.IsReachable)
                    continue;

                // 1st floor -> Monster
                if (floor == 0)
                {
                    node.AssignedType = NodeType.CombatNonElite;
                    continue;
                }

                // 9th floor -> Treasure
                if (floor == 8)
                {
                    node.AssignedType = NodeType.Treasure;
                    continue;
                }

                // 15th floor -> Rest
                if (floor == 14)
                {
                    node.AssignedType = NodeType.RestSite;
                    continue;
                }
            }
        }
    }

    private List<MapNodeData> GetReachableUnassignedNodes()
    {
        List<MapNodeData> result = new List<MapNodeData>();

        for (int floor = 0; floor < floorPerAct; floor++)
        {
            for (int index = 0; index < nodesPerFloor; index++)
            {
                MapNodeData node = nodeGrid[floor][index];

                if (!node.IsReachable)
                    continue;

                if (node.AssignedType != NodeType.None)
                    continue;

                result.Add(node);
            }
        }

        return result;
    }

    private bool TryAssignRemainingNodes(List<MapNodeData> nodesToAssign, bool ascensionOnePlus)
    {
        MapNodeData nextNode = null;
        List<NodeType> nextCandidates = null;
        int bestCandidateCount = int.MaxValue;

        // Minimum Remaining Values heuristic:
        // always pick the unassigned node with the fewest valid candidates.
        foreach (MapNodeData node in nodesToAssign)
        {
            if (node.AssignedType != NodeType.None)
                continue;

            List<NodeType> validCandidates = GetValidCandidates(node);

            if (validCandidates.Count == 0)
            {
                return false;
            }

            if (validCandidates.Count < bestCandidateCount)
            {
                bestCandidateCount = validCandidates.Count;
                nextNode = node;
                nextCandidates = validCandidates;

                if (bestCandidateCount == 1)
                    break;
            }
        }

        // All assigned
        if (nextNode == null)
            return true;

        List<NodeType> orderedCandidates = GetWeightedCandidateOrder(nextNode, nextCandidates, ascensionOnePlus);

        foreach (NodeType candidate in orderedCandidates)
        {
            nextNode.AssignedType = candidate;

            if (TryAssignRemainingNodes(nodesToAssign, ascensionOnePlus))
                return true;

            nextNode.AssignedType = NodeType.None;
        }

        return false;
    }

    private List<NodeType> GetValidCandidates(MapNodeData node)
    {
        List<NodeType> candidates = new List<NodeType>
        {
            NodeType.CombatNonElite,
            NodeType.CombatElite,
            NodeType.RestSite,
            NodeType.Merchant,
            NodeType.Unknown
        };

        List<NodeType> valid = new List<NodeType>();

        foreach (NodeType candidate in candidates)
        {
            if (IsValidNodeType(node, candidate))
            {
                valid.Add(candidate);
            }
        }

        return valid;
    }

    private List<NodeType> GetWeightedCandidateOrder(
        MapNodeData node,
        List<NodeType> candidates,
        bool ascensionOnePlus)
    {
        // Deterministic per node and seed
        Random rng = new Random(
            SeedUtility.DeriveNodeSeed(runSeed, node.Act, node.Floor, node.Index, RngCategory.NodeAssign)
        );

        Dictionary<NodeType, int> weights = GetNodeTypeWeights(ascensionOnePlus);

        List<NodeType> remaining = new List<NodeType>(candidates);
        List<NodeType> ordered = new List<NodeType>(candidates.Count);

        // Weighted sampling without replacement
        while (remaining.Count > 0)
        {
            int totalWeight = 0;
            foreach (NodeType type in remaining)
            {
                totalWeight += weights[type];
            }

            int roll = RngUtility.Range(rng, 0, totalWeight);

            for (int i = 0; i < remaining.Count; i++)
            {
                NodeType type = remaining[i];
                int weight = weights[type];

                if (roll < weight)
                {
                    ordered.Add(type);
                    remaining.RemoveAt(i);
                    break;
                }

                roll -= weight;
            }
        }

        return ordered;
    }

    private Dictionary<NodeType, int> GetNodeTypeWeights(bool ascensionOnePlus)
    {
        return new Dictionary<NodeType, int>
        {
            { NodeType.CombatNonElite, ascensionOnePlus ? 45 : 53 },
            { NodeType.CombatElite, ascensionOnePlus ? 16 : 8 },
            { NodeType.RestSite, 12 },
            { NodeType.Merchant, 5 },
            { NodeType.Unknown, 22 }
        };
    }

    private bool IsValidNodeType(MapNodeData node, NodeType candidate)
    {
        // Rule 1:
        // Elite and Rest cannot be below 6th floor.
        // 0-based floors: floor 0..4 == floors 1..5
        if (node.Floor < 5 &&
            (candidate == NodeType.CombatElite || candidate == NodeType.RestSite))
        {
            return false;
        }

        // Rule 4:
        // Rest cannot be on the 14th floor.
        // 0-based floor 13 == floor 14
        if (node.Floor == 13 && candidate == NodeType.RestSite)
        {
            return false;
        }

        // Rule 2:
        // Elite, Merchant, Rest cannot be consecutive with parent.
        if (candidate == NodeType.CombatElite ||
            candidate == NodeType.Merchant ||
            candidate == NodeType.RestSite)
        {
            foreach (MapNodeData parent in node.Parents)
            {
                if (parent.AssignedType == NodeType.CombatElite ||
                    parent.AssignedType == NodeType.Merchant ||
                    parent.AssignedType == NodeType.RestSite)
                {
                    return false;
                }
            }
        }

        // Rule 3:
        // If one parent has 2+ outgoing paths, all destinations must be unique.
        foreach (MapNodeData parent in node.Parents)
        {
            if (parent.Children.Count < 2)
                continue;

            foreach (MapNodeData sibling in parent.Children)
            {
                if (sibling == node)
                    continue;

                if (sibling.AssignedType == NodeType.None)
                    continue;

                if (sibling.AssignedType == candidate)
                    return false;
            }
        }

        return true;
    }

    #endregion

    #region Build Runtime Nodes

    private void BuildRuntimeNodes()
    {
        for (int floor = 0; floor < floorPerAct; floor++)
        {
            for (int index = 0; index < nodesPerFloor; index++)
            {
                MapNodeData mapNode = nodeGrid[floor][index];

                if (!mapNode.IsReachable)
                    continue;

                if (mapNode.AssignedType == NodeType.None)
                {
                    Debug.LogError($"AssignedType is None at floor={floor}, index={index}. Skipping runtime build.");
                    continue;
                }

                try
                {
                    System.Random nodeRng = new System.Random(
                        SeedUtility.DeriveNodeSeed(
                            runSeed,
                            mapNode.Act,
                            mapNode.Floor,
                            mapNode.Index,
                            RngCategory.NodeBuild
                        )
                    );

                    BaseNode runtimeNode = NodeFactory.CreateNode(mapNode, nodeRng, nodeConfigLibrary);

                    if (runtimeNode == null)
                    {
                        Debug.LogError($"Runtime node is null at floor={floor}, index={index}, type={mapNode.AssignedType}");
                        continue;
                    }

                    mapNode.RuntimeNode = runtimeNode;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(
                        $"BuildRuntimeNodes failed at floor={floor}, index={index}, type={mapNode.AssignedType}\n{ex}"
                    );
                }
            }
        }
    }
    
    #endregion

    private void OnDestroy()
    {
        if (RunManager.Instance != null)
        {
            RunManager.Instance.OnRunStateChange -= OnEnterMapState;
        }
    }
}
