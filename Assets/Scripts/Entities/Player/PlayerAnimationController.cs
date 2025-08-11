using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator animator;
    public float moveSpeed = 3f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");

        // 이동
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // Idle ↔ Walk 전환
        animator.SetBool("isWalking", moveX != 0);

        // 좌우 방향 전환
        if (moveX != 0)
            transform.localScale = new Vector3(Mathf.Sign(moveX), 1, 1);
    }
}