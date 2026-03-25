using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(BuffCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatSystem == null || context.CardInstance == null) return;
        if (context.Source == null || context.Target == null) return;

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
        
        context.CombatSystem.ResolveBuffs(context.Source, context.Target, buffConfig.BuffData, buffConfig.Stacks);
    }
}
