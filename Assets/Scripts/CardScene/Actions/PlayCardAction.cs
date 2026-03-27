using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly List<UnitInstance> _targets;
    private readonly CardInstance _card;
    

    public PlayCardAction(CombatSystem combatSystem, UnitInstance source, List<UnitInstance> targets, CardInstance card)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _card = card;
    }
    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolvePlayCard(_source, _targets, _card);
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"PlayCardAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Card-{string.Join(", ", _card)}";
    }
}
