using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameManager() => Instance = this;

    [Header("Player")]
    [SerializeField] Player playerPrefab;
    [SerializeField] Transform spawnPoint;
    readonly List<Player> players = new();
    private void Start()
    {
        var player = Instantiate(playerPrefab);
        player.transform.position = spawnPoint.position;
        player.NetworkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
    }
    public void AddPlayer(Player player) => players.Add(player);
    public void RemovePlayer(Player player) => players.Remove(player);
    private void Update()
    {
        if (!IsOwner) return;
    }
}