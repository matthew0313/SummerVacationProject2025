using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Spear : Weapon
{
    private List<GameObject> hits = new List<GameObject>();
    private Animator animator;

    public float shootCooldown = 1f;
    private float lastShootTime = -Mathf.Infinity;

    void Start()
    {
        Debug.Log("창");

        animator = GetComponent<Animator>();
    }

    public override void Shoot()
    {
        if (Time.time < lastShootTime + shootCooldown)
            return;
        lastShootTime = Time.time;

        animator.SetTrigger("Shoot");
        hits.Clear();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        GameObject hitObject = other.attachedRigidbody.gameObject;

        if (hitObject.TryGetComponent(out IDamagable enemy))
        {
            enemy.GetDamage(5.0f);
        }
    }
}
