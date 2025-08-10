using UnityEngine;

public class Handgun : Weapon
{
    private Animator animator;
    public GameObject bulletPrefab;

    public float shootCooldown = 0.05f;
    private float lastShootTime = -Mathf.Infinity;

    void Start()
    {
        Debug.Log("권총");

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
}
