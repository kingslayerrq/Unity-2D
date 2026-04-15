using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnemyBuffIntent : BaseEnemyIntent
{
    public override Type RequiredConfigType => typeof(EnemyBuffIntentConfig);
    public override void ExecuteIntent(EnemyIntentContext context, EnemyIntentData intentData)
    {
        if (context == null || context.CombatManager == null) return;
        if (context.Source == null || context.Targets == null ||
            context.Targets.Count == 0) return;

        if (intentData.EnemyIntentConfig is not EnemyBuffIntentConfig  buffIntentConfig)
        {
            Debug.LogError($"EnemyBuffActionData received wrong config type" +
                           $" on {context.Intent}");
            return;
        }

        if (buffIntentConfig.BuffData == null)
        {
            Debug.LogError("BuffIntent config is missing BuffData.");
            return;
        }

        ApplyBuffAction applyBuffAction = new ApplyBuffAction(context.CombatManager.CombatSystem, context.Source,
            context.Targets, buffIntentConfig.BuffData, buffIntentConfig.Stacks);

        context.CombatManager.ActionQueue.Enqueue(applyBuffAction, null);
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
