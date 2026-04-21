using UnityEngine;

public abstract class BaseNode
{
    public BaseNodeConfig NodeConfig { get; }
    public MapNodeData NodeData { get; private set; }

    public int Act => NodeData?.Act ?? 0;
    public int Floor => NodeData?.Floor ?? 0;
    public int Index => NodeData?.Index ?? 0;

    public bool IsNext => RunManager.Instance.CurrentAct == Act && RunManager.Instance.CurrentFloor == Floor;

    public NodeType Type;

    protected BaseNode(BaseNodeConfig nodeConfig)
    {
        NodeConfig = nodeConfig;
    }

    public void SetNodeData(MapNodeData data)
    {
        NodeData = data;
    }

    public abstract void Enter(RunManager runManager);
    public abstract void Exit(RunManager runManager);
}

public enum NodeType
{
    None,
    CombatBoss,
    CombatElite,
    CombatNonElite,
    Merchant,
    RestSite,
    Treasure,
    Event,
    Unknown
}