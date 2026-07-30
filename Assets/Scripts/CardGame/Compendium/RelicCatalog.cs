using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all relics definition
/// </summary>
[CreateAssetMenu(menuName = "Progression/Relic Catalog", fileName =  "RelicCatalog")]
public sealed class RelicCatalog : ScriptableObject
{
    [SerializeField] private List<RelicData> relics = new();
    
    private Dictionary<string, RelicData> relicsById;
    public IReadOnlyList<RelicData> Relics => relics;
    
    public bool TryGetRelic(string id, out RelicData relic)
    {
        EnsureLookupCreated();
        return relicsById.TryGetValue(id, out relic);
    }
    
    private void EnsureLookupCreated()
    {
        if (relicsById != null)
            return;

        relicsById = new Dictionary<string, RelicData>();

        foreach (RelicData relic in relics)
        {
            if (relic == null ||
                string.IsNullOrWhiteSpace(relic.RelicId))
            {
                continue;
            }

            if (!relicsById.TryAdd(
                    relic.RelicId,
                    relic))
            {
                Debug.LogError(
                    $"Duplicate relic ID " +
                    $"'{relic.RelicId}' found in catalog.",
                    this);
            }
        }
    }
    
#if UNITY_EDITOR
    private const string RelicDataRoot = "Assets/Data/RelicData";

    [ContextMenu("Refresh Catalog From RelicData")]
    public void RefreshFromRelicData()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets(
            $"t:{nameof(RelicData)}",
            new[] { RelicDataRoot });

        List<string> assetPaths = new(guids.Length);

        foreach (string guid in guids)
        {
            assetPaths.Add(
                UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
        }

        assetPaths.Sort(System.StringComparer.Ordinal);

        List<RelicData> foundRelics = new(assetPaths.Count);

        foreach (string assetPath in assetPaths)
        {
            RelicData relic =
                UnityEditor.AssetDatabase.LoadAssetAtPath<RelicData>(
                    assetPath);

            if (relic != null)
                foundRelics.Add(relic);
        }

        UnityEditor.Undo.RecordObject(
            this,
            "Refresh Relic Catalog");

        relics = foundRelics;
        relicsById = null;

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log(
            $"Loaded {relics.Count} cards from '{RelicDataRoot}'.",
            this);
    }

    private void OnValidate()
    {
        relicsById = null;

        HashSet<string> ids = new();

        foreach (RelicData relic in relics)
        {
            if (relic == null)
                continue;

            if (string.IsNullOrWhiteSpace(relic.RelicId))
            {
                Debug.LogError(
                    $"Relic '{relic.name}' has no persistent ID.",
                    relic);

                continue;
            }

            if (!ids.Add(relic.RelicId))
            {
                Debug.LogError(
                    $"Duplicate relic ID '{relic.RelicId}' on '{relic.name}'.",
                    relic);
            }
        }
    }
#endif
}
