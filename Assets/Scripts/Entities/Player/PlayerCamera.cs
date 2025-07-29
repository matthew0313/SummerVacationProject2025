using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class PlayerCamera : MonoBehaviour, ILocalPlayerSpawnCallback
{
    CinemachineCamera cineCamera;
    private void Awake()
    {
        cineCamera = GetComponent<CinemachineCamera>();
    }
    public void OnLocalPlayerSpawn(Player localPlayer)
    {
        cineCamera.Target.TrackingTarget = localPlayer.transform;
    }
}