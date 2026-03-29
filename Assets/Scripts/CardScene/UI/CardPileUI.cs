using System;
using TMPro;
using UnityEngine;

public abstract class CardPileUI: MonoBehaviour
{
    protected CardPileManager _cardPileManager;
    protected TMP_Text _cardPileCardCount;

    protected void Awake()
    {
        _cardPileManager = GetComponentInParent<CardPileManager>();
        _cardPileCardCount = GetComponentInChildren<TMP_Text>();
    }

    protected void Start()
    {
        if (_cardPileManager == null)
        {
            Debug.LogError("CardPileManager not found");
            return;
        }

        _cardPileManager.OnCardPilesInitialized += InitializeCardPileUI;
    }

    protected abstract void InitializeCardPileUI();

    protected virtual void UpdateCardPileUI(int count)
    {
        if (_cardPileCardCount == null)
        {
            Debug.LogError("CardPileCardCount not found");
            return;
        }
        _cardPileCardCount.text = count.ToString();
    }

    protected virtual void OnDestroy()
    {
        _cardPileManager.OnCardPilesInitialized -= InitializeCardPileUI;
    }
}
