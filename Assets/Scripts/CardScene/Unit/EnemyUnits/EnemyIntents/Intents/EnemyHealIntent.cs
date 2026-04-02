using System;
using UnityEngine;

[Serializable]
public class EnemyHealIntent : BaseEnemyIntent
{
    public override Type RequiredConfigType => typeof(EnemyHealIntentConfig);
    
    public override void ExecuteIntent(EnemyIntentContext context, EnemyIntentData intentData)
    {
        if (context == null || context.CombatManager == null) return;
        if (context.Source == null || context.Targets == null ||
            context.Targets.Count == 0) return;

        if (intentData.EnemyIntentConfig is not EnemyHealIntentConfig healIntentConfig)
        {
            Debug.LogError($"EnemyHealActionData received wrong config type" +
                           $" on {context.Intent}");
            return;
        }

        HealAction healAction = new HealAction(context.CombatManager.CombatSystem, context.Source,
            context.Targets, healIntentConfig.Amount);

        context.CombatManager.ActionQueue.Enqueue(healAction, null);
        
    }
}
