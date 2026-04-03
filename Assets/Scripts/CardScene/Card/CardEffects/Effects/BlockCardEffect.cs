using System;
using UnityEngine;

[Serializable]
public class BlockCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(BlockCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Targets == null) return;

        if (effectData.CardEffectConfig is not BlockCardEffectConfig blockConfig)
        {
            Debug.LogError($"BlockCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }
        
        BlockAction blockAction = new BlockAction(context.Source.CombatManager.CombatSystem,
            context.Source, context.Targets, blockConfig.Amount, true);
        
        context.Source.CombatManager.ActionQueue.Enqueue(blockAction, null);
    }
}
