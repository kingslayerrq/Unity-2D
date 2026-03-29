using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;
using Random = System.Random;

public class CardPile
{
    private readonly List<CardInstance> cards;
    private readonly Random rng = new();
    
    public int Count => cards.Count;
    public bool IsEmpty => Count == 0;
    
    public event Action<int> OnCardPileCountChanged;

    public CardPile(List<CardInstance> newCards)
    {
        cards = newCards;
    }

    public void Refresh()
    {
        OnCardPileCountChanged?.Invoke(cards.Count);
    }

    public void AddCardToPile(CardInstance card)
    {
        if (card == null) return;
        cards.Add(card);
        OnCardPileCountChanged?.Invoke(cards.Count);
    }

    public void AddCardsToPile(IEnumerable<CardInstance> newCards)
    {
        if (newCards == null) return;
        foreach (CardInstance card in newCards)
        {
            cards.Add(card);
        }
        OnCardPileCountChanged?.Invoke(cards.Count);
    }

    [CanBeNull]
    public CardInstance DrawTopCard()
    {
        if (cards.Count == 0) return null;

        CardInstance card = cards[0];
        cards.RemoveAt(0);
        OnCardPileCountChanged?.Invoke(cards.Count);
        return card;
    }

    /**
     * Return all cards
     * Clear the card pile
     */
    public List<CardInstance> GetAllCards()
    {
        var allCards = new List<CardInstance>(cards);
        cards.Clear();
        OnCardPileCountChanged?.Invoke(cards.Count);
        return allCards;
    }

    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }
}
