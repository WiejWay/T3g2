using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Prędkość ruchu poziomego
    public float jumpForce = 10f; // Siła skoku
    public float verticalClimbSpeed = 3f; // Prędkość wspinania się w górę
    public LayerMask groundLayer; // Warstwa dla podłoża

    private Rigidbody2D rb;
    private bool isGrounded; // Czy postać dotyka ziemi
    private bool canClimb; // Czy postać może się wspinać

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Ruch poziomy
        float horizontal = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        // Skok
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Wspinanie
        if (canClimb)
        {
            float vertical = Input.GetAxis("Vertical");
            rb.velocity = new Vector2(rb.velocity.x, vertical * verticalClimbSpeed);
        }

        // Sprawdzenie, czy postać stoi na ziemi
        isGrounded = CheckGrounded();
    }

    private bool CheckGrounded()
    {
        // Sprawdzenie, czy postać dotyka ziemi
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        return hit.collider != null;
    }

    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Ladder"))
    //     {
    //         canClimb = true;
    //         rb.gravityScale = 0; // Wyłącz grawitację podczas wspinania
    //     }
    // }

    // private void OnTriggerExit2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Ladder"))
    //     {
    //         canClimb = false;
    //         rb.gravityScale = 1; // Przywróć grawitację
    //     }
    // }
}
