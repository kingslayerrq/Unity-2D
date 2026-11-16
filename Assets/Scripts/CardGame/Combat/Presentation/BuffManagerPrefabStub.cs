using UnityEngine;

/// <summary>
/// Does nothing. It exists so the component that used to be the BuffManager
/// MonoBehaviour on the unit prefabs still resolves to a script.
///
/// <see cref="BuffManager"/> is now a plain class owned by each unit. Once
/// this component has been removed from the Player and Enemy prefabs in the
/// editor, this file can be deleted.
/// </summary>
[AddComponentMenu("")]
public class BuffManagerPrefabStub : MonoBehaviour
{
}
