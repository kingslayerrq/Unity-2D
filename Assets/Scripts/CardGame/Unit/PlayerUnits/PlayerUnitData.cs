using UnityEngine;

[CreateAssetMenu(fileName = "New Player Unit Data", menuName = "Units/Player Data")]
public class PlayerUnitData : UnitData
{
    [SerializeField] protected DeckData startingDeckData;
    [SerializeField] protected int drawAmount;
    [SerializeField] protected int maxEnergy;
    [SerializeField] protected CardClass playerCardClass;
    [SerializeField] private Sprite playerEnergyViewSprite;
    // TODO: starting relic
    
    public DeckData StartingDeckData => startingDeckData;
    public int DrawAmount => drawAmount;
    public int MaxEnergy => maxEnergy;
    public CardClass PlayerCardClass => playerCardClass;
    public Sprite PlayerEnergyViewSprite => playerEnergyViewSprite;
}
