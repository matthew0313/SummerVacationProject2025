using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player_Animations : NetworkBehaviour
{
    Player origin;
    private void Awake()
    {
        origin = GetComponent<Player>();
    }
    public readonly NetworkVariable<bool> isWalking = new(false);
    public readonly NetworkVariable<bool> isDead = new(false);
    private void Start()
    {
        isWalking.OnValueChanged += SetIsWalking;
        isDead.OnValueChanged += SetIsDead;
    }
    readonly int isWalkingID = Animator.StringToHash("IsWalking"), isDeadID = Animator.StringToHash("IsDead");
    void SetIsWalking(bool prev, bool val) => origin.anim.SetBool(isWalkingID, val);
    void SetIsDead(bool prev, bool val) => origin.anim.SetBool(isDeadID, val);

    readonly int damageID = Animator.StringToHash("Damage");
    [Rpc(SendTo.Everyone)] public void TriggerDamageRpc() => origin.anim.SetTrigger(damageID);
}