using UnityEngine;

public class BossCombatNode : BaseCombatNode
{
    public BossCombatNode(BaseCombatNodeConfig config) : base(config)
    {
    }


    public override void OnEnter() { }

    public override void OnExit()
    {
        Debug.Log("Exiting Boss Combat");
    }
}
