using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Character Unlock Definition")]
public sealed class CharacterUnlockDefinition : ScriptableObject
{
    [SerializeField]
    private PlayerUnitData character;

    [SerializeField]
    private CharacterTotalGamesUnlockCondition condition;

    public PlayerUnitData Character => character;
    public CharacterTotalGamesUnlockCondition Condition => condition;

    public string UnlockDescription =>
        condition != null
            ? condition.Description
            : "No unlock requirement configured.";

    public bool IsConditionMet(
        GlobalProgressionManager progression)
    {
        if (character == null ||
            condition == null ||
            progression == null)
        {
            return false;
        }

        return condition.IsMet(progression);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (character == null)
        {
            Debug.LogWarning(
                $"Character unlock definition '{name}' has no " +
                $"character.",
                this);
        }

        if (condition == null)
        {
            Debug.LogWarning(
                $"Character unlock definition '{name}' has no " +
                $"condition.",
                this);
        }
    }
#endif
}
