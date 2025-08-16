using NetworkAudioManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public abstract class Gun : Weapon
{
    public Sprite bulletSprite;

    public float damage, shootCooldown, bulletSpeed, bulletRange;
    public Bullet bulletPrefab;
    public Transform firePoint;
    public string fireSound;
    public float fireSoundMinDist = 20.0f, fireSoundMaxDist = 40.0f;

    private NetworkAnimator animator;
    private float lastShootTime = -Mathf.Infinity;
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
        NetworkAudioManager.Instance.PlaySound(fireSound)
            .SetDistance(firePoint.position, fireSoundMinDist, fireSoundMaxDist);
        animator.SetTrigger("Shoot");
        Bullet bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.NetworkObject.Spawn();
        bullet.Set(bulletSpeed, bulletRange, damage * owner.attackMultiplier);
    }
}
