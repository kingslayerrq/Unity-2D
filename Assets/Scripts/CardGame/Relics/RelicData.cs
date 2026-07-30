using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Relic Data", menuName = "Relics")]
public class RelicData : ScriptableObject
{
    [Header("Identity & Progression")]
    [SerializeField] private string relicId;
    [SerializeField] private bool isUnlockedByDefault;
    [SerializeField] private bool isDiscoveredByDefault;
    
    [SerializeField] private string relicName;
    [TextArea(3, 10)]
    [SerializeField] private string relicDescription;
    [SerializeField] private List<KeywordData> keywords;
    [SerializeField] private Sprite relicIcon;
    [SerializeField] private Sprite relicIconUndiscovered;
    [SerializeField] private RelicType relicType;
    
    public string RelicId => relicId;
    public bool IsUnlockedByDefault => isUnlockedByDefault;
    public bool IsDiscoveredByDefault => isDiscoveredByDefault;
    
    public string RelicName => relicName;
    public string RelicDescription => relicDescription;
    public IReadOnlyList<KeywordData> Keywords => keywords;
    public Sprite RelicIcon => relicIcon;
    public Sprite RelicIconUndiscovered => relicIconUndiscovered;
    public RelicType RelicType => relicType;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrWhiteSpace(relicId))
            return;

        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);

        if (string.IsNullOrWhiteSpace(assetPath))
            return;

        relicId =
            UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}

public enum RelicType
{
    Starter,
    Shop,
    Event,
    Common,
    Uncommon,
    Rare,
    Ancient
}
