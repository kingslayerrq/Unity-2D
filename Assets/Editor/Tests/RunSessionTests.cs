using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Plays whole runs with no scene: choose a character, pick nodes on the
/// map, fight, take rewards. Everything goes through decisions and commands.
/// </summary>
public sealed class RunSessionTests
{
    private RunTestRig rig;
    private RunTestRig.RecordingProgress progress;

    [SetUp]
    public void SetUp()
    {
        rig = new RunTestRig();
        progress = new RunTestRig.RecordingProgress();
    }

    [TearDown]
    public void TearDown()
    {
        rig.Dispose();
    }

    private RunSession StartedRun(int seed = 2024)
    {
        RunSession run = rig.CreateRun(seed, progress);
        run.ChooseCharacter(rig.CreateStrongCharacter());
        return run;
    }

    private static ChooseNodeDecision PendingNodeChoice(RunSession run)
    {
        Assert.That(run.PendingDecision, Is.InstanceOf<ChooseNodeDecision>());
        return (ChooseNodeDecision)run.PendingDecision;
    }

    private static CardRewardDecision PendingReward(RunSession run)
    {
        Assert.That(run.PendingDecision, Is.InstanceOf<CardRewardDecision>());
        return (CardRewardDecision)run.PendingDecision;
    }

    /// <summary>Enters the first available node and wins its combat.</summary>
    private static BaseNode WinFirstNode(RunSession run)
    {
        BaseNode node = PendingNodeChoice(run).Options[0];
        run.Submit(new ChooseNodeCommand(node));
        RunTestRig.PlayCombat(run);
        return node;
    }

    #region Map and nodes

    [Test]
    public void NothingIsPendingBeforeACharacterIsChosen()
    {
        RunSession run = rig.CreateRun(1, progress);

        Assert.That(run.State, Is.EqualTo(RunState.None));
        Assert.That(run.PendingDecision, Is.Null);
        Assert.That(run.Player, Is.Null);
    }

    [Test]
    public void ChoosingACharacterOpensTheMapOnTheFirstFloor()
    {
        RunSession run = StartedRun();

        Assert.That(run.State, Is.EqualTo(RunState.MapState));
        Assert.That(run.Player, Is.Not.Null);
        Assert.That(run.Map, Is.Not.Null);
        Assert.That(run.Map.FloorCount, Is.EqualTo(15));
        Assert.That(run.CurrentFloor, Is.EqualTo(0));

        ChooseNodeDecision choice = PendingNodeChoice(run);

        Assert.That(choice.Floor, Is.EqualTo(0));
        Assert.That(
            choice.Options.Count,
            Is.EqualTo(run.Map.GetReachableNodes(0).Count));

        foreach (BaseNode node in choice.Options)
        {
            Assert.That(node.Floor, Is.EqualTo(0));
            Assert.That(node.Type, Is.EqualTo(NodeType.CombatNonElite));
        }
    }

    [Test]
    public void EnteringACombatNodeStartsItsBattle()
    {
        RunSession run = StartedRun();
        BaseNode node = PendingNodeChoice(run).Options[0];

        CommandResult result = run.Submit(new ChooseNodeCommand(node));

        Assert.That(result.Accepted, Is.True, result.ToString());
        Assert.That(run.State, Is.EqualTo(RunState.CombatState));
        Assert.That(run.CurrentNode, Is.SameAs(node));

        // The run waits on the combat, whose own decision is now pending.
        Assert.That(run.PendingDecision, Is.Null);
        Assert.That(
            run.Combat.PendingDecision,
            Is.InstanceOf<PlayerTurnDecision>());
        Assert.That(run.Combat.Enemies.Count, Is.EqualTo(1));
        Assert.That(run.Combat.Player, Is.SameAs(run.Player));
    }

    [Test]
    public void BattleSetUpIsRaisedBeforeTheBattleBegins()
    {
        RunSession run = StartedRun();
        CombatManager.NodeState stateWhenRaised = CombatManager.NodeState.None;
        int enemiesWhenRaised = 0;

        run.BattleSetUp += combat =>
        {
            stateWhenRaised = combat.CurrentCombatState;
            enemiesWhenRaised = combat.Enemies.Count;
        };

        run.Submit(new ChooseNodeCommand(PendingNodeChoice(run).Options[0]));

        // Units exist, but the first turn has not started: this is where
        // views bind.
        Assert.That(
            stateWhenRaised,
            Is.EqualTo(CombatManager.NodeState.BattleSetup));
        Assert.That(enemiesWhenRaised, Is.EqualTo(1));
    }

