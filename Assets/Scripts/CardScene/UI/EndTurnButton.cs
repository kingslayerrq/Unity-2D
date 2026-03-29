using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurnButton : MonoBehaviour
{
    private UnitInstance _unitInstance;
    private Button _endTurnButton;

    private void Awake()
    {
        _unitInstance = GetComponentInParent<UnitInstance>();
        _endTurnButton = GetComponent<Button>();

        if (_endTurnButton != null)
        {
            _endTurnButton.onClick.AddListener(_unitInstance.CombatManager.EndPlayerTurn);
        }
    }

    private void Start()
    {
        if (_unitInstance != null)
        {
            _unitInstance.CombatManager.OnStateChange += HandlePlayerActionStateChange;
        }
    }

    private void OnDestroy()
    {
        if (_unitInstance != null)
        {
            _unitInstance.CombatManager.OnStateChange -= HandlePlayerActionStateChange;
        }
    }

    private void HandlePlayerActionStateChange(CombatManager.CombatState combatState)
    {
        if (combatState == CombatManager.CombatState.PlayerAction)
        {
            _endTurnButton.interactable = true;
        }
        else
        {
            _endTurnButton.interactable = false;
        }
    }
}
