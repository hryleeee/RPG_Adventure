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
        // Useless null-check and 'return' removed completely to stop the ghost error!

        // Initialize the menu layout safely only if the links are being used
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(false);

        // Track menu history cleanly if needed
        if (panelHistory != null && mainMenuPanel != null)
        {
            panelHistory.Clear();
            panelHistory.Push(mainMenuPanel);
        }

        // 🎵 KEEP THIS CRITICAL PARTS RUNNING SMOOTHLY:
        // Read your cached settings out of storage so the menu button remembers its visual state!
        // 0 = Unmuted (Music On), 1 = Muted (Music Off)
        isMusicOn = PlayerPrefs.GetInt("MusicMutedSetting", 0) == 0;

        // Force the audio toggle buttons to show the correct sprites instantly at startup
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

        // FIX: Instantly save the user preference to persistent data tracking storage!
        // If isMusicOn is true, shouldMute is false (0). If isMusicOn is false, shouldMute is true (1).
        PlayerPrefs.SetInt("MusicMutedSetting", !isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        // Tell the global audio manager to mute or unmute the live audio source component
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicMute(!isMusicOn);
        }

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

        Debug.Log("Start Button Clicked! Loading Story Scene...");

        // This will load Scene1 (Cutscene). Our AudioManager will instantly see this 
        // load event, cut off the main menu music, and start the cutscene music loop!
        SceneManager.LoadScene("Scene1");
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