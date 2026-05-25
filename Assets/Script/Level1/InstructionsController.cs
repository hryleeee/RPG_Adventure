using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsController : MonoBehaviour
{
    [Header("Timer Configuration")]
    [Tooltip("Time in seconds before the instructions panel disappears automatically")]
    [SerializeField] private float displayDuration = 7f;

    [Header("Target Level Configuration")]
    [Tooltip("The exact name of the level where instructions should appear")]
    [SerializeField] private string targetSceneName = "LEVEL 1";

    private void Start()
    {
        // Fetch the name of the current active scene layout
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Clean up checking string formatting to avoid capital letter mismatched bugs
        if (currentSceneName.Equals(targetSceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            // Make sure the panel is visible at the start of Level 1
            gameObject.SetActive(true);

            // Trigger the hide function automatically after 7 seconds
            Invoke(nameof(HideInstructions), displayDuration);
        }
        else
        {
            // If the player is on Levels 2-10, instantly turn this panel off at startup
            gameObject.SetActive(false);
        }
    }

    private void HideInstructions()
    {
        // Turn off the GameObject cleanly
        gameObject.SetActive(false);
    }
}