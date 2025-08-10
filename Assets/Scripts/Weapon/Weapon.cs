using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public abstract class Weapon : MonoBehaviour
{
    public Player owner;
    public Vector2 direction;
    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        direction = (mousePosition - owner.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 holdPosition = new Vector3(owner.transform.position.x + direction.x * 1.5f, owner.transform.position.y + direction.y * 1.5f, owner.transform.position.z);
        FixDistance(holdPosition);

        if (Trigger())
        {
            Shoot();
        }
    }

    public virtual void OnEquipped()
    {
        
    }

    public virtual void OnUnequipped()
    {

    }

    public void FixDistance(Vector3 holdPosition)
    {
        if (owner != null && holdPosition != transform.position)
        {
                transform.position = Vector3.MoveTowards(transform.position, holdPosition, 30.0f * Time.deltaTime);
        }
        // if (owner != null)
        // {
        //     transform.position = new Vector2(owner.transform.position.x + direction.x * 1.5f, owner.transform.position.y + direction.y * 1.5f);
        // }
    }

    public virtual void Shoot()
    {
        Debug.Log("발사!");
    }

    public virtual bool Trigger()
    {
        return Input.GetMouseButtonUp(0); 
    }
}
