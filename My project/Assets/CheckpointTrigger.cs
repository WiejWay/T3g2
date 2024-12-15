



using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    public RoomGenerator roomGenerator;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            roomGenerator.ActivateCheckpointGUI();
            Debug.Log("Player entered checkpoint");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            roomGenerator.DeactivateCheckpointGUI();
            Debug.Log("Player exited checkpoint");
        }
    }
}