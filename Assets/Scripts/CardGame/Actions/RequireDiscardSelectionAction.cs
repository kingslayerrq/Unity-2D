using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Asks the player to pick N cards from hand to discard. The combat queue is
/// suspended until the choice is made; a DiscardAction for the chosen cards is
/// then queued.
/// </summary>
public class RequestDiscardSelectionAction : ICombatAction
{
    private readonly CombatManager _cm;
    private readonly BasePlayerUnitInstance _source;
    private readonly CardInstance _selfCard;
    private readonly SelectDiscardCardEffectConfig _cfg;

    public RequestDiscardSelectionAction(CombatManager cm,
        BasePlayerUnitInstance source, CardInstance selfCard, SelectDiscardCardEffectConfig cfg)
    { _cm = cm; _source = source; _selfCard = selfCard; _cfg = cfg; }

    public void Execute()
    {
        var hand = _source.Hand.Cards;
        var pool = _cfg.ExcludeSelf
            ? hand.Where(c => c != _selfCard).ToList()
            : hand.ToList();

        if (pool.Count == 0) return;

        int maxCount = Mathf.Min(_cfg.Count, pool.Count);
        int minCount = _cfg.IsOptional ? 0 : maxCount;

        _cm.RequestCardChoice(new CardChoiceDecision(
            _source,
            CardChoiceKind.Discard,
            pool,
            minCount,
            maxCount,
            QueueDiscard));
    }

    private void QueueDiscard(IReadOnlyList<CardInstance> chosen)
    {
        if (chosen == null || chosen.Count == 0) return;

        _cm.ActionQueue.Enqueue(
            new DiscardAction(_cm.CombatSystem, _source,
                new List<BasePlayerUnitInstance>{_source}, chosen, CardMoveCause.CardEffect),
            null);
    }

    public string GetActionDetails() => "RequestDiscardSelectionAction";
}
