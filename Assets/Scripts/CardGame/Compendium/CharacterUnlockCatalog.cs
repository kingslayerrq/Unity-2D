using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Character Unlock Catalog",
    fileName = "CharacterUnlockCatalog")]
public sealed class CharacterUnlockCatalog : ScriptableObject
{
    [SerializeField]
    private List<CharacterUnlockDefinition> definitions = new();

    private Dictionary<string, CharacterUnlockDefinition>
        definitionsByCharacterId;

    public IReadOnlyList<CharacterUnlockDefinition> Definitions =>
        definitions;

    public bool TryGetDefinition(
        PlayerUnitData character,
        out CharacterUnlockDefinition definition)
    {
        definition = null;

        if (character == null ||
            string.IsNullOrWhiteSpace(character.CharacterID))
        {
            return false;
        }

        EnsureLookupCreated();

        return definitionsByCharacterId.TryGetValue(
            character.CharacterID,
            out definition);
    }

    private void EnsureLookupCreated()
    {
        if (definitionsByCharacterId != null)
            return;

        definitionsByCharacterId =
            new Dictionary<string, CharacterUnlockDefinition>();

        foreach (CharacterUnlockDefinition definition in definitions)
        {
            if (definition == null ||
                definition.Character == null ||
                string.IsNullOrWhiteSpace(
                    definition.Character.CharacterID))
            {
                continue;
            }

            if (!definitionsByCharacterId.TryAdd(
                    definition.Character.CharacterID,
                    definition))
            {
                Debug.LogError(
                    $"Multiple unlock definitions exist for " +
                    $"'{definition.Character.UnitName}'.",
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
            $"t:{nameof(CharacterUnlockDefinition)}",
            new[] { UnlockDataRoot });

        List<string> assetPaths = new(guids.Length);

        foreach (string guid in guids)
        {
            assetPaths.Add(
                UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
        }

        assetPaths.Sort(System.StringComparer.Ordinal);

        List<CharacterUnlockDefinition> foundDefinitions =
            new(assetPaths.Count);

        foreach (string assetPath in assetPaths)
        {
            CharacterUnlockDefinition definition =
                UnityEditor.AssetDatabase
                    .LoadAssetAtPath<CharacterUnlockDefinition>(
                        assetPath);

            if (definition != null)
                foundDefinitions.Add(definition);
        }

        UnityEditor.Undo.RecordObject(
            this,
            "Refresh Character Unlock Catalog");

        definitions = foundDefinitions;
        definitionsByCharacterId = null;

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log(
            $"Loaded {definitions.Count} character unlock " +
            $"definitions from '{UnlockDataRoot}'.",
            this);
    }

    private void OnValidate()
    {
        definitionsByCharacterId = null;
    }
#endif
}
