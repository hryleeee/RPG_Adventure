using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // REQUIRED: Tells the script to use the modern input package!

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Drag your PauseMenuPanel container GameObject here")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Drag your unique Pause Options Menu layout container here (if you have one)")]
    [SerializeField] private GameObject pauseSettingsPanel;

    private bool isPaused = false;

    private void Start()
    {
        // Enforce a completely clean scene canvas baseline on level boot
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (pauseSettingsPanel != null) pauseSettingsPanel.SetActive(false);

        Time.timeScale = 1f; // Standard time flow speed
    }

    private void Update()
    {
        // Pure Unity 6 New Input System syntax - This will NEVER throw an InvalidOperationException!
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (pauseSettingsPanel != null && pauseSettingsPanel.activeSelf)
            {
                ClosePauseSettings();
            }
            else if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (pauseSettingsPanel != null) pauseSettingsPanel.SetActive(false);

        Time.timeScale = 1f; // Resume normal gameplay timing
        isPaused = false;
    }

    public void Pause()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze all physics updates and character movements
        isPaused = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // CRITICAL: Always unfreeze time BEFORE reloading!
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenPauseSettings()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (pauseSettingsPanel != null) pauseSettingsPanel.SetActive(true); // <-- Looking for this!
    }

    public void ClosePauseSettings()
    {
        if (pauseSettingsPanel != null) pauseSettingsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f; // CRITICAL: Always unfreeze time BEFORE scene changing!
        SceneManager.LoadScene("GameMenu");
    }
}