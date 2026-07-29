using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


#if DISCORD_SOCIAL_SDK_EXISTS
using Discord.Sdk;
#endif

/// <summary>
/// Lobby handles creating, joining, and leaving Discord Lobbies. Through the UI the player can create or leave a lobby.
/// Joining a lobby directly happens through invites and Rich Presence.
/// </summary>
public class Lobby : MonoBehaviour
{
    private const string DefaultTeamAssemblySceneName = "EntryScene";

    [SerializeField] private GameObject teamAssemblyUI;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private int maxLobbySize = 4;
    [SerializeField] private string teamAssemblySceneName =
        DefaultTeamAssemblySceneName;
    private string lobbySecret = string.Empty;
    private ulong currentLobby = 0;
    
    private RunManager runManager;
    private bool teamAssemblyLocked = false;
    private bool endSessionLeavePending = false;
    public bool CanAssembleTeam =>
        !teamAssemblyLocked &&
        !endSessionLeavePending &&
        SceneManager.GetActiveScene().name == TeamAssemblySceneName;

    private string TeamAssemblySceneName =>
        string.IsNullOrWhiteSpace(teamAssemblySceneName)
            ? DefaultTeamAssemblySceneName
            : teamAssemblySceneName;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(teamAssemblySceneName))
        {
            teamAssemblySceneName = DefaultTeamAssemblySceneName;
        }
    }

