using System;
using UnityEngine;

[Serializable]
public class HealCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(HealCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Target == null) return;

        if (effectData.CardEffectConfig is not HealCardEffectConfig healConfig)
        {
            Debug.LogError($"HealCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }
        
        // context.CombatSystem.ResolveHeal(context.Source, context.Target, healConfig.Amount);

        HealAction healAction = new HealAction(context.CombatManager.CombatSystem,
                context.Source, context.Target, healConfig.Amount);
        
        context.CombatManager.ActionQueue.Enqueue(healAction, null);
    }
}
