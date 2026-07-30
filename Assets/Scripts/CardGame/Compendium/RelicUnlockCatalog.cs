using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Relic Unlock Catalog",
    fileName = "RelicUnlockCatalog")]
public class RelicUnlockCatalog : ScriptableObject
{
    [SerializeField] private List<RelicUnlockDefinition> definitions = new();

    private Dictionary<string, RelicUnlockDefinition> definitionByRelicId;
    public IReadOnlyList<RelicUnlockDefinition> Definitions => definitions;
    
    public bool TryGetDefinition(
        RelicData relic,
        out RelicUnlockDefinition definition)
    {
        definition = null;

        if (relic == null ||
            string.IsNullOrWhiteSpace(relic.RelicId))
        {
            return false;
        }

        EnsureLookupCreated();

        return definitionByRelicId.TryGetValue(
            relic.RelicId,
            out definition);
    }

    private void EnsureLookupCreated()
    {
        if (definitionByRelicId != null)
            return;

        definitionByRelicId =
            new Dictionary<string, RelicUnlockDefinition>();

        foreach (RelicUnlockDefinition definition in definitions)
        {
            if (definition == null ||
                definition.Relic == null ||
                string.IsNullOrWhiteSpace(
                    definition.Relic.RelicId))
            {
                continue;
            }

            if (!definitionByRelicId.TryAdd(
                    definition.Relic.RelicId,
                    definition))
            {
                Debug.LogError(
                    $"Multiple unlock definitions exist for " +
                    $"'{definition.Relic.RelicName}'.",
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
            $"t:{nameof(RelicUnlockDefinition)}",
            new[] { UnlockDataRoot });

        List<string> assetPaths = new(guids.Length);

        foreach (string guid in guids)
        {
            assetPaths.Add(
                UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
        }

        assetPaths.Sort(System.StringComparer.Ordinal);

        List<RelicUnlockDefinition> foundDefinitions =
            new(assetPaths.Count);

        foreach (string assetPath in assetPaths)
        {
            RelicUnlockDefinition definition =
                UnityEditor.AssetDatabase
                    .LoadAssetAtPath<RelicUnlockDefinition>(
                        assetPath);

            if (definition != null)
                foundDefinitions.Add(definition);
        }

        UnityEditor.Undo.RecordObject(
            this,
            "Refresh Relic Unlock Catalog");

        definitions = foundDefinitions;
        definitionByRelicId = null;

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log(
            $"Loaded {definitions.Count} relic unlock " +
            $"definitions from '{UnlockDataRoot}'.",
            this);
    }

    private void OnValidate()
    {
        definitionByRelicId = null;
    }
#endif
}
