using UnityEngine;

[CreateAssetMenu(fileName = "new BossCombatNodeConfig", menuName = "Nodes/Combat/BossCombatNodeConfig")]
public class BossCombatNodeConfig: BaseCombatNodeConfig
{
    public override NodeType NodeType => NodeType.CombatBoss;
    public override BaseNode CreateNode() => new BossCombatNode(this);
}
