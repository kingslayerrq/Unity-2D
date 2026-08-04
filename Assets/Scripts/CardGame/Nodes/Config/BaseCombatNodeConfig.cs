using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCombatNodeConfig : BaseNodeConfig
{
    [SerializeField] protected List<EnemyUnitData> enemyDatas;
    public IReadOnlyList<EnemyUnitData> EnemyUnitDatas => enemyDatas;
}
