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
            marker.transform.position = target.transform.position;
            marker.Show();
        }
        currentTarget = target;
    }
    protected override void Update()
    {
        base.Update();
        if (!IsOwner) return;
        if (!isShooting)
        {
            if (targets.Count > 0)
            {
                targets.Sort((a, b) =>
                {
                    if (a.broken.Value)
                    {
                        if (b.broken.Value) return 0;
                        else return 1;
                    }
                    if (b.broken.Value) return -1;

                    Vector2 dir = (a.transform.position - firePoint.position).normalized;
                    float dist = Vector2.Distance(a.transform.position, firePoint.position);
                    bool aBlocked = Physics2D.Raycast(firePoint.position, dir, dist, LayerMask.GetMask("Map"));
                    dir = (b.transform.position - firePoint.position).normalized;
                    dist = Vector2.Distance(b.transform.position, firePoint.position);
                    bool bBlocked = Physics2D.Raycast(firePoint.position, dir, dist, LayerMask.GetMask("Map"));
                    if (aBlocked)
                    {
                        if (bBlocked) return 0;
                        else return 1;
                    }
                    if (bBlocked) return -1;

                    return Vector2.Distance(transform.position, a.transform.position).CompareTo(Vector2.Distance(transform.position, b.transform.position));
                });

                var tmp = targets[0];
                Vector2 dir = (tmp.transform.position - firePoint.position).normalized;
                float dist = Vector2.Distance(tmp.transform.position, firePoint.position);
                bool blocked = Physics2D.Raycast(firePoint.position, dir, dist, LayerMask.GetMask("Map")).transform != null;
                Debug.DrawLine(firePoint.position, (Vector2)firePoint.position + dir * dist, blocked ? Color.green : Color.red, 1.0f);

                if (!tmp.broken.Value && !blocked) ChangeTarget(targets[0]);
                else ChangeTarget(null);
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
        if (!IsOwner) return;
        GameObject hitObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

        if (hitObject.TryGetComponent(out MortarTarget target) && target.team != owner.team.Value)
        {
            targets.Add(target);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsOwner) return;
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
