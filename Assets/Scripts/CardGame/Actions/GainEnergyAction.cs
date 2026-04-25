using System.Collections;
using System.Collections.Generic;

public class GainEnergyAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseUnitInstance _source;
    private readonly IReadOnlyList<BasePlayerUnitInstance> _targets;
    private readonly int _amount;

    public GainEnergyAction(CombatSystem combatSystem, BaseUnitInstance source, IReadOnlyList<BasePlayerUnitInstance> targets, int amount)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _amount = amount;
    }

    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveGainEnergy(_source, _targets, _amount);
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"GainEnergyAction: Source-{_source} | Targets-{string.Join(", ", _targets)} | Amount-{_amount}";
    }
}
