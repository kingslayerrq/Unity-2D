using System;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatSystem combatSystem;
    [SerializeField] private UnitInstance player;
    [SerializeField] private UnitInstance enemy;
    [SerializeField] private CombatActionQueue actionQueue = new CombatActionQueue();
    
    public int CurrentTurn { get; private set; } = 0;
    public CombatState currentCombatState { get; private set; } =  CombatState.None;
    
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
        
        // TODO: UI setup
        
        EnterPlayerTurnStart();
    }

    public void EnterPlayerTurnStart()
    {
        SetState(CombatState.PlayerTurnStart);
        
        player.ResetEnergy();
        player.BuffManager.OnTurnStart(player, combatSystem);
        
        actionQueue.Enqueue(new ResetBlockAction(combatSystem, player, player, BlockResetSource.TurnStart));
        actionQueue.Enqueue(new DrawAction(combatSystem, player, player, player.CurrentDrawAmount));
        actionQueue.StartResolveAll(OnPlayerTurnStartResolved);
        
    }

    private void OnPlayerTurnStartResolved()
    {
        player.UIManager.PlayerHandView.RefreshHand();
        player.UIManager.PlayerHandView.ShowHand(player.Hand.Cards);
        EnterPlayerAction();
    }

    public void EnterPlayerAction()
    {
        SetState(CombatState.PlayerAction);
        
    }
    public void TryPlayCard(CardInstance card) { }
    public void TrySelectTarget(UnitInstance target) { }
    public void EndPlayerTurn() { }

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
