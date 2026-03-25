using System.Collections.Generic;
using UnityEngine;

public class PlayerHandView : MonoBehaviour
{
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardViewPrefab;

    private readonly Dictionary<CardInstance, CardView> viewsByCard = new Dictionary<CardInstance, CardView>();
    private readonly List<CardView> activeCardViews = new List<CardView>();
    private readonly Stack<CardView> pooledViews = new Stack<CardView>();

    public IReadOnlyList<CardView> ActiveCardViews => activeCardViews;
    
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
    public void ShowHand(IReadOnlyList<CardInstance> hand)
    {
        if (hand == null)
        {
            Debug.LogWarning("ShowHand: hand is null");
            ReleaseAllToPool();
            return;
        }

        HashSet<CardInstance> cardsInHand = new HashSet<CardInstance>(hand);

        // Return removed cards to pool
        List<CardInstance> toRemove = new List<CardInstance>();
        foreach (KeyValuePair<CardInstance, CardView> pair in viewsByCard)
        {
            if (!cardsInHand.Contains(pair.Key))
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
    public void RefreshHand()
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

        view.ResetForReuse();
        view.gameObject.SetActive(false);
        view.transform.SetParent(cardContainer, false);
        pooledViews.Push(view);
    }
}