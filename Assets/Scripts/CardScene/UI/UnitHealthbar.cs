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
    private Tweener healthTween;
    private void Awake()
    {
        _image = GetComponent<Image>();
        _unitInstance = GetComponentInParent<BaseUnitInstance>();
    }

    private void Start()
    {
        if (_unitInstance == null)
        {
            Debug.LogWarning("UnitHealthbar: Unit Instance is null");
            return;
        }
        _unitInstance.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        // Calculate the percentage (0.0 to 1.0) and apply it to the image
        float targetFill = (float)currentHealth / maxHealth;

        // Check if the animation is currently running
        if (healthTween != null && healthTween.IsActive() && healthTween.IsPlaying())
        {
            healthTween.ChangeEndValue(targetFill, drainDuration, true);
        }
        else
        {
            healthTween = _image.DOFillAmount(targetFill, drainDuration).SetEase(drainEase);
        }
    }

    private void OnDestroy()
    {
        if (_unitInstance == null) return;
        _unitInstance.OnHealthChanged -= UpdateHealthBar;
    }
}
