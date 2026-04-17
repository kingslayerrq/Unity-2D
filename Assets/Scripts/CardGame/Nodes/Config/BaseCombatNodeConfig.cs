using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCombatNodeConfig : BaseNodeConfig
{
    [SerializeField] private List<EnemyUnitData> enemyDatas;
    public IReadOnlyList<EnemyUnitData> EnemyUnitDatas => enemyDatas;
}
