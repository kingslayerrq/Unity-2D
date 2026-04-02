using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnemyDmgIntent : BaseEnemyIntent
{
    public override Type RequiredConfigType => typeof(EnemyDmgIntentConfig);
    
    public override void ExecuteIntent(EnemyIntentContext context, EnemyIntentData intentData)
    {
        if (context == null || context.CombatManager == null) return;
        if (context.Source == null || context.Targets == null ||
            context.Targets.Count == 0) return;

        if (intentData.EnemyIntentConfig is not EnemyDmgIntentConfig dmgActionConfig)
        {
            Debug.LogError($"EnemyDmgActionData received wrong config type" +
                           $" on {context.Intent}");
            return;
        }
        Debug.Log($"Executing Enemy DMG intent");
        DamageAction damageAction = new DamageAction(context.CombatManager.CombatSystem, context.Source,
            context.Targets, dmgActionConfig.Amount, CardType.None);

        for (int i = 0; i < dmgActionConfig.NumberOfAttacks; i++)
        {
            context.CombatManager.ActionQueue.Enqueue(damageAction, null);
        }
        
    }

    public override BasePlayerUnitInstance ChoosePlayerTarget(CombatSnapshot snapshot)
    {
        return snapshot.AllPlayers[0] ?? null;
    }

    public override BaseEnemyUnitInstance ChooseEnemyAlly(CombatSnapshot snapshot)
    {
        var validEnemies = snapshot.AllEnemies.Where(e => e != snapshot.Self).ToList();
        
        if (validEnemies.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, validEnemies.Count); 
            var chosenEnemy = validEnemies[randomIndex];

            return chosenEnemy;
        }
        return null;
    }

    public override BaseUnitInstance ChooseAnyTarget(CombatSnapshot snapshot)
    {
        return snapshot.AllPlayers[0] ?? null;
    }
}
