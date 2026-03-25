namespace CardScene.Combat.Hooks
{
    public interface IOnTurnStart
    {
        void OnTurnStart(UnitInstance owner, CombatSystem combatSystem);
    }
    
    public interface IOnTurnEnd
    {
        void OnTurnEnd(UnitInstance owner, CombatSystem combatSystem);
    }

    public interface IOnEnergySpent
    {
        void OnEnergySpent(UnitInstance owner, CombatSystem combatSystem);
    }

    public interface IOnBeforeBlockReset
    {
        void OnBeforeBlockReset(UnitInstance owner, ResetBlockEvent blockEvent, CombatSystem combatSystem);
    }

    public interface IOnAfterBlockReset
    {
        void OnAfterBlockReset(UnitInstance owner, ResetBlockEvent blockEvent, CombatSystem combatSystem);
    }
    public interface IOnCardPlayed
    {
        void OnCardPlayed(UnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem);
    }

    public interface IOnCardDrawn
    {
        void OnCardDrawn(UnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem);
    }

    public interface IOnCardDiscarded
    {
        void OnCardDiscarded(UnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem);
    }
    
    public interface IOnBeforeDealDamage
    {
        void OnBeforeDealDamage(UnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnBeforeTakeDamage
    {
        void OnBeforeTakeDamage(UnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnAfterDealDamage
    {
        void OnAfterDealDamage(UnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnAfterTakeDamage
    {
        void OnAfterTakeDamage(UnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnApplyBlock
    {
        void OnApplyBlock(UnitInstance owner, BlockEvent blockEvent, CombatSystem combatSystem);
    }

    public interface IOnReceiveBlock
    {
        void OnReceiveBlock(UnitInstance owner, BlockEvent blockEvent, CombatSystem combatSystem);
    }

    public interface IOnApplyHeal
    {
        void OnApplyHeal(UnitInstance owner, HealEvent healEvent, CombatSystem combatSystem);
    }

    public interface IOnReceiveHeal
    {
        void OnReceiveHeal(UnitInstance owner, HealEvent healEvent, CombatSystem combatSystem);
    }

    public interface IOnApplyBuff
    {
        void OnApplyBuff(UnitInstance owner, BuffData buffData, int stacks, CombatSystem combatSystem);
    }

    public interface IOnReceiveBuff
    {
        void OnReceiveBuff(UnitInstance owner, BuffData buffData, int stacks, CombatSystem combatSystem);
    }
    
}