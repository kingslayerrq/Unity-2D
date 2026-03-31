using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Units/Enemy Data")]
public class EnemyUnitData : UnitData
{
    [Header("Available actions")]
    [SerializeField] protected List<EnemyActionData> availableActions;
    
    public IReadOnlyList<EnemyActionData> AvailableActions => availableActions;
}