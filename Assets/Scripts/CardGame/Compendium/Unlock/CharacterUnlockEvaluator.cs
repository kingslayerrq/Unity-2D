using UnityEngine;

public sealed class CharacterUnlockEvaluator : MonoBehaviour
{
    [SerializeField]
    private CharacterUnlockCatalog unlockCatalog;

    [SerializeField]
    private GlobalProgressionManager progression;

    private bool isEvaluating;

    private void OnEnable()
    {
        if (progression == null)
            progression = GlobalProgressionManager.Instance;

        if (progression != null)
        {
            progression.ProgressionChanged +=
                HandleProgressionChanged;
        }
    }

    private void Start()
    {
        EvaluateAll();
    }

    private void OnDisable()
    {
        if (progression != null)
        {
            progression.ProgressionChanged -=
                HandleProgressionChanged;
        }
    }

    private void HandleProgressionChanged()
    {
        EvaluateAll();
    }

    public void EvaluateAll()
    {
        if (isEvaluating ||
            unlockCatalog == null ||
            progression == null)
        {
            return;
        }

        isEvaluating = true;

        try
        {
            foreach (CharacterUnlockDefinition definition
                     in unlockCatalog.Definitions)
            {
                Evaluate(definition);
            }
        }
        finally
        {
            isEvaluating = false;
        }
    }

    private void Evaluate(
        CharacterUnlockDefinition definition)
    {
        if (definition == null ||
            definition.Character == null ||
            definition.Condition == null)
        {
            return;
        }

        if (progression.IsUnlocked(definition.Character))
            return;

        if (!definition.IsConditionMet(progression))
            return;

        progression.UnlockCharacter(definition.Character);

        Debug.Log(
            $"Unlocked character: " +
            $"{definition.Character.UnitName}");
    }
}
