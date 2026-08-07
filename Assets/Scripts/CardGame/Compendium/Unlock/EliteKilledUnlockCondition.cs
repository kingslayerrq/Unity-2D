using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Unlock Conditions/Elites killed")]
public class EliteKilledUnlockCondition : UnlockCondition
{
    [Tooltip("If left null then checks for global elites killed count")]
    [SerializeField] private PlayerUnitData character;
    
    [SerializeField, Min(1)] private int requiredKills = 1;
    
    public override string Description =>
        $"Defeat {requiredKills} Elites{(character == null ? "" : $" as {character.name}")}.";

    public override bool IsMet(GlobalProgressionManager progression)
    {
        return progression.GetCharacterElitesDefeated(character) >= requiredKills;
    }
}
