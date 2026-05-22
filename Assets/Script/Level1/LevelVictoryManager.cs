using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelVictoryManager : MonoBehaviour
{
    public static bool IsVictoryActive { get; private set; } = false;

    [Header("UI Reference")]
    public GameObject victoryBanner;

    [Header("Sequence Adjustments")]
    public float celebrationDelay = 3.5f;

    private void Awake()
    {
        IsVictoryActive = false;
    }

    private void Update()
    {
        if (IsVictoryActive) return;

        if (AreAllEnemiesDefeated())
        {
            StartCoroutine(TriggerVictorySequence());
        }
    }

    private bool AreAllEnemiesDefeated()
    {
        // 🔍 MODERN UNITY 6 METHOD: Scan the active scene topology for ANY object 
        // that is carrying an EnemyHealth component.
        EnemyHealth[] remainingEnemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        // If the array count drops down to absolutely 0, the level is officially cleared!
        return remainingEnemies.Length == 0;
    }

    private IEnumerator TriggerVictorySequence()
    {
        IsVictoryActive = true;
        Debug.Log("[LevelVictoryManager] All enemies cleared! Commencing level transition.");

        if (victoryBanner != null)
        {
            victoryBanner.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(celebrationDelay);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("[LevelVictoryManager] Final level cleared! Returning to Main Menu.");
            SceneManager.LoadScene(0);
        }
    }
}