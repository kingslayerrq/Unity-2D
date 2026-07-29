using System.Collections.Generic;
using UnityEngine;

public sealed class CompendiumViewer : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CardCatalog cardCatalog;

    [SerializeField]
    private CardUnlockCatalog unlockCatalog;

    [SerializeField]
    private GlobalProgressionManager progression;

    [Header("UI")]
    [SerializeField]
    private Transform entryContainer;

    [SerializeField]
    private CompendiumCardEntryView entryPrefab;

    private readonly List<CompendiumCardEntryView>
        spawnedEntries = new();

    private void OnEnable()
    {
        if (progression == null)
            progression = GlobalProgressionManager.Instance;

        if (progression == null)
        {
            Debug.LogError(
                "CompendiumViewer could not find " +
                "GlobalProgressionManager.",
                this);

            return;
        }

        progression.CardProgressChanged +=
            HandleCardProgressChanged;

        BuildCardCompendium();
    }

    private void OnDisable()
    {
        if (progression != null)
        {
            progression.CardProgressChanged -=
                HandleCardProgressChanged;
        }
    }

    public void BuildCardCompendium()
    {
        ClearEntries();

        foreach (CardData card in cardCatalog.Cards)
        {
            if (card == null)
                continue;

            if (!progression.TryGetProgress(
                    card,
                    out CardProgressRecord progress))
            {
                Debug.LogError(
                    $"No progression record exists for " +
                    $"'{card.CardName}'.",
                    card);

                continue;
            }

            unlockCatalog.TryGetDefinition(
                card,
                out CardUnlockDefinition unlockDefinition);

            CompendiumCardEntryView entry =
                Instantiate(entryPrefab, entryContainer);

            entry.Bind(
                card,
                progress,
                unlockDefinition);

            spawnedEntries.Add(entry);
        }
    }

    private void HandleCardProgressChanged(
        CardData changedCard,
        CardProgressRecord changedProgress)
    {
        foreach (CompendiumCardEntryView entry
                 in spawnedEntries)
        {
            if (entry.CurrentCard != changedCard)
                continue;

            unlockCatalog.TryGetDefinition(
                changedCard,
                out CardUnlockDefinition unlockDefinition);

            entry.Bind(
                changedCard,
                changedProgress,
                unlockDefinition);

            return;
        }
    }

    private void ClearEntries()
    {
        foreach (CompendiumCardEntryView entry
                 in spawnedEntries)
        {
            if (entry != null)
                Destroy(entry.gameObject);
        }

        spawnedEntries.Clear();
    }
}