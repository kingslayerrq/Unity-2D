using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage Effect")]
public class DMGCardEffectData : CardEffectData
{
    [SerializeField] private int amount;

    public override void Execute(CardContext context)
    {
        base.Execute(context);

        context.CombatSystem.ResolveDamage(context.Source, context.Target, amount, context.CardInstance.Data.CardType);
    }
}
