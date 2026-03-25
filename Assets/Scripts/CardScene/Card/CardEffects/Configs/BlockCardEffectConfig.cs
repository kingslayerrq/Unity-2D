using UnityEngine;
using System;

[Serializable]
public class BlockCardEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private int amount;
    
    public int Amount => amount;
}
