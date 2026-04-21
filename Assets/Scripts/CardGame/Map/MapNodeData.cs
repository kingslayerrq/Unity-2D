using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single source of truth for nodes generated on map
/// Basenodes holds reference
/// </summary>
public class MapNodeData
{
    public int Act;
    public int Floor;
    public int Index;
    public Vector2 Position;

    public bool IsReachable;
    public NodeType AssignedType;
    public BaseNode RuntimeNode;

    public List<MapNodeData> Parents;
    public List<MapNodeData> Children;

    public MapNodeData(int act, int floor, int index, Vector2 position)
    {
        Act = act;
        Floor = floor;
        Index = index;
        Position = position;
        IsReachable = false;
        AssignedType = NodeType.None;
        Parents = new List<MapNodeData>();
        Children = new List<MapNodeData>();
    }
}
