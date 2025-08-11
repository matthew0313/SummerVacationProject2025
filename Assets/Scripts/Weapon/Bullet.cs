using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    float speed, range, damage;
    public void Set(float speed, float range, float damage)
    {
        this.speed = speed;
        this.range = range;
        this.damage = damage;
    }

    float counter = 0.0f;
    void Update()
    {
        if (!IsOwner) return;
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        counter += speed * Time.deltaTime;
        if (counter > range) NetworkObject.Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other) {

        if (!IsOwner) return;
        GameObject hitObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

        if (hitObject.TryGetComponent(out IDamagable damagable) && Player.localPlayer.GetRelation(damagable.owner) == Relation.Enemy)
        {
            damagable.GetDamage(damage);
        }
        NetworkObject.Despawn();
    }
}

