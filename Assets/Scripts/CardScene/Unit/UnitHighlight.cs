using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UnitHighlight : MonoBehaviour, ICardHoverInteraction
{
    private BaseUnitInstance _unitInstance;
    private Image _highlightImage;
    public bool IsHighlighted { get; private set; } = false;

    public void Initialize(BaseUnitInstance unitInstance)
    {
        if (_unitInstance != null)
        {
            _unitInstance.OnHighlightTriggered -= HandleHighlight;
        }
        _unitInstance = unitInstance;

        if (_unitInstance != null)
        {
            _unitInstance.OnHighlightTriggered += HandleHighlight;
        }
    }

    private void Awake()
    {
        if (_highlightImage == null) _highlightImage = GetComponent<Image>();
        _highlightImage.enabled = false;
    }

    private void HandleHighlight(bool show)
    {
        if (show)
        {
            HighlightUnit();
        }
        else
        {
            HideHighlight();
        }
    }
    public void HighlightUnit()
    {
        if (_highlightImage.enabled) return;
        _highlightImage.enabled = true;
        IsHighlighted = true;
    }

    public void HideHighlight()
    {
        if (!_highlightImage.enabled) return;
        _highlightImage.enabled = false;
        IsHighlighted = false;
    }

    private void OnDisable()
    {
        if (_unitInstance != null)
        {
            _unitInstance.OnHighlightTriggered -= HandleHighlight;
        }
    }
}
