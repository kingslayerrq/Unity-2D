using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrawPileUI : CardPileUI
{
    

    protected override void InitializeCardPileUI()
    {
        _cardPileManager.DrawPile.OnCardPileCountChanged += UpdateCardPileUI;
        _cardPileManager.DrawPile.Refresh();
    }

    protected override void DisplayCardsInPile()
    {
        if (_cardPileManager.DrawPile == null || _cardPileManager.DrawPile.Count == 0) return;
        
        base.DisplayCardsInPile();
        Debug.Log($"Rendering draw pile, count: {_cardPileManager.DrawPile.Count}");
        RenderCardViews(_cardPileManager.DrawPile.Cards);
        
        Canvas.ForceUpdateCanvases();
        _cardPileScrollRect.normalizedPosition = new Vector2(0, 1);

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_cardPileManager.DrawPile != null)
        {
            _cardPileManager.DrawPile.OnCardPileCountChanged -= UpdateCardPileUI;
        }
    }
}
