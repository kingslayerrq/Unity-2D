using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly UnitInstance _target;
    private readonly List<CardInstance> _cards;

    public DiscardAction(CombatSystem combatSystem, UnitInstance source, UnitInstance target, List<CardInstance> cards)
    {
        _combatSystem = combatSystem;
        _source = source;
        _target = target;
        _cards = cards;
    }

    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveDiscard(_source, _target,  _cards);
        
        yield return null;
    }
}