    [Test]
    public void NodeThatIsNotNextIsRejected()
    {
        RunSession run = StartedRun();
        BaseNode entered = WinFirstNode(run);
        run.Submit(new SkipRewardCommand());

        // Back on the map, one floor up.
        Assert.That(run.CurrentFloor, Is.EqualTo(1));

        BaseNode alreadyPlayed = entered;
        BaseNode twoFloorsUp = run.Map.GetReachableNodes(2)[0].RuntimeNode;

        Assert.That(
            run.Submit(new ChooseNodeCommand(alreadyPlayed)).Accepted,
            Is.False);
        Assert.That(
            run.Submit(new ChooseNodeCommand(twoFloorsUp)).Accepted,
            Is.False);
        Assert.That(
            run.Submit(new ChooseNodeCommand(null)).Accepted,
            Is.False);

        Assert.That(run.State, Is.EqualTo(RunState.MapState));
        Assert.That(run.CurrentNode, Is.SameAs(entered));
    }

    [Test]
    public void NextNodesAreTheOnesLinkedFromTheCurrentNode()
    {
        RunSession run = StartedRun();
        BaseNode entered = WinFirstNode(run);
        run.Submit(new SkipRewardCommand());

        ChooseNodeDecision choice = PendingNodeChoice(run);

        Assert.That(choice.Floor, Is.EqualTo(1));
        Assert.That(
            choice.Options.Count,
            Is.EqualTo(entered.NodeData.Children.Count));

        foreach (BaseNode option in choice.Options)
        {
            Assert.That(
                entered.NodeData.Children,
                Has.Member(option.NodeData));
        }
    }

    #endregion

    #region Rewards

    [Test]
    public void WinningACombatOffersCardsAndMovesUpAFloor()
    {
        RunSession run = StartedRun();

        WinFirstNode(run);

        Assert.That(run.State, Is.EqualTo(RunState.WinState));
        Assert.That(run.IsOver, Is.False);
        Assert.That(run.CurrentFloor, Is.EqualTo(1));

        CardRewardDecision reward = PendingReward(run);

        Assert.That(reward.Offers.Count, Is.EqualTo(3));
        Assert.That(reward.Offers, Is.Unique);
        Assert.That(reward.Offers, Is.SubsetOf(rig.RewardCards));

        // Asking again shows the same cards; they are rolled once.
        CollectionAssert.AreEqual(
            reward.Offers,
            PendingReward(run).Offers);
        CollectionAssert.AreEqual(reward.Offers, progress.Offered);
    }

    [Test]
    public void TakingARewardAddsTheCardAndReturnsToTheMap()
    {
        RunSession run = StartedRun();
        WinFirstNode(run);

        int deckSize = run.Player.DeckInstance.Count;
        CardData wanted = PendingReward(run).Offers[1];

        CommandResult result = run.Submit(new TakeRewardCommand(wanted));

        Assert.That(result.Accepted, Is.True, result.ToString());
        Assert.That(run.Player.DeckInstance.Count, Is.EqualTo(deckSize + 1));
        Assert.That(run.State, Is.EqualTo(RunState.MapState));

        Assert.That(progress.Obtained.Count, Is.EqualTo(1));
        Assert.That(progress.Obtained[0].Data, Is.SameAs(wanted));
        Assert.That(
            progress.Obtained[0].CurrentOwner,
            Is.SameAs(run.Player));
        CollectionAssert.AreEqual(
            new[] { NodeType.CombatNonElite },
            progress.CompletedNodes);
    }

    [Test]
    public void SkippingARewardLeavesTheDeckAlone()
    {
        RunSession run = StartedRun();
        WinFirstNode(run);

        int deckSize = run.Player.DeckInstance.Count;

        CommandResult result = run.Submit(new SkipRewardCommand());

        Assert.That(result.Accepted, Is.True, result.ToString());
        Assert.That(run.Player.DeckInstance.Count, Is.EqualTo(deckSize));
        Assert.That(progress.Obtained, Is.Empty);
        Assert.That(run.State, Is.EqualTo(RunState.MapState));
    }

    [Test]
    public void CardThatIsNotOnOfferCannotBeTaken()
    {
        RunSession run = StartedRun();
        WinFirstNode(run);

        IReadOnlyList<CardData> offers = PendingReward(run).Offers;
        CardData notOffered = rig.RewardCards.Find(
            card => !Contains(offers, card));

        CommandResult result = run.Submit(new TakeRewardCommand(notOffered));

        Assert.That(result.Accepted, Is.False);
        Assert.That(run.State, Is.EqualTo(RunState.WinState));
        Assert.That(progress.Obtained, Is.Empty);
    }

