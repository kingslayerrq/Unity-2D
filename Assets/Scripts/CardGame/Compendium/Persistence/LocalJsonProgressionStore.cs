using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

internal sealed class LocalJsonProgressionStore : IProgressionStore
{
    internal const string SaveFileName = "global_progression.json";

    private const int LegacyVersion = 1;

    private readonly string primaryPath;
    private readonly string backupPath;
    private readonly string temporaryPath;

    public string Location => primaryPath;

    internal string BackupPath => backupPath;
    internal string TemporaryPath => temporaryPath;

    [Serializable]
    private sealed class SaveVersionShape
    {
        public int version = int.MinValue;
    }

    [Serializable]
    private sealed class BossDefeatData
    {
        public string bossId;
        public int count;

        public BossDefeatData()
        {
        }

        public BossDefeatData(string bossId, int count)
        {
            this.bossId = bossId;
            this.count = count;
        }
    }

    [Serializable]
    private sealed class CharacterProgressData
    {
        public string characterId;
        public CardClass cardClass;
        public bool isUnlocked;
        public int wins;
        public int losses;
        public List<BossDefeatData> defeatedBosses;
        public int eliteNodeCompleted;
        public int bossNodeCompleted;
        public int nonEliteNodeCompleted;
        public int eventNodeCompleted;
        public int shopNodeCompleted;
        public int restsiteNodeCompleted;
        public int treasureNodeCompleted;
        public int totalGames;

        public CharacterProgressData()
        {
        }

        public CharacterProgressData(CharacterProgressRecord record)
        {
            if (record == null)
            {
                throw new InvalidDataException(
                    "Character progression contains a null record.");
            }

            characterId = record.CharacterId;
            cardClass = record.CardClass;
            isUnlocked = record.IsUnlocked;
            wins = record.Wins;
            losses = record.Losses;
            defeatedBosses = CreateBossDefeatData(
                record.DefeatedBosses);
            eliteNodeCompleted = record.EliteNodeCompleted;
            bossNodeCompleted = record.BossNodeCompleted;
            nonEliteNodeCompleted = record.NonEliteNodeCompleted;
            eventNodeCompleted = record.EventNodeCompleted;
            shopNodeCompleted = record.ShopNodeCompleted;
            restsiteNodeCompleted = record.RestsiteNodeCompleted;
            treasureNodeCompleted = record.TreasureNodeCompleted;
            totalGames = record.TotalGames;
        }

        public bool TryCreateRecord(
            out CharacterProgressRecord record,
            out string error)
        {
            record = null;

            if (!TryCreateBossCounts(
                    defeatedBosses,
                    out Dictionary<string, int> bossCounts,
                    out error))
            {
                return false;
            }

            record = CharacterProgressRecord.FromPersistence(
                characterId,
                cardClass,
                isUnlocked,
                wins,
                losses,
                bossCounts,
                eliteNodeCompleted,
                bossNodeCompleted,
                nonEliteNodeCompleted,
                eventNodeCompleted,
                shopNodeCompleted,
                restsiteNodeCompleted,
                treasureNodeCompleted,
                totalGames);

            return true;
        }
    }

    [Serializable]
    private sealed class ProgressionSaveData
    {
        public int version;
        public int totalRunWins;
        public int totalGames;
        public int totalElitesDefeated;
        public List<CardProgressRecord> cardProgress;
        public List<BossDefeatData> defeatedBosses;
        public List<CharacterProgressData> characterProgress;
        public List<RelicProgressRecord> relicProgress;

        public static ProgressionSaveData FromSave(
            GlobalProgressionSave save)
        {
            save.EnsureCollectionsInitialized();

            ProgressionSaveData data = new()
            {
                version = GlobalProgressionSave.CurrentVersion,
                totalRunWins = save.TotalRunWins,
                totalGames = save.TotalGames,
                totalElitesDefeated = save.TotalElitesDefeated,
                cardProgress = new List<CardProgressRecord>(
                    save.CardProgress),
                defeatedBosses = CreateBossDefeatData(
                    save.DefeatedBosses),
                characterProgress =
                    new List<CharacterProgressData>(),
                relicProgress = new List<RelicProgressRecord>(
                    save.RelicProgress)
            };

            foreach (CharacterProgressRecord record
                     in save.CharacterProgress)
            {
                data.characterProgress.Add(
                    new CharacterProgressData(record));
            }

            return data;
        }

