using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds every playable character definition.
/// </summary>
[CreateAssetMenu(
    menuName = "Progression/Character Catalog",
    fileName = "CharacterCatalog")]
public sealed class CharacterCatalog : ScriptableObject
{
    [SerializeField]
    private List<PlayerUnitData> characters = new();

    private Dictionary<string, PlayerUnitData> charactersById;

    public IReadOnlyList<PlayerUnitData> Characters => characters;

    public bool TryGetCharacter(
        string characterId,
        out PlayerUnitData character)
    {
        character = null;

        if (string.IsNullOrWhiteSpace(characterId))
            return false;

        EnsureLookupCreated();

        return charactersById.TryGetValue(
            characterId,
            out character);
    }

    public bool TryGetCharacter(
        CardClass cardClass,
        out PlayerUnitData character)
    {
        foreach (PlayerUnitData candidate in characters)
        {
            if (candidate != null &&
                candidate.PlayerCardClass == cardClass)
            {
                character = candidate;
                return true;
            }
        }

        character = null;
        return false;
    }

    private void EnsureLookupCreated()
    {
        if (charactersById != null)
            return;

        charactersById = new Dictionary<string, PlayerUnitData>();

        foreach (PlayerUnitData character in characters)
        {
            if (character == null ||
                string.IsNullOrWhiteSpace(character.CharacterID))
            {
                continue;
            }

            if (!charactersById.TryAdd(
                    character.CharacterID,
                    character))
            {
                Debug.LogError(
                    $"Duplicate character ID " +
                    $"'{character.CharacterID}' found in catalog.",
                    this);
            }
        }
    }

#if UNITY_EDITOR
    private const string CharacterDataRoot =
        "Assets/Data/UnitData/PlayerUnitData";

    [ContextMenu("Refresh Catalog From PlayerUnitData")]
    public void RefreshFromPlayerUnitData()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets(
            $"t:{nameof(PlayerUnitData)}",
            new[] { CharacterDataRoot });

        List<string> assetPaths = new(guids.Length);

        foreach (string guid in guids)
        {
            assetPaths.Add(
                UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
        }

        assetPaths.Sort(System.StringComparer.Ordinal);

        List<PlayerUnitData> foundCharacters =
            new(assetPaths.Count);

        foreach (string assetPath in assetPaths)
        {
            PlayerUnitData character =
                UnityEditor.AssetDatabase
                    .LoadAssetAtPath<PlayerUnitData>(assetPath);

            if (character != null)
                foundCharacters.Add(character);
        }

        UnityEditor.Undo.RecordObject(
            this,
            "Refresh Character Catalog");

        characters = foundCharacters;
        charactersById = null;

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log(
            $"Loaded {characters.Count} characters from " +
            $"'{CharacterDataRoot}'.",
            this);
    }

    private void OnValidate()
    {
        charactersById = null;

        HashSet<string> ids = new();

        foreach (PlayerUnitData character in characters)
        {
            if (character == null)
                continue;

            if (string.IsNullOrWhiteSpace(character.CharacterID))
            {
                Debug.LogError(
                    $"Character '{character.name}' has no " +
                    $"persistent ID.",
                    character);

                continue;
            }

            if (!ids.Add(character.CharacterID))
            {
                Debug.LogError(
                    $"Duplicate character ID " +
                    $"'{character.CharacterID}' on " +
                    $"'{character.name}'.",
                    character);
            }
        }
    }
#endif
}
