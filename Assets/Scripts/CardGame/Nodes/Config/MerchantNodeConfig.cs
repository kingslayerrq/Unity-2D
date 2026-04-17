using UnityEngine;

[CreateAssetMenu(fileName = "new MerchantNodeConfig", menuName = "Nodes/MerchantNodeConfig")]
public class MerchantNodeConfig : BaseNodeConfig
{
    // TODO: Shop Config
    public override NodeType NodeType => NodeType.Merchant;
    public override BaseNode CreateNode() => new MerchantNode(this);
}
