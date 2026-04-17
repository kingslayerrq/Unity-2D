using UnityEngine;

public abstract class BaseNode
{
    public BaseNodeConfig NodeConfig { get; }
    public int Act { get; internal set; }
    public int Floor { get; internal set; }
    public int Index { get; internal set; }
    
    public NodeType Type;
    
    protected  BaseNode(BaseNodeConfig nodeConfig)
    {
        NodeConfig = nodeConfig;
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