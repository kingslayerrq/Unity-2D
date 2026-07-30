using System;
using System.IO;
using NUnit.Framework;

public sealed class LocalJsonProgressionStoreTests
{
    private string testDirectory;
    private LocalJsonProgressionStore store;

    [SetUp]
    public void SetUp()
    {
        testDirectory = Path.Combine(
            Path.GetTempPath(),
            "2D-progression-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(testDirectory);
        store = new LocalJsonProgressionStore(testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(testDirectory))
            Directory.Delete(testDirectory, true);
    }

    [Test]
    public void LoadWithoutExistingFileCreatesNewSave()
    {
        ProgressionLoadResult result = store.Load();

        Assert.That(
            result.Source,
            Is.EqualTo(ProgressionLoadSource.NewSave));
        Assert.That(result.RequiresSave, Is.True);
        Assert.That(result.Save, Is.Not.Null);
        Assert.That(
            result.Save.Version,
            Is.EqualTo(GlobalProgressionSave.CurrentVersion));
    }

    [Test]
    public void SaveAndLoadPreservesProgressionData()
    {
        GlobalProgressionSave save = CreatePopulatedSave();

        store.Save(save);
        ProgressionLoadResult result = store.Load();

        Assert.That(
            result.Source,
            Is.EqualTo(ProgressionLoadSource.Primary));
        Assert.That(result.Save.TotalRunWins, Is.EqualTo(3));
        Assert.That(
            result.Save.DefeatedBossIds,
            Does.Contain("boss-1"));

        CardProgressRecord card = result.Save.CardProgress[0];
        Assert.That(card.CardId, Is.EqualTo("card-1"));
        Assert.That(card.IsUnlocked, Is.True);
        Assert.That(card.TimesPlayed, Is.EqualTo(1));

        CharacterProgressRecord character =
            result.Save.CharacterProgress[0];
        Assert.That(
            character.CharacterId,
            Is.EqualTo("character-1"));
        Assert.That(character.IsUnlocked, Is.True);
        Assert.That(character.Wins, Is.EqualTo(1));
        Assert.That(character.TotalGames, Is.EqualTo(1));
    }

    [Test]
    public void CorruptPrimaryRestoresLastKnownGoodBackup()
    {
        GlobalProgressionSave first = new();
        first.TotalRunWins = 1;
        store.Save(first);

        GlobalProgressionSave second = new();
        second.TotalRunWins = 2;
        store.Save(second);

        File.WriteAllText(store.Location, "{invalid-json");

        ProgressionLoadResult result = store.Load();

        Assert.That(
            result.Source,
            Is.EqualTo(ProgressionLoadSource.Backup));
        Assert.That(result.Save.TotalRunWins, Is.EqualTo(1));
        Assert.That(File.Exists(store.Location), Is.True);
        Assert.That(
            Directory.GetFiles(
                testDirectory,
                "*.corrupt-*").Length,
            Is.EqualTo(1));
    }

    [Test]
    public void CorruptPrimaryAndBackupCreateFreshSave()
    {
        File.WriteAllText(store.Location, "{invalid-primary");
        File.WriteAllText(
            store.BackupPath,
            "{invalid-backup");

        ProgressionLoadResult result = store.Load();

        Assert.That(
            result.Source,
            Is.EqualTo(ProgressionLoadSource.NewSave));
        Assert.That(result.RequiresSave, Is.True);
        Assert.That(
            Directory.GetFiles(
                testDirectory,
                "*.corrupt-*").Length,
            Is.EqualTo(2));
    }

    [Test]
    public void UnsupportedVersionIsArchivedBeforeCreatingFreshSave()
    {
        File.WriteAllText(
            store.Location,
            "{\"version\":2,\"cards\":[]," +
            "\"defeatedBossIds\":[],\"characterProgress\":[]}");

        ProgressionLoadResult result = store.Load();

        Assert.That(
            result.Source,
            Is.EqualTo(ProgressionLoadSource.NewSave));
        Assert.That(result.RequiresSave, Is.True);
        Assert.That(File.Exists(store.Location), Is.False);
        Assert.That(
            Directory.GetFiles(
                testDirectory,
                "*.corrupt-*").Length,
            Is.EqualTo(1));
    }

    [Test]
    public void MissingRequiredCollectionsAreArchived()
    {
        File.WriteAllText(
            store.Location,
            "{\"version\":1,\"totalRunWins\":4}");

        ProgressionLoadResult result = store.Load();

        Assert.That(
            result.Source,
            Is.EqualTo(ProgressionLoadSource.NewSave));
        Assert.That(result.RequiresSave, Is.True);
        Assert.That(
            Directory.GetFiles(
                testDirectory,
                "*.corrupt-*").Length,
            Is.EqualTo(1));
    }

    [Test]
    public void ResetDeletesOnlyActiveSaveArtifacts()
    {
        store.Save(new GlobalProgressionSave());
        store.Save(new GlobalProgressionSave());
        File.WriteAllText(store.TemporaryPath, "{}");

        string diagnosticPath =
            Path.Combine(
                testDirectory,
                "global_progression.json.corrupt-diagnostic");
        File.WriteAllText(diagnosticPath, "{}");

        store.Reset();

        Assert.That(File.Exists(store.Location), Is.False);
        Assert.That(File.Exists(store.BackupPath), Is.False);
        Assert.That(File.Exists(store.TemporaryPath), Is.False);
        Assert.That(File.Exists(diagnosticPath), Is.True);
    }

    [Test]
    public void DefaultUnlockReconciliationOnlyMovesForward()
    {
        CardProgressRecord record =
            new("card-1", unlockedByDefault: false);

        Assert.That(
            record.ReconcileDefaultUnlock(false),
            Is.False);
        Assert.That(
            record.ReconcileDefaultUnlock(true),
            Is.True);
        Assert.That(record.IsUnlocked, Is.True);
        Assert.That(record.IsDiscovered, Is.True);
        Assert.That(
            record.ReconcileDefaultUnlock(true),
            Is.False);
    }

    private static GlobalProgressionSave CreatePopulatedSave()
    {
        GlobalProgressionSave save = new();
        save.TotalRunWins = 3;
        save.DefeatedBossIds.Add("boss-1");

        CardProgressRecord card =
            new("card-1", unlockedByDefault: false);
        card.Unlock();
        card.RecordPlayed();
        save.CardProgress.Add(card);

        CharacterProgressRecord character =
            new(
                "character-1",
                CardClass.Ironclad,
                unlockedByDefault: false);
        character.Unlock();
        character.RecordGame(won: true);
        save.CharacterProgress.Add(character);

        return save;
    }
}
