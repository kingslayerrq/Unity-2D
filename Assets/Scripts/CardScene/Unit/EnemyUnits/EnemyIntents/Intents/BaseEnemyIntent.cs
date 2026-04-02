using System;
using UnityEngine;

[Serializable]
public abstract class BaseEnemyIntent
{
    public abstract Type RequiredConfigType { get; }
    
    public abstract void ExecuteIntent(EnemyIntentContext context, EnemyIntentData  intentData);
}
