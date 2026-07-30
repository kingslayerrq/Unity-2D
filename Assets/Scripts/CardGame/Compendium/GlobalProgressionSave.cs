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
    private List<string> defeatedBossIds = new();
    
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

    public List<string> DefeatedBossIds => defeatedBossIds;

    public List<CharacterProgressRecord> CharacterProgress =>
        characterProgress;
    
    public List<RelicProgressRecord> RelicProgress => relicProgress;

    public void EnsureCollectionsInitialized()
    {
        cardProgress ??= new List<CardProgressRecord>();
        defeatedBossIds ??= new List<string>();
        characterProgress ??= new List<CharacterProgressRecord>();
        relicProgress ??= new List<RelicProgressRecord>();
    }
}
