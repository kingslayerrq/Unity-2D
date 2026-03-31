using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBlockAction: ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseUnitInstance _source;
    private readonly List<BaseUnitInstance> _targets;
    private readonly BlockResetSource _resetSource;

    public ResetBlockAction(CombatSystem combatSystem, BaseUnitInstance source, List<BaseUnitInstance> targets, BlockResetSource resetSource)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _resetSource = resetSource;
    }
    
    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveBlockReset(_source, _targets, _resetSource);
        
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"ResetBlockAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | ResetSource-{_resetSource}";
    }
}
