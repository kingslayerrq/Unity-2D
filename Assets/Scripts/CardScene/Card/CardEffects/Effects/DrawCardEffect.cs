using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DrawCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(DrawCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Target == null) return;

        if (effectData.CardEffectConfig is not DrawCardEffectConfig drawConfig)
        {
            Debug.LogError($"DrawCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }

        // filter out non player units
        var playerTargets = context.Target.OfType<BasePlayerUnitInstance>().ToList();
        
        if (playerTargets.Any())
        {
            // context.CombatSystem.ResolveDrawCards(context.Source, playerTargets, drawConfig.Amount);
            DrawAction drawAction = new DrawAction(context.CombatManager.CombatSystem,
                context.Source, playerTargets, drawConfig.Amount);
            context.CombatManager.ActionQueue.Enqueue(drawAction, null);
        }
    }
}
