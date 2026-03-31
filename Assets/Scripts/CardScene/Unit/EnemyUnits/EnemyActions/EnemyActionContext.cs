using System.Collections.Generic;
using UnityEngine;

public class EnemyActionContext
{
    public BaseEnemyUnitInstance Source { get; private set; }
    public List<BaseUnitInstance> Targets { get; private set; }
    public List<CardInstance> Cards { get; private set; }
    public BaseEnemyAction Action { get; private set; }
    public CombatManager CombatManager { get; private set; }

    public EnemyActionContext(BaseEnemyUnitInstance source, List<BaseUnitInstance> targets, List<CardInstance> cards,
        BaseEnemyAction action, CombatManager combatManager)
    {
        Source = source;
        Targets = targets;
        Cards = cards;
        Action = action;
        CombatManager = combatManager;
    }
}
