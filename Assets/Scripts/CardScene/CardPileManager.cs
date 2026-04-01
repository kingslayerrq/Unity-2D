using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CardPileManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private DrawPileUI drawPileUI;
    [SerializeField] private DiscardPileUI discardPileUI;
    public CardPile DrawPile { get; private set; } 
    public CardPile DiscardPile { get; private set;}
    public CardPile ExhaustPile { get; private set;}

    public event Action OnCardPilesInitialized;

    private void Awake()
    {
        if (drawPileUI != null) drawPileUI.Initialize(this);
        if (discardPileUI != null) discardPileUI.Initialize(this);
    }

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
        
        DrawPile.Shuffle();
        
        OnCardPilesInitialized?.Invoke();
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
