using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Dramatic story intro for Scene1.
///
/// Each panel:
///   1. Fade to black
///   2. Optional speaker label + text types out on scroll
///   3. Click/Space → image fades in (or stays black if no image)
///   4. Click/Space → fade to black → next panel
///
/// Canvas hierarchy:
///   Canvas
///   ├── StoryImage     (full screen, starts alpha=0)
///   ├── BlackOverlay   (full screen black, starts alpha=1)
///   ├── ScrollPanel    (scroll frame image 12.png)
///   │   ├── SpeakerText  (TMP — character name, e.g. "King Aldric")
///   │   └── StoryText    (TMP — dialogue/narration)
///   └── SkipHint       (TMP — "Click or SPACE to continue")
/// </summary>
public class StoryManager : MonoBehaviour
{
    [Header("UI References")]
    public Image            storyImage;
    public Image            blackOverlay;
    public GameObject       scrollPanel;
    public TextMeshProUGUI  speakerText;   // character name label (optional)
    public TextMeshProUGUI  storyText;     // main dialogue/narration
    public TextMeshProUGUI  skipHint;

    [Header("Story Sprites")]
    [Tooltip("Assign ALL images 1-25 here in order. Index 0 = image1, index 24 = image25.")]
    public Sprite[] storySprites;          // size 25, index = imageNumber - 1

    [Header("Timing")]
    public float fadeInDuration  = 1.4f;
    public float fadeOutDuration = 0.9f;
    public float typewriterSpeed = 38f;
    public float holdDuration    = 1.2f;

    [Header("Next Scene")]
    public string nextSceneName = "LEVEL 1";

    // ── Panel data ────────────────────────────────────────────────────────
    private struct Panel
    {
        public string speaker;    // "" = narration (no name shown)
        public string text;       // "" = no text (image only)
        public int    imgIndex;   // -1 = black, 0-based index into storySprites
        public bool   keepImg;    // true = don't change current image
    }

    private Panel[] panels;

    private void Awake()
    {
        // imgIndex = imageNumber - 1  (e.g. image 1 → index 0)
        panels = new Panel[]
        {
            // ── ACT 1: THE SEAL ──────────────────────────────────────────
            new Panel { speaker="", text="For 500 years, the kingdom of Elarion lived in peace after the ancient demons were sealed beneath the earth inside the forbidden abyss called Noctyra.", imgIndex=0, keepImg=false },
            // image 1 shown, then:
            new Panel { speaker="", text="But peace was never permanent.", imgIndex=-1, keepImg=true },
            new Panel { speaker="", text="One night, the moon turned crimson.", imgIndex=2, keepImg=false },
            new Panel { speaker="", text="The ground cracked. And from beneath the kingdom…\nThe demons began whispering again.", imgIndex=16, keepImg=false },

            // ── ACT 2: THE FEAR ──────────────────────────────────────────
            new Panel { speaker="", text="Entire villages vanished overnight. Knights sent into the abyss never returned. Fear spread across the kingdom.", imgIndex=3, keepImg=false },
            new Panel { speaker="", text="The king begged the legendary royal knights to fight.", imgIndex=4, keepImg=false },
            new Panel { speaker="", text="None accepted.", imgIndex=4, keepImg=true },

            // ── ACT 3: THE WARNING ───────────────────────────────────────
            new Panel { speaker="King Aldric", text="\"If the seal breaks… humanity ends.\"", imgIndex=24, keepImg=false },
            new Panel { speaker="", text="Silence.", imgIndex=-1, keepImg=true },
            new Panel { speaker="", text="Until one knight walked forward.", imgIndex=-1, keepImg=true },

            // ── ACT 4: KAEL STEPS UP ─────────────────────────────────────
            new Panel { speaker="", text="Kael steps forward.", imgIndex=8, keepImg=false },
            new Panel { speaker="Kael", text="\"Then I'll stop it.\"", imgIndex=7, keepImg=false },
            new Panel { speaker="", text="Not the strongest.", imgIndex=7, keepImg=true },
            new Panel { speaker="", text="Not the richest.", imgIndex=7, keepImg=true },
            new Panel { speaker="", text="Not the chosen one.", imgIndex=7, keepImg=true },
            new Panel { speaker="", text="Just a man with courage greater than fear.", imgIndex=1, keepImg=false },
            new Panel { speaker="", text="His name was Sir Kael Draven — the last knight willing to descend into Noctyra.", imgIndex=-1, keepImg=true },

            // ── ACT 5: THE FAREWELL ──────────────────────────────────────
            new Panel { speaker="King Aldric", text="\"Thousands died down there.\"", imgIndex=24, keepImg=false },
            new Panel { speaker="Kael", text="\"Then I'll carry their will with me.\"", imgIndex=7, keepImg=false },
            new Panel { speaker="", text="The king hands Kael an ancient sword.", imgIndex=23, keepImg=false },
            new Panel { speaker="King Aldric", text="\"Return alive, Sir Kael.\"", imgIndex=23, keepImg=true },
            new Panel { speaker="Kael", text="\"I'll return when the darkness dies.\"", imgIndex=6, keepImg=false },
            new Panel { speaker="", text="Kael walks toward the giant abyss gate as thunder strikes.", imgIndex=5, keepImg=false },
            new Panel { speaker="", text="The dungeon doors slowly open.", imgIndex=-1, keepImg=true },

            // ── ACT 6: INTO THE DUNGEON ──────────────────────────────────
            new Panel { speaker="", text="", imgIndex=21, keepImg=false },   // image 22 — empty dungeon, no text
            new Panel { speaker="", text="", imgIndex=13, keepImg=false },   // image 14 — exploring deeper
            new Panel { speaker="", text="He sees a lot of demon ghosts roaming the dungeon.", imgIndex=13, keepImg=true },
            new Panel { speaker="Kael", text="\"Bless me all saints and gods up there…\"", imgIndex=7, keepImg=false },
            new Panel { speaker="Kael", text="\"For the sake of our kingdom and my family.\"", imgIndex=7, keepImg=true },
            // Final black fade → LEVEL1
        };
    }

