using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Speaks natively to Unity 6 Input Package drivers

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
        PlayerHealth.OnPlayerDeath += HandleGameOverState;
        // Force clean state on scene boot
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (isGameOver) return;

        // 🛡️ KEY FIX: If LevelVictoryManager has declared the level won,
        // ignore the Escape key entirely so the player cannot accidentally pause during the victory lap!
        if (LevelVictoryManager.IsVictoryActive)
        {
            // If the game happened to be paused when the victory hit, unpause it safely
            if (isPaused)
            {
                ResumeGame();
            }
            return;
        }

        // Modern Unity 6 Input driver check for Escape Key
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
        Time.timeScale = 0f; // Freeze world physics
        isPaused = true;
        Debug.Log("⏸️ Game Paused.");
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f; // Freeze game physics completely

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Debug.Log("💀 Game Over Screen Displayed.");
    }

    public void RestartGame()
    {
        // Inject their exact level entrance health snapshot back into memory!
        PlayerHealth.SavedHealth = PlayerHealth.LevelStartHealth;

        Time.timeScale = 1f; // ALWAYS unfreeze time scaling before changing scenes!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log($"🔄 Reloading scene. Resetting player health back to checkpoint entry value: {PlayerHealth.LevelStartHealth}");
    }

    public void ExitToMenu()
    {
        Debug.Log("🎯 ExitToMenu() button function was physically clicked!");
        Time.timeScale = 1f; // ALWAYS unfreeze time scaling before returning to menus!

        // Exact name matching your Scenes folder configuration down to the case
        SceneManager.LoadScene("GameMenu");
        Debug.Log("🚀 SceneManager load execution fired for 'GameMenu'.");
    }

    private void OnDestroy()
    {
        // Always clean up your wires to prevent background memory leaks
        PlayerHealth.OnPlayerDeath -= HandleGameOverState;
    }

    private void HandleGameOverState()
    {
        // Route the event signal directly into the existing robust panel system!
        TriggerGameOver();
    }
}