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

    // Seeded per run in StartRun. The fallback only applies when a scene is
    // played directly without starting a run, where there is no seed to honour.
    public System.Random MapRng => _mapRng ??= new System.Random();
    public System.Random LootRng => _lootRng ??= new System.Random();
    public System.Random EventRng => _eventRng ??= new System.Random();
    public System.Random CombatRng => _combatRng ??= new System.Random();
    public System.Random ShopRng => _shopRng ??= new System.Random();
    
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

        // An Unknown node picks its real content in OnEnter, so route on the
        // resolved node rather than on the map node itself.
        BaseNode resolvedNode = node.ResolvedNode;

        if (resolvedNode is BaseCombatNode combatNode)
        {
            SetRunState(RunState.CombatState);
            combatPanel.SetActive(true);
            combatManager.StartBattle(combatNode);
            return;
        }

        // Rest sites, merchants, treasure and events have no screen yet.
        // Complete them immediately so the run cannot get stuck with every
        // panel hidden.
        Debug.LogWarning(
            $"Node type {resolvedNode.Type} is not implemented yet; skipping it.",
            this);
        _currentFloor += 1;
        ExitNode(node);
    }
    
    public void ExitNode(BaseNode node)
    {
        node.OnExit();
        if (GlobalProgressionManager.HasInstance)
        {
            GlobalProgressionManager.Instance.RecordNodeCompleted(playerUnitData, node.ResolvedNode);
        }
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

        // Drop everything left over from the previous run (map, floor, node,
        // player) before this one begins.
        ResetRunState();

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
    /// Clears all per-run gameplay state. The run seed and RNG streams are
    /// replaced by <see cref="StartRun"/> right after.
    /// </summary>
    private void ResetRunState()
    {
        currentNode = null;
        _currentAct = 0;
        _currentFloor = 0;
        _isMapGenerated = false;
        playerUnitData = null;
        playerUnitInstance = null;
        AllowCardClassOverride = false;
        currentRunState = RunState.None;
    }

    /// <summary>
    /// Ends the current run if one is still active and starts a new one with a
    /// new seed, reloading the run scene at character select.
    /// </summary>
    public void RestartRun()
    {
        EndRun();
        StartRunWithRandomSeed();
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
                currentNode.ResolvedNode.Type == NodeType.CombatBoss)
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
