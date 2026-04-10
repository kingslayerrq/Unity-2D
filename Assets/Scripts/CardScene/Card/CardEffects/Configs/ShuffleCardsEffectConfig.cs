using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShuffleCardsEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private CardPileType targetCardPiles;
    [Header("Spawn Settings")]
    [Tooltip("Used if Duplicate is false.")]
    [SerializeField] private List<DynamicCardInstantiateRecipe> cardDatas = new List<DynamicCardInstantiateRecipe>();

    [Header("Duplication Settings")]
    [Tooltip("If true, ignores the recipes and instead shuffles exact copies of the card that played this effect.")]
    [SerializeField] private bool duplicateCardInstance = false;
    [SerializeField] private int duplicateAmount;

    public bool DuplicateCardInstance => duplicateCardInstance;
    public int DuplicateAmount => duplicateAmount;
    public CardPileType TargetCardPiles => targetCardPiles;
    public IReadOnlyList<DynamicCardInstantiateRecipe> CardDatas => cardDatas;
}
