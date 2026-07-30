using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiscardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BasePlayerUnitInstance _source;
    private readonly List<BasePlayerUnitInstance> _targets;
    private readonly IReadOnlyList<CardInstance> _cards;
    private CardMoveCause _moveCause;

    public DiscardAction(CombatSystem combatSystem, BasePlayerUnitInstance source, 
        List<BasePlayerUnitInstance> targets, IReadOnlyList<CardInstance> cards,
        CardMoveCause moveCause)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _cards = cards?.ToList();
        _moveCause = moveCause;
    }

    public IEnumerator ExecuteAction()
    {
        if (_cards == null || _cards.Count == 0) yield break;

        foreach (var card in _cards)
        {
            _source.CombatManager.PlayerHandView.TryBeginAnimateOut(card, out var view);
        }
        
        _combatSystem.ResolveDiscard(_source, _targets, _cards, _moveCause);
        
        yield return _source.CombatManager.PlayerHandView.DiscardCardsAnimation(_cards);

        foreach (var card in _cards)
        {
            _source.CombatManager.PlayerHandView.CompleteAnimateOut(card);
        }
        _source.CombatManager.PlayerHandView.RefreshHand(_source.Hand.Cards);
    }

    public string GetActionDetails()
    {
        return $"DiscardAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Cards-{string.Join(", ", _cards)}";
    }

}
