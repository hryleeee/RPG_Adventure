using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // CRUCIAL: Required to talk to the Image component!

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio Settings")]
    [SerializeField] private Image musicButtonImage; // Drag your MusicButton's Image component here
    [SerializeField] private Sprite musicOnSprite;   // Your default music note sprite
    [SerializeField] private Sprite musicOffSprite;  // Your muted/disabled music note sprite
    private bool isMusicOn = true;

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

        // Make sure the button shows the correct sprite when the game starts
        UpdateMusicButtonUI();
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;

        if (isMusicOn)
        {
            Debug.Log("?? Music Status: ON");
        }
        else
        {
            Debug.Log("?? Music Status: OFF");
        }

        // Change the picture based on the new true/false state
        UpdateMusicButtonUI();
    }

    private void UpdateMusicButtonUI()
    {
        if (musicButtonImage == null || musicOnSprite == null || musicOffSprite == null) return;

        // Swap the sprite depending on if music is on or off
        if (isMusicOn)
        {
            musicButtonImage.sprite = musicOnSprite;
        }
        else
        {
            musicButtonImage.sprite = musicOffSprite;
        }
    }

    // --- Keep the rest of your existing Panel, Play, and Exit code below here ---
    public void OpenPanel(GameObject newPanel)
    {
        if (newPanel == null) return;
        if (panelHistory.Count > 0) panelHistory.Peek().SetActive(false);
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

    public void PlayGame() => Debug.Log("Start Button Clicked!");
    public void ExitGame() => Application.Quit();
}