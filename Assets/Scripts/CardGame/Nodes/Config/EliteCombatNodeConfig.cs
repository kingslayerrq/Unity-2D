using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new EliteCombatNodeConfig", menuName = "Nodes/Combat/EliteCombatNodeConfig")]
public class EliteCombatNodeConfig : BaseCombatNodeConfig
{
    public override NodeType NodeType => NodeType.CombatElite;
    public override BaseNode CreateNode() => new EliteCombatNode(this);
    
}
