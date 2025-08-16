using UnityEngine;
using System.Collections;
using Unity.Netcode.Components;
using Unity.Netcode;
using DG.Tweening;

[RequireComponent(typeof(NetworkTransform))]
public class MortarRocket : NetworkBehaviour
{

    public void Set(MortarTarget target, float flyTime)
    {
        transform.DOMove(target.transform.position, flyTime).SetEase(Ease.InCirc).OnComplete(() =>
        {
            target.BreakRpc();
            NetworkObject.Despawn();
        });
    }
}