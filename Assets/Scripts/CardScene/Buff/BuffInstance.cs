using UnityEngine;
using CardScene.Combat.Hooks;

public abstract class BuffInstance: IOnTurnStart, IOnTurnEnd
{
    public BuffData Data { get; private set; }
    public int Stacks { get; protected set; }
    
    // Dynamic Description
    public string BuffDescription => Data.GetDescription(Stacks);
    public bool IsExpired => Stacks <= 0;
    
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

    public virtual void OnTurnStart(UnitInstance owner, CombatSystem combatSystem)
    {
        throw new System.NotImplementedException();
    }

    /**
     * base OnTurnEnd removes decrease the stack by 1
     */
    public virtual void OnTurnEnd(UnitInstance owner, CombatSystem combatSystem)
    {
        if (Data.ExpiresAtEndOfTurn)
        {
            RemoveStacks(1);
        }
    }
}
