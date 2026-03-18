using UnityEngine;

public abstract class CardEffectData : ScriptableObject
{
    /**
     * base execute check for nulls and return
     * override to add behavior
     */
    public virtual void Execute(CardContext context)
    {
        if (context == null || context.CombatSystem == null) return;
        if (context.Source == null || context.Target == null) return;
    }
}
