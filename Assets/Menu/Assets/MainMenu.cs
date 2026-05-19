using UnityEngine;
using System.Collections.Generic; // Required to use the Stacks history system
using UnityEngine.SceneManagement; // Required if you want to load game levels

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    // You can easily add more panels here later, like:
    // [SerializeField] private GameObject storePanel;

    // This stack keeps track of the history of screens the player visits
    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    private void Start()
    {
        // Safety check to ensure the panels are assigned in the inspector
        if (mainMenuPanel == null || settingsPanel == null)
        {
            Debug.LogError("Please assign your MainMenuPanel and SettingsPanel in the Inspector!");
            return;
        }

        // Initialize the menu layout: Show Main Menu, Hide Settings
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        // Clear history and push the main menu as the starting location
        panelHistory.Clear();
        panelHistory.Push(mainMenuPanel);
    }

    /// <summary>
    /// Opens a new panel and safely hides the current active panel.
    /// </summary>
    public void OpenPanel(GameObject newPanel)
    {
        if (newPanel == null) return;

        // Hide the current active panel if there is one in history
        if (panelHistory.Count > 0)
        {
            panelHistory.Peek().SetActive(false);
        }

        // Open the new panel and add it to our tracking history
        newPanel.SetActive(true);
        panelHistory.Push(newPanel);
    }

    /// <summary>
    /// Closes the current panel and returns to the previous location in history.
    /// Attach this function to your "LeftButton" (Back Button).
    /// </summary>
    public void GoBack()
    {
        // We need at least 2 panels in history to go back (the current one and the previous one)
        if (panelHistory.Count > 1)
        {
            // Remove the current panel from the stack and turn it off
            GameObject currentPanel = panelHistory.Pop();
            currentPanel.SetActive(false);

            // Look at the panel right underneath it in history and turn it back on
            GameObject previousPanel = panelHistory.Peek();
            previousPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No previous panels left in the navigation history!");
        }
    }

    /// <summary>
    /// Call this function to start the actual game.
    /// </summary>
    public void PlayGame()
    {
        Debug.Log("Start Button Clicked! Loading your game scene...");
        // Swap "GameScene" with the exact name of your playable level scene
        // SceneManager.LoadScene("GameScene"); 
    }

    /// <summary>
    /// Call this function to close the application.
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Exit Button Clicked! Closing application.");
        Application.Quit();
    }
}