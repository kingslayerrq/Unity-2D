using UnityEngine;

/// <summary>
/// Placeholder for ChooseCharacterScene. Assign testPlayerUnitData in the Inspector
/// and call SelectCharacter() from a UI button.
/// </summary>
public class ChooseCharacterSceneController : MonoBehaviour
{
    [SerializeField] private PlayerUnitData testPlayerUnitData;

    public void SelectCharacter()
    {
        RunManager.Instance.ChoosePlayerUnit(testPlayerUnitData);
    }
}
