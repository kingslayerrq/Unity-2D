using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiscardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly List<UnitInstance> _targets;
    private readonly IReadOnlyList<CardInstance> _cards;

    public DiscardAction(CombatSystem combatSystem, UnitInstance source, List<UnitInstance> targets, IReadOnlyList<CardInstance> cards)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _cards = cards?.ToList();
    }

    public IEnumerator ExecuteAction()
    {
        if (_cards == null || _cards.Count == 0) yield break;

        foreach (var card in _cards)
        {
            _source.UnitUIManager.PlayerHandView.TryBeginAnimateOut(card, out var view);
        }
        
        _combatSystem.ResolveDiscard(_source, _targets,  _cards);
        
        yield return _source.UnitUIManager.PlayerHandView.DiscardCardsAnimation(_cards);

        foreach (var card in _cards)
        {
            _source.UnitUIManager.PlayerHandView.CompleteAnimateOut(card);
        }
        _source.UnitUIManager.PlayerHandView.RefreshHand(_source.Hand.Cards);
    }

    public string GetActionDetails()
    {
        return $"DiscardAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Cards-{string.Join(", ", _cards)}";
    }

}
