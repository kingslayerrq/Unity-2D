using UnityEngine;
using System;

[Serializable]
public class DmgCardEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private DynamicValue amount;
    [SerializeField] private int numberOfAttacks = 1;
    
    public DynamicValue Amount => amount;
    public int NumberOfAttacks => numberOfAttacks;
}
