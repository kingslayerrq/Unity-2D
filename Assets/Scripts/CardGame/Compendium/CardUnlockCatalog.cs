using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Card Unlock Catalog")]
public sealed class CardUnlockCatalog : ScriptableObject
{
    [SerializeField]
    private List<CardUnlockDefinition> definitions = new();

    private Dictionary<string, CardUnlockDefinition>
        definitionsByCardId;

    public IReadOnlyList<CardUnlockDefinition> Definitions =>
        definitions;

    public bool TryGetDefinition(
        CardData card,
        out CardUnlockDefinition definition)
    {
        if (card == null)
        {
            definition = null;
            return false;
        }

        EnsureLookupCreated();

        return definitionsByCardId.TryGetValue(
            card.ID,
            out definition);
    }

    private void EnsureLookupCreated()
    {
        if (definitionsByCardId != null)
            return;

        definitionsByCardId =
            new Dictionary<string, CardUnlockDefinition>();

        foreach (CardUnlockDefinition definition in definitions)
        {
            if (definition == null ||
                definition.Card == null)
            {
                continue;
            }

            if (!definitionsByCardId.TryAdd(
                    definition.Card.ID,
                    definition))
            {
                Debug.LogError(
                    $"Multiple unlock definitions exist for " +
                    $"'{definition.Card.CardName}'.",
                    this);
            }
        }
    }

#if UNITY_EDITOR
    private const string UnlockDataRoot =
        "Assets/Data/CompendiumData/UnlockData";

    [ContextMenu("Refresh Catalog From UnlockData")]
    public void RefreshFromUnlockData()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets(
            $"t:{nameof(CardUnlockDefinition)}",
            new[] { UnlockDataRoot });

        List<string> assetPaths = new(guids.Length);

        foreach (string guid in guids)
        {
            assetPaths.Add(
                UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
        }

        assetPaths.Sort(System.StringComparer.Ordinal);

        List<CardUnlockDefinition> foundDefinitions =
            new(assetPaths.Count);

        foreach (string assetPath in assetPaths)
        {
            CardUnlockDefinition definition =
                UnityEditor.AssetDatabase
                    .LoadAssetAtPath<CardUnlockDefinition>(assetPath);

            if (definition != null)
                foundDefinitions.Add(definition);
        }

        UnityEditor.Undo.RecordObject(
            this,
            "Refresh Card Unlock Catalog");

        definitions = foundDefinitions;
        definitionsByCardId = null;

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log(
            $"Loaded {definitions.Count} unlock definitions from " +
            $"'{UnlockDataRoot}'.",
            this);
    }

    private void OnValidate()
    {
        definitionsByCardId = null;
    }
#endif
}
