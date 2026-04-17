using System.Collections.Generic;
using UnityEngine;

public class MapNodeData
{
    public int Act;
    public int Floor;
    public int Index;

    public bool IsReachable;
    public NodeType AssignedType;

    public GameObject ViewObject;
    public BaseNode RuntimeNode;

    public List<MapNodeData> Parents;
    public List<MapNodeData> Children;

    public MapNodeData(int act, int floor, int index, bool isReachable, NodeType assignedType, GameObject viewObject)
    {
        Act = act;
        Floor = floor;
        Index = index;
        IsReachable = isReachable;
        AssignedType = assignedType;
        ViewObject = viewObject;
        Parents = new List<MapNodeData>();
        Children = new List<MapNodeData>();
    }
}
