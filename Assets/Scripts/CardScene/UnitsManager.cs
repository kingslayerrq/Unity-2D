using System;
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
        // Spawn the visual representation for this specific combat
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
    public void CreateAndSpawnEnemyUnits(List<EnemyUnitData> datas)
    {
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
    

    private void HandleStateChange(CombatManager.CombatState combatState)
    {
        switch (combatState)
        {
            case CombatManager.CombatState.BattleSetup:
                break;
            default:
                break;
        }
    }
    

    

    public void SetActivePlayerUnitInstance(BasePlayerUnitInstance unitInstance = null)
    {
        ActivePlayerUnitInstance = unitInstance;
    }

    private void OnDestroy()
    {
        if (combatManager != null)
        {
            combatManager.OnStateChange -= HandleStateChange;
        }
    }
}
