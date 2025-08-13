using System.Collections.Generic;
using NetworkAudioManagement;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class Spear : Weapon
{
    public float damage, shootCooldown = 1.0f;
    public string slashSound;
    public float slashSoundDistMin = 8.0f, slashSoundDistMax = 15.0f;

    private List<GameObject> hits = new List<GameObject>();
    private NetworkAnimator animator;
    private float lastShootTime = -Mathf.Infinity;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<NetworkAnimator>();
    }
    public override void Shoot()
    {
        if (Time.time < lastShootTime + shootCooldown)
            return;
        lastShootTime = Time.time;

        NetworkAudioManager.Instance.PlaySound(slashSound)
            .SetDistance(transform.position, slashSoundDistMin, slashSoundDistMax);
        animator.SetTrigger("Shoot");
        hits.Clear();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsOwner) return;
        GameObject hitObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        if (hits.Contains(hitObject)) return;
        else hits.Add(hitObject);

        if (hitObject.TryGetComponent(out IDamagable damagable))
        {
            if (owner.GetRelation(damagable.owner) == Relation.Enemy || damagable is Ward ward && ward.owner == owner)
            {
                damagable.GetDamage(new() { amount = damage, damageGiverClientID = OwnerClientId });
            }
        }
    }
}
