using UnityEngine;
using UnityEngine.UI;  // Pamiętaj o dodaniu przestrzeni nazw dla UI

public class TorchThrow : MonoBehaviour
{
    [Header("Ustawienia pochodni")]
    [SerializeField] private GameObject torchPrefab;        // Prefab z Rigidbody2D
    [SerializeField] private Transform spawnPoint;          // Miejsce w ręce gracza (punkt startowy)
    [SerializeField] private float minThrowForce = 5f;        // Minimalna siła rzutu
    [SerializeField] private float maxThrowForce = 20f;       // Maksymalna siła rzutu
    [SerializeField] private float maxChargeTime = 2f;        // Czas ładowania by osiągnąć maxThrowForce
    [SerializeField] private KeyCode throwKey = KeyCode.E;    // Klawisz do ładowania/rzutu oraz przyciągania

    [Header("Linia (wizualna)")]
    [SerializeField] private LineRenderer lineRenderer;       // Do rysowania "linki"

    [Header("Przyciąganie pochodni")]
    [SerializeField] private float returnSpeed = 5f;          // Bazowa prędkość przyciągania
    [SerializeField] private float returnSpeedMultiplier = 3f;  // Współczynnik zwiększania prędkości – im dłużej leci, tym większa prędkość

    [Header("Trzymanie pochodni (PPM)")]
    [SerializeField] private KeyCode torchHoldButton = KeyCode.Mouse1; // PPM
    [SerializeField] private Vector2 holdTorchOffset = new Vector2(1f, 0f); // Offset w prawo

    [Header("UI Slider")]
    [SerializeField] private Slider throwSlider;              // Slider z Player > Canvas > Slider

    [Header("Player Movement")]
    [SerializeField] private PlayerMovement playerMovementScript; // Referencja do skryptu ruchu gracza

    // ========================================
    // ZMIENNE STANU
    // ========================================
    private GameObject currentTorch;    // Aktualnie wyrzucona lub ładowana pochodnia
    private bool isReturning = false;   // Czy przyciągamy już pochodnię

    private GameObject holdTorch;       // Pochodnia "w ręce" (przytrzymanie PPM)

    // Zmienne związane z ładowaniem siły rzutu
    private float chargeTimer = 0f;
    private bool isCharging = false;
    // Zmienna do rejestrowania czasu rzutu
    private float throwTime = 0f;

    void Update()
    {
        // 1) Obsługa trzymania pochodni (PPM)
        HandleRightMouseHold();

        // 2) Obsługa ładowania, rzutu oraz przyciągania pochodni (klawisz E)
        HandleTorchThrowAndReturn();

        // 3) Aktualizacja wizualnej liny
        UpdateLineRenderer();
    }

    private void FixedUpdate()
    {
        // Jeśli pochodnia jest ładowana, zatrzymaj gracza (ustawiamy canMove = false).
        // Gdy nie ładuje, pozwól graczowi się poruszać.
        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = !isCharging;
        }

