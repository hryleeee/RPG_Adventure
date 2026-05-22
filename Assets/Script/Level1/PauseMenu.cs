using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Speaks natively to Unity 6 Input Package drivers

public class PauseMenu : MonoBehaviour
{
    [Header("UI HUD Interaction Buttons (Gameplay Screen Only)")]
    [Tooltip("Drag the Pause Button that sits on your live active gameplay HUD here. Leave empty if none.")]
    [SerializeField] private GameObject hudPauseButton;

    [Tooltip("Drag the Settings Button that sits on your live active gameplay HUD here. Leave empty if you ONLY open settings from inside panels.")]
    [SerializeField] private GameObject hudSettingsButton;

    [Header("UI Window Panels Container Layout")]
    [Tooltip("Drag your Canvas PauseMenuPanel here")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Drag your Canvas SettingsPanel here")]
    [SerializeField] private GameObject settingsPanel;

    [Tooltip("Drag your Canvas GameOverPanel here")]
    [SerializeField] private GameObject gameOverPanel;

    private bool isPaused;
    private bool isGameOver;

    // Tracks which master sub-panel opened the settings screen
    private GameObject sourcePanelBeforeSettings;

    private void Start()
    {
        PlayerHealth.OnPlayerDeath += HandleGameOverState;

        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Ensure only main live gameplay HUD triggers are visible on level boot
        if (hudPauseButton != null) hudPauseButton.SetActive(true);
        if (hudSettingsButton != null) hudSettingsButton.SetActive(true);
    }

    private void Update()
    {
        if (LevelVictoryManager.IsVictoryActive)
        {
            if (isPaused) ResumeGame();
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (isPaused && !isGameOver)
            {
                ResumeGame();
            }
            else if (!isPaused && !isGameOver)
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (isGameOver) return;
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        // Safely hide active HUD triggers so they don't overlay on top of menus
        if (hudPauseButton != null) hudPauseButton.SetActive(false);
        if (hudSettingsButton != null) hudSettingsButton.SetActive(false);

        Debug.Log("⏸️ Game Paused.");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (hudPauseButton != null) hudPauseButton.SetActive(true);
        if (hudSettingsButton != null) hudSettingsButton.SetActive(true);

        Debug.Log("▶️ Game Resumed.");
    }

    public void OpenSettingsFromHUD()
    {
        if (isGameOver) return;
        Time.timeScale = 0f;
        sourcePanelBeforeSettings = null;

        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (hudPauseButton != null) hudPauseButton.SetActive(false);
        if (hudSettingsButton != null) hudSettingsButton.SetActive(false);

        Debug.Log("⚙️ Settings Window opened directly from HUD.");
    }

    public void OpenSettingsFromPauseMenu()
    {
        sourcePanelBeforeSettings = pauseMenuPanel;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);

        Debug.Log("⚙️ Settings Window opened from inside Pause Menu.");
    }

    public void OpenSettingsFromGameOver()
    {
        sourcePanelBeforeSettings = gameOverPanel;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);

        Debug.Log("⚙️ Settings Window opened from inside Game Over Screen.");
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (sourcePanelBeforeSettings != null)
        {
            sourcePanelBeforeSettings.SetActive(true);
            Debug.Log($"⚙️ Closed Settings. Returned to: {sourcePanelBeforeSettings.name}");
        }
        else
        {
            Time.timeScale = 1f;
            if (hudPauseButton != null) hudPauseButton.SetActive(true);
            if (hudSettingsButton != null) hudSettingsButton.SetActive(true);
            Debug.Log("⚙️ Closed Settings. Restored active world gameplay.");
        }
    }

    // ── 🎵 NEW AUDIO SINGLETON TRANSLATOR BRIDGE METHOD ───────────────────
    /// <summary>
    /// Bridges local scene UI button click actions directly to the cross-scene 
    /// persistent BackgroundMusicManager Singleton instance without breaking engine scene boundaries.
    /// </summary>
    public void ToggleMusicMute()
    {
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.ToggleMute();
            Debug.Log("🎵 [PauseMenu UI Bridge] Forwarded mute toggle signal to persistent BGM manager.");
        }
        else
        {
            Debug.LogWarning("⚠️ [PauseMenu UI Bridge] BackgroundMusicManager instance not found in this game loop session!");
        }
    }
    // ───────────────────────────────────────────────────────────────────────

    public void TriggerGameOver()
    {
        isGameOver = true;
        isPaused = false;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (hudPauseButton != null) hudPauseButton.SetActive(false);
        if (hudSettingsButton != null) hudSettingsButton.SetActive(false);

        Debug.Log("💀 Game Over Screen Displayed.");
    }

    public void RestartGame()
    {
        PlayerHealth.SavedHealth = PlayerHealth.LevelStartHealth;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameMenu");
    }

    private void OnDestroy()
    {
        PlayerHealth.OnPlayerDeath -= HandleGameOverState;
    }

    private void HandleGameOverState()
    {
        TriggerGameOver();
    }
}