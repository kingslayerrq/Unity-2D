using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatSystem combatSystem;
    [SerializeField] private UnitsManager unitsManager;
    
    public BaseCombatNode currentCombatNode;

    [SerializeField] private int currentTurn;
    [SerializeField] private NodeState currentCombatState = NodeState.None;
    
    [Header("Canvas UI")]
    [SerializeField] private PlayerHandView playerHandView;
    [SerializeField] private EndTurnButton endTurnButton;
    [SerializeField] private UnitEnergyView unitEnergyView;
    
    private CombatActionQueue actionQueue = new CombatActionQueue();


    public event Action<NodeState> OnStateChange;
    public event Action<int> OnPlayerNewTurnStart;
    
    public PlayerHandView PlayerHandView => playerHandView;
    public int CurrentTurn => currentTurn;
    public NodeState CurrentCombatState => currentCombatState;
    public CombatActionQueue ActionQueue => actionQueue;
    public CombatSystem CombatSystem => combatSystem;
    public UnitsManager UnitsManager => unitsManager;
    
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

    private void Awake()
    {
        RunManager.Instance.RegisterCombatManager(this);
        if (combatSystem == null)
        {
            Debug.LogError("Combat system isn't set");
            return;
        }
        if (unitsManager == null)
        {
            Debug.LogError("Units Manager isn't set");
            return;
        }

        unitsManager.OnAllEnemiesDefeated += EnterWinState;
        unitsManager.OnAllPlayersDefeated += EnterDefeatState;

    }

    public void StartBattle(BaseCombatNode combatNode)
    {
        SetState(NodeState.BattleSetup);

        unitsManager.RegisterExistingPlayerUnit(RunManager.Instance.PlayerUnitInstance);
        unitsManager.SpawnAndBindPlayerView();
        
        if (endTurnButton != null) endTurnButton.Initialize(Player);
        if (unitEnergyView != null) unitEnergyView.Initialize(Player);

        IReadOnlyList<EnemyUnitData> enemyUnitDatas = combatNode.EnemyUnitDatas;

        unitsManager.CreateAndSpawnEnemyUnits(enemyUnitDatas);

        SetState(NodeState.BattleStart);
        EnterPlayerTurnStart();
    }

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

        playerHandView.UpdateHand();
        playerHandView.RefreshHand(Player.Hand.Cards);
        EnterPlayerAction();
    }

    public void EnterPlayerAction()
    {
        SetState(NodeState.PlayerAction);
        
        
    }

    /// <summary>
    /// True when the card is in the hand of the player, it is the player action
    /// phase, and the player can pay for it right now.
    /// </summary>
    public bool CanPlayCard(CardInstance card)
    {
        if (card == null || Player == null) return false;
        if (currentCombatState != NodeState.PlayerAction) return false;
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

        bool toDiscard = !card.ShouldExhaustAfterPlay() && !card.IsPower;
        combatSystem.ResolveSpendEnergy(Player, new List<BasePlayerUnitInstance> { Player }, card.CurrentCost);
        actionQueue.Enqueue(new PlayCardAction(combatSystem, Player, targets, card, toDiscard), null);
        return true;
    }

    public void EndPlayerTurn()
    {
        if (currentCombatState != NodeState.PlayerAction || Player == null) return;

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
             CombatSnapshot snapshot = new CombatSnapshot(enemy, Enemies, Players, currentTurn);
             
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

    private void OnDestroy()
    {
        if (unitsManager != null)
        {
            unitsManager.OnAllEnemiesDefeated -= EnterWinState;
            unitsManager.OnAllPlayersDefeated -= EnterDefeatState;
        }
    }


    /// <summary>Empty queue marker used to sequence turn phases.</summary>
    private sealed class PhaseTransitionAction : ICombatAction
    {
        private readonly string _label;

        public PhaseTransitionAction(string label)
        {
            _label = label;
        }

        public IEnumerator ExecuteAction()
        {
            yield break;
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
