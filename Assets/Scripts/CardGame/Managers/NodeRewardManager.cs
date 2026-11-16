using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class NodeRewardManager : MonoBehaviour
{
    [Header("Services")]
    [SerializeField]
    private GlobalProgressionManager progression;
    
    [Header("Card Rewards Data")]
    [SerializeField] private CardRewardData ironcladReward;
    [SerializeField] private CardRewardData silentReward;
    [SerializeField] private CardRewardData necroReward;
    [SerializeField] private CardRewardData regentReward;
    [SerializeField] private CardRewardData neutralReward;
    
    [Header("UI")]
    [SerializeField] private CanvasGroup _resultCanvasGroup;
    [SerializeField] private CanvasGroup _cardsSelectionCanvasGroup;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _losePanel;
    [SerializeField] private Button _cardRewardButton;
    [SerializeField] private Button _skipRewardButton;
    [FormerlySerializedAs("_continueButton")] [SerializeField] private Button _continueToMapButton;
    [SerializeField] private Button _retryButton;
    
    private UnityAction EnterMapAction => ContinueToMap;
    
    
    [Header("Card Reward UI")]
    [SerializeField] private CardView cardRewardPrefab; 
    [SerializeField] private Transform cardRewardContainer;
    private List<CardView> _spawnedRewardCards = new List<CardView>();


    private void Awake()
    {
        if (progression == null)
            progression = GlobalProgressionManager.Instance;

        if (progression == null)
        {
            Debug.LogError(
                "NodeRewardManager requires a " +
                "GlobalProgressionManager.",
                this);

            enabled = false;
            return;
        }
        
        if (_winPanel == null || _losePanel == null)
        {
            Debug.LogError($"winpanel: {_winPanel != null},  losepanel: {_losePanel != null}");
            return;
        }
        
        if (_cardRewardButton != null)
        {
            _cardRewardButton.onClick.AddListener(GetCardRewardButton);
        }

        if (_skipRewardButton != null)
        {
            _skipRewardButton.onClick.AddListener(HideSelectionCanvasGroup);
        }

        if (_continueToMapButton != null)
        {
            _continueToMapButton.onClick.AddListener(EnterMapAction);
        }
        
        if (_retryButton == null) _retryButton = _losePanel.GetComponentInChildren<Button>();
        if (_retryButton != null)
        {
            _retryButton.onClick.AddListener(RetryRun);
        }
        
        // set inactive
        _winPanel.SetActive(false);
        _losePanel.SetActive(false);
        
        
        if (_resultCanvasGroup != null)
        {
            // hide on awake
            _resultCanvasGroup.alpha = 0;
            _resultCanvasGroup.blocksRaycasts = false;
            _resultCanvasGroup.interactable = false;
        }

        if (_cardsSelectionCanvasGroup != null)
        {
            _cardsSelectionCanvasGroup.alpha = 0;
            _cardsSelectionCanvasGroup.blocksRaycasts = false;
            _cardsSelectionCanvasGroup.interactable = false;
        }
        
        RunManager.Instance.RegisterCombatRewardManager(this);
    }
    
    private void OnEnable()
    {
        if (progression == null)
            progression = GlobalProgressionManager.Instance;

        if (progression != null)
        {
            progression.CardUnlocked +=
                HandleCardUnlocked;
        }
    }

    private void OnDisable()
    {
        if (progression != null)
        {
            progression.CardUnlocked -=
                HandleCardUnlocked;
        }
    }
    
    /// <summary>
    /// Rebuild the reward pools when a card becomes unlocked. Plays and
    /// discoveries do not change the pools, so they are not listened to.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="progress"></param>
    private void HandleCardUnlocked(
        CardData card,
        CardProgressRecord progress)
    {
        if (!progress.IsUnlocked)
            return;

        Initialize();
    }


    /// <summary>
    /// Exit current node to map
    /// </summary>
    private void ContinueToMap()
    {
        _winPanel.SetActive(false);
        _losePanel.SetActive(false);
        RunManager.Instance.Run.CompleteNode();
    }

    public void Initialize()
    {
        if (progression == null)
        {
            Debug.LogError(
                "Global progression has not been assigned.",
                this);

            return;
        }
        
        // The run rolls the rewards. This screen only tells it which cards
        // are unlocked and may be offered.
        Dictionary<CardClass, List<CardData>> pools = new Dictionary<CardClass, List<CardData>>
        {
            { CardClass.Ironclad, UnlockedCards(ironcladReward) },
            { CardClass.Silent, UnlockedCards(silentReward) },
            { CardClass.Necro, UnlockedCards(necroReward) },
            { CardClass.Regent, UnlockedCards(regentReward) },
            { CardClass.None, UnlockedCards(neutralReward) },
        };

        RunManager.Instance.Run.Rewards.SetPools(pools);
    }

    private List<CardData> UnlockedCards(CardRewardData rewardData)
    {
        return rewardData.availableCards.Where(card => progression.IsUnlocked(card)).ToList();
    }

    public void DisplayResultCanvas(bool isWin)
    {
        if (_resultCanvasGroup == null || _winPanel == null || _losePanel == null) return;
        _winPanel.SetActive(isWin);
        _losePanel.SetActive(!isWin);

        if (isWin)
        {
            if (_cardRewardButton != null) _cardRewardButton.gameObject.SetActive(true);
        }

        _resultCanvasGroup.DOFade(1f, .1f);
        _resultCanvasGroup.interactable = true;
        _resultCanvasGroup.blocksRaycasts = true;
    }

    public void HideResultCanvas()
    {
        if (_resultCanvasGroup == null || _winPanel == null || _losePanel == null) return;
        
        _resultCanvasGroup.interactable = false;
        _resultCanvasGroup.blocksRaycasts = false;
        _resultCanvasGroup.DOFade(0f, .1f);
        
        _winPanel.SetActive(false);
        _losePanel.SetActive(false);
        
    }

    /// <summary>
    /// Leaves the lost run behind and starts a fresh one from character select.
    /// </summary>
    private void RetryRun()
    {
        HideResultCanvas();
        RunManager.Instance.RestartRun();
    }

    public void GetCardRewardButton()
    {
        // Rolled by the run the first time they are asked for, and the same
        // cards every time after that until the node is left.
        IReadOnlyList<CardData> offers = RunManager.Instance.Run.GetRewardOffers();

        ShowSelectionCanvasGroup();
        DisplayCardRewards(offers);
    }

    private void ShowSelectionCanvasGroup()
    {
        if (_cardsSelectionCanvasGroup != null)
        {
            _cardsSelectionCanvasGroup.DOFade(1f, 0);
            _cardsSelectionCanvasGroup.interactable = true;
            _cardsSelectionCanvasGroup.blocksRaycasts = true;
        }
    }

    private void HideSelectionCanvasGroup()
    {
        if (_cardsSelectionCanvasGroup != null)
        {
            _cardsSelectionCanvasGroup.interactable = false;
            _cardsSelectionCanvasGroup.blocksRaycasts = false;
            _cardsSelectionCanvasGroup.DOFade(0f, 0);
        }
        
        foreach (var spawned in _spawnedRewardCards)
            CardPileUI.ReturnViewToRewardPool(spawned);
        _spawnedRewardCards.Clear();
    }
    
    private void DisplayCardRewards(IReadOnlyList<CardData> cardDatas)
    {
        foreach (var spawned in _spawnedRewardCards)
        {
            CardPileUI.ReturnViewToRewardPool(spawned);
        }

        _spawnedRewardCards.Clear();

        for (int i = 0; i < cardDatas.Count; i++)
        {
            CardData cd = cardDatas[i];

            CardView cv = CardPileUI.GetOrCreateRewardView(cardRewardPrefab, cardRewardContainer);
            Canvas parentCanvas = _cardsSelectionCanvasGroup.GetComponentInParent<Canvas>();
            cv.SetSortingOrder(parentCanvas.sortingOrder+1);
            CardInstance cardInstance = new CardInstance(cd);
            cv.Bind(cardInstance);
            CardInteraction cardInteraction = cv.GetComponent<CardInteraction>();
            cardInteraction.OnCardSelectedToAdd = SelectCardReward;
            
            // cv.GetComponent<Button>().onClick.AddListener(() => SelectCardReward(cv.CardInstance));
            _spawnedRewardCards.Add(cv);
        }
    }

    private void SelectCardReward(CardInstance cardInstance)
    {
        // The card on screen is only a preview. The run adds its own copy
        // to the deck and reports it.
        if (!RunManager.Instance.Run.TakeReward(cardInstance.Data))
        {
            Debug.LogWarning("That card is no longer on offer.", this);
            return;
        }

        foreach (var spawned in _spawnedRewardCards)
            CardPileUI.ReturnViewToRewardPool(spawned);
        _spawnedRewardCards.Clear();
        
        HideSelectionCanvasGroup();
        _cardRewardButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_continueToMapButton != null)
        {
            _continueToMapButton.onClick.RemoveListener(EnterMapAction);
        }

        if (_retryButton != null)
        {
            _retryButton.onClick.RemoveListener(RetryRun);
        }
        
        if (_cardRewardButton != null)
        {
            _cardRewardButton.onClick.RemoveListener(GetCardRewardButton);
        }

        if (_skipRewardButton != null)
        {
            _skipRewardButton.onClick.RemoveListener(HideSelectionCanvasGroup);
        }
        
        transform.DOKill();
    }
}
