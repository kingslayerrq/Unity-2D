using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RequestDiscardSelectionAction : ICombatAction
{
    private readonly CombatManager _cm;
    private readonly BasePlayerUnitInstance _source;
    private readonly CardInstance _selfCard;
    private readonly SelectDiscardCardEffectConfig _cfg;

    public RequestDiscardSelectionAction(CombatManager cm,
        BasePlayerUnitInstance source, CardInstance selfCard, SelectDiscardCardEffectConfig cfg)
    { _cm = cm; _source = source; _selfCard = selfCard; _cfg = cfg; }

    public IEnumerator ExecuteAction()
    {
        var hand = _source.Hand.Cards;
        var pool = _cfg.ExcludeSelf
            ? hand.Where(c => c != _selfCard).ToList()
            : hand.ToList();

        if (pool.Count == 0) yield break;

        if (CardSelectionCanvas.Instance == null)
        {
            Debug.LogError("RequestDiscardSelectionAction: CardSelectionCanvas.Instance is null. " +
                           "Make sure the CardSelectionCanvas component is present on the combat scene's canvas.");
            yield break;
        }

        bool done = false;
        IReadOnlyList<CardInstance> chosen = null;

        // CardSelectionCanvas is a singleton-style scene component on the combat canvas.
        // Show with: source pile cards, min/max count, confirm callback.
        CardSelectionCanvas.Instance.Prompt(new CardSelectionRequest
        {
            SourceCards     = pool,
            MinCount        = _cfg.IsOptional ? 0 : Mathf.Min(_cfg.Count, pool.Count),
            MaxCount        = Mathf.Min(_cfg.Count, pool.Count),
            PromptText      = $"Discard {_cfg.Count} card(s)",
            ConfirmLabel    = "Discard",
            CanCancel       = _cfg.IsOptional,
            OnConfirm       = picks => { chosen = picks; done = true; },
            OnCancel        = ()    => { chosen = Array.Empty<CardInstance>(); done = true; },
        });

        yield return new WaitUntil(() => done);

        if (chosen != null && chosen.Count > 0)
        {
            _cm.ActionQueue.Enqueue(
                new DiscardAction(_cm.CombatSystem, _source,
                    new List<BasePlayerUnitInstance>{_source}, chosen, CardMoveCause.CardEffect),
                null);
        }
    }

    public string GetActionDetails() => "RequestDiscardSelectionAction";
}
