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
        _unitInstance = GetComponentInParent<BaseUnitInstance>();
    }

    private void Start()
    {
        if (_unitInstance) _unitInstance.OnHealthChanged += UpdateHealthText;

    }

    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        _text.text = $"{currentHealth}/{maxHealth}";
    }

    private void OnDestroy()
    {
        if (_unitInstance) _unitInstance.OnHealthChanged -= UpdateHealthText;
    }
}
