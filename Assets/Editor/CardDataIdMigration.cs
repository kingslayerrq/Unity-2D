#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CardDataIdMigration
{
    private const string MenuPath =
        "Tools/Card Game/Migrations/Assign Missing Card IDs";

    private const string CardDataRoot = "Assets/Data/CardData";
    private const string IdPropertyName = "id";

    [MenuItem(MenuPath)]
    private static void AssignMissingCardIds()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Assign Missing Card IDs",
            "Assign each blank CardData ID from the asset's existing " +
            ".meta GUID?\n\nExisting IDs will be preserved. Every " +
            "CardData asset will be saved so IDs currently held only " +
            "in Unity's cache are written to disk.",
            "Assign Missing IDs",
            "Cancel");

        if (!confirmed)
            return;

        string[] assetGuids = AssetDatabase.FindAssets(
            $"t:{nameof(CardData)}",
            new[] { CardDataRoot });

        Array.Sort(assetGuids, StringComparer.Ordinal);

        Dictionary<string, string> idOwners =
            new(StringComparer.Ordinal);

        int assignedCount = 0;
        int preservedCount = 0;
        int problemCount = 0;

        foreach (string assetGuid in assetGuids)
        {
            string assetPath =
                AssetDatabase.GUIDToAssetPath(assetGuid);

            CardData card =
                AssetDatabase.LoadAssetAtPath<CardData>(assetPath);

            if (card == null)
            {
                problemCount++;
                Debug.LogError(
                    $"Could not load CardData at '{assetPath}'.");
                continue;
            }

            SerializedObject serializedCard = new(card);

            bool assigned;

            try
            {
                assigned = AssignMissingId(
                    serializedCard,
                    assetGuid);
            }
            catch (InvalidOperationException exception)
            {
                problemCount++;
                Debug.LogError(
                    $"Could not migrate CardData at '{assetPath}': " +
                    exception.Message,
                    card);
                continue;
            }

            if (assigned)
            {
                assignedCount++;
                Debug.Log(
                    $"Assigned card ID '{assetGuid}' to " +
                    $"'{assetPath}'.",
                    card);
            }
            else
            {
                preservedCount++;
            }

            // Some IDs can exist in Unity's imported-object cache even
            // when the YAML asset has never serialized the field. Mark and
            // save each CardData individually to persist that cached value
            // without saving unrelated dirty project assets.
            EditorUtility.SetDirty(card);
            AssetDatabase.SaveAssetIfDirty(card);

            string cardId = card.ID;

            if (string.IsNullOrWhiteSpace(cardId))
            {
                problemCount++;
                Debug.LogError(
                    $"Card '{assetPath}' still has no persistent ID.",
                    card);
                continue;
            }

            if (idOwners.TryGetValue(
                    cardId,
                    out string existingAssetPath))
            {
                problemCount++;
                Debug.LogError(
                    $"Duplicate card ID '{cardId}' found on " +
                    $"'{existingAssetPath}' and '{assetPath}'.",
                    card);
                continue;
            }

            idOwners.Add(cardId, assetPath);
        }

        AssetDatabase.Refresh();

        string result =
            $"Card ID migration complete. Assigned {assignedCount} " +
            $"missing ID(s), preserved {preservedCount} existing " +
            $"ID(s), and found {problemCount} problem(s).";

        if (problemCount == 0)
            Debug.Log(result);
        else
            Debug.LogError(result);

        EditorUtility.DisplayDialog(
            "Card ID Migration Complete",
            result,
            "OK");
    }

    internal static bool AssignMissingId(
        SerializedObject serializedCard,
        string assetGuid)
    {
        if (serializedCard == null)
            throw new ArgumentNullException(nameof(serializedCard));

        if (string.IsNullOrWhiteSpace(assetGuid))
        {
            throw new ArgumentException(
                "An asset GUID is required.",
                nameof(assetGuid));
        }

        serializedCard.UpdateIfRequiredOrScript();

        SerializedProperty idProperty =
            serializedCard.FindProperty(IdPropertyName);

        if (idProperty == null)
        {
            throw new InvalidOperationException(
                $"Serialized property '{IdPropertyName}' was not found.");
        }

        if (!string.IsNullOrWhiteSpace(idProperty.stringValue))
            return false;

        idProperty.stringValue = assetGuid;
        serializedCard.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateAssignMissingCardIds()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode &&
               !EditorApplication.isCompiling;
    }
}
#endif
