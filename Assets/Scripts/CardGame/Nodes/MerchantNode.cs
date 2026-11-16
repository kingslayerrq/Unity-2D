using UnityEngine;

public class MerchantNode : BaseNode
{
    public MerchantNode(BaseNodeConfig nodeConfig) : base(nodeConfig)
    {
    }

    public override void OnEnter() { }

    public override void OnExit()
    {
        // Nothing to clean up yet.
    }
}
