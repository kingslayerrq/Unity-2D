using System;
using UnityEngine;

[Serializable]
public class BlockCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(BlockCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Target == null) return;

        if (effectData.CardEffectConfig is not BlockCardEffectConfig blockConfig)
        {
            Debug.LogError($"BlockCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }
        
        // context.CombatSystem.ResolveBlocks(context.Source, context.Target, blockConfig.Amount, true);
        BlockAction blockAction = new BlockAction(context.Source.CombatManager.CombatSystem,
            context.Source, context.Target, blockConfig.Amount, true);
        
        context.Source.CombatManager.ActionQueue.Enqueue(blockAction, null);
    }
}
