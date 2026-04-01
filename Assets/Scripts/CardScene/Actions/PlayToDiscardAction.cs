using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayToDiscardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BasePlayerUnitInstance _source;
    private readonly List<BaseUnitInstance> _targets;
    private readonly CardInstance _card;

    public PlayToDiscardAction(CombatSystem combatSystem, BasePlayerUnitInstance source, List<BaseUnitInstance> targets,
        CardInstance card)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _card = card;
    }
    public IEnumerator ExecuteAction()
    {
        yield return _source.CombatManager.PlayerHandView.PlayToDiscardAnimation(_card);
        
        _source.CombatManager.PlayerHandView.CompleteAnimateOut(_card);
        _source.CombatManager.PlayerHandView.RefreshHand(_source.Hand.Cards);
    }

    public string GetActionDetails()
    {
        return $"AnimateCardToDiscardAction: Card-{_card.CurrentName}";
    }
}
