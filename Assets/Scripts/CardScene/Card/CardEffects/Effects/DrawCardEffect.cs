using System;
using UnityEngine;

[Serializable]
public class DrawCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(DrawCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatSystem == null || context.CardInstance == null) return;
        if (context.Source == null || context.Target == null) return;

        if (effectData.CardEffectConfig is not DrawCardEffectConfig drawConfig)
        {
            Debug.LogError($"DrawCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }
        
        context.CombatSystem.ResolveDrawCards(context.Source, context.Target, drawConfig.Amount);
    }
}
