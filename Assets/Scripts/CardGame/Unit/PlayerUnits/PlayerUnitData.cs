using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Player Unit Data", menuName = "Units/Player Data")]
public class PlayerUnitData : UnitData
{
    [Header("Identity & Progression")]
    [SerializeField] private string characterID;
    [SerializeField] private bool isUnlockedByDefault;

    [Header("Run Data")]
    [SerializeField] protected DeckData startingDeckData;
    [SerializeField] protected int drawAmount;
    [SerializeField] protected int maxEnergy;
    [SerializeField] protected CardClass playerCardClass;
    [SerializeField] private Sprite playerEnergyViewSprite;
    
    [Header("Character Starting Data")]
    [SerializeField] private Sprite playerCharacterSelectBackground;
    [FormerlySerializedAs("playerCharacterSelectPfp")] [SerializeField] private Sprite playerCharacterSelectPfpUnlocked;
    [SerializeField] private Sprite playerCharacterSelectPfpLocked;
    [SerializeField] private int playerStartingHealth;
    [SerializeField] private int playerStartingGold;
    [SerializeField] private RelicData startingRelicData;
    
    // Getters
    public string CharacterID => characterID;
    public bool IsUnlockedByDefault => isUnlockedByDefault;
    public DeckData StartingDeckData => startingDeckData;
    public int DrawAmount => drawAmount;
    public int MaxEnergy => maxEnergy;
    public CardClass PlayerCardClass => playerCardClass;
    public Sprite PlayerEnergyViewSprite => playerEnergyViewSprite;
    public RelicData StartingRelicData => startingRelicData;
    public Sprite PlayerCharacterSelectBackground => playerCharacterSelectBackground;
    public Sprite PlayerCharacterSelectPfpUnlocked => playerCharacterSelectPfpUnlocked;
    public Sprite PlayerCharacterSelectPfpLocked => playerCharacterSelectPfpLocked;
    public int PlayerStartingHealth => playerStartingHealth;
    public int PlayerStartingGold => playerStartingGold;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrWhiteSpace(characterID))
            return;

        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);

        if (string.IsNullOrWhiteSpace(assetPath))
            return;

        characterID =
            UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
