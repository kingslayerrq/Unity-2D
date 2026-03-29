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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _cardPileManager.DiscardPile.OnCardPileCountChanged -= UpdateCardPileUI;
    }
}
