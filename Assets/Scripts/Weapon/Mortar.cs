using UnityEngine;
using System.Collections;
using Unity.Netcode.Components;

public class Mortar : Weapon
{
    [SerializeField] float shootCooldown = 60.0f;

    private NetworkAnimator animator;
    public TeamBase targetBase;
    private float lastShootTime = -Mathf.Infinity;

    private bool isShooting = false;
    private Coroutine ShootCoroutine;
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<NetworkAnimator>();
    }

    public override void Shoot()
    {
        if (Time.time < lastShootTime + shootCooldown || targetBase == null || isShooting)
            return;

        ShootCoroutine = StartCoroutine(ShootAfterDelay(3f));
    }

    private IEnumerator ShootAfterDelay(float delay)
    {
        isShooting = true;
        animator.Animator.SetBool("Shooting", true);
        yield return new WaitForSeconds(delay);
        animator.SetTrigger("Shoot");
        StartCoroutine(ExplosionAfterDelay(2f));
    }
    private IEnumerator ExplosionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitObject = other.attachedRigidbody.gameObject;

        if (hitObject.TryGetComponent(out TeamBase teamBase) && teamBase.team != owner.team.Value)
        {
            targetBase = teamBase;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject hitObject = other.attachedRigidbody.gameObject;

        if (hitObject.TryGetComponent(out TeamBase teambase) && teambase == targetBase)
        {
            targetBase = null;
        }
    }

    private void OnDamageTake(float damage)
    {
        if (isShooting)
        {
            StopCoroutine(ShootCoroutine);
            isShooting = false;
            animator.Animator.SetBool("Shooting", false);
        }
    }
    public override void OnEquipped()
    {
        base.OnEquipped();
        owner.hpComp.onDamage += OnDamageTake;
    }

    public override void OnUnequipped()
    {
        base.OnUnequipped();
        owner.hpComp.onDamage -= OnDamageTake;
        if (isShooting)
        {
            StopCoroutine(ShootCoroutine);
            isShooting = false;
        }
    }
}
