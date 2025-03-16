using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChangeSceneOnTrigger2D : MonoBehaviour
{
    [SerializeField] private SceneAsset sceneAsset;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (sceneAsset != null)
        {
            SceneManager.LoadScene(sceneAsset.name);
        }
    }
}
