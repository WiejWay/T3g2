using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public static CheckPointManager Instance;
    [Tooltip("Aktualny spawnpoint, na który będzie teleportowany gracz.")]
    public Transform currentSpawnPoint;

    private void Awake()
    {
        // Zapewniamy, że istnieje tylko jedna instancja managera
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
