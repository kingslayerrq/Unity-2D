using System;
using UnityEngine;

[Serializable]
public class EnemyDmgIntent : BaseEnemyIntent
{
    public override Type RequiredConfigType => typeof(EnemyDmgIntentConfig);
    
    public override void ExecuteIntent(EnemyIntentContext context, EnemyIntentData actionData)
    {
        if (context == null || context.CombatManager == null) return;
        if (context.Source == null || context.Targets == null ||
            context.Targets.Count == 0) return;

        if (actionData.EnemyIntentConfig is not EnemyDmgIntentConfig dmgActionConfig)
        {
            Debug.LogError($"EnemyDmgActionData received wrong config type" +
                           $" on {context.Intent}");
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
