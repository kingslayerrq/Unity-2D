using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Builds everything a whole run needs with no scene: a node library, a
/// character, a deck and reward pools. Runs made from it are played only
/// through decisions and commands, the way a bot plays.
///
/// Reflection is used only to fill in data assets that are normally authored
/// in the editor.
/// </summary>
public sealed class RunTestRig : IDisposable
{
    /// <summary>Records every progression report a run makes.</summary>
    public sealed class RecordingProgress : IRunProgressRecorder
    {
        public readonly List<CardInstance> Played = new List<CardInstance>();
        public readonly List<CardData> Offered = new List<CardData>();
        public readonly List<CardInstance> Obtained = new List<CardInstance>();
        public readonly List<NodeType> CompletedNodes = new List<NodeType>();
        public readonly List<bool> RunResults = new List<bool>();

        public void RecordCardPlayed(CardInstance card) => Played.Add(card);

        public void RecordCardsOffered(IReadOnlyList<CardData> cards) =>
            Offered.AddRange(cards);

        public void RecordCardObtained(CardInstance card) => Obtained.Add(card);

        public void RecordNodeCompleted(PlayerUnitData character, BaseNode node) =>
            CompletedNodes.Add(node.Type);

        public void RecordRunCompleted(PlayerUnitData character, bool won) =>
            RunResults.Add(won);
    }

    public static readonly MapSettings GameSizedMap = new MapSettings(7, 15);

    private readonly List<Object> created = new();

    public NodeConfigLibrary Library { get; }

    /// <summary>The cards rewards are drawn from, all of the Ironclad class.</summary>
    public List<CardData> RewardCards { get; } = new List<CardData>();

