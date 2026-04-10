using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(BuffCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Targets == null) return;

        if (effectData.CardEffectConfig is not BuffCardEffectConfig buffConfig)
        {
            Debug.LogError($"BuffCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }

        if (buffConfig.BuffData == null)
        {
            Debug.LogError("BuffCardEffectData config is missing BuffData.");
            return;
        }

        ApplyBuffAction applyBuffAction;
        if (buffConfig.SecondaryTarget != TargetType.None)
        {
            var secondaryTarget = context.CombatManager?.UnitsManager.GetUnitsByType(buffConfig.SecondaryTarget);
            if (secondaryTarget != null)
            {
                applyBuffAction = new ApplyBuffAction(context.CombatManager.CombatSystem, context.Source,
                    secondaryTarget,
                    buffConfig.BuffData, buffConfig.Stacks);
            }
            else
            {
                Debug.LogError($"BuffCardEffectData secondary target {buffConfig.SecondaryTarget} not found");
                return;
            }
        }
        else
        {
            applyBuffAction = new ApplyBuffAction(context.CombatManager.CombatSystem,
                context.Source, context.Targets, buffConfig.BuffData, buffConfig.Stacks);
        }
        
        context.CombatManager.ActionQueue.Enqueue(applyBuffAction, null);

    }
}
