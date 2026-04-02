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

    public event Action<EnemyIntentData> OnEnemyIntentDetermined;

    public override void Initialize(CombatManager combatManager, BuffManager buffManager)
    {
        base.Initialize(combatManager, buffManager);
    }
    
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
        int randomIndex = UnityEngine.Random.Range(0, currentAvailableIntentDatas.Count);
        currentIntent = currentAvailableIntentDatas[randomIndex];
        currentIntent.EnemyIntent.ChooseTargets(currentIntent, snapshot);
        // TODO: trigger ui updates
        OnEnemyIntentDetermined?.Invoke(currentIntent);
    }

    public void ExecuteActionIntent()
    {
        if (IsDead || currentIntent == null) return;
        
    }

    protected override void OnBattleStart()
    {
        InitHealth();
    }

    protected override void OnBattleSetup()
    {
        throw new NotImplementedException();
    }

    protected override void OnVictory()
    {
        throw new NotImplementedException();
    }

    protected override void OnDefeat()
    {
        throw new NotImplementedException();
    }
}
