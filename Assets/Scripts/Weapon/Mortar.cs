using UnityEngine;
using System.Collections;
using Unity.Netcode.Components;

public class Mortar : Weapon, Weapon.ICooldownDisplayed
{
    [SerializeField] float shootCooldown = 30.0f;
    [SerializeField] Transform firePoint;
    [SerializeField] MortarRocket rocket;

    private NetworkAnimator animator;
    private TeamBase targetBase;
    private float lastShootTime = -Mathf.Infinity;

    public float cooldownFill => 1.0f - Mathf.Max((Time.time - lastShootTime) / shootCooldown, 1.0f);

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
        float counter = 0.0f;
        while(counter < delay)
        {
            yield return null;
            counter += Time.deltaTime;
            if (targetBase == null)
            {
                isShooting = true;
                animator.Animator.SetBool("Shooting", false);
                yield break;
            }
        }
        animator.SetTrigger("Shoot");
        isShooting = false;
        animator.Animator.SetBool("Shooting", false);

        Vector2 firePos = firePoint.position, targetPos = targetBase.transform.position;
        MortarRocket rocket = Instantiate(this.rocket, firePos, Quaternion.Euler(0, 0, Mathf.Atan2(targetPos.y - firePos.y, targetPos.x - firePos.x) * Mathf.Rad2Deg));
        rocket.NetworkObject.Spawn(true);
        rocket.Set(targetBase, 2.0f);
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
