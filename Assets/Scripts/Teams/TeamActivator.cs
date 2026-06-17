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
        localPlayer.team.OnValueChanged += Check;
        Check(Team.None, localPlayer.team.Value);
    }
    void Check(Team prev, Team val)
    {
        gameObject.SetActive(val == team);
    }
}