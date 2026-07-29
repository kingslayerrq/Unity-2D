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
            progression.GetCharacterTotalGames(
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
            progression.GetCharacterTotalGames(
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
            "persistBetweenScenes",
            false);

        if (unlockCatalog != null)
        {
            CharacterUnlockEvaluator evaluator =
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
        return progression;
    }

    private void DestroyManager()
    {
        if (managerObject == null)
            return;

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

    private CharacterUnlockCatalog CreateUnlockCatalog(
        PlayerUnitData requiredCharacter,
        PlayerUnitData unlockedCharacter)
    {
        CharacterGamePlayedUnlockCondition condition =
            CreateAsset<CharacterGamePlayedUnlockCondition>();
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

        return result.Save.Cards.Find(
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
}
