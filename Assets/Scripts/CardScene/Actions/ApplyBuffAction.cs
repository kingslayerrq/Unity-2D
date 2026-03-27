using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyBuffAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly List<UnitInstance> _targets;
    private readonly BuffData _buffData;
    private readonly int _stacks;

    public ApplyBuffAction(CombatSystem combatSystem, UnitInstance source, List<UnitInstance> targets, BuffData buffData,
        int stacks)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _buffData = buffData;
        _stacks = stacks;
    }
    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveBuffs(_source, _targets, _buffData, _stacks);

        yield return null;
    }

    public string GetActionDetails()
    {
        return
            $"ApplyBuffAction: Source-{_source} | Targets-{string.Join(", ", _targets)} | Buff-{_buffData.BuffName} | " +
            $"Stacks-{_stacks}";
    }
}
