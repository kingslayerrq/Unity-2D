using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// The rules for what a card may be played at, checked directly and through
/// the three places that rely on them: the player-turn decision, the play
/// command, and the drop receiver on a unit.
/// </summary>
public sealed class CardTargetingTests
{
    private CombatTestRig rig;
    private CardData strike;
    private BasePlayerUnitInstance player;
    private BaseEnemyUnitInstance first;
    private BaseEnemyUnitInstance second;

    [SetUp]
    public void SetUp()
    {
        rig = new CombatTestRig();
        strike = rig.CreateCard(
            "Strike",
            cost: 1,
            targetType: TargetType.Enemy,
            effects: CombatTestRig.DamageEffect(6));

        player = rig.AddPlayer(
            new[] { strike, strike, strike, strike },
            maxEnergy: 3,
            drawAmount: 2);
        first = rig.AddEnemy("First", 20);
        second = rig.AddEnemy("Second", 20);
    }

    [TearDown]
    public void TearDown()
    {
        rig.Dispose();
    }

    private CardInstance Card(TargetType targetType)
    {
        CardData data = rig.CreateCard(
            "Card " + targetType,
            cost: 1,
            targetType: targetType,
            effects: CombatTestRig.DamageEffect(4));

        return new CardInstance(data, player);
    }

    private List<IReadOnlyList<BaseUnitInstance>> Options(TargetType targetType)
    {
        return rig.Combat.GetTargetOptions(Card(targetType));
    }

    #region Options

    [Test]
    public void SingleEnemyCardHasOneOptionPerEnemy()
    {
        List<IReadOnlyList<BaseUnitInstance>> options =
            Options(TargetType.Enemy);

        Assert.That(options.Count, Is.EqualTo(2));
        CollectionAssert.AreEqual(new[] { first }, options[0]);
        CollectionAssert.AreEqual(new[] { second }, options[1]);
    }

    [Test]
    public void AllEnemyCardHasOneOptionHoldingEveryEnemy()
    {
        List<IReadOnlyList<BaseUnitInstance>> options =
            Options(TargetType.AllEnemy);

        Assert.That(options.Count, Is.EqualTo(1));
        CollectionAssert.AreEquivalent(
            new BaseUnitInstance[] { first, second },
            options[0]);
    }

    [Test]
    public void SelfCardTargetsThePlayer()
    {
        List<IReadOnlyList<BaseUnitInstance>> options =
            Options(TargetType.Self);

        Assert.That(options.Count, Is.EqualTo(1));
        CollectionAssert.AreEqual(new[] { player }, options[0]);
    }

    [Test]
    public void AnyCardHasOneOptionPerUnitOnEitherSide()
    {
        Assert.That(Options(TargetType.Any).Count, Is.EqualTo(3));
    }

    [Test]
    public void CardWithNoTargetIsPlayedAtNothing()
    {
        List<IReadOnlyList<BaseUnitInstance>> options =
            Options(TargetType.None);

        Assert.That(options.Count, Is.EqualTo(1));
        Assert.That(options[0], Is.Empty);
    }

    [Test]
    public void DeadUnitIsNotATarget()
    {
        // Resolve the lethal hit but not the defeat that follows it, so the
        // unit is dead and still listed in the combat.
        rig.Queue.AutoDrain = false;
        rig.Queue.Enqueue(
            new DamageAction(
                rig.CombatSystem,
                player,
                new[] { first },
                99,
                CardType.Attack),
            null);
        rig.Queue.Step();

        Assert.That(first.IsDead, Is.True);
        Assert.That(rig.Combat.Enemies, Has.Member(first));

        List<IReadOnlyList<BaseUnitInstance>> single =
            Options(TargetType.Enemy);
        Assert.That(single.Count, Is.EqualTo(1));
        CollectionAssert.AreEqual(new[] { second }, single[0]);

        List<IReadOnlyList<BaseUnitInstance>> all =
            Options(TargetType.AllEnemy);
        CollectionAssert.AreEqual(new[] { second }, all[0]);
    }

