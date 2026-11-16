using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Builds a real combat with no scene, no views and no frames, for tests.
///
/// The combat is a plain <see cref="CombatManager"/> given a seeded
/// <see cref="CombatContext"/>, so nothing global is involved. With nothing
/// driving its queue, every action resolves inside the call that queued it,
/// and the combat is played only through
/// <see cref="CombatManager.PendingDecision"/> and <see cref="CombatManager.Submit"/>.
///
/// Reflection is used only to fill in data assets that are normally authored
/// in the editor.
/// </summary>
public sealed class CombatTestRig : IDisposable
{
    private readonly List<Object> created = new();

    public CombatManager Combat { get; }
    public CardPileManager Piles { get; }
    public BasePlayerUnitInstance Player { get; private set; }

    public CombatActionQueue Queue => Combat.ActionQueue;
    public CombatSystem CombatSystem => Combat.CombatSystem;

    /// <param name="seed">Seeds every random choice, so a combat repeats.</param>
    /// <param name="progress">Receives progression reports; null records nothing.</param>
    public CombatTestRig(int seed = 12345, ICombatProgressRecorder progress = null)
    {
        Combat = new CombatManager(
            new CombatContext(new System.Random(seed), progress));
        Piles = new CardPileManager();

        Queue.LogResolvedActions = false;
    }

    public void Dispose()
    {
        foreach (Object item in created)
        {
            if (item != null)
                Object.DestroyImmediate(item);
        }

        created.Clear();
    }

    /// <summary>Begins the battle: opening state, intents and the first draw.</summary>
    public void StartCombat()
    {
        Combat.BeginBattle();
    }

    public BasePlayerUnitInstance AddPlayer(
        IEnumerable<CardData> deck,
        int maxHealth = 50,
        int maxEnergy = 3,
        int drawAmount = 2)
    {
        DeckData deckData = CreateAsset<DeckData>();
        SetField(deckData, "cardDataList", new List<CardData>(deck));

        PlayerUnitData data = CreateAsset<PlayerUnitData>();
        SetField(data, "unitName", "Player");
        SetField(data, "unitType", UnitType.Player);
        SetField(data, "maxHealth", maxHealth);
        SetField(data, "maxEnergy", maxEnergy);
        SetField(data, "drawAmount", drawAmount);
        SetField(data, "startingDeckData", deckData);

        BasePlayerUnitInstance player = new(data);
        Combat.AddPlayer(player, Piles);

        Player = player;
        return player;
    }

    public BaseEnemyUnitInstance AddEnemy(
        string unitName,
        int maxHealth,
        params EnemyIntentData[] intents)
    {
        EnemyUnitData data = CreateAsset<EnemyUnitData>();
        SetField(data, "unitName", unitName);
        SetField(data, "unitType", UnitType.Enemy);
        SetField(data, "maxHealth", maxHealth);
        SetField(
            data,
            "availableIntents",
            new List<EnemyIntentData>(intents));

        return Combat.AddEnemy(data);
    }

    public CardData CreateCard(
        string cardName,
        int cost = 1,
        TargetType targetType = TargetType.Enemy,
        CardType cardType = CardType.Attack,
        params CardEffectData[] effects)
    {
        CardData card = CreateAsset<CardData>();
        SetField(card, "id", "test-" + cardName);
        SetField(card, "cardName", cardName);
        SetField(card, "cardCost", cost);
        SetField(card, "cardType", cardType);
        SetField(card, "targetType", targetType);
        SetField(card, "cardEffects", new List<CardEffectData>(effects));
        return card;
    }

    /// <summary>An on-play effect dealing damage to the card's targets.</summary>
    public static CardEffectData DamageEffect(int amount, int attacks = 1)
    {
        DmgCardEffectConfig config = new();
        SetField(config, "amount", new DynamicValue { baseValue = amount });
        SetField(config, "numberOfAttacks", attacks);

        CardEffectData effect = new();
        SetField(effect, "cardEffect", new DmgCardEffect());
        effect.SetConfig(config);
        return effect;
    }

    /// <summary>An on-play effect asking the player to discard from hand.</summary>
    public static CardEffectData SelectDiscardEffect(
        int count,
        bool isOptional = false)
    {
        SelectDiscardCardEffectConfig config = new();
        SetField(config, "discardMode", DiscardMode.SelectHandCard);
        SetField(config, "count", count);
        SetField(config, "isOptional", isOptional);
        SetField(config, "excludeSelf", true);

        CardEffectData effect = new();
        SetField(effect, "cardEffect", new SelectDiscardCardEffect());
        effect.SetConfig(config);
        return effect;
    }

    /// <summary>An enemy intent hitting the player.</summary>
    public static EnemyIntentData DamageIntent(int amount, int attacks = 1)
    {
        EnemyDmgIntentConfig config = new();
        SetField(config, "targetType", TargetType.Player);
        SetField(config, "amount", amount);
        SetField(config, "numberOfAttacks", attacks);

        EnemyIntentData intent = new();
        SetField(intent, "enemyIntent", new EnemyDmgIntent());
        intent.SetConfig(config);
        return intent;
    }

    public T CreateAsset<T>()
        where T : ScriptableObject
    {
        return Track(ScriptableObject.CreateInstance<T>());
    }

    /// <summary>A scene object for a view component, cleaned up with the rig.</summary>
    public GameObject CreateGameObject(string objectName)
    {
        return Track(new GameObject(objectName));
    }

    private T Track<T>(T item)
        where T : Object
    {
        created.Add(item);
        return item;
    }

    public static void SetField(
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
