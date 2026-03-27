using UnityEngine;

[CreateAssetMenu(fileName = "New UnitData", menuName = "Cards/Unit Data")]
public class UnitData : ScriptableObject
{
    
}

public enum UnitType
{
    Player,
    Enemy,
    Summons,
    Osty
}