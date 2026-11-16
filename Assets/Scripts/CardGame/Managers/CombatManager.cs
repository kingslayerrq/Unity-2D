using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One combat: its units, its turn flow, the queue of actions being resolved
/// and the decisions it waits on.
///
/// Plain C#. It needs no scene: create one, add units, begin the battle, then
/// read <see cref="PendingDecision"/> and answer with <see cref="Submit"/>.
/// In the game, <see cref="CombatPresenter"/> owns one and shows it on screen.
/// </summary>
public class CombatManager
{
    private readonly CombatContext context;
    private readonly CombatSystem combatSystem;
    private readonly UnitsManager unitsManager = new UnitsManager();
    private readonly CombatActionQueue actionQueue = new CombatActionQueue();

    private int currentTurn;
    private NodeState currentCombatState = NodeState.None;

    private PlayerTurnDecision playerTurnDecision;
    private CardChoiceDecision pendingCardChoice;

    /// <summary>
    /// Raised when combat starts waiting on a decision. The answer does not
    /// have to come from a listener; anyone may call <see cref="Submit"/>.
    /// </summary>
    public event Action<ICombatDecision> DecisionRequested;

    /// <summary>
    /// What combat is waiting for the player to decide. Null while actions are
    /// still resolving towards the next decision, during the enemy turn, and
    /// once the combat is over.
    /// </summary>
    public ICombatDecision PendingDecision
    {
        get
        {
            if (IsCombatOver) return null;
            if (pendingCardChoice != null) return pendingCardChoice;
            if (currentCombatState == NodeState.PlayerAction) return playerTurnDecision;
            return null;
        }
    }


    public event Action<NodeState> OnStateChange;
    public event Action<int> OnPlayerNewTurnStart;
    
    public int CurrentTurn => currentTurn;
    public NodeState CurrentCombatState => currentCombatState;
    public CombatActionQueue ActionQueue => actionQueue;
    public CombatSystem CombatSystem => combatSystem;
    public UnitsManager UnitsManager => unitsManager;

    /// <summary>The source of every random choice made in this combat.</summary>
    public System.Random Rng => context.Rng;
    
    public BasePlayerUnitInstance Player => unitsManager.ActivePlayerUnitInstance;
    public List<BaseEnemyUnitInstance> Enemies => unitsManager.ActiveEnemyUnitInstances;
    public IReadOnlyList<BaseUnitInstance> AllUnits => unitsManager.AllActiveUnits;
    public List<BasePlayerUnitInstance> Players => unitsManager.ActivePlayerUnitInstances;

    private bool IsCombatOver =>
        currentCombatState == NodeState.Victory || currentCombatState == NodeState.Defeat;

    public bool IsEnemyTurn =>
        currentCombatState == NodeState.EnemyTurnStart ||
        currentCombatState == NodeState.EnemyAction ||
        currentCombatState == NodeState.EnemyTurnEnd;

    /// <param name="context">
    /// What the combat is given from outside. Null gives an unseeded combat
    /// that records no progression.
    /// </param>
    public CombatManager(CombatContext context = null)
    {
        this.context = context ?? new CombatContext();
        combatSystem = new CombatSystem(this.context.Progress);

        unitsManager.OnAllEnemiesDefeated += EnterWinState;
        unitsManager.OnAllPlayersDefeated += EnterDefeatState;
    }

    #region Decisions and commands

    /// <summary>
    /// Carries out a player's command if it is a legal answer to
    /// <see cref="PendingDecision"/>. Bots act on a combat only through this
    /// call, and the UI's card plays and card choices come through it too.
    ///
    /// Without a scene attached, the call returns only when combat has
    /// resolved up to its next decision or its end.
    /// </summary>
    public CommandResult Submit(ICombatCommand command)
    {
        if (command == null) return CommandResult.Rejected("No command was given.");
        if (IsCombatOver) return CommandResult.Rejected("The combat is over.");

        switch (command)
        {
            case ChooseCardsCommand choose:
                return SubmitCardChoice(choose);
            case PlayCardCommand play:
                return SubmitPlayCard(play);
            case EndTurnCommand _:
                return SubmitEndTurn();
            default:
                return CommandResult.Rejected($"Unknown command {command.GetType().Name}.");
        }
    }

