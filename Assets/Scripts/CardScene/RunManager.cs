using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a run
/// </summary>
public class RunManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitsManager unitsManager;
    [SerializeField] private CombatManager combatManager;

    [Header("Tests")]
    [SerializeField] private List<EnemyUnitData> testEnemyDatas;
    [SerializeField] private PlayerUnitData testPlayerUnitData;
    
    [Header("Debug")]
    [SerializeField] private PlayerUnitData playerUnitData;
    [SerializeField] private BasePlayerUnitInstance playerUnitInstance;
    [SerializeField] private RunState currentRunState = RunState.None;

    public BasePlayerUnitInstance PlayerUnitInstance => playerUnitInstance;

    public event Action<RunState> OnRunStateChange;
    
    private void Awake()
    {
        if (unitsManager == null)
        {
            Debug.LogError($"[RunManager] {name}: No UnitsManager found!");
            return;
        }

        if (combatManager == null)
        {
            Debug.LogError($"[RunManager] {name}: No CombatManager found!");
            return;
        }
    }

    /// <summary>
    /// sets the player unit data
    /// create playerunit instance through UnitsManager
    /// enters SetupActState
    /// </summary>
    /// <param name="data"></param>
    public void ChoosePlayerUnit(PlayerUnitData data)
    {
        SetRunState(RunState.SelectPlayerUnitState);
        playerUnitData = data;
        unitsManager.CreatePlayerUnitInstance(playerUnitData);
        playerUnitInstance = unitsManager.ActivePlayerUnitInstance;
        
        SetupAct();
    }

    public void SetupAct()
    {
        SetRunState(RunState.SetupActState);
        // TODO: set up act logics
        EnterMap();
    }

    public void EnterMap()
    {
        SetRunState(RunState.MapState);
        
        EnterCombat();
    }
    
    public void EnterCombat()
    {
        SetRunState(RunState.CombatState);
        combatManager.StartBattle(new CombatNode(testEnemyDatas));
    }

    public void StartRun()
    {
        // TODO: seed
        
        // choose
        ChoosePlayerUnit(testPlayerUnitData);
    }

    private void SetRunState(RunState newRunState)
    {
        currentRunState = newRunState;
        OnRunStateChange?.Invoke(newRunState);
    }
}

public enum RunState
{
    None,
    SelectPlayerUnitState,
    SetupActState,
    MapState,
    CombatState,
    NodeState,
    WinState,
    LoseState
}
