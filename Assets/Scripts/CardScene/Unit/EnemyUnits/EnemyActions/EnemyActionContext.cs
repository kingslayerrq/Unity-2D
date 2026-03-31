using System.Collections.Generic;
using UnityEngine;

public class EnemyActionContext
{
    public BaseEnemyUnitInstance Source { get; private set; }
    public List<BaseUnitInstance> Targets { get; private set; }
    public List<CardInstance> Cards { get; private set; }
    public BaseEnemyAction Action { get; private set; }
    public CombatSystem CombatSystem { get; private set; }

    public EnemyActionContext(BaseEnemyUnitInstance source, List<BaseUnitInstance> targets, List<CardInstance> cards,
        BaseEnemyAction action, CombatSystem combatSystem)
    {
        Source = source;
        Targets = targets;
        Cards = cards;
        Action = action;
        CombatSystem = combatSystem;
    }
}
