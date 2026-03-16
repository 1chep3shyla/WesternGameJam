using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMovement
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 70f;
    [SerializeField] private float airAcceleration = 40f;
    [SerializeField] private float airDeceleration = 40f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private GameObject dashGhostPrefab;
    [SerializeField] private float dashGhostInterval = 0.03f;
    [SerializeField] private float[] dashGhostOpacities = { 0.8f, 0.6f, 0.4f, 0.25f };

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private GameObject jumpImpactPrefab;
    [SerializeField] private GameObject landImpactPrefab;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundMask;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private float walkStepInterval = 0.35f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float moveInput;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool isGrounded;
    private bool wasGrounded;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool dashPressed;
    private float dashTimer;
    private float dashCooldownTimer;
    private Coroutine dashGhostRoutine;
    private float nextStepTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (jumpPressed)
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
        }

        if (dashPressed && dashCooldownTimer <= 0f && dashTimer <= 0f && Mathf.Abs(moveInput) > 0.01f)
        {
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            StartDashGhosts();
            PlayClip(dashClip);
        }

        if (jumpReleased && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        CheckGrounded();
        HandleHorizontal();
        HandleJump();
    }

    private void CheckGrounded()
    {
        wasGrounded = isGrounded;

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        }
        else
        {
            isGrounded = false;
        }

        if (!wasGrounded && isGrounded)
        {
            SpawnImpact(landImpactPrefab);
            PlayClip(landClip);
        }

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }
    }

    private void HandleHorizontal()
    {
        if (dashTimer > 0f)
        {
            float dashDirection = moveInput != 0f ? Mathf.Sign(moveInput) : Mathf.Sign(transform.localScale.x);
            rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);
            return;
        }

        float targetSpeed = moveInput * maxSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;

        float accelRate = isGrounded
            ? (Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration)
            : (Mathf.Abs(targetSpeed) > 0.01f ? airAcceleration : airDeceleration);

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

        if (isGrounded && Mathf.Abs(moveInput) > 0.01f && Time.time >= nextStepTime)
        {
            PlayClip(walkClip);
            nextStepTime = Time.time + walkStepInterval;
        }
    }

    private void HandleJump()
    {
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            SpawnImpact(jumpImpactPrefab);
            PlayClip(jumpClip);
        }
    }

    private void StartDashGhosts()
    {
        if (dashGhostPrefab == null || dashGhostOpacities == null || dashGhostOpacities.Length == 0)
        {
            return;
        }

        if (dashGhostRoutine != null)
        {
            StopCoroutine(dashGhostRoutine);
        }

        dashGhostRoutine = StartCoroutine(SpawnDashGhosts());
    }

    private IEnumerator SpawnDashGhosts()
    {
        for (int i = 0; i < dashGhostOpacities.Length; i++)
        {
            SpawnDashGhost(dashGhostOpacities[i]);
            if (dashGhostInterval > 0f)
            {
                yield return new WaitForSeconds(dashGhostInterval);
            }
        }

        dashGhostRoutine = null;
    }

    private void SpawnDashGhost(float opacity)
    {
        if (dashGhostPrefab == null || spriteRenderer == null)
        {
            return;
        }

        GameObject ghost = Instantiate(dashGhostPrefab, transform.position, transform.rotation);
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();
        if (ghostRenderer != null)
        {
            ghostRenderer.sprite = spriteRenderer.sprite;
            Color color = ghostRenderer.color;
            color.a = Mathf.Clamp01(opacity);
            ghostRenderer.color = color;
        }
    }

    private void SpawnImpact(GameObject impactPrefab)
    {
        if (impactPrefab == null)
        {
            return;
        }

        Vector3 position = groundCheck != null ? groundCheck.position : transform.position;
        Instantiate(impactPrefab, position, Quaternion.identity);
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
        this.jumpPressed = jumpPressed;
        this.jumpReleased = jumpReleased;
    }

    public void SetDashInput(bool dashPressed)
    {
        this.dashPressed = dashPressed;
    }

    public bool IsGrounded => isGrounded;

    private void PlayClip(AudioClip clip)
    {
        if (AudioManager.Instance == null || clip == null)
        {
            return;
        }

        AudioManager.Instance.PlayOneShot(clip);
    }
}
