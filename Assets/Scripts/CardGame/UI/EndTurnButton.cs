using System;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class EndTurnButton : MonoBehaviour
{
    private BasePlayerUnitInstance _playerUnitInstance;
    private Button _endTurnButton;
    private TMP_Text _endTurnButtonText;
    
    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private float _offScreenYOffset = -150f;
    
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Vector2 _onScreenPos;

    private void Awake()
    {
        _endTurnButton = GetComponent<Button>();
        _endTurnButtonText = _endTurnButton.GetComponentInChildren<TMP_Text>();
        
        // Cache components needed for DOTween
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        
        // Store the initial starting position of the button
        _onScreenPos = _rectTransform.anchoredPosition;
        
    }

    // Called by UnitView when the player spawns
    public void Initialize(BasePlayerUnitInstance playerInstance)
    {
        // Clean up old listeners just in case this button gets recycled
        Cleanup();

        _playerUnitInstance = playerInstance;

        if (_endTurnButtonText != null)
            _endTurnButtonText.text = "";

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
        _rectTransform.DOKill();
        _canvasGroup.DOKill();
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
            _endTurnButtonText.text = "End Turn " + turnNumber.ToString();
    }

    private void HandlePlayerActionStateChange(CombatManager.NodeState combatState)
    {
        if (combatState == CombatManager.NodeState.PlayerAction)
        {
            _endTurnButton.interactable = true;
            AnimateButtonIn();
        }
        else
        {
            _endTurnButton.interactable = false;
            AnimateButtonOut();
        }
    }
    
    private void AnimateButtonIn()
    {
        // DOKill stops any current animations so they don't overlap if state changes quickly
        _rectTransform.DOKill();
        _canvasGroup.DOKill();

        // Slide up to original position and fade to 1
        _rectTransform.DOAnchorPosY(_onScreenPos.y, _animationDuration).SetEase(Ease.OutBack);
        _canvasGroup.DOFade(1f, _animationDuration);
    }

    private void AnimateButtonOut()
    {
        _rectTransform.DOKill();
        _canvasGroup.DOKill();

        // Slide down by the offset amount and fade to 0
        _rectTransform.DOAnchorPosY(_onScreenPos.y + _offScreenYOffset, _animationDuration).SetEase(Ease.InBack);
        _canvasGroup.DOFade(0f, _animationDuration);
    }
    
}