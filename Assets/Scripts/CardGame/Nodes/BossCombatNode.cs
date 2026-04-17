using UnityEngine;

public class BossCombatNode : BaseCombatNode
{
    public BossCombatNode(BaseCombatNodeConfig config) : base(config)
    {
    }


    public override void Enter(RunManager runManager)
    {
        Debug.Log("Entering Boss Combat");
    }

    public override void Exit(RunManager runManager)
    {
        Debug.Log("Exiting Boss Combat");
    }
}
