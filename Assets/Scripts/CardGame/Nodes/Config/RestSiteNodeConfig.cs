using UnityEngine;

[CreateAssetMenu(fileName = "new RestSiteNodeConfig", menuName = "Nodes/RestSiteNodeConfig")]
public class RestSiteNodeConfig : BaseNodeConfig
{
    // TODO: rest site config
    public override NodeType NodeType => NodeType.RestSite;
    public override BaseNode CreateNode() => new RestSiteNode(this);
}
