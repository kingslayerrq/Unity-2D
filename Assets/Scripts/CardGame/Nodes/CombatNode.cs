using System.Collections.Generic;
using UnityEngine;

public abstract class CombatNode : BaseNode
{
    public IReadOnlyList<EnemyUnitData> EnemyUnitDatas { get; }

    protected CombatNode(BaseCombatNodeConfig config) : base(config)
    {
        EnemyUnitDatas = config.EnemyUnitDatas;
    }
}

public class NonEliteCombatNode : CombatNode
{
    public NonEliteCombatNode(NonEliteCombatNodeConfig config) : base(config) { }
    public override void Enter(RunManager runManager) => runManager.EnterNonEliteCombat(this);

    public override void Exit(RunManager runManager)
    {
        Debug.Log("Exit Non Elite Combat");
    }
}

public class EliteCombatNode : CombatNode
{
    public EliteCombatNode(EliteCombatNodeConfig config) : base(config) { }
    public override void Enter(RunManager runManager) => runManager.EnterEliteCombat(this);

    public override void Exit(RunManager runManager)
    {
        Debug.Log("Exit Elite Combat");
    }
}