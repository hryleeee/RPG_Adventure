using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Scene4 — The Fallen Knight. Smooth crossfade, no black overlay between panels.
///
/// Story Sprites order (assign in Inspector):
///   Slot 0  = 21.png   Slot 1  = 35.png   Slot 2  = 33.png   Slot 3  = 36.png
///   Slot 4  = 34.png   Slot 5  = 32.png   Slot 6  = 41.png   Slot 7  = 40.png
///   Slot 8  = 37.png   Slot 9  = 38.png   Slot 10 = 39.png   Slot 11 = 8.png
///   Slot 12 = 20.png
///
/// Canvas hierarchy:
///   Canvas
///   ├── StoryImageA    (full screen, alpha=0)
///   ├── StoryImageB    (full screen, alpha=0)
///   ├── BlackOverlay   (full screen black, alpha=1)
///   ├── ScrollPanel    (12.png scroll)
///   │   ├── SpeakerText
///   │   └── StoryText
/// </summary>
public class Scene4Manager : MonoBehaviour
{
    [Header("UI References")]
    public Image           imageA;
    public Image           imageB;
    public Image           blackOverlay;
    public GameObject      scrollPanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI storyText;

    [Header("Story Sprites — size 13, assign in order shown above")]
    public Sprite[] storySprites;

    [Header("Timing")]
    public float crossfadeDuration = 1.2f;
    public float typewriterSpeed   = 38f;
    public float holdDuration      = 1.0f;

    [Header("Next Scene")]
    public string nextSceneName = "LEVEL 4";

    private struct Panel
    {
        public string speaker;
        public string text;
        public int    imgIndex; // index into storySprites array
    }

    // imgIndex = slot number in storySprites (0-based)
    private Panel[] panels;

