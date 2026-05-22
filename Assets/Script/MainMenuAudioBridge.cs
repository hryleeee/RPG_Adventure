using UnityEngine;

public class MainMenuAudioBridge : MonoBehaviour
{
    /// <summary>
    /// Locates the surviving, persistent music manager via code 
    /// at runtime to avoid broken editor references on scene reload.
    /// </summary>
    public void ToggleMainMenuMusic()
    {
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.ToggleMute();
            Debug.Log("🎵 [Main Menu Bridge] Successfully bypassed scene reload boundary to toggle mute.");
        }
        else
        {
            Debug.LogWarning("⚠️ [Main Menu Bridge] Active BackgroundMusicManager instance could not be found!");
        }
    }
}