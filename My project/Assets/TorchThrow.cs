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

    // Maksymalna odległość pochodni od gracza, po której zacznie się automatyczny powrót
    private float maxDistanceFromPlayer = 20f;

    void Start()
    {
        // Na starcie ukrywamy slider
        if (throwSlider != null)
        {
            throwSlider.gameObject.SetActive(false);
        }
    }

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
        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = !isCharging;
        }

        // Jeśli pochodnia istnieje i nie jest jeszcze w trybie powrotu,
        // sprawdzamy, czy nie oddaliła się zbyt daleko od gracza
        if (currentTorch != null && !isReturning)
        {
            float distanceFromPlayer = Vector2.Distance(currentTorch.transform.position, spawnPoint.position);
            if (distanceFromPlayer >= maxDistanceFromPlayer)
            {
                isReturning = true;
            }
        }

        if (isReturning && currentTorch != null)
        {
            Rigidbody2D torchRb = currentTorch.GetComponent<Rigidbody2D>();
            if (torchRb != null)
            {
                Vector2 directionToPlayer = (spawnPoint.position - currentTorch.transform.position);
                float distance = directionToPlayer.magnitude;

                // Wyłączanie colliderów pochodni, gdy ta wraca
                Collider2D mainCollider = currentTorch.GetComponent<Collider2D>();
                if (mainCollider != null && mainCollider.enabled)
                {
                    mainCollider.enabled = false;
                }
                // Dodatkowo, jeśli pochodnia ma pod-obiekt "Circle" z dodatkowym colliderem:
                Transform circleTransform = currentTorch.transform.Find("Circle");
                if (circleTransform != null)
                {
                    BoxCollider2D circleCollider = circleTransform.GetComponent<BoxCollider2D>();
                    if (circleCollider != null && circleCollider.enabled)
                    {
                        circleCollider.enabled = false;
                    }
                }

                if (distance < 0.2f)
                {
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
                    float flightDuration = Time.time - throwTime;
                    float currentReturnSpeed = returnSpeed + returnSpeedMultiplier * flightDuration;
                    Vector2 pullDir = directionToPlayer.normalized;
                    torchRb.velocity = pullDir * currentReturnSpeed;
                }
            }
        }
    }

    // ------------------- A) Trzymanie pochodni w ręce (PPM) ------------------- //
    private void HandleRightMouseHold()
    {
        if (Input.GetKeyDown(torchHoldButton))
        {
            if (currentTorch != null) return;

            if (holdTorch == null)
            {
                holdTorch = Instantiate(torchPrefab, spawnPoint.position, Quaternion.identity);
                holdTorch.transform.parent = null;
                Rigidbody2D rb = holdTorch.GetComponent<Rigidbody2D>();
                if (rb != null) rb.isKinematic = true;
                Collider2D col = holdTorch.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }

        if (Input.GetKey(torchHoldButton))
        {
            if (holdTorch != null)
            {
                Vector2 basePos = spawnPoint.position;
                holdTorch.transform.position = basePos + holdTorchOffset;
            }
        }

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
        if (Input.GetKeyDown(throwKey))
        {
            if (currentTorch != null)
            {
                // Jeśli pochodnia już istnieje, przytrzymanie E nie inicjuje ładowania
            }
            else
            {
                currentTorch = Instantiate(torchPrefab, spawnPoint.position, Quaternion.identity);
                currentTorch.transform.parent = null;
                Rigidbody2D torchRb = currentTorch.GetComponent<Rigidbody2D>();
                if (torchRb != null)
                {
                    torchRb.isKinematic = true;
                }
                isCharging = true;
                chargeTimer = 0f;
                if (throwSlider != null)
                {
                    throwSlider.value = 0f;
                    throwSlider.gameObject.SetActive(true); // Pokazujemy slider podczas ładowania
                }
            }
        }

        if (Input.GetKey(throwKey) && isCharging)
        {
            chargeTimer += Time.deltaTime;
            float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);
            if (throwSlider != null)
            {
                throwSlider.value = chargeRatio;
            }
        }

        if (Input.GetKeyUp(throwKey))
        {
            if (isCharging && currentTorch != null)
            {
                float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);
                float appliedForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRatio);
                Rigidbody2D torchRb = currentTorch.GetComponent<Rigidbody2D>();
                if (torchRb != null)
                {
                    torchRb.isKinematic = false;
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = spawnPoint.position.z;
                    Vector2 throwDir = (mousePos - spawnPoint.position).normalized;
                    torchRb.velocity = throwDir * appliedForce;
                    throwTime = Time.time;
                }
            }
            else if (currentTorch != null)
            {
                isReturning = true;
            }
            isCharging = false;
            chargeTimer = 0f;
            if (throwSlider != null)
            {
                throwSlider.value = 0f;
                throwSlider.gameObject.SetActive(false); // Ukrywamy slider po zakończeniu ładowania/rzutu
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
