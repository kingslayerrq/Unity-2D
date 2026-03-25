using UnityEngine;

public interface ICardUser
{
    int CurrentEnergy { get; }
    void ResetEnergy();
    void SpendEnergy(int amount);
    void ResetBlock(ResetBlockEvent resetEvent);
}
