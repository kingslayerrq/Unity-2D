using System.Collections;
using UnityEngine;

public interface ICombatAction
{
    IEnumerator ExecuteAction();
    string GetActionDetails();
}
