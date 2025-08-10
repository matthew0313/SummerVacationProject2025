using UnityEngine;
using System.Collections;

public class Mortar : Weapon
{
    private Animator animator;
    public Test_base TargetBase;

    public float shootCooldown = 60.0f;
    private float lastShootTime = -Mathf.Infinity;

    private bool isShooting = false;
    private Coroutine ShootCoroutine;
    void Start()
    {
        Debug.Log("박격포");

        animator = GetComponent<Animator>();
    }

    public override void Shoot()
    {
        if (Time.time < lastShootTime + shootCooldown || TargetBase == null || isShooting)
            return;

        ShootCoroutine = StartCoroutine(ShootAfterDelay(3f));
    }

    private IEnumerator ShootAfterDelay(float delay)
    {
        isShooting = true;
        yield return new WaitForSeconds(delay);
        animator.SetTrigger("Shoot");
        StartCoroutine(ExplosionAfterDelay(2f));
    }
    private IEnumerator ExplosionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("펑");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitObject = other.attachedRigidbody.gameObject;

        if (hitObject.TryGetComponent(out Test_base Testbase))
        {
            TargetBase = Testbase;
            Debug.Log("감지");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject hitObject = other.attachedRigidbody.gameObject;

        if (hitObject.TryGetComponent(out Test_base Testbase) && Testbase == TargetBase)
        {
            TargetBase = null;
        }
    }

    //private void OnDamageTake(float damage)
    //{
    //    if (isShooting)
    //    {
    //        StopCoroutine(ShootCoroutine);
    //        isShooting = false;
    //    }
    //}

    //public override void OnEquipped()
    //{
    //    base.OnEquipped();
    //    owner.hpComp.onDamage += OnDamageTake;
    //}

    //public override void OnUnequipped()
    //{
    //    base.OnUnequipped();
    //    owner.hpComp.onDamage -= OnDamageTake;
    //}
}
