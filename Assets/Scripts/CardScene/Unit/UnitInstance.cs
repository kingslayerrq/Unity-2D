using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UnitUIManager))]
[RequireComponent(typeof(BuffManager))]
[RequireComponent(typeof(CardPileManager))]
public abstract class BaseUnitInstance : MonoBehaviour, ICardTarget
{
    
    public BuffManager BuffManager { get; private set; }
    public UnitUIManager UnitUIManager { get; private set; }
    
    [SerializeField] private CombatManager combatManager;
    
    [Header("Instance stats")]
    [SerializeField] private UnitType unitType;
    
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentMaxHealth;
    [SerializeField] private int currentBlock;

    public event Action<int, int> OnHealthChanged;

    #region getters
        public CombatManager CombatManager => combatManager;
        
        public UnitType UnitType => unitType;
    
        public int CurrentHealth => currentHealth;
        public int CurrentMaxHealth => currentMaxHealth;
        public int CurrentBlock => currentBlock;
    #endregion
    

    protected virtual void Awake()
    {
        if (BuffManager == null) BuffManager = GetComponent<BuffManager>();
        if (UnitUIManager ==  null) UnitUIManager = GetComponent<UnitUIManager>();
    }

    

    public virtual void Initialize()
    {
        Debug.Log($"{name} initialize");
        
        if (BuffManager == null)
        {
            Debug.LogWarning("BuffManager is null");
            return;
        }
        
        InitHealth();
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

    

    private void InitHealth()
    {
        currentHealth =  currentMaxHealth;
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
    }
    
    #region ICardTargetImplementation

    public void TakeDamage(int amount)
    {
        Debug.Log($"{name} taking damage {amount}");
        int remainingDamage = amount;
        if (currentBlock > 0)
        {
            var blocked = Mathf.Min(currentBlock, remainingDamage);
            currentBlock -= blocked;
            remainingDamage -= blocked;
        }
        currentHealth -= remainingDamage;
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
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
    
    public BaseUnitInstance Target => this;

    #endregion
}
