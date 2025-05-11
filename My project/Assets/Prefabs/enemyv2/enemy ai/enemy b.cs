using UnityEngine;

public class EnemyChaseButFleeFromLight : MonoBehaviour
{
    public float speed = 3f;
    public float detectionRange = 5f;
    public float fleeRange = 2f;

    private GameObject torch;
    private GameObject player;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;
        if (torch == null) torch = GameObject.FindWithTag("Torch");

        float distTorch = torch != null ? Vector3.Distance(transform.position, torch.transform.position) : Mathf.Infinity;
        float distPlayer = Vector3.Distance(transform.position, player.transform.position);
        bool isMoving = false;
        Vector2 vel = rb.velocity;

        if (torch != null && distTorch <= fleeRange)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)torch.transform.position).normalized;
            vel.x = dir.x * speed;
            isMoving = true;
        }
        else if (distPlayer <= detectionRange)
        {
            Vector2 dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
            vel.x = dir.x * speed;
            isMoving = true;
        }
        else
        {
            vel.x = 0;
        }

        rb.velocity = new Vector2(vel.x, vel.y);

        if (animator != null)
            animator.SetBool("isMoving", isMoving);

        if (vel.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (vel.x > 0 ? 1 : -1);
            transform.localScale = scale;
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Player"))
            TeleportTrigger.TeleportPlayer(c.gameObject);
    }
}
