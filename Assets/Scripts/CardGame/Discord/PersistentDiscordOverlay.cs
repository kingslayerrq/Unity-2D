using UnityEngine;

public sealed class PersistentDiscordOverlay : MonoBehaviour
{
    public static PersistentDiscordOverlay Instance { get; private set; }

    private void Awake()
    {
#if !DISCORD_SOCIAL_SDK_EXISTS
        // Builds without the Discord Social SDK (WebGL) compile out the code
        // that hides the lobby and connect controls, so they would stay on
        // screen in every scene. Nothing in the overlay works there; hide it.
        gameObject.SetActive(false);
        return;
#else
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
#endif
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}