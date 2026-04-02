using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EnemyIntentData
{
    [SerializeReference] private BaseEnemyIntent enemyIntent;
    [SerializeReference] private BaseEnemyIntentConfig enemyIntentConfig;

    public BaseEnemyIntent EnemyIntent => enemyIntent;
    public BaseEnemyIntentConfig EnemyIntentConfig => enemyIntentConfig;

    public void SetConfig(BaseEnemyIntentConfig config)
    {
        this.enemyIntentConfig =  config;
    }
}
