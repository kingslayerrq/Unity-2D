using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseEnemyUnitInstance : BaseUnitInstance
{
    [SerializeField] private EnemyUnitData enemyUnitData;
    [SerializeField] private IReadOnlyList<EnemyActionData> currentAvailableActionDatas;
    
    public IReadOnlyList<EnemyActionData> CurrentAvailableActionDatas => currentAvailableActionDatas;

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
        this.currentAvailableActionDatas = enemyUnitData.AvailableActions;
        this.currentSprite = enemyUnitData.UnitSprite;
        this.currentUnitType = enemyUnitData.UnitType;
        
        this.isInitialized = false;
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
