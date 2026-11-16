using System;
using System.Collections.Generic;
using CardScene.Combat.Hooks;
using UnityEngine;

/// <summary>
/// Holds and manages all active buff instances on a unit. Plain C#: each unit
/// owns one, with or without a view on screen.
/// 
/// Responsibilities:
/// 1. Apply new buffs
/// 2. Stack existing buffs of the same BuffData
/// 3. Dispatch gameplay hook calls to buffs that implement the matching interface
/// 4. Remove expired buffs after relevant events
/// 
/// This manager does NOT contain buff logic itself.
/// Each BuffInstance implements its own behavior through small hook interfaces
/// like IOnTurnEnd, IOnBeforeDealDamage, etc.
/// </summary>
public class BuffManager
{
    /// <summary>
    /// Runtime list of buffs currently active on this unit.
    /// Each entry is a BuffInstance, not BuffData, because BuffInstance holds runtime state
    /// such as current stacks, duration, temporary values, etc.
    /// </summary>
    private readonly List<BuffInstance> activeBuffs = new();

    public IReadOnlyList<BuffInstance> ActiveBuffs => activeBuffs;

    public event Action OnBuffsChanged;

    /// <summary>
    /// Applies a buff to this unit.
    /// 
    /// If a matching buff already exists (same BuffData asset), it adds stacks to it.
    /// Otherwise, it creates a new BuffInstance and adds it to the active list.
    /// </summary>
    /// <param name="data">The static buff definition asset.</param>
    /// <param name="stacks">How many stacks/duration to add.</param>
    /// <param name="skipNextDurationTick">
    /// If a new instance is created, it skips its first round-end countdown.
    /// </param>
    public void ApplyBuff(BuffData data, int stacks, bool skipNextDurationTick = false)
    {
        if (data == null || stacks <= 0)
            return;

        BuffInstance existingBuffInstance = FindBuff(data);

        if (existingBuffInstance != null)
        {
            existingBuffInstance.AddStacks(stacks);
        }
        else
        {
            BuffInstance newBuffInstance = data.CreateInstance(stacks);
            newBuffInstance.SkipNextDurationTick = skipNextDurationTick;
            activeBuffs.Add(newBuffInstance);
        }

        OnBuffsChanged?.Invoke();
    }

    /// <summary>
    /// Generic dispatch helper.
    /// 
    /// This loops through all active buffs and checks:
    /// "Does this buff implement the requested hook interface?"
    /// 
    /// If yes, invoke the provided callback on that interface.
    /// 
    /// Example:
    /// Dispatch<IOnTurnEnd>(handler => handler.OnTurnEnd(owner));
    /// 
    /// Means:
    /// - Find all buffs that implement IOnTurnEnd
    /// - Call OnTurnEnd(owner) on each of them
    /// </summary>
    /// <typeparam name="THandler">
    /// The interface type to dispatch to, such as IOnTurnEnd or IOnBeforeDealDamage.
    /// </typeparam>
    /// <param name="invoke">
    /// The action to run on each matching handler.
    /// </param>
    private void Dispatch<THandler>(Action<THandler> invoke) where THandler : class
    {
        foreach (BuffInstance buffInstance in activeBuffs)
        {
            if (buffInstance is THandler handler)
            {
                invoke(handler);
            }
        }
    }

    #region Dispatch Events

