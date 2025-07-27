using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.InputSystem;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }
    public SessionManager() => Instance = this;
    ISession m_activeSession;
    public ISession activeSession
    {
        get => m_activeSession;
        private set
        {
            m_activeSession = value;
            Debug.Log($"Active Session Changed: {value.Name}");
            onSessionChange?.Invoke(m_activeSession);
        }
    }
    public Action<ISession> onSessionChange;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    public Action onSignIn;
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Player: {AuthenticationService.Instance.PlayerName}");
            onSignIn?.Invoke();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

    public const string playerNamePropertyKey = "playerName";
    async UniTask<Dictionary<string, PlayerProperty>> GetPlayerProperties()
    {
        string playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        PlayerProperty playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Public);
        return new Dictionary<string, PlayerProperty>()
        {
            { playerNamePropertyKey, playerNameProperty }
        };
    }
    public const int maxPlayers = 8;
    private async UniTask<SessionOptions> GetSessionOptions()
    {
        Dictionary<string, PlayerProperty> playerProperties = await GetPlayerProperties();

        SessionOptions options = new()
        {
            MaxPlayers = maxPlayers,
            IsLocked = false,
            IsPrivate = false,
            PlayerProperties = playerProperties
        };
        return options.WithDistributedAuthorityNetwork();
    }
    public async UniTaskVoid StartSession()
    {
        activeSession = await MultiplayerService.Instance.CreateSessionAsync(await GetSessionOptions());
    }
    public async UniTaskVoid JoinOrStartSession(string sessionId)
    {
        activeSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, await GetSessionOptions());
        Debug.Log($"Session {activeSession.Id} Joined");
    }
    public async UniTaskVoid JoinSessionById(string sessionId)
    {
        activeSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
        Debug.Log($"Session {activeSession.Id} Joined");
    }
    public async UniTaskVoid JoinSessionByCode(string sessionCode)
    {
        activeSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
        Debug.Log($"Session {activeSession.Id} Joined");
    }
    public async UniTaskVoid KickPlayer(string playerId)
    {
        if (!activeSession.IsHost) return;
        await activeSession.AsHost().RemovePlayerAsync(playerId);
    }
    public async UniTask<IList<ISessionInfo>> QuerySessions()
    {
        var sessionQueryOptions = new QuerySessionsOptions()
        {
            SortOptions = new List<SortOption>()
            {
                new SortOption(SortOrder.Descending, SortField.AvailableSlots)
            }
        };
        var results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
        return results.Sessions;
    }
    public async UniTask LeaveSession()
    {
        if(activeSession != null)
        {
            try
            {
                await activeSession.LeaveAsync();
            }
            catch { }
            finally
            {
                activeSession = null;
            }
        }
    }
}
