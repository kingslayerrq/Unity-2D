using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatSystem combatSystem;
    [SerializeField] private UnitInstance player;
    [SerializeField] private UnitInstance enemy;
    [SerializeField] private CombatActionQueue actionQueue = new CombatActionQueue();
    private List<UnitInstance> _players = new List<UnitInstance>();
    
    public int CurrentTurn { get; private set; } = 0;
    public CombatState currentCombatState { get; private set; } =  CombatState.None;
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
        
        // TODO: UI setup
        
        EnterPlayerTurnStart();
    }

    public void EnterPlayerTurnStart()
    {
        SetState(CombatState.PlayerTurnStart);
        
        player.ResetEnergy();
        player.BuffManager.OnTurnStart(player, combatSystem);
        
        actionQueue.Enqueue(new ResetBlockAction(combatSystem, player, _players, BlockResetSource.TurnStart), null);
        actionQueue.Enqueue(new DrawAction(combatSystem, player, _players, player.CurrentDrawAmount),
            OnPlayerTurnStartResolved);
        
    }

    private void OnPlayerTurnStartResolved()
    {
        Debug.Log("Player Turn Start Resolved");
        player.UnitUIManager.PlayerHandView.UpdateHand();
        player.UnitUIManager.PlayerHandView.RefreshHand(player.Hand.Cards);
        EnterPlayerAction();
    }

    public void EnterPlayerAction()
    {
        SetState(CombatState.PlayerAction);
        
    }

    // Spends Energy and Play the card
    public void PlayCard(CardInstance card, List<UnitInstance> targets)
    {
        actionQueue.Enqueue(new SpendEnergyAction(combatSystem, player, new List<UnitInstance>{player}, card.CurrentCost), null);
        actionQueue.Enqueue(new PlayCardAction(combatSystem, player, targets, card), null);
    }

    public void EndPlayerTurn()
    {
        // remove the card from player's hand
        //actionQueue.Enqueue(new DiscardAction(combatSystem, player, _players, player.Hand.Cards);
    }

    private void ResolvePlayerCard(CardInstance card, UnitInstance target) { }
    private void EnterEnemyTurnStart() { }
    private void RunEnemyTurn() { }
    private void CheckBattleEnd() { }
    private void SetState(CombatState newState) { }

    
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
