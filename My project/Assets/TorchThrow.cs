using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] private float fuelRegenRate = 60f;
    [SerializeField] private float fuelConsumptionRate = 20f;
    [SerializeField] private float maxLightDuration = 5f;
    [SerializeField] private float regenDelay = 2f;

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

    [Header("UI - Gracz")]
    [SerializeField] private Image playerFuelImage;

    [Header("UI - GUI Usera")]
    [SerializeField] private Image guiFuelImage;
    [SerializeField] private Slider guiFuelSlider;

    [Header("Ogień - Sprite'y")]
    [SerializeField] private Sprite blackFire;
    [SerializeField] private Sprite blackFire1;
    [SerializeField] private Sprite black2Fire;
    [SerializeField] private Sprite fullFire;

    [Header("UI - Ładowanie")]
    [SerializeField] private Slider chargeSlider;

    [Header("Kolory paliwa")]
    [SerializeField] private Color colorEmpty = Color.black;
    [SerializeField] private Color colorLow = Color.red;
    [SerializeField] private Color colorMedium = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color colorHigh = Color.yellow;

    private Image guiFuelFill;

    private bool isSimulatingFuel = false;
    private float guiSimulatedFuel = 0f;
    private float simulatedStartFuel = 0f;
    private float simulationTimer = 0f;
    private float simulationDuration = 0f;

    [System.Serializable]
    public class TorchFireVisual
    {
        public Image fireImage;
        public bool allowScaling;
        public Sprite blackFire;
        public Sprite blackFire1;
        public Sprite black2Fire;
        public Sprite fullFire;
    }

    private TorchFireVisual playerFireVisual;
    private TorchFireVisual guiFireVisual;

    void Start()
    {
        currentFuel = maxFuel;

        playerFireVisual = new TorchFireVisual
        {
            fireImage = playerFuelImage,
            allowScaling = true,
            blackFire = blackFire,
            blackFire1 = blackFire1,
            black2Fire = black2Fire,
            fullFire = fullFire
        };

        guiFireVisual = new TorchFireVisual
        {
            fireImage = guiFuelImage,
            allowScaling = false,
            blackFire = blackFire,
            blackFire1 = blackFire1,
            black2Fire = black2Fire,
            fullFire = fullFire
        };

        if (guiFuelSlider != null)
        {
            guiFuelSlider.minValue = 0f;
            guiFuelSlider.maxValue = maxFuel;
            guiFuelFill = guiFuelSlider.fillRect.GetComponent<Image>();
        }

        if (chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(false);
            chargeSlider.minValue = 0f;
            chargeSlider.maxValue = 1f;
        }

        UpdateFuelUI();
    }

    void Update()
    {
        HandleThrowInput();
        HandleHoldLight();

        if (!isHolding && activeTorch == null && regenTimer <= 0f && currentFuel < maxFuel)
        {
            currentFuel += fuelRegenRate * Time.deltaTime;
            currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
            UpdateFuelUI();
        }

        if (regenTimer > 0f)
            regenTimer -= Time.deltaTime;

        if (isSimulatingFuel)
        {
            simulationTimer += Time.deltaTime;
            guiSimulatedFuel = Mathf.Lerp(simulatedStartFuel, 0f, simulationTimer / simulationDuration);
            guiSimulatedFuel = Mathf.Clamp(guiSimulatedFuel, 0f, simulatedStartFuel);

            UpdateFuelUI();

            if (simulationTimer >= simulationDuration)
            {
                isSimulatingFuel = false;
                guiSimulatedFuel = 0f;
                UpdateFuelUI();
            }
        }
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

        Slider torchSlider = torch.GetComponentInChildren<Slider>(true);
        if (torchSlider != null)
        {
            torchSlider.gameObject.SetActive(true);
            StartCoroutine(UpdateTorchSlider(torchSlider, duration));
        }

        Image torchFireImage = torch.GetComponentInChildren<Image>(true);
        if (torchFireImage != null)
        {
            TorchFireVisual torchVisual = new TorchFireVisual
            {
                fireImage = torchFireImage,
                allowScaling = true,
                blackFire = blackFire,
                blackFire1 = blackFire1,
                black2Fire = black2Fire,
                fullFire = fullFire
            };

            StartCoroutine(UpdateTorchFireVisualOverTime(torchVisual, duration));
        }

        simulatedStartFuel = currentFuel;
        isSimulatingFuel = true;
        guiSimulatedFuel = simulatedStartFuel;
        simulationTimer = 0f;
        simulationDuration = duration;

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
        UpdateFireVisual(playerFireVisual, currentFuel, maxFuel);

        float guiDisplayFuel = isSimulatingFuel ? guiSimulatedFuel : currentFuel;
        UpdateFireVisual(guiFireVisual, guiDisplayFuel, maxFuel);

        if (guiFuelSlider != null)
        {
            guiFuelSlider.value = guiDisplayFuel;

            if (guiFuelFill != null)
            {
                float percent = guiDisplayFuel / maxFuel * 100f;

                if (percent == 0f)
                    guiFuelFill.color = colorEmpty;
                else if (percent <= 33f)
                    guiFuelFill.color = colorLow;
                else if (percent <= 80f)
                    guiFuelFill.color = colorMedium;
                else
                    guiFuelFill.color = colorHigh;
            }
        }
    }

    void UpdateFireVisual(TorchFireVisual visual, float current, float maximum)
    {
        if (visual.fireImage == null)
            return;

        float percent = current / maximum * 100f;

        if (percent <= 0f)
        {
            visual.fireImage.gameObject.SetActive(false);
        }
        else
        {
            visual.fireImage.gameObject.SetActive(true);

            if (percent <= 5f)
                visual.fireImage.sprite = visual.blackFire;
            else if (percent <= 33f)
                visual.fireImage.sprite = visual.blackFire1;
            else if (percent <= 80f)
                visual.fireImage.sprite = visual.black2Fire;
            else
                visual.fireImage.sprite = visual.fullFire;

            if (visual.allowScaling)
            {
                float targetScale = 0f;

                if (percent > 0f && percent <= 33f)
                    targetScale = Mathf.Lerp(0f, 0.7f, percent / 33f);
                else if (percent > 33f)
                    targetScale = Mathf.Lerp(0.7f, 1f, (percent - 33f) / 67f);

                visual.fireImage.transform.localScale = new Vector3(targetScale, targetScale, 1f);
            }
        }
    }

    IEnumerator UpdateTorchFireVisualOverTime(TorchFireVisual visual, float lifetime)
    {
        float timer = 0f;
        while (timer < lifetime)
        {
            timer += Time.deltaTime;
            float remainingFuel = Mathf.Lerp(maxFuel, 0f, timer / lifetime);
            UpdateFireVisual(visual, remainingFuel, maxFuel);
            yield return null;
        }

        if (visual.fireImage != null)
        {
            visual.fireImage.gameObject.SetActive(false);
        }
    }
}
