using System.Collections.Generic;
using UnityEngine;

public class PlayerHand
{
    private readonly List<CardInstance> cards = new();
    private readonly int maxHandSize = 10;
    public IReadOnlyList<CardInstance> Cards => cards;
    public bool AddCard(CardInstance card)
    {
        if (card == null) return false;
        if (IsFull) return false;
        cards.Add(card);
        return true;
    }
    public bool RemoveCard(CardInstance card)
    {
        if (card == null) return false;
        return cards.Remove(card);
    }
    public int Count => cards.Count;
    public bool IsEmpty => cards.Count == 0;
    public bool IsFull => cards.Count == maxHandSize;
}
