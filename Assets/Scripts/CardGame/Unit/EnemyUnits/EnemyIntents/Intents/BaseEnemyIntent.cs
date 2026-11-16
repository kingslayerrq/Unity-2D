using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class BaseEnemyIntent
{
    public abstract Type RequiredConfigType { get; }
    
    public abstract void ExecuteIntent(EnemyIntentContext context, EnemyIntentData intentData);

    /// <summary>
    /// Returns a new list of targets for this intent. Intent objects are shared
    /// through the EnemyUnitData asset, so they must not hold per-enemy state.
    /// </summary>
    public virtual List<BaseUnitInstance> ChooseTargets(EnemyIntentData intentData, CombatSnapshot snapshot)
    {
        List<BaseUnitInstance> targets = new List<BaseUnitInstance>();
        switch (intentData.EnemyIntentConfig.TargetType)
        {
            case TargetType.Self:
                targets.Add(snapshot.Self);
                break;
            case TargetType.Enemy:
                targets.Add(ChooseEnemyAlly(snapshot));
                break;
            case TargetType.AllEnemy:
                targets.AddRange(snapshot.AllEnemies);
                targets.Remove(snapshot.Self);
                break;
            case TargetType.Player:
                targets.Add(ChoosePlayerTarget(snapshot));
                break;
            case TargetType.AllPlayer:
                targets.AddRange(snapshot.AllPlayers);
                break;
            case TargetType.Any:
                targets.Add(ChooseAnyTarget(snapshot));
                break;
            case TargetType.None:
                break;
            default:
                break;
        }

        // Choose* helpers return null when there is no valid target.
        targets.RemoveAll(target => target == null);
        return targets;
    }
    
    public abstract BasePlayerUnitInstance ChoosePlayerTarget(CombatSnapshot snapshot);
    public abstract BaseEnemyUnitInstance ChooseEnemyAlly(CombatSnapshot snapshot);
    public abstract BaseUnitInstance ChooseAnyTarget(CombatSnapshot snapshot);
    
}
