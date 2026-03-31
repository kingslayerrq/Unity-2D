using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    
    public void ResolvePlayCard(BasePlayerUnitInstance source, List<BaseUnitInstance> target, CardInstance cardInstance)
    {
        if (source == null || cardInstance == null) return;
        if (cardInstance.RequireTarget && target == null) return;
        
        CardContext context = new CardContext(source, target, cardInstance, source.CombatManager);
        
        foreach (CardEffectData effect in cardInstance.Data.CardEffects)
        {
            if (effect == null) continue;
            Debug.Log($"Executing {cardInstance.Data.CardName}");
            effect.CardEffect.Execute(context, effect);
        }
        
        // Trigger on card played
        source.BuffManager.OnCardPlayed(source, cardInstance, this);
        
        if (source.Hand.RemoveCard(cardInstance))
        {
            source.CardPileManager.DiscardPile.AddCardToPile(cardInstance);
        }
        else
        {
            Debug.LogWarning($"Card {cardInstance.Data.CardName} failed to move to discard pile!");
        }
        
    }

    public void ResolveSpendEnergy(BaseUnitInstance source, List<BasePlayerUnitInstance> targets, int amount)
    {
        if (source == null || targets == null) return;

        foreach (var t in targets)
        {
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

    public void ResolveBlockReset(BaseUnitInstance source, List<BaseUnitInstance> targets, BlockResetSource resetSource)
    {
        if (source == null || targets == null) return;

        foreach (var t in targets)
        {
            ResetBlockEvent resetEvent = new ResetBlockEvent(source, t, resetSource);
        
            t.BuffManager.OnBeforeBlockReset(t, resetEvent, this);
        
            t.ResetBlock(resetEvent);
        
            t.BuffManager.OnAfterBlockReset(t, resetEvent, this);
        }
        
    }
    
    public void ResolveDrawCards(BaseUnitInstance source, List<BasePlayerUnitInstance> targets, int amount)
    {
        if (source == null || targets == null) return;

        foreach (var t in targets)
        {
            for (int i = 0; i < amount; i++)
            {
                if (!t.TryDrawToHand(out CardInstance cardInstance)) break;
            
                // Trigger on card drawn
                t.BuffManager.OnCardDrawn(t, cardInstance, this);

            }
        }
    }

    public void ResolveDiscard(BaseUnitInstance source, List<BasePlayerUnitInstance> targets, IReadOnlyList<CardInstance> cards)
    {
        if (source == null || targets == null) return;

        var cardsToDiscard = cards.ToList();
        foreach (var t in targets)
        {
            foreach (var card in cardsToDiscard)
            {
                t.DiscardCard(card);
                Debug.Log("Discard Card: " + card.CurrentName);
                // Trigger on card discarded
                t.BuffManager.OnCardDiscarded(t, card, this);
            }
        }
    }

    public void ResolveDamage(BaseUnitInstance source, List<BaseUnitInstance> targets, int amount, CardType cardType)
    {
        if (source == null || targets == null) return;

        foreach (var t in targets)
        {
            DamageEvent damageEvent = new DamageEvent(source, t, amount, cardType);
        
            source.BuffManager.OnBeforeDealDamage(source, damageEvent, this);
            t.BuffManager.OnBeforeTakeDamage(t, damageEvent, this);
        
            t.TakeDamage(damageEvent.Amount);
        
            source.BuffManager.OnAfterDealDamage(source, damageEvent, this);
            t.BuffManager.OnAfterTakeDamage(t, damageEvent, this);
        }
    }

    public void ResolveHeal(BaseUnitInstance source, List<BaseUnitInstance> targets, int amount)
    {
        if (source == null || targets == null) return;

        foreach (var t in targets)
        {
            HealEvent healEvent = new HealEvent(source, t, amount);

            source.BuffManager.OnApplyHeal(source, healEvent, this);
        
            t.Heal(healEvent.Amount);
        
            t.BuffManager.OnReceiveHeal(t, healEvent, this);
        }
    }
    
    public void ResolveBlocks(BaseUnitInstance source, List<BaseUnitInstance> targets, int amount, bool isFromCard)
    {
        if (source == null || targets == null) return;

        foreach (var t in targets)
        {
            BlockEvent blockEvent = new BlockEvent(source, t, amount, isFromCard);

            source.BuffManager.OnApplyBlock(source, blockEvent, this);

            t.Block(blockEvent.Amount);
        
            t.BuffManager.OnReceiveBlock(t, blockEvent, this);
        }
    }
    
    public void ResolveBuffs(BaseUnitInstance source, List<BaseUnitInstance> targets, BuffData buffData, int stacks)
    {
        if (source == null || targets == null) return;

        foreach (var t in targets)
        {
            source.BuffManager.OnApplyBuff(source, buffData, stacks, this);
        
            t.ReceiveBuff(buffData, stacks);
        
            t.BuffManager.OnReceiveBuff(t, buffData, stacks, this);
        }
    }

    public void ResolveEndTurn(BaseUnitInstance source, List<BaseUnitInstance> allUnits)
    {
        if (source == null || allUnits == null || allUnits.Count == 0) return;

        foreach (var u in allUnits)
        {
            u.BuffManager?.OnTurnEnd(u, this);
        }
        
    }
    
}
