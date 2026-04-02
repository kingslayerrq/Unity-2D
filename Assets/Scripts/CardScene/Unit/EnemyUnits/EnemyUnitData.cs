using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Units/Enemy Data")]
public class EnemyUnitData : UnitData
{
    [Header("Available actions")]
    [SerializeField] protected List<EnemyIntentData> availableActions;
    
    public IReadOnlyList<EnemyIntentData> AvailableActions => availableActions;
}