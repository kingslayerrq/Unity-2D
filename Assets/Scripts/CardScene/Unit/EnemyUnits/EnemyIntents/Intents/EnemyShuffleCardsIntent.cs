using System;
using System.Collections.Generic;
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
                playerTargets, GenerateCardsFromRecipes(enemyShuffleCardsIntentConfig.CardDatas),
                enemyShuffleCardsIntentConfig.TargetCardPiles);
            
            context.CombatManager.ActionQueue.Enqueue(shuffleCardsAction, null);
        }
        
    }
    
    private List<CardInstance> GenerateCardsFromRecipes(IReadOnlyList<DynamicCardInstantiateRecipe> recipes)
    {
        List<CardInstance> newCards = new List<CardInstance>();

        foreach (var recipe in recipes)
        {
            CardInstance newCardInstance = new CardInstance(recipe.BaseCard); 

            // Check if it should be upgraded
            bool shouldUpgrade = recipe.AlwaysSpawnUpgraded;

            if (shouldUpgrade)
            {
                newCardInstance.Upgrade(); 
            }

            newCards.Add(newCardInstance);
        }

        return newCards;
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
