using TMPro;
using UnityEngine;

public class DrawPileUI : CardPileUI
{
    

    protected override void InitializeCardPileUI()
    {
        _cardPileManager.DrawPile.OnCardPileCountChanged += UpdateCardPileUI;
        _cardPileManager.DrawPile.Refresh();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _cardPileManager.DrawPile.OnCardPileCountChanged -= UpdateCardPileUI;
    }
}
