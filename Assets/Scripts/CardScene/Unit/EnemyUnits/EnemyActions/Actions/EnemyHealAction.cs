using System;
using UnityEngine;

[Serializable]
public class EnemyHealAction : BaseEnemyAction
{
    public override Type RequiredConfigType => typeof(EnemyHealActionConfig);
    
    public override void ExecuteAction(EnemyActionContext context, EnemyActionData actionData)
    {
        if (context == null || context.CombatManager == null) return;
        if (context.Source == null || context.Targets == null ||
            context.Targets.Count == 0) return;

        if (actionData.EnemyActionConfig is not EnemyHealActionConfig healActionConfig)
        {
            Debug.LogError($"EnemyHealActionData received wrong config type" +
                           $" on {context.Action}");
            return;
        }

        HealAction healAction = new HealAction(context.CombatManager.CombatSystem, context.Source,
            context.Targets, healActionConfig.Amount);

        context.CombatManager.ActionQueue.Enqueue(healAction, null);
        
    }
}
