using UnityEngine;

public class EnemyMovement : MonoBehaviour, IMovement
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 60f;

    [Header("Chase")]
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private Vector2 chaseBoxSize = new Vector2(6f, 3f);
    [SerializeField] private Vector2 chaseBoxOffset;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody2D rb;
    private Animator animator;
    private float moveInput;
    private bool isGrounded;
    private bool hasSeenTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector2 boxCenter = (Vector2)transform.position + chaseBoxOffset;

        if (!hasSeenTarget)
        {
            Collider2D foundTarget = Physics2D.OverlapBox(boxCenter, chaseBoxSize, 0f, targetMask);
            if (foundTarget != null)
            {
                target = foundTarget.transform;
                hasSeenTarget = true;
            }
        }

        if (target == null)
        {
            moveInput = 0f;
        }
        else
        {
            if (hasSeenTarget)
            {
                moveInput = Mathf.Sign(target.position.x - transform.position.x);
            }
            else
            {
                moveInput = 0f;
            }
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        HandleHorizontal();
    }

    private void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        }
        else
        {
            isGrounded = true;
        }
    }

    private void HandleHorizontal()
    {
        float targetSpeed = moveInput * maxSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        float movement = accelRate * speedDiff;
        rb.AddForce(new Vector2(movement, 0f));

        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }

        if (moveInput != 0f)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1f, 1f);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.velocity.y);

        if (!isGrounded && rb.velocity.y > 0f)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("animation_player_jump"))
            {
                animator.Play("animation_player_jump");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 boxCenter = transform.position + (Vector3)chaseBoxOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(boxCenter, new Vector3(chaseBoxSize.x, chaseBoxSize.y, 0f));

        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    public void SetMoveInput(float moveInput)
    {
        this.moveInput = moveInput;
    }

    public void SetJumpInput(bool jumpPressed, bool jumpReleased)
    {
    }
}
