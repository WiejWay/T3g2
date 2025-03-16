using UnityEngine;

public class SpawnpointSave : MonoBehaviour
{
    [Tooltip("Odwołanie do spawnPoint, czyli pozycji, do której gracz ma być teleportowany.")]
    public Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Ustawiamy nowy spawnpoint w CheckPointManager
            if (CheckPointManager.Instance != null)
            {
                CheckPointManager.Instance.currentSpawnPoint = spawnPoint;
                Debug.Log("Nowy spawnpoint ustawiony!");
            }
            else
            {
                Debug.LogWarning("CheckPointManager nie został znaleziony w scenie.");
            }
        }
    }
}
