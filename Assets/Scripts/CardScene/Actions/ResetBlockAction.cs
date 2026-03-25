using System.Collections;
using UnityEngine;

public class ResetBlockAction: ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly UnitInstance _target;
    private readonly BlockResetSource _resetSource;

    public ResetBlockAction(CombatSystem combatSystem, UnitInstance source, UnitInstance target, BlockResetSource resetSource)
    {
        _combatSystem = combatSystem;
        _source = source;
        _target = target;
        _resetSource = resetSource;
    }
    
    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveBlockReset(_source, _target, _resetSource);
        
        yield return null;
    }
}
