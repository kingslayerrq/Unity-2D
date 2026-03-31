using System;
using UnityEngine;

[Serializable]
public abstract class BaseEnemyAction
{
    public abstract Type RequiredConfigType { get; }
    
    public abstract void ExecuteAction(EnemyActionContext context, EnemyActionData  actionData);
}
