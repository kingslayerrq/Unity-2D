using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One run of the game, from the seed to the win or loss: the character, the
/// map, which node the player is on, the combat in progress and the reward
/// that follows it.
///
/// Plain C#. It needs no scene: create one, choose a character, then read
/// <see cref="PendingDecision"/> and answer with <see cref="Submit"/>, playing
/// each combat through <see cref="Combat"/>. In the game,
/// <see cref="RunManager"/> owns one and shows it on screen.
/// </summary>
public sealed class RunSession
{
    private readonly IRunProgressRecorder progress;

    // The cards on offer for the node just won. Null until first asked for.
    private List<CardData> rewardOffers;
    private bool rewardTaken;
    private bool nodeCompletionRecorded;

    public RunConfig Config { get; }
    public int Seed { get; }

    public System.Random MapRng { get; }
    public System.Random LootRng { get; }
    public System.Random EventRng { get; }
    public System.Random CombatRng { get; }
    public System.Random ShopRng { get; }

    public RunState State { get; private set; } = RunState.None;

    public PlayerUnitData Character { get; private set; }
    public BasePlayerUnitInstance Player { get; private set; }

    public RunMap Map { get; private set; }
    public int CurrentAct { get; private set; }
    public int CurrentFloor { get; private set; }
    public BaseNode CurrentNode { get; private set; }

    /// <summary>
    /// The combat of this run. One is reused for every battle; it is set up
    /// again each time a combat node is entered.
    /// </summary>
    public CombatManager Combat { get; }

    /// <summary>The player's piles, dealt afresh for each battle.</summary>
    public CardPileManager CardPiles { get; } = new CardPileManager();

    public CardRewardGenerator Rewards { get; } = new CardRewardGenerator();

    /// <summary>When set, rewards draw from every class's cards.</summary>
    public bool AllowCardClassOverride { get; set; }

    /// <summary>True once the run has been won or lost.</summary>
    public bool IsOver { get; private set; }
    public bool Won { get; private set; }

    public event Action<RunState> StateChanged;
    public event Action<BasePlayerUnitInstance> PlayerCreated;

    /// <summary>
    /// Raised when a battle's units exist but it has not begun. Anything
    /// showing the battle should bind to the units now.
    /// </summary>
    public event Action<CombatManager> BattleSetUp;

    /// <summary>Raised once, when the run is won (true) or lost (false).</summary>
    public event Action<bool> Ended;

    /// <param name="progress">Where progression is reported. Null records nothing.</param>
    public RunSession(int seed, RunConfig config, IRunProgressRecorder progress = null)
    {
        Seed = seed;
        Config = config ?? new RunConfig();
        this.progress = progress;

        MapRng = new System.Random(SeedUtility.DeriveSeed(seed, RngCategory.Map));
        LootRng = new System.Random(SeedUtility.DeriveSeed(seed, RngCategory.Loot));
        EventRng = new System.Random(SeedUtility.DeriveSeed(seed, RngCategory.Event));
        CombatRng = new System.Random(SeedUtility.DeriveSeed(seed, RngCategory.Combat));
        ShopRng = new System.Random(SeedUtility.DeriveSeed(seed, RngCategory.Shop));

        Combat = new CombatManager(new CombatContext(CombatRng, progress));
        Combat.OnStateChange += HandleCombatStateChange;
    }

    #region Decisions and commands

    /// <summary>
    /// What the run is waiting for the player to decide. Null during a
    /// combat, once the run is over, and when the map has no node left to
    /// enter.
    /// </summary>
    public IRunDecision PendingDecision
    {
        get
        {
            if (IsOver) return null;

            if (State == RunState.MapState)
            {
                List<BaseNode> options = GetAvailableNodes();
                return options.Count == 0 ? null : new ChooseNodeDecision(CurrentFloor, options);
            }

            if (State == RunState.WinState)
            {
                return new CardRewardDecision(GetRewardOffers());
            }

            return null;
        }
    }

