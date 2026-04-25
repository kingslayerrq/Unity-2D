using System;
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
        SetState(NodeState.PlayerTurnStart);
        
        // Determine enemy intent
        DetermineEnemiesIntents();
        
        
        currentTurn++;
        OnPlayerNewTurnStart?.Invoke(currentTurn);
        
        Player.ResetEnergy();
        Player.BuffManager.OnTurnStart(Player, combatSystem);
        
        actionQueue.Enqueue(new ResetBlockAction(combatSystem, Player, AllUnits, BlockResetSource.TurnStart), null);
        actionQueue.Enqueue(new DrawAction(combatSystem, Player, new List<BasePlayerUnitInstance>{Player}, 
                Player.CurrentDrawAmount),
            OnPlayerTurnStartResolved);
        
    }

    private void OnPlayerTurnStartResolved()
    {
        playerHandView.UpdateHand();
        playerHandView.RefreshHand(Player.Hand.Cards);
        EnterPlayerAction();
    }

    public void EnterPlayerAction()
    {
        SetState(NodeState.PlayerAction);
        
        
    }

    // Spends Energy and Play the card wrapper
    public void PlayCard(CardInstance card, List<BaseUnitInstance> targets)
    {
        bool toDiscard = !card.IsExhaustOnPlay();
        actionQueue.Enqueue(new SpendEnergyAction(combatSystem, Player, new List<BasePlayerUnitInstance>{Player}, card.CurrentCost), null);
        actionQueue.Enqueue(new PlayCardAction(combatSystem, Player, targets, card, toDiscard), null);
    }

    public void EndPlayerTurn()
    {
        SetState(NodeState.PlayerTurnEnd);
        
        // remove the card from player's hand
        actionQueue.Enqueue(new DiscardAction(combatSystem, Player, new List<BasePlayerUnitInstance>{Player}, Player.Hand.Cards), null);
        
        actionQueue.Enqueue(new EndTurnAction(combatSystem, Player, AllUnits), null);
        
        EnterEnemyTurnStart();
    }
    

    private void EnterEnemyTurnStart()
    {
        SetState(NodeState.EnemyTurnStart);
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
        for( int i = 0; i < Enemies.Count; i++)
        {
            if (i == Enemies.Count - 1)
            {
                actionQueue.Enqueue(new ExecuteEnemyIntentAction(combatSystem, Enemies[i], null), EnterPlayerTurnStart);
            }
            else actionQueue.Enqueue(new ExecuteEnemyIntentAction(combatSystem, Enemies[i], null), null);
        }
    }

    private void BattleCleanup()
    {
        currentTurn = 0;
    }

    private void EnterWinState()
    {
        BattleCleanup();
        SetState(NodeState.Victory);
    }

    private void EnterDefeatState()
    {
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
