using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InRoundActivator : MonoBehaviour, ILocalPlayerSpawnCallback
{
    [SerializeField] bool inRound;
    public void OnLocalPlayerSpawn(Player localPlayer)
    {
        localPlayer.inRound.OnValueChanged += Check;
        Check(false, localPlayer.inRound.Value);
    }
    void Check(bool prev, bool val)
    {
        gameObject.SetActive(val == inRound);
    }
}