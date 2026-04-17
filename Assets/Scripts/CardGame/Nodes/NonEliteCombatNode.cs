using UnityEngine;

public class NonEliteCombatNode : BaseCombatNode
{
    public NonEliteCombatNode(NonEliteCombatNodeConfig config) : base(config) { }
    public override void Enter(RunManager runManager) => runManager.EnterNonEliteCombat(this);

    public override void Exit(RunManager runManager)
    {
        Debug.Log("Exit Non Elite Combat");
    }
}