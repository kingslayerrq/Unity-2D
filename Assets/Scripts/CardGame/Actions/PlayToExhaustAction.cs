using System.Collections;

public class PlayToExhaustAction : ICombatAction
{
    private readonly BasePlayerUnitInstance _source;
    private readonly CardInstance _card;

    public PlayToExhaustAction(BasePlayerUnitInstance source, CardInstance card)
    {
        _source = source;
        _card = card;
    }

    public IEnumerator ExecuteAction()
    {
        yield return _source.CombatManager.PlayerHandView.PlayToDiscardAnimation(_card);

        _source.CombatManager.PlayerHandView.CompleteAnimateOut(_card);
        _source.CombatManager.PlayerHandView.RefreshHand(_source.Hand.Cards);
    }

    public string GetActionDetails()
    {
        return $"PlayToExhaustAction: Card-{_card?.CurrentName}";
    }
}
