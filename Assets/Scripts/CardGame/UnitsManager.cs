using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private CardPileManager cardPileManager; 
    

    [Header("Prefabs")]
    [SerializeField] private UnitView playerPrefab;
    [SerializeField] private UnitView enemyPrefab; 

    [Header("Spawn Points")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform[] enemySpawnPoints;
    
    public event Action OnAllEnemiesDefeated;
    public event Action OnAllPlayersDefeated;

    public BasePlayerUnitInstance ActivePlayerUnitInstance { get; private set; } = null;
    public List<BasePlayerUnitInstance> ActivePlayerUnitInstances { get; private set; } = 
        new List<BasePlayerUnitInstance>();

    public List<BaseEnemyUnitInstance> ActiveEnemyUnitInstances { get; private set; } =
        new List<BaseEnemyUnitInstance>();
    private List<BaseUnitInstance> _allActiveUnits = new List<BaseUnitInstance>();
    public IReadOnlyList<BaseUnitInstance> AllActiveUnits => _allActiveUnits;

    private void Awake()
    {
        RunManager.Instance.RegisterUnitsManager(this);
    }

    /// <summary>
    /// RunManager calls RegisterExistingPlayerUnit on subsequent combats.
    /// </summary>
    /// <param name="existing"></param>
    public void RegisterExistingPlayerUnit(BasePlayerUnitInstance existing)
    {
        ActivePlayerUnitInstance = existing;
        AddUnit(existing);
    }
    
    public void AddUnit(BaseUnitInstance unit)
    {
        _allActiveUnits.Add(unit);
        
        if (unit is BaseEnemyUnitInstance enemy)
        {
            ActiveEnemyUnitInstances.Add(enemy);
        }
        else if (unit is BasePlayerUnitInstance player) 
        {
            ActivePlayerUnitInstances.Add(player);
        }
    }
    public void RemoveUnit(BaseUnitInstance unit)
    {
        _allActiveUnits.Remove(unit);

        if (unit is BaseEnemyUnitInstance enemy)
        {
            ActiveEnemyUnitInstances.Remove(enemy);
        
            // Check win condition
            if (ActiveEnemyUnitInstances.Count == 0)
            {
                OnAllEnemiesDefeated?.Invoke();
            }
        }
        else if (unit is BasePlayerUnitInstance player)
        {
            ActivePlayerUnitInstances.Remove(player);
            if (ReferenceEquals(unit, ActivePlayerUnitInstance))
            {
                ActivePlayerUnitInstance = null;
            }
            // Check lose condition
            if (ActivePlayerUnitInstances.Count == 0)
            {
                OnAllPlayersDefeated?.Invoke();
            }
        }
    }
    

    public void CreatePlayerUnitInstance(PlayerUnitData data)
    {
        ActivePlayerUnitInstance = new BasePlayerUnitInstance(data);
        AddUnit(ActivePlayerUnitInstance);
    }

    public void SpawnAndBindPlayerView()
    {
        if (ActivePlayerUnitInstance == null)
        {
            Debug.LogError("No active player unit");
            return;
        }
        foreach (Transform child in playerSpawnPoint)
            Destroy(child.gameObject);

        UnitView playerUnitView = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, playerSpawnPoint);
        
    
        // Fetch local components attached to the new view
        BuffManager buffManager = playerUnitView.GetComponent<BuffManager>();
    
        // Inject the current combat's managers into our persistent player model
        ActivePlayerUnitInstance.Initialize(combatManager, buffManager, cardPileManager);
    
        // Link the view to the model
        playerUnitView.Initialize(ActivePlayerUnitInstance);
    }
    

    /// <summary>
    /// Use the data to create enemy instances and instantiate prefabs
    /// </summary>
    /// <param name="datas">Enemy datas</param>
    public void CreateAndSpawnEnemyUnits(IReadOnlyList<EnemyUnitData> datas)
    {
        foreach (Transform spawnPoint in enemySpawnPoints)
            foreach (Transform child in spawnPoint)
                Destroy(child.gameObject);

        ActiveEnemyUnitInstances.Clear();

        for (int i = 0; i < datas.Count; i++)
        {
            // TODO: Decide spawnpoint logic
            Transform spawnPoint = (i < enemySpawnPoints.Length) ? enemySpawnPoints[i] : transform;
            
            UnitView enemyView = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
            
            BuffManager buffManager = enemyView.GetComponent<BuffManager>();

            // Create and initialize logic
            BaseEnemyUnitInstance enemyModel = new BaseEnemyUnitInstance(datas[i]);
            enemyModel.Initialize(combatManager, buffManager);
            
            enemyView.Initialize(enemyModel);
            AddUnit(enemyModel);
        }
    }

    private void Start()
    {
        if (combatManager != null)
        {
            combatManager.OnStateChange += HandleStateChange;
        }
    }
    

    private void HandleStateChange(CombatManager.NodeState combatState)
    {
        switch (combatState)
        {
            case CombatManager.NodeState.BattleSetup:
                break;
            default:
                break;
        }
    }
    

    public void SetActivePlayerUnitInstance(BasePlayerUnitInstance unitInstance = null)
    {
        ActivePlayerUnitInstance = unitInstance;
    }

    public IReadOnlyList<BaseUnitInstance> GetUnitsByType(TargetType targetType)
    {
        IReadOnlyList<BaseUnitInstance> units = null;
        switch (targetType)
        {
            case TargetType.Self:
                units = new BaseUnitInstance[] { this.ActivePlayerUnitInstance };
                break;
            case TargetType.AllPlayer:
                units = this.ActivePlayerUnitInstances;
                break;
            case TargetType.AllEnemy:
                units = this.ActiveEnemyUnitInstances;
                break;
            default:
                Debug.LogError("Secondary target type not supported");
                break;
        }

        return units;
    }

    private void OnDestroy()
    {
        if (combatManager != null)
        {
            combatManager.OnStateChange -= HandleStateChange;
        }
    }
}
