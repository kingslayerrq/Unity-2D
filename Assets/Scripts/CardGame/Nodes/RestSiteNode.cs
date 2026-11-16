using UnityEngine;

public class RestSiteNode : BaseNode
{
    public RestSiteNode(BaseNodeConfig nodeConfig) : base(nodeConfig)
    {
    }


    public override void OnEnter() { }

    public override void OnExit()
    {
        // Nothing to clean up yet.
    }
}