        public bool TryCreateSave(
            out GlobalProgressionSave save,
            out string error)
        {
            save = null;
            error = null;

            if (version != GlobalProgressionSave.CurrentVersion)
            {
                error =
                    $"Unsupported version {version}; expected " +
                    $"{GlobalProgressionSave.CurrentVersion}.";
                return false;
            }

            if (cardProgress == null ||
                defeatedBosses == null ||
                characterProgress == null ||
                relicProgress == null)
            {
                error = "Required progression collections are missing.";
                return false;
            }

            if (cardProgress.Exists(record => record == null) ||
                relicProgress.Exists(record => record == null))
            {
                error = "Progression contains a null record.";
                return false;
            }

            if (!TryCreateBossCounts(
                    defeatedBosses,
                    out Dictionary<string, int> globalBossCounts,
                    out error))
            {
                return false;
            }

            List<CharacterProgressRecord> characterRecords = new();

            foreach (CharacterProgressData data in characterProgress)
            {
                if (data == null)
                {
                    error =
                        "Character progression contains a null record.";
                    return false;
                }

                if (!data.TryCreateRecord(
                        out CharacterProgressRecord record,
                        out error))
                {
                    return false;
                }

                characterRecords.Add(record);
            }

            save = GlobalProgressionSave.FromPersistence(
                totalRunWins,
                Math.Max(totalGames, SumTotalGames(characterRecords)),
                Math.Max(
                    totalElitesDefeated,
                    SumElitesDefeated(characterRecords)),
                globalBossCounts,
                cardProgress,
                characterRecords,
                relicProgress);

            return true;
        }
    }

    [Serializable]
    private sealed class LegacyProgressionSaveData
    {
        public int version;
        public int totalRunWins;
        public List<CardProgressRecord> cards;
        public List<CardProgressRecord> cardProgress;
        public List<string> defeatedBossIds;
        public List<CharacterProgressRecord> characterProgress;
        public List<RelicProgressRecord> relicProgress;

        public bool TryCreateSave(
            out GlobalProgressionSave save,
            out string error)
        {
            save = null;
            error = null;

            if (version != LegacyVersion)
            {
                error =
                    $"Unsupported legacy version {version}; expected " +
                    $"{LegacyVersion}.";
                return false;
            }

            List<CardProgressRecord> resolvedCards =
                cardProgress ?? cards;

            if (resolvedCards == null || characterProgress == null)
            {
                error = "Required legacy progression collections are missing.";
                return false;
            }

            if (resolvedCards.Exists(record => record == null) ||
                characterProgress.Exists(record => record == null) ||
                (relicProgress != null &&
                 relicProgress.Exists(record => record == null)))
            {
                error = "Legacy progression contains a null record.";
                return false;
            }

            Dictionary<string, int> globalBossCounts = new();

            if (defeatedBossIds != null)
            {
                foreach (string bossId in defeatedBossIds)
                {
                    if (string.IsNullOrWhiteSpace(bossId))
                        continue;

                    globalBossCounts.TryAdd(bossId, 0);
                    globalBossCounts[bossId]++;
                }
            }

            foreach (CharacterProgressRecord record
                     in characterProgress)
            {
                record.EnsureCollectionsInitialized();
            }

            save = GlobalProgressionSave.FromPersistence(
                totalRunWins,
                SumTotalGames(characterProgress),
                SumElitesDefeated(characterProgress),
                globalBossCounts,
                resolvedCards,
                characterProgress,
                relicProgress ?? new List<RelicProgressRecord>());

            return true;
        }
    }

