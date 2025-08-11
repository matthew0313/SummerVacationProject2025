using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Ward : Entity, IDamagable
{
    public Player owner { get; private set; }
    Entity IRelationAffected.owner => owner;

    [SerializeField] Animator anim;
    [SerializeField] UnitTime disappearTime;
    public readonly NetworkVariable<int> wardHealth = new(3);

    readonly int relationID = Animator.StringToHash("Relation");
    [Rpc(SendTo.ClientsAndHost)]
    public void SetOwnerRpc(NetworkBehaviourReference owner)
    {
        if(owner.TryGet(out Player player))
        {
            Debug.Log(Player.localPlayer.GetRelation(player));
            this.owner = player;
            anim.SetInteger(relationID, (int)Player.localPlayer.GetRelation(player));
        }
    }

    public void GetDamage(float damage)
    {
        GetDamageRpc();
    }

    bool dead = false;
    [Rpc(SendTo.Owner)]
    public void GetDamageRpc()
    {
        if (dead) return;
        wardHealth.Value = Mathf.Max(wardHealth.Value - 1, 0);
        if(wardHealth.Value <= 0)
        {
            dead = true;
            SetDeadRpc();
            StartCoroutine(Despawning());
        }
    }
    readonly int deadID = Animator.StringToHash("Dead");
    [Rpc(SendTo.Everyone)]
    public void SetDeadRpc() => anim.SetBool(deadID, true);
    readonly int damageID = Animator.StringToHash("Damage");
    [Rpc(SendTo.Everyone)]
    public void TriggerDamageRpc() => anim.SetTrigger(damageID);
    IEnumerator Despawning()
    {
        yield return new WaitForSeconds(disappearTime);
        NetworkObject.Despawn();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        owner.wardComp.RemoveWard(this);
    }
    public enum WardType
    {
        Vision,
        Damage
    }
} 