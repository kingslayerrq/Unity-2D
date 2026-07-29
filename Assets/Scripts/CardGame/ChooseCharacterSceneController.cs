using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ChooseCharacterSceneController : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] private CharacterCatalog characterCatalog;
    
    [Header("Character Slots")]
    [SerializeField] private CharacterSlotVisual characterSlotPrefab;
    [SerializeField] private RectTransform characterSlotContainer;

    [Header("Character Details")]
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text characterStartingHealth;
    [SerializeField] private TMP_Text characterStartingGold;
    [SerializeField] private TMP_Text characterDescription;
    [SerializeField] private TMP_Text characterStartingRelicName;
    [SerializeField] private TMP_Text characterStartingRelicDescription;
    [SerializeField] private Image characterStartingRelicSprite;

    private readonly List<PlayerUnitData> displayedCharacters = new();
    private readonly List<CharacterSlotVisual> spawnedSlots = new();
    private GlobalProgressionManager progression;
    private bool isInitialized;
    private int currentIndex = 0;

    private void OnEnable()
    {
        if (isInitialized)
            RefreshAvailableCharacters();
    }

    private void Start()
    {
        isInitialized = true;
        RefreshAvailableCharacters();
    }
    

    private void UpdateCharacterSelectScreen(int index)
    {
        if (displayedCharacters.Count == 0)
            return;

        PlayerUnitData currentCharacter =
            displayedCharacters[index];
        
        if (currentCharacter == null) return;
        
        currentIndex = index;

        characterName.text = currentCharacter.UnitName;
        characterDescription.text = currentCharacter.UnitDescription;
        characterStartingHealth.text =
            $"{currentCharacter.PlayerStartingHealth}/" +
            $"{currentCharacter.PlayerStartingHealth}";
        characterStartingGold.text =
            currentCharacter.PlayerStartingGold.ToString();
        characterStartingRelicName.text =
            currentCharacter.StartingRelicData.RelicName;
        characterStartingRelicDescription.text =
            currentCharacter.StartingRelicData.RelicDescription;
        characterStartingRelicSprite.sprite =
            currentCharacter.StartingRelicData.RelicIcon;
        
        UpdateSelectedSlotVisuals();
    }

    /// <summary>
    /// Choose and select the character for the run
    /// </summary>
    public void SelectCharacter()
    {
        if (displayedCharacters.Count == 0)
        {
            Debug.LogError(
                "Cannot select a character because none are " +
                "unlocked.",
                this);

            return;
        }

        PlayerUnitData selectedCharacter =
            displayedCharacters[currentIndex];

        if (selectedCharacter == null)
        {
            Debug.LogError("Cannot select a character because it is null.", this);
            return;
        }
        
        if (!progression.IsUnlocked(selectedCharacter))
        {
            Debug.LogError(
                "Cannot select a locked character.",
                this);

            return;
        }

        RunManager.Instance.ChoosePlayerUnit(selectedCharacter);
    }

    private void RefreshAvailableCharacters()
    {
        displayedCharacters.Clear();
        ClearCharacterSlots();

        progression = GlobalProgressionManager.Instance;

        if (characterCatalog == null)
        {
            Debug.LogError(
                "Character selection requires a character catalog.",
                this);

            return;
        }

        if (progression == null)
        {
            Debug.LogError(
                "Character selection requires global progression.",
                this);

            return;
        }

        foreach (PlayerUnitData character in characterCatalog.Characters)
        {
            if (character != null)
                displayedCharacters.Add(character);
        }

        CreateCharacterSlots();

        int firstUnlockedIndex = FindFirstUnlockedCharacterIndex();

        if (firstUnlockedIndex < 0)
        {
            currentIndex = -1;

            Debug.LogWarning(
                "No unlocked characters are available.",
                this);

            return;
        }

        if (currentIndex < 0 ||
            currentIndex >= displayedCharacters.Count ||
            !progression.IsUnlocked(displayedCharacters[currentIndex]))
        {
            currentIndex = firstUnlockedIndex;
        }

        UpdateCharacterSelectScreen(currentIndex);
    }
    
    private int FindFirstUnlockedCharacterIndex()
    {
        for (int i = 0; i < displayedCharacters.Count; i++)
        {
            if (progression.IsUnlocked(displayedCharacters[i]))
                return i;
        }

        return -1;
    }
    
    private void CreateCharacterSlots()
    {
        for (int i = 0; i < displayedCharacters.Count; i++)
        {
            PlayerUnitData character = displayedCharacters[i];
            bool isUnlocked = progression.IsUnlocked(character);

            CharacterSlotVisual slot = Instantiate(
                characterSlotPrefab,
                characterSlotContainer);

            slot.name = $"CharacterSlot_{i}_{character.UnitName}";

            slot.Initialize(
                character,
                isUnlocked,
                i,
                HandleCharacterSlotClicked);

            spawnedSlots.Add(slot);
        }
    }
    
    private void ClearCharacterSlots()
    {
        foreach (CharacterSlotVisual slot in spawnedSlots)
        {
            if (slot == null)
                continue;

            // Remove it from the layout immediately.
            slot.gameObject.SetActive(false);
            Destroy(slot.gameObject);
        }

        spawnedSlots.Clear();
    }
    
    private void HandleCharacterSlotClicked(int index)
    {
        if (index < 0 || index >= displayedCharacters.Count)
            return;

        PlayerUnitData character = displayedCharacters[index];

        if (!progression.IsUnlocked(character))
            return;

        UpdateCharacterSelectScreen(index);
    }

    private void UpdateSelectedSlotVisuals()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            spawnedSlots[i].SetSelected(i == currentIndex);
        }
    }

}
