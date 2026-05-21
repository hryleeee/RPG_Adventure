using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required to talk to the Image component!

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject helpPanel; // Added for your new Help screen!

    [Header("Music Settings")]
    [SerializeField] private Image musicButtonImage; // Drag your MusicButton's Image component here
    [SerializeField] private Sprite musicOnSprite;    // Your default music note sprite
    [SerializeField] private Sprite musicOffSprite;   // Your muted/disabled music note sprite
    private bool isMusicOn = true;

    [Header("Sound Settings")]
    [SerializeField] private Image soundButtonImage; // Drag your SoundButton's Image component here
    [SerializeField] private Sprite soundOnSprite;    // Your default sound icon sprite
    [SerializeField] private Sprite soundOffSprite;   // Your muted/disabled sound icon sprite
    private bool isSoundOn = true;

    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    private void Start()
    {
        // Safety check to ensure all primary panels are assigned in the inspector
        if (mainMenuPanel == null || settingsPanel == null || helpPanel == null)
        {
            Debug.LogError("Please assign your MainMenuPanel, SettingsPanel, and HelpPanel in the Inspector!");
            return;
        }

        // Initialize the menu layout: Show Main Menu, Hide everything else
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        helpPanel.SetActive(false); // Keeps the help screen hidden at startup

        // Clear history and push the main menu as the starting location
        panelHistory.Clear();
        panelHistory.Push(mainMenuPanel);

        // Make sure the audio buttons show the correct sprites when the game starts
        UpdateMusicButtonUI();
        UpdateSoundButtonUI();
    }

    /// <summary>
    /// Opens a new panel and safely hides the current active panel.
    /// Pass your HelpPanel or SettingsPanel directly into this via the button in Unity.
    /// </summary>
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

    /// <summary>
    /// Closes the current panel and returns to the previous location in history.
    /// Attach this function to the "LeftButton" inside BOTH your Settings and Help panels.
    /// </summary>
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

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        Debug.Log(isMusicOn ? "Music Status: ON" : "Music Status: OFF");
        UpdateMusicButtonUI();
    }

    private void UpdateMusicButtonUI()
    {
        if (musicButtonImage == null || musicOnSprite == null || musicOffSprite == null) return;
        musicButtonImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        Debug.Log(isSoundOn ? "Sound Status: ON" : "Sound Status: OFF");
        UpdateSoundButtonUI();
    }

    private void UpdateSoundButtonUI()
    {
        if (soundButtonImage == null || soundOnSprite == null || soundOffSprite == null) return;
        soundButtonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }

    public void PlayGame()
    {
        PlayerHealth.SavedHealth = -1;

        Debug.Log("Start Button Clicked! Loading LEVEL1... Adventure awaits!");
        SceneManager.LoadScene("LEVEL 1");
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