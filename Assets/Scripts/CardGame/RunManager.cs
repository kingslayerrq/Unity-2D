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
    
    [Header("Settings")]
    [SerializeField] private NodeConfigLibrary nodeConfigLibrary;

    [Header("Tests")]
    // TODO: Run config
    [SerializeField] private NonEliteCombatNodeConfig testCombatNodeConfig;
    [SerializeField] private PlayerUnitData testPlayerUnitData;
    
    [SerializeField] private int currentRunSeed;
    private BaseNode currentNode = null;
    private int currentAct = 0;
    private int currentFloor = 0;
    

    public NodeConfigLibrary NodeConfigLibrary => nodeConfigLibrary;
    public int CurrentRunSeed => currentRunSeed;

    private System.Random _masterRng;
    private System.Random _mapRng;
    private System.Random _lootRng;
    private System.Random _eventRng;
    private System.Random _combatRng;
    private System.Random _shopRng;

    public System.Random MapRng => _mapRng;
    public System.Random LootRng => _lootRng;
    public System.Random EventRng => _eventRng;
    public System.Random CombatRng => _combatRng;
    public System.Random ShopRng => _shopRng;
    
    [Header("Debug")]
    [SerializeField] private PlayerUnitData playerUnitData;
    [SerializeField] private BasePlayerUnitInstance playerUnitInstance;
    [SerializeField] private RunState currentRunState = RunState.None;
    
    [SerializeField] private int cardRewardAmountAfterCombat;
    public bool AllowCardClassOverride { get; private set; } = false;
    

    public BasePlayerUnitInstance PlayerUnitInstance => playerUnitInstance;
    public int CardRewardAmountAfterCombat => cardRewardAmountAfterCombat;
    public BaseNode CurrentNode => currentNode;
    public int CurrentAct => currentAct;

    public int CurrentFloor => currentFloor;

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

    #region Enter Node
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
    #endregion
    
    
    public void StartRun(int? seed = null)
    {
        // TODO: seed
        currentRunSeed = seed ?? GenerateSeed();
        Debug.Log($"Starting run with seed: {currentRunSeed}");

        _masterRng = new System.Random(currentRunSeed);
        
        _mapRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Map));
        _lootRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Loot));
        _eventRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Event));
        _combatRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Combat));
        _shopRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Shop));
        
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
    
    private int GenerateSeed()
    {
        return Guid.NewGuid().GetHashCode();
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
