using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Components")]
    [SerializeField] private AudioSource musicSource;

    [Header("Music Tracks Assets")]
    public AudioClip mainMenuMusic;
    public AudioClip cutsceneMusic;
    public AudioClip level1To3Music;
    public AudioClip level4To6Music;
    public AudioClip level7To9Music;
    public AudioClip level10Music;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        ApplySavedMuteState();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneMusic(scene.name, scene.buildIndex);
    }

    private void UpdateSceneMusic(string sceneName, int buildIndex)
    {
        AudioClip targetTrack = null;

        // 1. Check for the Main Menu Scene
        if (sceneName.Equals("GameMenu", System.StringComparison.OrdinalIgnoreCase) || buildIndex == 0)
        {
            targetTrack = mainMenuMusic;
        }
        // 2. FIXED CONFIGURATION: Catch ALL cinematic scenes named "Scene1" through "Scene8"
        else if (sceneName.StartsWith("Scene", System.StringComparison.OrdinalIgnoreCase))
        {
            targetTrack = cutsceneMusic;
        }
        // 3. Dynamic grouping tracking layout logic rules for Level1 through Level10
        else if (sceneName.StartsWith("Level", System.StringComparison.OrdinalIgnoreCase))
        {
            int levelNumber = ExtractLevelNumber(sceneName);

            if (levelNumber >= 1 && levelNumber <= 3)
            {
                targetTrack = level1To3Music;
            }
            else if (levelNumber >= 4 && levelNumber <= 6)
            {
                targetTrack = level4To6Music;
            }
            else if (levelNumber >= 7 && levelNumber <= 9)
            {
                targetTrack = level7To9Music;
            }
            else if (levelNumber == 10)
            {
                targetTrack = level10Music;
            }
        }

        // Apply track changes cleanly ONLY if it's a completely new track asset file
        if (targetTrack != null && musicSource.clip != targetTrack)
        {
            musicSource.Stop();
            musicSource.clip = targetTrack;
            musicSource.Play();
        }

        // Enforce your global menu saved preferences instantly across scenes
        ApplySavedMuteState();
    }

    private int ExtractLevelNumber(string name)
    {
        string digits = "";
        foreach (char c in name)
        {
            if (char.IsDigit(c)) digits += c;
        }

        if (int.TryParse(digits, out int result))
        {
            return result;
        }
        return -1;
    }

    public void SetMusicMute(bool shouldMute)
    {
        PlayerPrefs.SetInt("MusicMutedSetting", shouldMute ? 1 : 0);
        PlayerPrefs.Save();

        if (musicSource != null)
        {
            musicSource.mute = shouldMute;
        }
    }

    public void ApplySavedMuteState()
    {
        if (musicSource != null)
        {
            bool isMutedSaved = PlayerPrefs.GetInt("MusicMutedSetting", 0) == 1;
            musicSource.mute = isMutedSaved;
        }
    }
}