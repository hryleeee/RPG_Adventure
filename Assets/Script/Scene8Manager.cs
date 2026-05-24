using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Scene8 — Deeper into Noctyra. Smooth crossfade, no black overlay between panels.
///
/// Story Sprites — assign in this exact order (Size = 5):
///   Slot 0 = 55.png
///   Slot 1 = 23.png
///   Slot 2 = 19.png
///   Slot 3 = 54.png
///   Slot 4 = 35.png
/// </summary>
public class Scene8Manager : MonoBehaviour
{
    [Header("UI References")]
    public Image           imageA;
    public Image           imageB;
    public Image           blackOverlay;
    public GameObject      scrollPanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI storyText;

    [Header("Story Sprites — size 5")]
    public Sprite[] storySprites;

    [Header("Timing")]
    public float crossfadeDuration = 1.2f;
    public float typewriterSpeed   = 38f;
    public float holdDuration      = 1.0f;

    [Header("Next Scene")]
    public string nextSceneName = "LEVEL 8";

    private struct Panel
    {
        public string speaker;
        public string text;
        public int    imgIndex;
    }

    // Slot map: 0=55  1=23  2=19  3=54  4=35
    private readonly Panel[] panels = new Panel[]
    {
        // Slot 0 = 55.png
        new Panel { speaker="",         text="Sir Kael continues descending deeper into Noctyra.",                                                                    imgIndex=0 },
        new Panel { speaker="",         text="Demon hounds.",                                                                                                         imgIndex=0 },
        // Slot 1 = 23.png
        new Panel { speaker="",         text="Bloodthirsty ghouls.",                                                                                                  imgIndex=1 },
        // Slot 2 = 19.png
        new Panel { speaker="",         text="But Kael shows no fear.",                                                                                               imgIndex=2 },
        // Slot 3 = 54.png
        new Panel { speaker="",         text="Steel clashes violently against claws and fangs as battle after battle echoes through the cursed halls.",               imgIndex=3 },
        new Panel { speaker="",         text="One monster falls.",                                                                                                    imgIndex=3 },
        new Panel { speaker="",         text="Then another.",                                                                                                         imgIndex=3 },
        new Panel { speaker="",         text="Kael stands among them, breathing heavily while holding his bloodstained sword.",                                       imgIndex=3 },
        // Slot 4 = 35.png
        new Panel { speaker="Sir Kael", text="\"No darkness will stop me.\"",                                                                                        imgIndex=4 },
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
