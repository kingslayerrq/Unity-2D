using System;
using TMPro;
using UnityEngine;

public class DiscardPileUI : CardPileUI
{
    protected override void InitializeCardPileUI()
    {
        _cardPileManager.DiscardPile.OnCardPileCountChanged += UpdateCardPileUI;
        _cardPileManager.DiscardPile.Refresh();
    }

    protected override void DisplayCardsInPile()
    {
        if (_cardPileManager.DiscardPile == null ||  _cardPileManager.DiscardPile.Count == 0) return;
        
        base.DisplayCardsInPile();
        
        Debug.Log($"Rendering discard pile, count: {_cardPileManager.DiscardPile.Count}");
        RenderCardViews(_cardPileManager.DiscardPile.Cards);
        
        Canvas.ForceUpdateCanvases();
        _cardPileScrollRect.normalizedPosition = new Vector2(0, 1);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_cardPileManager.DiscardPile != null)
        {
            _cardPileManager.DiscardPile.OnCardPileCountChanged -= UpdateCardPileUI;
        }
       
    }
}
