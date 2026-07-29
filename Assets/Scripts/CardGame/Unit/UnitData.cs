using UnityEngine;
using UnityEngine.Serialization;


public abstract class UnitData : ScriptableObject
{
    [Header("Unit Info")]
    [SerializeField] private string unitName;
    [TextArea]
    [SerializeField] private string unitDescription;
    [SerializeField] private UnitType unitType;
    [FormerlySerializedAs("unitSprite")] [SerializeField] private Sprite unitSpriteCombat;
    [SerializeField] private int maxHealth;

    public string UnitName => unitName;

    public string UnitDescription => unitDescription;

    public UnitType UnitType => unitType;

    public Sprite UnitSprite => unitSpriteCombat;

    public int MaxHealth => maxHealth;
}

public enum UnitType
{
    Player,
    Enemy,
    Summons,
    Osty
}