    // ── Runtime state ─────────────────────────────────────────────────────
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

    // ── Main sequence ─────────────────────────────────────────────────────
    private IEnumerator RunSequence()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            Panel p = panels[i];
            inputPressed = false;

            // Always start from black
            yield return StartCoroutine(Fade(blackOverlay, GetAlpha(blackOverlay), 1f, 0.3f));
            SetAlpha(storyImage, 0f);

            // Set image behind overlay (invisible for now)
            if (!p.keepImg && p.imgIndex >= 0 && p.imgIndex < storySprites.Length)
                storyImage.sprite = storySprites[p.imgIndex];

            // ── Show text on black ────────────────────────────────────────
            bool hasText = p.text.Length > 0;
            if (scrollPanel != null) scrollPanel.SetActive(hasText);

            if (hasText)
            {
                // Slightly transparent overlay so text is readable
                yield return StartCoroutine(Fade(blackOverlay, 1f, 0.88f, 0.35f));

                // Show speaker name
                if (speakerText != null)
                {
                    speakerText.text = p.speaker;
                    speakerText.gameObject.SetActive(p.speaker.Length > 0);
                }

                yield return StartCoroutine(TypeText(p.text));

                // Wait for click
                inputPressed = false;
                yield return StartCoroutine(WaitForInput(holdDuration));
                inputPressed = false;
            }

            // ── Reveal image ──────────────────────────────────────────────
            if (!p.keepImg && p.imgIndex >= 0 && p.imgIndex < storySprites.Length)
            {
                // Dramatic simultaneous: overlay fades out, image fades in
                yield return StartCoroutine(FadeBoth(fadeInDuration));
            }
            else if (p.imgIndex < 0 && !p.keepImg)
            {
                // No image — just ensure black
                yield return StartCoroutine(Fade(blackOverlay, GetAlpha(blackOverlay), 1f, 0.3f));
            }

            // Wait for click to advance
            inputPressed = false;
            yield return StartCoroutine(WaitForInput(holdDuration));
            inputPressed = false;

            // ── Fade to black ─────────────────────────────────────────────
            yield return StartCoroutine(Fade(blackOverlay, GetAlpha(blackOverlay), 1f, fadeOutDuration));

            // Clean up
            if (scrollPanel != null) scrollPanel.SetActive(false);
            if (storyText   != null) storyText.text = "";
            if (speakerText != null) { speakerText.text = ""; speakerText.gameObject.SetActive(false); }
            SetAlpha(storyImage, 0f);

            yield return new WaitForSeconds(0.15f);
        }

        // All done — load LEVEL1
        sequenceDone = true;
        if (skipHint != null) skipHint.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.6f);
        SceneManager.LoadScene(nextSceneName);
    }

    // ── Typewriter ────────────────────────────────────────────────────────
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

    // ── Wait for input or timeout ─────────────────────────────────────────
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

    // ── Fade overlay out + image in simultaneously ────────────────────────
    private IEnumerator FadeBoth(float duration)
    {
        float elapsed = 0f;
        float startOverlay = GetAlpha(blackOverlay);
        SetAlpha(storyImage, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease in-out for dramatic feel
            float smooth = t * t * (3f - 2f * t);
            SetAlpha(blackOverlay, Mathf.Lerp(startOverlay, 0f, smooth));
            SetAlpha(storyImage,   Mathf.Lerp(0f, 1f, smooth));
            yield return null;
        }
        SetAlpha(blackOverlay, 0f);
        SetAlpha(storyImage,   1f);
    }

    // ── Fade a single Image ───────────────────────────────────────────────
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

    // ── Helpers ───────────────────────────────────────────────────────────
    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = a; img.color = c;
    }

    private float GetAlpha(Image img) => img == null ? 0f : img.color.a;
}
