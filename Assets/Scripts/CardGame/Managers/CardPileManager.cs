using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// A player's draw, discard and exhaust piles for one combat. Plain C#; the
/// pile counters on screen are bound to it by <see cref="CardPilePresenter"/>.
/// </summary>
public class CardPileManager
{
    public CardPile DrawPile { get; private set; }
    public CardPile DiscardPile { get; private set;}
    public CardPile ExhaustPile { get; private set;}

    public event Action OnCardPilesInitialized;

    // The combat's random stream, kept for reshuffles later in the combat.
    private System.Random rng;

    /**
     * Initialize draw pile with deck(List<CardInstance>)
     * Initialize discard/exhaust pile
     */
    public void InitCardPiles(List<CardInstance> deck, System.Random rng)
    {
        this.rng = rng ?? throw new ArgumentNullException(nameof(rng));

        if (deck.Count <= 0)
        {
            Debug.LogWarning("Cannot set up card piles: the deck is empty.");
            return;
        }
        DrawPile = new CardPile(deck, CardPileType.Draw);
        ExhaustPile = new CardPile(new List<CardInstance>(), CardPileType.Exhaust);
        DiscardPile = new CardPile(new List<CardInstance>(), CardPileType.Discard);

        DrawPile.Shuffle(this.rng);

        OnCardPilesInitialized?.Invoke();
    }

    [CanBeNull]
    public CardPile GetCardPile(CardPileType type)
    {
        switch (type)
        {
            case CardPileType.Discard:
                return DiscardPile;
            case CardPileType.Exhaust:
                return ExhaustPile;
            case CardPileType.Draw:
                return DrawPile;
            default:
                return null;
        }
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
        DrawPile.Shuffle(this.rng);
        Debug.Log("Reshuffled discard pile into deck.");
    }
}