#if DISCORD_SOCIAL_SDK_EXISTS
    private Client client;
    private RichPresence richPresence;
    private LobbyList lobbyList;
    private DiscordManager subscribedManager;
    private bool lobbyRequestPending;
    private bool leaveRequestPending;
    private ulong lobbyBeingLeft;
    private Coroutine endSessionReconnectCoroutine;
    private readonly HashSet<ulong> deferredLobbyIds = new HashSet<ulong>();
    private readonly HashSet<ulong> staleLobbyLeaveRequests = new HashSet<ulong>();

    void Start()
    {
        richPresence = FindFirstObjectByType<RichPresence>();
        lobbyList = GetComponentInChildren<LobbyList>(true);

        subscribedManager = DiscordManager.Instance;
        if (subscribedManager == null)
        {
            Debug.LogError(
                "Lobby requires a DiscordManager in the persistent overlay.");
            enabled = false;
            return;
        }

        client = subscribedManager.GetClient();

        subscribedManager.OnDiscordStatusChanged += OnStatusChanged;
        subscribedManager.OnDiscordActivityJoin += OnActivityJoin;
        subscribedManager.OnDiscordLobbyCreated += OnDiscordLobbyCreated;
        subscribedManager.OnDiscordLobbyDeleted += OnDiscordLobbyDeleted;

        createLobbyButton.onClick.AddListener(CreateLobby);
        leaveLobbyButton.onClick.AddListener(LeaveLobby);

        createLobbyButton.gameObject.SetActive(false);
        leaveLobbyButton.gameObject.SetActive(false);
        
        runManager = RunManager.Instance;
        if (runManager != null)
        {
            teamAssemblyLocked = runManager.HasRunStarted;
            runManager.OnRunStarted += LockTeamAssembly;
            runManager.OnRunEnded += EndLobbySession;
        }

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        if (client != null && client.GetStatus() == Client.Status.Ready)
        {
            ReconcileLoadedLobbies();
        }
        
        RefreshLobbyControls();
    }
    
    private void RefreshLobbyControls()
    {
        bool canShowTeamAssembly = CanAssembleTeam;

        if (teamAssemblyUI != null)
        {
            teamAssemblyUI.SetActive(canShowTeamAssembly);
        }

        if (!canShowTeamAssembly)
        {
            createLobbyButton.gameObject.SetActive(false);
            leaveLobbyButton.gameObject.SetActive(false);
            return;
        }

        bool discordReady =
            client != null &&
            client.GetStatus() == Client.Status.Ready;

        createLobbyButton.gameObject.SetActive(
            discordReady &&
            !IsInLobby() &&
            !lobbyRequestPending &&
            !leaveRequestPending &&
            !endSessionLeavePending &&
            staleLobbyLeaveRequests.Count == 0);

        leaveLobbyButton.gameObject.SetActive(
            discordReady &&
            IsInLobby() &&
            !lobbyRequestPending &&
            !leaveRequestPending &&
            !endSessionLeavePending);
    }
    
    private void LockTeamAssembly()
    {
        teamAssemblyLocked = true;
        RefreshLobbyControls();
    }

    private void EndLobbySession()
    {
        teamAssemblyLocked = false;
        endSessionLeavePending = true;
        lobbyList?.ClearLobby();

        if (richPresence != null)
        {
            richPresence.SetDefaultRichPresence();
        }

        if (currentLobby == 0)
        {
            CompleteEndedSessionLocally();
            return;
        }

        if (client != null &&
            client.GetStatus() == Client.Status.Ready)
        {
            RequestLeaveCurrentLobby(true);
        }
        else
        {
            RefreshLobbyControls();
        }
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene nextScene)
    {
        RefreshLobbyControls();
    }

    void OnDestroy()
    {
        if (createLobbyButton != null)
        {
            createLobbyButton.onClick.RemoveListener(CreateLobby);
        }

        if (leaveLobbyButton != null)
        {
            leaveLobbyButton.onClick.RemoveListener(LeaveLobby);
        }

        if (subscribedManager != null)
        {
            subscribedManager.OnDiscordStatusChanged -= OnStatusChanged;
            subscribedManager.OnDiscordActivityJoin -= OnActivityJoin;
            subscribedManager.OnDiscordLobbyCreated -= OnDiscordLobbyCreated;
            subscribedManager.OnDiscordLobbyDeleted -= OnDiscordLobbyDeleted;
        }
        
        if (runManager != null)
        {
            runManager.OnRunStarted -= LockTeamAssembly;
            runManager.OnRunEnded -= EndLobbySession;
        }

        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    public bool IsInLobby()
    {
        // return lobbySecret != string.Empty;
        return currentLobby != 0;
    }

    public ulong GetCurrentLobbyId()
    {
        return currentLobby;
    }

    private void OnStatusChanged(Client.Status status, Client.Error error, int errorCode)
    {
        if (status == Client.Status.Ready)
        {
            ReconcileLoadedLobbies();
            ContinueEndedSessionCleanup();
        }
        else if (endSessionReconnectCoroutine != null)
        {
            StopCoroutine(endSessionReconnectCoroutine);
            endSessionReconnectCoroutine = null;
        }

        RefreshLobbyControls();
    }

    private void ReconcileLoadedLobbies()
    {
        foreach (ulong lobbyId in client.GetLobbyIds())
        {
            OnDiscordLobbyCreated(lobbyId);
        }
    }

    private void OnDiscordLobbyCreated(ulong lobbyId)
    {
        if (lobbyId == 0)
        {
            return;
        }

        if (endSessionLeavePending)
        {
            if (lobbyId == currentLobby)
            {
                RequestLeaveCurrentLobby(true);
            }
            else
            {
                LeaveStaleLobby(lobbyId);
            }

            return;
        }

        if (lobbyId == currentLobby)
        {
            lobbyList?.ShowLobby(lobbyId);
            return;
        }

        // A lobby-created callback can arrive before the matching
        // CreateOrJoinLobby callback. Defer deciding whether it is stale until
        // that request tells us which lobby was intentionally joined.
        if (lobbyRequestPending)
        {
            deferredLobbyIds.Add(lobbyId);
            return;
        }

        LeaveStaleLobby(lobbyId);
    }

    private void OnDiscordLobbyDeleted(ulong lobbyId)
    {
        staleLobbyLeaveRequests.Remove(lobbyId);
        deferredLobbyIds.Remove(lobbyId);

        if (lobbyId == lobbyBeingLeft)
        {
            leaveRequestPending = false;
            lobbyBeingLeft = 0;
        }

        if (lobbyId != currentLobby)
        {
            RefreshLobbyControls();
            return;
        }

        currentLobby = 0;
        lobbySecret = string.Empty;
        lobbyList?.ClearLobby();
        endSessionLeavePending = false;

        if (richPresence != null)
        {
            richPresence.SetDefaultRichPresence();
        }

        RefreshLobbyControls();
    }

    private void ContinueEndedSessionCleanup()
    {
        if (!endSessionLeavePending ||
            leaveRequestPending ||
            currentLobby == 0)
        {
            return;
        }

        if (IsLobbyLoaded(currentLobby))
        {
            RequestLeaveCurrentLobby(true);
            return;
        }

        if (endSessionReconnectCoroutine == null)
        {
            endSessionReconnectCoroutine =
                StartCoroutine(ResolveEndedSessionAfterReconnect());
        }
    }

    private IEnumerator ResolveEndedSessionAfterReconnect()
    {
        // Lobby IDs may not be loaded immediately when Discord first reports
        // Ready. Give the SDK callbacks time to restore the membership.
        yield return new WaitForSecondsRealtime(1f);
        endSessionReconnectCoroutine = null;

        if (!endSessionLeavePending ||
            client == null ||
            client.GetStatus() != Client.Status.Ready)
        {
            yield break;
        }

        if (leaveRequestPending)
        {
            yield break;
        }

        if (IsLobbyLoaded(currentLobby))
        {
            RequestLeaveCurrentLobby(true);
        }
        else
        {
            // If Discord later restores this membership, the normal
            // lobby-created handler will classify it as stale and leave it.
            CompleteEndedSessionLocally();
        }
    }

    private bool IsLobbyLoaded(ulong lobbyId)
    {
        if (client == null || lobbyId == 0)
        {
            return false;
        }

        foreach (ulong loadedLobbyId in client.GetLobbyIds())
        {
            if (loadedLobbyId == lobbyId)
            {
                return true;
            }
        }

        return false;
    }

    private void CompleteEndedSessionLocally()
    {
        if (endSessionReconnectCoroutine != null)
        {
            StopCoroutine(endSessionReconnectCoroutine);
            endSessionReconnectCoroutine = null;
        }

        endSessionLeavePending = false;
        leaveRequestPending = false;
        lobbyBeingLeft = 0;
        currentLobby = 0;
        lobbySecret = string.Empty;
        lobbyList?.ClearLobby();

        if (richPresence != null)
        {
            richPresence.SetDefaultRichPresence();
        }

        RefreshLobbyControls();
    }

    private void LeaveStaleLobby(ulong lobbyId)
    {
        if (client == null ||
            lobbyId == 0 ||
            lobbyId == currentLobby ||
            !staleLobbyLeaveRequests.Add(lobbyId))
        {
            return;
        }

        RefreshLobbyControls();
        client.LeaveLobby(
            lobbyId,
            result => OnStaleLobbyLeft(lobbyId, result));
    }

    private void OnStaleLobbyLeft(
        ulong lobbyId,
        ClientResult clientResult)
    {
        staleLobbyLeaveRequests.Remove(lobbyId);

        if (clientResult.Successful())
        {
            Debug.Log($"Left stale Discord lobby {lobbyId}.");
        }
        else
        {
            Debug.LogWarning(
                $"Could not leave stale Discord lobby {lobbyId}: " +
                $"{clientResult}");
        }

        RefreshLobbyControls();
    }

    private void ResolveDeferredLobbies(ulong acceptedLobbyId)
    {
        var lobbyIdsToResolve = new List<ulong>(deferredLobbyIds);
        deferredLobbyIds.Clear();

        foreach (ulong lobbyId in lobbyIdsToResolve)
        {
            if (lobbyId != acceptedLobbyId)
            {
                LeaveStaleLobby(lobbyId);
            }
        }
    }

    private void CreateLobby()
    {
        if (!CanAssembleTeam)
        {
            Debug.LogWarning(
                "A lobby can only be created while assembling a team " +
                "in the entry scene.");
            return;
        }

        if (!CanStartLobbyRequest())
        {
            return;
        }
        
        StopAllCoroutines();
        lobbyRequestPending = true;
        lobbySecret = System.Guid.NewGuid().ToString();
        RefreshLobbyControls();
        client.CreateOrJoinLobby(lobbySecret, OnCreateOrJoinLobby);
    }

    private void OnActivityJoin(string joinSecret)
    {
        JoinLobby(joinSecret);
    }

    public void JoinLobby(string lobbySecret)
    {
        if (!CanAssembleTeam)
        {
            Debug.LogWarning(
                "A lobby can only be joined while assembling a team " +
                "in the entry scene.");
            return;
        }

        if (!CanStartLobbyRequest())
        {
            return;
        }
        
        lobbyRequestPending = true;
        this.lobbySecret = lobbySecret;
        RefreshLobbyControls();
        client.CreateOrJoinLobby(this.lobbySecret, OnCreateOrJoinLobby);
    }

    private bool CanStartLobbyRequest()
    {
        if (client == null ||
            client.GetStatus() != Client.Status.Ready ||
            !CanAssembleTeam ||
            IsInLobby() ||
            lobbyRequestPending ||
            leaveRequestPending ||
            staleLobbyLeaveRequests.Count > 0)
        {
            Debug.LogWarning(
                "Discord is not ready to create or join a lobby.");
            return false;
        }

        return true;
    }

    private void OnCreateOrJoinLobby(ClientResult clientResult, ulong lobbyId)
    {
        lobbyRequestPending = false;

        if (clientResult.Successful())
        {
            currentLobby = lobbyId;
            lobbyList?.ShowLobby(lobbyId);

            if(richPresence != null)
            {
                richPresence.UpdateRichPresenceLobby(ActivityTypes.Playing, "In Lobby", "Waiting for players", lobbySecret, lobbyId.ToString(), maxLobbySize);
            }

            Debug.Log($"Successfully created or joined lobby {lobbyId}");
        }
        else
        {
            // Reset
            currentLobby = 0;
            lobbySecret = string.Empty;

            Debug.LogError($"Failed to create or join lobby: {clientResult}");
        }

        ResolveDeferredLobbies(currentLobby);
        RefreshLobbyControls();
    }

    private void LeaveLobby()
    {
        if (teamAssemblyLocked)
        {
            Debug.LogWarning(
                "The lobby cannot be left after the run has started.");
            return;
        }

        if (client == null || currentLobby == 0)
        {
            Debug.LogWarning(
                "Cannot leave because no lobby is active.");
            return;
        }

        RequestLeaveCurrentLobby(false);
    }

    private void RequestLeaveCurrentLobby(bool endingSession)
    {
        if (client == null ||
            client.GetStatus() != Client.Status.Ready ||
            currentLobby == 0 ||
            leaveRequestPending)
        {
            return;
        }

        ulong lobbyIdToLeave = currentLobby;
        leaveRequestPending = true;
        lobbyBeingLeft = lobbyIdToLeave;
        RefreshLobbyControls();
        client.LeaveLobby(
            lobbyIdToLeave,
            result => OnLeaveLobby(
                lobbyIdToLeave,
                endingSession,
                result));
    }

    private void OnLeaveLobby(
        ulong lobbyId,
        bool endingSession,
        ClientResult clientResult)
    {
        if (lobbyBeingLeft == lobbyId)
        {
            leaveRequestPending = false;
            lobbyBeingLeft = 0;
        }

        if (clientResult.Successful())
        {
            Debug.Log($"Successfully left lobby {lobbyId}");

            if (currentLobby == lobbyId)
            {
                currentLobby = 0;
                lobbySecret = string.Empty;
                lobbyList?.ClearLobby();
            }

            if(richPresence != null)
            {
                richPresence.SetDefaultRichPresence();
            }
        }
        else
        {
            Debug.LogError($"Failed to leave lobby: {clientResult}");

            if (endingSession)
            {
                // The game session is already over. Clear local ownership so
                // a later SDK restore is handled by stale-lobby cleanup.
                CompleteEndedSessionLocally();
                return;
            }
        }

        if (endingSession)
        {
            CompleteEndedSessionLocally();
            return;
        }

        RefreshLobbyControls();
    }
#endif
}
