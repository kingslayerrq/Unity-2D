using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShuffleCardsEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(ShuffleCardsEffectConfig);
    
    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Targets == null) return;
        
        if (effectData.CardEffectConfig is not ShuffleCardsEffectConfig shuffleConfig)
        {
            Debug.LogError($"ShuffleCardsEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }

        
        // filter out non player units
        var playerTargets = context.Targets.OfType<BasePlayerUnitInstance>().ToList();
        if (playerTargets.Any())
        {
            ShuffleCardsAction shuffleCardsAction = new ShuffleCardsAction(context.CombatManager.CombatSystem,
                context.Source, context.CardInstance, playerTargets, shuffleConfig.CardDatas,
                shuffleConfig.TargetCardPiles);
        
            context.CombatManager.ActionQueue.Enqueue(shuffleCardsAction, null);
        }
    }
}
