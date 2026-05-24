using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Scene2 — Floor 1 cleared cutscene.
///
/// Sequence:
///   1. Black → image 23 fades in (dungeon cleared)
///   2. Black → Kael: "That was close. Let's go to the next floor." → image 8 fades in
///   3. Black → loads LEVEL 2
///
/// Canvas hierarchy (same as Scene1):
///   Canvas
///   ├── StoryImage     (full screen, alpha=0 at start)
///   ├── BlackOverlay   (full screen black, alpha=1 at start)
///   ├── ScrollPanel    (scroll frame image 12.png)
///   │   ├── SpeakerText  (TMP)
///   │   └── StoryText    (TMP)
///   └── SkipHint       (TMP)
/// </summary>
public class Scene2Manager : MonoBehaviour
{
    [Header("UI References")]
    public Image            storyImage;
    public Image            blackOverlay;
    public GameObject       scrollPanel;
    public TextMeshProUGUI  speakerText;
    public TextMeshProUGUI  storyText;
    public TextMeshProUGUI  skipHint;

    [Header("Story Sprites")]
    [Tooltip("Slot 0 = image23.png, Slot 1 = image8.png")]
    public Sprite[] storySprites;   // size 2

    [Header("Timing")]
    public float fadeInDuration  = 1.4f;
    public float fadeOutDuration = 0.9f;
    public float typewriterSpeed = 38f;
    public float holdDuration    = 1.2f;

    [Header("Next Scene")]
    public string nextSceneName = "LEVEL 2";

    // ── Panel data ────────────────────────────────────────────────────────
    private struct Panel
    {
        public string speaker;
        public string text;
        public int    imgIndex;  // index into storySprites, -1 = black
        public bool   keepImg;
    }

    private readonly Panel[] panels = new Panel[]
    {
        // Panel 1 — image 23, no text (just the visual)
        new Panel { speaker="", text="Floor 1 of the dungeon has been cleared.", imgIndex=0, keepImg=false },

        // Panel 2 — Kael dialogue, image 8
        new Panel { speaker="Kael", text="\"That was close. Let's go to the next floor.\"", imgIndex=1, keepImg=false },
    };

    // ── Runtime ───────────────────────────────────────────────────────────
    private bool inputPressed = false;
    private bool sequenceDone = false;

    private void Start()
    {
        SetAlpha(blackOverlay, 1f);
        SetAlpha(storyImage,   0f);
        if (scrollPanel != null) scrollPanel.SetActive(false);
        if (skipHint    != null) skipHint.text = "Click or press SPACE to continue";

        StartCoroutine(RunSequence());
    }

    private void Update()
    {
        if (sequenceDone) return;
        bool clicked = Mouse.current    != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool spaced  = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (clicked || spaced) inputPressed = true;
    }

    private IEnumerator RunSequence()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            Panel p = panels[i];
            inputPressed = false;

            // Ensure black
            yield return StartCoroutine(Fade(blackOverlay, GetAlpha(blackOverlay), 1f, 0.3f));
            SetAlpha(storyImage, 0f);

            // Set image behind overlay
            if (!p.keepImg && p.imgIndex >= 0 && p.imgIndex < storySprites.Length)
                storyImage.sprite = storySprites[p.imgIndex];

            // Show text on black
            bool hasText = p.text.Length > 0;
            if (scrollPanel != null) scrollPanel.SetActive(hasText);

            if (hasText)
            {
                yield return StartCoroutine(Fade(blackOverlay, 1f, 0.88f, 0.35f));

                if (speakerText != null)
                {
                    speakerText.text = p.speaker;
                    speakerText.gameObject.SetActive(p.speaker.Length > 0);
                }

                yield return StartCoroutine(TypeText(p.text));

                inputPressed = false;
                yield return StartCoroutine(WaitForInput(holdDuration));
                inputPressed = false;
            }

            // Reveal image dramatically
            if (!p.keepImg && p.imgIndex >= 0 && p.imgIndex < storySprites.Length)
                yield return StartCoroutine(FadeBoth(fadeInDuration));

            // Wait for click
            inputPressed = false;
            yield return StartCoroutine(WaitForInput(holdDuration));
            inputPressed = false;

            // Fade to black
            yield return StartCoroutine(Fade(blackOverlay, GetAlpha(blackOverlay), 1f, fadeOutDuration));

            if (scrollPanel != null) scrollPanel.SetActive(false);
            if (storyText   != null) storyText.text = "";
            if (speakerText != null) { speakerText.text = ""; speakerText.gameObject.SetActive(false); }
            SetAlpha(storyImage, 0f);

            yield return new WaitForSeconds(0.15f);
        }

        // Load LEVEL 2
        sequenceDone = true;
        if (skipHint != null) skipHint.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator TypeText(string full)
    {
        if (storyText == null) yield break;
        storyText.text = "";
        inputPressed   = false;

        foreach (char c in full)
        {
            if (inputPressed) { storyText.text = full; break; }
            storyText.text += c;
            yield return new WaitForSeconds(1f / typewriterSpeed);
        }
        inputPressed = false;
    }

    private IEnumerator WaitForInput(float maxWait)
    {
        float t = 0f;
        while (!inputPressed && t < maxWait)
        {
            t += Time.deltaTime;
            yield return null;
        }
        inputPressed = false;
    }

    private IEnumerator FadeBoth(float duration)
    {
        float elapsed = 0f;
        float startOverlay = GetAlpha(blackOverlay);
        SetAlpha(storyImage, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = t * t * (3f - 2f * t);
            SetAlpha(blackOverlay, Mathf.Lerp(startOverlay, 0f, smooth));
            SetAlpha(storyImage,   Mathf.Lerp(0f, 1f, smooth));
            yield return null;
        }
        SetAlpha(blackOverlay, 0f);
        SetAlpha(storyImage,   1f);
    }

    private IEnumerator Fade(Image img, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(img, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetAlpha(img, to);
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = a; img.color = c;
    }

    private float GetAlpha(Image img) => img == null ? 0f : img.color.a;
}
