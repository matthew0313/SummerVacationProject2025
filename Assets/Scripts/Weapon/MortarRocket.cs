using UnityEngine;
using System.Collections;
using Unity.Netcode.Components;
using Unity.Netcode;
using DG.Tweening;
using NetworkAudioManagement;

[RequireComponent(typeof(NetworkTransform))]
public class MortarRocket : NetworkBehaviour
{
    [SerializeField] PooledEffect explosion;
    [SerializeField] string explodeSound;
    [SerializeField] float explodeSoundDistMin = 20.0f, explodeSoundDistMax = 40.0f;
    public void Set(MortarTarget target, float flyTime)
    {
        transform.DOMove(target.transform.position, flyTime).SetEase(Ease.InCirc).OnComplete(() =>
        {
            target.BreakRpc();
            NetworkAudioManager.Instance.PlaySound(explodeSound).SetDistance(transform.position, explodeSoundDistMin, explodeSoundDistMax);
            ExplodeRpc();
            NetworkObject.Despawn();
        });
    }
    [Rpc(SendTo.Everyone)]
    void ExplodeRpc() => explosion.Get(transform.position, Quaternion.identity);
}