using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurnButton : MonoBehaviour
{
    private UnitInstance _unitInstance;
    private Button _endTurnButton;
    private TMP_Text _endTurnButtonText;
    [SerializeField] private Image _endTurnButtonBackground;
    [SerializeField] private Color disabledColor;
    private Color _buttonBackgroundOrigColor;
    private void Awake()
    {
        _unitInstance = GetComponentInParent<UnitInstance>();
        _endTurnButton = GetComponent<Button>();
        _endTurnButtonBackground = GetComponentInChildren<Image>();
        _endTurnButtonText = _endTurnButton.GetComponentInChildren<TMP_Text>();

        if (_endTurnButton != null)
        {
            _endTurnButton.onClick.AddListener(_unitInstance.CombatManager.EndPlayerTurn);
        }

        if (_endTurnButtonBackground != null)
        {
            _buttonBackgroundOrigColor = _endTurnButtonBackground.color;
        }
    }

    private void Start()
    {
        if (_unitInstance != null)
        {
            _unitInstance.CombatManager.OnStateChange += HandlePlayerActionStateChange;
            _unitInstance.CombatManager.OnPlayerNewTurnStart += HandlePlayerNewTurnStart;
        }
    }

    private void OnDestroy()
    {
        if (_unitInstance != null)
        {
            _unitInstance.CombatManager.OnStateChange -= HandlePlayerActionStateChange;
            _unitInstance.CombatManager.OnPlayerNewTurnStart -= HandlePlayerNewTurnStart;
        }
    }

    private void HandlePlayerNewTurnStart(int turnNumber)
    {
        _endTurnButtonText.text = turnNumber.ToString();
    }

    private void HandlePlayerActionStateChange(CombatManager.CombatState combatState)
    {
        if (combatState == CombatManager.CombatState.PlayerAction)
        {
            _endTurnButton.interactable = true;
            _endTurnButtonBackground.color = _buttonBackgroundOrigColor;
        }
        else
        {
            _endTurnButton.interactable = false;
            _endTurnButtonBackground.color = disabledColor;
        }
    }
}
