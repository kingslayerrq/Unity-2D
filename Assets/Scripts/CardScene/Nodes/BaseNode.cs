using UnityEngine;

public abstract class BaseNode
{
    private NodeType nodeType;
    
    public NodeType NodeType => nodeType;
    
}

public enum NodeType
{
    Quest,
    Combat,
    Shop,
    Campfire,
    CombatElite,
    Treasure,
    Event
}