    #endregion

    #region Legality

    [Test]
    public void LegalityIgnoresOrderButNotContent()
    {
        CardInstance all = Card(TargetType.AllEnemy);
        CardInstance single = Card(TargetType.Enemy);

        Assert.That(
            rig.Combat.IsLegalTargets(
                all,
                new BaseUnitInstance[] { second, first }),
            Is.True);
        Assert.That(
            rig.Combat.IsLegalTargets(all, new[] { first }),
            Is.False);
        Assert.That(
            rig.Combat.IsLegalTargets(
                all,
                new BaseUnitInstance[] { first, first }),
            Is.False);

        Assert.That(
            rig.Combat.IsLegalTargets(single, new[] { second }),
            Is.True);
        Assert.That(
            rig.Combat.IsLegalTargets(
                single,
                new BaseUnitInstance[] { first, second }),
            Is.False);
        Assert.That(
            rig.Combat.IsLegalTargets(single, new[] { player }),
            Is.False);
        Assert.That(
            rig.Combat.IsLegalTargets(single, null),
            Is.False);
    }

    #endregion

    #region Drops

    [Test]
    public void OpenSpaceTakesOnlyCardsWithNoUnitToChoose()
    {
        UnitsManager units = rig.Combat.UnitsManager;

        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.Enemy), player, units, null, out _),
            Is.False);
        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.Any), player, units, null, out _),
            Is.False);

        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.AllEnemy),
                player,
                units,
                null,
                out IReadOnlyList<BaseUnitInstance> everyEnemy),
            Is.True);
        CollectionAssert.AreEquivalent(
            new BaseUnitInstance[] { first, second },
            everyEnemy);

        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.Self),
                player,
                units,
                null,
                out IReadOnlyList<BaseUnitInstance> self),
            Is.True);
        CollectionAssert.AreEqual(new[] { player }, self);

        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.None),
                player,
                units,
                null,
                out IReadOnlyList<BaseUnitInstance> nothing),
            Is.True);
        Assert.That(nothing, Is.Empty);
    }

    [Test]
    public void DropOnAUnitMeansTheWholeOptionThatUnitBelongsTo()
    {
        UnitsManager units = rig.Combat.UnitsManager;

        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.Enemy),
                player,
                units,
                second,
                out IReadOnlyList<BaseUnitInstance> single),
            Is.True);
        CollectionAssert.AreEqual(new[] { second }, single);

        // Dropping an all-enemies card on one enemy still hits them all.
        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.AllEnemy),
                player,
                units,
                second,
                out IReadOnlyList<BaseUnitInstance> everyEnemy),
            Is.True);
        CollectionAssert.AreEquivalent(
            new BaseUnitInstance[] { first, second },
            everyEnemy);

        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.Enemy), player, units, player, out _),
            Is.False);
        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.Self), player, units, first, out _),
            Is.False);
        Assert.That(
            CardTargeting.TryResolveDrop(
                Card(TargetType.Self), player, units, player, out _),
            Is.True);
    }

    [Test]
    public void UnitDropReceiverFollowsTheSameRules()
    {
        UnitTarget receiver =
            rig.CreateGameObject("Second View").AddComponent<UnitTarget>();
        receiver.Initialize(second);

        CardInstance single = Card(TargetType.Enemy);
        CardInstance all = Card(TargetType.AllEnemy);
        CardInstance self = Card(TargetType.Self);

        Assert.That(receiver.CanReceiveCard(player, single), Is.True);
        CollectionAssert.AreEqual(
            new[] { second },
            receiver.GetReceiverUnitInstance(player, single));

        Assert.That(receiver.CanReceiveCard(player, all), Is.True);
        CollectionAssert.AreEquivalent(
            new BaseUnitInstance[] { first, second },
            receiver.GetReceiverUnitInstance(player, all));

        Assert.That(receiver.CanReceiveCard(player, self), Is.False);
        Assert.That(
            receiver.GetReceiverUnitInstance(player, self),
            Is.Empty);
    }

    [Test]
    public void UnitDropReceiverHighlightsEverythingTheCardWouldHit()
    {
        UnitTarget receiver =
            rig.CreateGameObject("Second View").AddComponent<UnitTarget>();
        receiver.Initialize(second);

        bool firstHighlighted = false;
        bool secondHighlighted = false;
        first.OnHighlightTriggered += on => firstHighlighted = on;
        second.OnHighlightTriggered += on => secondHighlighted = on;

        CardInstance all = Card(TargetType.AllEnemy);

        receiver.OnHoverEnter(player, all);

        Assert.That(firstHighlighted, Is.True);
        Assert.That(secondHighlighted, Is.True);

        receiver.OnHoverExit(player, all);

        Assert.That(firstHighlighted, Is.False);
        Assert.That(secondHighlighted, Is.False);
    }

    #endregion

    #region Through decisions and commands

    [Test]
    public void LegalPlaysCoverEveryCardAndTargetOption()
    {
        rig.StartCombat();

        PlayerTurnDecision turn =
            (PlayerTurnDecision)rig.Combat.PendingDecision;

        // Two Strikes in hand, two enemies each.
        List<PlayCardCommand> plays = turn.GetLegalPlays();
        Assert.That(plays.Count, Is.EqualTo(4));

        foreach (PlayCardCommand play in plays)
        {
            Assert.That(
                rig.Combat.IsLegalTargets(play.Card, play.Targets),
                Is.True);
        }

        CommandResult result = rig.Combat.Submit(plays[0]);

        Assert.That(result.Accepted, Is.True, result.ToString());
    }

    [Test]
    public void PlayAtIllegalTargetsIsRejectedAndCostsNothing()
    {
        rig.StartCombat();

        CardInstance card = rig.Player.Hand.Cards[0];

        Assert.That(
            rig.Combat.Submit(
                new PlayCardCommand(card, new[] { player })).Accepted,
            Is.False);
        Assert.That(
            rig.Combat.Submit(
                new PlayCardCommand(
                    card,
                    new BaseUnitInstance[] { first, second })).Accepted,
            Is.False);

        Assert.That(rig.Player.CurrentEnergy, Is.EqualTo(3));
        Assert.That(rig.Player.Hand.Cards, Has.Member(card));
        Assert.That(first.CurrentHealth, Is.EqualTo(20));
        Assert.That(player.CurrentHealth, Is.EqualTo(50));
    }

    [Test]
    public void AllEnemyCardHitsEveryEnemyAndCannotBeAimedAtOne()
    {
        // A fresh rig, so the whole hand is Cleaves.
        rig.Dispose();
        rig = new CombatTestRig();
        CardData cleave = rig.CreateCard(
            "Cleave",
            cost: 1,
            targetType: TargetType.AllEnemy,
            effects: CombatTestRig.DamageEffect(4));
        player = rig.AddPlayer(new[] { cleave, cleave, cleave });
        first = rig.AddEnemy("First", 20);
        second = rig.AddEnemy("Second", 20);
        rig.StartCombat();

        PlayerTurnDecision turn =
            (PlayerTurnDecision)rig.Combat.PendingDecision;
        CardInstance card = turn.GetPlayableCards()[0];

        Assert.That(
            rig.Combat.Submit(
                new PlayCardCommand(card, new[] { first })).Accepted,
            Is.False);

        List<IReadOnlyList<BaseUnitInstance>> options =
            turn.GetTargetOptions(card);
        Assert.That(options.Count, Is.EqualTo(1));

        CommandResult result = rig.Combat.Submit(
            new PlayCardCommand(card, options[0]));

        Assert.That(result.Accepted, Is.True, result.ToString());
        Assert.That(first.CurrentHealth, Is.EqualTo(16));
        Assert.That(second.CurrentHealth, Is.EqualTo(16));
    }

    #endregion
}
