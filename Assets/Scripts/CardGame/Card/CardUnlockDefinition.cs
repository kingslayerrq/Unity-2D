using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Card Unlock Definition")]
public sealed class CardUnlockDefinition : ScriptableObject
{
    [SerializeField]
    private CardData card;

    [SerializeField]
    private UnlockCondition condition;

    public CardData Card => card;
    public UnlockCondition Condition => condition;

    public string UnlockDescription =>
        condition != null
            ? condition.Description
            : "No unlock requirement configured.";

    public bool IsConditionMet(
        GlobalProgressionManager progression)
    {
        if (card == null || condition == null)
            return false;

        return condition.IsMet(progression);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (card == null)
        {
            Debug.LogWarning(
                $"Unlock definition '{name}' has no card.",
                this);
        }

        if (condition == null)
        {
            Debug.LogWarning(
                $"Unlock definition '{name}' has no condition.",
                this);
        }
    }
#endif
}