    public LocalJsonProgressionStore(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException(
                "A progression directory is required.",
                nameof(directoryPath));
        }

        primaryPath = Path.Combine(directoryPath, SaveFileName);
        backupPath = primaryPath + ".bak";
        temporaryPath = primaryPath + ".tmp";
    }

    public ProgressionLoadResult Load()
    {
        DeleteIfExists(temporaryPath);

        if (TryLoadFile(
                primaryPath,
                out GlobalProgressionSave primarySave,
                out bool primaryRequiresSave,
                out string primaryError))
        {
            return new ProgressionLoadResult(
                primarySave,
                ProgressionLoadSource.Primary,
                primaryRequiresSave);
        }

        if (File.Exists(primaryPath))
        {
            Debug.LogWarning(
                $"Primary progression save is invalid: {primaryError}");
            ArchiveCorruptFile(primaryPath);
        }

        if (TryLoadFile(
                backupPath,
                out GlobalProgressionSave backupSave,
                out bool backupRequiresSave,
                out string backupError))
        {
            RestoreBackup();

            return new ProgressionLoadResult(
                backupSave,
                ProgressionLoadSource.Backup,
                backupRequiresSave);
        }

        if (File.Exists(backupPath))
        {
            Debug.LogWarning(
                $"Backup progression save is invalid: {backupError}");
            ArchiveCorruptFile(backupPath);
        }

        return new ProgressionLoadResult(
            new GlobalProgressionSave(),
            ProgressionLoadSource.NewSave,
            true);
    }

    public void Save(GlobalProgressionSave save)
    {
        if (save == null)
            throw new ArgumentNullException(nameof(save));

        save.EnsureCollectionsInitialized();

        if (save.Version != GlobalProgressionSave.CurrentVersion)
        {
            throw new InvalidOperationException(
                $"Cannot save progression version {save.Version}; " +
                $"expected {GlobalProgressionSave.CurrentVersion}.");
        }

        string directoryPath = Path.GetDirectoryName(primaryPath);
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new InvalidOperationException(
                "The progression save path has no directory.");
        }

        Directory.CreateDirectory(directoryPath);

        ProgressionSaveData data =
            ProgressionSaveData.FromSave(save);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(temporaryPath, json);

        if (!TryLoadFile(
                temporaryPath,
                out _,
                out bool temporaryRequiresSave,
                out string validationError) ||
            temporaryRequiresSave)
        {
            DeleteIfExists(temporaryPath);

            throw new InvalidDataException(
                "Temporary progression save failed validation: " +
                validationError);
        }

        try
        {
            if (File.Exists(primaryPath))
            {
                if (TryLoadFile(primaryPath, out _, out _, out _))
                {
                    ReplacePrimaryWithTemporary();
                }
                else
                {
                    ArchiveCorruptFile(primaryPath);
                    File.Move(temporaryPath, primaryPath);
                }
            }
            else
            {
                File.Move(temporaryPath, primaryPath);
            }
        }
        catch
        {
            DeleteIfExists(temporaryPath);
            throw;
        }
    }

    public void Reset()
    {
        DeleteIfExists(temporaryPath);
        DeleteIfExists(backupPath);
        DeleteIfExists(primaryPath);
    }

    private void ReplacePrimaryWithTemporary()
    {
        try
        {
            File.Replace(
                temporaryPath,
                primaryPath,
                backupPath,
                true);
        }
        catch (PlatformNotSupportedException)
        {
            File.Copy(primaryPath, backupPath, true);
            File.Copy(temporaryPath, primaryPath, true);
            File.Delete(temporaryPath);
        }
    }

    private void RestoreBackup()
    {
        File.Copy(backupPath, temporaryPath, true);

        if (!TryLoadFile(
                temporaryPath,
                out _,
                out _,
                out string error))
        {
            DeleteIfExists(temporaryPath);

            throw new InvalidDataException(
                $"Backup restoration failed validation: {error}");
        }

        if (File.Exists(primaryPath))
            ArchiveCorruptFile(primaryPath);

        File.Move(temporaryPath, primaryPath);
    }

    private static bool TryLoadFile(
        string path,
        out GlobalProgressionSave save,
        out bool requiresSave,
        out string error)
    {
        save = null;
        requiresSave = false;
        error = null;

        if (!File.Exists(path))
        {
            error = "File does not exist.";
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "File is empty.";
                return false;
            }

            SaveVersionShape versionShape = new();
            JsonUtility.FromJsonOverwrite(json, versionShape);

            if (versionShape.version ==
                GlobalProgressionSave.CurrentVersion)
            {
                ProgressionSaveData data =
                    JsonUtility.FromJson<ProgressionSaveData>(json);

                if (data == null)
                {
                    error = "JSON did not contain progression data.";
                    return false;
                }

                return data.TryCreateSave(out save, out error);
            }

            if (versionShape.version == LegacyVersion)
            {
                LegacyProgressionSaveData data =
                    JsonUtility.FromJson<LegacyProgressionSaveData>(
                        json);

                if (data == null)
                {
                    error = "JSON did not contain legacy progression data.";
                    return false;
                }

                if (!data.TryCreateSave(out save, out error))
                    return false;

                requiresSave = true;
                return true;
            }

            error =
                $"Unsupported version {versionShape.version}; expected " +
                $"{GlobalProgressionSave.CurrentVersion}.";
            return false;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            save = null;
            requiresSave = false;
            return false;
        }
    }

    private static int SumTotalGames(
        List<CharacterProgressRecord> records)
    {
        int total = 0;

        foreach (CharacterProgressRecord record in records)
        {
            if (record != null)
                total += record.TotalGames;
        }

        return total;
    }

    private static int SumElitesDefeated(
        List<CharacterProgressRecord> records)
    {
        int total = 0;

        foreach (CharacterProgressRecord record in records)
        {
            if (record != null)
                total += record.EliteNodeCompleted;
        }

        return total;
    }

    private static List<BossDefeatData> CreateBossDefeatData(
        IReadOnlyDictionary<string, int> bossCounts)
    {
        List<BossDefeatData> data = new();

        if (bossCounts == null)
            return data;

        foreach (KeyValuePair<string, int> bossCount in bossCounts)
        {
            data.Add(
                new BossDefeatData(
                    bossCount.Key,
                    bossCount.Value));
        }

        data.Sort(
            (left, right) =>
                string.CompareOrdinal(left.bossId, right.bossId));

        return data;
    }

    private static bool TryCreateBossCounts(
        List<BossDefeatData> data,
        out Dictionary<string, int> bossCounts,
        out string error)
    {
        bossCounts = new Dictionary<string, int>();
        error = null;

        if (data == null)
        {
            error = "A defeated-boss collection is missing.";
            return false;
        }

        foreach (BossDefeatData entry in data)
        {
            if (entry == null ||
                string.IsNullOrWhiteSpace(entry.bossId))
            {
                error = "A defeated-boss entry has no persistent ID.";
                return false;
            }

            if (entry.count < 0)
            {
                error =
                    $"Defeated-boss count for '{entry.bossId}' " +
                    "cannot be negative.";
                return false;
            }

            if (!bossCounts.TryAdd(entry.bossId, entry.count))
            {
                error =
                    $"Duplicate defeated-boss ID '{entry.bossId}'.";
                return false;
            }
        }

        return true;
    }

    private static void ArchiveCorruptFile(string path)
    {
        if (!File.Exists(path))
            return;

        string timestamp =
            DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        string archivePath =
            $"{path}.corrupt-{timestamp}";
        int suffix = 1;

        while (File.Exists(archivePath))
        {
            archivePath =
                $"{path}.corrupt-{timestamp}-{suffix}";
            suffix++;
        }

        File.Move(path, archivePath);

        Debug.LogWarning(
            $"Archived invalid progression save to '{archivePath}'.");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
