using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public abstract class Weapon : NetworkBehaviour
{
    public readonly NetworkVariable<bool> active = new(true);

    public Sprite weaponSprite;
    public Player owner;
   
    protected virtual void Awake()
    {
        active.OnValueChanged += (prev, val) => gameObject.SetActive(val);
    }
    protected virtual void Start()
    {
        gameObject.SetActive(active.Value);
    }
    private Vector2 direction;
    void Update()
    {
        if (!IsOwner) return;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        direction = (mousePosition - owner.transform.position).normalized;
        if (direction.x > 0) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        if (direction.x < 0) transform.localScale = new Vector3(1.0f, -1.0f, 1.0f);
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
        active.Value = true;
        transform.position = owner.transform.position;
    }

    public virtual void OnUnequipped()
    {
        active.Value = false;
    }

    public void FixDistance(Vector3 holdPosition)
    {
        if (owner != null && holdPosition != transform.position)
        {
                transform.position = Vector3.Lerp(transform.position, holdPosition, 30.0f * Time.deltaTime);
        }
        // if (owner != null)
        // {
        //     transform.position = new Vector2(owner.transform.position.x + direction.x * 1.5f, owner.transform.position.y + direction.y * 1.5f);
        // }
    }

    public virtual void Shoot()
    {
        
    }

    public virtual bool Trigger()
    {
        return Input.GetMouseButtonDown(0) && UIScanner.ScanUI(Input.mousePosition).Count <= 0; 
    }
}
