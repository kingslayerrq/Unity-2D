using System.Collections.Generic;
using UnityEngine;

public interface ICardDropReceiver
{
    int Priority { get; }
    List<UnitInstance> GetReceiverUnitInstance (UnitInstance source, CardInstance cardInstance);
    bool CanReceiveCard(UnitInstance source, CardInstance cardInstance);
    List<ICardTarget> ValidateCardTargets(UnitInstance source, CardInstance cardInstance);
    void OnHoverEnter(UnitInstance source, CardInstance cardInstance);
    void OnHoverExit(UnitInstance source, CardInstance cardInstance);
}
