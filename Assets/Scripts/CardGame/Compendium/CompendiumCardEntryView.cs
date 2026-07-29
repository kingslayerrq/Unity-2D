using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CompendiumCardEntryView : MonoBehaviour
{
    [Header("Card Information")]
    [SerializeField] private Image cardArtwork;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("State Objects")]
    [Tooltip("Shown when the card has never been discovered.")]
    [SerializeField] private GameObject hiddenStateRoot;

    [Tooltip("Shown when the card is discovered but still locked.")]
    [SerializeField] private GameObject lockedStateRoot;

    [Tooltip("Shown when the card is fully unlocked.")]
    [SerializeField] private GameObject unlockedStateRoot;

    [Header("Hidden State")]
    [SerializeField] private Sprite hiddenCardSprite;
    [SerializeField] private TMP_Text hiddenText;

    [Header("Locked State")]
    [SerializeField] private TMP_Text unlockRequirementText;

    [Header("Statistics")]
    [SerializeField] private TMP_Text obtainedCountText;
    [SerializeField] private TMP_Text playedCountText;
    [SerializeField] private TMP_Text runWinsText;

    [Header("Optional Selection")]
    [SerializeField] private Button entryButton;

    private CardData currentCard;
    private CardProgressRecord currentProgress;
    private CardUnlockDefinition currentUnlockDefinition;

    public CardData CurrentCard => currentCard;

    public void Bind(
        CardData card,
        CardProgressRecord progress,
        CardUnlockDefinition unlockDefinition)
    {
        if (card == null)
        {
            Debug.LogError(
                "Cannot bind a null CardData.",
                this);

            gameObject.SetActive(false);
            return;
        }

        if (progress == null)
        {
            Debug.LogError(
                $"Cannot bind '{card.CardName}' because its " +
                "progress record is null.",
                this);

            gameObject.SetActive(false);
            return;
        }

        currentCard = card;
        currentProgress = progress;
        currentUnlockDefinition = unlockDefinition;

        gameObject.SetActive(true);

        if (!progress.IsDiscovered)
        {
            ShowHidden();
            return;
        }

        if (!progress.IsUnlocked)
        {
            ShowLocked();
            return;
        }

        ShowUnlocked();
    }

    private void ShowHidden()
    {
        SetStateRoots(
            showHidden: true,
            showLocked: false,
            showUnlocked: false);

        cardArtwork.sprite = hiddenCardSprite;

        cardNameText.text = "???";
        costText.text = "?";
        rarityText.text = "???";
        typeText.text = "???";
        descriptionText.text =
            "This card has not been discovered.";

        if (hiddenText != null)
            hiddenText.text = "Undiscovered";

        SetButtonInteractable(false);
    }

    private void ShowLocked()
    {
        SetStateRoots(
            showHidden: false,
            showLocked: true,
            showUnlocked: false);

        SetCardInformation(currentCard);

        if (unlockRequirementText != null)
        {
            unlockRequirementText.text =
                GetUnlockRequirementText();
        }

        SetButtonInteractable(false);
    }

    private void ShowUnlocked()
    {
        SetStateRoots(
            showHidden: false,
            showLocked: false,
            showUnlocked: true);

        SetCardInformation(currentCard);
        SetStatistics(currentProgress);

        SetButtonInteractable(true);
    }

    private void SetCardInformation(CardData card)
    {
        cardArtwork.sprite = card.CardSprite;
        cardNameText.text = card.CardName;
        costText.text = card.CardCost.ToString();
        rarityText.text = card.CardRarity.ToString();
        typeText.text = card.CardType.ToString();
        descriptionText.text = card.CardDescription;
    }

    private void SetStatistics(CardProgressRecord progress)
    {
        if (obtainedCountText != null)
        {
            obtainedCountText.text =
                $"Obtained: {progress.TimesObtained}";
        }

        if (playedCountText != null)
        {
            playedCountText.text =
                $"Played: {progress.TimesPlayed}";
        }

        if (runWinsText != null)
        {
            runWinsText.text =
                $"Run Wins: {progress.RunsWonWithCard}";
        }
    }

    private string GetUnlockRequirementText()
    {
        if (currentUnlockDefinition == null)
        {
            return "Unlock requirement unavailable.";
        }

        if (currentUnlockDefinition.Condition == null)
        {
            return "Unlock condition is not configured.";
        }

        return currentUnlockDefinition.UnlockDescription;
    }

    private void SetStateRoots(
        bool showHidden,
        bool showLocked,
        bool showUnlocked)
    {
        if (hiddenStateRoot != null)
            hiddenStateRoot.SetActive(showHidden);

        if (lockedStateRoot != null)
            lockedStateRoot.SetActive(showLocked);

        if (unlockedStateRoot != null)
            unlockedStateRoot.SetActive(showUnlocked);
    }

    private void SetButtonInteractable(bool interactable)
    {
        if (entryButton != null)
            entryButton.interactable = interactable;
    }
}