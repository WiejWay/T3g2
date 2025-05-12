using UnityEngine;

public class EnemyChaseWhenPlayerIsLit : MonoBehaviour
{
    public float speed = 5f;
    public float moveThreshold = 0.01f;
    public float detectionRange = 5f;
    public float visionRange = 10f;

    private GameObject torch;
    private GameObject player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool shouldChase;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");
        if (torch == null)
            torch = GameObject.FindWithTag("Torch");

        if (player == null || torch == null)
        {
            shouldChase = false;
            animator?.SetBool("isMoving", false);
            return;
        }

        float dPT = Vector2.Distance(player.transform.position, torch.transform.position);
        float dEP = Vector2.Distance(transform.position, player.transform.position);
        shouldChase = (dPT <= detectionRange && dEP <= visionRange);

        float vx = rb.velocity.x;
        bool moving = Mathf.Abs(vx) > moveThreshold;
        animator.SetBool("isMoving", moving);

        if (vx > moveThreshold)
            spriteRenderer.flipX = false;
        else if (vx < -moveThreshold)
            spriteRenderer.flipX = true;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (shouldChase)
        {
            Vector2 dir = ((Vector2)player.transform.position - rb.position).normalized;
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TeleportTrigger.TeleportPlayer(collision.gameObject);
    }
}
