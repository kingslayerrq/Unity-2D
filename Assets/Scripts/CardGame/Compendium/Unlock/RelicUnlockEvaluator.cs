using System;
using UnityEngine;

public sealed class RelicUnlockEvaluator : MonoBehaviour
{
    [SerializeField] private RelicUnlockCatalog unlockCatalog;

    [SerializeField] private GlobalProgressionManager progression;

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
            foreach (RelicUnlockDefinition definition
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
        RelicUnlockDefinition definition)
    {
        if (definition == null ||
            definition.Relic == null ||
            definition.Condition == null)
        {
            return;
        }

        if (progression.IsUnlocked(definition.Relic))
            return;

        if (!definition.IsConditionMet(progression))
            return;

        progression.UnlockRelic(definition.Relic);

        Debug.Log(
            $"Unlocked relic: " +
            $"{definition.Relic.RelicName}");
    }
}
