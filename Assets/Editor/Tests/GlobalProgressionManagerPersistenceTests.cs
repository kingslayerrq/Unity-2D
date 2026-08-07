using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class GlobalProgressionManagerPersistenceTests
{
    private readonly List<Object> createdAssets = new();

    private string testDirectory;
    private GameObject managerObject;

    [SetUp]
    public void SetUp()
    {
        testDirectory = Path.Combine(
            Path.GetTempPath(),
            "2D-progression-manager-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        DestroyManager();

        foreach (Object asset in createdAssets)
        {
            if (asset != null)
                Object.DestroyImmediate(asset);
        }

        createdAssets.Clear();

        if (Directory.Exists(testDirectory))
            Directory.Delete(testDirectory, true);
    }

    [Test]
    public void CompletedRunUnlocksNextCharacterAfterReload()
    {
        PlayerUnitData startingCharacter =
            CreateCharacter(
                "character-starting",
                "Starting Character",
                unlockedByDefault: true);

        PlayerUnitData nextCharacter =
            CreateCharacter(
                "character-next",
                "Next Character",
                unlockedByDefault: false);

        CharacterCatalog characterCatalog =
            CreateCharacterCatalog(
                startingCharacter,
                nextCharacter);

        CharacterUnlockCatalog unlockCatalog =
            CreateUnlockCatalog(
                startingCharacter,
                nextCharacter);

        LocalJsonProgressionStore store =
            new(testDirectory);

        GlobalProgressionManager progression =
            CreateManager(
                store,
                CreateCardCatalog(),
                characterCatalog,
                unlockCatalog);

        Assert.That(
            progression.IsUnlocked(nextCharacter),
            Is.False);

        progression.RecordRunCompleted(
            startingCharacter,
            won: false);

        Assert.That(
            progression.GetTotalGames(
                startingCharacter),
            Is.EqualTo(1));
        Assert.That(
            progression.IsUnlocked(nextCharacter),
            Is.True);

        DestroyManager();

        progression =
            CreateManager(
                new LocalJsonProgressionStore(testDirectory),
                CreateCardCatalog(),
                characterCatalog);

        Assert.That(
            progression.GetTotalGames(
                startingCharacter),
            Is.EqualTo(1));
        Assert.That(
            progression.IsUnlocked(nextCharacter),
            Is.True);
    }

    [Test]
    public void DirtyCardCounterPersistsAtCheckpointAndDestruction()
    {
        CardData card = CreateCard("card-1");
        CardCatalog cardCatalog = CreateCardCatalog(card);
        CharacterCatalog characterCatalog =
            CreateCharacterCatalog();
        LocalJsonProgressionStore store =
            new(testDirectory);

        GlobalProgressionManager progression =
            CreateManager(
                store,
                cardCatalog,
                characterCatalog);

        progression.RecordCardPlayed(
            new CardInstance(card));

        Assert.That(
            LoadCardRecord(store, card).TimesPlayed,
            Is.EqualTo(0));

        progression.SaveCheckpoint();

        Assert.That(
            LoadCardRecord(store, card).TimesPlayed,
            Is.EqualTo(1));

        progression.RecordCardPlayed(
            new CardInstance(card));
        DestroyManager();

        Assert.That(
            LoadCardRecord(store, card).TimesPlayed,
            Is.EqualTo(2));
    }

    [Test]
    public void StoreRoundTripPreservesDictionaryBackedProgression()
    {
        LocalJsonProgressionStore store =
            new(testDirectory);
        GlobalProgressionSave save = new();

        for (int i = 0; i < 3; i++)
        {
            save.RecordGame(true);
        }
        save.RecordBossDefeated("global-boss");
        save.RecordBossDefeated("global-boss");

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
        character.RecordBoss("character-boss");
        character.RecordBoss("character-boss");
        save.CharacterProgress.Add(character);

        RelicProgressRecord relic =
            new(
                "relic-1",
                isUnlockedByDefault: false,
                isDiscoveredByDefault: false);
        relic.Unlock();
        relic.Discover();
        save.RelicProgress.Add(relic);

        store.Save(save);
        ProgressionLoadResult result = store.Load();

        Assert.That(
            result.Source,
            Is.EqualTo(ProgressionLoadSource.Primary));
        Assert.That(result.RequiresSave, Is.False);
        Assert.That(result.Save.TotalRunWins, Is.EqualTo(3));
        Assert.That(
            result.Save.DefeatedBosses["global-boss"],
            Is.EqualTo(2));

        CardProgressRecord loadedCard =
            result.Save.CardProgress[0];
        Assert.That(loadedCard.CardId, Is.EqualTo("card-1"));
        Assert.That(loadedCard.IsUnlocked, Is.True);
        Assert.That(loadedCard.TimesPlayed, Is.EqualTo(1));

        CharacterProgressRecord loadedCharacter =
            result.Save.CharacterProgress[0];
        Assert.That(loadedCharacter.Wins, Is.EqualTo(1));
        Assert.That(loadedCharacter.TotalGames, Is.EqualTo(1));
        Assert.That(loadedCharacter.BossNodeCompleted, Is.EqualTo(2));
        Assert.That(
            loadedCharacter.DefeatedBosses["character-boss"],
            Is.EqualTo(2));

        RelicProgressRecord loadedRelic =
            result.Save.RelicProgress[0];
        Assert.That(loadedRelic.RelicId, Is.EqualTo("relic-1"));
        Assert.That(loadedRelic.IsUnlocked, Is.True);
        Assert.That(loadedRelic.IsDiscovered, Is.True);
    }

    [Test]
    public void VersionOneSaveLoadsAndIsRewrittenAsVersionTwo()
    {
        LocalJsonProgressionStore store =
            new(testDirectory);

        File.WriteAllText(
            store.Location,
            "{\"version\":1,\"totalRunWins\":4," +
            "\"cardProgress\":[{\"cardId\":\"legacy-card\"," +
            "\"isDiscovered\":true,\"isUnlocked\":true," +
            "\"timesPlayed\":3}]," +
            "\"defeatedBossIds\":[\"legacy-boss\"," +
            "\"legacy-boss\"],\"characterProgress\":[]," +
            "\"relicProgress\":[]}");

        ProgressionLoadResult migrated = store.Load();

        Assert.That(
            migrated.Source,
            Is.EqualTo(ProgressionLoadSource.Primary));
        Assert.That(migrated.RequiresSave, Is.True);
        Assert.That(
            migrated.Save.Version,
            Is.EqualTo(GlobalProgressionSave.CurrentVersion));
        Assert.That(migrated.Save.TotalRunWins, Is.EqualTo(4));
        Assert.That(
            migrated.Save.DefeatedBosses["legacy-boss"],
            Is.EqualTo(2));
        Assert.That(
            migrated.Save.CardProgress[0].TimesPlayed,
            Is.EqualTo(3));

        store.Save(migrated.Save);
        ProgressionLoadResult reloaded = store.Load();

        Assert.That(reloaded.RequiresSave, Is.False);
        StringAssert.Contains(
            "\"version\": 2",
            File.ReadAllText(store.Location));
    }

    private GlobalProgressionManager CreateManager(
        IProgressionStore store,
        CardCatalog cardCatalog,
        CharacterCatalog characterCatalog,
        CharacterUnlockCatalog unlockCatalog = null)
    {
        managerObject =
            new GameObject("Progression Manager Test");
        managerObject.SetActive(false);

        GlobalProgressionManager progression =
            managerObject.AddComponent<GlobalProgressionManager>();

        SetField(progression, "progressionStore", store);
        SetField(progression, "cardCatalog", cardCatalog);
        SetField(
            progression,
            "characterCatalog",
            characterCatalog);
        SetField(
            progression,
            "relicCatalog",
            CreateRelicCatalog());
        SetField(
            progression,
            "persistBetweenScenes",
            false);

        CharacterUnlockEvaluator evaluator = null;

        if (unlockCatalog != null)
        {
            evaluator =
                managerObject
                    .AddComponent<CharacterUnlockEvaluator>();

            SetField(
                evaluator,
                "unlockCatalog",
                unlockCatalog);
            SetField(
                evaluator,
                "progression",
                progression);
        }

        managerObject.SetActive(true);

        // EditMode does not automatically invoke MonoBehaviour lifecycle
        // methods for objects created directly by a test.
        InvokeMethod(progression, "Awake");

        if (evaluator != null)
            InvokeMethod(evaluator, "OnEnable");

        return progression;
    }

    private void DestroyManager()
    {
        if (managerObject == null)
            return;

        GlobalProgressionManager progression =
            managerObject.GetComponent<GlobalProgressionManager>();

        if (progression != null)
            InvokeMethod(progression, "OnDestroy");

        Object.DestroyImmediate(managerObject);
        managerObject = null;
    }

    private CardCatalog CreateCardCatalog(
        params CardData[] cards)
    {
        CardCatalog catalog = CreateAsset<CardCatalog>();
        SetField(
            catalog,
            "cards",
            new List<CardData>(cards));
        return catalog;
    }

    private CharacterCatalog CreateCharacterCatalog(
        params PlayerUnitData[] characters)
    {
        CharacterCatalog catalog =
            CreateAsset<CharacterCatalog>();

        SetField(
            catalog,
            "characters",
            new List<PlayerUnitData>(characters));

        return catalog;
    }

    private RelicCatalog CreateRelicCatalog()
    {
        RelicCatalog catalog = CreateAsset<RelicCatalog>();
        SetField(
            catalog,
            "relics",
            new List<RelicData>());
        return catalog;
    }

    private CharacterUnlockCatalog CreateUnlockCatalog(
        PlayerUnitData requiredCharacter,
        PlayerUnitData unlockedCharacter)
    {
        CharacterTotalGamesUnlockCondition condition =
            CreateAsset<CharacterTotalGamesUnlockCondition>();
        SetField(
            condition,
            "requiredCharacter",
            requiredCharacter);

        CharacterUnlockDefinition definition =
            CreateAsset<CharacterUnlockDefinition>();
        SetField(
            definition,
            "character",
            unlockedCharacter);
        SetField(definition, "condition", condition);

        CharacterUnlockCatalog catalog =
            CreateAsset<CharacterUnlockCatalog>();
        SetField(
            catalog,
            "definitions",
            new List<CharacterUnlockDefinition>
            {
                definition
            });

        return catalog;
    }

    private PlayerUnitData CreateCharacter(
        string characterId,
        string unitName,
        bool unlockedByDefault)
    {
        PlayerUnitData character =
            CreateAsset<PlayerUnitData>();

        SetField(character, "characterID", characterId);
        SetField(
            character,
            "isUnlockedByDefault",
            unlockedByDefault);
        SetField(character, "unitName", unitName);

        return character;
    }

    private CardData CreateCard(string cardId)
    {
        CardData card = CreateAsset<CardData>();
        SetField(card, "id", cardId);
        SetField(card, "isUnlockedByDefault", true);
        return card;
    }

    private T CreateAsset<T>()
        where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        createdAssets.Add(asset);
        return asset;
    }

    private static CardProgressRecord LoadCardRecord(
        IProgressionStore store,
        CardData card)
    {
        ProgressionLoadResult result = store.Load();

        return result.Save.CardProgress.Find(
            record => record.CardId == card.ID);
    }

    private static void SetField(
        object target,
        string fieldName,
        object value)
    {
        Type type = target.GetType();

        while (type != null)
        {
            FieldInfo field = type.GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly);

            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            type = type.BaseType;
        }

        throw new MissingFieldException(
            target.GetType().FullName,
            fieldName);
    }

    private static void InvokeMethod(
        object target,
        string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public);

        if (method == null)
        {
            throw new MissingMethodException(
                target.GetType().FullName,
                methodName);
        }

        method.Invoke(target, null);
    }
}
