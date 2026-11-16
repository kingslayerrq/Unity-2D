# Unity 2D

This project uses Unity `6000.3.10f1`.

## Code structure

Game rules are plain C# classes and are kept apart from the MonoBehaviours
that show them. Read [ARCHITECTURE.md](ARCHITECTURE.md) before adding a
feature; it says where each kind of code goes and has step-by-step recipes.

## WebGL deployment

Pushes to `main` run the EditMode test suite, build the WebGL player with
GameCI, and upload the generated player to the `html5` channel for
`Kingslayerrq/sts-clone` on itch.io.

`Assets/Scenes/CardGameScene/EntryScene.unity` is the bootstrap scene and must
remain the first enabled scene in **File > Build Profiles > Scene List**. The
Discord Social SDK is supported only in standalone, Android, and iOS players;
it is intentionally excluded from WebGL builds.

The workflow requires these repository secrets:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`
- `BUTLER_API_KEY`

Run EditMode tests locally from **Window > General > Test Runner**, or in
batch mode:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath . -runTests -testPlatform EditMode `
  -testResults TestResults.xml -logFile TestResults.log
```
