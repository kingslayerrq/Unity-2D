using UnityEngine;

public class TreasureNode : BaseNode
{
    public TreasureNode(BaseNodeConfig nodeConfig) : base(nodeConfig)
    {
    }


    public override void OnEnter() { }

    public override void OnExit()
    {
        // Nothing to clean up yet.
    }
}
