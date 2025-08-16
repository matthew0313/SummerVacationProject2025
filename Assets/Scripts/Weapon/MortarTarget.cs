using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public abstract class MortarTarget : NetworkBehaviour
{
    [SerializeField] Team m_team;
    public Team team => m_team;
    public readonly NetworkVariable<bool> broken = new(false);

    [Rpc(SendTo.Authority)]
    public void BreakRpc()
    {
        broken.Value = true;
        OnBreakRpc();
    }
    public event Action onBreak;
    [Rpc(SendTo.Everyone)] public void OnBreakRpc() => OnBreak();
    protected virtual void OnBreak() { onBreak?.Invoke(); }
}