    [Test]
    public void RewardCommandsAreRejectedOnTheMap()
    {
        RunSession run = StartedRun();

        Assert.That(
            run.Submit(new SkipRewardCommand()).Accepted,
            Is.False);
        Assert.That(
            run.Submit(new TakeRewardCommand(rig.RewardCards[0])).Accepted,
            Is.False);
        Assert.That(run.GetRewardOffers(), Is.Empty);
    }

    #endregion

    #region Ending

    [Test]
    public void LosingACombatEndsTheRun()
    {
        rig.Dispose();
        rig = new RunTestRig(enemyHealth: 500, enemyDamage: 50);

        // A deck the player can never afford, so they only end turns.
        CardData unplayable = rig.CreateCard("Unplayable", 99, 1);
        PlayerUnitData character = rig.CreateCharacter(
            new[] { unplayable, unplayable, unplayable },
            maxHealth: 20);

        RunSession run = rig.CreateRun(5, progress);
        bool? endedWith = null;
        run.Ended += won => endedWith = won;

        run.ChooseCharacter(character);
        run.Submit(new ChooseNodeCommand(PendingNodeChoice(run).Options[0]));
        RunTestRig.PlayCombat(run);

        Assert.That(run.State, Is.EqualTo(RunState.LoseState));
        Assert.That(run.IsOver, Is.True);
        Assert.That(run.Won, Is.False);
        Assert.That(endedWith, Is.False);
        Assert.That(run.PendingDecision, Is.Null);
        CollectionAssert.AreEqual(new[] { false }, progress.RunResults);

        Assert.That(
            run.Submit(new SkipRewardCommand()).Accepted,
            Is.False);
    }

    [Test]
    public void BotPlaysARunToTheEndOfTheMap()
    {
        RunSession run = StartedRun();

        List<string> path = RunTestRig.PlayRun(run);

        // One node per floor, all fifteen floors.
        Assert.That(path.Count, Is.EqualTo(15));
        Assert.That(run.CurrentFloor, Is.EqualTo(15));
        Assert.That(run.Player.IsDead, Is.False);

        // The map has no boss node yet, so the run is not won; there is
        // simply nothing left to choose.
        Assert.That(run.IsOver, Is.False);
        Assert.That(run.PendingDecision, Is.Null);

        // Floors with nothing to play yet were passed through, and reported.
        StringAssert.EndsWith(":Treasure", path[8]);
        StringAssert.EndsWith(":RestSite", path[14]);
        Assert.That(progress.CompletedNodes.Count, Is.EqualTo(15));
        Assert.That(progress.CompletedNodes, Has.Member(NodeType.Treasure));
        Assert.That(progress.CompletedNodes, Has.Member(NodeType.RestSite));

        // Combats were fought, and their card plays reported.
        Assert.That(progress.Played, Is.Not.Empty);
        Assert.That(progress.Obtained, Is.Not.Empty);
        Assert.That(progress.RunResults, Is.Empty);
    }

    [Test]
    public void SameSeedPlaysTheSameRun()
    {
        RunSession first = StartedRun(seed: 31337);
        List<string> firstPath = RunTestRig.PlayRun(first);
        List<string> firstDeck = DeckNames(first);
        int firstHealth = first.Player.CurrentHealth;

        progress = new RunTestRig.RecordingProgress();
        RunSession second = StartedRun(seed: 31337);
        List<string> secondPath = RunTestRig.PlayRun(second);

        CollectionAssert.AreEqual(firstPath, secondPath);
        CollectionAssert.AreEqual(firstDeck, DeckNames(second));
        Assert.That(second.Player.CurrentHealth, Is.EqualTo(firstHealth));
    }

    [Test]
    public void DifferentSeedsPlayDifferentRuns()
    {
        List<string> first = RunTestRig.PlayRun(StartedRun(seed: 1));
        List<string> second = RunTestRig.PlayRun(StartedRun(seed: 2));

        CollectionAssert.AreNotEqual(first, second);
    }

    [Test]
    public void PlayingARunNeverCreatesTheRunManager()
    {
        RunTestRig.PlayRun(StartedRun());

        Assert.That(
            Object.FindObjectsByType<RunManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None),
            Is.Empty);
    }

    #endregion

    private static List<string> DeckNames(RunSession run)
    {
        List<string> names = new List<string>();

        foreach (CardInstance card in run.Player.DeckInstance.CardInstances)
        {
            names.Add(card.CurrentName);
        }

        return names;
    }

    private static bool Contains(IReadOnlyList<CardData> cards, CardData card)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == card) return true;
        }

        return false;
    }
}
