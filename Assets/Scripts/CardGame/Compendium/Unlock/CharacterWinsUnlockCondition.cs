using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Unlock Conditions/Character Wins")]
public sealed class CharacterWinsUnlockCondition : UnlockCondition
{
    [SerializeField]
    private CardClass cardClass;

    [SerializeField, Min(1)]
    private int requiredWins = 1;

    public override string Description =>
        $"Win {requiredWins} run(s) as {cardClass}.";

    public override bool IsMet(
        GlobalProgressionManager progression)
    {
        return progression.GetCharacterWins(cardClass)
               >= requiredWins;
    }
}