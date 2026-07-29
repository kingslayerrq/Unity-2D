#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static class ProgressionSaveEditor
{
    private const string ResetMenuPath =
        "Tools/Card Game/Progression/Reset Local Progression";

    [MenuItem(ResetMenuPath)]
    private static void ResetLocalProgression()
    {
        LocalJsonProgressionStore store =
            new(Application.persistentDataPath);

        bool confirmed = EditorUtility.DisplayDialog(
            "Reset Local Progression",
            "Delete the active progression save, backup, and " +
            "temporary file?\n\n" +
            $"Location:\n{store.Location}",
            "Reset Progression",
            "Cancel");

        if (!confirmed)
            return;

        try
        {
            store.Reset();

            Debug.Log(
                $"Local progression reset at '{store.Location}'.");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to reset local progression:\n{exception}");
        }
    }

    [MenuItem(ResetMenuPath, true)]
    private static bool ValidateResetLocalProgression()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
#endif
