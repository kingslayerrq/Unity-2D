using UnityEngine;

public class EliteCombatNode : BaseCombatNode
{
    public EliteCombatNode(EliteCombatNodeConfig config) : base(config) { }
    public override void OnEnter() { }

    public override void OnExit()
    {
        Debug.Log("Exit Elite Combat");
    }
}