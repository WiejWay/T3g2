using UnityEngine;

public class RopeGenerator : MonoBehaviour
{
    [Header("Segment linki")]
    public GameObject ropeSegmentPrefab;   // Prefab segmentu (musi mieć Rigidbody2D + HingeJoint2D)
    public int segmentCount = 5;           // Ilu członów ma mieć linka
    public float segmentSpacing = 0.2f;    // Odstęp między segmentami

    [Header("Punkt zaczepu")]
    public Transform startPoint;           // Gdzie zaczyna się linka (np. sufit)
    public Rigidbody2D startRb;            // Opcjonalnie, jeżeli chcesz się zaczepić do ciała RigidBody

    private GameObject[] segments;         // Tablica referencji do wygenerowanych segmentów

    void Start()
    {
        GenerateRope();
    }

    void GenerateRope()
    {
        // Tablica na segmenty
        segments = new GameObject[segmentCount];

        // Poprzedni rigidbody (na początku to startRb)
        Rigidbody2D previousRb = startRb;

        // Pozycja startowa
        Vector2 spawnPos = startPoint.position;

        for (int i = 0; i < segmentCount; i++)
        {
            // Tworzymy segment linki
            GameObject newSegment = Instantiate(ropeSegmentPrefab, spawnPos, Quaternion.identity);
            segments[i] = newSegment;

            // Ustawiamy nazwę w Hierarchy, żeby było wiadomo co jest czym
            newSegment.name = "RopeSegment_" + i;

            // Pobieramy rigidbody segmentu
            Rigidbody2D rb = newSegment.GetComponent<Rigidbody2D>();
            HingeJoint2D joint = newSegment.GetComponent<HingeJoint2D>();

            if (joint != null)
            {
                // Podpinamy do poprzedniego segmentu (albo do startRb, jeżeli i=0)
                joint.connectedBody = previousRb;

                // Ustawiamy anchor tak, by segment łączył się z poprzednim na końcu
                // w zależności od wielkości collidery segmentu, trzeba korygować anchor
                // Tu zakładam, że anchor jest lokalnie (0, +0.1f) a connectedAnchor = (0, -0.1f) itp.
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = new Vector2(0f, 0.1f);
                joint.connectedAnchor = new Vector2(0f, -0.1f);
            }

            // Aktualizujemy spawnPos na kolejny segment
            // "segmentSpacing" to odległość między środkami segmentów
            spawnPos.y -= segmentSpacing;

            // Ustawiamy poprzedniRb na obecny
            previousRb = rb;
        }
    }
}
