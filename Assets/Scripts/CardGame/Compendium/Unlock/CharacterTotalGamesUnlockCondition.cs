using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Unlock Conditions/Character Games")]
public class CharacterTotalGamesUnlockCondition : UnlockCondition
{
    [SerializeField]
    private CardClass cardClass;

    [SerializeField, Min(1)]
    private int requiredGames = 1;

    public override string Description =>
        $"Play {requiredGames} game(s) as {cardClass}.";

    public override bool IsMet(
        GlobalProgressionManager progression)
    {
        return progression.GetCharacterTotalGames(cardClass)
               >= requiredGames;
    }
}
