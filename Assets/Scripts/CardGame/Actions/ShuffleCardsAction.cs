using System.Collections.Generic;
using UnityEngine;

public class ShuffleCardsAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly CardInstance _sourceCard;
    private readonly IReadOnlyList<CardInstance> _cardInstances;
    private readonly CardPileType _targetCardPilesTypes;
    private readonly BaseUnitInstance _source;
    private readonly IReadOnlyList<BasePlayerUnitInstance> _targets;

    public ShuffleCardsAction(CombatSystem combatSystem, BaseUnitInstance source,
        CardInstance sourceCard,
        IReadOnlyList<BasePlayerUnitInstance> targets,
        IReadOnlyList<CardInstance> cardInstances, CardPileType targetCardPilesTypes)
    {
        _combatSystem = combatSystem;
        _sourceCard = sourceCard;
        _cardInstances = cardInstances;
        _targetCardPilesTypes = targetCardPilesTypes;
        _source = source;
        _targets = targets;
    }

    public void Execute()
    {
        _combatSystem.ResolveShuffleCardsToPile(_source, _targets, _sourceCard,  _cardInstances, _targetCardPilesTypes);
    }

    public string GetActionDetails()
    {
        return $"ShuffleCardsAction: SourceUnit-{_source} | SourceCard-{_sourceCard} | Targets-{string.Join(", ", _targets)} | " +
               $"CardsToShuffle-{string.Join(", ", _cardInstances)} | TargetCardPileTypes-{string.Join(", ", _targetCardPilesTypes)}";
    }
}
