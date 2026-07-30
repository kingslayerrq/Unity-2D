

using System;
using UnityEngine;

[Serializable]
public class CardPileCountCondition : BaseCardCondition
{
    [SerializeField] private CardPileType pileType;
    [SerializeField] private NumericComparison comparison;
    [SerializeField, Min(0)] private int requiredCount;

    public override bool IsMet(CardContext context)
    {
        BasePlayerUnitInstance owner =
            context?.CardInstance?.CurrentOwner;

        CardPile pile = owner?.CardPileManager?.GetCardPile(pileType);
        if (pile == null)
            return false;

        return Compare(pile.Count, requiredCount);
    }

    private bool Compare(int actual, int expected)
    {
        return comparison switch
        {
            NumericComparison.Equal => actual == expected,
            NumericComparison.NotEqual => actual != expected,
            NumericComparison.GreaterThan => actual > expected,
            NumericComparison.GreaterThanOrEqual => actual >= expected,
            NumericComparison.LessThan => actual < expected,
            NumericComparison.LessThanOrEqual => actual <= expected,
            _ => false
        };
    }
}