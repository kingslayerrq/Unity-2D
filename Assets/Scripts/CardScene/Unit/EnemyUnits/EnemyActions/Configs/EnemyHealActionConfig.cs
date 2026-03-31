using System;
using UnityEngine;

[Serializable]
public class EnemyHealActionConfig : BaseEnemyActionConfig
{
    [SerializeField] private int amount;
    
    public int Amount => amount;
}
