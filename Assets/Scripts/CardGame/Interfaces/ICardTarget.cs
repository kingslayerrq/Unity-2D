using UnityEngine;

public interface ICardTarget
{
    void TakeDamage(int amount, BaseUnitInstance source);
    void Heal(int amount);
    void Block(int amount);
    void ReceiveBuff(BuffData buffData, int stacks);
    /// <summary>
    /// After dmg taken -> Buffmanager executes -> check final health
    /// </summary>
    void OnDmgResolved();
    /// <summary>
    /// get the UnitInstance 
    /// </summary>
    BaseUnitInstance Target { get; }
}
