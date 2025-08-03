using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameManager() => Instance = this;

    [Header("Player")]
    [SerializeField] Player playerPrefab;
    [SerializeField] Transform lobbySpawnPoint;

    [Header("Game")]
    [SerializeField] int intermissionTime = 30;
    [SerializeField] int roundTime = 300;
    [SerializeField] Transform teamRedSpawnpoint, teamBlueSpawnpoint;

    public bool roundInProgress { get; private set; } = false;
    [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
    public void SetRoundInProgressRpc(bool roundInProgress, RpcParams rpcParams = new())
    {
        this.roundInProgress = roundInProgress;
        onRoundInProgressChange?.Invoke(roundInProgress);
    }
    public event Action<bool> onRoundInProgressChange;

    public int timeLeft { get; private set; } = 30;
    [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
    public void SetTimeLeftRpc(int timeLeft, RpcParams rpcParams = new()) => this.timeLeft = timeLeft;

    readonly List<Player> players = new();
    private void Start()
    {
        var player = Instantiate(playerPrefab);
        player.transform.position = lobbySpawnPoint.position;
        player.NetworkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
        player.hpComp.SetRespawnPosRpc(lobbySpawnPoint.position);
    }
    public void AddPlayer(Player player) => players.Add(player);
    public void RemovePlayer(Player player) => players.Remove(player);

    float counter = 0.0f;
    private void Update()
    {
        if (!HasAuthority) return;
        counter += Time.deltaTime;
        if(counter >= 1.0f)
        {
            if (timeLeft > 0) SetTimeLeftRpc(timeLeft - 1);
            counter -= 1.0f;
        }
        if(timeLeft <= 0)
        {
            if (roundInProgress) EndRound();
            else StartRound();
        }
    }
    private void OnEnable()
    {
        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
    }
    private void OnDisable()
    {
        NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
    }
    void OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        if (!HasAuthority) return;
        if(data.EventType == ConnectionEvent.PeerConnected)
        {
            SetRoundInProgressRpc(roundInProgress, RpcTarget.Single(data.ClientId, RpcTargetUse.Temp));
            SetTimeLeftRpc(timeLeft, RpcTarget.Single(data.ClientId, RpcTargetUse.Temp));
        }
    }
    void StartRound()
    {
        if (players.Count < 2)
        {
            SetTimeLeftRpc(intermissionTime); return;
        }
        SetRoundInProgressRpc(true);
        SetTimeLeftRpc(roundTime);
        List<Player> playerCache = players.ToList();
        List<Player> teamRed = new();
        for(int i = 0; i < playerCache.Count / 2; i++)
        {
            Player player = playerCache[UnityEngine.Random.Range(0, playerCache.Count)];
            teamRed.Add(player);
            playerCache.Remove(player);
        }
        List<Player> teamBlue = playerCache;
        foreach (var i in teamRed)
        {
            i.SetTeamRpc(Team.Red);
            i.hpComp.SetRespawnPosRpc(teamRedSpawnpoint.position);
            i.TeleportRpc(teamRedSpawnpoint.position);
        }
        foreach (var i in teamBlue)
        {
            i.SetTeamRpc(Team.Blue);
            i.hpComp.SetRespawnPosRpc(teamBlueSpawnpoint.position);
            i.TeleportRpc(teamBlueSpawnpoint.position);
        }
        foreach (var i in players) i.SetPVPRpc(true);
    }
    public event Action onRoundStart, onRoundEnd;
    [Rpc(SendTo.ClientsAndHost)]
    public void RoundStartRpc() => onRoundStart?.Invoke();
    void EndRound()
    {
        SetRoundInProgressRpc(false);
        SetTimeLeftRpc(intermissionTime);
        foreach(var i in players)
        {
            if (i.team.Value != Team.None)
            {
                i.SetPVPRpc(false);
                i.SetTeamRpc(Team.None);
                i.hpComp.SetRespawnPosRpc(lobbySpawnPoint.position);
                i.TeleportRpc(lobbySpawnPoint.position);
            }
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void RoundEndRpc() => onRoundEnd?.Invoke();
}