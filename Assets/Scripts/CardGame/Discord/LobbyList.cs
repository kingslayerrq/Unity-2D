using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

#if DISCORD_SOCIAL_SDK_EXISTS
using Discord.Sdk;
#endif

/// <summary>
/// LobbyList manages the UI list of players currently in the lobby. It loads and displays each player's avatar and keeps the list updated
/// as players join or leave the lobby.
/// </summary>
public class LobbyList : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPlayerUIPrefab;
    [SerializeField] private Transform content;
    private Dictionary<ulong, Transform> lobbyPlayerUIObjects = new Dictionary<ulong, Transform>();

#if DISCORD_SOCIAL_SDK_EXISTS
    private Client client;
    private DiscordManager subscribedManager;
    private Lobby lobbyController;
    private ulong displayedLobbyId;

    void Start()
    {
        lobbyController = GetComponentInParent<Lobby>();
        subscribedManager = DiscordManager.Instance;
        if (subscribedManager == null)
        {
            Debug.LogError(
                "LobbyList requires a DiscordManager in the persistent overlay.");
            content?.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        client = subscribedManager.GetClient();

        subscribedManager.OnDiscordLobbyCreated += LobbyCreatedOrJoined;
        subscribedManager.OnDiscordLobbyDeleted += LobbyDeleted;
        subscribedManager.OnDiscordLobbyMemberAdded += LobbyMemberAdded;
        subscribedManager.OnDiscordLobbyMemberRemoved += LobbyMemberRemoved;

        ClearLobby();
    }

    private void OnDestroy()
    {
        if (subscribedManager == null)
        {
            return;
        }

        subscribedManager.OnDiscordLobbyCreated -= LobbyCreatedOrJoined;
        subscribedManager.OnDiscordLobbyDeleted -= LobbyDeleted;
        subscribedManager.OnDiscordLobbyMemberAdded -= LobbyMemberAdded;
        subscribedManager.OnDiscordLobbyMemberRemoved -= LobbyMemberRemoved;
    }

    private void LobbyCreatedOrJoined(ulong lobbyId)
    {
        if (lobbyController == null ||
            lobbyController.GetCurrentLobbyId() != lobbyId)
        {
            return;
        }

        ShowLobby(lobbyId);
    }

    public void ShowLobby(ulong lobbyId)
    {
        ClearLobby();

        if (lobbyId == 0 || client == null)
        {
            return;
        }

        LobbyHandle lobby = client.GetLobbyHandle(lobbyId);
        if (lobby == null)
        {
            return;
        }

        displayedLobbyId = lobbyId;
        content.gameObject.SetActive(true);

        foreach (ulong userId in lobby.LobbyMemberIds())
        {
            LobbyMemberAdded(lobbyId, userId);
        }
    }

    public void ClearLobby()
    {
        StopAllCoroutines();

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        lobbyPlayerUIObjects.Clear();
        displayedLobbyId = 0;
        content.gameObject.SetActive(false);
    }

    private void LobbyDeleted(ulong lobbyId)
    {
        if (lobbyId == displayedLobbyId)
        {
            ClearLobby();
        }
    }

    private void LobbyMemberAdded(ulong lobbyId, ulong userId)
    {
        if (lobbyId != displayedLobbyId)
        {
            return;
        }

        if (lobbyPlayerUIObjects.ContainsKey(userId))
        {
            return;
        }

        GameObject playerUIObject = Instantiate(lobbyPlayerUIPrefab, content);
        lobbyPlayerUIObjects[userId] = playerUIObject.transform;

        UserHandle user = client.GetLobbyHandle(lobbyId)?.GetLobbyMemberHandle(userId)?.User();
        if (user != null)
        {
            StartCoroutine(LoadAvatarFromUrl(user.AvatarUrl(UserHandle.AvatarType.Png, UserHandle.AvatarType.Png), playerUIObject.GetComponentInChildren<Image>()));
        }
        else
        {
            Debug.LogWarning($"No UserHandle available for lobby member {userId}; avatar will not be loaded.");
        }
    }

    private void LobbyMemberRemoved(ulong lobbyId, ulong userId)
    {
        if (lobbyId != displayedLobbyId)
        {
            return;
        }

        if (lobbyPlayerUIObjects.TryGetValue(userId, out Transform playerUITransform))
        {
            Destroy(playerUITransform.gameObject);
            lobbyPlayerUIObjects.Remove(userId);
        }
    }

    private IEnumerator LoadAvatarFromUrl(string url, Image profileImage)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                profileImage.sprite = sprite;
            }
            else
            {
                Debug.LogError($"Failed to load profile image from URL: {url}. Error: {request.error}");
            }
        }
    }
#endif
}
