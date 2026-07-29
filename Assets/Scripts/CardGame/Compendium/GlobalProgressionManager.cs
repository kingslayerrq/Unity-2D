using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GlobalProgressionManager : MonoBehaviour
{
    public static GlobalProgressionManager Instance { get; private set; }
    
    public static bool HasInstance => Instance != null;

    [SerializeField] private CardCatalog cardCatalog;
    [SerializeField] private CharacterCatalog characterCatalog;
    [SerializeField] private bool persistBetweenScenes = true;

    public event Action ProgressionChanged;

    public int TotalRunWins => currentSave.TotalRunWins;

    private readonly Dictionary<string, CardProgressRecord>
        cardProgressById = new();

    private readonly Dictionary<string, CharacterProgressRecord>
        characterProgressById = new();

    private GlobalProgressionSave currentSave;
    private IProgressionStore progressionStore;
    private bool isDirty;

    public event Action<CardData, CardProgressRecord>
        CardProgressChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "Duplicate GlobalProgressionManager destroyed.",
                gameObject);

            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistBetweenScenes)
            DontDestroyOnLoad(gameObject);

        progressionStore ??=
            new LocalJsonProgressionStore(
                Application.persistentDataPath);

        Load();
        ReconcileWithCatalogs();
        SaveCheckpoint();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SaveCheckpoint();
        Instance = null;
    }

    #region Card progress
    public bool IsUnlocked(CardData card)
    {
        if (card == null)
            return false;

        if (TryGetProgress(card, out CardProgressRecord record))
            return record.IsUnlocked;

        // Fallback for cards not yet reconciled.
        return card.IsUnlockedByDefault;
    }

    public bool IsDiscovered(CardData card)
    {
        if (card == null)
            return false;

        if (TryGetProgress(card, out CardProgressRecord record))
            return record.IsDiscovered;

        return card.IsUnlockedByDefault;
    }

    public void DiscoverCard(CardData card)
    {
        CardProgressRecord record = EnsureRecord(card);

        if (!record.Discover())
            return;

        CommitCardChange(card, record);
    }

    public void UnlockCard(CardData card)
    {
        CardProgressRecord record = EnsureRecord(card);

        if (!record.Unlock())
            return;

        CommitCardChange(
            card,
            record,
            saveImmediately: true);
    }

    public void RecordCardObtained(CardInstance card)
    {
        if (card == null)
            throw new ArgumentNullException(nameof(card));

        CardProgressRecord record =
            EnsureRecord(card.Data);

        record.RecordObtained();

        CommitCardChange(card.Data, record);
    }

    public void RecordCardPlayed(CardInstance card)
    {
        if (card == null)
            throw new ArgumentNullException(nameof(card));

        CardProgressRecord record = EnsureRecord(card.Data);
        record.RecordPlayed();

        CommitCardChange(card.Data, record);
    }

    public void RecordRunWonWithCard(CardData card)
    {
        CardProgressRecord record = EnsureRecord(card);
        record.RecordRunWon();

        CommitCardChange(card, record);
    }

    public bool TryGetProgress(
        CardData card,
        out CardProgressRecord record)
    {
        record = null;

        if (card == null)
            return false;

        if (string.IsNullOrWhiteSpace(card.ID))
            return false;

        return cardProgressById.TryGetValue(
            card.ID,
            out record);
    }

    /// <summary>
    /// Return the record, and create it if necessary.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private CardProgressRecord EnsureRecord(CardData card)
    {
        if (card == null)
            throw new ArgumentNullException(nameof(card));

        if (string.IsNullOrWhiteSpace(card.ID))
        {
            throw new InvalidOperationException(
                $"Card '{card.name}' has no persistent ID.");
        }

        if (cardProgressById.TryGetValue(
                card.ID,
                out CardProgressRecord record))
        {
            return record;
        }

        record = new CardProgressRecord(
            card.ID,
            card.IsUnlockedByDefault);

        cardProgressById.Add(card.ID, record);
        currentSave.Cards.Add(record);
        MarkDirty();

        return record;
    }


    #endregion

    public bool HasDefeatedBoss(string bossId)
    {
        if (string.IsNullOrWhiteSpace(bossId))
            return false;

        return currentSave.DefeatedBossIds.Contains(bossId);
    }

    #region Character progress

    public bool IsUnlocked(PlayerUnitData character)
    {
        if (character == null)
            return false;

        if (TryGetProgress(
                character,
                out CharacterProgressRecord record))
        {
            return record.IsUnlocked;
        }

        return character.IsUnlockedByDefault;
    }

    public bool TryGetProgress(
        PlayerUnitData character,
        out CharacterProgressRecord record)
    {
        record = null;

        if (character == null ||
            string.IsNullOrWhiteSpace(character.CharacterID))
        {
            return false;
        }

        return characterProgressById.TryGetValue(
            character.CharacterID,
            out record);
    }

    public int GetCharacterWins(
        PlayerUnitData character)
    {
        return TryGetProgress(
            character,
            out CharacterProgressRecord record)
                ? record.Wins
                : 0;
    }

    public int GetCharacterTotalGames(
        PlayerUnitData character)
    {
        return TryGetProgress(
            character,
            out CharacterProgressRecord record)
                ? record.TotalGames
                : 0;
    }

    public int GetCharacterWins(CardClass cardClass)
    {
        int wins = 0;

        foreach (CharacterProgressRecord record
                 in currentSave.CharacterProgress)
        {
            if (record != null &&
                record.CardClass == cardClass)
            {
                wins += record.Wins;
            }
        }

        return wins;
    }

    public int GetCharacterTotalGames(CardClass cardClass)
    {
        int totalGames = 0;

        foreach (CharacterProgressRecord record
                 in currentSave.CharacterProgress)
        {
            if (record != null &&
                record.CardClass == cardClass)
            {
                totalGames += record.TotalGames;
            }
        }

        return totalGames;
    }

    public void UnlockCharacter(
        PlayerUnitData character)
    {
        CharacterProgressRecord record =
            EnsureCharacterRecord(character);

        if (!record.Unlock())
            return;

        CommitCharacterChange();
    }

    public void RecordRunCompleted(
        PlayerUnitData character,
        bool won)
    {
        CharacterProgressRecord record =
            EnsureCharacterRecord(character);

        record.RecordGame(won);

        if (won)
            currentSave.TotalRunWins++;

        CommitCharacterChange();
    }

    private CharacterProgressRecord EnsureCharacterRecord(
        PlayerUnitData character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));

        if (string.IsNullOrWhiteSpace(character.CharacterID))
        {
            throw new InvalidOperationException(
                $"Character '{character.name}' has no persistent ID.");
        }

        if (characterProgressById.TryGetValue(
                character.CharacterID,
                out CharacterProgressRecord record))
        {
            return record;
        }

        record = new CharacterProgressRecord(
            character.CharacterID,
            character.PlayerCardClass,
            character.IsUnlockedByDefault);

        characterProgressById.Add(
            character.CharacterID,
            record);

        currentSave.CharacterProgress.Add(record);
        MarkDirty();

        return record;
    }

    #endregion

    public void RecordBossDefeated(string bossId)
    {
        if (string.IsNullOrWhiteSpace(bossId))
            return;

        if (currentSave.DefeatedBossIds.Contains(bossId))
            return;

        currentSave.DefeatedBossIds.Add(bossId);

        MarkDirty();
        ProgressionChanged?.Invoke();
        SaveCheckpoint();
    }

    private void CommitCardChange(
        CardData card,
        CardProgressRecord record,
        bool saveImmediately = false)
    {
        MarkDirty();
        CardProgressChanged?.Invoke(card, record);
        ProgressionChanged?.Invoke();

        if (saveImmediately)
            SaveCheckpoint();
    }

    private void CommitCharacterChange()
    {
        MarkDirty();
        ProgressionChanged?.Invoke();
        SaveCheckpoint();
    }

    private void ReconcileWithCatalogs()
    {
        bool changed = ReconcileCardsWithCatalog();
        changed |= ReconcileCharactersWithCatalog();

        if (changed)
            MarkDirty();
    }

    private bool ReconcileCardsWithCatalog()
    {
        if (cardCatalog == null)
        {
            Debug.LogError(
                "Global progression requires a card catalog.",
                this);

            return false;
        }

        bool changed = false;

        foreach (CardData card in cardCatalog.Cards)
        {
            if (card == null)
                continue;

            if (string.IsNullOrWhiteSpace(card.ID))
            {
                Debug.LogError(
                    $"Card '{card.name}' has no persistent ID.",
                    card);

                continue;
            }

            if (cardProgressById.TryGetValue(
                    card.ID,
                    out CardProgressRecord existingRecord))
            {
                changed |=
                    existingRecord.ReconcileDefaultUnlock(
                        card.IsUnlockedByDefault);
                continue;
            }

            CardProgressRecord record = new(
                card.ID,
                card.IsUnlockedByDefault);

            cardProgressById.Add(card.ID, record);
            currentSave.Cards.Add(record);

            changed = true;
        }

        return changed;
    }

    private bool ReconcileCharactersWithCatalog()
    {
        if (characterCatalog == null)
        {
            Debug.LogError(
                "Global progression requires a character catalog.",
                this);

            return false;
        }

        bool changed = false;

        foreach (PlayerUnitData character
                 in characterCatalog.Characters)
        {
            if (character == null)
                continue;

            if (string.IsNullOrWhiteSpace(character.CharacterID))
            {
                Debug.LogError(
                    $"Character '{character.name}' has no " +
                    $"persistent ID.",
                    character);

                continue;
            }

            if (!characterProgressById.TryGetValue(
                    character.CharacterID,
                    out CharacterProgressRecord record))
            {
                record = FindLegacyCharacterRecord(character);

                if (record == null)
                {
                    record = new CharacterProgressRecord(
                        character.CharacterID,
                        character.PlayerCardClass,
                        character.IsUnlockedByDefault);

                    currentSave.CharacterProgress.Add(record);
                }

                if (!characterProgressById.TryAdd(
                        character.CharacterID,
                        record))
                {
                    Debug.LogWarning(
                        $"Duplicate progression record for " +
                        $"character ID '{character.CharacterID}'.");

                    continue;
                }

                changed = true;
            }

            changed |= record.ReconcileIdentity(character);
        }

        return changed;
    }

    private CharacterProgressRecord FindLegacyCharacterRecord(
        PlayerUnitData character)
    {
        foreach (CharacterProgressRecord record
                 in currentSave.CharacterProgress)
        {
            if (record != null &&
                string.IsNullOrWhiteSpace(record.CharacterId) &&
                record.CardClass == character.PlayerCardClass)
            {
                return record;
            }
        }

        return null;
    }

    private void Load()
    {
        cardProgressById.Clear();
        characterProgressById.Clear();

        try
        {
            ProgressionLoadResult result =
                progressionStore.Load();

            currentSave = result.Save;
            isDirty = result.RequiresSave;

            if (result.Source == ProgressionLoadSource.Backup)
            {
                Debug.LogWarning(
                    "Loaded progression from the last-known-good " +
                    $"backup at '{progressionStore.Location}'.",
                    this);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to load global progression:\n{exception}",
                this);

            currentSave = new GlobalProgressionSave();
            isDirty = true;
        }

        currentSave ??= new GlobalProgressionSave();
        currentSave.EnsureCollectionsInitialized();

        foreach (CardProgressRecord record in currentSave.Cards)
        {
            if (record == null ||
                string.IsNullOrWhiteSpace(record.CardId))
            {
                continue;
            }

            if (!cardProgressById.TryAdd(
                    record.CardId,
                    record))
            {
                Debug.LogWarning(
                    $"Duplicate progression record for " +
                    $"card ID '{record.CardId}'.");
            }
        }

        foreach (CharacterProgressRecord record
                 in currentSave.CharacterProgress)
        {
            if (record == null ||
                string.IsNullOrWhiteSpace(record.CharacterId))
            {
                continue;
            }

            if (!characterProgressById.TryAdd(
                    record.CharacterId,
                    record))
            {
                Debug.LogWarning(
                    $"Duplicate progression record for " +
                    $"character ID '{record.CharacterId}'.");
            }
        }
    }

    public void SaveCheckpoint()
    {
        if (!isDirty ||
            currentSave == null ||
            progressionStore == null)
        {
            return;
        }

        try
        {
            progressionStore.Save(currentSave);
            isDirty = false;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to save global progression:\n{exception}",
                this);
        }
    }

    private void MarkDirty()
    {
        isDirty = true;
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
            SaveCheckpoint();
    }

    private void OnApplicationQuit()
    {
        SaveCheckpoint();
    }
}
