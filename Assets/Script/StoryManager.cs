using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Scene1 story intro — no black overlay between panels.
/// Each panel: image crossfades in, text types on scroll, click advances.
///
/// Canvas hierarchy:
///   Canvas
///   ├── StoryImageA    (full screen Image — ping-pong buffer A)
///   ├── StoryImageB    (full screen Image — ping-pong buffer B)
///   ├── BlackOverlay   (full screen black — only used for very first fade-in and final fade-out)
///   ├── ScrollPanel    (Image with 12.png scroll sprite)
///   │   ├── SpeakerText  (TMP — character name)
///   │   └── StoryText    (TMP — dialogue / narration)
///   └── (no SkipHint needed)
/// </summary>
public class StoryManager : MonoBehaviour
{
    [Header("UI References")]
    public Image           imageA;         // ping-pong buffer A
    public Image           imageB;         // ping-pong buffer B
    public Image           blackOverlay;   // only for first/last fade
    public GameObject      scrollPanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI storyText;

    [Header("Story Sprites — assign 1.png to 31.png in order (index = number - 1)")]
    public Sprite[] storySprites;          // size 31

    [Header("Timing")]
    public float crossfadeDuration = 1.2f;
    public float typewriterSpeed   = 38f;
    public float holdDuration      = 1.0f;

    [Header("Next Scene")]
    public string nextSceneName = "LEVEL 1";

    // ── Panel definition ──────────────────────────────────────────────────
    private struct Panel
    {
        public string speaker;
        public string text;
        public int    imgIndex;   // 0-based index into storySprites (imgNumber - 1)
    }

    // imgIndex = image number - 1
    private Panel[] panels;

    private void Awake()
    {
        panels = new Panel[]
        {
            // "For 500 years..." → image 1
            new Panel { speaker="",           text="For 500 years, the kingdom of Elarion lived in peace after the ancient demons were sealed beneath the earth inside the forbidden abyss called Noctyra.", imgIndex=0 },
            // "But peace was never permanent." → image 27
            new Panel { speaker="",           text="But peace was never permanent.",                                                                imgIndex=26 },
            // "One night, the moon turned crimson." → image 3
            new Panel { speaker="",           text="One night, the moon turned crimson.",                                                           imgIndex=2  },
            // "The ground cracked..." → image 31
            new Panel { speaker="",           text="The ground cracked. And from beneath the kingdom…",                                             imgIndex=30 },
            // "the demons began whispering again." → image 17
            new Panel { speaker="",           text="The demons began whispering again.",                                                            imgIndex=16 },
            // "Entire villages vanished..." → image 4
            new Panel { speaker="",           text="Entire villages vanished overnight. Knights sent into the abyss never returned. Fear spread across the kingdom.", imgIndex=3 },
            // "The king begged..." → image 5
            new Panel { speaker="",           text="The king begged the legendary royal knights to fight.",                                         imgIndex=4  },
            // "None accepted." → image 5 (same)
            new Panel { speaker="",           text="None accepted.",                                                                                imgIndex=4  },
            // King Aldric → image 28
            new Panel { speaker="King Aldric",text="\"If the seal breaks… humanity ends.\"",                                                       imgIndex=27 },
            // Kael steps forward → image 9
            new Panel { speaker="",           text="Kael steps forward.",                                                                          imgIndex=8  },
            // Kael → image 8
            new Panel { speaker="Kael",       text="\"Then I'll stop it.\"",                                                                       imgIndex=7  },
            // "Not the strongest." → image 8
            new Panel { speaker="",           text="Not the strongest.",                                                                            imgIndex=7  },
            // "Not the richest." → image 8
            new Panel { speaker="",           text="Not the richest.",                                                                              imgIndex=7  },
            // "Not the chosen one." → image 8
            new Panel { speaker="",           text="Not the chosen one.",                                                                           imgIndex=7  },
            // "Just a man with courage..." → image 9
            new Panel { speaker="",           text="Just a man with courage greater than fear.",                                                    imgIndex=8  },
            // King → image 28
            new Panel { speaker="King Aldric",text="\"Thousands died down there.\"",                                                               imgIndex=27 },
            // Kael → image 8
            new Panel { speaker="Kael",       text="\"Then I'll carry their will with me.\"",                                                      imgIndex=7  },
            // "The king hands Kael an ancient sword." → image 24
            new Panel { speaker="",           text="The king hands Kael an ancient sword.",                                                        imgIndex=23 },
            // King → image 29
            new Panel { speaker="King Aldric",text="\"Return alive, Sir Kael.\"",                                                                  imgIndex=28 },
            // Kael → image 7
            new Panel { speaker="Kael",       text="\"I'll return when the darkness dies.\"",                                                      imgIndex=6  },
            // "Kael walks toward the abyss gate..." → image 6
            new Panel { speaker="",           text="Kael walks toward the giant abyss gate as thunder strikes.",                                    imgIndex=5  },
            // Empty dungeon → image 22
            new Panel { speaker="",           text="",                                                                                              imgIndex=21 },
            // Exploring deeper → image 14
            new Panel { speaker="",           text="",                                                                                              imgIndex=13 },
            // "He sees a lot of demon ghosts..." → image 14
            new Panel { speaker="",           text="He sees a lot of demon ghosts roaming the dungeon.",                                            imgIndex=13 },
            // Kael → image 8
            new Panel { speaker="Kael",       text="\"Bless me all saints and gods up there…\"",                                                   imgIndex=7  },
            // Kael → image 8
            new Panel { speaker="Kael",       text="\"For the sake of our kingdom and my family.\"",                                               imgIndex=7  },
        };
    }

