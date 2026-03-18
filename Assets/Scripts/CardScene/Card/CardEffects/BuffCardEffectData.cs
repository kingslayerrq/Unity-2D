using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Apply Buff Effect")]
public class BuffCardEffectData : CardEffectData
{
    [SerializeField] private BuffData buffData;
    [SerializeField] private int stacks;

    public override void Execute(CardContext context)
    {
        base.Execute(context);
        
        context.CombatSystem.ResolveBuffs(context.Source, context.Target, buffData,  stacks);
    }
}
