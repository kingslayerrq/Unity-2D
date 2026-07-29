using UnityEngine;

[CreateAssetMenu(
    menuName =
        "Progression/Character Unlock Conditions/Play Character Game")]
public sealed class CharacterGamePlayedUnlockCondition : UnlockCondition
{
    [SerializeField]
    private PlayerUnitData requiredCharacter;

    public PlayerUnitData RequiredCharacter => requiredCharacter;

    public override string Description =>
        requiredCharacter != null
            ? $"Play 1 game as {requiredCharacter.UnitName}."
            : "Play 1 game as the required character.";

    public override bool IsMet(
        GlobalProgressionManager progression)
    {
        return progression != null &&
               requiredCharacter != null &&
               progression.GetCharacterTotalGames(
                   requiredCharacter) >= 1;
    }
}
