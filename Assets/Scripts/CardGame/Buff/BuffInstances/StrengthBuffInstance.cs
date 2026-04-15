using CardScene.Combat.Hooks;
using UnityEngine;

public class StrengthBuffInstance : BuffInstance, IOnBeforeDealDamage
{
    public StrengthBuffInstance(BuffData data, int stacks) : base(data, stacks)
    {
    }

    public void OnBeforeDealDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem)
    {
        if (owner == null) return;
        var type = damageEvent.CardType;
        if (type == CardType.Attack)
        {
            damageEvent.Amount += Stacks;
            Debug.Log($"{Stacks} stacks of StrengthBuff applied on before damage dealt, final damage outgoing: {damageEvent.Amount}");
        }
    }
}

