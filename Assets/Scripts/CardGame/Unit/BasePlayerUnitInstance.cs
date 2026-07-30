using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BasePlayerUnitInstance : BaseUnitInstance, ICardUser
{
    [SerializeField] private PlayerUnitData playerUnitData;
    [SerializeField] private PlayerHand hand;
    
    public Deck DeckInstance {get; private set;}
    public CardPileManager CardPileManager { get; private set; }

    [SerializeField] private DeckData currentStartingDeckData;
    [SerializeField] private int currentDrawAmount;
    [SerializeField] private int currentEnergy;
    [SerializeField] private int currentMaxEnergy;
    
    public event Action<int, int> OnEnergyChanged;
    public event Action OnPlayerDeckInitialized;
    
    public PlayerUnitData PlayerUnitData => playerUnitData;
    public PlayerHand Hand => hand;
    public int CurrentDrawAmount => currentDrawAmount;
    public int CurrentEnergy => currentEnergy;
    public int CurrentMaxEnergy => currentMaxEnergy;
    
    

    public BasePlayerUnitInstance(PlayerUnitData playerUnitData)
    {
        this.playerUnitData = playerUnitData;
        
        this.currentName = playerUnitData.UnitName;
        this.currentDescription = playerUnitData.UnitDescription;
        this.currentSprite = playerUnitData.UnitSprite;
        this.currentUnitType = playerUnitData.UnitType;
        this.currentStartingDeckData = playerUnitData.StartingDeckData;
        this.currentDrawAmount = playerUnitData.DrawAmount;
        this.currentMaxEnergy = playerUnitData.MaxEnergy;
        this.currentMaxHealth = playerUnitData.MaxHealth;
        this.isInitialized = false;
    }

    public void Initialize(CombatManager combatManager, BuffManager buffManager, 
        CardPileManager cardPileManager)
    {
        base.Initialize(combatManager, buffManager);
        
        this.CardPileManager = cardPileManager;
        if (CardPileManager == null)
        {
            Debug.LogWarning("CardPileManager is null");
            return;
        }
        if (DeckInstance == null)
        {
            DeckInstance = new Deck(currentStartingDeckData);
            DeckInstance.InstantiateDeck(this);
            Debug.Log($"Invoked deck init");
            OnPlayerDeckInitialized?.Invoke();
        }
        
        CardPileManager.InitCardPiles(new List<CardInstance>(DeckInstance.CardInstances));
        hand = new PlayerHand();
    }

    protected override void OnBattleStart()
    {
        InitHealth();
        InitEnergy();
    }

    protected override void OnBattleSetup()
    {
    }

    protected override void OnVictory()
    {
        Debug.Log($"Player - {CurrentName} Victory");
        hand?.Clear();
    }

    protected override void OnDefeat()
    {
        Debug.Log($"Player - {CurrentName} Defeat");
        hand?.Clear();
        currentEnergy = 0;
    }

    protected void InitEnergy()
    {
        currentEnergy = currentMaxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy, currentMaxEnergy);
    }

    public override void Dispose()
    {
        Debug.Log($"Player - {CurrentName} Disposed");

        if (CombatManager != null)
        {
            CombatManager.OnStateChange -= HandleCombatStateChange;
            if (CombatManager.UnitsManager != null)
            {
                CombatManager.UnitsManager.RemoveUnit(this);
            }
        }
        NotifyDisposed();
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
            Debug.Log($"{currentName}'s hand is full. Sent drawn card to discard pile.");
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

    /// <summary>
    /// Exhause the card instance from the unit's hand add to exhaust pile
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public bool ExhaustCard(CardInstance card)
    {
        if (card == null) return false;

        if (hand.RemoveCard(card))
        {
            CardPileManager.ExhaustPile.AddCardToPile(card);
            return true;
        }
        return false;
    }
    
    #endregion
    
    #region ICardUser
    public void ResetEnergy()
    {
        currentEnergy = currentMaxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy, currentMaxEnergy);
    }

    public void SpendEnergy(int amount)
    {
        Debug.Log($"Player - {CurrentName} Spend Energy");
        currentEnergy -= amount;
        OnEnergyChanged?.Invoke(currentEnergy, currentMaxEnergy);
    }

    public void GainEnergy(int amount)
    {
        currentEnergy += amount;
        OnEnergyChanged?.Invoke(currentEnergy, currentMaxEnergy);
    }
    #endregion
    
}
