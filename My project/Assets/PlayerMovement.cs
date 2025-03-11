using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;           // Prędkość ruchu poziomego
    public float jumpForce = 10f;          // Siła skoku
    public float verticalClimbSpeed = 3f;  // Prędkość wspinania się w górę
    public LayerMask groundLayer;          // Warstwa dla podłoża

    [HideInInspector]
    public bool canMove = true;            // Flaga decydująca o możliwości ruchu

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D playerCollider;     // Referencja do komponentu Collider2D
    private bool isGrounded;               // Czy postać dotyka ziemi
    private bool canClimb;                 // Czy postać może się wspinać

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();

        if (animator == null)
        {
            Debug.LogError("Nie znaleziono komponentu Animator!");
        }
        if (playerCollider == null)
        {
            Debug.LogError("Nie znaleziono komponentu Collider2D!");
        }
    }

    void Update()
    {
        // Jeśli gracz nie powinien się ruszać – zatrzymaj i zakończ Update
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Aktualizacja stanu "czy na ziemi"
        isGrounded = CheckGrounded();

        // Ruch poziomy
        float horizontal = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        // Skok - działa, gdy gracz dotyka ziemi (groundLayer)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Wspinanie (jeśli używasz drabiny)
        if (canClimb)
        {
            float vertical = Input.GetAxis("Vertical");
            rb.velocity = new Vector2(rb.velocity.x, vertical * verticalClimbSpeed);
        }

        // Aktualizacja animacji oraz obracanie postaci
        if (horizontal > 0)
        {
            animator.SetBool("MovingRight", true);
            animator.SetBool("MovingLeft", false);
            // Obracamy postać w prawo
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontal < 0)
        {
            animator.SetBool("MovingRight", false);
            animator.SetBool("MovingLeft", true);
            // Obracamy postać w lewo (flip)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            animator.SetBool("MovingRight", false);
            animator.SetBool("MovingLeft", false);
        }
    }

    // Metoda sprawdzająca, czy gracz stoi na ziemi (groundLayer)
    private bool CheckGrounded()
    {
        Vector2 position = (Vector2)transform.position - new Vector2(0, playerCollider.bounds.extents.y);
        Vector2 direction = Vector2.down;
        float distance = 0.1f; // Dystans promienia

        Debug.DrawRay(position, direction * distance, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        return hit.collider != null;
    }
}
