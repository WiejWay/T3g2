using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Torch : MonoBehaviour
{
    private LineRenderer line;
    private SpringJoint springJoint;

    void Awake()
    {
        // Pobierz (lub dołącz) komponent LineRenderer
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;   // Na start wyłączamy rysowanie
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        // Możesz ustawić materiał, kolor, teksturę itp.
        // line.material = ...
        // line.startColor = Color.yellow;
        // line.endColor = Color.red;
    }

    void Update()
    {
        // Znajdź SpringJoint (tylko raz)
        if (!springJoint)
        {
            springJoint = GetComponent<SpringJoint>();
        }

        // Jeśli jest joint i jest podpięty do ciała gracza, rysujemy linkę
        if (springJoint && springJoint.connectedBody != null)
        {
            line.positionCount = 2;

            // Punkt 0: pozycja pochodni
            line.SetPosition(0, transform.position);

            // Punkt 1: pozycja obiektu gracza (connectedBody)
            line.SetPosition(1, springJoint.connectedBody.transform.position);
        }
        else
        {
            // Gdy nie ma jointa – nic nie rysujemy
            line.positionCount = 0;
        }
    }
}
