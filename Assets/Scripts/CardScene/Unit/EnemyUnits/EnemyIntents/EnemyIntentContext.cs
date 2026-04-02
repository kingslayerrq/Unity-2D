using System.Collections.Generic;
using UnityEngine;

public class EnemyIntentContext
{
    public BaseEnemyUnitInstance Source { get; private set; }
    public List<BaseUnitInstance> Targets { get; private set; }
    public List<CardInstance> Cards { get; private set; }
    public BaseEnemyIntent Intent { get; private set; }
    public CombatManager CombatManager { get; private set; }

    public EnemyIntentContext(BaseEnemyUnitInstance source, List<BaseUnitInstance> targets, List<CardInstance> cards,
        BaseEnemyIntent intent, CombatManager combatManager)
    {
        Source = source;
        Targets = targets;
        Cards = cards;
        Intent = intent;
        CombatManager = combatManager;
    }
}
