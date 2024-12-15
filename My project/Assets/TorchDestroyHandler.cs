using UnityEngine;

public class TorchDestroyHandler : MonoBehaviour
{
    public System.Action OnTorchDestroyed; // Akcja wywoływana, gdy pochodnia zostaje zniszczona

    private void OnDestroy()
    {
        // Wywołanie akcji przy zniszczeniu obiektu
        OnTorchDestroyed?.Invoke();
    }
}
