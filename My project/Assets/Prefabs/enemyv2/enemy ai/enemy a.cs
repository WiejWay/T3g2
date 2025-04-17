using UnityEngine;

public class EnemyChaseWhenPlayerIsLit : MonoBehaviour
{
    public float speed = 3f;
    public float detectionRange = 5f; // Zasiêg œwiat³a – czy gracz jest oœwietlony
    public float visionRange = 10f;   // Zasiêg widzenia wroga

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

        if (torch == null) return;

        float distancePlayerToTorch = Vector3.Distance(player.transform.position, torch.transform.position);
        float distanceEnemyToPlayer = Vector3.Distance(transform.position, player.transform.position);

        bool playerIsInLight = distancePlayerToTorch <= detectionRange;
        bool playerIsVisibleToEnemy = distanceEnemyToPlayer <= visionRange;

        if (playerIsInLight && playerIsVisibleToEnemy)
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TeleportTrigger.TeleportPlayer(collision.gameObject);
        }
    }
}
