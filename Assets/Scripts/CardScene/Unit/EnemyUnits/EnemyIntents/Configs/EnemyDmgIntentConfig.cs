using System;
using UnityEngine;

[Serializable]
public class EnemyDmgIntentConfig : BaseEnemyIntentConfig
{
    [SerializeField] private int amount;
    [SerializeField] private int numberOfAttacks;
    
    public int Amount => amount;
    public int NumberOfAttacks => numberOfAttacks;
}
