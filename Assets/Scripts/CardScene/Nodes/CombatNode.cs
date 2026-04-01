using System.Collections.Generic;
using UnityEngine;

public class CombatNode : BaseNode
{
    private List<EnemyUnitData> enemyDatas;
    
    public List<EnemyUnitData> EnemyUnitDatas => enemyDatas;


    public CombatNode(List<EnemyUnitData> enemyDatas)
    {
        this.enemyDatas = enemyDatas;
    }
}
