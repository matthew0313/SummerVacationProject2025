using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public GameObject owner;

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Vector2 direction = (mousePosition - owner.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (owner != null)
        {
            transform.position = new Vector2(owner.transform.position.x + direction.x * 1.5f, owner.transform.position.y + direction.y * 1.5f);
        }

        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        if (Input.GetMouseButtonUp(0))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        Debug.Log("발사!");
    }
}
