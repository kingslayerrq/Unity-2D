using UnityEngine;

public interface ICardTarget
{
    void TakeDamage(int amount);
    void Heal(int amount);
    void Block(int amount);
    void ReceiveBuff(BuffData buffData, int stacks);
    /// <summary>
    /// get the UnitInstance 
    /// </summary>
    UnitInstance Target { get; }
}