    /// <param name="enemyHealth">Health of every enemy in the library.</param>
    /// <param name="enemyDamage">Damage every enemy deals each turn.</param>
    public RunTestRig(int enemyHealth = 10, int enemyDamage = 1)
    {
        EnemyUnitData enemy = CreateAsset<EnemyUnitData>();
        CombatTestRig.SetField(enemy, "unitName", "Enemy");
        CombatTestRig.SetField(enemy, "unitType", UnitType.Enemy);
        CombatTestRig.SetField(enemy, "maxHealth", enemyHealth);
        CombatTestRig.SetField(enemy, "enemyId", "test-enemy");
        CombatTestRig.SetField(
            enemy,
            "availableIntents",
            new List<EnemyIntentData>
            {
                CombatTestRig.DamageIntent(enemyDamage),
            });

        Library = CreateAsset<NodeConfigLibrary>();
        Library.nonEliteCombatNodeConfigs = new List<NonEliteCombatNodeConfig>
        {
            CombatConfig<NonEliteCombatNodeConfig>(enemy),
        };
        Library.EliteCombatNodeConfigs = new List<EliteCombatNodeConfig>
        {
            CombatConfig<EliteCombatNodeConfig>(enemy),
        };
        Library.BossCombatNodeConfigs = new List<BossCombatNodeConfig>
        {
            CombatConfig<BossCombatNodeConfig>(enemy),
        };
        Library.MerchantNodeConfigs = new List<MerchantNodeConfig>
        {
            CreateAsset<MerchantNodeConfig>(),
        };
        Library.RestSiteNodeConfigs = new List<RestSiteNodeConfig>
        {
            CreateAsset<RestSiteNodeConfig>(),
        };
        Library.TreasureNodeConfigs = new List<TreasureNodeConfig>
        {
            CreateAsset<TreasureNodeConfig>(),
        };
        Library.EventNodeConfigs = new List<EventNodeConfig>
        {
            CreateAsset<EventNodeConfig>(),
        };
        Library.UnknownNodeConfig = CreateAsset<UnknownNodeConfig>();

        CardRarity[] rarities =
        {
            CardRarity.Common, CardRarity.Common, CardRarity.Common,
            CardRarity.Uncommon, CardRarity.Uncommon, CardRarity.Rare,
        };

        for (int i = 0; i < rarities.Length; i++)
        {
            RewardCards.Add(
                CreateCard("Reward " + i, 1, 50, rarities[i], CardClass.Ironclad));
        }
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

    /// <summary>A single-enemy attack card.</summary>
    public CardData CreateCard(
        string cardName,
        int cost,
        int damage,
        CardRarity rarity = CardRarity.Common,
        CardClass cardClass = CardClass.Ironclad)
    {
        CardData card = CreateAsset<CardData>();
        CombatTestRig.SetField(card, "id", "test-" + cardName);
        CombatTestRig.SetField(card, "cardName", cardName);
        CombatTestRig.SetField(card, "cardCost", cost);
        CombatTestRig.SetField(card, "cardType", CardType.Attack);
        CombatTestRig.SetField(card, "targetType", TargetType.Enemy);
        CombatTestRig.SetField(card, "cardRarity", rarity);
        CombatTestRig.SetField(card, "cardClass", cardClass);
        CombatTestRig.SetField(
            card,
            "cardEffects",
            new List<CardEffectData> { CombatTestRig.DamageEffect(damage) });
        return card;
    }

    /// <summary>An Ironclad character with the given deck.</summary>
    public PlayerUnitData CreateCharacter(
        IEnumerable<CardData> deck,
        int maxHealth = 80,
        int maxEnergy = 3,
        int drawAmount = 5)
    {
        DeckData deckData = CreateAsset<DeckData>();
        CombatTestRig.SetField(deckData, "cardDataList", new List<CardData>(deck));

        PlayerUnitData character = CreateAsset<PlayerUnitData>();
        CombatTestRig.SetField(character, "characterID", "test-character");
        CombatTestRig.SetField(character, "unitName", "Player");
        CombatTestRig.SetField(character, "unitType", UnitType.Player);
        CombatTestRig.SetField(character, "maxHealth", maxHealth);
        CombatTestRig.SetField(character, "maxEnergy", maxEnergy);
        CombatTestRig.SetField(character, "drawAmount", drawAmount);
        CombatTestRig.SetField(character, "playerCardClass", CardClass.Ironclad);
        CombatTestRig.SetField(character, "startingDeckData", deckData);
        return character;
    }

    /// <summary>A character whose cards each kill an enemy outright.</summary>
    public PlayerUnitData CreateStrongCharacter()
    {
        CardData strike = CreateCard("Strike", 1, 50);
        return CreateCharacter(new[] { strike, strike, strike, strike, strike, strike });
    }

    /// <summary>A run with reward pools filled in and combat logs off.</summary>
    public RunSession CreateRun(
        int seed,
        IRunProgressRecorder progress = null,
        MapSettings? map = null)
    {
        RunConfig config = new RunConfig
        {
            Map = map ?? GameSizedMap,
            NodeLibrary = Library,
            CardRewardAmount = 3,
        };

        RunSession run = new RunSession(seed, config, progress);
        run.Combat.ActionQueue.LogResolvedActions = false;
        run.Rewards.SetPools(
            new Dictionary<CardClass, List<CardData>>
            {
                { CardClass.Ironclad, RewardCards },
            });
        return run;
    }

    /// <summary>
    /// Plays the current combat to its end: the first legal card play each
    /// time, ending the turn when there is none.
    /// </summary>
    public static void PlayCombat(RunSession run)
    {
        CombatManager combat = run.Combat;

        for (int step = 0; step < 5000; step++)
        {
            if (run.State != RunState.CombatState) return;

            switch (combat.PendingDecision)
            {
                case PlayerTurnDecision turn:
                    List<PlayCardCommand> plays = turn.GetLegalPlays();
                    if (plays.Count > 0)
                    {
                        combat.Submit(plays[0]);
                    }
                    else
                    {
                        combat.Submit(new EndTurnCommand());
                    }
                    break;

                case CardChoiceDecision choice:
                    combat.Submit(new ChooseCardsCommand(choice.GetMinimalPicks()));
                    break;

                default:
                    return;
            }
        }

        throw new InvalidOperationException("The combat did not end.");
    }

    /// <summary>
    /// Plays a run until it is over or has nothing left to decide: the first
    /// node each time, every reward taken.
    /// </summary>
    /// <returns>Every node entered, as "floor:index:type".</returns>
    public static List<string> PlayRun(RunSession run)
    {
        List<string> path = new List<string>();

        for (int step = 0; step < 5000; step++)
        {
            if (run.IsOver) return path;

            if (run.State == RunState.CombatState)
            {
                PlayCombat(run);
                continue;
            }

            switch (run.PendingDecision)
            {
                case ChooseNodeDecision choose:
                    BaseNode node = choose.Options[0];
                    run.Submit(new ChooseNodeCommand(node));
                    path.Add($"{node.Floor}:{node.Index}:{node.ResolvedNode.Type}");
                    break;

                case CardRewardDecision reward:
                    if (reward.Offers.Count > 0)
                    {
                        run.Submit(new TakeRewardCommand(reward.Offers[0]));
                    }
                    else
                    {
                        run.Submit(new SkipRewardCommand());
                    }
                    break;

                default:
                    return path;
            }
        }

        throw new InvalidOperationException("The run did not end.");
    }

    public T CreateAsset<T>()
        where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        created.Add(asset);
        return asset;
    }

    private T CombatConfig<T>(EnemyUnitData enemy)
        where T : BaseCombatNodeConfig
    {
        T config = CreateAsset<T>();
        CombatTestRig.SetField(
            config,
            "enemyDatas",
            new List<EnemyUnitData> { enemy });
        return config;
    }
}
