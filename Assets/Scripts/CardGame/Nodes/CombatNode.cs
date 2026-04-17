using System.Collections.Generic;

public abstract class BaseCombatNode : BaseNode
{
    public IReadOnlyList<EnemyUnitData> EnemyUnitDatas { get; }

    protected BaseCombatNode(BaseCombatNodeConfig config) : base(config)
    {
        EnemyUnitDatas = config.EnemyUnitDatas;
    }
}