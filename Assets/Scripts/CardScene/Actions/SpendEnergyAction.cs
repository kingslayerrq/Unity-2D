using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpendEnergyAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseUnitInstance _source;
    private readonly List<BasePlayerUnitInstance> _targets;
    private readonly int _amount;

    public SpendEnergyAction(CombatSystem combatSystem, BaseUnitInstance source, List<BasePlayerUnitInstance> targets, int amount)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _amount = amount;
    }

    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveSpendEnergy(_source, _targets, _amount);
        
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"SpendEnergyAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Amount-{_amount}";
    }
}
