using System;
using UnityEngine;

public class DeckPileUI : CardPileUI
{
    private BasePlayerUnitInstance _playerUnit;
    
    protected override void Awake()
    {
        base.Awake();
        RunManager.Instance.OnPlayerUnitInstanceCreated += OnPlayerUnitCreated;

        // Panel was inactive when the player unit was created — bootstrap immediately
        if (RunManager.Instance.PlayerUnitInstance != null)
            OnPlayerUnitCreated(RunManager.Instance.PlayerUnitInstance);
    }

    private void OnPlayerUnitCreated(BasePlayerUnitInstance unit)
    {
        if (_playerUnit != null)
        {
            _playerUnit.OnPlayerDeckInitialized -= OnDeckReady;
            if (_playerUnit.DeckInstance != null)
                _playerUnit.DeckInstance.OnDeckChanged -= UpdateDeckCount;
        }

        _playerUnit = unit;
        _playerUnit.OnPlayerDeckInitialized += OnDeckReady;

        // Deck already initialized before this panel became active
        if (_playerUnit.DeckInstance != null)
            OnDeckReady();
    }

    private void OnDeckReady()
    {
        _playerUnit.DeckInstance.OnDeckChanged += UpdateDeckCount;
        UpdateDeckCount();
    }
    private void UpdateDeckCount()
    {
        UpdateCardPileUI(_playerUnit.DeckInstance.Count);
    }
    protected override void InitializeCardPileUI()
    {
        
    }
    
    protected override void DisplayCardsInPile()
    {
        base.DisplayCardsInPile();
        Debug.Log($"Rendering DeckPile, count: {RunManager.Instance.PlayerUnitInstance.DeckInstance.Count}");
        RenderCardViews(RunManager.Instance.PlayerUnitInstance.DeckInstance.CardInstances);
        Canvas.ForceUpdateCanvases();
        _cardPileScrollRect.normalizedPosition = new Vector2(0, 1);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (RunManager.Instance != null)
            RunManager.Instance.OnPlayerUnitInstanceCreated -= OnPlayerUnitCreated;
        if (_playerUnit != null)
        {
            _playerUnit.OnPlayerDeckInitialized -= OnDeckReady;
            if (_playerUnit.DeckInstance != null)
                _playerUnit.DeckInstance.OnDeckChanged -= UpdateDeckCount;
        }
    }
}
