using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardEffectData
{
    [SerializeReference] private BaseCardEffect cardEffect;
    [SerializeReference] private BaseCardEffectConfig cardEffectConfig;
    [SerializeField] private CardEffectTrigger trigger = CardEffectTrigger.OnPlay;
    [SerializeReference] private List<BaseCardCondition> conditions = new();
    
    public BaseCardEffect CardEffect => cardEffect;
    public BaseCardEffectConfig CardEffectConfig => cardEffectConfig;
    public CardEffectTrigger Trigger => trigger;
    public IReadOnlyList<BaseCardCondition> Conditions => conditions;

    public void SetConfig(BaseCardEffectConfig config)
    {
        this.cardEffectConfig = config;
    }

}
