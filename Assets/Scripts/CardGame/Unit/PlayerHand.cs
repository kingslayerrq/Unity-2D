using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerHand
{
    [SerializeField] private List<CardInstance> cards = new List<CardInstance>();
    private readonly int maxHandSize = 10;
    public IReadOnlyList<CardInstance> Cards => cards;
    
    public int Count => cards.Count;
    public bool IsEmpty => cards.Count == 0;
    public bool IsFull => cards.Count == maxHandSize;
    
    public bool AddCard(CardInstance card)
    {
        if (card == null) return false;
        if (IsFull) return false;
        cards.Add(card);
        card.CurrentCardState = CardState.InHand;
        return true;
    }
    public bool RemoveCard(CardInstance card)
    {
        if (card == null) return false;
        return cards.Remove(card);
    }
    
    public void Clear()
    {
        foreach (var card in cards)
        {
            card.CurrentCardState = CardState.InDeck; // reset state
        }
        cards.Clear();
    }
    
}
