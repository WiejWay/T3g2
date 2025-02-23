using UnityEngine;

public class TorchThrow : MonoBehaviour
{
    [Header("Ustawienia pochodni")]
    [SerializeField] private GameObject torchPrefab;        // Prefab z Rigidbody2D
    [SerializeField] private Transform spawnPoint;          // Miejsce w ręce gracza (punkt startowy)
    [SerializeField] private float throwForce = 10f;        // Siła rzutu pochodni
    [SerializeField] private KeyCode throwKey = KeyCode.E;  // Klawisz do rzucania i przyciągania

    [Header("Linia (wizualna)")]
    [SerializeField] private LineRenderer lineRenderer;     // Do rysowania "linki" 

    [Header("Przyciąganie pochodni")]
    [SerializeField] private float returnSpeed = 5f;        // Prędkość, z jaką wraca pochodnia

    [Header("Trzymanie pochodni (PPM)")]
    [SerializeField] private KeyCode torchHoldButton = KeyCode.Mouse1; // Prawy przycisk myszy
    [SerializeField] private Vector2 holdTorchOffset = new Vector2(1f, 0f); // Offset w prawo

    // ========================================
    //  ZMIENNE STANU
    // ========================================

    private GameObject currentTorch;    // Aktualnie wyrzucona pochodnia
    private bool isReturning = false;   // Czy przyciągamy już pochodnię

    private GameObject holdTorch;       // Pochodnia "w ręce" (po przytrzymaniu PPM)

    void Update()
    {
        // 1) Trzymanie pochodni (PPM)
        HandleRightMouseHold();

        // 2) Rzut / przyciąganie pochodni (E)
        HandleTorchThrowAndReturn();

        // 3) Rysowanie liny (wizualnie)
        UpdateLineRenderer();
    }

    private void FixedUpdate()
    {
        // Jeśli przyciągamy wyrzuconą pochodnię (drugie naciśnięcie E)
        if (isReturning && currentTorch != null)
        {
            Rigidbody2D torchRb = currentTorch.GetComponent<Rigidbody2D>();
            if (torchRb != null)
            {
                Vector2 directionToPlayer = (spawnPoint.position - currentTorch.transform.position);
                float distance = directionToPlayer.magnitude;

                if (distance < 0.2f)
                {
                    // Gdy jest blisko, niszczymy pochodnię
                    Destroy(currentTorch);
                    currentTorch = null;
                    isReturning = false;
                }
                else
                {
                    // Przyciąganie - nadaj velocity w stronę gracza
                    Vector2 pullDir = directionToPlayer.normalized;
                    torchRb.velocity = pullDir * returnSpeed;
                }
            }
        }
    }

    // ================================================================
    //  METODY SZCZEGÓŁOWE
    // ================================================================

    // ------------------- A) Trzymanie pochodni w ręce przy PPM ------------------- //
    private void HandleRightMouseHold()
    {
        // Jeśli wyrzuciłeś pochodnię (currentTorch != null), to nie możesz trzymać w ręce
        // (i odwrotnie - jeśli trzymasz w ręce, nie możesz rzucić).
        // Sprawdzamy tę zależność w logice poniżej.

        // 1) Naciśnięcie PPM
        if (Input.GetKeyDown(torchHoldButton))
        {
            // Jeśli już jest wyrzucona pochodnia, blokujemy trzymanie w ręce.
            if (currentTorch != null) return;

            // Utwórz holdTorch, jeśli jeszcze go nie ma
            if (holdTorch == null)
            {
                holdTorch = Instantiate(torchPrefab, spawnPoint.position, Quaternion.identity);

                // Wyłącz fizykę i kolizje, bo chcemy tylko wizualnie trzymać
                var rb = holdTorch.GetComponent<Rigidbody2D>();
                if (rb != null) rb.isKinematic = true;

                var col = holdTorch.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }

        // 2) Podczas trzymania PPM
        if (Input.GetKey(torchHoldButton))
        {
            if (holdTorch != null)
            {
                // Aktualizujemy pozycję z offsetem w prawo (lub innym)
                Vector2 basePos = spawnPoint.position;
                holdTorch.transform.position = basePos + holdTorchOffset;
            }
        }

        // 3) Puszczenie PPM => niszczymy holdTorch
        if (Input.GetKeyUp(torchHoldButton))
        {
            if (holdTorch != null)
            {
                Destroy(holdTorch);
                holdTorch = null;
            }
        }
    }

    // ------------------- B) Rzucanie pochodnią / przyciąganie klawiszem E ------------------- //
    private void HandleTorchThrowAndReturn()
    {
        // Gdy wciśniemy E
        if (Input.GetKeyDown(throwKey))
        {
            // Jeśli TRZYMAMY pochodnię w ręce, nie pozwól rzucić
            if (holdTorch != null) return;

            // A) jeśli nie ma wyrzuconej pochodni, to rzuć nową
            if (currentTorch == null)
            {
                ThrowTorch();
            }
            else
            {
                // B) jeśli jest już wyrzucona, to zacznij ją przyciągać
                isReturning = true;
            }
        }
    }

    // ------------------- C) Rzut pochodnią (pierwsze naciśnięcie E) ------------------- //
    private void ThrowTorch()
    {
        if (torchPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Brakuje TorchPrefab lub SpawnPoint w Inspektorze!");
            return;
        }

        // Kierunek w stronę myszy (2D)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = spawnPoint.position.z;
        Vector2 throwDir = (mousePos - spawnPoint.position).normalized;

        // Tworzymy pochodnię
        currentTorch = Instantiate(torchPrefab, spawnPoint.position, Quaternion.identity);

        // Nadajemy jej prędkość
        Rigidbody2D torchRb = currentTorch.GetComponent<Rigidbody2D>();
        if (torchRb == null)
        {
            Debug.LogError("TorchPrefab nie ma Rigidbody2D!");
            return;
        }

        torchRb.velocity = throwDir * throwForce;

        // Nie przyciągamy jeszcze
        isReturning = false;
    }

    // ------------------- D) Rysowanie liny (wizualnie) ------------------- //
    private void UpdateLineRenderer()
    {
        if (lineRenderer == null) return;

        if (currentTorch != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, spawnPoint.position);
            lineRenderer.SetPosition(1, currentTorch.transform.position);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }
}
