using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatSystem combatSystem;
    [SerializeField] private UnitsManager unitsManager;
    
    public BaseCombatNode currentCombatNode;

    [SerializeField] private int currentTurn;
    [SerializeField] private CombatState currentCombatState = CombatState.None;
    
    [Header("Canvas UI")]
    [SerializeField] private PlayerHandView playerHandView;
    [SerializeField] private EndTurnButton endTurnButton;
    
    private CombatActionQueue actionQueue = new CombatActionQueue();


    public event Action<CombatState> OnStateChange;
    public event Action<int> OnPlayerNewTurnStart;
    
    public PlayerHandView PlayerHandView => playerHandView;
    public int CurrentTurn => currentTurn;
    public CombatState CurrentCombatState => currentCombatState;
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
        SetState(CombatState.BattleSetup);

        unitsManager.RegisterExistingPlayerUnit(RunManager.Instance.PlayerUnitInstance);
        unitsManager.SpawnAndBindPlayerView();
        
        if (endTurnButton != null) endTurnButton.Initialize(Player);

        IReadOnlyList<EnemyUnitData> enemyUnitDatas = combatNode.EnemyUnitDatas;

        unitsManager.CreateAndSpawnEnemyUnits(enemyUnitDatas);

        SetState(CombatState.BattleStart);
        EnterPlayerTurnStart();
    }

    public void EnterPlayerTurnStart()
    {
        SetState(CombatState.PlayerTurnStart);
        
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
        SetState(CombatState.PlayerAction);
        
        
    }

    // Spends Energy and Play the card wrapper
    public void PlayCard(CardInstance card, List<BaseUnitInstance> targets)
    {
        actionQueue.Enqueue(new SpendEnergyAction(combatSystem, Player, new List<BasePlayerUnitInstance>{Player}, card.CurrentCost), null);
        actionQueue.Enqueue(new PlayCardAction(combatSystem, Player, targets, card), null);
    }

    public void EndPlayerTurn()
    {
        SetState(CombatState.PlayerTurnEnd);
        
        // remove the card from player's hand
        actionQueue.Enqueue(new DiscardAction(combatSystem, Player, new List<BasePlayerUnitInstance>{Player}, Player.Hand.Cards), null);
        
        actionQueue.Enqueue(new EndTurnAction(combatSystem, Player, AllUnits), null);
        
        EnterEnemyTurnStart();
    }
    

    private void EnterEnemyTurnStart()
    {
        SetState(CombatState.EnemyTurnStart);
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
        SetState(CombatState.EnemyAction);
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

    private void EnterWinState()
    {
        // if (actionQueue != null)
        // {
        //     actionQueue.Clear();
        // }
        SetState(CombatState.Victory);
    }

    private void EnterDefeatState()
    {
        // if (actionQueue != null)
        // {
        //     actionQueue.Clear();
        // }
        SetState(CombatState.Defeat);
    }

    private void SetState(CombatState newState)
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


    public enum CombatState
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
