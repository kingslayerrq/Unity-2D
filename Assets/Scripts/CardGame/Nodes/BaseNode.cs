using UnityEngine;

public abstract class BaseNode
{
    public BaseNodeConfig NodeConfig { get; }
    public MapNodeData NodeData { get; private set; }

    public int Act => NodeData?.Act ?? 0;
    public int Floor => NodeData?.Floor ?? 0;
    public int Index => NodeData?.Index ?? 0;

    public NodeType Type => NodeConfig.NodeType;

    /// <summary>
    /// The node whose content actually runs. Differs from this node only for
    /// nodes that turn into another type when entered, such as Unknown nodes.
    /// </summary>
    public virtual BaseNode ResolvedNode => this;

    protected BaseNode(BaseNodeConfig nodeConfig)
    {
        NodeConfig = nodeConfig;
    }

    public void SetNodeData(MapNodeData data)
    {
        NodeData = data;
    }

    public abstract void OnEnter();
    public abstract void OnExit();
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