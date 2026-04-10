using System;
using UnityEngine;

[Serializable]
public class HealCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(HealCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Targets == null) return;

        if (effectData.CardEffectConfig is not HealCardEffectConfig healConfig)
        {
            Debug.LogError($"HealCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }

        HealAction healAction;

        if (healConfig.SecondaryTarget != TargetType.None)
        {
            var secondaryTarget = context.CombatManager.UnitsManager.GetUnitsByType(healConfig.SecondaryTarget);
            if (secondaryTarget != null)
            {
                healAction = new HealAction(context.CombatManager.CombatSystem, context.Source, secondaryTarget,
                    healConfig.Amount);
            }
            else
            {
                Debug.LogError($"HealCardEffectData secondary target {healConfig.SecondaryTarget} not found");
                return;
            }
        }
        else
        {
            healAction = new HealAction(context.CombatManager.CombatSystem,
                context.Source, context.Targets, healConfig.Amount);
        }
        
        
        context.CombatManager.ActionQueue.Enqueue(healAction, null);
    }
}
