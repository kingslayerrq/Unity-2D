using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BasePlayerUnitInstance _source;
    private readonly List<BaseUnitInstance> _targets;
    private readonly CardInstance _card;
    

    public PlayCardAction(CombatSystem combatSystem, BasePlayerUnitInstance source, List<BaseUnitInstance> targets, CardInstance card)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _card = card;
    }
    public IEnumerator ExecuteAction()
    {
        // lock, pull the card out of hand
        _source.CombatManager.PlayerHandView.TryBeginAnimateOut(_card, out CardView view);
        
        _combatSystem.ResolvePlayCard(_source, _targets, _card);
        
        _source.CombatManager.ActionQueue.Enqueue(new PlayToDiscardAction(_combatSystem, _source, _targets, _card), null);
        
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"PlayCardAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Card-{string.Join(", ", _card.CurrentName)}";
    }
}
