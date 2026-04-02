using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class BaseEnemyIntent
{
    public abstract Type RequiredConfigType { get; }
    public List<BaseUnitInstance> IntentTargets = new List<BaseUnitInstance>();
    
    public abstract void ExecuteIntent(EnemyIntentContext context, EnemyIntentData intentData);

    public virtual void ChooseTargets(EnemyIntentData intentData, CombatSnapshot snapshot)
    {
        IntentTargets.Clear();
        switch (intentData.EnemyIntentConfig.TargetType)
        {
            case TargetType.Self:
                IntentTargets.Add(snapshot.Self);
                break;
            case TargetType.Enemy:
                IntentTargets.Add(ChoosePlayerTarget(snapshot));
                break;
            case TargetType.AllEnemy:
                IntentTargets.AddRange(snapshot.AllPlayers);
                break;
            case TargetType.Ally:
                IntentTargets.Add(ChooseEnemyAlly(snapshot));
                break;
            case TargetType.AllAlly:
                IntentTargets.AddRange(snapshot.AllEnemies);
                IntentTargets.Remove(snapshot.Self);
                break;
            case TargetType.Any:
                IntentTargets.Add(ChooseAnyTarget(snapshot));
                break;
            case TargetType.None:
                break;
            default:
                break;
        }
    }
    
    public abstract BasePlayerUnitInstance ChoosePlayerTarget(CombatSnapshot snapshot);
    public abstract BaseEnemyUnitInstance ChooseEnemyAlly(CombatSnapshot snapshot);
    public abstract BaseUnitInstance ChooseAnyTarget(CombatSnapshot snapshot);
    
}
