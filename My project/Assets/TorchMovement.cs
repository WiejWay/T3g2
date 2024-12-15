using UnityEngine;

public class TorchMovement : MonoBehaviour
{
    private Vector3 velocity;
    private float gravity;
    private Vector3 startPosition;
    private float elapsedTime;

    public void Initialize(Vector3 initialVelocity, float gravityValue, Vector3 startPos)
    {
        velocity = initialVelocity;
        gravity = gravityValue;
        startPosition = startPos;
        elapsedTime = 0f;
    }

    void Update()
    {
        if (velocity != Vector3.zero)
        {
            // Aktualizacja czasu
            elapsedTime += Time.deltaTime;

            // Obliczanie nowej pozycji
            Vector3 displacement = new Vector3(
                velocity.x * elapsedTime,
                velocity.y * elapsedTime + 0.5f * gravity * Mathf.Pow(elapsedTime, 2),
                0
            );

            transform.position = startPosition + displacement;

            // Wykrywanie kolizji z podłożem
            if (transform.position.y <= 0f)
            {
                transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
                velocity = Vector3.zero; // Zatrzymanie ruchu
            }
        }
    }
}
