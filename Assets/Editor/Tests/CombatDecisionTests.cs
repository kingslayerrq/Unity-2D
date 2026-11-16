using System;
using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Plays combat the way a bot does: read the pending decision, submit a
/// command, read the next decision. No scene, views or frames are involved.
/// </summary>
public sealed class CombatDecisionTests
{
    private sealed class LambdaAction : ICombatAction
    {
        private readonly Action _body;

        public LambdaAction(Action body)
        {
            _body = body;
        }

        public void Execute() => _body?.Invoke();
        public string GetActionDetails() => "LambdaAction";
    }

    private CombatTestRig rig;
    private CardData strike;

    [SetUp]
    public void SetUp()
    {
        rig = new CombatTestRig();
        strike = rig.CreateCard(
            "Strike",
            cost: 1,
            effects: CombatTestRig.DamageEffect(6));
    }

    [TearDown]
    public void TearDown()
    {
        rig.Dispose();
    }

    private CardData[] Deck(CardData card, int count)
    {
        CardData[] deck = new CardData[count];
        for (int i = 0; i < count; i++)
        {
            deck[i] = card;
        }

        return deck;
    }

    private PlayerTurnDecision PendingTurn()
    {
        Assert.That(
            rig.Combat.PendingDecision,
            Is.InstanceOf<PlayerTurnDecision>());

        return (PlayerTurnDecision)rig.Combat.PendingDecision;
    }

    #region Player turn

    [Test]
    public void PlayerTurnIsPendingOnceTheTurnHasStarted()
    {
        rig.AddPlayer(Deck(strike, 5), drawAmount: 2);
        rig.AddEnemy("Enemy", 20);

        Assert.That(rig.Combat.PendingDecision, Is.Null);

        rig.StartCombat();

        PlayerTurnDecision turn = PendingTurn();
        Assert.That(turn.Turn, Is.EqualTo(1));
        Assert.That(rig.Player.Hand.Count, Is.EqualTo(2));
        Assert.That(turn.GetPlayableCards().Count, Is.EqualTo(2));
    }

    [Test]
    public void DecisionRequestedAnnouncesThePlayerTurn()
    {
        List<ICombatDecision> requested = new List<ICombatDecision>();
        rig.Combat.DecisionRequested += requested.Add;

        rig.AddPlayer(Deck(strike, 5));
        rig.AddEnemy("Enemy", 20);
        rig.StartCombat();

        Assert.That(requested.Count, Is.EqualTo(1));
        Assert.That(requested[0], Is.SameAs(rig.Combat.PendingDecision));
    }

    [Test]
    public void PlayingACardPaysForItAndResolvesItsEffects()
    {
        rig.AddPlayer(Deck(strike, 5), maxEnergy: 3);
        BaseEnemyUnitInstance enemy = rig.AddEnemy("Enemy", 20);
        rig.StartCombat();

        CardInstance card = PendingTurn().GetPlayableCards()[0];

        CommandResult result = rig.Combat.Submit(
            new PlayCardCommand(card, new[] { enemy }));

        Assert.That(result.Accepted, Is.True, result.ToString());
        Assert.That(enemy.CurrentHealth, Is.EqualTo(14));
        Assert.That(rig.Player.CurrentEnergy, Is.EqualTo(2));
        Assert.That(rig.Player.Hand.Cards, Has.No.Member(card));
        Assert.That(
            card.CurrentCardState,
            Is.EqualTo(CardState.InDiscard));

        // Still the player's turn, with one fewer card to play.
        Assert.That(PendingTurn().GetPlayableCards().Count, Is.EqualTo(1));
    }

