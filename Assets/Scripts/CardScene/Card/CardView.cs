using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardSprite;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;
    [SerializeField] private TMP_Text cardPrice;
    [SerializeField] private TMP_Text cardCost;
    [SerializeField] private TMP_Text cardLevel;

    private CardInstance _cardInstance;

    public void Bind(CardInstance instance)
    {
        _cardInstance = instance;
        Refresh();
    }

    public void Refresh()
    {
        if (_cardInstance == null) return;

        cardName.text = _cardInstance.Data.CardName;
        cardDescription.text = _cardInstance.Data.CardDescription;
        cardPrice.text = _cardInstance.Data.CardPrice.ToString();
        cardCost.text = _cardInstance.Data.CardCost.ToString();
        cardLevel.text = _cardInstance.Data.CardLevel.ToString();
        cardSprite.sprite = _cardInstance.Data.CardSprite;
        
    }
}
