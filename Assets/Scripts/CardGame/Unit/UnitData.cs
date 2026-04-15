using UnityEngine;


public abstract class UnitData : ScriptableObject
{
    [Header("Unit Info")]
    [SerializeField] private string unitName;
    [TextArea]
    [SerializeField] private string unitDescription;
    [SerializeField] private UnitType unitType;
    [SerializeField] private Sprite unitSprite;
    [SerializeField] private int maxHealth;

    public string UnitName => unitName;

    public string UnitDescription => unitDescription;

    public UnitType UnitType => unitType;

    public Sprite UnitSprite => unitSprite;

    public int MaxHealth => maxHealth;
}

public enum UnitType
{
    Player,
    Enemy,
    Summons,
    Osty
}