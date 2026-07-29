using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

internal sealed class LocalJsonProgressionStore : IProgressionStore
{
    internal const string SaveFileName = "global_progression.json";

    private readonly string primaryPath;
    private readonly string backupPath;
    private readonly string temporaryPath;

    public string Location => primaryPath;

    internal string BackupPath => backupPath;
    internal string TemporaryPath => temporaryPath;

    [Serializable]
    private sealed class SaveShape
    {
        public int version;
        public List<CardProgressRecord> cards;
        public List<string> defeatedBossIds;
        public List<CharacterProgressRecord> characterProgress;

        public SaveShape(string missingFieldMarker)
        {
            version = int.MinValue;
            cards = new List<CardProgressRecord>
            {
                new(missingFieldMarker, false)
            };
            defeatedBossIds = new List<string>
            {
                missingFieldMarker
            };
            characterProgress = new List<CharacterProgressRecord>
            {
                new(
                    missingFieldMarker,
                    default,
                    false)
            };
        }

        public bool HasRequiredCollections(string missingFieldMarker)
        {
            return cards != null &&
                   defeatedBossIds != null &&
                   characterProgress != null &&
                   !cards.Exists(
                       record =>
                           record?.CardId == missingFieldMarker) &&
                   !defeatedBossIds.Contains(missingFieldMarker) &&
                   !characterProgress.Exists(
                       record =>
                           record?.CharacterId == missingFieldMarker);
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
                out string primaryError))
        {
            return new ProgressionLoadResult(
                primarySave,
                ProgressionLoadSource.Primary,
                false);
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
                out string backupError))
        {
            RestoreBackup();

            return new ProgressionLoadResult(
                backupSave,
                ProgressionLoadSource.Backup,
                false);
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

        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(temporaryPath, json);

        if (!TryLoadFile(
                temporaryPath,
                out _,
                out string validationError))
        {
            DeleteIfExists(temporaryPath);

            throw new InvalidDataException(
                $"Temporary progression save failed validation: " +
                $"{validationError}");
        }

        try
        {
            if (File.Exists(primaryPath))
            {
                if (TryLoadFile(primaryPath, out _, out _))
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

        if (!TryLoadFile(temporaryPath, out _, out string error))
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
        out string error)
    {
        save = null;
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

            string missingFieldMarker =
                Guid.NewGuid().ToString("N");
            SaveShape shape = new(missingFieldMarker);
            JsonUtility.FromJsonOverwrite(json, shape);

            if (shape.version != GlobalProgressionSave.CurrentVersion)
            {
                error =
                    $"Unsupported version {shape.version}; expected " +
                    $"{GlobalProgressionSave.CurrentVersion}.";
                return false;
            }

            if (!shape.HasRequiredCollections(missingFieldMarker))
            {
                error = "Required progression collections are missing.";
                return false;
            }

            save = JsonUtility.FromJson<GlobalProgressionSave>(json);
            if (save == null)
            {
                error = "JSON did not contain progression data.";
                return false;
            }

            if (save.Version != GlobalProgressionSave.CurrentVersion)
            {
                error =
                    $"Unsupported version {save.Version}; expected " +
                    $"{GlobalProgressionSave.CurrentVersion}.";
                save = null;
                return false;
            }

            save.EnsureCollectionsInitialized();
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            save = null;
            return false;
        }
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
