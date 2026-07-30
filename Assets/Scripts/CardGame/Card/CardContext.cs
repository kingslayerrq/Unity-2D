using System.Collections.Generic;
using UnityEngine;

public class CardContext
{
    public BaseUnitInstance Source { get; private set; }
    public IReadOnlyList<BaseUnitInstance> Targets { get; private set; }
    public CardEffectTrigger Trigger { get; }
    public CardMoveCause MoveCause { get; }
    public CardInstance CardInstance { get; private set; }
    public CombatManager CombatManager { get; private set; }

    public CardContext(BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> targets,
        CardEffectTrigger trigger, CardMoveCause moveCause,
        CardInstance cardInstance, CombatManager combatManager)
    {
        Source = source;
        Targets = targets;
        Trigger = trigger;
        MoveCause = moveCause;
        CardInstance = cardInstance;
        CombatManager = combatManager;
    }
}

