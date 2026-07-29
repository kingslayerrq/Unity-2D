using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class GlobalProgressionSave
{
    public const int CurrentVersion = 1;

    [SerializeField]
    private int version = CurrentVersion;

    [SerializeField]
    private List<CardProgressRecord> cards = new();

    [SerializeField]
    private int totalRunWins;

    [SerializeField]
    private List<string> defeatedBossIds = new();

    [SerializeField]
    private List<CharacterProgressRecord> characterProgress = new();

    public int Version => version;
    public List<CardProgressRecord> Cards => cards;

    public int TotalRunWins
    {
        get => totalRunWins;
        set => totalRunWins = value;
    }

    public List<string> DefeatedBossIds => defeatedBossIds;

    public List<CharacterProgressRecord> CharacterProgress =>
        characterProgress;

    public void EnsureCollectionsInitialized()
    {
        cards ??= new List<CardProgressRecord>();
        defeatedBossIds ??= new List<string>();
        characterProgress ??= new List<CharacterProgressRecord>();
    }
}
