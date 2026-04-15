using CardScene.Combat.Hooks;
using UnityEngine;

public class VulnerableBuffInstance : BuffInstance, IOnBeforeTakeDamage
{
    public VulnerableBuffInstance(BuffData data, int stacks) : base(data, stacks)
    {
        
    }


    public void OnBeforeTakeDamage(BaseUnitInstance owner, DamageEvent damageEvent, CombatSystem combatSystem)
    {
        if (owner == null) return;
        var type = damageEvent.CardType;
        if (type == CardType.Attack)
        {
            damageEvent.Amount = Mathf.CeilToInt(damageEvent.Amount * 1.5f);
            Debug.Log("VulnerableBuff applied on damage taken, final damage: " + damageEvent.Amount);
        }
    }
}
