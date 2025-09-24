using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NetworkAudioManagement;
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
    [SerializeField] int roundEndTime = 5;
    [SerializeField] Transform teamRedSpawnpoint, teamBlueSpawnpoint;

    [Header("Map")]
    [SerializeField] TeamBase redBase;
    [SerializeField] TeamBase blueBase;

    public readonly NetworkVariable<GameState> gameState = new(GameState.Intermission);
    public readonly NetworkVariable<int> timeLeft = new(30);
    public readonly NetworkVariable<bool> timerPaused = new(false);

    public readonly NetworkVariable<ulong> cheatOwner = new(0);

    public readonly List<Player> players = new();

    [Rpc(SendTo.Authority)] public void ChangeCheatOwnerRpc(ulong cheatOwner, RpcParams rpcParams = new())
    {
        if (rpcParams.Receive.SenderClientId != this.cheatOwner.Value) return;
        this.cheatOwner.Value = cheatOwner;
    }
    private void Start()
    {
        var player = Instantiate(playerPrefab);
        player.transform.position = lobbySpawnPoint.position;
        player.NetworkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
        player.hpComp.SetRespawnPosRpc(lobbySpawnPoint.position);
    }
    public void AddPlayer(Player player)
    {
        if (HasAuthority)
        {
            if(players.Count == 0)
            {
                cheatOwner.Value = player.OwnerClientId;
            }
        }
        players.Add(player);
    }
    public void RemovePlayer(Player player)
    {
        players.Remove(player);
        if (HasAuthority)
        {
            if(cheatOwner.Value == player.OwnerClientId)
            {
                if (players.Count > 0) cheatOwner.Value = players[0].OwnerClientId;
            }
        }
    }
    public Player GetPlayer(ulong clientID)
    {
        foreach (var i in players) if (i.OwnerClientId == clientID) return i;
        return null;
    }
    public TeamBase GetBase(Team team)
    {
        if (team == Team.Red) return redBase;
        if (team == Team.Blue) return blueBase;
        return null;
    }

    float counter = 0.0f;
    private void Update()
    {
        if (!HasAuthority) return;
        if(timeLeft.Value > 0 && !timerPaused.Value)
        {
            counter += Time.deltaTime;
            if(counter >= 1.0f)
            {
                counter -= 1.0f;
                if(--timeLeft.Value <= 0)
                {
                    switch (gameState.Value)
                    {
                        case GameState.Intermission: StartRound(); break;
                        case GameState.RoundInProgress: WinRound(Team.None); break;
                        case GameState.RoundCompleted: EndRound();  break;
                    }
                }
            }
        }
    }
    void StartRound()
    {
        if (!HasAuthority) return;
        if (players.Count < 2)
        {
            timeLeft.Value = intermissionTime; return;
        }
        gameState.Value = GameState.RoundInProgress;
        timeLeft.Value = roundTime;
        NetworkAudioManager.Instance.ChangeMusic("InGame");
        List<Player> playerCache = players.ToList();
        playerCache.RemoveAll(item => item.spectator.Value);
        List<Player> teamRed = new(), teamBlue = new();
        int tmp = playerCache.Count;
        for(int i = 0; i < playerCache.Count;)
        {
            if (playerCache[i].team.Value == Team.Blue)
            {
                teamBlue.Add(playerCache[i]);
                playerCache.RemoveAt(i); continue;
            }
            if (playerCache[i].team.Value == Team.Red)
            {
                teamRed.Add(playerCache[i]);
                playerCache.RemoveAt(i); continue;
            }
            i++;
        }
        while(playerCache.Count > 0)
        {
            Player player = playerCache[UnityEngine.Random.Range(0, playerCache.Count)];
            if (teamRed.Count > teamBlue.Count) teamBlue.Add(player);
            else teamRed.Add(player);
            playerCache.Remove(player);
        }
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
    public event Action<RoundResult> onRoundComplete;

    [Rpc(SendTo.Everyone)]
    public void OnRoundStartRpc() => onRoundStart?.Invoke();
    public void WinRound(Team winningTeam)
    {
        if (!HasAuthority || gameState.Value != GameState.RoundInProgress) return;
        gameState.Value = GameState.RoundCompleted;
        timeLeft.Value = roundEndTime;
        foreach(var i in players)
        {
            if (i.inRound.Value)
            {
                var target = RpcTarget.Single(i.OwnerClientId, RpcTargetUse.Temp);
                if (winningTeam == Team.None)
                {
                    OnRoundCompleteRpc(RoundResult.Tied, target);
                }
                else
                {
                    if(i.team.Value == winningTeam) OnRoundCompleteRpc(RoundResult.Won, target);
                    else OnRoundCompleteRpc(RoundResult.Lost, target);
                }
                i.SetTeamRpc(Team.None);
            }
        }
    }
    [Rpc(SendTo.SpecifiedInParams)]
    public void OnRoundCompleteRpc(RoundResult result, RpcParams rpcParams) => onRoundComplete?.Invoke(result);
    public void EndRound()
    {
        if (!HasAuthority) return;
        gameState.Value = GameState.Intermission;
        timeLeft.Value = intermissionTime;
        NetworkAudioManager.Instance.ChangeMusic("Intermission");
        foreach(var i in players)
        {
            if (i.inRound.Value)
            {
                i.SetPVPRpc(false);
                i.SetInRoundRpc(false);
                i.hpComp.SetRespawnPosRpc(lobbySpawnPoint.position);
                i.TeleportRpc(lobbySpawnPoint.position);
            }
        }
        OnRoundEndRpc();
    }
    [Rpc(SendTo.Everyone)]
    public void OnRoundEndRpc() => onRoundEnd?.Invoke();

}
[System.Serializable]
public enum GameState
{
    Intermission,
    RoundInProgress,
    RoundCompleted
}
[System.Serializable]
public enum RoundResult
{
    Tied = 0,
    Won = 1,
    Lost = 2
}