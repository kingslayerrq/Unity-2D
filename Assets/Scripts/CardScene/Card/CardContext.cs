using System.Collections.Generic;
using UnityEngine;

public class CardContext
{
    public UnitInstance Source { get; private set; }
    public List<UnitInstance> Target { get; private set; }
    public CardInstance CardInstance { get; private set; }
    public CombatSystem CombatSystem { get; private set; }

    public CardContext(UnitInstance source, List<UnitInstance> target, CardInstance cardInstance, CombatSystem combatSystem)
    {
        Source = source;
        Target = target;
        CardInstance = cardInstance;
        CombatSystem = combatSystem;
    }
}

