using System.Collections.Generic;
using UnityEngine;

public class CardContext
{
    public BaseUnitInstance Source { get; private set; }
    public List<BaseUnitInstance> Target { get; private set; }
    public CardInstance CardInstance { get; private set; }
    public CombatManager CombatManager { get; private set; }

    public CardContext(BaseUnitInstance source, List<BaseUnitInstance> target, CardInstance cardInstance, CombatManager combatManager)
    {
        Source = source;
        Target = target;
        CardInstance = cardInstance;
        CombatManager = combatManager;
    }
}

