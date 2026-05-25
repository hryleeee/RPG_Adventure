using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Explicitly define our audio states cleanly
    public enum MusicState { MainMenu, Cutscene, Level1To3, Level4To6, Level7To9, Level10, None }
    private MusicState currentTrackState = MusicState.None;

    [Header("Audio Components")]
    [SerializeField] private AudioSource musicSource;

    [Header("Music Tracks Assets")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip cutsceneMusic;
    [SerializeField] private AudioClip level1To3Music;
    [SerializeField] private AudioClip level4To6Music;
    [SerializeField] private AudioClip level7To9Music;
    [SerializeField] private AudioClip level10Music;

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
        EvaluateSceneState(scene.name, scene.buildIndex);
    }

    /// <summary>
    /// Evaluates structural scene parameters safely and maps them cleanly to an architectural state
    /// </summary>
    private void EvaluateSceneState(string sceneName, int buildIndex)
    {
        MusicState targetState = MusicState.MainMenu;

        if (buildIndex == 0 || sceneName.Equals("GameMenu", System.StringComparison.OrdinalIgnoreCase))
        {
            targetState = MusicState.MainMenu;
        }
        else if (sceneName.StartsWith("Scene", System.StringComparison.OrdinalIgnoreCase))
        {
            targetState = MusicState.Cutscene;
        }
        else if (sceneName.StartsWith("Level", System.StringComparison.OrdinalIgnoreCase))
        {
            int levelNum = ExtractDigits(sceneName);
            if (levelNum >= 1 && levelNum <= 3) targetState = MusicState.Level1To3;
            else if (levelNum >= 4 && levelNum <= 6) targetState = MusicState.Level4To6;
            else if (levelNum >= 7 && levelNum <= 9) targetState = MusicState.Level7To9;
            else if (levelNum == 10) targetState = MusicState.Level10;
        }

        TransitionToState(targetState);
    }

    private void TransitionToState(MusicState newState)
    {
        // Don't interrupt or restart the audio tracking loop if we are staying within the same track state block
        if (currentTrackState == newState && musicSource.isPlaying)
        {
            ApplySavedMuteState();
            return;
        }

        musicSource.Stop();
        currentTrackState = newState;

        switch (currentTrackState)
        {
            case MusicState.MainMenu: musicSource.clip = mainMenuMusic; break;
            case MusicState.Cutscene: musicSource.clip = cutsceneMusic; break;
            case MusicState.Level1To3: musicSource.clip = level1To3Music; break;
            case MusicState.Level4To6: musicSource.clip = level4To6Music; break;
            case MusicState.Level7To9: musicSource.clip = level7To9Music; break;
            case MusicState.Level10: musicSource.clip = level10Music; break;
            default: musicSource.clip = null; break;
        }

        if (musicSource.clip != null)
        {
            musicSource.Play();
        }

        ApplySavedMuteState();
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
            musicSource.mute = PlayerPrefs.GetInt("MusicMutedSetting", 0) == 1;
        }
    }

    private int ExtractDigits(string text)
    {
        string digits = "";
        foreach (char c in text) if (char.IsDigit(c)) digits += c;
        return int.TryParse(digits, out int val) ? val : -1;
    }
}