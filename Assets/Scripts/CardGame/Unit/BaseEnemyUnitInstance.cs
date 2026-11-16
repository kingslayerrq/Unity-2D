using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BaseEnemyUnitInstance : BaseUnitInstance
{
    [SerializeField] private EnemyUnitData enemyUnitData;
    private IReadOnlyList<EnemyIntentData> currentAvailableIntentDatas;
    [SerializeField] private EnemyIntentData currentIntent;
    
    public IReadOnlyList<EnemyIntentData> CurrentAvailableIntentDatas => currentAvailableIntentDatas;
    public EnemyIntentData CurrentIntent => currentIntent;

    private List<BaseUnitInstance> currentIntentTargets = new List<BaseUnitInstance>();
    public List<BaseUnitInstance> CurrentIntentTargets => currentIntentTargets;

    public event Action<EnemyIntentData> OnEnemyIntentDetermined;

    public BaseEnemyUnitInstance(EnemyUnitData enemyUnitData)
    {
        this.enemyUnitData = enemyUnitData;

        this.currentName = enemyUnitData.UnitName;
        this.currentDescription = enemyUnitData.UnitDescription;
        this.currentMaxHealth = enemyUnitData.MaxHealth;
        this.currentAvailableIntentDatas = enemyUnitData.AvailableIntents;
        this.currentSprite = enemyUnitData.UnitSprite;
        this.currentUnitType = enemyUnitData.UnitType;
        
        this.isInitialized = false;
    }

    public void DetermineIntent(CombatSnapshot snapshot)
    {
        if (currentAvailableIntentDatas == null || currentAvailableIntentDatas.Count == 0)
        {
            Debug.LogWarning($"{currentName} has no actions available!");
            return;
        }
        // TODO: use snapshot to determine intent
        // Seeded combat stream, so the same run seed gives the same enemy moves.
        int randomIndex = snapshot.Rng.Next(currentAvailableIntentDatas.Count);
        currentIntent = currentAvailableIntentDatas[randomIndex];
        // The intent object lives in the shared EnemyUnitData asset, so the chosen
        // targets are stored on this instance. A fresh list each time keeps
        // already-queued actions pointing at the targets they were given.
        currentIntentTargets = currentIntent.EnemyIntent.ChooseTargets(currentIntent, snapshot);
        // TODO: trigger ui updates
        OnEnemyIntentDetermined?.Invoke(currentIntent);
    }
    

    public override void Dispose()
    {
        Debug.Log($"Enemy - {CurrentName} Disposed");
        if (CombatManager != null)
        {
            CombatManager.OnStateChange -= HandleCombatStateChange;
            if (CombatManager.UnitsManager != null)
            {
                CombatManager.UnitsManager.RemoveUnit(this);
            }
        }
        NotifyDisposed();
    }

    protected override void OnBattleStart()
    {
        InitHealth();
    }

    protected override void OnBattleSetup()
    {
    }

    protected override void OnVictory()
    {
    }

    protected override void OnDefeat()
    {
    }
}
