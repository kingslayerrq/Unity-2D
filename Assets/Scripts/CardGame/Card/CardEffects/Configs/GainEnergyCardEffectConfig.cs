using System;
using UnityEngine;

[Serializable]
public class GainEnergyCardEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private int amount;
    public int Amount => amount;
}
