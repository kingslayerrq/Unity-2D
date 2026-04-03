using System.Collections.Generic;
using UnityEngine;

public class CardContext
{
    public BaseUnitInstance Source { get; private set; }
    public IReadOnlyList<BaseUnitInstance> Targets { get; private set; }
    public CardInstance CardInstance { get; private set; }
    public CombatManager CombatManager { get; private set; }

    public CardContext(BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, CardInstance cardInstance, CombatManager combatManager)
    {
        Source = source;
        Targets = targets;
        CardInstance = cardInstance;
        CombatManager = combatManager;
    }
}

