using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class GlobalProgressionSave
{
    public const int CurrentVersion = 2;

    [SerializeField]
    private int version = CurrentVersion;

    [SerializeField]
    private int totalRunWins;
    
    private Dictionary<string, int> defeatedBosses = new();         // [bossid, count]
    
    [SerializeField]
    private int totalElitesDefeated = 0;
    
    [FormerlySerializedAs("cards")] [SerializeField]
    private List<CardProgressRecord> cardProgress = new();

    [SerializeField]
    private List<CharacterProgressRecord> characterProgress = new();
    
    [SerializeField] private List<RelicProgressRecord> relicProgress = new();

    public int Version => version;
    public List<CardProgressRecord> CardProgress => cardProgress;
    public int TotalElitesDefeated => totalElitesDefeated;
    public int TotalRunWins
    {
        get => totalRunWins;
        set => totalRunWins = value;
    }

    public IReadOnlyDictionary<string, int> DefeatedBosses => defeatedBosses;

    public List<CharacterProgressRecord> CharacterProgress =>
        characterProgress;
    
    public List<RelicProgressRecord> RelicProgress => relicProgress;

    public void RecordBossDefeated(string bossId)
    {
        if (string.IsNullOrWhiteSpace(bossId))
            return;

        defeatedBosses ??= new Dictionary<string, int>();
        defeatedBosses.TryAdd(bossId, 0);
        defeatedBosses[bossId]++;
    }

    public void RecordEliteDefeated()
    {
        totalElitesDefeated++;
    }
    

    public void EnsureCollectionsInitialized()
    {
        cardProgress ??= new List<CardProgressRecord>();
        defeatedBosses ??= new Dictionary<string, int>();
        characterProgress ??= new List<CharacterProgressRecord>();
        relicProgress ??= new List<RelicProgressRecord>();

        foreach (CharacterProgressRecord record in characterProgress)
            record?.EnsureCollectionsInitialized();
    }

    internal static GlobalProgressionSave FromPersistence(
        int totalRunWins,
        Dictionary<string, int> defeatedBosses,
        List<CardProgressRecord> cardProgress,
        List<CharacterProgressRecord> characterProgress,
        List<RelicProgressRecord> relicProgress)
    {
        GlobalProgressionSave save = new()
        {
            version = CurrentVersion,
            totalRunWins = totalRunWins,
            defeatedBosses = defeatedBosses,
            cardProgress = cardProgress,
            characterProgress = characterProgress,
            relicProgress = relicProgress
        };

        save.EnsureCollectionsInitialized();
        return save;
    }
}
