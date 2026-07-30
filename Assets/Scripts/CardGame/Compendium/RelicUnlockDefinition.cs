using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Relic Unlock Definition")]
public class RelicUnlockDefinition : ScriptableObject
{
    [SerializeField] private RelicData relic;
    [SerializeField] private UnlockCondition condition;
    
    public RelicData Relic => relic;
    public UnlockCondition Condition => condition;
    public string UnlockDescription =>
        condition != null
            ? condition.Description
            : "No unlock requirement configured.";
    
    public bool IsConditionMet(
        GlobalProgressionManager progression)
    {
        if (relic == null || condition == null)
            return false;

        return condition.IsMet(progression);
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (relic == null)
        {
            Debug.LogWarning(
                $"Unlock definition '{name}' has no relic.",
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