    [Test]
    public void CardThatCannotBePaidForIsRejectedAndChangesNothing()
    {
        CardData expensive = rig.CreateCard(
            "Expensive",
            cost: 5,
            effects: CombatTestRig.DamageEffect(30));

        rig.AddPlayer(Deck(expensive, 5), maxEnergy: 3);
        BaseEnemyUnitInstance enemy = rig.AddEnemy("Enemy", 20);
        rig.StartCombat();

        PlayerTurnDecision turn = PendingTurn();
        CardInstance card = rig.Player.Hand.Cards[0];

        Assert.That(turn.CanPlay(card), Is.False);
        Assert.That(turn.GetPlayableCards(), Is.Empty);

        CommandResult result = rig.Combat.Submit(
            new PlayCardCommand(card, new[] { enemy }));

        Assert.That(result.Accepted, Is.False);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(20));
        Assert.That(rig.Player.CurrentEnergy, Is.EqualTo(3));
        Assert.That(rig.Player.Hand.Cards, Has.Member(card));
    }

    [Test]
    public void CardNeedingATargetIsRejectedWithoutOne()
    {
        rig.AddPlayer(Deck(strike, 5), maxEnergy: 3);
        rig.AddEnemy("Enemy", 20);
        rig.StartCombat();

        CardInstance card = PendingTurn().GetPlayableCards()[0];

        CommandResult result = rig.Combat.Submit(
            new PlayCardCommand(card, null));

        Assert.That(result.Accepted, Is.False);
        Assert.That(rig.Player.CurrentEnergy, Is.EqualTo(3));
        Assert.That(rig.Player.Hand.Cards, Has.Member(card));
    }

    [Test]
    public void EndingTheTurnRunsTheEnemyTurnAndStartsTheNextOne()
    {
        rig.AddPlayer(
            Deck(strike, 6),
            maxHealth: 50,
            maxEnergy: 3,
            drawAmount: 2);
        rig.AddEnemy("Enemy", 20, CombatTestRig.DamageIntent(5));
        rig.StartCombat();

        CardInstance card = PendingTurn().GetPlayableCards()[0];
        rig.Combat.Submit(
            new PlayCardCommand(card, new[] { rig.Combat.Enemies[0] }));

        CommandResult result = rig.Combat.Submit(new EndTurnCommand());

        Assert.That(result.Accepted, Is.True, result.ToString());

        // The call returns only after the enemy has acted and the next
        // player turn is ready.
        Assert.That(rig.Player.CurrentHealth, Is.EqualTo(45));

        PlayerTurnDecision next = PendingTurn();
        Assert.That(next.Turn, Is.EqualTo(2));
        Assert.That(rig.Player.CurrentEnergy, Is.EqualTo(3));
        Assert.That(rig.Player.Hand.Count, Is.EqualTo(2));
    }

    [Test]
    public void KillingTheLastEnemyEndsTheCombat()
    {
        rig.AddPlayer(Deck(strike, 5));
        BaseEnemyUnitInstance enemy = rig.AddEnemy("Enemy", 6);
        rig.StartCombat();

        CardInstance card = PendingTurn().GetPlayableCards()[0];
        rig.Combat.Submit(new PlayCardCommand(card, new[] { enemy }));

        Assert.That(enemy.IsDead, Is.True);
        Assert.That(
            rig.Combat.CurrentCombatState,
            Is.EqualTo(CombatManager.NodeState.Victory));
        Assert.That(rig.Combat.PendingDecision, Is.Null);

        CommandResult afterEnd = rig.Combat.Submit(new EndTurnCommand());

        Assert.That(afterEnd.Accepted, Is.False);
    }

    [Test]
    public void CommandsAreRejectedOutsideThePlayersActionPhase()
    {
        rig.AddPlayer(Deck(strike, 5));
        rig.AddEnemy("Enemy", 20);

        // The combat has not started, so nothing is pending.
        Assert.That(
            rig.Combat.Submit(new EndTurnCommand()).Accepted,
            Is.False);
        Assert.That(
            rig.Combat.Submit(new ChooseCardsCommand(null)).Accepted,
            Is.False);
        Assert.That(rig.Combat.Submit(null).Accepted, Is.False);
    }

    #endregion

    #region Card choice

    [Test]
    public void CardThatAsksForADiscardWaitsForTheChoice()
    {
        CardData prepare = rig.CreateCard(
            "Prepare",
            cost: 1,
            targetType: TargetType.Self,
            cardType: CardType.Skill,
            effects: CombatTestRig.SelectDiscardEffect(1));

        rig.AddPlayer(Deck(prepare, 6), drawAmount: 3);
        rig.AddEnemy("Enemy", 20);
        rig.StartCombat();

        CardInstance played = rig.Player.Hand.Cards[0];
        CardInstance other = rig.Player.Hand.Cards[1];

        CommandResult playResult = rig.Combat.Submit(
            new PlayCardCommand(played, new[] { rig.Player }));

        Assert.That(playResult.Accepted, Is.True, playResult.ToString());

        // Combat has stopped on the choice the card asked for.
        Assert.That(
            rig.Combat.PendingDecision,
            Is.InstanceOf<CardChoiceDecision>());

        CardChoiceDecision choice =
            (CardChoiceDecision)rig.Combat.PendingDecision;

        Assert.That(choice.Kind, Is.EqualTo(CardChoiceKind.Discard));
        Assert.That(choice.MinCount, Is.EqualTo(1));
        Assert.That(choice.MaxCount, Is.EqualTo(1));
        Assert.That(choice.Options, Has.No.Member(played));
        Assert.That(choice.Options.Count, Is.EqualTo(2));

        CommandResult chooseResult = rig.Combat.Submit(
            new ChooseCardsCommand(new[] { other }));

        Assert.That(chooseResult.Accepted, Is.True, chooseResult.ToString());
        Assert.That(
            other.CurrentCardState,
            Is.EqualTo(CardState.InDiscard));
        Assert.That(rig.Player.Hand.Cards, Has.No.Member(other));
        Assert.That(rig.Player.Hand.Count, Is.EqualTo(1));

        // Back to the player's turn.
        PendingTurn();
    }

    [Test]
    public void OtherCommandsAreRejectedWhileACardChoiceIsPending()
    {
        CardData prepare = rig.CreateCard(
            "Prepare",
            cost: 1,
            targetType: TargetType.Self,
            cardType: CardType.Skill,
            effects: CombatTestRig.SelectDiscardEffect(1));

        rig.AddPlayer(Deck(prepare, 6), drawAmount: 3);
        rig.AddEnemy("Enemy", 20);
        rig.StartCombat();

        CardInstance played = rig.Player.Hand.Cards[0];
        CardInstance other = rig.Player.Hand.Cards[1];

        rig.Combat.Submit(new PlayCardCommand(played, new[] { rig.Player }));

        Assert.That(
            rig.Combat.Submit(
                new PlayCardCommand(other, new[] { rig.Player })).Accepted,
            Is.False);
        Assert.That(
            rig.Combat.Submit(new EndTurnCommand()).Accepted,
            Is.False);
        Assert.That(
            rig.Combat.PendingDecision,
            Is.InstanceOf<CardChoiceDecision>());
    }

    [Test]
    public void InvalidCardChoiceIsRejectedAndStaysPending()
    {
        List<CardInstance> options = CreateLooseCards(3);
        CardInstance outsider = CreateLooseCards(1)[0];
        IReadOnlyList<CardInstance> accepted = null;

        CardChoiceDecision choice = new(
            null,
            CardChoiceKind.Exhaust,
            options,
            minCount: 1,
            maxCount: 2,
            picks => accepted = picks);

        rig.Combat.RequestCardChoice(choice);

        Assert.That(rig.Combat.PendingDecision, Is.SameAs(choice));
        Assert.That(rig.Queue.IsSuspended, Is.True);

        AssertRejected(Array.Empty<CardInstance>());
        AssertRejected(options);
        AssertRejected(new[] { outsider });
        AssertRejected(new[] { options[0], options[0] });

        Assert.That(accepted, Is.Null);
        Assert.That(rig.Combat.PendingDecision, Is.SameAs(choice));
        Assert.That(rig.Queue.IsSuspended, Is.True);

        CommandResult result = rig.Combat.Submit(
            new ChooseCardsCommand(new[] { options[2], options[0] }));

        Assert.That(result.Accepted, Is.True, result.ToString());
        CollectionAssert.AreEqual(
            new[] { options[2], options[0] },
            accepted);
        Assert.That(rig.Combat.PendingDecision, Is.Null);
        Assert.That(rig.Queue.IsSuspended, Is.False);
    }

    [Test]
    public void CardChoiceHoldsQueuedActionsUntilAnswered()
    {
        List<CardInstance> options = CreateLooseCards(2);
        List<string> log = new List<string>();
        List<ICombatDecision> requested = new List<ICombatDecision>();
        rig.Combat.DecisionRequested += requested.Add;

        rig.Queue.AutoDrain = false;
        rig.Queue.Enqueue(
            new LambdaAction(
                () => rig.Combat.RequestCardChoice(
                    new CardChoiceDecision(
                        null,
                        CardChoiceKind.Discard,
                        options,
                        minCount: 1,
                        maxCount: 1,
                        picks => log.Add("picked " + picks.Count)))),
            null);
        rig.Queue.Enqueue(
            new LambdaAction(() => log.Add("after")),
            null);
        rig.Queue.Drain();

        Assert.That(log, Is.Empty);
        Assert.That(requested.Count, Is.EqualTo(1));
        Assert.That(requested[0], Is.InstanceOf<CardChoiceDecision>());

        rig.Queue.AutoDrain = true;
        rig.Combat.Submit(new ChooseCardsCommand(new[] { options[1] }));

        CollectionAssert.AreEqual(
            new[] { "picked 1", "after" },
            log);
        Assert.That(rig.Queue.IsIdle, Is.True);
    }

    [Test]
    public void OptionalCardChoiceAcceptsNothing()
    {
        IReadOnlyList<CardInstance> accepted = null;

        CardChoiceDecision choice = new(
            null,
            CardChoiceKind.Discard,
            CreateLooseCards(2),
            minCount: 0,
            maxCount: 2,
            picks => accepted = picks);

        rig.Combat.RequestCardChoice(choice);

        Assert.That(choice.IsOptional, Is.True);

        CommandResult result = rig.Combat.Submit(
            new ChooseCardsCommand(null));

        Assert.That(result.Accepted, Is.True, result.ToString());
        Assert.That(accepted, Is.Empty);
    }

    [Test]
    public void ChoiceWithNothingToPickResolvesWithoutWaiting()
    {
        IReadOnlyList<CardInstance> accepted = null;

        rig.Combat.RequestCardChoice(
            new CardChoiceDecision(
                null,
                CardChoiceKind.Discard,
                new List<CardInstance>(),
                minCount: 1,
                maxCount: 1,
                picks => accepted = picks));

        Assert.That(accepted, Is.Empty);
        Assert.That(rig.Combat.PendingDecision, Is.Null);
        Assert.That(rig.Queue.IsSuspended, Is.False);
    }

    [Test]
    public void MinimalPicksAreTheSmallestValidAnswer()
    {
        List<CardInstance> options = CreateLooseCards(4);

        CardChoiceDecision choice = new(
            null,
            CardChoiceKind.Exhaust,
            options,
            minCount: 2,
            maxCount: 3,
            null);

        List<CardInstance> picks = choice.GetMinimalPicks();

        CollectionAssert.AreEqual(
            new[] { options[0], options[1] },
            picks);
        Assert.That(choice.IsValid(picks, out _), Is.True);
    }

    [Test]
    public void ChoiceCountsAreClampedToTheOptions()
    {
        CardChoiceDecision choice = new(
            null,
            CardChoiceKind.Discard,
            CreateLooseCards(2),
            minCount: 5,
            maxCount: 5,
            null);

        Assert.That(choice.MaxCount, Is.EqualTo(2));
        Assert.That(choice.MinCount, Is.EqualTo(2));
    }

    #endregion

    private void AssertRejected(IReadOnlyList<CardInstance> picks)
    {
        CommandResult result = rig.Combat.Submit(
            new ChooseCardsCommand(picks));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Reason, Is.Not.Null.And.Not.Empty);
    }

    private List<CardInstance> CreateLooseCards(int count)
    {
        List<CardInstance> cards = new List<CardInstance>();

        for (int i = 0; i < count; i++)
        {
            cards.Add(new CardInstance(strike));
        }

        return cards;
    }
}
