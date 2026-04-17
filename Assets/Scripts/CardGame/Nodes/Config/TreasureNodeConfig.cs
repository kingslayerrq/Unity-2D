using UnityEngine;

[CreateAssetMenu(fileName = "new TreasureNodeConfig", menuName = "Nodes/TreasureNodeConfig")]
public class TreasureNodeConfig : BaseNodeConfig
{
    // TODO: treasure config
    public override NodeType NodeType => NodeType.Treasure;
    public override BaseNode CreateNode() => new TreasureNode(this);
}
