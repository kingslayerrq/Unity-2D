using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Units/Enemy Data")]
public class EnemyUnitData : ScriptableObject
{
    [Header("Enemy Info")]
    [SerializeField] private string enemyName;
    [SerializeField] private string enemyDescription;
    [SerializeField] private UnitType unitType;
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private int maxHealth;
    
    [Header("Available actions")]
    [SerializeField] private List<CardEffectData> cardEffects;
}