    /// <summary>
    /// Called after a card is played, so buffs that react to card play can respond.
    /// </summary>
    public void OnCardPlayed(BaseUnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem)
    {
        Dispatch<IOnCardPlayed>(handler => handler.OnCardPlayed(owner, cardInstance, combatSystem));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called before resetting a unit's block
    /// </summary>
    public void OnBeforeBlockReset(BaseUnitInstance owner, ResetBlockEvent blockEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnBeforeBlockReset>(handler => handler.OnBeforeBlockReset(owner, blockEvent, combatSystem));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called after resetting a unit's block
    /// </summary>
    public void OnAfterBlockReset(BaseUnitInstance owner, ResetBlockEvent blockEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnAfterBlockReset>(handler => handler.OnAfterBlockReset(owner, blockEvent, combatSystem));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called after One energy is spent
    /// </summary>
    public void OnEnergySpent(BaseUnitInstance owner, CombatSystem combatSystem)
    {
        Dispatch<IOnEnergySpent>(handler => handler.OnEnergySpent(owner, combatSystem ));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called after a card is drawn
    /// </summary>
    public void OnCardDrawn(BaseUnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem)
    {
        Dispatch<IOnCardDrawn>(handler => handler.OnCardDrawn(owner, cardInstance, combatSystem));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called after a card is discarded
    /// </summary>
    public void OnCardDiscarded(BaseUnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem)
    {
        Dispatch<IOnCardDiscarded>(handler => handler.OnCardDiscarded(owner, cardInstance, combatSystem));
        CleanUpExpiredBuffs();
    }

    public void OnCardExhausted(BaseUnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem)
    {
        Dispatch<IOnCardExhausted>(handler => handler.OnCardExhausted(owner, cardInstance, combatSystem));
        CleanUpExpiredBuffs();
    }

    /// <summary>
    /// Called at the start of the owner's turn.
    /// Buffs like regen, poison ticks, or start-of-turn resets may react here.
    /// </summary>
    public void OnTurnStart(BaseUnitInstance owner, CombatSystem combatSystem)
    {
        Dispatch<IOnTurnStart>(handler => handler.OnTurnStart(owner, combatSystem));
        CleanUpExpiredBuffs();
    }

    /// <summary>
    /// Called at the end of the turn of the owner's own side.
    /// Buffs that only last that turn, like Temp Strength, expire here.
    /// </summary>
    public void OnTurnEnd(BaseUnitInstance owner, CombatSystem combatSystem)
    {
        Dispatch<IOnTurnEnd>(handler => handler.OnTurnEnd(owner, combatSystem));
        CleanUpExpiredBuffs();
    }

    /// <summary>
    /// Called once per round after both sides have acted.
    /// Timed buffs like Weak/Vulnerable count down here.
    /// </summary>
    public void OnRoundEnd(BaseUnitInstance owner, CombatSystem combatSystem)
    {
        Dispatch<IOnRoundEnd>(handler => handler.OnRoundEnd(owner, combatSystem));
        CleanUpExpiredBuffs();
    }

    /// <summary>
    /// Called before this unit deals damage.
    /// Buffs like Strength or Weak can modify outgoing damage here.
    /// </summary>
    public void OnBeforeDealDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnBeforeDealDamage>(handler => handler.OnBeforeDealDamage(owner, damageEvent, combatSystem));
        CleanUpExpiredBuffs();
    }

    /// <summary>
    /// Called after this unit has dealt damage.
    /// Buffs like lifesteal, combo tracking, or on-hit triggers can react here.
    /// </summary>
    public void OnAfterDealDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnAfterDealDamage>(handler => handler.OnAfterDealDamage(owner, damageEvent, combatSystem));
        CleanUpExpiredBuffs();
    }

    /// <summary>
    /// Called before this unit takes damage.
    /// Buffs like Vulnerable, shield modifiers, or damage reduction can react here.
    /// </summary>
    public void OnBeforeTakeDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnBeforeTakeDamage>(handler => handler.OnBeforeTakeDamage(owner, damageEvent, combatSystem));
        CleanUpExpiredBuffs();
    }

    /// <summary>
    /// Called after this unit has taken damage.
    /// Buffs like Thorns retaliation, enrage, or reactive effects can respond here.
    /// </summary>
    public void OnAfterTakeDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnAfterTakeDamage>(handler => handler.OnAfterTakeDamage(owner, damageEvent, combatSystem));
        CleanUpExpiredBuffs();
    }

    /// <summary>
    /// Called when this unit gives block.
    /// </summary>
    public void OnApplyBlock(BaseUnitInstance owner, BlockEvent blockEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnApplyBlock>(handler => handler.OnApplyBlock(owner, blockEvent, combatSystem));
        CleanUpExpiredBuffs();
    }

    /// <summary>
    /// Called after this unit gains block.
    /// Buffs that react to successful block gain can respond here.
    /// </summary>
    public void OnReceiveBlock(BaseUnitInstance owner, BlockEvent blockEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnReceiveBlock>(handler => handler.OnReceiveBlock(owner, blockEvent, combatSystem));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called when this unit heals something.
    /// Buffs can modify the amount of heal or stats
    /// </summary>
    public void OnApplyHeal(BaseUnitInstance owner, HealEvent healEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnApplyHeal>(handler => handler.OnApplyHeal(owner, healEvent, combatSystem));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called when this unit is healed.
    /// </summary>
    public void OnReceiveHeal(BaseUnitInstance owner, HealEvent healEvent, CombatSystem combatSystem)
    {
        Dispatch<IOnReceiveHeal>(handler => handler.OnReceiveHeal(owner, healEvent, combatSystem));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called when this unit applies buff to someone.
    /// </summary>
    public void OnApplyBuff(BaseUnitInstance owner, BuffData buffData, int stacks, CombatSystem combatSystem)
    {
        Dispatch<IOnApplyBuff>(handler => handler.OnApplyBuff(owner, buffData, stacks, combatSystem));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called when this unit receives a buff.
    /// </summary>
    public void OnReceiveBuff(BaseUnitInstance owner, BuffData buffData, int stacks, CombatSystem combatSystem)
    {
        Dispatch<IOnReceiveBuff>(handler => handler.OnReceiveBuff(owner, buffData, stacks, combatSystem));
        CleanUpExpiredBuffs();
    }
    
    /// <summary>
    /// Called when this unit receives a buff.
    /// </summary>
    public void OnUnitDefeated(BaseUnitInstance owner, BaseUnitInstance defeatedUnit, CombatSystem combatSystem)
    {
        Dispatch<IOnUnitDefeated>(handler => handler.OnUnitDefeated(owner, defeatedUnit, combatSystem));
        CleanUpExpiredBuffs();
    }

    #endregion

    /// <summary>
    /// Finds an active buff instance by its BuffData asset.
    /// 
    /// Matching by BuffData means buffs created from the same asset stack together.
    /// Example:
    /// - one Weak asset
    /// - reapplying Weak adds stacks to the same Weak instance
    /// </summary>
    private BuffInstance FindBuff(BuffData data)
    {
        foreach (BuffInstance buff in activeBuffs)
        {
            if (buff.Data == data)
                return buff;
        }

        return null;
    }
    

    /// <summary>
    /// Removes all expired buffs from the active list.
    /// 
    /// A buff is considered expired when its runtime instance reports IsExpired == true.
    /// This usually happens when duration/stacks fall to zero or below.
    /// </summary>
    public void ClearAllBuffs()
    {
        activeBuffs.Clear();
        OnBuffsChanged?.Invoke();
    }

    private void CleanUpExpiredBuffs()
    {
        activeBuffs.RemoveAll(buffInstance => buffInstance.IsExpired);
        OnBuffsChanged?.Invoke();
    }
}