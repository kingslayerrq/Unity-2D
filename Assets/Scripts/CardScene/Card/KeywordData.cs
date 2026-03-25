using UnityEngine;

[CreateAssetMenu(fileName = "NewKeyword", menuName = "Cards/Keyword Data")]
public class KeywordData : ScriptableObject
{
    [Header("Keyword Info")]
    public string keywordName; // e.g., "Exhaust"
    
    [TextArea(3, 5)] 
    public string definition; // e.g., "Remove this card from your deck until the end of combat."
    
    public Sprite keywordIcon; 
}