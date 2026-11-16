using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

/// <summary>
/// A combat takes its random numbers and its progression recording from the
/// <see cref="CombatContext"/> it is given, and from nowhere else.
/// </summary>
public sealed class CombatContextTests
{
    private sealed class RecordingProgress : ICombatProgressRecorder
    {
        public readonly List<CardInstance> Played = new List<CardInstance>();

        public void RecordCardPlayed(CardInstance card)
        {
            Played.Add(card);
        }
    }

    private sealed class FailingProgress : ICombatProgressRecorder
    {
        public void RecordCardPlayed(CardInstance card)
        {
            throw new InvalidOperationException("recorder broke");
        }
    }

    private readonly List<CombatTestRig> rigs = new List<CombatTestRig>();

    [TearDown]
    public void TearDown()
    {
        foreach (CombatTestRig rig in rigs)
        {
            rig.Dispose();
        }

        rigs.Clear();
    }

    private CombatTestRig NewRig(
        int seed = 12345,
        ICombatProgressRecorder progress = null)
    {
        CombatTestRig rig = new CombatTestRig(seed, progress);
        rigs.Add(rig);
        return rig;
    }

    /// <summary>A deck of differently named cards, so order can be compared.</summary>
    private static CardData[] DistinctDeck(CombatTestRig rig, int count)
    {
        CardData[] deck = new CardData[count];

        for (int i = 0; i < count; i++)
        {
            deck[i] = rig.CreateCard(
                "Card " + i,
                cost: 1,
                effects: CombatTestRig.DamageEffect(1));
        }

        return deck;
    }

    private static List<string> Names(IReadOnlyList<CardInstance> cards)
    {
        List<string> names = new List<string>();

        foreach (CardInstance card in cards)
        {
            names.Add(card.CurrentName);
        }

        return names;
    }

    /// <summary>
    /// Deals a ten-card deck with the seed and returns the order of the hand
    /// followed by the draw pile.
    /// </summary>
    private List<string> DealtOrder(int seed)
    {
        CombatTestRig rig = NewRig(seed);
        rig.AddPlayer(DistinctDeck(rig, 10), drawAmount: 3);
        rig.AddEnemy("Enemy", 50);
        rig.StartCombat();

        List<string> order = Names(rig.Player.Hand.Cards);
        order.AddRange(Names(rig.Piles.DrawPile.Cards));
        return order;
    }

    /// <summary>
    /// Ends three turns against an enemy that picks between a weak and a
    /// strong attack, and returns the player's health after each.
    /// </summary>
    private List<int> HealthAfterEachTurn(int seed)
    {
        CombatTestRig rig = NewRig(seed);
        rig.AddPlayer(DistinctDeck(rig, 6), maxHealth: 200);
        rig.AddEnemy(
            "Enemy",
            50,
            CombatTestRig.DamageIntent(1),
            CombatTestRig.DamageIntent(10));
        rig.StartCombat();

        List<int> health = new List<int>();

        for (int turn = 0; turn < 6; turn++)
        {
            rig.Combat.Submit(new EndTurnCommand());
            health.Add(rig.Player.CurrentHealth);
        }

        return health;
    }

    [Test]
    public void SameSeedDealsTheSameCards()
    {
        CollectionAssert.AreEqual(DealtOrder(7), DealtOrder(7));
    }

    [Test]
    public void DifferentSeedsDealDifferently()
    {
        CollectionAssert.AreNotEqual(DealtOrder(1), DealtOrder(2));
    }

    [Test]
    public void SameSeedGivesTheSameEnemyMoves()
    {
        List<int> first = HealthAfterEachTurn(99);
        List<int> second = HealthAfterEachTurn(99);

        CollectionAssert.AreEqual(first, second);

        // The enemy did choose between its attacks, so the match above is
        // not just one move repeated.
        Assert.That(first[5], Is.LessThan(200 - 6));
        Assert.That(first[5], Is.GreaterThan(200 - 60));
    }

    [Test]
    public void CardPlaysAreReportedToTheProgressRecorder()
    {
        RecordingProgress progress = new RecordingProgress();
        CombatTestRig rig = NewRig(progress: progress);
        rig.AddPlayer(DistinctDeck(rig, 5));
        BaseEnemyUnitInstance enemy = rig.AddEnemy("Enemy", 50);
        rig.StartCombat();

        CardInstance card = rig.Player.Hand.Cards[0];
        rig.Combat.Submit(new PlayCardCommand(card, new[] { enemy }));

        CollectionAssert.AreEqual(new[] { card }, progress.Played);
    }

    [Test]
    public void FailingRecorderDoesNotStopTheCard()
    {
        LogAssert.Expect(LogType.Exception, new Regex("recorder broke"));

        CombatTestRig rig = NewRig(progress: new FailingProgress());
        rig.AddPlayer(DistinctDeck(rig, 5));
        BaseEnemyUnitInstance enemy = rig.AddEnemy("Enemy", 50);
        rig.StartCombat();

        CardInstance card = rig.Player.Hand.Cards[0];
        CommandResult result = rig.Combat.Submit(
            new PlayCardCommand(card, new[] { enemy }));

        Assert.That(result.Accepted, Is.True, result.ToString());
        Assert.That(enemy.CurrentHealth, Is.EqualTo(49));
        Assert.That(
            card.CurrentCardState,
            Is.EqualTo(CardState.InDiscard));
    }

    [Test]
    public void CombatRunsWithoutAnyContext()
    {
        CombatManager combat = new CombatManager();
        combat.ActionQueue.LogResolvedActions = false;

        Assert.That(combat.Rng, Is.Not.Null);
        Assert.That(combat.PendingDecision, Is.Null);
    }

    [Test]
    public void PlayingACombatNeverCreatesTheRunManager()
    {
        CombatTestRig rig = NewRig();
        rig.AddPlayer(DistinctDeck(rig, 6));
        BaseEnemyUnitInstance enemy = rig.AddEnemy(
            "Enemy",
            50,
            CombatTestRig.DamageIntent(3));
        rig.StartCombat();

        rig.Combat.Submit(
            new PlayCardCommand(
                rig.Player.Hand.Cards[0],
                new[] { enemy }));
        rig.Combat.Submit(new EndTurnCommand());
        rig.Combat.Submit(new EndTurnCommand());

        // RunManager.Instance builds one on demand, so finding none proves
        // the combat never asked for it.
        Assert.That(
            Object.FindObjectsByType<RunManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None),
            Is.Empty);
    }
}
