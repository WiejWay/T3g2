using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelBarrowMovement : MonoBehaviour {
    public float pushForce;
    public Rigidbody2D rb;

    void Start() {
        rb = GetComponent<Rigidbody2D>();

        SetRigidbodyToStatic();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            rb.bodyType = RigidbodyType2D.Dynamic;
        } else {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        foreach (ContactPoint2D contact in collision.contacts) {
            Vector2 contactDirection = (Vector2)transform.position - contact.point;

            if (contactDirection.x > 0.1f) {
                rb.AddForce(Vector2.right * pushForce, ForceMode2D.Impulse);
            }
        }
    }

    void SetRigidbodyToStatic() {
        Rigidbody2D[] rigidbodies = GetComponents<Rigidbody2D>();

        foreach (var rb in rigidbodies) {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
}
