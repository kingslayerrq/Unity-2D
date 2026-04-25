using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitEnergyView : MonoBehaviour
{
    private Image _image;
    private TMP_Text _text;
    private BasePlayerUnitInstance _unitInstance;
    
    private void Awake()
    {
        _image = GetComponentInChildren<Image>();
        _text = GetComponentInChildren<TMP_Text>();
    }
    
    
    /// <summary>
    /// Called by UnitView when the prefab is spawned and linked
    /// </summary>
    /// <param name="unitInstance"></param>
    public void Initialize(BasePlayerUnitInstance unitInstance)
    {
        if (unitInstance == null) return;
        if (_unitInstance != null)
            _unitInstance.OnEnergyChanged -= UpdateEnergyText;
        _unitInstance = unitInstance;
        _unitInstance.OnEnergyChanged += UpdateEnergyText;

        _image.sprite = unitInstance.PlayerUnitData.PlayerEnergyViewSprite;

        UpdateEnergyText(_unitInstance.CurrentEnergy, _unitInstance.CurrentMaxEnergy);
    }

    private void UpdateEnergyText(int currentEnergy, int maxEnergy)
    {
        Debug.Log("UpdateEnergyText");
        _text.text = $"{currentEnergy}/{maxEnergy}";
    }
}
