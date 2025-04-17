using UnityEngine;

public class EnemyChaseButFleeFromLight : MonoBehaviour
{
    public float speed = 3f;
    public float detectionRange = 5f; // Zasiêg wykrycia gracza
    public float fleeRange = 2f;      // Zasiêg ucieczki od pochodni

    private GameObject torch;
    private GameObject player;
    private Rigidbody rb;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;

        if (player == null) Debug.LogWarning("Player not found!");
    }

    void Update()
    {
        if (player == null) return;

        if (torch == null)
        {
            torch = GameObject.FindWithTag("Torch");
        }

        float distanceToTorch = (torch != null) ? Vector3.Distance(transform.position, torch.transform.position) : Mathf.Infinity;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Ucieka, jeœli jest zbyt blisko pochodni
        if (torch != null && distanceToTorch <= fleeRange)
        {
            MoveAway(torch.transform.position);
        }
        // Inaczej – goni gracza, jeœli ten jest w zasiêgu wykrycia
        else if (distanceToPlayer <= detectionRange)
        {
            MoveTowards(player.transform.position);
        }
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        if (rb != null)
            rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
        else
            transform.position += direction * speed * Time.deltaTime;
    }

    void MoveAway(Vector3 target)
    {
        Vector3 direction = (transform.position - target).normalized;
        if (rb != null)
            rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
        else
            transform.position += direction * speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TeleportTrigger.TeleportPlayer(collision.gameObject);
        }
    }
}
