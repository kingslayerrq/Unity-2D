using UnityEngine;

public class EventNode : BaseNode
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public EventNode(BaseNodeConfig nodeConfig) : base(nodeConfig)
    {
    }

    public override void Enter(RunManager runManager)
    {
        throw new System.NotImplementedException();
    }

    public override void Exit(RunManager runManager)
    {
        throw new System.NotImplementedException();
    }
}
