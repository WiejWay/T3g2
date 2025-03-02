using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public enum EnemyMode { ChaseWhenLit, ChaseButFleeFromLight }
    public EnemyMode enemyMode;

    public float speed = 3f;
    public float detectionRange = 5f;
    public float fleeRange = 2f;

    private GameObject torch;
    private GameObject player;
    private Rigidbody rb;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;

        if (player == null) Debug.LogWarning("⚠️ Player (gracz) nie znaleziony! Ustaw tag 'Player'.");
    }

    void Update()
    {
        // 🔥 SZUKAMY POCHODNI CO KAŻDĄ KLATKĘ (jeśli nie została znaleziona)
        if (torch == null)
        {
            torch = GameObject.FindWithTag("Torch");
            if (torch != null) Debug.Log("🔦 Pochodnia znaleziona!");
        }

        if (player == null) return;

        float distanceToTorch = (torch != null) ? Vector3.Distance(transform.position, torch.transform.position) : Mathf.Infinity;
        bool isInLight = (torch != null) && (distanceToTorch <= detectionRange);

        if (enemyMode == EnemyMode.ChaseWhenLit)
        {
            if (isInLight)
            {
                Debug.Log("🔥 Wróg goni gracza, bo jest w świetle.");
                MoveTowards(player.transform.position);
            }
        }
        else if (enemyMode == EnemyMode.ChaseButFleeFromLight)
        {
            if (isInLight && distanceToTorch <= fleeRange)
            {
                Debug.Log("🏃 Wróg ucieka, bo dotknął światła!");
                MoveAway(torch.transform.position);
            }
            else
            {
                Debug.Log("👀 Wróg goni gracza.");
                MoveTowards(player.transform.position);
            }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRange);
    }
}
