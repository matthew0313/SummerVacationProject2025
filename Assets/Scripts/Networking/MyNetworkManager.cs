using Cysharp.Threading.Tasks;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkManager))]
public class MyNetworkManager : MonoBehaviour
{
    public static MyNetworkManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Instance = Instantiate(Resources.Load<MyNetworkManager>("MyNetworkManager"));
        DontDestroyOnLoad(Instance.gameObject);
    }
    
    NetworkManager networkManager;
    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
    }
    private void OnEnable()
    {
        networkManager.OnConnectionEvent += OnConnectionEvent;
    }
    private void OnDisable()
    {
        networkManager.OnConnectionEvent -= OnConnectionEvent;
    }
    void OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        if(data.EventType == ConnectionEvent.ClientConnected && data.ClientId == manager.LocalClientId)
        {
            if(manager.LocalClient.IsSessionOwner) networkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }
}
