using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeEndScene : MonoBehaviour
{
    public Dropdown sceneDropdown;
    public List<string> sceneNames;

  void Start()
    {
        if (sceneDropdown == null)
        {
            Debug.LogError("Dropdown reference is missing!");
            return;
        }

        PopulateDropdown();
    }

    void PopulateDropdown()
    {
        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(sceneNames);
    }

    public void LoadSelectedScene()
    {
        string selectedScene = sceneNames[sceneDropdown.value];

        if (!string.IsNullOrEmpty(selectedScene))
        {
            SceneManager.LoadScene(selectedScene);
        }
        else
        {
            Debug.LogWarning("Selected scene name is empty!");
        }
    }
}