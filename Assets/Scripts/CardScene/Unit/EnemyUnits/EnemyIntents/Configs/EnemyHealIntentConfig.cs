using System;
using UnityEngine;

[Serializable]
public class EnemyHealIntentConfig : BaseEnemyIntentConfig
{
    [SerializeField] private int amount;
    
    public int Amount => amount;
}
