using System;
using System.Collections.Generic;

public class Deck
{
    private readonly List<CardInstance> cards = new();
    private readonly Random rng = new();

    public int Count => cards.Count;
    public bool IsEmpty => cards.Count == 0;

    public void AddCard(CardInstance card)
    {
        if (card == null) return;
        cards.Add(card);
    }

    public void AddCards(IEnumerable<CardInstance> newCards)
    {
        if (newCards == null) return;

        foreach (CardInstance card in newCards)
        {
            if (card != null)
                cards.Add(card);
        }
    }

    public CardInstance DrawTopCard()
    {
        if (cards.Count == 0) return null;

        CardInstance card = cards[0];
        cards.RemoveAt(0);
        return card;
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