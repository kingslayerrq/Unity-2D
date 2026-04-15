using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NonEliteCombatNodeConfig", menuName = "Nodes/Combat")]
public class NonEliteCombatNodeConfig : BaseCombatNodeConfig
{
    
    public override NodeType NodeType => NodeType.CombatNonElite;
    public override BaseNode CreateNode() => new NonEliteCombatNode(this);
}
