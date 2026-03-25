using UnityEngine;
using System;

[Serializable]
public class DmgCardEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private int amount;
    [SerializeField] private int numberOfAttacks;
    
    public int Amount => amount;
    public int NumberOfAttacks => numberOfAttacks;
}
