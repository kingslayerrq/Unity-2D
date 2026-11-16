using System.Collections.Generic;
using UnityEngine;

public class ExecuteEnemyIntentAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseEnemyUnitInstance _source;
    private readonly List<CardInstance> _cards;

    public ExecuteEnemyIntentAction(CombatSystem combatSystem, BaseEnemyUnitInstance source, List<CardInstance> cards)
    {
        _combatSystem = combatSystem;
        _source = source;
        _cards = cards;
    }
    
    public void Execute()
    {
        // TODO:  
        _combatSystem.ResolveEnemyIntent(_source, _cards);
    }

    public string GetActionDetails()
    {
        return $"Enemy-{_source?.CurrentName} is executing intent-{_source?.CurrentIntent?.EnemyIntent}";
    }
}
