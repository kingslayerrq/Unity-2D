using System;
using UnityEngine;

[Serializable]
public class EnemyBuffIntentConfig : BaseEnemyIntentConfig
{
    [SerializeField] private BuffData buffData;
    [SerializeField] private int stacks;
    
    public BuffData BuffData => buffData;
    public int Stacks => stacks;
}
