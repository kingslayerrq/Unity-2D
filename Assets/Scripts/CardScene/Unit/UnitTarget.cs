using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitTarget : MonoBehaviour, ICardDropReceiver
{
    private BaseUnitInstance _unitInstance;
    public int Priority { get; private set; } = CONSTANTS.UNIT_TARGET_PRIORITY;
    private void Awake()
    {
        _unitInstance = GetComponent<BaseUnitInstance>();
    }

    public List<BaseUnitInstance> GetReceiverUnitInstance(BaseUnitInstance source, CardInstance cardInstance)
    {
        var instances = new List<BaseUnitInstance> { _unitInstance };
        return instances;
    }

    public bool CanReceiveCard(BaseUnitInstance source, CardInstance cardInstance)
    {
        return ValidateCardTargets(source, cardInstance).Count != 0;
    }

    public List<ICardTarget> ValidateCardTargets(BaseUnitInstance source, CardInstance cardInstance)
    {
        List<ICardTarget> targets = new List<ICardTarget>();
        switch (cardInstance.CurrentTargetType)
        {
            case TargetType.AllAlly:
                if (_unitInstance.UnitType == UnitType.Player) targets.Add(_unitInstance);
                break;
            case TargetType.Ally:
                if (_unitInstance.UnitType == UnitType.Player) targets.Add(_unitInstance);
                break;
            case TargetType.AllEnemy:
                if (_unitInstance.UnitType == UnitType.Enemy) targets.Add(_unitInstance);
                break;
            case TargetType.Enemy:
                if (_unitInstance.UnitType == UnitType.Enemy) targets.Add(_unitInstance);
                break;
            case TargetType.None:
                break;
            case TargetType.Any:
                targets.Add(_unitInstance);
                break;
            case TargetType.Self:
                if (_unitInstance == source) targets.Add(_unitInstance);
                break;
            default:
                break;
        }
        return targets;
    }

    public void OnHoverEnter(BaseUnitInstance source, CardInstance cardInstance)
    {
        // highlight validate targets
        if (CanReceiveCard(source, cardInstance)) _unitInstance.UnitUIManager.UnitHighlight.HighlightUnit();
    }

    public void OnHoverExit(BaseUnitInstance source, CardInstance cardInstance)
    {
        if (_unitInstance.UnitUIManager.UnitHighlight.IsHighlighted)
        {
            _unitInstance.UnitUIManager.UnitHighlight.HideHighlight();
        }
    }
}
