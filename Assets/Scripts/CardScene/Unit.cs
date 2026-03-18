using System;
using UnityEngine;

[RequireComponent(typeof(BuffManager))]
public class Unit : MonoBehaviour, ICardUser, ICardTarget
{
    public Deck Deck { get; private set; }
    public PlayerHand Hand { get; private set; }
    public DiscardPile DiscardPile { get; private set; }
    public BuffManager BuffManager { get; private set; }
    
    [SerializeField] private int currentEnergy;
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentBlock;
    
    public int CurrentEnergy => currentEnergy;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentBlock => currentBlock;

    private void Awake()
    {
        if (BuffManager == null) BuffManager = GetComponent<BuffManager>();
    }

    public void SpendEnergy(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0)
        {
            Debug.LogWarning("SpendEnergy: Current energy is negative");
        }
            
    }

    public void InitializeCardZones(Deck deck, PlayerHand hand, DiscardPile discardPile)
    {
        this.Deck = deck;
        this.Hand = hand;
        this.DiscardPile = discardPile;
    }

    #region Draw Cards Healper
    
    public bool TryDrawToHand(out CardInstance drawnCard)
    {
        drawnCard = null;

        if (Deck == null || Hand == null || DiscardPile == null)
            return false;

        drawnCard = DrawOneCard();

        if (drawnCard == null)
            return false;

        if (!Hand.AddCard(drawnCard))
        {
            DiscardPile.AddCard(drawnCard);
            Debug.Log($"{name}'s hand is full. Sent drawn card to discard pile.");
        }

        return true;
    }

    public CardInstance DrawOneCard()
    {
        if (Deck.IsEmpty)
        {
            ReshuffleDiscardIntoDeck();
        }

        if (Deck.IsEmpty)
        {
            return null;
        }

        return Deck.DrawTopCard();
    }

    public void ReshuffleDiscardIntoDeck()
    {
        if (DiscardPile.IsEmpty) return;
        var discarded = DiscardPile.TransferAllCards();
        Deck.AddCards(discarded);
        Deck.Shuffle();
        
        Debug.Log($"{name} reshuffled discard pile into deck.");
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
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
    
    

    public void ReceiveBuff(BuffData buffData, int stacks)
    {
        BuffManager.ApplyBuff(buffData, stacks);
    }
    
    #endregion
}
