using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public enum EnemyReaction { MoveTowards, Flee } // Typ reakcji
    public EnemyReaction reactionType; // Ustawiany typ reakcji

    public float speed = 3f; // Prędkość ruchu
    public float detectionRange = 5f; // Zasięg wykrywania pochodni

    private GameObject torch; // Referencja do pochodni

    void Update()
    {
        // Szukamy pochodni w scenie (lub można podać jej referencję)
        if (torch == null)
        {
            torch = GameObject.FindWithTag("Torch");
        }

        if (torch != null)
        {
            float distanceToTorch = Vector3.Distance(transform.position, torch.transform.position);

            if (distanceToTorch <= detectionRange)
            {
                ReactToTorch(torch.transform.position);
            }
        }
    }

    void ReactToTorch(Vector3 torchPosition)
    {
        // Kierunek ruchu zależny od reakcji
        Vector3 direction;

        if (reactionType == EnemyReaction.MoveTowards)
        {
            // Ruch w stronę pochodni
            direction = (torchPosition - transform.position).normalized;
        }
        else if (reactionType == EnemyReaction.Flee)
        {
            // Ruch w przeciwnym kierunku
            direction = (transform.position - torchPosition).normalized;
        }
        else
        {
            return; // Brak reakcji
        }

        // Aktualizacja pozycji wroga
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        // Rysowanie zasięgu wykrywania w edytorze
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
