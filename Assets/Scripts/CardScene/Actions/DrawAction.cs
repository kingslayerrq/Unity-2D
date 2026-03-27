using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly List<UnitInstance> _targets;
    private readonly int _amount;

    public DrawAction(CombatSystem combatSystem, UnitInstance source, List<UnitInstance> targets, int amount)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _amount = amount;
    }

    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveDrawCards(_source, _targets, _amount);
        
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"DrawAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Amount-{_amount}";
    }
}
