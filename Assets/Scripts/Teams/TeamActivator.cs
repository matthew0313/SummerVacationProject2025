using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeamActivator : MonoBehaviour, ILocalPlayerSpawnCallback
{
    [SerializeField] Team team;
    public void OnLocalPlayerSpawn(Player localPlayer)
    {
        localPlayer.team.OnValueChanged += (prev, val) => Check();
        localPlayer.inRound.OnValueChanged += (prev, val) => Check();
        Check();
    }
    void Check()
    {
        gameObject.SetActive(Player.localPlayer.inRound.Value && Player.localPlayer.team.Value == team);
    }
}