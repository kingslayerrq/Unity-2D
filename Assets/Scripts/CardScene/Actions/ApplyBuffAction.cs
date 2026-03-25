using System.Collections;
using UnityEngine;

public class ApplyBuffAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly UnitInstance _target;
    private readonly BuffData _buffData;
    private readonly int _stacks;

    public ApplyBuffAction(CombatSystem combatSystem, UnitInstance source, UnitInstance target, BuffData buffData,
        int stacks)
    {
        _combatSystem = combatSystem;
        _source = source;
        _target = target;
        _buffData = buffData;
        _stacks = stacks;
    }
    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveBuffs(_source, _target, _buffData, _stacks);

        yield return null;
    }
    
}
