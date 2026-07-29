using UnityEngine;

[CreateAssetMenu(fileName = "New Relic Data", menuName = "Relics")]
public class RelicData : ScriptableObject
{
    [SerializeField] private string relicName;
    [SerializeField] private string relicDescription;
    [SerializeField] private Sprite relicIcon;
    
    public string RelicName => relicName;
    public string RelicDescription => relicDescription;
    public Sprite RelicIcon => relicIcon;
}
