using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Draw Card Effect")]
public class DrawCardEffectData : CardEffectData
{
    [SerializeField] private int amount;
    
    public override void Execute(CardContext context)
    {
        base.Execute(context);
        
        context.CombatSystem.ResolveDrawCards(context.Source, context.Target, amount);
    }
}
