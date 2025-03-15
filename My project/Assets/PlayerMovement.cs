using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 16f;
    private bool isFacingRight = true;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;

    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float increasedGravity = 10f;
    private bool isPressingS = false;

    private bool canDoubleJump = false;
    private bool canJump = true;

    public bool canMove = true;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (IsGrounded())
        {
            canDoubleJump = true;
        }

        if (Input.GetButtonDown("Jump") && IsGrounded() && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            canDoubleJump = true;
        }
        else if (Input.GetButtonDown("Jump") && !IsGrounded() && canDoubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            canDoubleJump = false;
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f)
        {
            StartCoroutine(Dash());
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        isPressingS = Input.GetKey(KeyCode.S);

        Flip();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isPressingS)
        {
            rb.gravityScale = increasedGravity;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }

        if (!isDashing && canMove)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void UpdateAnimations()
    {
        animator.SetBool("MovingRight", horizontal > 0);
        animator.SetBool("MovingLeft", horizontal < 0);
        animator.SetBool("IsGrounded", IsGrounded());
    }

    private System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashDirection = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }
}
