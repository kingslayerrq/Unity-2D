using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ShuffleCardsEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(ShuffleCardsEffectConfig);
    
    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null || context.Targets == null) return;
        
        if (effectData.CardEffectConfig is not ShuffleCardsEffectConfig shuffleConfig)
        {
            Debug.LogError($"ShuffleCardsEffectData received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }

        List<BasePlayerUnitInstance> playerTargets = null;

        if (shuffleConfig.SecondaryTarget != TargetType.None)
        {
            var secondaryTargets = context.CombatManager.UnitsManager.GetUnitsByType(shuffleConfig.SecondaryTarget);
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
            List<CardInstance> cardsToShuffle = new List<CardInstance>();
            if (shuffleConfig.DuplicateCardInstance)
            {
                for (int i = 0; i < shuffleConfig.DuplicateAmount; i++)
                {
                    var ci = context.CardInstance?.Clone();
                    cardsToShuffle.Add(ci);
                }
            }
            else
            {
                cardsToShuffle.AddRange(GenerateCardsFromRecipes(shuffleConfig.CardDatas, context.CardInstance));
            }
            ShuffleCardsAction shuffleCardsAction = new ShuffleCardsAction(context.CombatManager.CombatSystem,
                context.Source, context.CardInstance, playerTargets, cardsToShuffle,
                shuffleConfig.TargetCardPiles);
            
            context.CombatManager.ActionQueue.Enqueue(shuffleCardsAction, null);
        }
        
    }
    private List<CardInstance> GenerateCardsFromRecipes(IReadOnlyList<DynamicCardInstantiateRecipe> recipes, CardInstance sourceCard)
    {
        List<CardInstance> newCards = new List<CardInstance>();

        foreach (var recipe in recipes)
        {
            CardInstance newCardInstance = new CardInstance(recipe.BaseCard); 

            // Check if it should be upgraded
            bool shouldUpgrade = recipe.AlwaysSpawnUpgraded || 
                                 (recipe.InheritUpgradeState && sourceCard.CurrentIsUpgraded);

            if (shouldUpgrade)
            {
                newCardInstance.Upgrade(); 
            }

            newCards.Add(newCardInstance);
        }

        return newCards;
    }
}
