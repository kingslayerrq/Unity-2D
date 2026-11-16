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

    protected BaseUnitInstance lastDamagedBy = null;

    // True once this unit's defeat has been queued, so it is only handled once.
    private bool isDefeatPending = false;

    public bool IsDead => currentHealth <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnBlockChanged;
    public event Action<bool> OnHighlightTriggered;
    public event Action OnDisposed;
    

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
        if (!isInitialized)
            currentHealth = currentMaxHealth;
        isInitialized = true;
    }

    protected virtual void HandleCombatStateChange(CombatManager.NodeState combatState)
    {
        switch (combatState)
        {
            case CombatManager.NodeState.BattleSetup:
                OnBattleSetup();
                break;
            case CombatManager.NodeState.BattleStart:
                OnBattleStart();
                break;
            case CombatManager.NodeState.Victory:
                BuffManager?.ClearAllBuffs();
                OnVictory();
                break;
            case CombatManager.NodeState.Defeat:
                BuffManager?.ClearAllBuffs();
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
                ResetBlock();
                break;
            case BlockResetSource.TurnEnd:
                ResetBlock();
                break;
            default:
                break;
        }
    }

    protected void ResetBlock()
    {
        currentBlock = 0;
        OnBlockChanged?.Invoke(currentBlock);
    }
    
    /// <summary>
    /// Set the current health to max or current based on <value>isInitialized</value>
    /// Called on battle start (battle scene setup)
    /// </summary>
    protected void InitHealth()
    {
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
    }

    public void TriggerHighlight(bool showHighlight)
    {
        OnHighlightTriggered?.Invoke(showHighlight);
    }

    /// <summary>
    /// Call Dispose when defeated
    /// </summary>
    public abstract void Dispose();

    protected void NotifyDisposed() => OnDisposed?.Invoke();
    
    #region ICardTargetImplementation

    public void TakeDamage(int amount, BaseUnitInstance source)
    {
        Debug.Log($"{CurrentName} taking damage {amount}");
        int remainingDamage = amount;
        if (currentBlock > 0)
        {
            var blocked = Mathf.Min(currentBlock, remainingDamage);
            currentBlock -= blocked;
            OnBlockChanged?.Invoke(currentBlock);
            remainingDamage -= blocked;
        }
        if (remainingDamage > 0) lastDamagedBy = source;
        currentHealth = Mathf.Max(currentHealth - remainingDamage, 0);
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
        
    }

    public void Block(int amount)
    {
        currentBlock += amount;
        OnBlockChanged?.Invoke(currentBlock);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, currentMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
    }
    
    

    public void ReceiveBuff(BuffData buffData, int stacks)
    {
        if (BuffManager == null) return;

        // A timed buff applied during the enemy turn must survive this round's
        // countdown, or it would expire before the enemy turn it is meant for.
        bool appliedDuringEnemyTurn = CombatManager != null && CombatManager.IsEnemyTurn;
        BuffManager.ApplyBuff(buffData, stacks, appliedDuringEnemyTurn);
    }
    
    /// <summary>
    /// Queues this unit's defeat once it is dead. Safe to call repeatedly:
    /// only the first call after death has any effect, so multi-hit attacks
    /// cannot dispose the same unit twice.
    /// </summary>
    public void OnDmgResolved()
    {
        Debug.Log($"{CurrentName} OnDmgResolved, final health: {currentHealth}");
        if (IsDead && !isDefeatPending)
        {
            isDefeatPending = true;
            if (lastDamagedBy != null && lastDamagedBy.CombatManager != null)
            {
                lastDamagedBy.CombatManager.ActionQueue.Enqueue(new DefeatUnitAction(lastDamagedBy.CombatManager.CombatSystem,
                    lastDamagedBy, this), Dispose);
            }
            else
            {
                Dispose();
            }
        }
    }

    public BaseUnitInstance Target => this;

    #endregion
}
