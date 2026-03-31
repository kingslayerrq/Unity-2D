using System.Collections.Generic;
using UnityEngine;

public class BasePlayerUnitInstance : BaseUnitInstance, ICardUser
{
    [SerializeField] private DeckData startingDeckData;
    [SerializeField] private PlayerHand hand;
    public Deck DeckInstance {get; private set;}
    public CardPileManager CardPileManager { get; private set; }
    
    [SerializeField] private int currentDrawAmount;
    [SerializeField] private int currentEnergy;
    [SerializeField] private int currentMaxEnergy;
    
    public PlayerHand Hand => hand;
    public int CurrentDrawAmount => currentDrawAmount;
    public int CurrentEnergy => currentEnergy;
    public int CurrentMaxEnergy => currentMaxEnergy;


    protected override void Awake()
    {
        base.Awake();
        if (CardPileManager == null) CardPileManager = GetComponent<CardPileManager>();
    }

    public override void Initialize()
    {
        base.Initialize();
        if (CardPileManager == null)
        {
            Debug.LogWarning("CardPileManager is null");
            return;
        }
        hand = new PlayerHand();
        
        // TODO: separate deck init
        DeckInstance = new Deck(startingDeckData);
        DeckInstance.InstantiateDeck(this);
        
        CardPileManager.InitCardPiles(new List<CardInstance>(DeckInstance.CardInstances));
    }
    
    #region Draw Cards Healper
    
    public bool TryDrawToHand(out CardInstance drawnCard)
    {
        drawnCard = null;
        if (CardPileManager == null)
        {
            Debug.LogWarning("CardPileManager is null");
            return false;
        }
        if (DeckInstance == null || hand == null || CardPileManager.DiscardPile == null 
            || CardPileManager.DrawPile == null)
            return false;

        drawnCard = CardPileManager.DrawOneCard();

        if (drawnCard == null)
            return false;

        // when add to hand fails
        if (!hand.AddCard(drawnCard))
        {
            CardPileManager.DiscardPile.AddCardToPile(drawnCard);
            Debug.Log($"{name}'s hand is full. Sent drawn card to discard pile.");
        }
        
        return true;
    }

    public bool DiscardCard(CardInstance discardedCard)
    {
        if (discardedCard == null) return false;

        if (hand.RemoveCard(discardedCard))
        {
            CardPileManager.DiscardPile.AddCardToPile(discardedCard);
            return true;
        }
        return false;
    }
    
    #endregion
    
    #region ICardUser
    public void ResetEnergy()
    {
        currentEnergy = currentMaxEnergy;
    }

    public void SpendEnergy(int amount)
    {
        currentEnergy -= amount;
    }
    #endregion
}
