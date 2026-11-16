using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Loads the combat scene and checks that its components still resolve to
/// scripts and still point at each other. This guards the scene-side classes
/// (<see cref="CombatPresenter"/>, <see cref="UnitSpawner"/>,
/// <see cref="CardPilePresenter"/>) against losing their serialized links
/// when code is moved or renamed.
/// </summary>
public sealed class CombatSceneWiringTests
{
    private const string ScenePath =
        "Assets/Scenes/CardGameScene/CombatNodeScene.unity";

    private Scene scene;
    private bool openedByTest;

    [OneTimeSetUp]
    public void OpenScene()
    {
        scene = SceneManager.GetSceneByPath(ScenePath);

        if (!scene.IsValid() || !scene.isLoaded)
        {
            scene = EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Additive);
            openedByTest = true;
        }
    }

    [OneTimeTearDown]
    public void CloseScene()
    {
        // Leave a scene the user already had open alone.
        if (openedByTest && scene.IsValid())
            EditorSceneManager.CloseScene(scene, true);
    }

    [Test]
    public void SceneHasNoMissingScripts()
    {
        List<string> offenders = new List<string>();

        foreach (Transform transform in AllTransforms())
        {
            int missing =
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    transform.gameObject);

            if (missing > 0)
                offenders.Add($"{PathOf(transform)} ({missing})");
        }

        Assert.That(
            offenders,
            Is.Empty,
            "Objects with missing scripts: " + string.Join(", ", offenders));
    }

    [Test]
    public void CombatPresenterIsWired()
    {
        CombatPresenter presenter = Single<CombatPresenter>();

        AssertAssigned(presenter, "unitSpawner");
        AssertAssigned(presenter, "playerHandView");
        AssertAssigned(presenter, "endTurnButton");
    }

    [Test]
    public void UnitSpawnerIsWired()
    {
        UnitSpawner spawner = Single<UnitSpawner>();

        AssertAssigned(spawner, "playerPrefab");
        AssertAssigned(spawner, "enemyPrefab");
        AssertAssigned(spawner, "playerSpawnPoint");

        Transform[] enemySpawnPoints =
            (Transform[])ReadField(spawner, "enemySpawnPoints");

        Assert.That(enemySpawnPoints, Is.Not.Null.And.Not.Empty);
        Assert.That(enemySpawnPoints, Has.None.Null);
    }

    [Test]
    public void CombatPresenterUsesTheScenesUnitSpawner()
    {
        Assert.That(
            ReadField(Single<CombatPresenter>(), "unitSpawner"),
            Is.SameAs(Single<UnitSpawner>()));
    }

    [Test]
    public void CardPilePresenterIsWired()
    {
        CardPilePresenter piles = Single<CardPilePresenter>();

        AssertAssigned(piles, "drawPileUI");
        AssertAssigned(piles, "discardPileUI");
    }

    [Test]
    public void MapGeneratorIsWired()
    {
        MapGenerator generator = Single<MapGenerator>();

        AssertAssigned(generator, "mapContainer");
        AssertAssigned(generator, "nodePrefab");
        AssertAssigned(generator, "linePrefab");

        Assert.That(generator.Settings.IsValid, Is.True);
        Assert.That(generator.NodeConfigLibrary != null, Is.True);
    }

    [Test]
    public void RewardScreenIsWired()
    {
        NodeRewardManager rewards = Single<NodeRewardManager>();

        foreach (string pool in new[]
                 {
                     "ironcladReward", "silentReward", "necroReward",
                     "regentReward", "neutralReward",
                 })
        {
            AssertAssigned(rewards, pool);
        }

        AssertAssigned(rewards, "_winPanel");
        AssertAssigned(rewards, "_losePanel");
        AssertAssigned(rewards, "cardRewardPrefab");
    }

    [Test]
    public void RunManagerPrefabHasWhatARunNeeds()
    {
        GameObject prefab = Resources.Load<GameObject>("RunManager");
        Assert.That(prefab != null, Is.True, "Resources/RunManager is missing.");

        RunManager runManager = prefab.GetComponent<RunManager>();
        Assert.That(runManager != null, Is.True);

        AssertAssigned(runManager, "nodeConfigLibrary");
        Assert.That(
            runManager.CardRewardAmountAfterCombat,
            Is.GreaterThan(0));
    }

    [Test]
    public void PlayCardAreasPointAtTheCombatPresenter()
    {
        CombatPresenter presenter = Single<CombatPresenter>();
        List<PlayCardArea> areas = All<PlayCardArea>();

        Assert.That(areas, Is.Not.Empty);

        foreach (PlayCardArea area in areas)
        {
            Assert.That(
                ReadField(area, "combatPresenter"),
                Is.SameAs(presenter),
                PathOf(area.transform));
        }
    }

    [Test]
    public void UnitPrefabsHaveNoMissingScripts()
    {
        UnitSpawner spawner = Single<UnitSpawner>();

        foreach (string fieldName in new[] { "playerPrefab", "enemyPrefab" })
        {
            UnitView prefab = (UnitView)ReadField(spawner, fieldName);
            Assert.That(prefab != null, Is.True, fieldName);

            foreach (Transform transform in
                     prefab.GetComponentsInChildren<Transform>(true))
            {
                Assert.That(
                    GameObjectUtility
                        .GetMonoBehavioursWithMissingScriptCount(
                            transform.gameObject),
                    Is.EqualTo(0),
                    $"{fieldName}: {PathOf(transform)}");
            }
        }
    }

    private IEnumerable<Transform> AllTransforms()
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in
                     root.GetComponentsInChildren<Transform>(true))
            {
                yield return transform;
            }
        }
    }

    private List<T> All<T>()
        where T : Component
    {
        List<T> found = new List<T>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            found.AddRange(root.GetComponentsInChildren<T>(true));
        }

        return found;
    }

    private T Single<T>()
        where T : Component
    {
        List<T> found = All<T>();

        Assert.That(
            found.Count,
            Is.EqualTo(1),
            $"Expected exactly one {typeof(T).Name} in the combat scene.");

        return found[0];
    }

    private static void AssertAssigned(Component owner, string fieldName)
    {
        // Compare as a Unity object so a destroyed or unresolved reference
        // counts as missing.
        Object value = ReadField(owner, fieldName) as Object;

        Assert.That(
            value != null,
            Is.True,
            $"{owner.GetType().Name}.{fieldName} is not assigned.");
    }

    private static object ReadField(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public);

        Assert.That(
            field,
            Is.Not.Null,
            $"{target.GetType().Name} has no field '{fieldName}'.");

        return field.GetValue(target);
    }

    private static string PathOf(Transform transform)
    {
        string path = transform.name;

        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }
}
