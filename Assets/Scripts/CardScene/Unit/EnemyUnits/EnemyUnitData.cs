using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Units/Enemy Data")]
public class EnemyUnitData : UnitData
{
    [FormerlySerializedAs("availableActions")]
    [Header("Available Intents")]
    [SerializeField] protected List<EnemyIntentData> availableIntents;
    
    public IReadOnlyList<EnemyIntentData> AvailableIntents => availableIntents;
}