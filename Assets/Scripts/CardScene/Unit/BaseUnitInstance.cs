using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public abstract class BaseUnitInstance :  ICardTarget
{
    
    public BuffManager BuffManager { get; protected set; }
    
    public CombatManager CombatManager { get; protected set; }

    [SerializeField] protected string currentName;
    [SerializeField] protected string currentDescription;
    [SerializeField] protected Sprite currentSprite;
    
    [Header("Instance stats")]
    [SerializeField] protected UnitType currentUnitType;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int currentMaxHealth;
    [SerializeField] protected int currentBlock;

    protected bool isInitialized = false;
    
    public bool IsDead => currentHealth <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action<bool> OnHighlightTriggered;
    

    #region getters
    
        public string CurrentName => currentName;
        public string CurrentDescription => currentDescription;
        public Sprite CurrentSprite => currentSprite;
    
        public UnitType UnitType => currentUnitType;
        public int CurrentHealth => currentHealth;
        public int CurrentMaxHealth => currentMaxHealth;
        public int CurrentBlock => currentBlock;
        public bool IsInitialized => isInitialized;
    #endregion
    

    /// <summary>
    /// Initialize should be called on each battle start
    /// </summary>
    /// <param name="combatManager"></param>
    /// <param name="buffManager"></param>
    public virtual void Initialize(CombatManager combatManager, BuffManager buffManager)
    {
        if (CombatManager != null)
        {
            CombatManager.OnStateChange -= HandleCombatStateChange;
        }
        CombatManager = combatManager;
        BuffManager = buffManager;
        if (this.CombatManager != null)
        {
            this.CombatManager.OnStateChange += HandleCombatStateChange;
        }
        Debug.Log($"{CurrentName} Initialize");
        currentHealth = currentMaxHealth;
        isInitialized = true;
    }

    protected virtual void HandleCombatStateChange(CombatManager.CombatState combatState)
    {
        switch (combatState)
        {
            case CombatManager.CombatState.BattleSetup:
                OnBattleSetup();
                break;
            case CombatManager.CombatState.BattleStart:
                OnBattleStart();
                break;
            case CombatManager.CombatState.Victory:
                OnVictory();
                break;
            case CombatManager.CombatState.Defeat:
                OnDefeat();
                break;
            default:
                break;
        }
    }
    
    protected abstract void OnBattleStart();
    protected abstract void OnBattleSetup();
    protected abstract void OnVictory();
    protected abstract void OnDefeat();
    

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

    

    /// <summary>
    /// Set the current health to max or current based on <value>isInitialized</value>
    /// Called on battle start (battle scene setup)
    /// </summary>
    protected void InitHealth()
    {
        Debug.Log($"{CurrentName} InitHealth {isInitialized} - {currentHealth}");
        currentHealth = isInitialized ? currentHealth : currentMaxHealth;
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
    }

    public void TriggerHighlight(bool showHighlight)
    {
        OnHighlightTriggered?.Invoke(showHighlight);
    }

    /// <summary>
    /// Call Dispose when defeated
    /// </summary>
    public virtual void Dispose() 
    {
        if (CombatManager != null)
        {
            CombatManager.OnStateChange -= HandleCombatStateChange;
        }
    }
    
    #region ICardTargetImplementation

    public void TakeDamage(int amount)
    {
        Debug.Log($"{CurrentName} taking damage {amount}");
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
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
    }
    
    

    public void ReceiveBuff(BuffData buffData, int stacks)
    {
        if (BuffManager == null) return;
        BuffManager.ApplyBuff(buffData, stacks);
    }
    
    public BaseUnitInstance Target => this;

    #endregion
}
