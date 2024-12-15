using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public List<GameObject> roomPrefabs; // Lista prefabów do losowego wyboru
    public GameObject checkpointPrefab; // Prefab checkpointu
    public GameObject startRoom; // Początkowy pokój
    public int totalRooms = 80; // Łączna liczba pokojów do wygenerowania
    public List<int> checkpointIndices = new List<int> { 5, 10, 20 }; // Indeksy pokojów, na których mają być checkpointy
    public bool hasMadeChoiceInCheckpoint = false; // Flaga określająca, czy wybór został dokonany
    public bool isWaitingForInput = false; // Czy czekamy na wybór trasy
   
    private GameObject lastRoom; // Ostatnio wygenerowany pokój
    private int roomCount = 0; // Licznik wygenerowanych pokoi
    private bool isPlayerInCheckpoint = false; // Czy gracz jest w checkpointcie
    private List<Transform> availableEndpoints = new List<Transform>(); // Lista dostępnych punktów końcowych

    void Start()
    {
        if (checkpointIndices.Count == 0)
        {
            Debug.LogError("Checkpoint indices are empty!");
            return;
        }

        lastRoom = startRoom; // Pierwszy pokój to pokój startowy
        GenerateRooms(totalRooms); // Wygeneruj wszystkie pokoje
    }

    public void GenerateRooms(int count)
    {
        while (count > 0)
        {
            if (isWaitingForInput) return;

            if (checkpointIndices.Contains(roomCount))
            {
                GenerateCheckpoint();
            }
            else
            {
                GenerateNextRoom();
            }

            count--;
        }
    }

    public void GenerateNextRoom()
    {
        // Znajdź `EndPoint` w ostatnim pokoju
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
            Debug.LogError("No valid EndPoint found in the last room!");
        }
    }

    public void GenerateNextRoomMatchingEndpoint(string direction, Transform selectedEndpoint)
    {
        // Filtruj pokoje, które mają odpowiedni StartPoint (Horizontal lub Vertical)
        List<GameObject> matchingRooms = roomPrefabs.FindAll(room =>
            room.transform.Find($"StartPoint{direction}") != null);

        if (matchingRooms.Count == 0)
        {
            Debug.LogError($"No rooms with StartPoint{direction} available!");
            return;
        }

        // Wybierz losowy prefab z pasujących
        GameObject newRoomPrefab = matchingRooms[Random.Range(0, matchingRooms.Count)];

        // Utwórz nowy pokój
        GameObject newRoom = Instantiate(newRoomPrefab);

        // Znajdź StartPoint w nowym pokoju
        Transform startPoint = newRoom.transform.Find($"StartPoint{direction}");
        if (startPoint == null)
        {
            Debug.LogError($"StartPoint{direction} not found in the new room: {newRoomPrefab.name}");
            Destroy(newRoom);
            return;
        }

        // Przesuń nowy pokój tak, aby jego StartPoint stykał się z wybranym EndPoint
        Vector3 offset = selectedEndpoint.position - startPoint.position;
        newRoom.transform.position += offset;

        // Zaktualizuj ostatni pokój
        lastRoom = newRoom;
        roomCount++;
    }


    public void GenerateCheckpoint()
    {
        availableEndpoints.Clear();
        hasMadeChoiceInCheckpoint = false; // Resetuj flagę przy generowaniu nowego checkpointu

        // Utwórz checkpoint
        GameObject checkpoint = Instantiate(checkpointPrefab);

        // Znajdź wszystkie punkty końcowe w checkpointcie
        foreach (Transform child in checkpoint.transform)
        {
            if (child.name.StartsWith("EndPointHorizontal") || child.name.StartsWith("EndPointVertical"))
            {
                availableEndpoints.Add(child);
            }
        }

        if (availableEndpoints.Count < 2)
        {
            Debug.LogError("Checkpoint should have at least two EndPoints!");
            Destroy(checkpoint);
            return;
        }

        // Znajdź StartPoint w checkpointcie
        Transform startPointHorizontal = checkpoint.transform.Find("StartPointHorizontal");
        Transform startPointVertical = checkpoint.transform.Find("StartPointVertical");

        Transform endPointHorizontal = lastRoom.transform.Find("EndPointHorizontal");
        Transform endPointVertical = lastRoom.transform.Find("EndPointVertical");

        // Dopasuj typy punktów (Horizontal lub Vertical)
        if (endPointHorizontal != null && startPointHorizontal != null)
        {
            Vector3 offset = endPointHorizontal.position - startPointHorizontal.position;
            checkpoint.transform.position += offset;
        }
        else if (endPointVertical != null && startPointVertical != null)
        {
            Vector3 offset = endPointVertical.position - startPointVertical.position;
            checkpoint.transform.position += offset;
        }
        else
        {
            Debug.LogError("No matching StartPoint and EndPoint found between last room and checkpoint!");
            Destroy(checkpoint);
            return;
        }

        // Dodaj komponent triggera do checkpointu
        checkpoint.AddComponent<CheckpointTrigger>().roomGenerator = this;

        // Zaktualizuj ostatni pokój
        lastRoom = checkpoint;
        roomCount++;
        isWaitingForInput = true; // Czekamy na wybór
    }
    public void ActivateCheckpointGUI()
    {
        isPlayerInCheckpoint = true;
    }

    public void DeactivateCheckpointGUI()
    {
        isPlayerInCheckpoint = false;
    }


    private void OnGUI()
    {
        // GUI jest widoczne tylko, gdy gracz jest w checkpoint i czekamy na wybór, ale nie dokonano wyboru
        if (!isPlayerInCheckpoint || !isWaitingForInput || hasMadeChoiceInCheckpoint) return;

        for (int i = 0; i < availableEndpoints.Count; i++)
        {
            Transform endpoint = availableEndpoints[i];
            if (GUI.Button(new Rect(10, 50 + i * 30, 200, 20), $"Choose Path {i + 1}"))
            {
                string direction = endpoint.name.Contains("Horizontal") ? "Horizontal" : "Vertical";
                GenerateNextRoomMatchingEndpoint(direction, endpoint);

                // Ustaw flagi po dokonaniu wyboru
                hasMadeChoiceInCheckpoint = true;
                isWaitingForInput = false;

                // Kontynuuj generowanie reszty pokoi
                GenerateRooms(totalRooms - roomCount);
            }
        }
    }


}














