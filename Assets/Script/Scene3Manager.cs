using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Scene3 — Between LEVEL 2 and LEVEL 3. Smooth crossfade, no black overlay between panels.
///
/// Canvas hierarchy:
///   Canvas
///   ├── StoryImageA    (full screen, alpha=0)
///   ├── StoryImageB    (full screen, alpha=0)
///   ├── BlackOverlay   (full screen black, alpha=1 — only for first/last fade)
///   ├── ScrollPanel    (12.png scroll)
///   │   ├── SpeakerText
///   │   └── StoryText
/// </summary>
public class Scene3Manager : MonoBehaviour
{
    [Header("UI References")]
    public Image           imageA;
    public Image           imageB;
    public Image           blackOverlay;
    public GameObject      scrollPanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI storyText;

    [Header("Story Sprites")]
    [Tooltip("Slot 0=19.png, Slot 1=10.png, Slot 2=18.png, Slot 3=11.png, Slot 4=17.png")]
    public Sprite[] storySprites;   // size 5

    [Header("Timing")]
    public float crossfadeDuration = 1.2f;
    public float typewriterSpeed   = 38f;
    public float holdDuration      = 1.0f;

    [Header("Next Scene")]
    public string nextSceneName = "LEVEL 3";

    private struct Panel
    {
        public string speaker;
        public string text;
        public int    imgIndex;  // -1 = keep black (no image change)
    }

    private readonly Panel[] panels = new Panel[]
    {
        // Slot 0 = 19.png — no text, just visual
        new Panel { speaker="",        text="",                                                                    imgIndex=0 },
        // Slot 1 = 10.png — Kael dialogue
        new Panel { speaker="Kael",    text="\"They are so many, I almost didn't make it!\"",                     imgIndex=1 },
        // Slot 2 = 18.png — Unknown voice
        new Panel { speaker="Unknown", text="\"You think you are strong enough? You are a fool, human!\"",        imgIndex=2 },
        // Slot 3 = 11.png — Kael responds
        new Panel { speaker="Kael",    text="\"Who are you!\"",                                                    imgIndex=3 },
        // Slot 4 = 17.png — Unknown final line
        new Panel { speaker="Unknown", text="\"Let's see how further you can go...\"",                            imgIndex=4 },
    };

    private bool inputPressed = false;
    private bool sequenceDone = false;
    private bool usingA       = true;
    private bool hasImage     = false;  // tracks if any image is currently shown

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
        for (int i = 0; i < panels.Length; i++)
        {
            Panel p = panels[i];
            inputPressed = false;

            if (i == 0)
            {
                // First panel — if no image, just fade black overlay slightly
                // to show text on near-black background
                if (p.imgIndex < 0)
                {
                    // Fade overlay to 90% (slightly transparent so scroll is visible)
                    yield return StartCoroutine(FadeTo(blackOverlay, 1f, 0.9f, 0.4f));
                }
                else
                {
                    // First panel has image — fade in from black
                    imageA.sprite = GetSprite(p.imgIndex);
                    SetAlpha(imageA, 0f);
                    usingA   = true;
                    hasImage = true;
                    yield return StartCoroutine(FadeFirstIn(imageA, 1.5f));
                }
            }
            else
            {
                // Subsequent panels
                if (p.imgIndex >= 0)
                {
                    Image front = usingA ? imageA : imageB;
                    Image back  = usingA ? imageB : imageA;

                    back.sprite = GetSprite(p.imgIndex);
                    SetAlpha(back, 0f);

                    if (scrollPanel != null) scrollPanel.SetActive(false);
                    if (speakerText != null) speakerText.gameObject.SetActive(false);

                    if (hasImage)
                    {
                        // Crossfade from current image to new image
                        yield return StartCoroutine(Crossfade(front, back, crossfadeDuration));
                    }
                    else
                    {
                        // Coming from black — fade black out, image in
                        yield return StartCoroutine(FadeFirstIn(back, 1.5f));
                    }

                    usingA   = !usingA;
                    hasImage = true;
                }
                // If imgIndex == -1 and we already have an image, keep it showing
            }

            // Show text
            yield return StartCoroutine(ShowText(p));
            inputPressed = false;
            yield return StartCoroutine(WaitForInput(holdDuration));
        }

        // Fade to black → load LEVEL 3
        sequenceDone = true;
        if (scrollPanel != null) scrollPanel.SetActive(false);
        if (speakerText != null) speakerText.gameObject.SetActive(false);

        Image current = usingA ? imageA : imageB;
        if (hasImage)
            yield return StartCoroutine(FadeLastOut(current, 1.2f));
        else
            yield return StartCoroutine(FadeTo(blackOverlay, GetAlpha(blackOverlay), 1f, 0.8f));

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

    private IEnumerator FadeTo(Image img, float from, float to, float duration)
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

    private float Smooth(float t) => t * t * (3f - 2f * t);

    private Sprite GetSprite(int i) =>
        (storySprites != null && i >= 0 && i < storySprites.Length) ? storySprites[i] : null;

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = a; img.color = c;
    }

    private float GetAlpha(Image img) => img == null ? 0f : img.color.a;
}
