using UnityEngine;

/// <summary>
/// Binds the draw and discard pile counters to the piles of the current run.
/// The piles themselves belong to the run.
/// </summary>
public class CardPilePresenter : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private DrawPileUI drawPileUI;
    [SerializeField] private DiscardPileUI discardPileUI;

    private void Awake()
    {
        // The same piles are reused for every battle of the run; each battle
        // deals the deck into them again and the counters follow.
        CardPileManager cardPiles = RunManager.Instance.Run.CardPiles;

        if (drawPileUI != null) drawPileUI.Initialize(cardPiles);
        if (discardPileUI != null) discardPileUI.Initialize(cardPiles);
    }
}
