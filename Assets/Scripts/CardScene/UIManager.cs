using UnityEngine;

public class UnitUIManager : MonoBehaviour
{
    [SerializeField] private PlayerHandView playerHandView;
    [SerializeField] private UnitHighlight unitHighlight;
    [SerializeField] private EndTurnButton endTurnButton;
    
    public PlayerHandView PlayerHandView => playerHandView;
    public UnitHighlight UnitHighlight => unitHighlight;
    


}
