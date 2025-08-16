using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode.Components;
using System.Net.NetworkInformation;
using NetworkAudioManagement;

public class Mortar : Weapon
{
    [SerializeField] Transform firePoint;
    [SerializeField] MortarMarker markerPrefab;
    [SerializeField] MortarRocket rocket;
    [SerializeField] float shootTime = 2.0f, shootEndlag = 1.0f;
    [SerializeField] float shootSpeed = 0.3f;
    [SerializeField] string fireSound;
    [SerializeField] float fireSoundMinDist = 20.0f, fireSoundMaxDist = 40.0f;

    private NetworkAnimator animator;
    readonly List<MortarTarget> targets = new();
    MortarTarget currentTarget;
    MortarMarker marker;

    private bool isShooting = false;
    private Coroutine ShootCoroutine;
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<NetworkAnimator>();
    }
    void ChangeTarget(MortarTarget target)
    {
        if (currentTarget == target) return;
        if(target == null)
        {
            marker.Hide();
        }
        else
        {
            marker.transform.position = targets[0].transform.position;
            marker.Show();
        }
    }
    protected override void Update()
    {
        base.Update();
        targets.RemoveAll(item => item.broken.Value);
        if (!isShooting)
        {
            if (targets.Count > 0)
            {
                targets.Sort((a, b) => Vector2.Distance(transform.position, a.transform.position).CompareTo(Vector2.Distance(transform.position, b.transform.position)));
                ChangeTarget(targets[0]);
            }
            else ChangeTarget(null);
        }
    }

    public override void Shoot()
    {
        if (currentTarget == null || isShooting)
            return;

        ShootCoroutine = StartCoroutine(ShootAfterDelay());
    }

    private IEnumerator ShootAfterDelay()
    {
        owner.inventory.mortarUnlocked = false;
        owner.speedMultiplier *= shootSpeed;
        isShooting = true;
        animator.Animator.SetBool("Shooting", true);
        yield return new WaitForSeconds(shootTime);
        animator.SetTrigger("Shoot");
        animator.Animator.SetBool("Shooting", false);
        owner.inventory.mortarUnlocked = false;
        NetworkAudioManager.Instance.PlaySound(fireSound)
            .SetDistance(firePoint.position, fireSoundMinDist, fireSoundMaxDist);

        Vector2 firePos = firePoint.position, targetPos = currentTarget.transform.position;
        MortarRocket rocket = Instantiate(this.rocket, firePos, Quaternion.Euler(0, 0, Mathf.Atan2(targetPos.y - firePos.y, targetPos.x - firePos.x) * Mathf.Rad2Deg));
        rocket.NetworkObject.Spawn(true);
        rocket.Set(currentTarget, 2.0f);

        yield return new WaitForSeconds(shootEndlag);
        StopShooting();
    }
    void StopShooting()
    {
        if (!isShooting) return;
        StopCoroutine(ShootCoroutine);
        isShooting = false;
        owner.speedMultiplier /= shootSpeed;
        animator.Animator.SetBool("Shooting", false);
        owner.inventory.ChangeSlot(0);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

        if (hitObject.TryGetComponent(out MortarTarget target) && !target.broken.Value && target.team != owner.team.Value)
        {
            targets.Add(target);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject hitObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

        if (hitObject.TryGetComponent(out MortarTarget target) && targets.Contains(target))
        {
            targets.Remove(target);
        }
    }

    private void OnDamageTake(float damage)
    {
        if (isShooting) StopShooting();
    }
    public override void OnEquipped()
    {
        base.OnEquipped();
        if (marker == null) marker = Instantiate(markerPrefab);
        owner.hpComp.onDamage += OnDamageTake;
    }

    public override void OnUnequipped()
    {
        if (isShooting) StopShooting();
        marker.Hide();
        owner.hpComp.onDamage -= OnDamageTake;
        base.OnUnequipped();
    }
}
