using System;
using UnityEngine;

[Serializable]
public class SelectDiscardCardEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private DiscardMode discardMode;
    [SerializeField] private int count = 1;
    [SerializeField] private bool isOptional = false;   // "discard up to N"
    [SerializeField] private bool excludeSelf = true;   // skip the card that played the effect

    public DiscardMode DiscardMode => discardMode;
    public int Count => count;
    public bool IsOptional => isOptional;
    public bool ExcludeSelf => excludeSelf;
}

public enum DiscardMode
{
    RandomHandCard,
    SelectHandCard,
    EntireHand,        
}
