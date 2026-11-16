using System.Collections.Generic;

/// <summary>
/// The size of a map: how many node slots each floor has and how many floors
/// make up an act.
/// </summary>
public readonly struct MapSettings
{
    public int NodesPerFloor { get; }
    public int FloorsPerAct { get; }

    public MapSettings(int nodesPerFloor, int floorsPerAct)
    {
        NodesPerFloor = nodesPerFloor;
        FloorsPerAct = floorsPerAct;
    }

    public bool IsValid => NodesPerFloor > 0 && FloorsPerAct > 0;
}

/// <summary>
/// The map of one act as a graph: a grid of node slots, of which the
/// reachable ones are linked floor to floor and carry a node to play.
///
/// This is the map as rules see it. Where each node is drawn on screen is
/// decided by <see cref="MapGenerator"/>.
/// </summary>
public sealed class RunMap
{
    /// <summary>Every slot, indexed [floor][index]. Many are unreachable.</summary>
    public MapNodeData[][] Grid { get; }

    public int Seed { get; }
    public int FloorCount => Grid.Length;
    public int NodesPerFloor => Grid.Length == 0 ? 0 : Grid[0].Length;

    public RunMap(int seed, MapNodeData[][] grid)
    {
        Seed = seed;
        Grid = grid;
    }

    /// <summary>The slots on a floor that are part of a path.</summary>
    public List<MapNodeData> GetReachableNodes(int floor)
    {
        List<MapNodeData> nodes = new List<MapNodeData>();
        if (floor < 0 || floor >= Grid.Length) return nodes;

        MapNodeData[] row = Grid[floor];
        for (int index = 0; index < row.Length; index++)
        {
            if (row[index].IsReachable) nodes.Add(row[index]);
        }

        return nodes;
    }

    /// <summary>Every slot that is part of a path, floor by floor.</summary>
    public List<MapNodeData> GetAllReachableNodes()
    {
        List<MapNodeData> nodes = new List<MapNodeData>();

        for (int floor = 0; floor < Grid.Length; floor++)
        {
            nodes.AddRange(GetReachableNodes(floor));
        }

        return nodes;
    }
}
