using UnityEngine;

public class EnemyChaseWhenPlayerIsLit : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    [Tooltip("Minimalna prêdkoœæ pozioma, przy której uznajemy, ¿e wróg siê porusza")]
    public float moveThreshold = 0.01f;

    [Header("Detection")]
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
        // 1) Znajduj gracza i pochodniê, jeœli jeszcze nie masz
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

        // 2) Detekcja: gracz blisko pochodni i w zasiêgu widzenia
        float dPT = Vector2.Distance(player.transform.position, torch.transform.position);
        float dEP = Vector2.Distance(transform.position, player.transform.position);
        shouldChase = (dPT <= detectionRange && dEP <= visionRange);

        // 3) Animacja: opieramy siê na faktycznej prêdkoœci poziomej
        float vx = rb.velocity.x;
        bool moving = Mathf.Abs(vx) > moveThreshold;
        animator.SetBool("isMoving", moving);

        // 4) Flip sprite'a w zale¿noœci od kierunku ruchu
        if (vx > moveThreshold)
            spriteRenderer.flipX = false; // patrzy w prawo
        else if (vx < -moveThreshold)
            spriteRenderer.flipX = true;  // patrzy w lewo
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (shouldChase)
        {
            // Ruch fizyki: tylko oœ X, oœ Y zostawiamy grawitacji
            Vector2 dir = ((Vector2)player.transform.position - rb.position).normalized;
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            // Zerujemy prêdkoœæ poziom¹ – dziêki temu velocity.x == 0
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TeleportTrigger.TeleportPlayer(collision.gameObject);
    }
}
