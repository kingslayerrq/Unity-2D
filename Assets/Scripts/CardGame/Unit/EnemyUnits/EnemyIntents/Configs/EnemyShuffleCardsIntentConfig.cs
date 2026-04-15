using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EnemyShuffleCardsIntentConfig : BaseEnemyIntentConfig
{
    [SerializeField] private List<DynamicCardInstantiateRecipe> cardDatas = new List<DynamicCardInstantiateRecipe>();
    [SerializeField] private CardPileType targetCardPiles;

    public IReadOnlyList<DynamicCardInstantiateRecipe> CardDatas => cardDatas;
    public CardPileType TargetCardPiles => targetCardPiles;
}
