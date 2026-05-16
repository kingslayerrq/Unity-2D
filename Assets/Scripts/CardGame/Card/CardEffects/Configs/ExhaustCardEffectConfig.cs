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

    [Tooltip("Used by SelectHandCard mode. Number of cards the player picks to exhaust.")]
    [SerializeField] private int count = 1;

    [Tooltip("Used by SelectHandCard mode. If true, player may confirm with 0 selected (\"exhaust up to N\").")]
    [SerializeField] private bool isOptional = false;

    [Tooltip("Used by SelectHandCard / RandomHandCard. Skip the card that played the effect from the selection pool.")]
    [SerializeField] private bool excludeSelf = true;

    public ExhaustMode ExhaustMode => exhaustMode;
    public int Count => count;
    public bool IsOptional => isOptional;
    public bool ExcludeSelf => excludeSelf;
}
