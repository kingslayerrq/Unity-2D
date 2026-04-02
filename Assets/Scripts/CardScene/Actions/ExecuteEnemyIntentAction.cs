using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteEnemyIntentAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseEnemyUnitInstance _source;
    private readonly List<BaseUnitInstance> _targets;
    private readonly List<CardInstance> _cards;

    public ExecuteEnemyIntentAction(CombatSystem combatSystem, BaseEnemyUnitInstance source,
        List<BaseUnitInstance> targets, List<CardInstance> cards)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _cards = cards;
    }
    
    public IEnumerator ExecuteAction()
    {
        // TODO:  
        yield return null;
    }

    public string GetActionDetails()
    {
        throw new System.NotImplementedException();
    }
}
