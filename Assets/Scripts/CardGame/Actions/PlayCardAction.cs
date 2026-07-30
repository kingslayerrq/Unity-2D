using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BasePlayerUnitInstance _source;
    private readonly List<BaseUnitInstance> _targets;
    private readonly CardInstance _card;
    private readonly bool _toDiscard;

    public PlayCardAction(CombatSystem combatSystem, BasePlayerUnitInstance source, List<BaseUnitInstance> targets, CardInstance card, bool toDiscard = true)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _card = card;
        _toDiscard = toDiscard;
    }

    public IEnumerator ExecuteAction()
    {
        _source.CombatManager.PlayerHandView.TryBeginAnimateOut(_card, out CardView view);

        _combatSystem.ResolvePlayCard(_source, _targets, _card);

        if (_toDiscard)
            _source.CombatManager.ActionQueue.Enqueue(new PlayToDiscardAction(_combatSystem, _source, _targets, _card), null);
        else
            _source.CombatManager.ActionQueue.Enqueue(new PlayToExhaustAction(_source, _card), null);

        yield return null;
    }

    public string GetActionDetails()
    {
        return $"PlayCardAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Card-{string.Join(", ", _card.CurrentName)}";
    }
}
