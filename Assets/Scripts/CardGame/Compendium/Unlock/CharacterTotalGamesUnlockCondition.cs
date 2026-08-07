using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Unlock Conditions/Character Games")]
public class CharacterTotalGamesUnlockCondition : UnlockCondition
{
    [SerializeField]
    private PlayerUnitData character;

    [SerializeField, Min(1)]
    private int requiredGames = 1;

    public override string Description =>
        $"Play {requiredGames} game(s) as {character}.";

    public override bool IsMet(
        GlobalProgressionManager progression)
    {
        return progression.GetTotalGames(character)
               >= requiredGames;
    }
}
