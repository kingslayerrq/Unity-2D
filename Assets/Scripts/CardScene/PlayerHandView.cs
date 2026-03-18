using UnityEngine;

public class PlayerHandView : MonoBehaviour
{
    [SerializeField] private Transform cardParent;
    [SerializeField] private CardView cardViewPrefab;

    public void Refresh(PlayerHand hand)
    {
        // destroy old views
        // spawn views for each CardInstance
        // bind each view to that instance
        
        if (hand == null) return;
        var cards = hand.Cards;
        
    }
}
