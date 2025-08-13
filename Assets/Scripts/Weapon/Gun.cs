using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public abstract class Gun : Weapon, Weapon.ICooldownDisplayed
{
    public Sprite bulletSprite;

    public float damage, shootCooldown, bulletSpeed, bulletRange;
    public Bullet bulletPrefab;
    public Transform firePoint;

    private NetworkAnimator animator;
    private float lastShootTime = -Mathf.Infinity;

    public float cooldownFill => 1.0f - Mathf.Max((Time.time - lastShootTime) / shootCooldown, 1.0f);
    public abstract int bullets { get; set; }
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<NetworkAnimator>();
    }

    public override void Shoot()
    {
        if (Time.time < lastShootTime + shootCooldown || bullets <= 0)
            return;
        lastShootTime = Time.time;

        bullets--;
        animator.SetTrigger("Shoot");
        Bullet bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.NetworkObject.Spawn();
        bullet.Set(bulletSpeed, bulletRange, damage);
    }
}
