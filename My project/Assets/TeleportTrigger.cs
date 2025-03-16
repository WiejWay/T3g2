using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [Tooltip("Tag gracza (np. 'Player')")]
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            // Sprawdzamy, czy mamy ustawiony aktualny spawnpoint
            if (CheckPointManager.Instance != null && CheckPointManager.Instance.currentSpawnPoint != null)
            {
                // Teleportujemy gracza do ustawionego spawnpointa
                collision.transform.position = CheckPointManager.Instance.currentSpawnPoint.position;
                
                // Zatrzymujemy ruch gracza, aby nie spadał (zakładamy, że gracz ma Rigidbody2D)
                Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }
            }
            else
            {
                Debug.LogWarning("Brak ustawionego spawnpointa w CheckPointManager!");
            }
        }
    }
     /// <summary>
    /// Teleportuje obiekt (gracza) do aktualnego spawnpointa ustawionego w CheckPointManager
    /// i zatrzymuje jego ruch.
    /// </summary>
    /// <param name="player">Obiekt gracza do teleportacji.</param>
    public static void TeleportPlayer(GameObject player)
    {
        if (CheckPointManager.Instance != null && CheckPointManager.Instance.currentSpawnPoint != null)
        {
            // Ustaw pozycję gracza na pozycję aktualnego spawnpointa
            player.transform.position = CheckPointManager.Instance.currentSpawnPoint.position;
            
            // Zatrzymaj ruch gracza
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            Debug.LogWarning("Brak ustawionego spawnpointa w CheckPointManager!");
        }
    }
}
