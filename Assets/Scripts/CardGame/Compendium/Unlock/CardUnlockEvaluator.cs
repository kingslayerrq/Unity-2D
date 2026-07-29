using UnityEngine;

public sealed class CardUnlockEvaluator : MonoBehaviour
{
    [SerializeField]
    private CardUnlockCatalog unlockCatalog;

    [SerializeField]
    private GlobalProgressionManager progression;

    private bool isEvaluating;

    private void OnEnable()
    {
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
        // UnlockCard may invoke ProgressionChanged again.
        // This prevents recursive evaluation.
        if (isEvaluating)
            return;

        isEvaluating = true;

        try
        {
            foreach (CardUnlockDefinition definition
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

    private void Evaluate(CardUnlockDefinition definition)
    {
        if (definition == null ||
            definition.Card == null ||
            definition.Condition == null)
        {
            return;
        }

        if (progression.IsUnlocked(definition.Card))
            return;

        if (!definition.IsConditionMet(progression))
            return;

        progression.UnlockCard(definition.Card);

        Debug.Log(
            $"Unlocked card: {definition.Card.CardName}");
    }
}