using System;
using Unity.Netcode;
using UnityEngine;

public abstract class HpComp : NetworkBehaviour, IDamagable
{
    public abstract Entity entity { get; }

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

    public void GetDamage(float damage) => GetDamageRpc(damage);

    [Rpc(SendTo.Authority)]
    public void GetDamageRpc(float damage) => ProcessDamage(damage);
    protected virtual void ProcessDamage(float damage)
    {
        if (isDead.Value) return;
        hp.Value = Mathf.Max(hp.Value - damage, 0);
        onDamage?.Invoke(damage);
        if (hp.Value <= 0.0f)
        {
            isDead.Value = true;
            onDeath?.Invoke();
            OnDeath();
        }
    }
    protected virtual void OnDeath() { }
}