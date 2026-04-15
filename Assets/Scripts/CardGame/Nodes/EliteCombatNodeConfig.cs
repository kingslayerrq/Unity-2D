using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EliteCombatNodeConfig", menuName = "Nodes/Combat")]
public class EliteCombatNodeConfig : BaseCombatNodeConfig
{
    public override NodeType NodeType => NodeType.CombatElite;
    public override BaseNode CreateNode() => new EliteCombatNode(this);
    
}
