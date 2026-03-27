using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// For graphics raycast
[RequireComponent(typeof(Image))]
public class PlayCardArea : MonoBehaviour, ICardDropReceiver
{
    [SerializeField] private List<UnitInstance> playerTargets;
    [SerializeField] private List<UnitInstance> enemiesTargets;
    
    private List<ICardTarget> highlightedTargets;

    public int Priority { get; private set; } = CONSTANTS.PLAY_AREA_TARGET_PRIORITY;
    

    public List<UnitInstance> GetReceiverUnitInstance(UnitInstance source, CardInstance cardInstance)
    {
        return highlightedTargets.Select(target => target.Target).ToList();
    }

    public bool CanReceiveCard(UnitInstance source, CardInstance cardInstance)
    {
        return ValidateCardTargets(source, cardInstance).Count != 0;
    }

    public List<ICardTarget> ValidateCardTargets(UnitInstance source, CardInstance cardInstance)
    {
        List<ICardTarget> targets = new List<ICardTarget>();
        switch (cardInstance.CurrentTargetType)
        {
            case TargetType.AllAlly:
                targets.AddRange(playerTargets);
                break;
            case TargetType.AllEnemy:
                targets.AddRange(enemiesTargets);
                break;
            case TargetType.None:
                break;
            case TargetType.Any:
                targets.AddRange(playerTargets);
                targets.AddRange(enemiesTargets);
                break;
            case TargetType.Self:
                targets.Add(source);
                break;
            default:
                break;
        }
        return targets;
    }

    public void OnHoverEnter(UnitInstance source, CardInstance cardInstance)
    {
        var targets = ValidateCardTargets(source, cardInstance);
        highlightedTargets = targets;
        foreach (var target in targets)
        {
            target.Target.UnitUIManager.UnitHighlight.HighlightUnit();
        }
    }

    public void OnHoverExit(UnitInstance source, CardInstance cardInstance)
    {
        if (highlightedTargets == null) return;
        foreach (var target in highlightedTargets)
        {
            target.Target.UnitUIManager.UnitHighlight.HideHighlight();
        }

        highlightedTargets = null;
    }
}
