using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Ward : Entity, IDamagable
{
    public Entity entity => this;

    [SerializeField] Animator anim;
    [SerializeField] UnitTime disappearTime;
    public readonly NetworkVariable<int> wardHealth = new(3);
    public Player owner { get; private set; }

    readonly int relationID = Animator.StringToHash("Relation");
    [Rpc(SendTo.ClientsAndHost)]
    public void SetOwnerRpc(NetworkBehaviourReference owner)
    {
        if(owner.TryGet(out Player player))
        {
            this.owner = player;
            anim.SetInteger(relationID, (int)Player.localPlayer.GetRelation(player));
        }
    }

    public void GetDamage(float damage)
    {
        GetDamageRpc();
    }

    bool dead = false;
    readonly int deadID = Animator.StringToHash("Dead");
    [Rpc(SendTo.Owner)]
    public void GetDamageRpc()
    {
        if (dead) return;
        wardHealth.Value = Mathf.Max(wardHealth.Value - 1, 0);
        if(wardHealth.Value <= 0)
        {
            dead = true;
            anim.SetBool(deadID, true);
            StartCoroutine(Despawning());
        }
    }
    IEnumerator Despawning()
    {
        yield return new WaitForSeconds(disappearTime);
        NetworkObject.Despawn();
    }

    public enum WardType
    {
        Vision,
        Damage
    }
} 