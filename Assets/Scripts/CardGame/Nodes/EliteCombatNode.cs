using UnityEngine;

public class EliteCombatNode : BaseCombatNode
{
    public EliteCombatNode(EliteCombatNodeConfig config) : base(config) { }
    public override void Enter(RunManager runManager) => runManager.EnterEliteCombat(this);

    public override void Exit(RunManager runManager)
    {
        Debug.Log("Exit Elite Combat");
    }
}