using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Controls a run
/// </summary>
public class RunManager : MonoBehaviour
{
    private static RunManager _instance;
    private static bool _isQuitting = false;
    private bool _hasRunStarted = false;
    
    public bool HasRunStarted => _hasRunStarted;
    
    public static RunManager Instance { get 
    {
        if (_isQuitting)
        {
            return null;
        }
        if (_instance == null)
        {
            // Try to find it in the scene first
            _instance = FindFirstObjectByType<RunManager>();

            // If it's STILL null, we spawn it from a prefab in the Resources folder!
            if (_instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("RunManager");
                GameObject singletonObject = Instantiate(prefab);
                _instance = singletonObject.GetComponent<RunManager>();
            }
        }
        return _instance;
    }}
    [Header("References")]
    private MapGenerator mapGenerator;
    private UnitsManager unitsManager;
    private CombatManager combatManager;
    private NodeRewardManager combatRewardManager;
    private GameObject characterSelectPanel;
    private GameObject mapPanel;
    private GameObject combatPanel;
    
    public GameObject CombatPanel => combatPanel;
    public GameObject MapPanel => mapPanel;
    
    
    [Header("Settings")]
    [SerializeField] private NodeConfigLibrary nodeConfigLibrary;

    [Header("Tests")]
    // TODO: Run config
    [SerializeField] private NonEliteCombatNodeConfig testCombatNodeConfig;
    [SerializeField] private PlayerUnitData testPlayerUnitData;
    
    [SerializeField] private int currentRunSeed;
    private BaseNode currentNode = null;
    private int _currentAct = 0;
    private int _currentFloor = 0;
    private bool _isMapGenerated = false;
    

    public NodeConfigLibrary NodeConfigLibrary => nodeConfigLibrary;
    public int CurrentRunSeed => currentRunSeed;

    // private System.Random _masterRng;
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
    public int CurrentAct => _currentAct;
    public int CurrentFloor => _currentFloor;

    public event Action OnRunStarted;
    public event Action OnRunEnded;
    public event Action<RunState> OnRunStateChange;
    public event Action<BasePlayerUnitInstance> OnPlayerUnitInstanceCreated;
    
