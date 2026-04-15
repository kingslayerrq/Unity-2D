using System;
using UnityEngine;

public class UnitView : MonoBehaviour
{
    public BaseUnitInstance UnitInstance { get; private set; }
    private UnitHealthbar _unitHealthbar;
    private UnitHealthText _unitHealthText;
    private UnitTarget _unitTarget;
    private UnitHighlight _unitHighlight;
    private EndTurnButton _endTurnButton;

    private void Awake()
    {
        _unitHealthbar = GetComponentInChildren<UnitHealthbar>();
        _unitHealthText = GetComponentInChildren<UnitHealthText>();
        _unitTarget = GetComponent<UnitTarget>();
        _unitHighlight = GetComponentInChildren<UnitHighlight>();
        _endTurnButton = GetComponent<EndTurnButton>();
    }

    /// <summary>
    /// This is called by UnitsManager right after creation
    /// </summary>
    /// <param name="unitInstance"></param>
    public void Initialize(BaseUnitInstance unitInstance)
    {
        UnitInstance = unitInstance;
        
        if (_unitHealthbar != null) _unitHealthbar.Initialize(unitInstance);
        if (_unitHealthText != null) _unitHealthText.Initialize(unitInstance);
        if (_unitTarget != null) _unitTarget.Initialize(unitInstance);
        if (_unitHighlight != null) _unitHighlight.Initialize(unitInstance);
        if (_endTurnButton != null && unitInstance is BasePlayerUnitInstance playerInstance)
        {
            _endTurnButton.Initialize(playerInstance);
        }
        
        // TODO: visuals
    }
}
