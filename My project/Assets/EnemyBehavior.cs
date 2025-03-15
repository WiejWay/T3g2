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
        bool isInLight = (torch != null) && (distanceToTorch <= detectionRange);

        if (enemyMode == EnemyMode.ChaseWhenLit)
        {
            if (isInLight)
            {
                MoveTowards(player.transform.position);
            }
        }
        else if (enemyMode == EnemyMode.ChaseButFleeFromLight)
        {
            if (isInLight && distanceToTorch <= fleeRange)
            {
                MoveAway(torch.transform.position);
            }
            else if (distanceToPlayer <= detectionRange)
            {
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
}
