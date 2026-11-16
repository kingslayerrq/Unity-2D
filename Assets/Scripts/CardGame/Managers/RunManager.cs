using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// The scene side of a run. It owns the <see cref="RunSession"/> that holds
/// the run's state and rules, and connects it to what is on screen: the
/// panels, the map, the combat and the result screen. It also loads the run
/// scene and survives between scenes.
///
/// It holds no run rules of its own. A run plays the same without it.
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
    private CombatPresenter combatPresenter;
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

    private RunSession run;
    private bool _isMapDrawn = false;

    /// <summary>
    /// The run in progress. One is created on first use if no run was
    /// started, so a run scene opened directly in the editor still works.
    /// </summary>
    public RunSession Run
    {
        get
        {
            if (run == null) SetRun(CreateRun(GenerateSeed()));
            return run;
        }
    }

    public NodeConfigLibrary NodeConfigLibrary => nodeConfigLibrary;
    public int CurrentRunSeed => Run.Seed;

    public System.Random MapRng => Run.MapRng;
    public System.Random LootRng => Run.LootRng;
    public System.Random EventRng => Run.EventRng;
    public System.Random CombatRng => Run.CombatRng;
    public System.Random ShopRng => Run.ShopRng;

    // The serialized fields below only mirror the run for the inspector.
    [Header("Debug")]
    [SerializeField] private PlayerUnitData playerUnitData;
    [SerializeField] private BasePlayerUnitInstance playerUnitInstance;
    [SerializeField] private RunState currentRunState = RunState.None;

    [SerializeField] private int cardRewardAmountAfterCombat;
    public bool AllowCardClassOverride => Run.AllowCardClassOverride;


    public BasePlayerUnitInstance PlayerUnitInstance => Run.Player;
    public int CardRewardAmountAfterCombat => cardRewardAmountAfterCombat;
    public BaseNode CurrentNode => Run.CurrentNode;
    public int CurrentAct => Run.CurrentAct;
    public int CurrentFloor => Run.CurrentFloor;

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
        _isMapDrawn = false;

        if (mapGenerator == null) return;

        // The map's size is authored on the generator in the run scene, so it
        // reaches the run only once that scene has loaded.
        Run.Config.Map = mapGenerator.Settings;
        if (Run.Config.NodeLibrary == null)
        {
            Run.Config.NodeLibrary = mapGenerator.NodeConfigLibrary;
        }
    }

    public void RegisterCombatPresenter(CombatPresenter presenter)
    {
        combatPresenter = presenter;
    }

    public void RegisterCombatRewardManager(NodeRewardManager crm)
    {
        combatRewardManager = crm;
        combatRewardManager.Initialize();
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

    #region Run session

    private RunSession CreateRun(int seed)
    {
        RunConfig config = new RunConfig
        {
            NodeLibrary = nodeConfigLibrary,
            CardRewardAmount = cardRewardAmountAfterCombat,
        };

        if (mapGenerator != null)
        {
            config.Map = mapGenerator.Settings;
        }

        return new RunSession(seed, config, new GlobalProgressionRecorder());
    }

    private void SetRun(RunSession newRun)
    {
        UnsubscribeFromRun();

        run = newRun;
        _isMapDrawn = false;

        currentRunSeed = run.Seed;
        currentRunState = run.State;
        playerUnitData = null;
        playerUnitInstance = null;

        run.StateChanged += HandleRunStateChanged;
        run.PlayerCreated += HandlePlayerCreated;
        run.BattleSetUp += HandleBattleSetUp;
        run.Ended += HandleRunEnded;
    }

    private void UnsubscribeFromRun()
    {
        if (run == null) return;

        run.StateChanged -= HandleRunStateChanged;
        run.PlayerCreated -= HandlePlayerCreated;
        run.BattleSetUp -= HandleBattleSetUp;
        run.Ended -= HandleRunEnded;
    }

    /// <summary>
    /// Shows each change of the run on screen, then tells the scene's own
    /// listeners. The scene work comes first, so a listener that refreshes
    /// the map finds it already drawn.
    /// </summary>
    private void HandleRunStateChanged(RunState newRunState)
    {
        currentRunState = newRunState;

        switch (newRunState)
        {
            case RunState.MapState:
                ShowMap();
                break;

            case RunState.CombatState:
                HideAllPanels();
                if (combatPanel != null) combatPanel.SetActive(true);
                break;

            case RunState.WinState:
                // show reward panel
                if (combatRewardManager != null) combatRewardManager.DisplayResultCanvas(true);
                break;

            case RunState.LoseState:
                if (combatRewardManager != null) combatRewardManager.DisplayResultCanvas(false);
                break;
        }

        OnRunStateChange?.Invoke(newRunState);
    }

    private void HandlePlayerCreated(BasePlayerUnitInstance player)
    {
        playerUnitData = run.Character;
        playerUnitInstance = player;
        OnPlayerUnitInstanceCreated?.Invoke(player);
    }

    private void HandleBattleSetUp(CombatManager combat)
    {
        if (combatPresenter == null)
        {
            Debug.LogError("Cannot show the battle without a combat presenter.", this);
            return;
        }

        combatPresenter.ShowBattle(combat);
    }

    private void HandleRunEnded(bool won)
    {
        EndRun();
    }

    #endregion

    private void HideAllPanels()
    {
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (combatPanel != null) combatPanel.SetActive(false);
    }

    /// <summary>
    /// Creates the player unit for this run and moves on to the map.
    /// </summary>
    /// <param name="data"></param>
    public void ChoosePlayerUnit(PlayerUnitData data)
    {
        Run.ChooseCharacter(data);
    }

    private void ShowMap()
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

        if (!_isMapDrawn)
        {
            if (mapGenerator == null)
            {
                Debug.LogError(
                    "Cannot draw the map without a map generator.",
                    this);
                return;
            }

            // Drawing needs the panel laid out, which is why it waits until
            // the panel is active. If it fails it is tried again next time.
            _isMapDrawn = mapGenerator.ShowMap(run.Map);
        }

        if (_isMapDrawn)
        {
            Debug.Log("Entering Map");
        }
    }

    #region Node

    /// <summary>
    /// True when the player may enter the node from the map right now.
    /// </summary>
    public bool IsNodeAvailable(BaseNode node)
    {
        return Run.IsAvailable(node);
    }

    public void EnterNode(BaseNode node)
    {
        Run.EnterNode(node);
    }

    /// <summary>
    /// Leaves the node that was just won and returns to the map.
    /// </summary>
    public void ExitNode(BaseNode node)
    {
        Run.CompleteNode();
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

        // A new session replaces everything left over from the previous run
        // (map, floor, node, player).
        SetRun(CreateRun(seed ?? GenerateSeed()));

        _hasRunStarted = true;
        OnRunStarted?.Invoke();

        Debug.Log($"Starting run with seed: {run.Seed}");

        CoroutineRunner.Instance.StartCoroutine(LoadScene("CombatNodeScene"));
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
    /// The caller remains responsible for choosing the next scene.
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
    /// Ends the run as won or lost, which records the result, then marks it
    /// as ended.
    /// </summary>
    public void CompleteRun(bool won)
    {
        if (!HasRunStarted)
            return;

        // Ending the session raises Ended, which calls EndRun.
        Run.End(won);
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

    public void SetCardClassRewardOverride(bool allow)
    {
        Run.AllowCardClassOverride = allow;
    }

    private int GenerateSeed()
    {
        return Guid.NewGuid().GetHashCode();
    }

    private void OnDestroy()
    {
        UnsubscribeFromRun();
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}
