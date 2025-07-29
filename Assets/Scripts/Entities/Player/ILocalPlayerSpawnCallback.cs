using System.Collections;
using Unity.Netcode;
using UnityEngine;

public interface ILocalPlayerSpawnCallback
{
    public void OnLocalPlayerSpawn(Player localPlayer);
}