    private void Awake()
    {
        panels = new Panel[]
        {
            // Slot 0 = 21.png
            new Panel { speaker="",              text="After surviving countless monsters from the lower dungeon floors, Sir Kael enters a massive hallway hidden deep within Noctyra.", imgIndex=0 },
            // Slot 1 = 35.png
            new Panel { speaker="",              text="The hallway is enormous, lined with broken pillars and statues of ancient knights covered in dust and blood. Dim blue flames flicker along the walls.", imgIndex=1 },
            // Slot 2 = 33.png
            new Panel { speaker="",              text="Kael slowly walks forward.",                                imgIndex=2 },
            // Slot 3 = 36.png
            new Panel { speaker="",              text="A figure lies motionless ahead.",                           imgIndex=3 },
            // Slot 4 = 34.png
            new Panel { speaker="",              text="A knight.",                                                 imgIndex=4 },
            // Slot 5 = 32.png
            new Panel { speaker="Sir Kael",      text="\"Hey… stay awake!\"",                                     imgIndex=5 },
            // Slot 5 = 32.png (Kael kneels)
            new Panel { speaker="",              text="Kael kneels beside him.",                                   imgIndex=5 },
            // Slot 6 = 41.png
            new Panel { speaker="",              text="The wounded knight slowly opens his eyes.",                 imgIndex=6 },
            // Slot 5 = 32.png
            new Panel { speaker="Fallen Knight", text="\"…another one…\"",                                        imgIndex=5 },
            // Slot 5 = 32.png
            new Panel { speaker="Sir Kael",      text="\"Who did this to you?\"",                                 imgIndex=5 },
            // Slot 7 = 40.png
            new Panel { speaker="",              text="The knight trembles in fear.",                              imgIndex=7 },
            // Slot 8 = 37.png
            new Panel { speaker="Fallen Knight", text="\"…Not… who…\"",                                           imgIndex=8 },
            // Slot 8 = 37.png
            new Panel { speaker="Fallen Knight", text="\"…WHAT.\"",                                               imgIndex=8 },
            // Slot 7 = 40.png
            new Panel { speaker="Fallen Knight", text="\"Listen to me carefully…\"",                              imgIndex=7 },
            // Slot 7 = 40.png
            new Panel { speaker="Sir Kael",      text="\"Save your strength.\"",                                  imgIndex=7 },
            // Slot 8 = 37.png
            new Panel { speaker="Fallen Knight", text="\"You don't understand…\"",                                imgIndex=8 },
            // Slot 8 = 37.png
            new Panel { speaker="",              text="The knight begins crying.",                                 imgIndex=8 },
            // Slot 8 = 37.png
            new Panel { speaker="Fallen Knight", text="\"The deeper floors… they're alive.\"",                    imgIndex=8 },
            // Slot 5 = 32.png
            new Panel { speaker="",              text="Kael's expression hardens.",                               imgIndex=5 },
            // Slot 5 = 32.png
            new Panel { speaker="Fallen Knight", text="\"The dungeon watches us…\"",                              imgIndex=5 },
            // Slot 7 = 40.png
            new Panel { speaker="Sir Kael",      text="\"What's waiting ahead?\"",                                imgIndex=7 },
            // Slot 7 = 40.png
            new Panel { speaker="",              text="The knight freezes.",                                       imgIndex=7 },
            // Slot 9 = 38.png
            new Panel { speaker="",              text="Pure terror fills his eyes.",                               imgIndex=9 },
            // Slot 9 = 38.png
            new Panel { speaker="Fallen Knight", text="\"A monster… that even demons fear.\"",                    imgIndex=9 },
            // Slot 8 = 37.png
            new Panel { speaker="Fallen Knight", text="\"…finish what we couldn't…\"",                            imgIndex=8 },
            // Slot 10 = 39.png
            new Panel { speaker="",              text="The knight's hand falls.",                                  imgIndex=10 },
            // Slot 10 = 39.png
            new Panel { speaker="",              text="Kael lowers his head respectfully.",                       imgIndex=10 },
            // Slot 10 = 39.png
            new Panel { speaker="",              text="Alone again.",                                              imgIndex=10 },
            // Slot 11 = 8.png
            new Panel { speaker="",              text="",                                                          imgIndex=11 },
            // Slot 12 = 20.png
            new Panel { speaker="",              text="He looks deeper into the endless darkness ahead.",          imgIndex=12 },
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
        // Track which imgIndex is currently displayed
        int currentImgIndex = panels[0].imgIndex;

        // First panel — fade in from black
        if (panels.Length > 0)
        {
            imageA.sprite = GetSprite(currentImgIndex);
            imageB.sprite = GetSprite(currentImgIndex); // keep both in sync
            SetAlpha(imageA, 0f);
            SetAlpha(imageB, 0f);
            usingA = true;
            yield return StartCoroutine(FadeFirstIn(imageA, 1.5f));
            yield return StartCoroutine(ShowText(panels[0]));
            inputPressed = false;
            yield return StartCoroutine(WaitForInput(holdDuration));
        }

        // Remaining panels — crossfade only when image changes
        for (int i = 1; i < panels.Length; i++)
        {
            inputPressed = false;

            if (scrollPanel != null) scrollPanel.SetActive(false);
            if (speakerText != null) speakerText.gameObject.SetActive(false);

            int nextImgIndex = panels[i].imgIndex;

            if (nextImgIndex != currentImgIndex)
            {
                // Image changes — crossfade
                Image front = usingA ? imageA : imageB;
                Image back  = usingA ? imageB : imageA;

                // Always set the correct sprite on back buffer before fading
                back.sprite = GetSprite(nextImgIndex);
                SetAlpha(back, 0f);

                yield return StartCoroutine(Crossfade(front, back, crossfadeDuration));
                usingA = !usingA;
                currentImgIndex = nextImgIndex;
            }
            // Same image — no crossfade, just show new text on same image

            yield return StartCoroutine(ShowText(panels[i]));
            inputPressed = false;
            yield return StartCoroutine(WaitForInput(holdDuration));
        }

        // Fade to black → load LEVEL 4
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
