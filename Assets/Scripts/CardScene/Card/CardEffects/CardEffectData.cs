using System;
using UnityEngine;

[Serializable]
public class CardEffectData
{
    [SerializeReference] private BaseCardEffect cardEffect;

    [SerializeReference] private BaseCardEffectConfig cardEffectConfig;
    
    public BaseCardEffect CardEffect => cardEffect;
    public BaseCardEffectConfig CardEffectConfig => cardEffectConfig;

    public void SetConfig(BaseCardEffectConfig config)
    {
        this.cardEffectConfig = config;
    }

}
