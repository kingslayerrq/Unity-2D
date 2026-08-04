using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class GlobalProgressionSave
{
    public const int CurrentVersion = 1;

    [SerializeField]
    private int version = CurrentVersion;

    [SerializeField]
    private int totalRunWins;

    [SerializeField]
    private List<EnemyUnitData> defeatedBosses = new();         // TODO: change to dict [bossdata, count]
    
    [FormerlySerializedAs("cards")] [SerializeField]
    private List<CardProgressRecord> cardProgress = new();

    [SerializeField]
    private List<CharacterProgressRecord> characterProgress = new();
    
    [SerializeField] private List<RelicProgressRecord> relicProgress = new();

    public int Version => version;
    public List<CardProgressRecord> CardProgress => cardProgress;

    public int TotalRunWins
    {
        get => totalRunWins;
        set => totalRunWins = value;
    }

    public List<EnemyUnitData> DefeatedBosses => defeatedBosses;

    public List<CharacterProgressRecord> CharacterProgress =>
        characterProgress;
    
    public List<RelicProgressRecord> RelicProgress => relicProgress;

    public void EnsureCollectionsInitialized()
    {
        cardProgress ??= new List<CardProgressRecord>();
        defeatedBosses ??= new List<EnemyUnitData>();
        characterProgress ??= new List<CharacterProgressRecord>();
        relicProgress ??= new List<RelicProgressRecord>();
    }
}
