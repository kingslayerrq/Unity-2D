using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    
    public void ResolveEnemyIntent(BaseEnemyUnitInstance source, List<CardInstance> cards)
    {
        if (source == null || source.IsDead || source.CurrentIntent == null) return;
        EnemyIntentData intent = source.CurrentIntent;
        EnemyIntentContext context =
            new EnemyIntentContext(source, source.CurrentIntentTargets, cards, intent.EnemyIntent, source.CombatManager);
        
        intent.EnemyIntent.ExecuteIntent(context, intent);
    }

    public void ResolveShuffleCardsToPile(BaseUnitInstance source, IReadOnlyList<BasePlayerUnitInstance> targets, [CanBeNull] CardInstance sourceCardInstance,
        IReadOnlyList<CardInstance> cardInstances, CardPileType targetCardPileTypes)
    {
        if (source == null || targets == null || cardInstances == null) return;

        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            if (target.IsDead) continue;
            if (target.CardPileManager == null)
            {
                Debug.Log($"{target.CurrentName} has no CardPileManager!");
                continue;
            }

            for (var index = 0; index < cardInstances.Count; index++)
            {
                var ci = cardInstances[index];
                foreach (CardPileType pileType in Enum.GetValues(typeof(CardPileType)))
                {
                    if (targetCardPileTypes.HasFlag(pileType))
                    {
                        var cardPile = target.CardPileManager.GetCardPile(pileType);
                        if (cardPile == null)
                        {
                            Debug.Log($"{target.CurrentName} has no CardPile of type-{pileType}!");
                            continue;
                        }

                        cardPile.AddCardToPile(ci);
                        ci.SetOwner(target);
                    }
                }
            }
        }
    }
    
    private void ResolveCardEffects(CardContext context)
    {
        IReadOnlyList<CardEffectData> rules =
            context.CardInstance.CurrentCardEffects;

        if (rules == null)
            return;

        foreach (CardEffectData rule in rules)
        {
            if (rule == null || rule.Trigger != context.Trigger)
                continue;

            if (!ConditionsPass(rule, context))
                continue;

            if (rule.CardEffect == null)
            {
                Debug.LogWarning("Card effect rule has no effect.");
                continue;
            }

            rule.CardEffect.Execute(context, rule);
        }
    }

    private bool ConditionsPass(CardEffectData rule, CardContext context)
    {
        if (rule.Conditions == null || rule.Conditions.Count == 0) return true;
        foreach (var condition in rule.Conditions)
        {
            if (condition == null)
            {
                Debug.LogError($"Condition is null!", this);
                return false;
            }
            if (!condition.IsMet(context)) return false;
        }

        return true;
    }
    
    
    public void ResolvePlayCard(BasePlayerUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, CardInstance cardInstance)
    {
        if (source == null || cardInstance == null) return;
        if (cardInstance.RequireTarget && targets == null) return;
        
        CardContext context = new CardContext(source, targets, CardEffectTrigger.OnPlay, CardMoveCause.None,
            cardInstance, source.CombatManager);

        ResolveCardEffects(context);

        // Trigger on card played
        source.BuffManager.OnCardPlayed(source, cardInstance, this);
        
        // Record progression only after the card resolves successfully.
        if (GlobalProgressionManager.HasInstance)
        {
            GlobalProgressionManager.Instance.RecordCardPlayed(
                cardInstance);
        }
        
        if (source.Hand.RemoveCard(cardInstance))
        {
            if (cardInstance.IsPower)
            {
                // A Power stays in play until the combat ends. It joins no pile
                // and does not count as exhausted, so exhaust triggers stay quiet.
                cardInstance.CurrentCardState = CardState.InPlay;
            }
            // exhaust
            else if (cardInstance.ShouldExhaustAfterPlay())
            {
                source.CardPileManager.ExhaustPile.AddCardToPile(cardInstance);

                CardContext exhaustContext = new CardContext(
                    source,
                    targets ?? Array.Empty<BaseUnitInstance>(),
                    CardEffectTrigger.OnExhaust,
                    CardMoveCause.PlayCleanup,
                    cardInstance,
                    source.CombatManager);

                ResolveCardEffects(exhaustContext);
                source.BuffManager.OnCardExhausted(
                    source,
                    cardInstance,
                    this);
            }
            else
            {
                source.CardPileManager.DiscardPile.AddCardToPile(cardInstance);
            }
        }
        else
        {
            Debug.LogWarning($"Card {cardInstance.Data.CardName} failed to move to discard pile!");
        }
    }

    /// <summary>
    /// Source makes targets spend their energy
    /// </summary>
    /// <param name="source"></param>
    /// <param name="targets"></param>
    /// <param name="amount"></param>
    public void ResolveSpendEnergy(BaseUnitInstance source, IReadOnlyList<BasePlayerUnitInstance> targets, int amount)
    {
        if (source == null || targets == null) return;

        for (var index = 0; index < targets.Count; index++)
        {
            var t = targets[index];
            if (t.CurrentEnergy < amount)
            {
                Debug.LogWarning($"Dont have enough energy! Current-{t.CurrentEnergy}: " +
                                 $"Required-{amount}");
                continue;
            }

            t.SpendEnergy(amount);

            for (int i = 0; i < amount; i++)
            {
                t.BuffManager.OnEnergySpent(t, this);
            }
        }
    }

    public void ResolveGainEnergy(BaseUnitInstance source, IReadOnlyList<BasePlayerUnitInstance> targets, int amount)
    {
        if (source == null || targets == null) return;

        for (var index = 0; index < targets.Count; index++)
        {
            var t = targets[index];
            if (t.IsDead) continue;
            t.GainEnergy(amount);
        }
    }

    public void ResolveBlockReset(BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, BlockResetSource resetSource)
    {
        if (source == null || targets == null) return;

        for (var index = 0; index < targets.Count; index++)
        {
            var t = targets[index];
            if (t.IsDead) continue;

            ResetBlockEvent resetEvent = new ResetBlockEvent(source, t, resetSource);

            t.BuffManager.OnBeforeBlockReset(t, resetEvent, this);

            t.ResetBlock(resetEvent);

            t.BuffManager.OnAfterBlockReset(t, resetEvent, this);
        }
    }
    
    public void ResolveDrawCards(BaseUnitInstance source, IReadOnlyList<BasePlayerUnitInstance> targets, int amount)
    {
        if (source == null || targets == null) return;

        for (var index = 0; index < targets.Count; index++)
        {
            var t = targets[index];
            for (int i = 0; i < amount; i++)
            {
                if (t.IsDead) continue;
                if (!t.TryDrawToHand(out CardInstance cardInstance)) break;

                // Trigger on card drawn
                t.BuffManager.OnCardDrawn(t, cardInstance, this);
            }
        }
    }

    public void ResolveDiscard(BaseUnitInstance source, IReadOnlyList<BasePlayerUnitInstance> targets,
        IReadOnlyList<CardInstance> cards, CardMoveCause moveCause)
    {
        if (source == null || targets == null) return;
        
        var cardsToDiscard = cards.ToList();
        for (var index = 0; index < targets.Count; index++)
        {
            var t = targets[index];
            if (t.IsDead) continue;
            for (var i = 0; i < cardsToDiscard.Count; i++)
            {
                var card = cardsToDiscard[i];
                bool moved = t.DiscardCard(card);
                if (!moved) continue;
                Debug.Log("Discard Card: " + card.CurrentName);
                // Trigger on card discarded on CardEffect move
                if (moveCause == CardMoveCause.CardEffect)
                {
                    CardContext context = new CardContext(
                        t,
                        Array.Empty<BaseUnitInstance>(),
                        CardEffectTrigger.OnDiscard,
                        moveCause,
                        card,
                        t.CombatManager);

                    ResolveCardEffects(context);
                    t.BuffManager.OnCardDiscarded(t, card, this);
                }
            }
        }
    }

    public void ResolveExhaust(BasePlayerUnitInstance source, CardMoveCause moveCause, CardInstance card)
    {
        if (source == null || card == null)
            return;

        if (!source.ExhaustCard(card))
            return;

        CardContext context = new CardContext(source, Array.Empty<BaseUnitInstance>(),
            CardEffectTrigger.OnExhaust, moveCause, card,
            source.CombatManager);

        ResolveCardEffects(context);
        source.BuffManager.OnCardExhausted(source, card, this);
    }

    public void ResolveDamage(BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, int amount, CardType cardType)
    {
        if (source == null || targets == null) return;

        for (var i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (t.IsDead) continue;

            DamageEvent damageEvent = new DamageEvent(source, t, amount, cardType);

            source.BuffManager.OnBeforeDealDamage(source, damageEvent, this);
            t.BuffManager.OnBeforeTakeDamage(t, damageEvent, this);

            t.TakeDamage(damageEvent.Amount, damageEvent.Source);

            source.BuffManager.OnAfterDealDamage(source, damageEvent, this);
            t.BuffManager.OnAfterTakeDamage(t, damageEvent, this);

            // Death is checked here, for the units actually hit, so every
            // damage source (cards, intents, secondary targets) is covered.
            t.OnDmgResolved();
        }
    }

    public void ResolveHeal(BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, int amount)
    {
        if (source == null || targets == null) return;

        for (var index = 0; index < targets.Count; index++)
        {
            var t = targets[index];
            if (t.IsDead) continue;

            HealEvent healEvent = new HealEvent(source, t, amount);

            source.BuffManager.OnApplyHeal(source, healEvent, this);

            t.Heal(healEvent.Amount);

            t.BuffManager.OnReceiveHeal(t, healEvent, this);
        }
    }
    
    public void ResolveBlocks(BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, int amount, bool isFromCard)
    {
        if (source == null || targets == null) return;

        for (var index = 0; index < targets.Count; index++)
        {
            var t = targets[index];
            if (t.IsDead) continue;

            BlockEvent blockEvent = new BlockEvent(source, t, amount, isFromCard);

            source.BuffManager.OnApplyBlock(source, blockEvent, this);

            t.Block(blockEvent.Amount);

            t.BuffManager.OnReceiveBlock(t, blockEvent, this);
        }
    }
    
    public void ResolveBuffs(BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, BuffData buffData, int stacks)
    {
        if (source == null || targets == null) return;

        for (var index = 0; index < targets.Count; index++)
        {
            var t = targets[index];
            if (t.IsDead) continue;

            source.BuffManager.OnApplyBuff(source, buffData, stacks, this);

            t.ReceiveBuff(buffData, stacks);

            t.BuffManager.OnReceiveBuff(t, buffData, stacks, this);
        }
    }
    

    public void ResolveEndTurn(BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> allUnits)
    {
        if (source == null || allUnits == null || allUnits.Count == 0) return;

        for (var index = 0; index < allUnits.Count; index++)
        {
            var u = allUnits[index];
            if (u.IsDead) continue;

            u.BuffManager?.OnTurnEnd(u, this);
        }
    }

    /// <summary>
    /// Runs the once-per-round countdown of timed buffs on every living unit.
    /// </summary>
    public void ResolveEndRound(IReadOnlyList<BaseUnitInstance> allUnits)
    {
        if (allUnits == null) return;

        for (var index = 0; index < allUnits.Count; index++)
        {
            var u = allUnits[index];
            if (u == null || u.IsDead) continue;

            u.BuffManager?.OnRoundEnd(u, this);
        }
    }

    public void ResolveDefeatUnit(BaseUnitInstance defeatedBy, BaseUnitInstance defeated)
    {
        if (defeatedBy == null || defeated == null) return;
        
        defeatedBy.BuffManager?.OnUnitDefeated(defeatedBy, defeated, this);
        
    }
    
}
