using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnemyShuffleCardsIntent : BaseEnemyIntent
{
    public override Type RequiredConfigType => typeof(EnemyShuffleCardsIntentConfig);
    public override void ExecuteIntent(EnemyIntentContext context, EnemyIntentData intentData)
    {
        if (context == null || context.CombatManager == null) return;
        if (context.Source == null || context.Targets == null ||
            context.Targets.Count == 0) return;

        if (intentData.EnemyIntentConfig is not EnemyShuffleCardsIntentConfig enemyShuffleCardsIntentConfig)
        {
            Debug.LogError($"EnemyShuffleCardsActionData received wrong config type" +
                           $" on {context.Intent}");
            return;
        }

        var playerTargets = context.Targets.OfType<BasePlayerUnitInstance>().ToList();
        if (playerTargets.Any())
        {
            ShuffleCardsAction shuffleCardsAction = new ShuffleCardsAction(context.CombatManager.CombatSystem,
                context.Source, null,
                playerTargets, enemyShuffleCardsIntentConfig.CardDatas,
                enemyShuffleCardsIntentConfig.TargetCardPiles);
            
            context.CombatManager.ActionQueue.Enqueue(shuffleCardsAction, null);
        }
        
    }

    public override BasePlayerUnitInstance ChoosePlayerTarget(CombatSnapshot snapshot)
    {
        return snapshot.AllPlayers[0] ?? null;
    }

    public override BaseEnemyUnitInstance ChooseEnemyAlly(CombatSnapshot snapshot)
    {
        return null;
    }

    public override BaseUnitInstance ChooseAnyTarget(CombatSnapshot snapshot)
    {
        return snapshot.AllPlayers[0] ?? null;
    }
}
