using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public void PlayCard(UnitInstance source, UnitInstance target, CardInstance cardInstance)
    {
        if (source == null || cardInstance == null) return;
        if (cardInstance.RequireTarget && target == null) return;
        
        
        CardContext context = new CardContext(source, target, cardInstance, this);

        foreach (CardEffectData effect in cardInstance.Data.CardEffects)
        {
            if (effect == null) continue;
            // TODO: ???
            Debug.Log($"Executing {cardInstance.Data.CardName}");
            effect.CardEffect.Execute(context, effect);
        }
        
        // Trigger on card played
        target.BuffManager.OnCardPlayed(target, cardInstance, this);
        
        // TODO: move to discard
        
    }

    public void ResolveSpendEnergy(UnitInstance source, UnitInstance target, int amount)
    {
        if (source == null || target == null) return;

        if (target.CurrentEnergy < amount)
        {
            Debug.LogWarning($"Dont have enough energy! Current-{target.CurrentEnergy}: " +
                             $"Required-{amount}");
            return;
        }
        
        target.SpendEnergy(amount);
        
        for (int i = 0; i <  amount; i++)
        {
            target.BuffManager.OnEnergySpent(target, this);
        }
    }

    public void ResolveBlockReset(UnitInstance source, UnitInstance target, BlockResetSource resetSource)
    {
        if (source == null || target == null) return;
        
        ResetBlockEvent resetEvent = new ResetBlockEvent(source, target, resetSource);
        
        target.BuffManager.OnBeforeBlockReset(target, resetEvent, this);
        
        target.ResetBlock(resetEvent);
        
        target.BuffManager.OnAfterBlockReset(target, resetEvent, this);
    }
    
    public void ResolveDrawCards(UnitInstance source, UnitInstance target, int amount)
    {
        if (source == null || target == null) return;

        for (int i = 0; i < amount; i++)
        {
            if (!target.TryDrawToHand(out CardInstance cardInstance)) break;
            
            // Trigger on card drawn
            target.BuffManager.OnCardDrawn(target, cardInstance, this);

        }
    }

    public void ResolveDiscard(UnitInstance source, UnitInstance target, List<CardInstance> cards)
    {
        if (source == null || target == null) return;
        
        var amount = cards.Count;
        for (int i = 0; i < amount; i++)
        {
            target.DiscardCard(cards[i]);
            
            // Trigger on card discarded
            target.BuffManager.OnCardDiscarded(target, cards[i], this);
        }
    }

    public void ResolveDamage(UnitInstance source, UnitInstance target, int amount, CardType cardType)
    {
        if (source == null || target == null) return;
        DamageEvent damageEvent = new DamageEvent(source, target, amount, cardType);
        
        source.BuffManager.OnBeforeDealDamage(source, damageEvent, this);
        target.BuffManager.OnBeforeTakeDamage(target, damageEvent, this);
        
        target.TakeDamage(damageEvent.Amount);
        
        source.BuffManager.OnAfterDealDamage(source, damageEvent, this);
        target.BuffManager.OnAfterTakeDamage(target, damageEvent, this);
        
    }

    public void ResolveHeal(UnitInstance source, UnitInstance target, int amount)
    {
        if (source == null || target == null) return;
        HealEvent healEvent = new HealEvent(source, target, amount);

        source.BuffManager.OnApplyHeal(source, healEvent, this);
        
        target.Heal(healEvent.Amount);
        
        target.BuffManager.OnReceiveHeal(target, healEvent, this);

    }
    
    public void ResolveBlocks(UnitInstance source, UnitInstance target, int amount, bool isFromCard)
    {
        if (source == null || target == null) return;
        BlockEvent blockEvent = new BlockEvent(source, target, amount, isFromCard);

        source.BuffManager.OnApplyBlock(source, blockEvent, this);

        target.Block(blockEvent.Amount);
        
        target.BuffManager.OnReceiveBlock(target, blockEvent, this);
    }
    
    public void ResolveBuffs(UnitInstance source, UnitInstance target, BuffData buffData, int stacks)
    {
        if (source == null || target == null) return;
        
        source.BuffManager.OnApplyBuff(source, buffData, stacks, this);
        
        target.ReceiveBuff(buffData, stacks);
        
        target.BuffManager.OnReceiveBuff(target, buffData, stacks, this);
    }
    
    
}
