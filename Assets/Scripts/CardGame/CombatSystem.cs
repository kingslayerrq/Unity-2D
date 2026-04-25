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
            new EnemyIntentContext(source, intent.EnemyIntent.IntentTargets, cards, intent.EnemyIntent, source.CombatManager);
        
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
    
    
    public void ResolvePlayCard(BasePlayerUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, CardInstance cardInstance)
    {
        if (source == null || cardInstance == null) return;
        if (cardInstance.RequireTarget && targets == null) return;
        
        CardContext context = new CardContext(source, targets, cardInstance, source.CombatManager);

        for (var index = 0; index < cardInstance.CurrentCardEffects.Count; index++)
        {
            var effect = cardInstance.CurrentCardEffects[index];
            if (effect == null) continue;
            Debug.Log($"Executing {cardInstance.Data.CardName}");
            effect.CardEffect.Execute(context, effect);
        }

        // Trigger on card played
        source.BuffManager.OnCardPlayed(source, cardInstance, this);
        
        if (source.Hand.RemoveCard(cardInstance))
        {
            // exhaust
            if (cardInstance.CurrentCardType == CardType.Power || cardInstance.IsExhaustOnPlay())
                source.CardPileManager.ExhaustPile.AddCardToPile(cardInstance);
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
                return;
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

    public void ResolveDiscard(BaseUnitInstance source, IReadOnlyList<BasePlayerUnitInstance> targets, IReadOnlyList<CardInstance> cards)
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
                t.DiscardCard(card);
                Debug.Log("Discard Card: " + card.CurrentName);
                // Trigger on card discarded
                t.BuffManager.OnCardDiscarded(t, card, this);
            }
        }
    }

    public void ResolveExhaust(BasePlayerUnitInstance source, CardInstance card)
    {
        if (source == null || card == null) return;
        source.ExhaustCard(card);
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

    public void ResolveDefeatUnit(BaseUnitInstance defeatedBy, BaseUnitInstance defeated)
    {
        if (defeatedBy == null || defeated == null) return;
        
        defeatedBy.BuffManager?.OnUnitDefeated(defeatedBy, defeated, this);
        
    }
    
}
