using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance { get; private set; }

    [Header("Audio Playlist Assets")]
    [Tooltip("Track 1: Main Menu Background Music")]
    public AudioClip mainMenuMusic;

    [Tooltip("Track 2: Music for Levels 1, 2, and 3")]
    public AudioClip levels1To3Music;

    [Tooltip("Track 3: Music for Levels 4, 5, and 6")]
    public AudioClip levels4To6Music;

    [Tooltip("Track 4: Music for Levels 7, 8, and 9")]
    public AudioClip levels7To9Music;

    [Tooltip("Track 5: Music for Level 10 (Boss Theme)")]
    public AudioClip level10Music;

    private AudioSource audioSource;

    private void Awake()
    {
        // 🛠️ SINGLETON INFRASTRUCTURE: Ensures only one music jukebox persists.
        // Wipes out duplicates if you cycle back to the Main Menu later.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Makes this object survive changing scenes!

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;          // Ambient tracks must loop seamlessly
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        // Subscribe to Unity's scene loading event listener pipeline
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateThemeTrack(scene.name);
    }

    private void UpdateThemeTrack(string sceneName)
    {
        AudioClip selectedTrack = null;

        // 🔍 PLAYLIST ROUTING MATRIX (Matches your exact project naming conventions)
        if (sceneName == "GameMenu" || sceneName == "MainMenu" || sceneName == "LEVEL 1" || sceneName == "LEVEL 2" || sceneName == "LEVEL 3")
        {
            selectedTrack = mainMenuMusic;
        }
        else if (sceneName == "LEVEL 4" || sceneName == "LEVEL 5" || sceneName == "LEVEL 6")
        {
            selectedTrack = levels4To6Music;
        }
        else if (sceneName == "LEVEL 7" || sceneName == "LEVEL 8" || sceneName == "LEVEL 9")
        {
            selectedTrack = levels7To9Music;
        }
        else if (sceneName == "LEVEL 10")
        {
            selectedTrack = level10Music;
        }

        // 🛑 CROSS-LEVEL SEAMLESS CHECK: Only change tracks if the new song is actually different!
        // This ensures moving from Level 1 to Level 2 won't stutter or reset the track back to the start.
        if (selectedTrack != null && audioSource.clip != selectedTrack)
        {
            audioSource.clip = selectedTrack;
            audioSource.Play();
            Debug.Log($"🎵 [BGM Manager] Audio channel swapped to track for scene: '{sceneName}'");
        }
    }

    // ── Place this anywhere inside your BackgroundMusicManager class ──

    /// <summary>
    /// Toggles the mute state of the background music jukebox.
    /// Can be called directly by Unity UI Button OnClick events!
    /// </summary>
    public void ToggleMute()
    {
        if (audioSource != null)
        {
            audioSource.mute = !audioSource.mute;
            Debug.Log($"🎵 [BGM Manager] Mute state toggled! Muted = {audioSource.mute}");
        }
    }
}