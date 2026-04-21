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
    private CombatRewardManager combatRewardManager;
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
    private int currentAct = 0;
    private int currentFloor = 0;
    private bool isMapGenerated = false;
    

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
    public int CurrentAct => currentAct;

    public int CurrentFloor => currentFloor;

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

    public void RegisterCombatRewardManager(CombatRewardManager crm)
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
        if (!isMapGenerated)
        {
            mapGenerator.GenerateNewMapWithSeed(currentRunSeed);
            isMapGenerated = true;
        }
        EnterMap();
    }

    public void EnterMap()
    {
        HideAllPanels();
        MapPanel?.SetActive(true);
        SetRunState(RunState.MapState);
        Debug.Log("Entering Map");
    }

    #region Enter Node
    /// <summary>
    /// Create the node instance, then call Enter on the node
    /// </summary>
    /// <param name="node"></param>
    public void EnterNode(BaseNode node)
    {
        HideAllPanels();
        node.Enter(this);
        currentNode = node;
    }
    public void EnterNonEliteCombat(NonEliteCombatNode node)
    {
        SetRunState(RunState.CombatState);
        combatPanel.SetActive(true);
        combatManager.StartBattle(node);
    }

    public void EnterEliteCombat(EliteCombatNode node)
    {
        SetRunState(RunState.CombatState);
        combatPanel.SetActive(true);
        combatManager.StartBattle(node);
    }

    private void EnterRestSite(RestSiteNode node)
    {
        Debug.Log("Entering Campfire");
    }

    private void EnterMerchant(MerchantNode node)
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

    private void EnterTreasure(TreasureNode node)
    {
        Debug.Log("Entering Treasure");
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

    private void EnterCombatVictory()
    {
        // show reward panel
        combatRewardManager.DisplayResultCanvas(true);
        currentFloor += 1;
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
