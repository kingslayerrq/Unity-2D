#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using System.Linq;

/// <summary>
/// Detects the presence of the Discord Social SDK and adds the DISCORD_SOCIAL_SDK_EXISTS define symbol if it is present.
/// Without this all of the code using the Discord Social SDK will not compile and Unity will start with errors in safe mode.
/// The SDK only ships native libraries for desktop and mobile targets, so the
/// define must never be enabled for WebGL.
/// 
/// In your own game you can safely remove this file once the Discord Social SDK package is installed.
/// </summary>
[InitializeOnLoad]
public class DiscordSocialSDKDetector
{
    static DiscordSocialSDKDetector()
    {
        var discordSocialSDKExists = System.AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.GetName().Name == "Discord.Sdk");

        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var shouldEnable = discordSocialSDKExists && IsSupported(targetGroup);
        var namedBuildTarget =
            NamedBuildTarget.FromBuildTargetGroup(targetGroup);
        var defines =
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        var definesList = defines.Split(';').ToList();

        if (shouldEnable && !definesList.Contains("DISCORD_SOCIAL_SDK_EXISTS"))
        {
            definesList.Add("DISCORD_SOCIAL_SDK_EXISTS");
            PlayerSettings.SetScriptingDefineSymbols(
                namedBuildTarget,
                string.Join(";", definesList));
        }
        else if (!shouldEnable && definesList.Contains("DISCORD_SOCIAL_SDK_EXISTS"))
        {
            definesList.Remove("DISCORD_SOCIAL_SDK_EXISTS");
            PlayerSettings.SetScriptingDefineSymbols(
                namedBuildTarget,
                string.Join(";", definesList));
        }
    }

    internal static bool IsSupported(BuildTargetGroup targetGroup)
    {
        return targetGroup == BuildTargetGroup.Standalone ||
               targetGroup == BuildTargetGroup.Android ||
               targetGroup == BuildTargetGroup.iOS;
    }
}
#endif
