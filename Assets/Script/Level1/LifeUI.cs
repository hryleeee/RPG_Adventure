using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the player's health as a row of heart icons in the HUD.
///
/// ── How it works ─────────────────────────────────────────────────────────
///  • You create 5 UI Image objects (one per heart) in a Canvas.
///  • Each Image uses the LIFE sprite.
///  • This script shows/hides them based on the player's current HP.
///  • When the player takes damage, hearts disappear from right to left.
///
/// ── Setup in Unity ───────────────────────────────────────────────────────
///  1. Create a Canvas (if you don't have one):
///       GameObject → UI → Canvas
///       Canvas Scaler → UI Scale Mode: Scale With Screen Size
///       Reference Resolution: 1920 × 1080
///
///  2. Inside the Canvas, create an empty GameObject → name it "LifePanel"
///       Anchor: Top-Left  |  Pos X: 20, Pos Y: -20
///       Add a Horizontal Layout Group component:
///         Spacing: 8  |  Child Force Expand Width & Height: OFF
///
///  3. Inside LifePanel, create 5 UI Images → name them
///       Heart1, Heart2, Heart3, Heart4, Heart5
///       Each Image:
///         Source Image → assign the LIFE sprite (the heart icon)
///         Width: 32  Height: 32
///         Preserve Aspect: ON
///
///  4. Create another empty GameObject in the scene → name it "LifeUI"
///       Add this script (LifeUI.cs) to it
///
///  5. In the Inspector on LifeUI:
///       • Drag the Player GameObject into "Player"
///       • Set Max Hearts to 5
///       • Expand "Heart Images" array to size 5
///       • Drag Heart1 → Heart2 → ... → Heart5 into the slots
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class LifeUI : MonoBehaviour
{
    [Header("Player Reference")]
    [Tooltip("Drag the Player GameObject here")]
    public PlayerHealth player;

    [Header("Heart Images")]
    [Tooltip("Drag all 5 heart Image objects here in order (Heart1 … Heart5)")]
    public Image[] heartImages;

    [Header("Settings")]
    [Tooltip("Must match PlayerHealth.maxHealth (default 5)")]
    public int maxHearts = 5;

    [Tooltip("Color when the heart is full (alive)")]
    public Color fullColor = Color.white;

    [Tooltip("Color when the heart is empty (lost). Set alpha to 0 to hide, or use a dark tint.")]
    public Color emptyColor = new Color(1f, 1f, 1f, 0.15f);

    private void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.GetComponent<PlayerHealth>();
        }

        if (player == null)
        {
            Debug.LogError("[LifeUI] PlayerHealth not found. Tag the Player as 'Player' or assign it manually.");
            return;
        }

        // Subscribe to health change events
        player.OnHealthChanged += UpdateHearts;

        // Draw the initial state (full health)
        UpdateHearts(player.CurrentHealth);
    }

    private void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks
        if (player != null)
            player.OnHealthChanged -= UpdateHearts;
    }

    /// <summary>
    /// Called automatically whenever the player's health changes.
    /// Shows full hearts up to currentHealth, dims the rest.
    /// </summary>
    private void UpdateHearts(int currentHealth)
    {
        if (heartImages == null) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;

            // Hearts 0..(currentHealth-1) are full; the rest are empty
            bool isFull = i < currentHealth;
            heartImages[i].color = isFull ? fullColor : emptyColor;
        }
    }
}
