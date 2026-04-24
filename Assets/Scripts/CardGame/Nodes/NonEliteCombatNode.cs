using UnityEngine;

public class NonEliteCombatNode : BaseCombatNode
{
    public NonEliteCombatNode(NonEliteCombatNodeConfig config) : base(config) { }
    public override void OnEnter() { }

    public override void OnExit()
    {
        Debug.Log("Exit Non Elite Combat");
    }
}