    private CommandResult SubmitPlayCard(PlayCardCommand play)
    {
        if (pendingCardChoice != null)
            return CommandResult.Rejected("A card choice must be answered first.");
        if (currentCombatState != NodeState.PlayerAction)
            return CommandResult.Rejected("It is not the player's action phase.");
        if (play.Card == null)
            return CommandResult.Rejected("No card was given.");
        if (!CanPlayCard(play.Card))
            return CommandResult.Rejected("That card cannot be played right now.");
        if (!IsLegalTargets(play.Card, play.Targets))
            return CommandResult.Rejected("Those are not legal targets for that card.");

        return PlayCard(play.Card, new List<BaseUnitInstance>(play.Targets))
            ? CommandResult.Ok()
            : CommandResult.Rejected("That card cannot be played right now.");
    }

    private CommandResult SubmitEndTurn()
    {
        if (pendingCardChoice != null)
            return CommandResult.Rejected("A card choice must be answered first.");
        if (currentCombatState != NodeState.PlayerAction)
            return CommandResult.Rejected("It is not the player's action phase.");

        EndPlayerTurn();
        return CommandResult.Ok();
    }

    private CommandResult SubmitCardChoice(ChooseCardsCommand choose)
    {
        CardChoiceDecision decision = pendingCardChoice;
        if (decision == null)
            return CommandResult.Rejected("No card choice is pending.");
        if (!decision.IsValid(choose.Cards, out string reason))
            return CommandResult.Rejected(reason);

        pendingCardChoice = null;
        try
        {
            decision.Resolve(choose.Cards);
        }
        finally
        {
            actionQueue.Resume();
        }

        return CommandResult.Ok();
    }

    /// <summary>
    /// Called by combat logic when the player must pick cards. The queue is
    /// suspended until a <see cref="ChooseCardsCommand"/> answers the choice.
    /// </summary>
    public void RequestCardChoice(CardChoiceDecision decision)
    {
        if (decision == null) return;

        if (decision.MaxCount == 0)
        {
            // Nothing can be picked, so there is nothing to wait for.
            decision.Resolve(Array.Empty<CardInstance>());
            return;
        }

        if (pendingCardChoice != null)
        {
            Debug.LogError("A card choice was requested while another is still pending. Ignoring the new one.");
            return;
        }

        pendingCardChoice = decision;
        actionQueue.Suspend();
        RaiseDecisionRequested(decision);
    }

    private void RaiseDecisionRequested(ICombatDecision decision)
    {
        try
        {
            DecisionRequested?.Invoke(decision);
        }
        catch (Exception exception)
        {
            // A listener that fails to show the decision must not break combat.
            Debug.LogException(exception);
        }
    }

    #endregion

    #region Targeting

    /// <summary>
    /// Every legal way for the active player to target the card right now.
    /// Each entry is the full list of units one play would be aimed at.
    /// </summary>
    public List<IReadOnlyList<BaseUnitInstance>> GetTargetOptions(CardInstance card)
    {
        return CardTargeting.GetTargetOptions(card, Player, unitsManager);
    }

    /// <summary>True when the targets are exactly one of the card's legal options.</summary>
    public bool IsLegalTargets(CardInstance card, IReadOnlyList<BaseUnitInstance> targets)
    {
        return CardTargeting.IsLegal(card, Player, unitsManager, targets);
    }

    #endregion

    #region Battle setup

    /// <summary>
    /// Clears the previous battle and adds the units for a new one. Views, if
    /// there are any, should be bound to the units after this call and before
    /// <see cref="BeginBattle"/>, so they see the opening health and energy.
    /// </summary>
    public void SetUpBattle(
        BasePlayerUnitInstance player,
        CardPileManager cardPiles,
        IReadOnlyList<EnemyUnitData> enemies)
    {
        ResetForBattle();
        AddPlayer(player, cardPiles);

        if (enemies == null) return;

        for (int i = 0; i < enemies.Count; i++)
        {
            AddEnemy(enemies[i]);
        }
    }

    /// <summary>Starts the battle and the first player turn.</summary>
    public void BeginBattle()
    {
        SetState(NodeState.BattleStart);
        EnterPlayerTurnStart();
    }

    /// <summary>
    /// Brings the player into this combat with the given piles. The player
    /// unit lives across combats; its deck is dealt into the piles afresh.
    /// </summary>
    public void AddPlayer(BasePlayerUnitInstance player, CardPileManager cardPiles)
    {
        if (player == null)
        {
            Debug.LogError("Cannot add a null player to the combat.");
            return;
        }

        unitsManager.RegisterExistingPlayerUnit(player);
        player.Initialize(this, cardPiles);
    }

