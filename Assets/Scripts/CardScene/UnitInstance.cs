using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIManager))]
[RequireComponent(typeof(BuffManager))]
[RequireComponent(typeof(CardPileManager))]
public class UnitInstance : MonoBehaviour, ICardUser, ICardTarget
{
    [SerializeField] private DeckData startingDeckData;
    public Deck DeckInstance {get; private set;}
    public PlayerHand Hand { get; private set; }
    public CardPileManager CardPileManager { get; private set; }
    public BuffManager BuffManager { get; private set; }
    public UIManager UIManager { get; private set; }
    
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private int currentDrawAmount;
    [SerializeField] private int currentEnergy;
    [SerializeField] private int currentMaxEnergy;
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentMaxHealth;
    [SerializeField] private int currentBlock;
    
    public CombatManager CombatManager => combatManager;
    public int CurrentDrawAmount => currentDrawAmount;
    public int CurrentEnergy => currentEnergy;
    public int CurrentMaxEnergy => currentMaxEnergy;
    public int CurrentHealth => currentHealth;
    public int CurrentMaxHealth => currentMaxHealth;
    public int CurrentBlock => currentBlock;

    private void Awake()
    {
        if (BuffManager == null) BuffManager = GetComponent<BuffManager>();
        if (UIManager ==  null) UIManager = GetComponent<UIManager>();
        if (CardPileManager == null) CardPileManager = GetComponent<CardPileManager>();
    }

    

    public void Initialize()
    {
        Debug.Log($"{name} initialize");
        if (CardPileManager == null)
        {
            Debug.LogWarning("CardPileManager is null");
            return;
        }
        if (BuffManager == null)
        {
            Debug.LogWarning("BuffManager is null");
            return;
        }

        Hand = new PlayerHand();
        
        // TODO: separate deck init
        DeckInstance = new Deck(startingDeckData);
        DeckInstance.InstantiateDeck();
        
        CardPileManager.InitCardPiles(new List<CardInstance>(DeckInstance.CardInstances));
        
    }


    public void ResetEnergy()
    {
        currentEnergy = currentMaxEnergy;
    }

    public void SpendEnergy(int amount)
    {
        currentEnergy -= amount;
    }

    public void ResetBlock(ResetBlockEvent resetEvent)
    {
        switch (resetEvent.ResetSource)
        {
            case BlockResetSource.None:
                break;
            case BlockResetSource.BuffEffect:
                break;
            case BlockResetSource.CardEffect:
                break;
            case BlockResetSource.TurnStart:
                currentBlock = 0;
                break;
            case BlockResetSource.TurnEnd:
                currentBlock = 0;
                break;
            default:
                break;
        }
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
        if (DeckInstance == null || Hand == null || CardPileManager.DiscardPile == null 
            || CardPileManager.DrawPile == null)
            return false;

        drawnCard = CardPileManager.DrawOneCard();

        if (drawnCard == null)
            return false;

        // when add to hand fails
        if (!Hand.AddCard(drawnCard))
        {
            CardPileManager.DiscardPile.AddCardToPile(drawnCard);
            Debug.Log($"{name}'s hand is full. Sent drawn card to discard pile.");
        }

        return true;
    }

    public bool DiscardCard(CardInstance discardedCard)
    {
        if (discardedCard == null) return false;

        if (Hand.RemoveCard(discardedCard))
        {
            CardPileManager.DiscardPile.AddCardToPile(discardedCard);
            return true;
        }
        return false;
    }
    
    #endregion
    
    
    
    #region ICardTargetImplementation

    public void TakeDamage(int amount)
    {
        int remainingDamage = amount;
        if (currentBlock > 0)
        {
            var blocked = Mathf.Min(currentBlock, remainingDamage);
            currentBlock -= blocked;
            remainingDamage -= blocked;
        }
        currentHealth -= remainingDamage;
        if (currentHealth <= 0)
        {
            // TODO: die
        }
    }

    public void Block(int amount)
    {
        currentBlock += amount;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, currentMaxHealth);
    }
    
    

    public void ReceiveBuff(BuffData buffData, int stacks)
    {
        BuffManager.ApplyBuff(buffData, stacks);
    }
    
    #endregion
}
