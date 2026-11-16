using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Builds the map of an act from a seed: which slots are on a path, how they
/// connect, what type each node is, and the node to play there.
///
/// Plain C#, with no scene or layout involved. The same seed, settings and
/// library always give the same map.
/// </summary>
public static class MapBuilder
{
    private const int PathCount = 6;

    /// <returns>Null when the settings cannot produce a map.</returns>
    public static RunMap Build(
        int runSeed,
        MapSettings settings,
        NodeConfigLibrary library,
        bool ascensionOnePlus = false)
    {
        if (!settings.IsValid)
        {
            Debug.LogError(
                "Cannot build a map: nodes per floor and floors per act must both be positive.");
            return null;
        }

        MapNodeData[][] grid = CreateGrid(settings);
        Random mapRng = new Random(SeedUtility.DeriveSeed(runSeed, RngCategory.Map));

        ConnectNodes(grid, settings, mapRng);
        AssignNodeTypes(grid, settings, runSeed, ascensionOnePlus);
        BuildRuntimeNodes(grid, settings, runSeed, library);

        return new RunMap(runSeed, grid);
    }

    private static MapNodeData[][] CreateGrid(MapSettings settings)
    {
        MapNodeData[][] grid = new MapNodeData[settings.FloorsPerAct][];

        for (int floor = 0; floor < settings.FloorsPerAct; floor++)
        {
            grid[floor] = new MapNodeData[settings.NodesPerFloor];
            for (int index = 0; index < settings.NodesPerFloor; index++)
            {
                grid[floor][index] = new MapNodeData(0, floor, index, Vector2.zero);
            }
        }

        return grid;
    }

    #region Connect Nodes

    private static void ConnectNodes(MapNodeData[][] nodeGrid, MapSettings settings, Random mapRng)
    {
        int nodesPerFloor = settings.NodesPerFloor;
        int floorPerAct = settings.FloorsPerAct;
        int firstStartIndex = -1; // dummy

        for (int p = 0; p < PathCount; p++)
        {
            // pick starting index on floor 0
            int currentIndex = RngUtility.Range(mapRng, 0, nodesPerFloor);

            // Rule: First two paths cannot start on the same node.
            // With a single slot per floor there is no other node to pick.
            if (p == 1 && nodesPerFloor > 1)
            {
                while (currentIndex == firstStartIndex)
                {
                    currentIndex = RngUtility.Range(mapRng, 0, nodesPerFloor);
                }
            }

            if (p == 0) firstStartIndex = currentIndex;

            // !!! Mark node
            nodeGrid[0][currentIndex].IsReachable = true;

            for (int currentFloor = 0; currentFloor < floorPerAct - 1; currentFloor++)
            {
                int nextFloor = currentFloor + 1;

                // Get valid candidates: X-1, X, X+1 (clamped to 0 and nodesPerFloor-1)
                int minX = Mathf.Max(0, currentIndex - 1);
                int maxX = Mathf.Min(nodesPerFloor - 1, currentIndex + 1);

                List<int> validCandidates = new List<int>();

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

                // move up
                currentIndex = chosenNextIndex;
            }
        }
    }

    #endregion

    #region Assign Nodes

    private static void AssignNodeTypes(
        MapNodeData[][] nodeGrid,
        MapSettings settings,
        int runSeed,
        bool ascensionOnePlus)
    {
        ApplyFixedFloorAssignments(nodeGrid);

        List<MapNodeData> nodesToAssign = GetReachableUnassignedNodes(nodeGrid);

        bool success = TryAssignRemainingNodes(nodesToAssign, runSeed, ascensionOnePlus);

        if (!success)
        {
            throw new InvalidOperationException(
                $"Failed to assign valid node types for runSeed={runSeed}. " +
                $"No complete valid assignment exists for the generated graph."
            );
        }
    }

    private static void ApplyFixedFloorAssignments(MapNodeData[][] nodeGrid)
    {
        for (int floor = 0; floor < nodeGrid.Length; floor++)
        {
            for (int index = 0; index < nodeGrid[floor].Length; index++)
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

    private static List<MapNodeData> GetReachableUnassignedNodes(MapNodeData[][] nodeGrid)
    {
        List<MapNodeData> result = new List<MapNodeData>();

        for (int floor = 0; floor < nodeGrid.Length; floor++)
        {
            for (int index = 0; index < nodeGrid[floor].Length; index++)
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

    private static bool TryAssignRemainingNodes(
        List<MapNodeData> nodesToAssign,
        int runSeed,
        bool ascensionOnePlus)
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

        List<NodeType> orderedCandidates =
            GetWeightedCandidateOrder(nextNode, nextCandidates, runSeed, ascensionOnePlus);

        foreach (NodeType candidate in orderedCandidates)
        {
            nextNode.AssignedType = candidate;

            if (TryAssignRemainingNodes(nodesToAssign, runSeed, ascensionOnePlus))
                return true;

            nextNode.AssignedType = NodeType.None;
        }

        return false;
    }

    private static List<NodeType> GetValidCandidates(MapNodeData node)
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

    private static List<NodeType> GetWeightedCandidateOrder(
        MapNodeData node,
        List<NodeType> candidates,
        int runSeed,
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

    private static Dictionary<NodeType, int> GetNodeTypeWeights(bool ascensionOnePlus)
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

    private static bool IsValidNodeType(MapNodeData node, NodeType candidate)
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

    private static void BuildRuntimeNodes(
        MapNodeData[][] nodeGrid,
        MapSettings settings,
        int runSeed,
        NodeConfigLibrary library)
    {
        for (int floor = 0; floor < nodeGrid.Length; floor++)
        {
            for (int index = 0; index < nodeGrid[floor].Length; index++)
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
                    Random nodeRng = new Random(
                        SeedUtility.DeriveNodeSeed(
                            runSeed,
                            mapNode.Act,
                            mapNode.Floor,
                            mapNode.Index,
                            RngCategory.NodeBuild
                        )
                    );

                    BaseNode runtimeNode = NodeFactory.CreateNode(mapNode, nodeRng, library, runSeed);

                    if (runtimeNode == null)
                    {
                        Debug.LogError($"Runtime node is null at floor={floor}, index={index}, type={mapNode.AssignedType}");
                        continue;
                    }

                    mapNode.RuntimeNode = runtimeNode;
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"BuildRuntimeNodes failed at floor={floor}, index={index}, type={mapNode.AssignedType}\n{ex}"
                    );
                }
            }
        }
    }

    #endregion
}
