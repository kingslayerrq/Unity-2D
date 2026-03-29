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
    
    
    
}
