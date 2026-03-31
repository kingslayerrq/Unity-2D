using System;
using UnityEngine;

[Serializable]
public class DmgCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(DmgCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Target == null) return;

        if (effectData.CardEffectConfig is not DmgCardEffectConfig dmgConfig)
        {
            Debug.LogError($"DmgCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }
        
        
        DamageAction damageAction = new DamageAction(
            context.CombatManager.CombatSystem, 
            context.Source, 
            context.Target, 
            dmgConfig.Amount, 
            context.CardInstance.Data.CardType
        );
        context.CombatManager.ActionQueue.Enqueue(damageAction, null);
    }
}
