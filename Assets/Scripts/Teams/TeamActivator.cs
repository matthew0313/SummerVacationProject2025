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
        localPlayer.team.OnValueChanged += OnTeamChange;
        OnTeamChange(Team.None, localPlayer.team.Value);
        Debug.Log("RRRRR");
    }
    void OnTeamChange(Team prev, Team team)
    {
        gameObject.SetActive(team == this.team);
    }
}