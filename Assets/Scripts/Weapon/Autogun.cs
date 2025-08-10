using UnityEngine;

public class Autogun : Weapon
{
    private Animator animator;
    public GameObject bulletPrefab;

    public float shootCooldown = 0.1f;
    private float lastShootTime = -Mathf.Infinity;

    void Start()
    {
        Debug.Log("ø¨ªÁ√—");

        animator = GetComponent<Animator>();
    }

    public override void Shoot()
    {
        if (Time.time < lastShootTime + shootCooldown)
            return;
        lastShootTime = Time.time;

        animator.SetTrigger("Shoot");
        Instantiate(bulletPrefab, transform.position, transform.rotation);
    }

    public override bool Trigger()
    {
        return Input.GetMouseButton(0);
    }
}

