using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Queued action that pauses the combat queue (via WaitUntil) while the player
/// picks N cards from hand to exhaust. On confirm it enqueues one ExhaustAction
/// per chosen card so each card flows through the existing exhaust animation
/// pipeline (TryBeginAnimateOut → DiscardCardsAnimation → CompleteAnimateOut).
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

    public IEnumerator ExecuteAction()
    {
        var hand = _source.Hand.Cards;
        var pool = _cfg.ExcludeSelf
            ? hand.Where(c => c != _selfCard).ToList()
            : hand.ToList();

        if (pool.Count == 0) yield break;

        if (CardSelectionCanvas.Instance == null)
        {
            Debug.LogError("RequestExhaustSelectionAction: CardSelectionCanvas.Instance is null. " +
                           "Make sure the CardSelectionCanvas component is present on the combat scene's canvas.");
            yield break;
        }

        bool done = false;
        IReadOnlyList<CardInstance> chosen = null;

        CardSelectionCanvas.Instance.Prompt(new CardSelectionRequest
        {
            SourceCards  = pool,
            MinCount     = _cfg.IsOptional ? 0 : Mathf.Min(_cfg.Count, pool.Count),
            MaxCount     = Mathf.Min(_cfg.Count, pool.Count),
            PromptText   = $"Exhaust {_cfg.Count} card(s)",
            ConfirmLabel = "Exhaust",
            CanCancel    = _cfg.IsOptional,
            OnConfirm    = picks => { chosen = picks; done = true; },
            OnCancel     = ()    => { chosen = Array.Empty<CardInstance>(); done = true; },
        });

        yield return new WaitUntil(() => done);

        if (chosen == null || chosen.Count == 0) yield break;

        for (int i = 0; i < chosen.Count; i++)
        {
            _cm.ActionQueue.Enqueue(
                new ExhaustAction(_cm.CombatSystem, _source, chosen[i]),
                null);
        }
    }

    public string GetActionDetails() => "RequestExhaustSelectionAction";
}
