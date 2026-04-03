using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleCardsAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly CardInstance _sourceCard;
    private readonly IReadOnlyList<CardData> _cardDatas;
    private readonly CardPileType _targetCardPilesTypes;
    private readonly BaseUnitInstance _source;
    private readonly IReadOnlyList<BasePlayerUnitInstance> _targets;

    public ShuffleCardsAction(CombatSystem combatSystem, BaseUnitInstance source,
        CardInstance sourceCard,
        IReadOnlyList<BasePlayerUnitInstance> targets,
        IReadOnlyList<CardData> cardDatas, CardPileType targetCardPilesTypes)
    {
        _combatSystem = combatSystem;
        _sourceCard = sourceCard;
        _cardDatas = cardDatas;
        _targetCardPilesTypes = targetCardPilesTypes;
        _source = source;
        _targets = targets;
    }

    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveShuffleCardsToPile(_source, _targets, _sourceCard,  _cardDatas, _targetCardPilesTypes);
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"ShuffleCardsAction: SourceUnit-{_source} | SourceCard-{_sourceCard} | Targets-{string.Join(", ", _targets)} | " +
               $"CardsToShuffle-{string.Join(", ", _cardDatas)} | TargetCardPileTypes-{string.Join(", ", _targetCardPilesTypes)}";
    }
}
