using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Scene9 — The Devil's Voice. Smooth crossfade, no black overlay between panels.
///
/// Story Sprites — assign in this exact order (Size = 10):
///   Slot 0 = 60.png
///   Slot 1 = 65.png
///   Slot 2 = 52.png
///   Slot 3 = 61.png
///   Slot 4 = 57.png
///   Slot 5 = 59.png
///   Slot 6 = 58.png
///   Slot 7 = 17.png
///   Slot 8 = 18.png
/// </summary>
public class Scene9Manager : MonoBehaviour
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
    public string nextSceneName = "LEVEL 9";

    private struct Panel
    {
        public string speaker;
        public string text;
        public int    imgIndex;
    }

    // Slot map: 0=60  1=65  2=52  3=61  4=57  5=59  6=58  7=17  8=18
    private readonly Panel[] panels = new Panel[]
    {
        // Slot 0 = 60.png
        new Panel { speaker="",          text="Sir Kael fights through another wave of terrifying monsters deep within Noctyra.",                imgIndex=0 },
        // Slot 1 = 65.png
        new Panel { speaker="",          text="Outnumbered and wounded, he continues battling alone without fear.",                              imgIndex=1 },
        // Slot 2 = 52.png
        new Panel { speaker="",          text="For a moment, Kael nearly collapses.",                                                           imgIndex=2 },
        // Slot 3 = 61.png
        new Panel { speaker="",          text="But he slowly stands once more and grips his sword tightly.",                                    imgIndex=3 },
        // Slot 3 = 61.png (same)
        new Panel { speaker="Sir Kael",  text="\"This darkness ends tonight.\"",                                                                imgIndex=3 },
        // Slot 4 = 57.png
        new Panel { speaker="",          text="As Kael walks toward the next floor, the dungeon suddenly becomes darker and colder than before.", imgIndex=4 },
        // Slot 5 = 59.png
        new Panel { speaker="",          text="An eerie silence fills the hallway.",                                                            imgIndex=5 },
        // Slot 5 = 59.png (same)
        new Panel { speaker="",          text="Then—",                                                                                          imgIndex=5 },
        // Slot 6 = 58.png
        new Panel { speaker="",          text="A swarm of black bats flies past him violently.",                                                imgIndex=6 },
        // Slot 6 = 58.png (same)
        new Panel { speaker="",          text="A deep voice echoes throughout the dungeon.",                                                    imgIndex=6 },
        // Slot 7 = 17.png
        new Panel { speaker="The Devil", text="\"You are truly exceptional, human…\"",                                                         imgIndex=7 },
        // Slot 7 = 17.png (same)
        new Panel { speaker="",          text="Crimson eyes appear within the darkness.",                                                       imgIndex=7 },
        // Slot 7 = 17.png (same)
        new Panel { speaker="The Devil", text="\"You slaughtered my armies inside my dungeon.\"",                                              imgIndex=7 },
        // Slot 8 = 18.png
        new Panel { speaker="The Devil", text="\"I shall await you in my chamber…\"",                                                          imgIndex=8 },
        // Slot 8 = 18.png (same)
        new Panel { speaker="The Devil", text="\"Let us see how long you can withstand my power.\"",                                           imgIndex=8 },
        // Slot 5 = 59.png
        new Panel { speaker="",          text="Silence returns.",                                                                               imgIndex=5 },
        // Slot 6 = 58.png
        new Panel { speaker="",          text="Kael takes a deep breath, then fearlessly walks deeper into the abyss—",                        imgIndex=6 },
        // Slot 6 = 58.png (same)
        new Panel { speaker="",          text="Toward the Devil himself.",                                                                      imgIndex=6 },
    };

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

        imageA.sprite = GetSprite(currentImgIndex);
        imageB.sprite = GetSprite(currentImgIndex);
        SetAlpha(imageA, 0f);
        SetAlpha(imageB, 0f);
        usingA = true;
        yield return StartCoroutine(FadeFirstIn(imageA, 1.5f));
        yield return StartCoroutine(ShowText(panels[0]));
        inputPressed = false;
        yield return StartCoroutine(WaitForInput(holdDuration));

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
