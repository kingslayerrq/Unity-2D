using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly List<UnitInstance> _allUnits;

    public EndTurnAction(CombatSystem combatSystem, UnitInstance source, List<UnitInstance> allUnits)
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
        return $"{_source.name} is ending turn.";
    }
}
