using System;
using UnityEngine;

[Serializable]
public class EnemyDmgActionConfig : BaseEnemyActionConfig
{
    [SerializeField] private int amount;
    [SerializeField] private int numberOfAttacks;
    
    public int Amount => amount;
    public int NumberOfAttacks => numberOfAttacks;
}
