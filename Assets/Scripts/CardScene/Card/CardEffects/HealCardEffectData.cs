using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Heal Effect")]
public class HealCardEffectData : CardEffectData
{
    [SerializeField] private int amount;
    
    public override void Execute(CardContext context)
    {
        base.Execute(context);
        
        context.CombatSystem.ResolveHeal(context.Source, context.Target, amount);
    }
}
