using System.Collections.Generic;
using UnityEngine;

public class DiscardPile
{
    private readonly List<CardInstance> discardPile = new List<CardInstance>();
    
    public int Count => discardPile.Count;
    public bool IsEmpty => discardPile.Count == 0;

    public void AddCard(CardInstance card)
    {
        if (card == null) return;
        discardPile.Add(card);
    }

    public List<CardInstance> TransferAllCards()
    {
        var cards = new List<CardInstance>(discardPile);
        discardPile.Clear();
        return  cards;
    }
}
