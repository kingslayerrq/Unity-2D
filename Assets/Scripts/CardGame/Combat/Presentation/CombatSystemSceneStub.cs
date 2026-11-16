using UnityEngine;

/// <summary>
/// Does nothing. It exists so the component that used to be the CombatSystem
/// MonoBehaviour in the combat scene still resolves to a script.
///
/// <see cref="CombatSystem"/> is now a plain class created by
/// <see cref="CombatManager"/>. Once this component has been removed from the
/// scene in the editor, this file can be deleted.
/// </summary>
[AddComponentMenu("")]
public class CombatSystemSceneStub : MonoBehaviour
{
}
