using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Asks the player to pick N cards from hand to exhaust. The combat queue is
/// suspended until the choice is made; one ExhaustAction per chosen card is
/// then queued, so each card goes through the normal exhaust path.
/// </summary>
public class RequestExhaustSelectionAction : ICombatAction
{
    private readonly CombatManager _cm;
    private readonly BasePlayerUnitInstance _source;
    private readonly CardInstance _selfCard;
    private readonly ExhaustCardEffectConfig _cfg;

    public RequestExhaustSelectionAction(CombatManager cm,
        BasePlayerUnitInstance source, CardInstance selfCard, ExhaustCardEffectConfig cfg)
    {
        _cm = cm;
        _source = source;
        _selfCard = selfCard;
        _cfg = cfg;
    }

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
            CardChoiceKind.Exhaust,
            pool,
            minCount,
            maxCount,
            QueueExhausts));
    }

    private void QueueExhausts(IReadOnlyList<CardInstance> chosen)
    {
        if (chosen == null) return;

        for (int i = 0; i < chosen.Count; i++)
        {
            _cm.ActionQueue.Enqueue(
                new ExhaustAction(_cm.CombatSystem, _source, chosen[i]),
                null);
        }
    }

    public string GetActionDetails() => "RequestExhaustSelectionAction";
}
