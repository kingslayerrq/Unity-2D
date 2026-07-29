using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if DISCORD_SOCIAL_SDK_EXISTS
using Discord.Sdk;
#endif

/// <summary>
/// ConnectButton handles the connection to Discord when the user clicks
/// the connect button.
/// </summary>
public class ConnectButton : MonoBehaviour
{
    [SerializeField] private Button connectButton;

#if DISCORD_SOCIAL_SDK_EXISTS
    private DiscordManager subscribedManager;

    private void Start()
    {
        if (connectButton == null)
        {
            Debug.LogError(
                "Connect to Discord button reference is missing.");
            return;
        }

        subscribedManager = DiscordManager.Instance;

        if (subscribedManager == null)
        {
            Debug.LogError(
                "There is no DiscordManager instance in the scene.");
            return;
        }

        if (GetComponentInParent<Canvas>() == null)
        {
            Debug.LogError("The ConnectButton must be inside a Canvas.");
        }

        if (EventSystem.current == null)
        {
            Debug.LogError("No EventSystem was found.");
        }

        // Subscribe here.
        connectButton.onClick.AddListener(OnConnectButtonClicked);
        subscribedManager.OnDiscordStatusChanged += OnStatusChanged;

        // The persistent manager might already have connected before this
        // scene-local button was created.
        if (subscribedManager.GetClient().GetStatus() == Client.Status.Ready)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe here.
        if (connectButton != null)
        {
            connectButton.onClick.RemoveListener(OnConnectButtonClicked);
        }

        if (subscribedManager != null)
        {
            subscribedManager.OnDiscordStatusChanged -= OnStatusChanged;
        }
    }

    private void OnConnectButtonClicked()
    {
        if (subscribedManager == null)
        {
            Debug.LogError("The DiscordManager is no longer available.");
            return;
        }

        subscribedManager.StartOAuthFlow();
    }

    private void OnStatusChanged(
        Client.Status status,
        Client.Error error,
        int errorCode)
    {
        if (status == Client.Status.Ready)
        {
            gameObject.SetActive(false);
        }
    }
#endif
}