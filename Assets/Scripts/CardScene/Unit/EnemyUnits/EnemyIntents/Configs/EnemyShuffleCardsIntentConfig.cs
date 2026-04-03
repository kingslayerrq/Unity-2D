using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EnemyShuffleCardsIntentConfig : BaseEnemyIntentConfig
{
    [SerializeField] private List<CardData> cardDatas = new List<CardData>();
    [SerializeField] private CardPileType targetCardPiles;
    
    public IReadOnlyList<CardData> CardDatas => cardDatas;
    public CardPileType TargetCardPiles => targetCardPiles;
}
