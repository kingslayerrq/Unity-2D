using UnityEngine;

public abstract class BaseNode
{
    public BaseNodeConfig NodeConfig { get; }
    
    protected  BaseNode(BaseNodeConfig nodeConfig)
    {
        NodeConfig = nodeConfig;
    }
    
    public abstract void Enter(RunManager runManager);
    public abstract void Exit(RunManager runManager);
}

public enum NodeType
{
    Quest,
    CombatNonElite,
    Shop,
    Campfire,
    CombatElite,
    Treasure,
    Event
}