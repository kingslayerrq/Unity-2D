using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DmgCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(DmgCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Targets == null) return;

        if (effectData.CardEffectConfig is not DmgCardEffectConfig dmgConfig)
        {
            Debug.LogError($"DmgCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }
        
        
        for (int i = 0; i < dmgConfig.NumberOfAttacks; i++)
        {
            DamageAction damageAction = new DamageAction(
                context.CombatManager.CombatSystem, 
                context.Source, 
                context.Targets, 
                dmgConfig.Amount, 
                context.CardInstance.Data.CardType
            );
            
            context.CombatManager.ActionQueue.Enqueue(damageAction, () => CheckTargetsHealth(context.Targets));
        }
    }

    private void CheckTargetsHealth(IReadOnlyList<BaseUnitInstance> targets)
    {
        foreach (var target in targets)
        {
            target.OnDmgResolved();
        }
    }

    
}
