using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatSystem combatSystem;
    [SerializeField] private BasePlayerUnitInstance player;
    [SerializeField] private BaseEnemyUnitInstance enemy;
    [SerializeField] private int currentTurn;
    [SerializeField] private CombatState currentCombatState = CombatState.None;
    private CombatActionQueue actionQueue = new CombatActionQueue();
    private List<BasePlayerUnitInstance> _players = new List<BasePlayerUnitInstance>();
    private List<BaseEnemyUnitInstance> _enemies = new List<BaseEnemyUnitInstance>();
    private List<BaseUnitInstance> _allUnits = new List<BaseUnitInstance>();

    public event Action<CombatState> OnStateChange;
    public event Action<int> OnPlayerNewTurnStart;
    
    public int CurrentTurn => currentTurn;
    public CombatState CurrentCombatState => currentCombatState;
    public CombatActionQueue ActionQueue => actionQueue;
    public CombatSystem CombatSystem => combatSystem;

    private void Awake()
    {
        if (combatSystem == null)
        {
            Debug.LogError("Combat system isn't set");
            return;
        }
    }

    public void StartBattle()
    {
        SetState(CombatState.BattleStart);
        
        player.Initialize();
        enemy.Initialize();
        
        _players.Clear();
        _players.Add(player);
        
        _enemies.Clear();
        _enemies.Add(enemy);
        
        _allUnits.Clear();
        _allUnits.AddRange(_players);
        _allUnits.AddRange(_enemies);
        
        // TODO: UI setup
        
        EnterPlayerTurnStart();
    }

    public void EnterPlayerTurnStart()
    {
        SetState(CombatState.PlayerTurnStart);
        
        currentTurn++;
        OnPlayerNewTurnStart?.Invoke(currentTurn);
        
        player.ResetEnergy();
        player.BuffManager.OnTurnStart(player, combatSystem);
        
        actionQueue.Enqueue(new ResetBlockAction(combatSystem, player, _allUnits, BlockResetSource.TurnStart), null);
        actionQueue.Enqueue(new DrawAction(combatSystem, player, _players, player.CurrentDrawAmount),
            OnPlayerTurnStartResolved);
        
    }

    private void OnPlayerTurnStartResolved()
    {
        player.UnitUIManager.PlayerHandView.UpdateHand();
        player.UnitUIManager.PlayerHandView.RefreshHand(player.Hand.Cards);
        EnterPlayerAction();
    }

    public void EnterPlayerAction()
    {
        SetState(CombatState.PlayerAction);
        
        
    }

    // Spends Energy and Play the card wrapper
    public void PlayCard(CardInstance card, List<BaseUnitInstance> targets)
    {
        if (combatSystem == null)
        {
            bool isPrefab = gameObject.scene.name == null;
            Debug.LogError($"CombatSystem is NULL during PlayCard! " +
                           $"Am I a prefab? {isPrefab}. " +
                           $"My GameObject name is: {gameObject.name}");
            return; 
        }
        actionQueue.Enqueue(new SpendEnergyAction(combatSystem, player, new List<BasePlayerUnitInstance>{player}, card.CurrentCost), null);
        actionQueue.Enqueue(new PlayCardAction(combatSystem, player, targets, card), null);
        // actionQueue.Enqueue(new PlayToDiscardAction(combatSystem, player, targets, card), null);
    }

    public void EndPlayerTurn()
    {
        SetState(CombatState.PlayerTurnEnd);
        
        // remove the card from player's hand
        actionQueue.Enqueue(new DiscardAction(combatSystem, player, _players, player.Hand.Cards), null);
        
        actionQueue.Enqueue(new EndTurnAction(combatSystem, player, _allUnits), null);
        
        EnterEnemyTurnStart();
    }
    

    private void EnterEnemyTurnStart()
    {
        SetState(CombatState.EnemyTurnStart);
        RunEnemyTurn();
    }

    private void RunEnemyTurn()
    {
        SetState(CombatState.EnemyAction);
        Debug.Log("Enemy Turn Action");
        EnterPlayerTurnStart();
    }
    private void CheckBattleEnd() { }

    private void SetState(CombatState newState)
    {
        currentCombatState = newState;
        OnStateChange?.Invoke(currentCombatState);
    }

    
    public enum CombatState
    {
        None,
        BattleSetup,
        BattleStart,
        PlayerTurnStart,
        PlayerAction,
        PlayerTargeting,
        ResolvingAction,
        PlayerTurnEnd,
        EnemyTurnStart,
        EnemyAction,
        EnemyTurnEnd,
        CheckBattleEnd,
        Victory,
        Defeat
    }
}