        // Jeśli przyciągamy wyrzuconą pochodnię
        if (isReturning && currentTorch != null)
        {
            Rigidbody2D torchRb = currentTorch.GetComponent<Rigidbody2D>();
            if (torchRb != null)
            {
                Vector2 directionToPlayer = (spawnPoint.position - currentTorch.transform.position);
                float distance = directionToPlayer.magnitude;

                if (distance < 0.2f)
                {
                    // Gdy jest blisko, niszczymy pochodnię i przywracamy ruch gracza
                    Destroy(currentTorch);
                    currentTorch = null;
                    isReturning = false;
                    if (playerMovementScript != null)
                    {
                        playerMovementScript.canMove = true;
                    }
                }
                else
                {
                    // Obliczamy dodatkowe zwiększenie prędkości na podstawie czasu lotu
                    float flightDuration = Time.time - throwTime;
                    float currentReturnSpeed = returnSpeed + returnSpeedMultiplier * flightDuration;

                    // Przyciąganie – nadaj velocity w stronę gracza z dynamicznie zwiększoną prędkością
                    Vector2 pullDir = directionToPlayer.normalized;
                    torchRb.velocity = pullDir * currentReturnSpeed;

                    // Wyłączenie kolizji w dziecku "Square" (jeśli istnieje)
                    Transform squareTransform = currentTorch.transform.Find("Square");
                    if (squareTransform != null)
                    {
                        BoxCollider2D squareCollider = squareTransform.GetComponent<BoxCollider2D>();
                        if (squareCollider != null)
                        {
                            squareCollider.enabled = false;
                        }
                    }
                }
            }
        }
    }

    // ------------------- A) Trzymanie pochodni w ręce (PPM) ------------------- //
    private void HandleRightMouseHold()
    {
        // Naciśnięcie PPM – stworzenie wizualnej pochodni
        if (Input.GetKeyDown(torchHoldButton))
        {
            // Jeśli już jest wyrzucona (lub ładowana) pochodnia, blokujemy trzymanie
            if (currentTorch != null) return;

            if (holdTorch == null)
            {
                // Tworzymy pochodnię bez ustawiania jej jako dziecko gracza (rodzic = null)
                holdTorch = Instantiate(torchPrefab, spawnPoint.position, Quaternion.identity);
                holdTorch.transform.parent = null;
                // Wyłącz fizykę i kolizje, bo chcemy tylko wizualnie trzymać
                Rigidbody2D rb = holdTorch.GetComponent<Rigidbody2D>();
                if (rb != null) rb.isKinematic = true;
                Collider2D col = holdTorch.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }

        // Podczas przytrzymania PPM – aktualizacja pozycji
        if (Input.GetKey(torchHoldButton))
        {
            if (holdTorch != null)
            {
                Vector2 basePos = spawnPoint.position;
                holdTorch.transform.position = basePos + holdTorchOffset;
            }
        }

        // Puszczenie PPM – niszczenie wizualnej pochodni
        if (Input.GetKeyUp(torchHoldButton))
        {
            if (holdTorch != null)
            {
                Destroy(holdTorch);
                holdTorch = null;
            }
        }
    }

    // ------------------- B) Ładowanie, rzut oraz przyciąganie pochodni (klawisz E) ------------------- //
    private void HandleTorchThrowAndReturn()
    {
        // Na początku ładowania (KeyDown)
        if (Input.GetKeyDown(throwKey))
        {
            // Jeśli już jest wyrzucona pochodnia, zamiast ładowania wykonujemy przyciąganie
            if (currentTorch != null)
            {
                // W tym momencie przytrzymanie E nie inicjuje ponownie ładowania
            }
            else
            {
                // Tworzymy pochodnię już na początku ładowania – instancjujemy ją globalnie (parent = null)
                currentTorch = Instantiate(torchPrefab, spawnPoint.position, Quaternion.identity);
                currentTorch.transform.parent = null;
                // Ustawiamy, że na czas ładowania pochodnia nie podlega fizyce
                Rigidbody2D torchRb = currentTorch.GetComponent<Rigidbody2D>();
                if (torchRb != null)
                {
                    torchRb.isKinematic = true;
                }
                // Rozpoczynamy ładowanie
                isCharging = true;
                chargeTimer = 0f;
                if (throwSlider != null)
                {
                    throwSlider.value = 0f;
                }
            }
        }

        // Podczas trzymania E – ładowanie i aktualizacja slidera
        if (Input.GetKey(throwKey) && isCharging)
        {
            chargeTimer += Time.deltaTime;
            float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);
            if (throwSlider != null)
            {
                throwSlider.value = chargeRatio;
            }
        }

        // Zwolnienie E – wykonanie rzutu lub przyciągnięcia
        if (Input.GetKeyUp(throwKey))
        {
            if (isCharging && currentTorch != null)
            {
                float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);
                float appliedForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRatio);

                // Przywracamy fizykę pochodni i nadajemy jej prędkość
                Rigidbody2D torchRb = currentTorch.GetComponent<Rigidbody2D>();
                if (torchRb != null)
                {
                    torchRb.isKinematic = false;
                    // Obliczenie kierunku rzutu – na podstawie pozycji myszy
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = spawnPoint.position.z;
                    Vector2 throwDir = (mousePos - spawnPoint.position).normalized;

                    torchRb.velocity = throwDir * appliedForce;
                    // Rejestrujemy moment rzutu, by później zwiększyć prędkość przyciągania
                    throwTime = Time.time;
                }
            }
            else if (currentTorch != null)
            {
                // Jeśli pochodnia już istnieje (a nie jest ładowana), uruchamiamy przyciąganie
                isReturning = true;
            }
            isCharging = false;
            chargeTimer = 0f;
            if (throwSlider != null)
            {
                throwSlider.value = 0f;
            }
        }
    }

    // ------------------- C) Aktualizacja liny (wizualnie) ------------------- //
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