    public BaseEnemyUnitInstance AddEnemy(EnemyUnitData data)
    {
        if (data == null)
        {
            Debug.LogError("Cannot add an enemy without data.");
            return null;
        }

        BaseEnemyUnitInstance enemy = new BaseEnemyUnitInstance(data);
        enemy.Initialize(this);
        unitsManager.AddUnit(enemy);
        return enemy;
    }

    private void ResetForBattle()
    {
        playerTurnDecision = null;
        if (pendingCardChoice != null)
        {
            // A choice left over from an abandoned combat would keep the
            // queue suspended forever.
            pendingCardChoice = null;
            actionQueue.Clear();
            actionQueue.Resume();
        }

        currentTurn = 0;
        SetState(NodeState.BattleSetup);
        unitsManager.Clear();
    }

    #endregion

    public void EnterPlayerTurnStart()
    {
        if (IsCombatOver || Player == null) return;

        SetState(NodeState.PlayerTurnStart);
        
        // Determine enemy intent
        DetermineEnemiesIntents();
        
        
        currentTurn++;
        OnPlayerNewTurnStart?.Invoke(currentTurn);
        
        Player.ResetEnergy();
        Player.BuffManager.OnTurnStart(Player, combatSystem);
        
        // Only the block of the player expires here. Enemy block lasts through
        // the player turn and expires when the enemy turn starts.
        actionQueue.Enqueue(new ResetBlockAction(combatSystem, Player, new List<BaseUnitInstance> { Player },
            BlockResetSource.TurnStart), null);
        actionQueue.Enqueue(new DrawAction(combatSystem, Player, new List<BasePlayerUnitInstance>{Player}, 
                Player.CurrentDrawAmount),
            OnPlayerTurnStartResolved);
        
    }

    private void OnPlayerTurnStartResolved()
    {
        if (IsCombatOver || Player == null) return;

        actionQueue.Publish(new HandChangedEvent(Player));
        EnterPlayerAction();
    }

    public void EnterPlayerAction()
    {
        SetState(NodeState.PlayerAction);

        playerTurnDecision = new PlayerTurnDecision(this, Player, currentTurn);
        RaiseDecisionRequested(playerTurnDecision);
    }

    /// <summary>
    /// True when the card is in the hand of the player, it is the player action
    /// phase, no card choice is waiting, and the player can pay for it right now.
    /// </summary>
    public bool CanPlayCard(CardInstance card)
    {
        if (card == null || Player == null) return false;
        if (currentCombatState != NodeState.PlayerAction) return false;
        if (pendingCardChoice != null) return false;
        if (!card.CanBePlayed) return false;

        IReadOnlyList<CardInstance> hand = Player.Hand.Cards;
        for (int i = 0; i < hand.Count; i++)
        {
            if (ReferenceEquals(hand[i], card)) return true;
        }

        return false;
    }

    /// <summary>
    /// Pays for the card and queues it. Energy is spent immediately, not in a
    /// queued action, so a card played while earlier actions are still
    /// animating is checked against the real remaining energy.
    /// </summary>
    /// <returns>False if the card could not be played; nothing is spent.</returns>
    public bool PlayCard(CardInstance card, List<BaseUnitInstance> targets)
    {
        if (!CanPlayCard(card)) return false;

        // The targets must be one of the card's legal options. Refuse
        // anything else before any energy is spent.
        if (!IsLegalTargets(card, targets)) return false;

        combatSystem.ResolveSpendEnergy(Player, new List<BasePlayerUnitInstance> { Player }, card.CurrentCost);
        actionQueue.Enqueue(new PlayCardAction(combatSystem, Player, targets, card), null);
        return true;
    }

    public void EndPlayerTurn()
    {
        if (currentCombatState != NodeState.PlayerAction || Player == null) return;
        if (pendingCardChoice != null) return;

        playerTurnDecision = null;
        SetState(NodeState.PlayerTurnEnd);
        
        // remove the card from player's hand
        actionQueue.Enqueue(new DiscardAction(combatSystem, Player, 
            new List<BasePlayerUnitInstance>{Player}, Player.Hand.Cards, CardMoveCause.EndTurn), null);
        
        // Only the player side ends its turn here. Enemies end theirs after
        // they have acted, in EnterEnemyTurnEnd.
        actionQueue.Enqueue(new EndTurnAction(combatSystem, Player, new List<BaseUnitInstance>(Players)), null);

        EnqueuePhaseTransition("EnemyTurnStart", EnterEnemyTurnStart);
    }