    private void Awake()
    {
        if (_instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region Dependencies
    public void RegisterMapGenerator(MapGenerator mg)
    {
        mapGenerator = mg;
    }
    public void RegisterCombatManager(CombatManager cm)
    {
        combatManager = cm;
        if (combatManager != null)
        {
            combatManager.OnStateChange += HandleCombatStateChange;
        }
    }

    public void RegisterCombatRewardManager(NodeRewardManager crm)
    {
        combatRewardManager = crm;
        combatRewardManager.Initialize();
    }

    public void RegisterUnitsManager(UnitsManager um)
    {
        unitsManager = um;
    }
    
    public void RegisterUIPanels(GameObject charSelect, GameObject map, GameObject combat)
    {
        characterSelectPanel = charSelect;
        mapPanel = map;
        combatPanel = combat;
        HideAllPanels();
        characterSelectPanel.SetActive(true);
    }
    

    #endregion
    

    private void HideAllPanels()
    {
        characterSelectPanel.SetActive(false);
        mapPanel.SetActive(false);
        combatPanel.SetActive(false);
    }

    /// <summary>
    /// sets the player unit data
    /// creates playerunit instance directly (no UnitsManager needed pre-combat)
    /// enters SetupActState
    /// </summary>
    /// <param name="data"></param>
    public void ChoosePlayerUnit(PlayerUnitData data)
    {
        SetRunState(RunState.SelectPlayerUnitState);
        playerUnitData = data;
        playerUnitInstance = new BasePlayerUnitInstance(data);
        OnPlayerUnitInstanceCreated?.Invoke(playerUnitInstance);
        SetupAct();
    }

    public void SetupAct()
    {
        SetRunState(RunState.SetupActState);
        // TODO: set up act logics
        EnterMap();
    }

    private void EnterMap()
    {
        HideAllPanels();

        if (mapPanel == null)
        {
            Debug.LogError(
                "Cannot enter the map without a map panel.",
                this);
            return;
        }

        mapPanel.SetActive(true);
        Canvas.ForceUpdateCanvases();

        if (!_isMapGenerated)
        {
            if (mapGenerator == null)
            {
                Debug.LogError(
                    "Cannot generate the map without a map generator.",
                    this);
                return;
            }

            _isMapGenerated =
                mapGenerator.GenerateNewMapWithSeed(currentRunSeed);
        }

        if (!_isMapGenerated)
            return;

        SetRunState(RunState.MapState);
        Debug.Log("Entering Map");
    }

    #region Node
    public void EnterNode(BaseNode node)
    {
        HideAllPanels();
        currentNode = node;
        // On enter setup
        node.OnEnter();

        switch (node.Type)
        {
            case NodeType.CombatNonElite:
            case NodeType.CombatElite:
            case NodeType.CombatBoss:
                SetRunState(RunState.CombatState);
                combatPanel.SetActive(true);
                combatManager.StartBattle((BaseCombatNode)node);
                break;
            case NodeType.RestSite:
                Debug.Log("Entering Campfire");
                break;
            case NodeType.Merchant:
                Debug.Log("Entering Shop");
                break;
            case NodeType.Treasure:
                Debug.Log("Entering Treasure");
                break;
            case NodeType.Event:
                Debug.Log("Entering Event");
                break;
            default:
                Debug.Log($"Node type {node.Type}");
                break;
        }
    }
    
    public void ExitNode(BaseNode node)
    {
        node.OnExit();
        EnterMap();
    }
    #endregion

    public void StartRunWithRandomSeed()
    {
        StartRun(null);
    }

    public void StartRunWithSeed(int seed)
    {
        StartRun(seed);
    }
    
    private void StartRun(int? seed = null)
    {
        if (HasRunStarted)
        {
            return;
        }

        _hasRunStarted = true;
        OnRunStarted?.Invoke();
        
        // seed
        currentRunSeed = seed ?? GenerateSeed();
        Debug.Log($"Starting run with seed: {currentRunSeed}");

        // _masterRng = new System.Random(currentRunSeed);

        _mapRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Map));
        _lootRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Loot));
        _eventRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Event));
        _combatRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Combat));
        _shopRng = new System.Random(SeedUtility.DeriveSeed(currentRunSeed, RngCategory.Shop));

        CoroutineRunner.Instance.StartCoroutine(LoadScene("CombatNodeScene"));
    }

    /// <summary>
    /// Marks the current run as ended and notifies persistent systems.
    /// The caller remains responsible for choosing the next scene and for
    /// resetting run-specific gameplay data.
    /// </summary>
    public void EndRun()
    {
        if (!HasRunStarted)
        {
            return;
        }

        _hasRunStarted = false;
        OnRunEnded?.Invoke();
    }

    /// <summary>
    /// Records the result of a completed run, then ends it.
    /// </summary>
    public void CompleteRun(bool won)
    {
        if (!HasRunStarted)
            return;

        if (playerUnitData == null)
        {
            Debug.LogError(
                "Cannot record run completion without a character.",
                this);
        }
        else if (GlobalProgressionManager.HasInstance)
        {
            GlobalProgressionManager.Instance.RecordRunCompleted(
                playerUnitData,
                won);
        }
        else
        {
            Debug.LogError(
                "Cannot record run completion without global " +
                "progression.",
                this);
        }

        EndRun();
    }

    private IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (operation != null && !operation.isDone)
        {
            yield return null;
        }
    }

    private void SetRunState(RunState newRunState)
    {
        currentRunState = newRunState;
        OnRunStateChange?.Invoke(newRunState);
    }

    /// <summary>
    /// Displaying reward
    /// progress to next floor
    /// </summary>
    private void EnterNodeReward()
    {
        SetRunState(RunState.WinState);
        // show reward panel
        combatRewardManager.DisplayResultCanvas(true);
        _currentFloor += 1;
    }

    private void EnterRunVictory()
    {
        SetRunState(RunState.WinState);
        combatRewardManager.DisplayResultCanvas(true);
        CompleteRun(true);
    }

    private void EnterRunLoss()
    {
        SetRunState(RunState.LoseState);
        combatRewardManager.DisplayResultCanvas(false);
        CompleteRun(false);
    }

    private void HandleCombatStateChange(CombatManager.NodeState newNodeState)
    {
        if (newNodeState == CombatManager.NodeState.Victory)
        {
            if (currentNode != null &&
                currentNode.Type == NodeType.CombatBoss)
            {
                EnterRunVictory();
            }
            else
            {
                EnterNodeReward();
            }
        }
        else if (newNodeState == CombatManager.NodeState.Defeat)
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
    
    private void OnApplicationQuit()
    {
        _isQuitting = true;
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
