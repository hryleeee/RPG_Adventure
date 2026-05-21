using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelVictoryManager : MonoBehaviour
{
    // 🛡️ KEY GLOBAL FIX: Public static variable that ANY other script (like your Pause script) 
    // can check instantly without needing a complex reference wire hookup!
    public static bool IsVictoryActive { get; private set; } = false;

    [Header("UI Reference")]
    [Tooltip("Drag your Victory_Banner GameObject from the hierarchy here")]
    public GameObject victoryBanner;

    [Header("Sequence Adjustments")]
    [Tooltip("How many seconds the victory screen shows before changing scenes")]
    public float celebrationDelay = 3.5f;

    private void Awake()
    {
        // Reset this state every time a brand new level loads up
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
        GameObject[] enemiesTagged = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemiesTagged.Length > 0) return false;

        Demon1Controller[] demonsType1 = Object.FindObjectsByType<Demon1Controller>(FindObjectsSortMode.None);
        Demon2Controller[] demonsType2 = Object.FindObjectsByType<Demon2Controller>(FindObjectsSortMode.None);

        return (demonsType1.Length == 0 && demonsType2.Length == 0);
    }

    private IEnumerator TriggerVictorySequence()
    {
        IsVictoryActive = true; // Lock down state immediately
        Debug.Log("[LevelVictoryManager] Victory conditions met! Commencing progression transition.");

        if (victoryBanner != null)
        {
            victoryBanner.SetActive(true);
        }

        // ✅ FIXED: Changed to WaitForSecondsRealtime! Even if your teammate's pause code sets 
        // Time.timeScale to 0, this timer counts real-world clock seconds so it can't get stuck!
        yield return new WaitForSecondsRealtime(celebrationDelay);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("[LevelVictoryManager] Final Boss clear! Returning to main menu.");
            SceneManager.LoadScene(0); // Safely bounce back to scene zero index
        }
    }
}