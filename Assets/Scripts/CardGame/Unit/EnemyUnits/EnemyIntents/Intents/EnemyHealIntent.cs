using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnemyHealIntent : BaseEnemyIntent
{
    public override Type RequiredConfigType => typeof(EnemyHealIntentConfig);
    
    public override void ExecuteIntent(EnemyIntentContext context, EnemyIntentData intentData)
    {
        if (context == null || context.CombatManager == null) return;
        if (context.Source == null || context.Targets == null ||
            context.Targets.Count == 0) return;

        if (intentData.EnemyIntentConfig is not EnemyHealIntentConfig healIntentConfig)
        {
            Debug.LogError($"EnemyHealActionData received wrong config type" +
                           $" on {context.Intent}");
            return;
        }

        HealAction healAction = new HealAction(context.CombatManager.CombatSystem, context.Source,
            context.Targets, healIntentConfig.Amount);

        context.CombatManager.ActionQueue.Enqueue(healAction, null);
        
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
            int randomIndex = RunManager.Instance.CombatRng.Next(validEnemies.Count); 
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
