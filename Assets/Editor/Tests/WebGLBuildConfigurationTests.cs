using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;

public sealed class WebGLBuildConfigurationTests
{
    private const string BootstrapScene =
        "Assets/Scenes/CardGameScene/EntryScene.unity";

    [Test]
    public void EntrySceneIsFirstEnabledBuildScene()
    {
        EditorBuildSettingsScene[] enabledScenes =
            EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .ToArray();

        Assert.That(enabledScenes, Is.Not.Empty);
        Assert.That(enabledScenes[0].path, Is.EqualTo(BootstrapScene));
    }

    [Test]
    public void WebGLDoesNotDefineDiscordSocialSdk()
    {
        string[] defines = PlayerSettings
            .GetScriptingDefineSymbols(
                NamedBuildTarget.FromBuildTargetGroup(
                    BuildTargetGroup.WebGL))
            .Split(';');

        Assert.That(
            defines,
            Does.Not.Contain("DISCORD_SOCIAL_SDK_EXISTS"));
        Assert.That(
            DiscordSocialSDKDetector.IsSupported(BuildTargetGroup.WebGL),
            Is.False);
    }
}
