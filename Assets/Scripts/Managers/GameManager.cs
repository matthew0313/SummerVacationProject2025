using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private readonly Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // 플레이어 스폰 로직은 여기서 처리하거나, 플레이어가 직접 스폰을 요청하도록 할 수 있습니다.
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (players.ContainsKey(clientId))
        {
            // 플레이어 오브젝트 파괴 등 정리 로직
            Destroy(players[clientId].gameObject);
            players.Remove(clientId);
        }
    }

    public void RegisterPlayer(ulong clientId, Player player)
    {
        if (!players.ContainsKey(clientId))
        {
            players.Add(clientId, player);
        }
        else
        {
            players[clientId] = player;
        }
    }

    public void UnregisterPlayer(ulong clientId)
    {
        if (players.ContainsKey(clientId))
        {
            players.Remove(clientId);
        }
    }

    public Player GetPlayer(ulong clientId)
    {
        return players.GetValueOrDefault(clientId);
    }

    public IEnumerable<Player> GetAllPlayers() => players.Values;

    public IEnumerable<Player> GetTeamPlayers(Team team)
    {
        return players.Values.Where(p => p.team.Value == team);
    }
}