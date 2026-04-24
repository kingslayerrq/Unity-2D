using UnityEngine;

public class EventNode : BaseNode
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public EventNode(BaseNodeConfig nodeConfig) : base(nodeConfig)
    {
    }

    public override void OnEnter() { }

    public override void OnExit()
    {
        throw new System.NotImplementedException();
    }
}
