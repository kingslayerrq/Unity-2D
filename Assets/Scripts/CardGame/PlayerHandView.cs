using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerHandView : MonoBehaviour
{
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private RectTransform discardPileIcon;
    [SerializeField] private Canvas rootCanvas;

    private readonly Dictionary<CardInstance, CardView> viewsByCard = new Dictionary<CardInstance, CardView>();
    private readonly List<CardView> activeCardViews = new List<CardView>();
    private readonly Stack<CardView> pooledViews = new Stack<CardView>();
    
    public IReadOnlyList<CardView> ActiveCardViews => activeCardViews;
    public RectTransform DiscardPileIcon => discardPileIcon;
    
    public Canvas RootCanvas => rootCanvas;
    
    private readonly HashSet<CardInstance> animatingCards = new HashSet<CardInstance>();

    public bool TryBeginAnimateOut(CardInstance card, out CardView view)
    {
        view = null;

        if (card == null)
            return false;

        if (!viewsByCard.TryGetValue(card, out view) || view == null)
            return false;

        animatingCards.Add(card);
        activeCardViews.Remove(view);

        return true;
    }

    public void CompleteAnimateOut(CardInstance card)
    {
        if (card == null)
            return;

        animatingCards.Remove(card);

        if (viewsByCard.TryGetValue(card, out CardView view) && view != null)
        {
            viewsByCard.Remove(card);
            ReturnToPool(view);
        }
    }

    public void CancelAnimateOut(CardInstance card)
    {
        if (card == null)
            return;

        animatingCards.Remove(card);
    }

    /// <summary>
    /// Synchronizes the hand UI with the supplied runtime hand data.
    /// Reuses existing <see cref="CardView"/> instances for cards that are still present,
    /// returns obsolete views to the pool, and creates or reuses views for newly added cards.
    /// Also updates sibling order so the visual hand matches the order of the input list.
    /// </summary>
    /// <param name="hand">
    /// The ordered collection of runtime card instances that should currently appear in the player's hand.
    /// If <c>null</c>, all active hand views are released back to the pool.
    /// </param>
    public void RefreshHand(IReadOnlyList<CardInstance> hand)
    {
        if (hand == null)
        {
            Debug.LogWarning("ShowHand: hand is null");
            ReleaseAllToPool();
            animatingCards.Clear();
            return;
        }

        HashSet<CardInstance> cardsInHand = new HashSet<CardInstance>(hand);

        // Return removed cards to pool, skip the ones animating
        List<CardInstance> toRemove = new List<CardInstance>();
        foreach (KeyValuePair<CardInstance, CardView> pair in viewsByCard)
        {
            if (!cardsInHand.Contains(pair.Key) && !animatingCards.Contains(pair.Key))
                toRemove.Add(pair.Key);
        }

        foreach (CardInstance card in toRemove)
        {
            CardView view = viewsByCard[card];
            viewsByCard.Remove(card);
            ReturnToPool(view);
        }

        activeCardViews.Clear();

        // Ensure every card in hand has a view, in the correct order
        for (int i = 0; i < hand.Count; i++)
        {
            CardInstance card = hand[i];

            if (!viewsByCard.TryGetValue(card, out CardView view))
            {
                view = GetOrCreateView();
                view.Bind(card);
                viewsByCard.Add(card, view);
            }
            else
            {
                view.gameObject.SetActive(true);
                view.Refresh();
            }

            view.transform.SetSiblingIndex(i);
            activeCardViews.Add(view);
        }
    }

    
    /// <summary>
    /// Refreshes all currently active hand views without changing hand membership or order.
    /// This should be used when the same cards remain in hand but one or more displayed values
    /// have changed, such as cost, name, description, or artwork.
    /// </summary>
    public void UpdateHand()
    {
        foreach (CardView view in activeCardViews)
        {
            if (view != null && view.gameObject.activeSelf)
                view.Refresh();
        }
    }
    
    /// <summary>
    /// Releases all active <see cref="CardView"/> instances back to the internal reuse pool
    /// and clears the active-view and card-to-view mappings.
    /// This method does not destroy pooled objects.
    /// </summary>
    public void ReleaseAllToPool()
    {
        foreach (CardView view in activeCardViews)
        {
            ReturnToPool(view);
        }

        activeCardViews.Clear();
        viewsByCard.Clear();
    }

    /// <summary>
    /// Retrieves a reusable <see cref="CardView"/> from the pool if one is available;
    /// otherwise instantiates a new view from <see cref="cardViewPrefab"/>.
    /// The returned view is activated, parented under the card container,
    /// and prepared for binding.
    /// </summary>
    /// <returns>
    /// An active <see cref="CardView"/> ready to display a card in the player's hand.
    /// </returns>
    private CardView GetOrCreateView()
    {
        CardView view;

        view = pooledViews.Count > 0 ? pooledViews.Pop() : Instantiate(cardViewPrefab, cardContainer);

        view.transform.SetParent(cardContainer, false);
        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    /// Resets the specified <see cref="CardView"/> and stores it in the reuse pool
    /// instead of destroying it.
    /// This clears temporary interaction and presentation state so the view can be safely reused.
    /// </summary>
    /// <param name="view">
    /// The <see cref="CardView"/> instance to return to the internal pool.
    /// If <c>null</c>, no action is taken.
    /// </param>
    private void ReturnToPool(CardView view)
    {
        if (view == null) return;

        view.ResetForPool();
        view.gameObject.SetActive(false);
        view.transform.SetParent(cardContainer, false);
        pooledViews.Push(view);
    }

    public bool TryGetView(CardInstance card, out CardView view)
    {
        return viewsByCard.TryGetValue(card, out view);
    }
    
    // play card animation
    public IEnumerator PlayToDiscardAnimation(CardInstance card)
    {
        if (!TryGetView(card, out CardView view) || view == null)
            yield break;

        if (view.CardAnimation == null)
            yield break;

        Sequence seq = view.CardAnimation.PlayToDiscardAnimationSelf(rootCanvas, discardPileIcon);
        
        
        if (seq != null)
        {
            yield return seq.WaitForCompletion();
        }
    }
    
    // batch discard animation (for turn end, etc)
    public IEnumerator DiscardCardsAnimation(IReadOnlyList<CardInstance> cards)
    {
        if (cards == null || cards.Count == 0)
            yield break;

        List<Sequence> sequences = new List<Sequence>();
        int batchIndex = 0;

        foreach (CardInstance card in cards)
        {
            if (!TryGetView(card, out CardView view))
                continue;

            if (view == null || view.CardAnimation == null)
                continue;

            Sequence seq = view.CardAnimation.DiscardAnimationSelf(rootCanvas, discardPileIcon, batchIndex);

            if (seq != null)
            {
                sequences.Add(seq);
                batchIndex++;
            }
        }

        if (sequences.Count == 0)
            yield break;

        yield return new WaitUntil(() =>
        {
            for (int i = 0; i < sequences.Count; i++)
            {
                Sequence seq = sequences[i];

                if (seq != null && seq.IsActive() && !seq.IsComplete())
                    return false;
            }

            return true;
        });
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}