    /// <summary>
    /// Runs <paramref name="next"/> once everything queued so far, and anything
    /// those actions queue in turn (damage, defeats, animations), has resolved.
    /// Does nothing if the combat ended in the meantime.
    /// </summary>
    private void EnqueuePhaseTransition(string label, Action next)
    {
        actionQueue.Enqueue(new PhaseTransitionAction(label), () =>
        {
            if (IsCombatOver) return;

            if (actionQueue.Count > 0)
            {
                // Follow-up actions were queued behind this marker; wait for them.
                EnqueuePhaseTransition(label, next);
                return;
            }

            next();
        });
    }

    private void EnterEnemyTurnStart()
    {
        if (IsCombatOver || Player == null) return;

        SetState(NodeState.EnemyTurnStart);

        for (int i = 0; i < Enemies.Count; i++)
        {
            BaseEnemyUnitInstance enemy = Enemies[i];
            if (enemy.IsDead) continue;

            enemy.BuffManager?.OnTurnStart(enemy, combatSystem);
        }

        // Block the enemies gained on their previous turn expires now.
        actionQueue.Enqueue(new ResetBlockAction(combatSystem, Player, new List<BaseUnitInstance>(Enemies),
            BlockResetSource.TurnStart), null);

        RunEnemyTurn();
    }

    private void DetermineEnemiesIntents()
    {
        foreach (var enemy in Enemies)
        {
            if (enemy.IsDead) continue;
             CombatSnapshot snapshot = new CombatSnapshot(enemy, Enemies, Players, currentTurn, Rng);
             
             enemy.DetermineIntent(snapshot);
        }
    }

    private void RunEnemyTurn()
    {
        SetState(NodeState.EnemyAction);
        Debug.Log("Enemy Turn Action");
        for (int i = 0; i < Enemies.Count; i++)
        {
            actionQueue.Enqueue(new ExecuteEnemyIntentAction(combatSystem, Enemies[i], null), null);
        }

        // Intents only queue their effects, so the turn must not end until
        // those effects (and any defeats they cause) have resolved.
        EnqueuePhaseTransition("EnemyTurnEnd", EnterEnemyTurnEnd);
    }

    private void EnterEnemyTurnEnd()
    {
        if (IsCombatOver || Player == null) return;

        SetState(NodeState.EnemyTurnEnd);

        if (Enemies.Count > 0)
        {
            actionQueue.Enqueue(new EndTurnAction(combatSystem, Enemies[0], new List<BaseUnitInstance>(Enemies)),
                null);
        }

        EnqueuePhaseTransition("RoundEnd", EnterRoundEnd);
    }

    private void EnterRoundEnd()
    {
        if (IsCombatOver || Player == null) return;

        // Timed buffs such as Vulnerable count down once per round, after
        // both sides have acted.
        combatSystem.ResolveEndRound(new List<BaseUnitInstance>(AllUnits));

        EnqueuePhaseTransition("PlayerTurnStart", EnterPlayerTurnStart);
    }

    private void BattleCleanup()
    {
        currentTurn = 0;
    }

    private void EnterWinState()
    {
        if (IsCombatOver) return;
        BattleCleanup();
        SetState(NodeState.Victory);
    }

    private void EnterDefeatState()
    {
        if (IsCombatOver) return;
        BattleCleanup();
        SetState(NodeState.Defeat);
    }

    private void SetState(NodeState newState)
    {
        currentCombatState = newState;
        OnStateChange?.Invoke(currentCombatState);
    }

    /// <summary>Empty queue marker used to sequence turn phases.</summary>
    private sealed class PhaseTransitionAction : ICombatAction
    {
        private readonly string _label;

        public PhaseTransitionAction(string label)
        {
            _label = label;
        }

        public void Execute()
        {
        }

        public string GetActionDetails() => $"PhaseTransition: {_label}";
    }

    public enum NodeState
    {
        None,
        BattleSetup,
        BattleStart,
        PlayerTurnStart,
        PlayerAction,
        PlayerTurnEnd,
        EnemyTurnStart,
        EnemyAction,
        EnemyTurnEnd,
        Victory,
        Defeat
    }
}
