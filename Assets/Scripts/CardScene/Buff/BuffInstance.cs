using UnityEngine;
using CardScene.Combat.Hooks;

public abstract class BuffInstance: IOnTurnStart, IOnTurnEnd
{
    public BuffData Data { get; private set; }
    public int Stacks { get; protected set; }
    public bool IsExpired => Stacks <= 0;
    
    protected BuffInstance(BuffData data, int stacks)
    {
        Data = data;
        Stacks = stacks;
    }

    public virtual void AddStacks(int amount)
    {
        Stacks += amount;
    }
    
    public virtual void RemoveStacks(int amount)
    {
        Stacks -= amount;
        if (Stacks <= 0) 
        {
            Stacks = 0;
        }
    }

    public virtual void OnTurnStart(Unit owner, CombatSystem combatSystem)
    {
        throw new System.NotImplementedException();
    }

    /**
     * base OnTurnEnd removes decrease the stack by 1
     */
    public virtual void OnTurnEnd(Unit owner, CombatSystem combatSystem)
    {
        RemoveStacks(1);
    }
}
