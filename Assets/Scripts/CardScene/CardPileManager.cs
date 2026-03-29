using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CardPileManager : MonoBehaviour
{
    public CardPile DrawPile { get; private set; } 
    public CardPile DiscardPile { get; private set;}
    public CardPile ExhaustPile { get; private set;}

    public event Action OnCardPilesInitialized;
    /**
     * Initialize draw pile with deck(List<CardInstance>)
     * Initialize discard/exhaust pile
     */
    public void InitCardPiles(List<CardInstance> deck)
    {
        if (deck.Count <= 0)
        {
            Debug.LogWarning($"{name} deck is empty.");
            return;
        }
        DrawPile = new CardPile(deck);
        ExhaustPile = new CardPile(new List<CardInstance>());
        DiscardPile = new CardPile(new List<CardInstance>());
        OnCardPilesInitialized?.Invoke();
        
        DrawPile.Shuffle();
    }

 

    [CanBeNull]
    public CardInstance DrawOneCard()
    {
        if (DrawPile.IsEmpty)
        {
            ReshuffleDiscardIntoDeck();
        }
        return DrawPile.IsEmpty ? null : DrawPile.DrawTopCard();
    }

    public void ReshuffleDiscardIntoDeck()
    {
        if (DiscardPile.IsEmpty) return;
        var discarded = DiscardPile.GetAllCards();
        DrawPile.AddCardsToPile(discarded);
        DiscardPile.Shuffle();
        Debug.Log($"{name} reshuffled discard pile into deck.");
    }
}
