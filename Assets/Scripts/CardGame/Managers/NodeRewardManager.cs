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

    private List<CardData> _cachedRewardCards = null;
    private List<CardData> _cachedOverridePool = null;

    private int _rareChanceOffset = -5;
    
    private Dictionary<CardClass, List<CardData>> _cardRewardDict = new Dictionary<CardClass, List<CardData>>();

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
        RunManager.Instance.ExitNode(RunManager.Instance.CurrentNode);
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
        
        _cardRewardDict.Clear();
        _cardRewardDict.Add(CardClass.Ironclad, ironcladReward.availableCards.Where(card => progression.IsUnlocked(card)).ToList());
        _cardRewardDict.Add(CardClass.Silent, silentReward.availableCards.Where(card => progression.IsUnlocked(card)).ToList());
        _cardRewardDict.Add(CardClass.Necro, necroReward.availableCards.Where(card => progression.IsUnlocked(card)).ToList());
        _cardRewardDict.Add(CardClass.Regent, regentReward.availableCards.Where(card => progression.IsUnlocked(card)).ToList());
        _cardRewardDict.Add(CardClass.None, neutralReward.availableCards.Where(card => progression.IsUnlocked(card)).ToList());
        _cachedOverridePool = null;
    }

    public void DisplayResultCanvas(bool isWin)
    {
        if (_resultCanvasGroup == null || _winPanel == null || _losePanel == null) return;
        _winPanel.SetActive(isWin);
        _losePanel.SetActive(!isWin);

        if (isWin)
        {
            _cachedRewardCards = null;
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
        if (_cachedRewardCards == null)
        {
            _cachedRewardCards = GetCardReward(RunManager.Instance.PlayerUnitInstance.PlayerUnitData.PlayerCardClass,
                RunManager.Instance.CardRewardAmountAfterCombat,
                RunManager.Instance.CurrentNode.ResolvedNode.Type == NodeType.CombatElite,
                RunManager.Instance.AllowCardClassOverride);
        }
        ShowSelectionCanvasGroup();
        DisplayCardRewards(_cachedRewardCards);
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
    
    private void DisplayCardRewards(List<CardData> cardDatas)
    {
        foreach (var spawned in _spawnedRewardCards)
        {
            CardPileUI.ReturnViewToRewardPool(spawned);
        }

        _spawnedRewardCards.Clear();

        for (int i = 0; i < cardDatas.Count; i++)
        {
            CardData cd = cardDatas[i];
            
            progression.DiscoverCard(cd);
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

        progression.SaveCheckpoint();
    }

    private void SelectCardReward(CardInstance cardInstance)
    {
        cardInstance.SetOwner(RunManager.Instance.PlayerUnitInstance);
        RunManager.Instance.PlayerUnitInstance.DeckInstance.AddCardToDeck(cardInstance);
        
        progression.RecordCardObtained(cardInstance);
        progression.SaveCheckpoint();

        foreach (var spawned in _spawnedRewardCards)
            CardPileUI.ReturnViewToRewardPool(spawned);
        _spawnedRewardCards.Clear();
        
        HideSelectionCanvasGroup();
        _cardRewardButton.gameObject.SetActive(false);
        _cachedRewardCards = null;
    }

    public List<CardData> GetCardReward(CardClass cardClass, int amount, bool isElite, 
        bool allowClassOverride = false)
    {
        List<CardData> rewardPool;

        if (allowClassOverride)
        {
            if (_cachedOverridePool == null)
            {
                _cachedOverridePool = new List<CardData>();
                foreach (var pool in _cardRewardDict.Values)
                {
                    _cachedOverridePool.AddRange(pool);
                }
            }
            rewardPool = _cachedOverridePool;
        }
        else
        {
            if (_cardRewardDict.TryGetValue(cardClass, out List<CardData> pool))
            {
                rewardPool = pool;
            }
            else
            {
                Debug.LogError($"Reward pool for class {cardClass} not found");
                return new List<CardData>();
            }
        }
        return GenerateCardRewardsFromPool(rewardPool, amount, isElite);
    }

   

    private List<CardData> GenerateCardRewardsFromPool(List<CardData> pool, int amount, bool isElite)
    {
        List<CardData> selectedCards = new List<CardData>();
        // Copy the pool so we can remove cards and prevent duplicates
        List<CardData> availablePool = new List<CardData>(pool);
        
        // Rewards come from the run's seeded loot stream so a seed reproduces them.
        System.Random rng = RunManager.Instance.LootRng;

        int baseRareChance = isElite ? 10 : 3;
        int baseUncommonChance = isElite ? 40 : 37;

        for (int i = 0; i < amount; i++)
        {
            int actualRareChance = baseRareChance + _rareChanceOffset;
            actualRareChance = Mathf.Clamp(actualRareChance, 0, 100);

            int roll = rng.Next(0, 100);
            CardRarity rarity;

            if (roll < actualRareChance)
            {
                rarity = CardRarity.Rare;
                // reset pity
                _rareChanceOffset = -5;
            }
            else if (roll < actualRareChance + baseUncommonChance)
            {
                rarity = CardRarity.Uncommon;
            }
            else
            {
                rarity = CardRarity.Common;
                _rareChanceOffset += 1;
            }
            
            List<CardData> cardsOfRarity = availablePool.Where(cd => cd.CardRarity == rarity).ToList();

            if (cardsOfRarity.Count == 0)
            {
                Debug.LogWarning($"Ran out of {rarity} cards! Falling back to general pool.");
                cardsOfRarity = availablePool;
            }
            
            int randomInd = rng.Next(cardsOfRarity.Count);
            CardData chosenCard = cardsOfRarity[randomInd];
            
            selectedCards.Add(chosenCard);
            
            availablePool.Remove(chosenCard);
        }
        return selectedCards;
    }

    private CardData GetWeightedRandomCard(List<CardData> pool)
    {
        int totalWeight = 0;
        foreach (var cd in pool)
        {
            totalWeight += cd.SpawnWeight;
        }
        
        int randValue = RunManager.Instance.LootRng.Next(Mathf.Max(totalWeight, 0));
        int currentWeight = 0;
        foreach (var cd in pool)
        {
            currentWeight += cd.SpawnWeight;
            if (randValue < currentWeight)
            {
                return cd;
            }
        }
        Debug.LogWarning("Weighted random fallback triggered. Check your card weights.");
        return pool[0];
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
