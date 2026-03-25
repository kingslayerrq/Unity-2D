using System.Collections;
using UnityEngine;

public class SpendEnergyAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly UnitInstance _target;
    private readonly int _amount;

    public SpendEnergyAction(CombatSystem combatSystem, UnitInstance source, UnitInstance target, int amount)
    {
        _combatSystem = combatSystem;
        _source = source;
        _target = target;
        _amount = amount;
    }

    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveSpendEnergy(_source, _target, _amount);
        
        yield return null;
    }
}
