using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly List<UnitInstance> _targets;
    private readonly List<CardInstance> _cards;

    public DiscardAction(CombatSystem combatSystem, UnitInstance source, List<UnitInstance> targets, List<CardInstance> cards)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _cards = cards;
    }

    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveDiscard(_source, _targets,  _cards);
        
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"DiscardAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Cards-{string.Join(", ", _cards)}";
    }

}
