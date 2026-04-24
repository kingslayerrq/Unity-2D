using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurnButton : MonoBehaviour
{
    private BasePlayerUnitInstance _playerUnitInstance;
    private Button _endTurnButton;
    private TMP_Text _endTurnButtonText;
    
    [SerializeField] private Image _endTurnButtonBackground;
    [SerializeField] private Color disabledColor;
    
    private Color _buttonBackgroundOrigColor;

    private void Awake()
    {
        _endTurnButton = GetComponent<Button>();
        _endTurnButtonText = _endTurnButton.GetComponentInChildren<TMP_Text>();

        // Safely try to get the background image if not set in Inspector
        if (_endTurnButtonBackground == null) 
            _endTurnButtonBackground = GetComponent<Image>();

        if (_endTurnButtonBackground != null)
        {
            _buttonBackgroundOrigColor = _endTurnButtonBackground.color;
        }
    }

    // Called by UnitView when the player spawns
    public void Initialize(BasePlayerUnitInstance playerInstance)
    {
        // Clean up old listeners just in case this button gets recycled
        Cleanup();

        _playerUnitInstance = playerInstance;

        if (_playerUnitInstance != null && _playerUnitInstance.CombatManager != null)
        {
            _endTurnButton.onClick.AddListener(_playerUnitInstance.CombatManager.EndPlayerTurn);
            _playerUnitInstance.CombatManager.OnStateChange += HandlePlayerActionStateChange;
            _playerUnitInstance.CombatManager.OnPlayerNewTurnStart += HandlePlayerNewTurnStart;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (_playerUnitInstance != null && _playerUnitInstance.CombatManager != null)
        {
            _endTurnButton.onClick.RemoveListener(_playerUnitInstance.CombatManager.EndPlayerTurn);
            _playerUnitInstance.CombatManager.OnStateChange -= HandlePlayerActionStateChange;
            _playerUnitInstance.CombatManager.OnPlayerNewTurnStart -= HandlePlayerNewTurnStart;
        }
    }

    private void HandlePlayerNewTurnStart(int turnNumber)
    {
        if (_endTurnButtonText != null)
            _endTurnButtonText.text = turnNumber.ToString();
    }

    private void HandlePlayerActionStateChange(CombatManager.NodeState combatState)
    {
        if (combatState == CombatManager.NodeState.PlayerAction)
        {
            _endTurnButton.interactable = true;
            if (_endTurnButtonBackground != null) 
                _endTurnButtonBackground.color = _buttonBackgroundOrigColor;
        }
        else
        {
            _endTurnButton.interactable = false;
            if (_endTurnButtonBackground != null) 
                _endTurnButtonBackground.color = disabledColor;
        }
    }
    
    
}