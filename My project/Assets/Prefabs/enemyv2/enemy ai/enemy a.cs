using UnityEngine;

public class EnemyChaseWhenPlayerIsLit : MonoBehaviour
{
    public float speed = 3f;
    public float detectionRange = 5f;
    public float visionRange = 10f;

    private GameObject torch;
    private GameObject player;
    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        if (player == null) return;
        if (torch == null) torch = GameObject.FindWithTag("Torch");
        if (torch == null) return;

        float dPT = Vector3.Distance(player.transform.position, torch.transform.position);
        float dEP = Vector3.Distance(transform.position, player.transform.position);

        if (dPT <= detectionRange && dEP <= visionRange)
            MoveTowards(player.transform.position);
        else if (animator != null)
            animator.SetBool("isMoving", false);
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        bool moving = dir.sqrMagnitude > 0.001f;
        if (animator != null) animator.SetBool("isMoving", moving);
        if (!moving) return;

        transform.right = dir;
        Vector3 step = dir * speed * Time.deltaTime;
        if (rb != null) rb.MovePosition(transform.position + step);
        else transform.position += step;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TeleportTrigger.TeleportPlayer(collision.gameObject);
    }
}
