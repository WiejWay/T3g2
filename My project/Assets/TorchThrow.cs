using UnityEngine;

public class TorchThrow : MonoBehaviour
{
    public float throwForce = 10f;  // Siła rzutu
    public float angle = 45f;      // Kąt rzutu w stopniach
    public GameObject torchPrefab; // Prefab pochodni
    public Transform throwPoint;   // Punkt startowy rzutu

    private GameObject currentTorch; // Referencja do aktywnej pochodni
    private float gravity = -9.8f;

    void Update()
    {
        // Rzut pochodnią pod klawiszem E
        if (Input.GetKeyDown(KeyCode.E))
        {
            ThrowTorch();
        }
    }

    void ThrowTorch()
    {
        if (torchPrefab == null || throwPoint == null)
        {
            Debug.LogError("TorchPrefab or ThrowPoint is not assigned in the Inspector.");
            return;
        }

        // Jeśli istnieje już pochodnia, usuń ją
        if (currentTorch != null)
        {
            Destroy(currentTorch);
        }

        // Tworzenie nowej pochodni
        currentTorch = Instantiate(torchPrefab, throwPoint.position, Quaternion.identity);

        // Obliczanie prędkości rzutu
        float radianAngle = angle * Mathf.Deg2Rad;
        Vector3 velocity = new Vector3(
            throwForce * Mathf.Cos(radianAngle),
            throwForce * Mathf.Sin(radianAngle),
            0
        );

        // Inicjalizacja ruchu pochodni
        var torchMovement = currentTorch.GetComponent<TorchMovement>();
        if (torchMovement != null)
        {
            torchMovement.Initialize(velocity, gravity, throwPoint.position);
        }
        else
        {
            Debug.LogError("Torch prefab is missing the TorchMovement component.");
        }
    }
}
