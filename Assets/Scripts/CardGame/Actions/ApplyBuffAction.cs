using System.Collections.Generic;
using UnityEngine;

public class ApplyBuffAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseUnitInstance _source;
    private readonly IReadOnlyList<BaseUnitInstance> _targets;
    private readonly BuffData _buffData;
    private readonly int _stacks;

    public ApplyBuffAction(CombatSystem combatSystem, BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, BuffData buffData,
        int stacks)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _buffData = buffData;
        _stacks = stacks;
    }
    public void Execute()
    {
        _combatSystem.ResolveBuffs(_source, _targets, _buffData, _stacks);
    }

    public string GetActionDetails()
    {
        return
            $"ApplyBuffAction: Source-{_source} | Targets-{string.Join(", ", _targets)} | Buff-{_buffData.BuffName} | " +
            $"Stacks-{_stacks}";
    }
}
