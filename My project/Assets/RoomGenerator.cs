using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> roomPrefabs;     // Lista prefabów do losowego wyboru
    public GameObject checkpointPrefab;      // Prefab checkpointu
    public GameObject startRoom;             // Początkowy pokój
    public GameObject finalRoomPrefab;       // Prefab ostatniego pokoju (ma tylko StartPointHorizontal)

    [Header("Generation Settings")]
    public int totalRooms = 80;              // Łączna liczba pokojów do wygenerowania
    public List<int> checkpointIndices = new List<int> { 5, 10, 20 }; // Indeksy pokojów, w których wstawiamy checkpoint

    private GameObject lastRoom;             // Ostatnio wygenerowany pokój/checkpoint
    private int roomCount = 0;               // Licznik wygenerowanych pomieszczeń (pokojów + checkpointów)
    
    // Przechowuje prefab ostatnio użytego pokoju, żeby uniknąć powtórzeń
    private GameObject previousRoomPrefab;

    void Start()
    {
        // Ustawiamy pierwszy pokój jako ostatnio wygenerowany
        lastRoom = startRoom;
        roomCount++;

        // Generujemy kolejne pomieszczenia
        for (int i = 0; i < totalRooms; i++)
        {
            // Jeśli indeks pokoju jest na liście checkpointów – generujemy checkpoint.
            if (checkpointIndices.Contains(i))
            {
                GenerateCheckpoint();
            }
            else
            {
                GenerateNextRoom();
            }
        }

        // Po wygenerowaniu wszystkich pokoi, dodajemy final room jako dodatkowy pokój na końcu
        if (finalRoomPrefab != null)
        {
            GenerateFinalRoom();
        }
        else
        {
            Debug.LogError("FinalRoomPrefab nie został ustawiony!");
        }
    }

    /// <summary>
    /// Metoda generująca kolejny pokój.
    /// Sprawdza, który z endpointów (Horizontal/Vertical) występuje w ostatnim pokoju,
    /// a następnie wywołuje generowanie pokoju w oparciu o ten endpoint.
    /// </summary>
    private void GenerateNextRoom()
    {
        Transform endPointHorizontal = lastRoom.transform.Find("EndPointHorizontal");
        Transform endPointVertical = lastRoom.transform.Find("EndPointVertical");

        if (endPointHorizontal != null)
        {
            GenerateNextRoomMatchingEndpoint("Horizontal", endPointHorizontal);
        }
        else if (endPointVertical != null)
        {
            GenerateNextRoomMatchingEndpoint("Vertical", endPointVertical);
        }
        else
        {
            Debug.LogError("Nie znaleziono poprawnego EndPoint (Horizontal/Vertical) w ostatnim pokoju!");
        }
    }

    /// <summary>
    /// Generuje checkpoint, zakładając, że ostatni pokój posiada EndPointHorizontal.
    /// Jeśli nie, wypisuje błąd i nie generuje checkpointu.
    /// </summary>
    private void GenerateCheckpoint()
    {
        Transform endPointHorizontal = lastRoom.transform.Find("EndPointHorizontal");
        if (endPointHorizontal == null)
        {
            Debug.LogError("Nie można wygenerować checkpointu – ostatni pokój nie posiada EndPointHorizontal!");
            return;
        }

        GenerateNextCheckpointMatchingEndpoint("Horizontal", endPointHorizontal);
    }

    /// <summary>
    /// Oblicza offset, który należy dodać, aby nowy obiekt (pokój lub checkpoint) stykał się z poprzednim.
    /// Uwzględnia różnicę pozycji między endpointem a startpointem oraz dodatkowe przesunięcie wynikające z wielkości collidera endpointu.
    /// offsetMultiplier pozwala regulować dodatkowy offset (np. mniejszy dla checkpointu).
    /// </summary>
    private Vector3 CalculateOffset(Transform endpoint, Transform newStartPoint, string direction, float offsetMultiplier = 1f)
    {
        // Podstawowa różnica pozycji
        Vector3 offset = endpoint.position - newStartPoint.position;

        // Pobieramy BoxCollider z endpointu
        float additionalOffset = 0;
        BoxCollider col = endpoint.GetComponent<BoxCollider>();
        if (col != null)
        {
            // Używamy bounds.extents, aby uzyskać rzeczywisty wymiar od środka do krawędzi
            if (direction == "Horizontal")
            {
                additionalOffset = col.bounds.extents.x * 2;
                offset.x += additionalOffset;
            }
            else if (direction == "Vertical")
            {
                additionalOffset = col.bounds.extents.y * 2;
                offset.y += additionalOffset;
            }
        }
        else
        {
            Debug.LogWarning("Nie znaleziono BoxCollidera na endpoint. Dodatkowy offset nie zostanie dodany.");
        }
        Debug.Log($"CalculateOffset ({direction}): Podstawowy offset: {endpoint.position - newStartPoint.position}, additionalOffset: {additionalOffset}");
        return offset;
    }

    /// <summary>
    /// Generuje kolejny zwykły pokój, ustawiając go na podstawie endpointu poprzedniego pokoju.
    /// Dla zwykłych pokoi offsetMultiplier wynosi 1.
    /// Dodatkowo upewnia się, że nie zostanie wybrany ten sam prefabrykat co poprzednio.
    /// </summary>
    private void GenerateNextRoomMatchingEndpoint(string direction, Transform selectedEndpoint)
    {
        List<GameObject> matchingRooms = roomPrefabs.FindAll(room =>
            room.transform.Find($"StartPoint{direction}") != null);

        if (matchingRooms.Count == 0)
        {
            Debug.LogError($"Brak pokoju z StartPoint{direction} w dostępnych prefabach!");
            return;
        }

        // Jeśli dostępnych jest więcej niż jeden prefab, usuń ostatnio użyty prefab, by uniknąć powtórzeń.
        if (previousRoomPrefab != null && matchingRooms.Count > 1)
        {
            matchingRooms = matchingRooms.FindAll(room => room != previousRoomPrefab);
        }

        GameObject newRoomPrefab = matchingRooms[Random.Range(0, matchingRooms.Count)];
        GameObject newRoom = Instantiate(newRoomPrefab);

        Transform startPoint = newRoom.transform.Find($"StartPoint{direction}");
        if (startPoint == null)
        {
            Debug.LogError($"StartPoint{direction} nie znaleziony w {newRoomPrefab.name}!");
            Destroy(newRoom);
            return;
        }

        // Dla pierwszego pokoju względem startRoom sprawdzamy, czy lastRoom (startRoom) posiada odpowiedni endpoint
        Vector3 offset = CalculateOffset(selectedEndpoint, startPoint, direction, 1f);
        newRoom.transform.position += offset;

        Debug.Log($"Nowy pokój {newRoom.name} ustawiony na pozycji: {newRoom.transform.position}");

        lastRoom = newRoom;
        previousRoomPrefab = newRoomPrefab;
        roomCount++;
    }

    /// <summary>
    /// Generuje checkpoint – używa CalculateOffset z możliwością modyfikacji offsetMultiplier.
    /// Możesz zmienić offsetMultiplier, aby uzyskać mniejszy lub większy offset przed checkpointem.
    /// </summary>
    private void GenerateNextCheckpointMatchingEndpoint(string direction, Transform selectedEndpoint)
    {
        GameObject newCheckpoint = Instantiate(checkpointPrefab);

        Transform startPoint = newCheckpoint.transform.Find($"StartPoint{direction}");
        if (startPoint == null)
        {
            Debug.LogError($"StartPoint{direction} nie znaleziony w checkpoint {checkpointPrefab.name}!");
            Destroy(newCheckpoint);
            return;
        }

        // Możesz eksperymentować z offsetMultiplier – poniżej przykład z wartością 1f
        Vector3 offset = CalculateOffset(selectedEndpoint, startPoint, direction, 1f);
        newCheckpoint.transform.position += offset;

        Debug.Log($"Checkpoint {newCheckpoint.name} ustawiony na pozycji: {newCheckpoint.transform.position}");

        lastRoom = newCheckpoint;
        roomCount++;
    }

    /// <summary>
    /// Generuje final room – ustawiany względem EndPointHorizontal ostatniego pokoju.
    /// Jeśli endpoint nie jest znaleziony, final room jest ustawiany względem pozycji ostatniego pokoju.
    /// </summary>
    private void GenerateFinalRoom()
    {
        GameObject finalRoom = Instantiate(finalRoomPrefab);

        Transform startPointHorizontal = finalRoom.transform.Find("StartPointHorizontal");
        if (startPointHorizontal == null)
        {
            Debug.LogError($"StartPointHorizontal nie znaleziony w final room {finalRoomPrefab.name}!");
            Destroy(finalRoom);
            return;
        }

        Transform endPointHorizontal = lastRoom.transform.Find("EndPointHorizontal");

        Vector3 newPosition = finalRoom.transform.position;
        if (endPointHorizontal != null)
        {
            Vector3 offset = CalculateOffset(endPointHorizontal, startPointHorizontal, "Horizontal", 1f);
            newPosition += offset;
        }
        else
        {
            Debug.LogWarning("Brak EndPointHorizontal w ostatnim pokoju. Final room ustawiony względem pozycji ostatniego pokoju.");
            newPosition = lastRoom.transform.position;
        }

        finalRoom.transform.position = newPosition;
        Debug.Log($"Final room {finalRoom.name} ustawiony na pozycji: {finalRoom.transform.position}");

        lastRoom = finalRoom;
        roomCount++;
    }
}
