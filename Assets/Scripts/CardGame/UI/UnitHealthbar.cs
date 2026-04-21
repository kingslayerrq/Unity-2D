using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(Image))]
public class UnitHealthbar : MonoBehaviour
{
    private Image _image;
    private BaseUnitInstance _unitInstance;
    
    [Header("Animation Settings")]
    [SerializeField] private float drainDuration = 0.5f; 
    [SerializeField] private Ease drainEase = Ease.OutCubic;
    private Tweener _healthTween;
    
    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    /// <summary>
    /// Called by UnitView when the prefab is spawned and linked
    /// </summary>
    /// <param name="unitInstance"></param>
    public void Initialize(BaseUnitInstance unitInstance)
    {
        if (_unitInstance != null)
        {
            _unitInstance.OnHealthChanged -= UpdateHealthBar;
        }
        _unitInstance = unitInstance;

        if (_unitInstance != null)
        {
            _unitInstance.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(_unitInstance.CurrentHealth, _unitInstance.CurrentMaxHealth);
        }
    }
    

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        // Calculate the percentage (0.0 to 1.0) and apply it to the image
        float targetFill = (float)currentHealth / maxHealth;

        // Check if the animation is currently running
        if (_healthTween != null && _healthTween.IsActive() && _healthTween.IsPlaying())
        {
            _healthTween.ChangeEndValue(targetFill, drainDuration, true);
        }
        else
        {
            _healthTween = _image.DOFillAmount(targetFill, drainDuration).SetEase(drainEase);
        }
    }

    private void OnDestroy()
    {
        if (_unitInstance == null) return;
        _unitInstance.OnHealthChanged -= UpdateHealthBar;
        
        transform.DOKill();
    }
}
