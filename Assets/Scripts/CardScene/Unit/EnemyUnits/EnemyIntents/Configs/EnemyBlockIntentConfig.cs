using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EnemyBlockIntentConfig : BaseEnemyIntentConfig
{
    [SerializeField] private int amount; 
    [SerializeField] private bool isFromCard;
    
    public int Amount => amount;
    public bool IsFromCard => isFromCard;
}
