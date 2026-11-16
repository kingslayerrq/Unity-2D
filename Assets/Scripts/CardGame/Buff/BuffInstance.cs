using UnityEngine;
using CardScene.Combat.Hooks;

public abstract class BuffInstance: IOnTurnStart, IOnTurnEnd, IOnRoundEnd
{
    public BuffData Data { get; private set; }
    public int Stacks { get; protected set; }
    
    // Dynamic Description
    public string BuffDescription => Data.GetDescription(Stacks);
    public bool IsExpired => Stacks <= 0;

    /// <summary>
    /// When set, the next round-end countdown is skipped once. Used for timed
    /// buffs applied during the enemy turn, so they are still active on the
    /// following enemy turn instead of expiring in the round they were applied.
    /// </summary>
    public bool SkipNextDurationTick { get; set; }
    
    protected BuffInstance(BuffData data, int stacks)
    {
        Data = data;
        Stacks = stacks;
    }

    public virtual void AddStacks(int amount)
    {
        if (Data.IsStackable)
        {
            Stacks += amount;
        }
    }
    
    public virtual void RemoveStacks(int amount)
    {
        Stacks -= amount;
        if (Stacks <= 0) 
        {
            Stacks = 0;
        }
    }

    public virtual void OnTurnStart(BaseUnitInstance owner, CombatSystem combatSystem)
    {
        Debug.Log($"{this.Data.BuffName} on turn start");
    }

    /**
     * Called at the end of the turn of the owner's own side.
     * Override for effects that only last that turn (e.g. Temp Strength).
     */
    public virtual void OnTurnEnd(BaseUnitInstance owner, CombatSystem combatSystem)
    {
    }

    /**
     * Called once per round after both sides have acted.
     * Timed buffs (Data.ExpiresAtEndOfTurn) lose one stack here.
     */
    public virtual void OnRoundEnd(BaseUnitInstance owner, CombatSystem combatSystem)
    {
        if (!Data.ExpiresAtEndOfTurn) return;

        if (SkipNextDurationTick)
        {
            SkipNextDurationTick = false;
            return;
        }

        Debug.Log(BuffDescription);
        RemoveStacks(1);
    }
}