    // ── Runtime state ─────────────────────────────────────────────────────
    private bool inputPressed  = false;
    private bool sequenceDone  = false;
    private bool usingA        = true;   // which buffer is currently "front"

    private void Start()
    {
        // Start fully black
        SetAlpha(blackOverlay, 1f);
        SetAlpha(imageA, 0f);
        SetAlpha(imageB, 0f);
        if (scrollPanel != null) scrollPanel.SetActive(false);
        if (speakerText != null) speakerText.gameObject.SetActive(false);

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
        // First panel — fade in from black
        if (panels.Length > 0)
        {
            Image front = imageA;
            front.sprite = GetSprite(panels[0].imgIndex);
            SetAlpha(front, 0f);
            usingA = true;

            // Fade black overlay out while first image fades in
            yield return StartCoroutine(FadeFirstIn(front, 1.5f));

            yield return StartCoroutine(ShowText(panels[0]));
            inputPressed = false;
            yield return StartCoroutine(WaitForInput(holdDuration));
        }

        // Remaining panels — crossfade between images
        for (int i = 1; i < panels.Length; i++)
        {
            inputPressed = false;

            Image front = usingA ? imageA : imageB;
            Image back  = usingA ? imageB : imageA;

            // Load next image into back buffer
            back.sprite = GetSprite(panels[i].imgIndex);
            SetAlpha(back, 0f);

            // Hide scroll during transition
            if (scrollPanel != null) scrollPanel.SetActive(false);
            if (speakerText != null) speakerText.gameObject.SetActive(false);

            // Crossfade: front fades out, back fades in
            yield return StartCoroutine(Crossfade(front, back, crossfadeDuration));

            // Swap buffers
            usingA = !usingA;

            // Show text on new image
            yield return StartCoroutine(ShowText(panels[i]));

            inputPressed = false;
            yield return StartCoroutine(WaitForInput(holdDuration));
        }

        // Final fade to black then load LEVEL 1
        sequenceDone = true;
        Image current = usingA ? imageA : imageB;
        if (scrollPanel != null) scrollPanel.SetActive(false);
        if (speakerText != null) speakerText.gameObject.SetActive(false);

        yield return StartCoroutine(FadeLastOut(current, 1.2f));
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(nextSceneName);
    }

    // ── Show text for a panel ─────────────────────────────────────────────
    private IEnumerator ShowText(Panel p)
    {
        bool hasText = p.text.Length > 0;
        if (scrollPanel != null) scrollPanel.SetActive(hasText);

        if (speakerText != null)
        {
            speakerText.text = p.speaker;
            speakerText.gameObject.SetActive(p.speaker.Length > 0);
        }

        if (hasText)
            yield return StartCoroutine(TypeText(p.text));
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

    // ── Crossfade between two images ──────────────────────────────────────
    private IEnumerator Crossfade(Image outImg, Image inImg, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t      = Mathf.Clamp01(elapsed / duration);
            float smooth = t * t * (3f - 2f * t);
            SetAlpha(outImg, 1f - smooth);
            SetAlpha(inImg,  smooth);
            yield return null;
        }
        SetAlpha(outImg, 0f);
        SetAlpha(inImg,  1f);
    }

    // ── First panel: fade black overlay out + image in ────────────────────
    private IEnumerator FadeFirstIn(Image img, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float s = t * t * (3f - 2f * t);
            SetAlpha(blackOverlay, 1f - s);
            SetAlpha(img, s);
            yield return null;
        }
        SetAlpha(blackOverlay, 0f);
        SetAlpha(img, 1f);
    }

    // ── Last panel: fade current image + fade black in ────────────────────
    private IEnumerator FadeLastOut(Image img, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float s = t * t * (3f - 2f * t);
            SetAlpha(img,          1f - s);
            SetAlpha(blackOverlay, s);
            yield return null;
        }
        SetAlpha(img,          0f);
        SetAlpha(blackOverlay, 1f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private Sprite GetSprite(int index)
    {
        if (storySprites == null || index < 0 || index >= storySprites.Length) return null;
        return storySprites[index];
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = a; img.color = c;
    }
}
