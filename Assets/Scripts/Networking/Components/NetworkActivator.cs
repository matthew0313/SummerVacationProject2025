using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkActivator : NetworkBehaviour
{
    [SerializeField] NetworkActivatorMode mode;
    private void Start()
    {
        switch (mode)
        {
            case NetworkActivatorMode.Owner: gameObject.SetActive(IsOwner); break;
            case NetworkActivatorMode.NotOwner: gameObject.SetActive(!IsOwner); break;
        }
    }
}
[System.Serializable]
public enum NetworkActivatorMode
{
    Owner,
    NotOwner
}