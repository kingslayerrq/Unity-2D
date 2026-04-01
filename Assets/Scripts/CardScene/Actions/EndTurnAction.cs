using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseUnitInstance _source;
    private readonly List<BaseUnitInstance> _allUnits;

    public EndTurnAction(CombatSystem combatSystem, BaseUnitInstance source, List<BaseUnitInstance> allUnits)
    {
        _combatSystem = combatSystem;
        _source = source;
        _allUnits = allUnits;
    }
    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveEndTurn(_source, _allUnits);
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"{_source.CurrentName} is ending turn.";
    }
}
