using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private SceneAsset sceneAsset; 

    public void LoadScene()
    {
        if (sceneAsset != null)
        {
            string sceneName = sceneAsset.name;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Nie ustawiono sceny w edytorze!");
        }
    }
}
