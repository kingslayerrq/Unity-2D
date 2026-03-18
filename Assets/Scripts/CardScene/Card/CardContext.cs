using UnityEngine;

public class CardContext
{
    public Unit Source { get; private set; }
    public Unit Target { get; private set; }
    public CardInstance CardInstance { get; private set; }
    public CombatSystem CombatSystem { get; private set; }

    public CardContext(Unit source, Unit target, CardInstance cardInstance, CombatSystem combatSystem)
    {
        Source = source;
        Target = target;
        CardInstance = cardInstance;
        CombatSystem = combatSystem;
    }
}

