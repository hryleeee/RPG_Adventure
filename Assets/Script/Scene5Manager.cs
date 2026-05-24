using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Scene5 — The Witch of Noctyra. Smooth crossfade, no black overlay between panels.
///
/// Story Sprites — assign in this exact order (Size = 8):
///   Slot 0 = 43.png
///   Slot 1 = 7.png
///   Slot 2 = 47.png
///   Slot 3 = 10.png
///   Slot 4 = 42.png
///   Slot 5 = 11.png
///   Slot 6 = 8.png
///   Slot 7 = 45.png
///   Slot 8 = 46.png
///
/// Canvas hierarchy (same as all other scenes):
///   Canvas
///   ├── StoryImageA    (full screen, alpha=0)
///   ├── StoryImageB    (full screen, alpha=0)
///   ├── BlackOverlay   (full screen black, alpha=1)
///   ├── ScrollPanel    (12.png scroll)
///   │   ├── SpeakerText
///   │   └── StoryText
/// </summary>
public class Scene5Manager : MonoBehaviour
{
    [Header("UI References")]
    public Image           imageA;
    public Image           imageB;
    public Image           blackOverlay;
    public GameObject      scrollPanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI storyText;

    [Header("Story Sprites — size 9")]
    public Sprite[] storySprites;

    [Header("Timing")]
    public float crossfadeDuration = 1.2f;
    public float typewriterSpeed   = 38f;
    public float holdDuration      = 1.0f;

    [Header("Next Scene")]
    public string nextSceneName = "LEVEL 5";

    private struct Panel
    {
        public string speaker;
        public string text;
        public int    imgIndex; // slot in storySprites
    }

    // Slot map:
    // 0=43  1=7  2=47  3=10  4=42  5=11  6=8  7=45  8=46
    private Panel[] panels;

    private void Awake()
    {
        panels = new Panel[]
        {
            // Slot 0 = 43.png
            new Panel { speaker="",                  text="Sir Kael slowly walks through the dark hallway of Noctyra, breathing heavily after surviving countless monsters from the lower floors.",  imgIndex=0 },
            // Slot 0 = 43.png (same)
            new Panel { speaker="",                  text="The dungeon is silent.",                                                                                                                   imgIndex=0 },
            // Slot 1 = 7.png
            new Panel { speaker="Sir Kael",          text="\"After all those monsters… I can't believe I made it this deep.\"",                                                                       imgIndex=1 },
            // Slot 2 = 47.png
            new Panel { speaker="",                  text="He continues walking carefully.",                                                                                                          imgIndex=2 },
            // Slot 2 = 47.png (same)
            new Panel { speaker="Sir Kael",          text="\"I must survive… for my family… and for the kingdom.\"",                                                                                  imgIndex=2 },
            // Slot 1 = 7.png
            new Panel { speaker="",                  text="Suddenly—",                                                                                                                               imgIndex=1 },
            // Slot 3 = 10.png
            new Panel { speaker="",                  text="A cold whisper echoes through the hallway.",                                                                                              imgIndex=3 },
            // Slot 4 = 42.png
            new Panel { speaker="Unknown Voice",     text="\"Fool…\"",                                                                                                                               imgIndex=4 },
            // Slot 5 = 11.png
            new Panel { speaker="",                  text="Kael instantly turns around.",                                                                                                            imgIndex=5 },
            // Slot 5 = 11.png (same)
            new Panel { speaker="Sir Kael",          text="\"Who's there?!\"",                                                                                                                       imgIndex=5 },
            // Slot 3 = 10.png
            new Panel { speaker="",                  text="Dark purple smoke begins gathering in the shadows.",                                                                                       imgIndex=3 },
            // Slot 4 = 42.png
            new Panel { speaker="",                  text="A woman slowly emerges from the darkness.",                                                                                               imgIndex=4 },
            // Slot 4 = 42.png (same)
            new Panel { speaker="Witch of Noctyra",  text="\"Foolish human…\"",                                                                                                                      imgIndex=4 },
            // Slot 6 = 8.png
            new Panel { speaker="",                  text="Kael raises his sword.",                                                                                                                  imgIndex=6 },
            // Slot 6 = 8.png (same)
            new Panel { speaker="Sir Kael",          text="\"Show yourself! I command you!\"",                                                                                                       imgIndex=6 },
            // Slot 7 = 45.png
            new Panel { speaker="",                  text="The witch smirks.",                                                                                                                       imgIndex=7 },
            // Slot 7 = 45.png (same)
            new Panel { speaker="Witch of Noctyra",  text="\"No human commands the Witches of Noctyra.\"",                                                                                           imgIndex=7 },
            // Slot 7 = 45.png (same)
            new Panel { speaker="Witch of Noctyra",  text="\"Your people deserve to perish.\"",                                                                                                      imgIndex=7 },
            // Slot 5 = 11.png
            new Panel { speaker="",                  text="Kael steps forward fearlessly.",                                                                                                          imgIndex=5 },
            // Slot 6 = 8.png
            new Panel { speaker="Sir Kael",          text="\"Then I'll stop this darkness myself.\"",                                                                                                imgIndex=6 },
            // Slot 8 = 46.png
            new Panel { speaker="",                  text="The witch laughs softly as shadows begin crawling across the floor.",                                                                      imgIndex=8 },
        };
    }

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

    private IEnumerator RunSequence()
    {
        int currentImgIndex = panels[0].imgIndex;

        // First panel — fade in from black
        imageA.sprite = GetSprite(currentImgIndex);
        imageB.sprite = GetSprite(currentImgIndex);
        SetAlpha(imageA, 0f);
        SetAlpha(imageB, 0f);
        usingA = true;
        yield return StartCoroutine(FadeFirstIn(imageA, 1.5f));
        yield return StartCoroutine(ShowText(panels[0]));
        inputPressed = false;
        yield return StartCoroutine(WaitForInput(holdDuration));

        // Remaining panels
        for (int i = 1; i < panels.Length; i++)
        {
            inputPressed = false;
            int nextImgIndex = panels[i].imgIndex;

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

            yield return StartCoroutine(ShowText(panels[i]));
            inputPressed = false;
            yield return StartCoroutine(WaitForInput(holdDuration));
        }

        // Fade to black → load LEVEL 5
        sequenceDone = true;
        if (scrollPanel != null) scrollPanel.SetActive(false);
        if (speakerText != null) speakerText.gameObject.SetActive(false);
        Image current = usingA ? imageA : imageB;
        yield return StartCoroutine(FadeLastOut(current, 1.2f));
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(nextSceneName);
    }

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
        while (!inputPressed && t < maxWait) { t += Time.deltaTime; yield return null; }
        inputPressed = false;
    }

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

    private float Smooth(float t) => t * t * (3f - 2f * t);

    private Sprite GetSprite(int i) =>
        (storySprites != null && i >= 0 && i < storySprites.Length) ? storySprites[i] : null;

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = a; img.color = c;
    }
}
