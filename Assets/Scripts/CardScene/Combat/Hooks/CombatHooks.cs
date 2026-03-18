namespace CardScene.Combat.Hooks
{
    public interface IOnTurnStart
    {
        void OnTurnStart(Unit owner, CombatSystem combatSystem);
    }
    
    public interface IOnTurnEnd
    {
        void OnTurnEnd(Unit owner, CombatSystem combatSystem);
    }

    public interface IOnCardPlayed
    {
        void OnCardPlayed(Unit owner, CardInstance cardInstance, CombatSystem combatSystem);
    }

    public interface IOnCardDrawn
    {
        void OnCardDrawn(Unit owner, CardInstance cardInstance, CombatSystem combatSystem);
    }

    public interface IOnBeforeDealDamage
    {
        void OnBeforeDealDamage(Unit owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnBeforeTakeDamage
    {
        void OnBeforeTakeDamage(Unit owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnAfterDealDamage
    {
        void OnAfterDealDamage(Unit owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnAfterTakeDamage
    {
        void OnAfterTakeDamage(Unit owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnApplyBlock
    {
        void OnApplyBlock(Unit owner, BlockEvent blockEvent, CombatSystem combatSystem);
    }

    public interface IOnReceiveBlock
    {
        void OnReceiveBlock(Unit owner, BlockEvent blockEvent, CombatSystem combatSystem);
    }

    public interface IOnApplyHeal
    {
        void OnApplyHeal(Unit owner, HealEvent healEvent, CombatSystem combatSystem);
    }

    public interface IOnReceiveHeal
    {
        void OnReceiveHeal(Unit owner, HealEvent healEvent, CombatSystem combatSystem);
    }

    public interface IOnApplyBuff
    {
        void OnApplyBuff(Unit owner, BuffData buffData, int stacks, CombatSystem combatSystem);
    }

    public interface IOnReceiveBuff
    {
        void OnReceiveBuff(Unit owner, BuffData buffData, int stacks, CombatSystem combatSystem);
    }
    
}