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
        if (context.Source == null || context.Targets == null) return;

        if (effectData.CardEffectConfig is not DrawCardEffectConfig drawConfig)
        {
            Debug.LogError($"DrawCardEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }

        List<BasePlayerUnitInstance> playerTargets = null;
        if (drawConfig.SecondaryTarget != TargetType.None)
        {
            var secondaryTargets = context.CombatManager?.UnitsManager.
                GetUnitsByType(drawConfig.SecondaryTarget);
            if (secondaryTargets != null)
            {
                for (int i = 0; i < secondaryTargets.Count; i++)
                {
                    if (secondaryTargets[i] is BasePlayerUnitInstance player)
                    {
                        playerTargets ??= new List<BasePlayerUnitInstance>();
                
                        playerTargets.Add(player);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < context.Targets.Count; i++)
            {
                if (context.Targets[i] is BasePlayerUnitInstance player)
                {
                    playerTargets ??= new List<BasePlayerUnitInstance>();
                
                    playerTargets.Add(player);
                }
            }
        }
        
        
        if (playerTargets != null)
        {
            var drawAction = new DrawAction(
                context.CombatManager.CombatSystem,
                context.Source, 
                playerTargets,
                drawConfig.Amount
            );

            context.CombatManager.ActionQueue.Enqueue(drawAction, null);
        }
        
    }
}
