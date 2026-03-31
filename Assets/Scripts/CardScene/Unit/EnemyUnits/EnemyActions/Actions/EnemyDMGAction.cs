using System;
using UnityEngine;

public class EnemyDmgAction : BaseEnemyAction
{
    public override Type RequiredConfigType => typeof(EnemyDMGActionConfig);
    
    public override void ExecuteAction(EnemyActionContext context, EnemyActionData actionData)
    {
        if (context == null || context.CombatSystem == null) return;
        if (context.Source == null || context.Targets == null ||
            context.Targets.Count == 0) return;

        if (actionData.EnemyActionConfig is not EnemyDMGActionConfig dmgActionConfig)
        {
            Debug.LogError($"EnemyDmgActionData received wrong config type" +
                           $" on {context.Action}");
            return;
        }
        
        context.CombatSystem.ResolveDamage(context.Source, context.Targets, dmgActionConfig.Amount,
            CardType.None);
    }
}
