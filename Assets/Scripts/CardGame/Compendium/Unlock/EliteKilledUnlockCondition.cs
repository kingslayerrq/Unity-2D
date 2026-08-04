using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Unlock Conditions/Elites killed")]
public class EliteKilledUnlockCondition : UnlockCondition
{
    [SerializeField] private CardClass requiredClass = CardClass.None;
    
    [SerializeField, Min(1)] private int requiredKills = 1;
    
    public override string Description =>
        $"Defeat {requiredKills} Elites{(requiredClass == CardClass.None ? "" : $" as {requiredClass}")}.";

    public override bool IsMet(GlobalProgressionManager progression)
    {
        throw new System.NotImplementedException();
    }
}
