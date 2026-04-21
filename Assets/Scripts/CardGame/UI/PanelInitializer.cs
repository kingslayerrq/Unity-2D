using UnityEngine;

public class PanelInitializer : MonoBehaviour
{
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject combatPanel;

    private void Awake()
    {
        RunManager.Instance.RegisterUIPanels(characterSelectPanel, mapPanel, combatPanel);
    }
}
