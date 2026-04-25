using System;
using UnityEngine;

public enum ExhaustMode
{
    ThisCard,
    RandomHandCard,
    SelectHandCard,
}

[Serializable]
public class ExhaustCardEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private ExhaustMode exhaustMode;
    public ExhaustMode ExhaustMode => exhaustMode;
}
