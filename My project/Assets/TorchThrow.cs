using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TorchThrow : MonoBehaviour
{
    [Header("Pochodnia")]
    [SerializeField] private GameObject torchPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float maxChargeTime = 2f;

    [Header("Paliwo")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float fuelRegenRate = 20f;
    [SerializeField] private float fuelConsumptionRate = 20f;
    [SerializeField] private float maxLightDuration = 5f; // 100 paliwa = 5s światła
    [SerializeField] private float regenDelay = 1f;

    private float currentFuel;
    private bool canThrow = true;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private GameObject activeTorch;
    private bool isHolding = false;
    private GameObject holdTorch;
    private float regenTimer = 0f;

    [Header("Input")]
    [SerializeField] private KeyCode throwKey = KeyCode.E;
    [SerializeField] private KeyCode holdKey = KeyCode.Mouse1;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private Slider chargeSlider;

    void Start()
    {
        currentFuel = maxFuel;
        UpdateFuelUI();

        if (chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(false);
            chargeSlider.minValue = 0f;
            chargeSlider.maxValue = 1f;
        }
    }

    void Update()
    {
        HandleThrowInput();
        HandleHoldLight();

        // Opóźniona regeneracja paliwa
        if (!isHolding && activeTorch == null && regenTimer <= 0f && currentFuel < maxFuel)
        {
            currentFuel += fuelRegenRate * Time.deltaTime;
            currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
            UpdateFuelUI();
        }

        if (regenTimer > 0f)
            regenTimer -= Time.deltaTime;
    }

    void HandleThrowInput()
    {
        if (Input.GetKeyDown(throwKey) && canThrow && currentFuel >= 5f)
        {
            isCharging = true;
            chargeTimer = 0f;

            if (chargeSlider != null)
            {
                chargeSlider.value = 0f;
                chargeSlider.gameObject.SetActive(true);
            }
        }

        if (Input.GetKey(throwKey) && isCharging)
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, maxChargeTime);

            if (chargeSlider != null)
                chargeSlider.value = chargeTimer / maxChargeTime;
        }

        if (Input.GetKeyUp(throwKey) && isCharging)
        {
            isCharging = false;
            if (chargeSlider != null) chargeSlider.gameObject.SetActive(false);

            ThrowTorch();
        }
    }

    void ThrowTorch()
    {
        float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);
        float appliedForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRatio);

        float duration = currentFuel / (maxFuel / maxLightDuration);
        if (duration <= 0f) return;

        GameObject torch = Instantiate(torchPrefab, spawnPoint.position, Quaternion.identity);
        activeTorch = torch;

        Rigidbody2D rb = torch.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = spawnPoint.position.z;
            Vector2 dir = (mousePos - spawnPoint.position).normalized;
            rb.velocity = dir * appliedForce;
        }

        // 🎯 Wskaźnik czasu życia na prefabie pochodni
        Slider torchSlider = torch.GetComponentInChildren<Slider>(true);
        if (torchSlider != null)
        {
            torchSlider.gameObject.SetActive(true);
            StartCoroutine(UpdateTorchSlider(torchSlider, duration));
        }

        currentFuel = 0f;
        UpdateFuelUI();
        canThrow = false;
        regenTimer = regenDelay;

        StartCoroutine(TorchLifetime(torch, duration));
    }

    IEnumerator TorchLifetime(GameObject torch, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (torch != null)
        {
            float shrinkTime = 0.5f;
            float timer = 0f;
            Vector3 originalScale = torch.transform.localScale;

            while (timer < shrinkTime)
            {
                torch.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, timer / shrinkTime);
                timer += Time.deltaTime;
                yield return null;
            }

            Destroy(torch);
            activeTorch = null;
            canThrow = true;
        }

        regenTimer = regenDelay;
    }

    IEnumerator UpdateTorchSlider(Slider slider, float duration)
    {
        float timer = 0f;
        slider.maxValue = duration;
        slider.value = duration;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            slider.value = duration - timer;
            yield return null;
        }
    }

    void HandleHoldLight()
    {
        if (Input.GetKeyDown(holdKey))
        {
            if (holdTorch == null && currentFuel > 0f)
            {
                holdTorch = Instantiate(torchPrefab, spawnPoint.position, Quaternion.identity);
                Rigidbody2D rb = holdTorch.GetComponent<Rigidbody2D>();
                if (rb != null) rb.isKinematic = true;

                Collider2D col = holdTorch.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                isHolding = true;
            }
        }

        if (Input.GetKey(holdKey) && isHolding && currentFuel > 0f)
        {
            holdTorch.transform.position = spawnPoint.position;
            currentFuel -= fuelConsumptionRate * Time.deltaTime;
            currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
            UpdateFuelUI();

            if (currentFuel <= 0f)
            {
                Destroy(holdTorch);
                isHolding = false;
                regenTimer = regenDelay;
            }
        }

        if (Input.GetKeyUp(holdKey) && isHolding)
        {
            if (holdTorch != null)
                Destroy(holdTorch);

            isHolding = false;
            regenTimer = regenDelay;
        }
    }

    void UpdateFuelUI()
    {
        if (fuelText != null)
        {
            fuelText.text = $"Paliwo: {Mathf.FloorToInt(currentFuel)}";
        }
    }
}
