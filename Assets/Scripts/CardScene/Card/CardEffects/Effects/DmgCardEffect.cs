using System;
using UnityEngine;

[Serializable]
public class DmgCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(DmgCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatSystem == null || context.CardInstance == null) return;
        if (context.Source == null || context.Target == null) return;

        if (effectData.CardEffectConfig is not DmgCardEffectConfig dmgConfig)
        {
            Debug.LogError($"DmgCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }
        
        // TODO: Multi hit attacks
        context.CombatSystem.ResolveDamage(context.Source, context.Target, dmgConfig.Amount,
            context.CardInstance.Data.CardType);
    }
}
