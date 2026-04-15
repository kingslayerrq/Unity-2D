using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls a run
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }
    [Header("References")]
    private UnitsManager unitsManager;
    private CombatManager combatManager;
    private CombatRewardManager combatRewardManager;

    [Header("Tests")]
    // TODO: Run config
    [SerializeField] private NonEliteCombatNodeConfig testCombatNodeConfig;
    [SerializeField] private PlayerUnitData testPlayerUnitData;
    
    
    [Header("Debug")]
    [SerializeField] private PlayerUnitData playerUnitData;
    [SerializeField] private BasePlayerUnitInstance playerUnitInstance;
    [SerializeField] private RunState currentRunState = RunState.None;
    
    [SerializeField] private int cardRewardAmountAfterCombat;
    public bool AllowCardClassOverride { get; private set; } = false;
    private BaseNode currentNode = null;
    

    public BasePlayerUnitInstance PlayerUnitInstance => playerUnitInstance;
    public int CardRewardAmountAfterCombat => cardRewardAmountAfterCombat;
    public BaseNode CurrentNode => currentNode;

    public event Action<RunState> OnRunStateChange;
    public event Action<BasePlayerUnitInstance> OnPlayerUnitInstanceCreated;
    
    private void Awake()
    {
        if (Instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
    }
    

    public void RegisterCombatManager(CombatManager cm)
    {
        combatManager = cm;
        if (combatManager != null)
        {
            combatManager.OnStateChange += HandleCombatStateChange;
        }
    }

    public void RegisterCombatRewardManager(CombatRewardManager crm)
    {
        combatRewardManager = crm;
    }

    public void RegisterUnitsManager(UnitsManager um)
    {
        unitsManager = um;
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
        OnPlayerUnitInstanceCreated?.Invoke(playerUnitInstance);
        
        combatRewardManager.Initialize(); // init
        
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
        Debug.Log("Entering Map");
        EnterNode(testCombatNodeConfig);
    }

    /// <summary>
    /// Create the node instance, then call Enter on the node
    /// </summary>
    /// <param name="nodeConfig"></param>
    public void EnterNode(BaseNodeConfig nodeConfig)
    {
        BaseNode node = nodeConfig.CreateNode();
        node.Enter(this);
        currentNode = node;
    }
    public void EnterNonEliteCombat(NonEliteCombatNode node)
    {
        SetRunState(RunState.CombatState);
        combatManager.StartBattle(node);
    }

    public void EnterEliteCombat(EliteCombatNode node)
    {
        SetRunState(RunState.CombatState);
        combatManager.StartBattle(node);
    }

    private void EnterCampfire()
    {
        Debug.Log("Entering Campfire");
    }

    private void EnterShop()
    {
        Debug.Log("Entering Shop");
    }

    private void EnterQuest()
    {
        Debug.Log("Entering Quest");
    }

    private void EnterEvent()
    {
        Debug.Log("Entering Event");
    }

    private void EnterTreasure()
    {
        Debug.Log("Entering Treasure");
    }

    public void StartRun()
    {
        // TODO: seed
        CoroutineRunner.Instance.StartCoroutine(LoadScene("CombatNodeScene"));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (operation != null && !operation.isDone)
        {
            yield return null;
        }
        ChoosePlayerUnit(testPlayerUnitData);
    }

    private void SetRunState(RunState newRunState)
    {
        currentRunState = newRunState;
        OnRunStateChange?.Invoke(newRunState);
    }

    private void EnterCombatVictory()
    {
        // show reward panel
        combatRewardManager.DisplayResultCanvas(true);
    }

    private void EnterRunVictory()
    {
        
    }

    private void EnterRunLoss()
    {
        SetRunState(RunState.LoseState);
        combatRewardManager.DisplayResultCanvas(false);
    }

    private void HandleCombatStateChange(CombatManager.CombatState newCombatState)
    {
        if (newCombatState == CombatManager.CombatState.Victory)
        {
            // TODO: enter run victory if last node is beaten
            EnterCombatVictory();
        }
        else if (newCombatState == CombatManager.CombatState.Defeat)
        {
            EnterRunLoss();
        }
    }

    public void SetCardClassRewardOverride(bool allow)
    {
        AllowCardClassOverride = allow;
    }

    private void OnDestroy()
    {
        if (combatManager != null) 
        {
            combatManager.OnStateChange -= HandleCombatStateChange;
        }
    }
}

public enum RunState
{
    None,
    SelectPlayerUnitState,
    SetupActState,
    MapState,
    CombatState,
    WinState,
    LoseState
}
