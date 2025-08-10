using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public float destroyTime = 2f;
    public float damage = 10f;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) {

        GameObject hitObject = other.attachedRigidbody.gameObject;

        if (hitObject.TryGetComponent(out IDamagable enemy))
        {
            enemy.GetDamage(damage);
            Destroy(gameObject);
        }
    }
}

