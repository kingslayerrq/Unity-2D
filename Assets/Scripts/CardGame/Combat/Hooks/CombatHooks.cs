namespace CardScene.Combat.Hooks
{
    public interface IOnTurnStart
    {
        void OnTurnStart(BaseUnitInstance owner, CombatSystem combatSystem);
    }
    
    public interface IOnTurnEnd
    {
        void OnTurnEnd(BaseUnitInstance owner, CombatSystem combatSystem);
    }

    public interface IOnEnergySpent
    {
        void OnEnergySpent(BaseUnitInstance owner, CombatSystem combatSystem);
    }

    public interface IOnBeforeBlockReset
    {
        void OnBeforeBlockReset(BaseUnitInstance owner, ResetBlockEvent blockEvent, CombatSystem combatSystem);
    }

    public interface IOnAfterBlockReset
    {
        void OnAfterBlockReset(BaseUnitInstance owner, ResetBlockEvent blockEvent, CombatSystem combatSystem);
    }
    public interface IOnCardPlayed
    {
        void OnCardPlayed(BaseUnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem);
    }

    public interface IOnCardDrawn
    {
        void OnCardDrawn(BaseUnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem);
    }

    public interface IOnCardDiscarded
    {
        void OnCardDiscarded(BaseUnitInstance owner, CardInstance cardInstance, CombatSystem combatSystem);
    }
    
    public interface IOnBeforeDealDamage
    {
        void OnBeforeDealDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnBeforeTakeDamage
    {
        void OnBeforeTakeDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnAfterDealDamage
    {
        void OnAfterDealDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnAfterTakeDamage
    {
        void OnAfterTakeDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem);
    }

    public interface IOnApplyBlock
    {
        void OnApplyBlock(BaseUnitInstance owner, BlockEvent blockEvent, CombatSystem combatSystem);
    }

    public interface IOnReceiveBlock
    {
        void OnReceiveBlock(BaseUnitInstance owner, BlockEvent blockEvent, CombatSystem combatSystem);
    }

    public interface IOnApplyHeal
    {
        void OnApplyHeal(BaseUnitInstance owner, HealEvent healEvent, CombatSystem combatSystem);
    }

    public interface IOnReceiveHeal
    {
        void OnReceiveHeal(BaseUnitInstance owner, HealEvent healEvent, CombatSystem combatSystem);
    }

    public interface IOnApplyBuff
    {
        void OnApplyBuff(BaseUnitInstance owner, BuffData buffData, int stacks, CombatSystem combatSystem);
    }

    public interface IOnReceiveBuff
    {
        void OnReceiveBuff(BaseUnitInstance owner, BuffData buffData, int stacks, CombatSystem combatSystem);
    }

    public interface IOnUnitDefeated
    {
        void OnUnitDefeated(BaseUnitInstance owner, BaseUnitInstance defeatedUnit, CombatSystem combatSystem);
    }
    
}