using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UnitUIManager))]
[RequireComponent(typeof(BuffManager))]
[RequireComponent(typeof(CardPileManager))]
public abstract class BaseUnitInstance : MonoBehaviour, ICardTarget
{
    
    public BuffManager BuffManager { get; protected set; }
    public UnitUIManager UnitUIManager { get; protected set; }
    
    [SerializeField] protected CombatManager combatManager;

    [SerializeField] protected string currentName;
    [SerializeField] protected string currentDescription;
    [SerializeField] protected Sprite currentSprite;
    
    [Header("Instance stats")]
    [SerializeField] protected UnitType unitType;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int currentMaxHealth;
    [SerializeField] protected int currentBlock;

    protected bool isInitialized = false;

    public event Action<int, int> OnHealthChanged;

    #region getters
        public CombatManager CombatManager => combatManager;
    
        public string CurrentName => currentName;
        public string CurrentDescription => currentDescription;
        public Sprite CurrentSprite => currentSprite;
    
        public UnitType UnitType => unitType;
        public int CurrentHealth => currentHealth;
        public int CurrentMaxHealth => currentMaxHealth;
        public int CurrentBlock => currentBlock;
        public bool IsInitialized => isInitialized;
    #endregion
    

    protected virtual void Awake()
    {
        if (BuffManager == null) BuffManager = GetComponent<BuffManager>();
        if (UnitUIManager ==  null) UnitUIManager = GetComponent<UnitUIManager>();
    }


    public abstract void Initialize();

    public abstract void OnBattleStart();

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

    

    // <summary>
    // Set currentHealth to currentMaxHealth
    // Use on battle start
    // <summary>
    protected void InitHealth()
    {
        currentHealth = isInitialized ? currentHealth : currentMaxHealth;
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
