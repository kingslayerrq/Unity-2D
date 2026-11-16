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

        int resolvedDmgAmount = dmgConfig.Amount.Resolve(context);
        
        for (int i = 0; i < dmgConfig.NumberOfAttacks; i++)
        {
            DamageAction damageAction;
            if (dmgConfig.SecondaryTarget != TargetType.None)
            {
                var secondaryTarget = context.CombatManager?.UnitsManager.GetUnitsByType(dmgConfig.SecondaryTarget);
                if (secondaryTarget != null)
                {
                    damageAction = new DamageAction(context.CombatManager.CombatSystem,
                        context.Source,
                        secondaryTarget,
                        resolvedDmgAmount,
                        context.CardInstance.Data.CardType);
                }
                else
                {
                    Debug.LogError($"DmgCardEffectData secondary target {dmgConfig.SecondaryTarget} not found");
                    return;
                }
                
            }
            else
            {
                damageAction = new DamageAction(
                    context.CombatManager.CombatSystem, 
                    context.Source, 
                    context.Targets, 
                    resolvedDmgAmount, 
                    context.CardInstance.Data.CardType
                );
            }
            
            // Death is handled inside CombatSystem.ResolveDamage for the units actually hit.
            context.CombatManager.ActionQueue.Enqueue(damageAction, null);
        }
    }
}
