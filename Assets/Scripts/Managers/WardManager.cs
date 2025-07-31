using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/*
public class WardManager : NetworkBehaviour
{
    [Header("Ward Management")]
    [SerializeField] private GameObject wardPrefab;
    [SerializeField] private int maxWardsPerPlayer = 3;
    [SerializeField] private float wardCooldown = 30f;
    
    [Header("Vision System")]
    [SerializeField] private LayerMask visionLayerMask = -1;
    [SerializeField] private float globalVisionRadius = 5f;
    [SerializeField] private bool enableFogOfWar = true;
    
    private Dictionary<ulong, List<Ward>> playerWards = new Dictionary<ulong, List<Ward>>();
    private Dictionary<ulong, float> playerWardCooldowns = new Dictionary<ulong, float>();
    private List<Ward> allWards = new List<Ward>();
    
    public static WardManager Instance { get; private set; }
    
    public event System.Action<Ward> OnWardPlaced;
    public event System.Action<Ward> OnWardDestroyed;
    public event System.Action<ulong> OnWardCooldownChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        if (!playerWards.ContainsKey(clientId))
        {
            playerWards[clientId] = new List<Ward>();
            playerWardCooldowns[clientId] = 0f;
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        if (playerWards.ContainsKey(clientId))
        {
            foreach (var ward in playerWards[clientId])
            {
                if (ward != null && ward.NetworkObject != null)
                {
                    ward.NetworkObject.Despawn();
                }
            }
            playerWards[clientId].Clear();
            playerWards.Remove(clientId);
            playerWardCooldowns.Remove(clientId);
        }
    }
    
    private void Update()
    {
        if (IsServer)
        {
            UpdateWardCooldowns();
        }
    }
    
    private void UpdateWardCooldowns()
    {
        var clientIds = new List<ulong>(playerWardCooldowns.Keys);
        foreach (var clientId in clientIds)
        {
            if (playerWardCooldowns[clientId] > 0)
            {
                playerWardCooldowns[clientId] -= Time.deltaTime;
                OnWardCooldownChanged?.Invoke(clientId);
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void PlaceWardServerRpc(Vector3 position, Ward.WardType wardType, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        
        if (!CanPlaceWard(clientId))
        {
            return;
        }
        
        if (!IsValidWardPosition(position))
        {
            return;
        }
        
        GameObject wardObject = Instantiate(wardPrefab, position, Quaternion.identity);
        Ward ward = wardObject.GetComponent<Ward>();
        
        if (ward != null)
        {
            ward.SetWardType(wardType);
            wardObject.GetComponent<NetworkObject>().Spawn();
            
            if (!playerWards.ContainsKey(clientId))
            {
                playerWards[clientId] = new List<Ward>();
            }
            
            playerWards[clientId].Add(ward);
            allWards.Add(ward);
            
            playerWardCooldowns[clientId] = wardCooldown;
            
            var wardComponent = ward.GetComponent<WardComponent>();
            if (wardComponent != null)
            {
                wardComponent.onWardDestroyed += (w) => HandleWardDestroyed(ward, clientId);
            }
            
            OnWardPlaced?.Invoke(ward);
            OnWardCooldownChanged?.Invoke(clientId);
        }
    }
    
    private void HandleWardDestroyed(Ward ward, ulong clientId)
    {
        if (playerWards.ContainsKey(clientId))
        {
            playerWards[clientId].Remove(ward);
        }
        
        allWards.Remove(ward);
        OnWardDestroyed?.Invoke(ward);
    }
    
    public bool CanPlaceWard(ulong clientId)
    {
        if (playerWardCooldowns.ContainsKey(clientId) && playerWardCooldowns[clientId] > 0)
        {
            return false;
        }
        
        if (playerWards.ContainsKey(clientId) && playerWards[clientId].Count >= maxWardsPerPlayer)
        {
            return false;
        }
        
        return true;
    }
    
    private bool IsValidWardPosition(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, 0.1f, visionLayerMask);
        if (hit.collider == null)
        {
            return false;
        }
        
        foreach (var ward in allWards)
        {
            if (ward != null && Vector3.Distance(ward.transform.position, position) < 2f)
            {
                return false;
            }
        }
        
        return true;
    }
    
    public float GetWardCooldown(ulong clientId)
    {
        if (playerWardCooldowns.ContainsKey(clientId))
        {
            return playerWardCooldowns[clientId];
        }
        return 0f;
    }
    
    public int GetWardCount(ulong clientId)
    {
        if (playerWards.ContainsKey(clientId))
        {
            return playerWards[clientId].Count;
        }
        return 0;
    }
    
    public List<Ward> GetPlayerWards(ulong clientId)
    {
        if (playerWards.ContainsKey(clientId))
        {
            return new List<Ward>(playerWards[clientId]);
        }
        return new List<Ward>();
    }
    
    public List<Ward> GetAllWards()
    {
        return new List<Ward>(allWards);
    }
    
    public bool IsPositionVisible(Vector3 position, ulong clientId)
    {
        if (!enableFogOfWar)
        {
            return true;
        }
        
        if (Vector3.Distance(position, Vector3.zero) <= globalVisionRadius)
        {
            return true;
        }
        
        if (playerWards.ContainsKey(clientId))
        {
            foreach (var ward in playerWards[clientId])
            {
                if (ward != null && ward.GetComponent<WardComponent>()?.IsInWardRange(position) == true)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    public void DestroyAllWards(ulong clientId)
    {
        if (playerWards.ContainsKey(clientId))
        {
            foreach (var ward in playerWards[clientId])
            {
                if (ward != null && ward.NetworkObject != null)
                {
                    ward.NetworkObject.Despawn();
                }
            }
            playerWards[clientId].Clear();
        }
    }
    
    public void SetMaxWardsPerPlayer(int maxWards)
    {
        maxWardsPerPlayer = maxWards;
    }
    
    public void SetWardCooldown(float cooldown)
    {
        wardCooldown = cooldown;
    }
    
    public void SetFogOfWar(bool enabled)
    {
        enableFogOfWar = enabled;
    }
    
    private void OnDrawGizmos()
    {
        if (enableFogOfWar)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Vector3.zero, globalVisionRadius);
        }
        
        Gizmos.color = Color.cyan;
        foreach (var ward in allWards)
        {
            if (ward != null && ward.GetComponent<WardComponent>()?.IsActive == true)
            {
                Gizmos.DrawWireSphere(ward.transform.position, ward.GetComponent<WardComponent>().WardRadius);
            }
        }
    }
} */