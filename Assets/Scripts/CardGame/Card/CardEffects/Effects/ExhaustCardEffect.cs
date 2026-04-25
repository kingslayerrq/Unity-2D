using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExhaustCardEffect : BaseCardEffect
{
    public override Type RequiredConfigType => typeof(ExhaustCardEffectConfig);

    public override void Execute(CardContext context, CardEffectData effectData)
    {
        if (context == null || context.CombatManager == null || context.CardInstance == null) return;
        if (context.Source == null) return;

        if (context.Source is not BasePlayerUnitInstance sourcePlayer) return;

        // No config or ThisCard → routing is handled by PlayToExhaustAction via CombatManager.PlayCard
        if (effectData.CardEffectConfig is not ExhaustCardEffectConfig exhaustConfig) return;
        if (exhaustConfig.ExhaustMode == ExhaustMode.ThisCard) return;

        CardInstance cardToExhaust = ResolveTarget(exhaustConfig.ExhaustMode, context, sourcePlayer);
        if (cardToExhaust == null) return;

        context.CombatManager.ActionQueue.Enqueue(
            new ExhaustAction(context.CombatManager.CombatSystem, sourcePlayer, cardToExhaust),
            null);
    }

    private CardInstance ResolveTarget(ExhaustMode mode, CardContext context, BasePlayerUnitInstance source)
    {
        switch (mode)
        {
            case ExhaustMode.ThisCard:
                return context.CardInstance;

            case ExhaustMode.RandomHandCard:
                var hand = source.Hand?.Cards;
                if (hand == null || hand.Count == 0) return null;
                var candidates = new List<CardInstance>();
                foreach (var c in hand)
                {
                    if (c != context.CardInstance) candidates.Add(c);
                }
                if (candidates.Count == 0) return null;
                return candidates[RunManager.Instance.CombatRng.Next(candidates.Count)];

            case ExhaustMode.SelectHandCard:
                Debug.LogWarning("ExhaustCardEffect: SelectHandCard requires a selection UI — not yet implemented.");
                return null;

            default:
                return null;
        }
    }
}
