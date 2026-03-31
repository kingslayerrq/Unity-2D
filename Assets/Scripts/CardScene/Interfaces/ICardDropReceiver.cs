using System.Collections.Generic;
using UnityEngine;

public interface ICardDropReceiver
{
    int Priority { get; }
    List<BaseUnitInstance> GetReceiverUnitInstance (BaseUnitInstance source, CardInstance cardInstance);
    bool CanReceiveCard(BaseUnitInstance source, CardInstance cardInstance);
    List<ICardTarget> ValidateCardTargets(BaseUnitInstance source, CardInstance cardInstance);
    void OnHoverEnter(BaseUnitInstance source, CardInstance cardInstance);
    void OnHoverExit(BaseUnitInstance source, CardInstance cardInstance);
}
