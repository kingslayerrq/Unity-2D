using UnityEngine;

public class EnemyDMGActionConfig : BaseEnemyActionConfig
{
    [SerializeField] private int amount;
    [SerializeField] private int numberOfAttacks;
    
    public int Amount => amount;
    public int NumberOfAttacks => numberOfAttacks;
}
