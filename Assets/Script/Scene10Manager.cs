using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Scene10 — Final Battle Begins. Branches on win/lose outcome from LEVEL 10.
///
/// Story Sprites — assign in this exact order (Size = 5):
///   Slot 0 = 71.png   (battle clash / Kael falls)
///   Slot 1 = 70.png   (steel vs demonic power)
///   Slot 2 = 72.png   (Kael delivers final strike — WIN only)
///   Slot 3 = 67.png   (Devil turns to ashes — WIN only)
///   Slot 4 = 69.png   (Kael returns as hero / peace restored — WIN only)
///
/// Win branch  → plays full victory epilogue, then loads "GameMenu"
/// Lose branch → plays defeat panels, then loads "GameMenu"
///
/// Canvas hierarchy (same as all other scenes):
///   Canvas
///   ├── StoryImageA    (full screen, alpha=0)
///   ├── StoryImageB    (full screen, alpha=0)
///   ├── BlackOverlay   (full screen black, alpha=1)
///   ├── ScrollPanel    (scroll background)
///   │   ├── SpeakerText
///   │   └── StoryText
/// </summary>
public class Scene10Manager : MonoBehaviour
{
    [Header("UI References")]
    public Image           imageA;
    public Image           imageB;
    public Image           blackOverlay;
    public GameObject      scrollPanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI storyText;

    [Header("Story Sprites — size 5, assign in order shown above")]
    public Sprite[] storySprites;

    [Header("Timing")]
    public float crossfadeDuration = 1.2f;
    public float typewriterSpeed   = 38f;
    public float holdDuration      = 1.0f;

    [Header("End Destination")]
    public string endSceneName = "GameMenu";

    // ── Panel definition ──────────────────────────────────────────────────
    private struct Panel
    {
        public string speaker;
        public string text;
        public int    imgIndex; // index into storySprites
    }

    // ── Shared opening panels (always shown) ─────────────────────────────
    // Slot 0 = 71.png, Slot 1 = 70.png
    private readonly Panel[] openingPanels = new Panel[]
    {
        new Panel { speaker="",         text="The clash between Kael and the Devil shakes the entire dungeon.",                          imgIndex=0 },
        new Panel { speaker="",         text="Steel collides against demonic power as the fate of the kingdom rests on one final fight.", imgIndex=1 },
    };

    // ── WIN branch panels ─────────────────────────────────────────────────
    // Slot 2 = 72.png, Slot 3 = 67.png, Slot 4 = 69.png
    private readonly Panel[] winPanels = new Panel[]
    {
        new Panel { speaker="",                  text="After an exhausting battle, Sir Kael delivers the final strike.",                 imgIndex=2 },
        new Panel { speaker="",                  text="The Devil lets out a monstrous roar before turning into ashes.",                  imgIndex=3 },
        new Panel { speaker="",                  text="Kael escapes Noctyra and finally returns to the kingdom as a hero.",              imgIndex=4 },
        new Panel { speaker="",                  text="Peace is restored once again.",                                                   imgIndex=4 },
        new Panel { speaker="Kingdom Narrator",  text="\"The great knight who conquered the darkness… saved the kingdom.\"",            imgIndex=4 },
    };

    // ── LOSE branch panels ────────────────────────────────────────────────
    // Slot 0 = 71.png (reused)
    private readonly Panel[] losePanels = new Panel[]
    {
        new Panel { speaker="", text="Sir Kael falls before the Devil.", imgIndex=0 },
        new Panel { speaker="", text="His sword shatters across the floor.", imgIndex=0 },
    };

    // ── Runtime state ─────────────────────────────────────────────────────
    private bool inputPressed = false;
    private bool sequenceDone = false;
    private bool usingA       = true;

    private void Start()
    {
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
        // Determine outcome: SavedHealth > 0 means the player survived LEVEL 10
        bool playerWon = PlayerHealth.SavedHealth > 0;

        // Build the full panel list: opening + branch
        Panel[] branchPanels = playerWon ? winPanels : losePanels;
        int totalCount = openingPanels.Length + branchPanels.Length;
        Panel[] allPanels = new Panel[totalCount];
        openingPanels.CopyTo(allPanels, 0);
        branchPanels.CopyTo(allPanels, openingPanels.Length);

        int currentImgIndex = allPanels[0].imgIndex;

        // First panel — fade in from black
        imageA.sprite = GetSprite(currentImgIndex);
        imageB.sprite = GetSprite(currentImgIndex);
        SetAlpha(imageA, 0f);
        SetAlpha(imageB, 0f);
        usingA = true;
        yield return StartCoroutine(FadeFirstIn(imageA, 1.5f));
        yield return StartCoroutine(ShowText(allPanels[0]));
        inputPressed = false;
        yield return StartCoroutine(WaitForInput(holdDuration));

        // Remaining panels — crossfade only when image changes
        for (int i = 1; i < allPanels.Length; i++)
        {
            inputPressed = false;
            int nextImgIndex = allPanels[i].imgIndex;

            if (nextImgIndex != currentImgIndex)
            {
                Image front = usingA ? imageA : imageB;
                Image back  = usingA ? imageB : imageA;

                if (scrollPanel != null) scrollPanel.SetActive(false);
                if (speakerText != null) speakerText.gameObject.SetActive(false);

                back.sprite = GetSprite(nextImgIndex);
                SetAlpha(back, 0f);
                yield return StartCoroutine(Crossfade(front, back, crossfadeDuration));
                usingA = !usingA;
                currentImgIndex = nextImgIndex;
            }

            yield return StartCoroutine(ShowText(allPanels[i]));
            inputPressed = false;
            yield return StartCoroutine(WaitForInput(holdDuration));
        }

        // Fade to black → load end scene
        sequenceDone = true;
        if (scrollPanel != null) scrollPanel.SetActive(false);
        if (speakerText != null) speakerText.gameObject.SetActive(false);
        Image current = usingA ? imageA : imageB;
        yield return StartCoroutine(FadeLastOut(current, 1.2f));
        yield return new WaitForSeconds(0.4f);

        // Reset saved health so next playthrough starts fresh
        PlayerHealth.SavedHealth = -1;
        SceneManager.LoadScene(endSceneName);
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
        if (hasText) yield return StartCoroutine(TypeText(p.text));
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
        while (!inputPressed && t < maxWait) { t += Time.deltaTime; yield return null; }
        inputPressed = false;
    }

    // ── Crossfade between two images ──────────────────────────────────────
    private IEnumerator Crossfade(Image outImg, Image inImg, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float s = Smooth(Mathf.Clamp01(elapsed / duration));
            SetAlpha(outImg, 1f - s);
            SetAlpha(inImg,  s);
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
            float s = Smooth(Mathf.Clamp01(elapsed / duration));
            SetAlpha(blackOverlay, 1f - s);
            SetAlpha(img, s);
            yield return null;
        }
        SetAlpha(blackOverlay, 0f);
        SetAlpha(img, 1f);
    }

    // ── Last panel: fade current image out + black in ─────────────────────
    private IEnumerator FadeLastOut(Image img, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float s = Smooth(Mathf.Clamp01(elapsed / duration));
            SetAlpha(img,          1f - s);
            SetAlpha(blackOverlay, s);
            yield return null;
        }
        SetAlpha(img,          0f);
        SetAlpha(blackOverlay, 1f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private float Smooth(float t) => t * t * (3f - 2f * t);

    private Sprite GetSprite(int i) =>
        (storySprites != null && i >= 0 && i < storySprites.Length) ? storySprites[i] : null;

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = a; img.color = c;
    }
}
