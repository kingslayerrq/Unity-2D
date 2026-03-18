using UnityEngine;

public interface ICardUser
{
    int CurrentEnergy { get; }
    void SpendEnergy(int amount);
}