    /// <summary>
    /// Carries out a command if it is a legal answer to
    /// <see cref="PendingDecision"/>. A rejected command changes nothing.
    /// </summary>
    public CommandResult Submit(IRunCommand command)
    {
        if (command == null) return CommandResult.Rejected("No command was given.");
        if (IsOver) return CommandResult.Rejected("The run is over.");

        switch (command)
        {
            case ChooseNodeCommand choose:
                if (State != RunState.MapState)
                    return CommandResult.Rejected("The run is not waiting for a node to be chosen.");
                if (!IsAvailable(choose.Node))
                    return CommandResult.Rejected("That node cannot be entered from here.");

                return EnterNode(choose.Node)
                    ? CommandResult.Ok()
                    : CommandResult.Rejected("That node cannot be entered from here.");

            case TakeRewardCommand take:
                if (State != RunState.WinState)
                    return CommandResult.Rejected("There is no reward to take.");
                if (!TakeReward(take.Card))
                    return CommandResult.Rejected("That card is not on offer.");

                CompleteNode();
                return CommandResult.Ok();

            case SkipRewardCommand _:
                if (State != RunState.WinState)
                    return CommandResult.Rejected("There is no reward to skip.");

                CompleteNode();
                return CommandResult.Ok();

            default:
                return CommandResult.Rejected($"Unknown command {command.GetType().Name}.");
        }
    }

    #endregion

    #region Character and map

    /// <summary>Creates the player for this run and moves on to the map.</summary>
    public void ChooseCharacter(PlayerUnitData character)
    {
        if (character == null)
        {
            Debug.LogError("Cannot start a run without a character.");
            return;
        }

        SetState(RunState.SelectPlayerUnitState);

        Character = character;
        Player = new BasePlayerUnitInstance(character);
        PlayerCreated?.Invoke(Player);

        SetState(RunState.SetupActState);
        EnterMap();
    }

    /// <summary>
    /// True when the node is one the player may enter next: on the current
    /// floor and, after the first, linked from the node they are on.
    /// </summary>
    public bool IsAvailable(BaseNode node)
    {
        if (node == null || node.NodeData == null) return false;
        if (CurrentAct != node.Act || CurrentFloor != node.Floor) return false;
        if (CurrentNode == null) return node.NodeData.IsReachable;
        return CurrentNode.NodeData.Children.Contains(node.NodeData);
    }

    /// <summary>The nodes the player may enter from the map right now.</summary>
    public List<BaseNode> GetAvailableNodes()
    {
        List<BaseNode> nodes = new List<BaseNode>();
        if (Map == null || State != RunState.MapState) return nodes;

        foreach (MapNodeData data in Map.GetReachableNodes(CurrentFloor))
        {
            if (data.RuntimeNode != null && IsAvailable(data.RuntimeNode))
            {
                nodes.Add(data.RuntimeNode);
            }
        }

        return nodes;
    }

    private void EnterMap()
    {
        EnsureMap();
        SetState(RunState.MapState);
    }

    private void EnsureMap()
    {
        if (Map != null) return;

        if (!Config.Map.IsValid || Config.NodeLibrary == null)
        {
            Debug.LogError("Cannot build the run's map: its size or its node library has not been set.");
            return;
        }

        Map = MapBuilder.Build(Seed, Config.Map, Config.NodeLibrary);
    }

    #endregion

    #region Nodes

    /// <summary>
    /// Enters a node from the map. A combat node starts its battle; a node
    /// type with nothing to play yet is completed at once.
    /// </summary>
    /// <returns>False when the node cannot be entered; nothing changes.</returns>
    public bool EnterNode(BaseNode node)
    {
        if (IsOver || Player == null) return false;
        if (State != RunState.MapState || !IsAvailable(node)) return false;

        CurrentNode = node;
        rewardOffers = null;
        rewardTaken = false;
        nodeCompletionRecorded = false;

        // On enter setup
        node.OnEnter();

        // An Unknown node picks its real content in OnEnter, so route on the
        // resolved node rather than on the map node itself.
        BaseNode resolvedNode = node.ResolvedNode;

        if (resolvedNode is BaseCombatNode combatNode)
        {
            SetState(RunState.CombatState);

            Combat.SetUpBattle(Player, CardPiles, combatNode.EnemyUnitDatas);
            BattleSetUp?.Invoke(Combat);
            Combat.BeginBattle();
            return true;
        }

        // Rest sites, merchants, treasure and events have nothing to play
        // yet. Complete them immediately so the run cannot get stuck.
        Debug.LogWarning($"Node type {resolvedNode.Type} is not implemented yet; skipping it.");
        CurrentFloor += 1;
        FinishNode();
        return true;
    }

