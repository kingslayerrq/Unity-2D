using UnityEngine;
using System;

[Serializable]
public class HealCardEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private int amount;
    
    public int Amount => amount;
}
