using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // CRITICAL: Gives us access to TextMesh Pro UI elements

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("UI Feedback Elements")]
    [Tooltip("Drag a TextMeshProUGUI component from your Canvas here to display progression updates.")]
    public TextMeshProUGUI completionText;

    [Header("Transition Layout Parameters")]
    [Tooltip("How long to display the complete banner before transitioning scenes.")]
    public float levelTransitionDelay = 3.0f;

    private int totalEnemies;
    private int deadEnemiesCount;
    private bool isLevelTransitioning = false;

    private void Awake()
    {
        // Secure the Singleton connection
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Keep the text completely hidden on level startup
        if (completionText != null)
        {
            completionText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Scan the entire battlefield for enemies right on launch
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        totalEnemies = enemies.Length;
        deadEnemiesCount = 0;
        isLevelTransitioning = false;

        Debug.Log($"[LevelManager] Active zone scanned. Total enemies registered: {totalEnemies}");
    }

    // Called automatically by Demon1Controller or Demon2Controller when they hit 0 HP
    public void RegisterEnemyDeath()
    {
        if (isLevelTransitioning) return;

        deadEnemiesCount++;
        Debug.Log($"[LevelManager] Enemy slain. Current counter: {deadEnemiesCount}/{totalEnemies}");

        if (deadEnemiesCount >= totalEnemies)
        {
            StartCoroutine(SequenceChronologicalTransition());
        }
    }

    private IEnumerator SequenceChronologicalTransition()
    {
        isLevelTransitioning = true;

        // Fetch the name of your active map asset
        string currentLevelName = SceneManager.GetActiveScene().name;

        // Display the requested custom feedback message on-screen
        if (completionText != null)
        {
            completionText.text = $"{currentLevelName} CLEARED!";
            completionText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[LevelManager] {currentLevelName} done! (Completion UI slot missing in Inspector)");
        }

        // FREEZE physics/time if desired, or let the player run around during the brief buffer window
        Debug.Log($"[LevelManager] Level finished! Delaying transition for {levelTransitionDelay} seconds...");

        // Wait for your customized duration clock to tick down
        yield return new WaitForSeconds(levelTransitionDelay);

        // Calculate the next chronological scene in your Build Settings profile sequence
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Safety verification check: Make sure there's another scene left in the build tree
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Ultimate endgame state hit (LEVEL10 cleared!)
            Debug.Log("[LevelManager] Absolute victory achieved! All levels completely cleared.");
            if (completionText != null)
            {
                completionText.text = "VICTORY! ALL THREATS PURGED!";
            }

            yield return new WaitForSeconds(3.0f);
            SceneManager.LoadScene("GameMenu"); // Return to title screen
        }
    }
}