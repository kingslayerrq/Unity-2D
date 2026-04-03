using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShuffleCardsEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private CardPileType targetCardPiles;
    [SerializeField] private List<CardData> cardDatas = new List<CardData>();

    public CardPileType TargetCardPiles => targetCardPiles;
    public IReadOnlyList<CardData> CardDatas => cardDatas;
}
