using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GainEnergyCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(GainEnergyCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null) return;

        if (effectData.CardEffectConfig is not GainEnergyCardEffectConfig gainConfig)
        {
            Debug.LogError($"GainEnergyCardEffect received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }

        List<BasePlayerUnitInstance> playerTargets;
        if (gainConfig.SecondaryTarget != TargetType.None)
        {
            var units = context.CombatManager.UnitsManager.GetUnitsByType(gainConfig.SecondaryTarget);
            playerTargets = units.OfType<BasePlayerUnitInstance>().ToList();
        }
        else if (context.Source is BasePlayerUnitInstance sourcePlayer)
        {
            playerTargets = new List<BasePlayerUnitInstance> { sourcePlayer };
        }
        else
        {
            Debug.LogError($"GainEnergyCardEffect source is not a player on card {context.CardInstance?.Data?.CardName}");
            return;
        }

        context.CombatManager.ActionQueue.Enqueue(
            new GainEnergyAction(context.CombatManager.CombatSystem, context.Source, playerTargets, gainConfig.Amount),
            null);
    }
}
