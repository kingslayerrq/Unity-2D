using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Units/Enemy Data")]
public class EnemyUnitData : UnitData
{
    [SerializeField] protected string enemyId;
    [SerializeField] protected bool isBoss = false;
    [FormerlySerializedAs("availableActions")]
    [Header("Available Intents")]
    [SerializeField] protected List<EnemyIntentData> availableIntents;

    public string EnemyId => enemyId;

    public bool IsBoss => isBoss;

    public IReadOnlyList<EnemyIntentData> AvailableIntents => availableIntents;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrWhiteSpace(enemyId))
            return;

        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);

        if (string.IsNullOrWhiteSpace(assetPath))
            return;

        enemyId =
            UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}