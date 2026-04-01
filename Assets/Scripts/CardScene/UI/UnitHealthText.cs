using System;
using TMPro;
using UnityEngine;

public class UnitHealthText : MonoBehaviour
{
    private TMP_Text _text;
    private BaseUnitInstance _unitInstance;
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();

    }

    public void Initialize(BaseUnitInstance unitInstance)
    {
        if (_unitInstance != null)
        {
            _unitInstance.OnHealthChanged -= UpdateHealthText;
        }
        
        _unitInstance = unitInstance;

        if (_unitInstance != null)
        {
            _unitInstance.OnHealthChanged += UpdateHealthText;
            UpdateHealthText(_unitInstance.CurrentHealth, unitInstance.CurrentMaxHealth);
        }
    }

    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        _text.text = $"{currentHealth}/{maxHealth}";
    }

    private void OnDestroy()
    {
        if (_unitInstance != null) _unitInstance.OnHealthChanged -= UpdateHealthText;
    }
}
