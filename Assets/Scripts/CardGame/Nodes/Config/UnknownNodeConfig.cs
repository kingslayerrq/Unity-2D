using UnityEngine;

[CreateAssetMenu(fileName = "new UnknownNodeConfig", menuName = "Nodes/UnknownNodeConfig")]
public class UnknownNodeConfig : BaseNodeConfig
{
    public override NodeType NodeType => NodeType.Unknown;
    public override BaseNode CreateNode() => new UnknownNode(this);
}
