using UnityEngine;
using System.Collections.Generic; // Required to use the Stacks history system
using UnityEngine.SceneManagement; // Required for loading scenes

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    private void Start()
    {
        if (mainMenuPanel == null || settingsPanel == null)
        {
            Debug.LogError("Please assign your MainMenuPanel and SettingsPanel in the Inspector!");
            return;
        }

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        panelHistory.Clear();
        panelHistory.Push(mainMenuPanel);
    }

    public void OpenPanel(GameObject newPanel)
    {
        if (newPanel == null) return;

        if (panelHistory.Count > 0)
        {
            panelHistory.Peek().SetActive(false);
        }

        newPanel.SetActive(true);
        panelHistory.Push(newPanel);
    }

    public void GoBack()
    {
        if (panelHistory.Count > 1)
        {
            GameObject currentPanel = panelHistory.Pop();
            currentPanel.SetActive(false);

            GameObject previousPanel = panelHistory.Peek();
            previousPanel.SetActive(true);
        }
    }

    public void PlayGame()
    {
        Debug.Log("Start Button Clicked! Loading LEVEL1... Adventure awaits!");
        SceneManager.LoadScene("LEVEL1");
    }

    public void ExitGame()
    {
        Debug.Log("Exit Button Clicked! Closing application.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
        Application.Quit();
#endif
    }
}