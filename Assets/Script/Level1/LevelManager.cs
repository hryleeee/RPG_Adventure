using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("UI Feedback Elements")]
    public TextMeshProUGUI completionText;

    [Header("Transition Layout Parameters")]
    public float levelTransitionDelay = 3.0f;

    private int totalEnemies;
    private int deadEnemiesCount;
    private bool isLevelTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (completionText != null)
        {
            completionText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        totalEnemies = enemies.Length;
        deadEnemiesCount = 0;
        isLevelTransitioning = false;

        Debug.Log($"[LevelManager] Active zone scanned. Total enemies registered: {totalEnemies}");
    }

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
        string currentLevelName = SceneManager.GetActiveScene().name;

        float elapsed = 0f;

        // Dynamic frame-by-frame loop to watch out for pause commands
        while (elapsed < levelTransitionDelay)
        {
            if (Time.timeScale == 0f)
            {
                // The game is paused! Hide the clear message so it doesn't clutter the pause menu
                if (completionText != null && completionText.gameObject.activeSelf)
                {
                    completionText.gameObject.SetActive(false);
                }
            }
            else
            {
                // Game is active! Show the message and tick down our transition clock
                if (completionText != null && !completionText.gameObject.activeSelf)
                {
                    completionText.text = $"{currentLevelName} CLEARED";
                    completionText.gameObject.SetActive(true);
                }

                elapsed += Time.deltaTime;
            }

            yield return null; // Wait for the next frame render pass
        }

        // Clock has run out cleanly! Process the next scene index transition
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("[LevelManager] Absolute victory achieved! All levels completely cleared.");
            if (completionText != null)
            {
                completionText.text = "VICTORY! ALL THREATS PURGED!";
                completionText.gameObject.SetActive(true);
            }

            yield return new WaitForSecondsRealtime(3.0f);
            SceneManager.LoadScene("GameMenu");
        }
    }
}