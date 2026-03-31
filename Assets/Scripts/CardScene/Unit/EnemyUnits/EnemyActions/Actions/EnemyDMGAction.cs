using System;
using UnityEngine;

[Serializable]
public class EnemyDmgAction : BaseEnemyAction
{
    public override Type RequiredConfigType => typeof(EnemyDmgActionConfig);
    
    public override void ExecuteAction(EnemyActionContext context, EnemyActionData actionData)
    {
        if (context == null || context.CombatManager == null) return;
        if (context.Source == null || context.Targets == null ||
            context.Targets.Count == 0) return;

        if (actionData.EnemyActionConfig is not EnemyDmgActionConfig dmgActionConfig)
        {
            Debug.LogError($"EnemyDmgActionData received wrong config type" +
                           $" on {context.Action}");
            return;
        }

        DamageAction damageAction = new DamageAction(context.CombatManager.CombatSystem, context.Source,
            context.Targets, dmgActionConfig.Amount, CardType.None);

        for (int i = 0; i < dmgActionConfig.NumberOfAttacks; i++)
        {
            context.CombatManager.ActionQueue.Enqueue(damageAction, null);
        }
        
    }
}
