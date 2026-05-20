using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Enemy Tracking Tag")]
    [SerializeField] private string enemyTag = "Enemy";

    private int currentEnemyCount;

    private void Awake()
    {
        // Typo corrected: Changed 'nth' to 'this' so the Singleton establishes perfectly
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Dynamically find and count every monster manually placed or spawned at start
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        currentEnemyCount = enemies.Length;
        Debug.Log($"👾 Level initialized. Monsters remaining to clear: {currentEnemyCount}");

        // Safeguard check in case a room starts out empty
        CheckProgression();
    }

    /// <summary>
    /// Called globally by any enemy script right as they execute their death logic.
    /// </summary>
    public void RegisterEnemyDeath()
    {
        currentEnemyCount--;
        Debug.Log($"⚔️ A monster was slain! Monsters remaining: {currentEnemyCount}");

        CheckProgression();
    }

    private void CheckProgression()
    {
        if (currentEnemyCount <= 0)
        {
            LoadNextCustomLevel();
        }
    }

    private void LoadNextCustomLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = "GameMenu"; // Default fallback

        // Exact uppercase mapping matching your repository structure
        switch (currentScene)
        {
            case "LEVEL1":
                nextScene = "LEVEL3"; // Skips LEVEL2
                break;
            case "LEVEL3":
                nextScene = "LEVEL4";
                break;
            case "LEVEL4":
                nextScene = "LEVEL9"; // Skips 5, 6, 7, 8
                break;
            case "LEVEL9":
                nextScene = "LEVEL10";
                break;
            case "LEVEL10":
                nextScene = "GameMenu"; // Victory loop back
                break;
            default:
                nextScene = "GameMenu";
                break;
        }

        Debug.Log($"🏆 All monsters cleared! Transitioning from {currentScene} to {nextScene}...");

        Time.timeScale = 1f; // Keep time moving smoothly
        SceneManager.LoadScene(nextScene);
    }
}