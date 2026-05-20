using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // CRITICAL: Speaks native Unity 6 Input package drivers

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels Container Layout")]
    [Tooltip("Drag your Canvas PauseMenuPanel here")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Drag your Canvas GameOverPanel here")]
    [SerializeField] private GameObject gameOverPanel;

    private bool isPaused;
    private bool isGameOver;

    private void Start()
    {
        // Force state cleanup on boot to clear out lifecycle timing glitches
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        // If the player is dead, prevent keyboard input checks completely
        if (isGameOver) return;

        // Modern Input System check: Listens flawlessly for the Escape Key
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("▶️ Game Resumed.");
    }

    public void PauseGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game physics entirely
        isPaused = true;
        Debug.Log("⏸️ Game Paused.");
    }

    /// <summary>
    /// Called directly by PlayerHealth.cs when lives touch absolute zero!
    /// </summary>
    public void TriggerGameOver()
    {
        if (isGameOver) return; // Prevent double trigger hazards

        isGameOver = true;
        Time.timeScale = 0f; // Freeze reality completely solid

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false); // Cleanly hide pause frame
        if (gameOverPanel != null) gameOverPanel.SetActive(true);    // Display game over screen overlay

        Debug.Log("💀 Game Over Interface Displayed Masterfully!");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // ALWAYS unfreeze time scaling before changing scenes
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f; // ALWAYS unfreeze time scaling before traveling back home
        SceneManager.LoadScene("GameMenu"); // Adjust if your main menu asset file uses a different string casing name
    }
}