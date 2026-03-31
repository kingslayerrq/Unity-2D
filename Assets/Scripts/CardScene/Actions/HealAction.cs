using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseUnitInstance _source;
    private readonly List<BaseUnitInstance> _targets;
    private readonly int _amount;

    public HealAction(CombatSystem combatSystem, BaseUnitInstance source, List<BaseUnitInstance> targets, int amount)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _amount = amount;
    }

    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveHeal(_source, _targets, _amount);
        
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"HealAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Amount-{_amount}";
    }
}
