using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    // 💾 Cross-scene memory tracking
    public static int SavedHealth = -1;

    // 📸 NEW CHECKPOINT LAYER: Remembers exactly what health you entered the current level with!
    public static int LevelStartHealth = -1;

    [Header("Health Core Parameters")]
    public int maxHealth = 5;

    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        private set
        {
            currentHealth = value;
            SavedHealth = currentHealth; // Keep the cross-scene save perfectly in sync!
            OnHealthChanged?.Invoke(currentHealth);
        }
    }

    // 🔌 Global System Event Wires
    public static event Action<int> OnHealthChanged;
    public static event Action OnPlayerDeath; // 💀 Broadcasts globally when HP hits 0

    [Header("Combat Mechanics")]
    public float invincibilityDuration = 1f;
    public bool isGuarding { get; private set; }

    private void Awake()
    {
        // 1. Establish current health from cross-scene memory
        if (SavedHealth > 0)
        {
            currentHealth = SavedHealth;
        }
        else
        {
            currentHealth = maxHealth;
            SavedHealth = maxHealth;
        }

        // 2. Take a snapshot of our entrance health right as this specific level boots up!
        LevelStartHealth = currentHealth;
        Debug.Log($"[PlayerHealth] Scene Checkpoint Captured! Entering level with: {LevelStartHealth} HP.");
    }

    private void Start()
    {
        // Immediate UI heart render pass right on map load
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    public void SetGuarding(bool guarding)
    {
        isGuarding = guarding;
    }

    public void TakeDamage(int damage)
    {
        if (isGuarding)
        {
            Debug.Log("[PlayerHealth] Attack mitigated by active guard!");
            return;
        }

        // Clamp health processing so it never drops below absolute zero
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        Debug.Log($"[PlayerHealth] Hurt processed. HP remaining: {CurrentHealth}/{maxHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Safely restores a set amount of hearts, clamping tightly to max capacity.
    /// </summary>
    public void Heal(int healAmount)
    {
        // Safety verification: Don't heal if the player is already dead
        if (CurrentHealth <= 0) return;

        // Increment health and use Mathf.Min to ensure we never blast past maxHealth limit!
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + healAmount);

        Debug.Log($"[PlayerHealth] Heart pickup processed! Current HP: {CurrentHealth}/{maxHealth}");
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Character flatlined. Triggering Game Over loop.");

        // Clear active tracking memory
        SavedHealth = -1;

        // 🚀 FIRE THE FLARE: Alert the PauseMenu UI manager to turn on the Game Over screen!
        OnPlayerDeath?.Invoke();
    }
}