using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new NonEliteCombatNodeConfig", menuName = "Nodes/Combat/NonEliteCombatNodeConfig")]
public class NonEliteCombatNodeConfig : BaseCombatNodeConfig
{
    
    public override NodeType NodeType => NodeType.CombatNonElite;
    public override BaseNode CreateNode() => new NonEliteCombatNode(this);
}
