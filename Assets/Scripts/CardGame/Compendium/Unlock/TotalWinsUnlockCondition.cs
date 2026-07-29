using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Unlock Conditions/Total Wins")]
public sealed class TotalWinsUnlockCondition : UnlockCondition
{
    [SerializeField, Min(1)]
    private int requiredWins = 1;

    public override string Description =>
        $"Win {requiredWins} run{(requiredWins == 1 ? "" : "s")}.";

    public override bool IsMet(
        GlobalProgressionManager progression)
    {
        return progression.TotalRunWins >= requiredWins;
    }
}