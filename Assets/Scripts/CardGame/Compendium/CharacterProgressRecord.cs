using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class CharacterProgressRecord
{
    [SerializeField]
    private string characterId;

    [SerializeField]
    private CardClass cardClass;

    [SerializeField]
    private bool isUnlocked;

    [SerializeField]
    private int wins;

    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("loss")]
    private int losses;

    [SerializeField] private Dictionary<string, int> defeatedBosses = new();

    [FormerlySerializedAs("elitesKilled")] [SerializeField] private int eliteNodeCompleted;
    [SerializeField] private int bossNodeCompleted;
    [SerializeField] private int nonEliteNodeCompleted;
    [SerializeField] private int eventNodeCompleted;
    [SerializeField] private int shopNodeCompleted;
    [SerializeField] private int restsiteNodeCompleted;
    [SerializeField] private int treasureNodeCompleted;
    

    [SerializeField] private int totalGames;

    public string CharacterId => characterId;
    public CardClass CardClass => cardClass;
    public bool IsUnlocked => isUnlocked;
    public int Wins => wins;
    public int Losses => losses;
    public IReadOnlyDictionary<string, int> DefeatedBosses => defeatedBosses;
    public int EliteNodeCompleted => eliteNodeCompleted;
    public int BossNodeCompleted => bossNodeCompleted;
    public int NonEliteNodeCompleted => nonEliteNodeCompleted;
    public int EventNodeCompleted => eventNodeCompleted;
    public int ShopNodeCompleted => shopNodeCompleted;
    public int RestsiteNodeCompleted => restsiteNodeCompleted;
    public int TreasureNodeCompleted => treasureNodeCompleted;
    public int TotalGames => totalGames;
    

    public CharacterProgressRecord(
        string characterId,
        CardClass cardClass,
        bool unlockedByDefault)
    {
        this.characterId = characterId;
        this.cardClass = cardClass;
        isUnlocked = unlockedByDefault;
    }

    public bool Unlock()
    {
        if (isUnlocked)
            return false;

        isUnlocked = true;
        return true;
    }

    public void RecordGame(bool won)
    {
        totalGames++;

        if (won)
            wins++;
        else
            losses++;
    }

    public void RecordNonElite()
    {
        nonEliteNodeCompleted++;
    }
    
    public void RecordElite()
    {
        eliteNodeCompleted++;
    }

    public void RecordBoss(string bossId)
    {
        if (string.IsNullOrWhiteSpace(bossId))
            return;

        EnsureCollectionsInitialized();
        bossNodeCompleted++;

        if (!defeatedBosses.ContainsKey(bossId))
        {
            defeatedBosses.Add(bossId, 0);
        }
        defeatedBosses[bossId]++;
    }

    public void RecordShop()
    {
        shopNodeCompleted++;
    }

    public void RecordEvent()
    {
        eventNodeCompleted++;
    }

    public void RecordRestSite()
    {
        restsiteNodeCompleted++;
    }

    public void RecordTreasure()
    {
        treasureNodeCompleted++;
    }

    public bool ReconcileIdentity(
        PlayerUnitData character)
    {
        if (character == null)
            return false;

        bool changed = false;

        if (string.IsNullOrWhiteSpace(characterId))
        {
            characterId = character.CharacterID;
            changed = true;
        }

        if (cardClass != character.PlayerCardClass)
        {
            cardClass = character.PlayerCardClass;
            changed = true;
        }

        if (character.IsUnlockedByDefault && !isUnlocked)
        {
            isUnlocked = true;
            changed = true;
        }

        return changed;
    }

    internal void EnsureCollectionsInitialized()
    {
        defeatedBosses ??= new Dictionary<string, int>();
    }

    internal static CharacterProgressRecord FromPersistence(
        string characterId,
        CardClass cardClass,
        bool isUnlocked,
        int wins,
        int losses,
        Dictionary<string, int> defeatedBosses,
        int eliteNodeCompleted,
        int bossNodeCompleted,
        int nonEliteNodeCompleted,
        int eventNodeCompleted,
        int shopNodeCompleted,
        int restsiteNodeCompleted,
        int treasureNodeCompleted,
        int totalGames)
    {
        CharacterProgressRecord record = new(
            characterId,
            cardClass,
            isUnlocked)
        {
            wins = wins,
            losses = losses,
            defeatedBosses = defeatedBosses,
            eliteNodeCompleted = eliteNodeCompleted,
            bossNodeCompleted = bossNodeCompleted,
            nonEliteNodeCompleted = nonEliteNodeCompleted,
            eventNodeCompleted = eventNodeCompleted,
            shopNodeCompleted = shopNodeCompleted,
            restsiteNodeCompleted = restsiteNodeCompleted,
            treasureNodeCompleted = treasureNodeCompleted,
            totalGames = totalGames
        };

        record.EnsureCollectionsInitialized();
        return record;
    }
}
