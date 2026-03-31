using System;
using UnityEngine;

[Serializable]
public class EnemyActionData
{
    [SerializeReference] private BaseEnemyAction enemyAction;
    [SerializeReference] private BaseEnemyActionConfig enemyActionConfig;

    public BaseEnemyAction EnemyAction => enemyAction;
    public BaseEnemyActionConfig EnemyActionConfig => enemyActionConfig;

    public void SetConfig(BaseEnemyActionConfig config)
    {
        this.enemyActionConfig =  config;
    }
}
