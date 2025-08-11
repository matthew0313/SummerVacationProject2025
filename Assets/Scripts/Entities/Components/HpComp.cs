using System;
using Unity.Netcode;
using UnityEngine;

public abstract class HpComp : NetworkBehaviour, IDamagable
{
    public abstract Entity owner { get; }

    [SerializeField] float defaultHp = 100.0f;

    public readonly NetworkVariable<float> maxHp = new(100.0f), hp = new(100.0f);
    public readonly NetworkVariable<bool> isDead = new(false);

    public event Action onHpChange, onDeath;
    public event Action<float> onDamage;

    protected void ResetHp()
    {
        if (!HasAuthority) return;
        maxHp.Value = defaultHp;
        hp.Value = defaultHp;
    }
    protected virtual void Awake()
    {
        if (HasAuthority) ResetHp();
        maxHp.OnValueChanged += (prev, val) => onHpChange?.Invoke();
        hp.OnValueChanged += (prev, val) => onHpChange?.Invoke();
    }

    public void GetDamage(DamageInfo damage) => GetDamageRpc(damage);

    [Rpc(SendTo.Authority)]
    public void GetDamageRpc(DamageInfo damage)
    {
        ProcessDamage(damage);
    }
    protected virtual DamageReceivedInfo ProcessDamage(DamageInfo damage)
    {
        DamageReceivedInfo info = new() { damageInfo = damage };
        if (isDead.Value)
        {
            info.isNullified = true;
            return info;
        }
        hp.Value = Mathf.Max(hp.Value - damage.amount, 0);
        onDamage?.Invoke(damage.amount);
        if (hp.Value <= 0.0f)
        {
            isDead.Value = true;
            onDeath?.Invoke();
            OnDeath();
        }
        return info;
    }
    protected virtual void OnDeath() { }
}
[System.Serializable]
public struct DamageReceivedInfo : INetworkSerializable
{
    public DamageInfo damageInfo;
    public bool isNullified;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeNetworkSerializable(ref damageInfo);
        serializer.SerializeValue(ref isNullified);
    }
}