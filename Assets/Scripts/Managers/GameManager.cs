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

    public readonly NetworkVariable<bool> roundInProgress = new(false);
    public readonly NetworkVariable<int> timeLeft = new(30);

    public readonly List<Player> players = new();
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
        if(timeLeft.Value > 0)
        {
            counter += Time.deltaTime;
            if(counter >= 1.0f)
            {
                counter -= 1.0f;
                if(--timeLeft.Value <= 0)
                {
                    if (roundInProgress.Value) EndRound();
                    else StartRound();
                }
            }
        }
    }
    void StartRound()
    {
        if (players.Count < 2)
        {
            timeLeft.Value = intermissionTime; return;
        }
        roundInProgress.Value = true;
        timeLeft.Value = roundTime;
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
        foreach (var i in players)
        {
            i.SetPVPRpc(true);
            i.SetInRoundRpc(true);
        }
        OnRoundStartRpc();
    }
    public event Action onRoundStart, onRoundEnd;
    [Rpc(SendTo.ClientsAndHost)]
    public void OnRoundStartRpc() => onRoundStart?.Invoke();
    void EndRound()
    {
        roundInProgress.Value = false;
        timeLeft.Value = intermissionTime;
        foreach(var i in players)
        {
            if (i.inRound.Value)
            {
                i.SetPVPRpc(false);
                i.SetInRoundRpc(false);
                i.SetTeamRpc(Team.None);
                i.hpComp.SetRespawnPosRpc(lobbySpawnPoint.position);
                i.TeleportRpc(lobbySpawnPoint.position);
            }
        }
        OnRoundEndRpc();
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void OnRoundEndRpc() => onRoundEnd?.Invoke();
}