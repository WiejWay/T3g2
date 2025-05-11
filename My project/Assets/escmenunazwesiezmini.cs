using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject menuPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuPanel.activeSelf)
            {
                menuPanel.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                menuPanel.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }
}
