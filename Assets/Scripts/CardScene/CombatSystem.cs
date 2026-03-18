using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public void PlayCard(Unit source, Unit target, CardInstance cardInstance)
    {
        if (source == null || cardInstance == null) return;
        if (cardInstance.RequireTarget && target == null) return;

        if (source.CurrentEnergy < cardInstance.CurrentCost)
        {
            Debug.Log($"Not enough energy to play {cardInstance.Data.CardName}");
            return;
        }
        
        source.SpendEnergy(cardInstance.CurrentCost);
        
        CardContext context = new CardContext(source, target, cardInstance, this);

        foreach (CardEffectData effect in cardInstance.Data.cardEffects)
        {
            if (effect == null) continue;
            effect.Execute(context);
        }
        
        // Trigger on card played
        source.BuffManager.OnCardPlayed(source, cardInstance, this);
        
        // TODO: move to discard

    }

    public void ResolveDrawCards(Unit source, Unit target, int amount)
    {
        if (source == null || target == null) return;

        for (int i = 0; i < amount; i++)
        {
            if (!target.TryDrawToHand(out CardInstance cardInstance)) break;
            
            // Trigger on card drawn
            target.BuffManager.OnCardDrawn(source, cardInstance, this);

        }
    }

    public void ResolveDamage(Unit source, Unit target, int amount, CardType cardType)
    {
        if (source == null || target == null) return;
        DamageEvent damageEvent = new DamageEvent(source, target, amount, cardType);
        
        source.BuffManager.OnBeforeDealDamage(source, damageEvent, this);
        target.BuffManager.OnBeforeTakeDamage(target, damageEvent, this);

        int finalDmg = damageEvent.Amount;
        target.TakeDamage(finalDmg);
        
        source.BuffManager.OnAfterDealDamage(source, damageEvent, this);
        target.BuffManager.OnAfterTakeDamage(target, damageEvent, this);
        
    }

    public void ResolveHeal(Unit source, Unit target, int amount)
    {
        if (source == null || target == null) return;
        HealEvent healEvent = new HealEvent(source, target, amount);

        source.BuffManager.OnApplyHeal(source, healEvent, this);
        
        target.Heal(amount);
        
        target.BuffManager.OnReceiveHeal(target, healEvent, this);

    }
    
    public void ResolveBlocks(Unit source, Unit target, int amount, bool isFromCard)
    {
        if (source == null || target == null) return;
        BlockEvent blockEvent = new BlockEvent(source, target, amount, isFromCard);

        source.BuffManager.OnApplyBlock(source, blockEvent, this);

        target.Block(amount);
        
        target.BuffManager.OnReceiveBlock(source, blockEvent, this);
    }
    
    public void ResolveBuffs(Unit source, Unit target, BuffData buffData, int stacks)
    {
        if (source == null || target == null) return;
        
        source.BuffManager.OnApplyBuff(source, buffData, stacks, this);
        
        target.ReceiveBuff(buffData, stacks);
        
        target.BuffManager.OnReceiveBuff(target, buffData, stacks, this);
    }
    
    
}
