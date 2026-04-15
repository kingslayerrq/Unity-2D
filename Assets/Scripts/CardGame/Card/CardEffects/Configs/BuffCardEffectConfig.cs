using UnityEngine;
using System;

[Serializable]
public class BuffCardEffectConfig : BaseCardEffectConfig
{
    [SerializeField] private BuffData buffData;
    [SerializeField] private int stacks;
    
    public BuffData BuffData => buffData;
    public int Stacks => stacks;
}
