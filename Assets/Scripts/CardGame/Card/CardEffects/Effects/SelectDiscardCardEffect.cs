using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SelectDiscardCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(SelectDiscardCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context?.CombatManager == null || context.CardInstance == null) return;
        if (context.Source is not BasePlayerUnitInstance source) return;
        if (effectData.CardEffectConfig is not SelectDiscardCardEffectConfig cfg)
        {
            Debug.LogError($"SelectDiscardCardEffect received wrong config type" +
                           $" on card {context.CardInstance?.Data?.CardName}");
            return;
        }

        var hand = source.Hand?.Cards;
        if (hand == null || hand.Count == 0) return;

        switch (cfg.DiscardMode)
        {
            case DiscardMode.RandomHandCard:
            {
                var picks = PickRandom(hand, cfg, context.CardInstance, context.CombatManager.Rng);
                if (picks.Count == 0) return;
                context.CombatManager.ActionQueue.Enqueue(
                    new DiscardAction(context.CombatManager.CombatSystem,
                        source, new List<BasePlayerUnitInstance> { source }, picks, CardMoveCause.CardEffect),
                    null);
                break;
            }
            case DiscardMode.EntireHand:
            {
                var picks = new List<CardInstance>();
                for (int i = 0; i < hand.Count; i++)
                {
                    var c = hand[i];
                    if (cfg.ExcludeSelf && c == context.CardInstance) continue;
                    picks.Add(c);
                }
                if (picks.Count == 0) return;
                context.CombatManager.ActionQueue.Enqueue(
                    new DiscardAction(context.CombatManager.CombatSystem,
                        source, new List<BasePlayerUnitInstance> { source }, picks, CardMoveCause.CardEffect),
                    null);
                break;
            }
            case DiscardMode.SelectHandCard:
            {
                // Ask in a queued action, so the choice comes up in order. The action
                // suspends the queue on a CardChoiceDecision and queues the actual
                // DiscardAction once the choice has been answered.
                context.CombatManager.ActionQueue.Enqueue(
                    new RequestDiscardSelectionAction(context.CombatManager, source,
                        context.CardInstance, cfg),
                    null);
                break;
            }
        }
    }

    private static List<CardInstance> PickRandom(IReadOnlyList<CardInstance> hand,
        SelectDiscardCardEffectConfig cfg, CardInstance self, System.Random rng)
    {
        var pool = new List<CardInstance>();
        for (int i = 0; i < hand.Count; i++)
        {
            var c = hand[i];
            if (cfg.ExcludeSelf && c == self) continue;
            pool.Add(c);
        }

        int picksCount = Mathf.Min(cfg.Count, pool.Count);
        var result = new List<CardInstance>(picksCount);
        for (int i = 0; i < picksCount; i++)
        {
            int idx = rng.Next(pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return result;
    }
}
