using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player_HpComp : HpComp
{
    Player origin;
    public override Entity owner => origin;

    [SerializeField] float respawnTime = 3.0f;
    public Vector2 respawnPos = Vector2.zero;

    [Rpc(SendTo.Owner)]
    public void SetRespawnPosRpc(Vector2 respawnPos) => this.respawnPos = respawnPos;
    protected override void Awake()
    {
        base.Awake();
        origin = GetComponent<Player>();
    }
    private void Start()
    {
        if (HasAuthority)
        {
            GameManager.Instance.onRoundStart += ResetHp;
            GameManager.Instance.onRoundEnd += ResetHp;
        }
    }
    protected override DamageReceivedInfo ProcessDamage(DamageInfo damage)
    {
        var info = base.ProcessDamage(damage);
        if (!info.isNullified)
        {
            origin.animations.TriggerDamageRpc();
        }
        return info;
    }
    readonly int deadID = Animator.StringToHash("Dead");
    protected override void OnDeath()
    {
        base.OnDeath();
        origin.anim.SetBool(deadID, true);
        StartCoroutine(Respawning());
    }
    public event Action onRespawn;
    IEnumerator Respawning()
    {
        yield return new WaitForSeconds(respawnTime);
        isDead.Value = false;
        hp.Value = maxHp.Value;
        origin.anim.SetBool(deadID, false);
        origin.TeleportRpc(respawnPos);
        onRespawn?.Invoke();
    }
}