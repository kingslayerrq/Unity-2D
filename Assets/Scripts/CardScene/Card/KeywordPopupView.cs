using TMPro;
using UnityEngine;

public class KeywordPopupView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    
    public void Setup(KeywordData keywordData)
    {
        titleText.text = keywordData.keywordName;
        descriptionText.text = keywordData.definition;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
