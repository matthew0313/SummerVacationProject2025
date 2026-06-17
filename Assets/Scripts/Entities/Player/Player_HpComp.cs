using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player_HpComp : HpComp
{
    Player origin;
    public override Entity owner => origin;

    [SerializeField] float respawnTime = 3.0f, regenTime = 15.0f, regenTick = 1.0f, regenAmount = 1.0f;
    public Vector2 respawnPos = Vector2.zero;

    [Rpc(SendTo.Owner)]
    public void SetRespawnPosRpc(Vector2 respawnPos) => this.respawnPos = respawnPos;
    protected override void Awake()
    {
        base.Awake();
        origin = GetComponent<Player>();
    }
    float regenCounter = 0.0f, regenTickCounter = 0.0f;
    private void Update()
    {
        if (!HasAuthority) return;
        if (regenCounter > 0.0f)
        {
            regenCounter -= Time.deltaTime;
            regenTickCounter = 0.0f;
        }
        else
        {
            if (regenTickCounter < regenTick) regenTickCounter += Time.deltaTime;
            else
            {
                regenTickCounter -= regenTick;
                Heal(1.0f);
            }
        }
    }
    private void Start()
    {
        if (HasAuthority)
        {
            GameManager.Instance.onRoundStart += () => ResetHp();
            GameManager.Instance.onRoundEnd += () => ResetHp();
        }
    }
    public void Heal(float amount)
    {
        if (!HasAuthority || isDead.Value) return;
        hp.Value = Mathf.Min(maxHp.Value, hp.Value + amount);
    }
    protected override DamageReceivedInfo ProcessDamage(DamageInfo damage)
    {
        var info = base.ProcessDamage(damage);
        if (!info.isNullified)
        {
            origin.animations.TriggerDamageRpc();
            regenCounter = regenTime;
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