    /// <summary>
    /// Leaves the node that was just won and returns to the map. In the game
    /// this is the Continue button.
    /// </summary>
    /// <returns>False when there is no won node to leave.</returns>
    public bool CompleteNode()
    {
        if (CurrentNode == null || State != RunState.WinState) return false;

        FinishNode();
        return true;
    }

    private void FinishNode()
    {
        CurrentNode.OnExit();
        RecordNodeCompleted();

        rewardOffers = null;
        EnterMap();
    }

    private void HandleCombatStateChange(CombatManager.NodeState combatState)
    {
        if (State != RunState.CombatState) return;

        if (combatState == CombatManager.NodeState.Victory)
        {
            bool isBoss =
                CurrentNode != null &&
                CurrentNode.ResolvedNode.Type == NodeType.CombatBoss;

            SetState(RunState.WinState);

            if (isBoss)
            {
                RecordNodeCompleted();
                End(true);
            }
            else
            {
                CurrentFloor += 1;
            }
        }
        else if (combatState == CombatManager.NodeState.Defeat)
        {
            SetState(RunState.LoseState);
            End(false);
        }
    }

    #endregion

    #region Rewards

    /// <summary>
    /// True while a combat has just been won and its card reward has not
    /// been taken.
    /// </summary>
    public bool CanClaimReward =>
        State == RunState.WinState &&
        !rewardTaken &&
        Player != null &&
        CurrentNode != null &&
        CurrentNode.ResolvedNode is BaseCombatNode;

    /// <summary>
    /// The cards on offer for the combat just won. They are rolled the first
    /// time this is asked and stay the same until the node is left.
    /// </summary>
    public IReadOnlyList<CardData> GetRewardOffers()
    {
        if (!CanClaimReward) return Array.Empty<CardData>();

        if (rewardOffers == null)
        {
            bool isElite = CurrentNode.ResolvedNode.Type == NodeType.CombatElite;

            rewardOffers = Rewards.Generate(
                Player.PlayerUnitData.PlayerCardClass,
                Config.CardRewardAmount,
                isElite,
                AllowCardClassOverride,
                LootRng);

            List<CardData> offered = rewardOffers;
            Report(recorder => recorder.RecordCardsOffered(offered));
        }

        return rewardOffers;
    }

    /// <summary>Adds one of the offered cards to the player's deck.</summary>
    /// <returns>False when the card is not on offer; nothing changes.</returns>
    public bool TakeReward(CardData card)
    {
        if (card == null || !CanClaimReward) return false;
        if (rewardOffers == null || !rewardOffers.Contains(card)) return false;
        if (Player.DeckInstance == null) return false;

        CardInstance instance = new CardInstance(card, Player);
        Player.DeckInstance.AddCardToDeck(instance);
        rewardTaken = true;

        Report(recorder => recorder.RecordCardObtained(instance));
        return true;
    }

    #endregion

    #region Ending

    /// <summary>
    /// Ends the run as won or lost and reports it. Does nothing if the run
    /// has already ended.
    /// </summary>
    public void End(bool won)
    {
        if (IsOver) return;

        IsOver = true;
        Won = won;

        Report(recorder => recorder.RecordRunCompleted(Character, won));
        Ended?.Invoke(won);
    }

    private void RecordNodeCompleted()
    {
        if (nodeCompletionRecorded || CurrentNode == null) return;

        nodeCompletionRecorded = true;

        BaseNode completed = CurrentNode.ResolvedNode;
        Report(recorder => recorder.RecordNodeCompleted(Character, completed));
    }

    /// <summary>
    /// Reports to the recorder, if there is one. A recorder that fails is
    /// logged and never interrupts the run.
    /// </summary>
    private void Report(Action<IRunProgressRecorder> report)
    {
        if (progress == null) return;

        try
        {
            report(progress);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void SetState(RunState newState)
    {
        State = newState;
        StateChanged?.Invoke(newState);
    }

    #endregion
}
