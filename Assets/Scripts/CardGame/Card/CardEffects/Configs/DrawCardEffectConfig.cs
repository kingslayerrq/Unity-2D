using UnityEngine;
using System;

[Serializable]
public class DrawCardEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private int amount;
    
    public int Amount => amount;
}
