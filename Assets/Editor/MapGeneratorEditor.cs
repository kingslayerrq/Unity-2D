#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        MapGenerator generator = (MapGenerator)target;

        if (GUILayout.Button("Generate Map"))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Generate Map");
            generator.GenerateMap();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Randomize Seed + Generate"))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Randomize Seed + Generate");
            generator.GenerateNewMapWithRandomSeed();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Clear Generated Map"))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Clear Generated Map");
            generator.ClearGeneratedMap();
            EditorUtility.SetDirty(generator);
        }
    }
}
#endif