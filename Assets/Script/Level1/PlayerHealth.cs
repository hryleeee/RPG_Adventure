using System;
using UnityEngine;

/// <summary>
/// Tracks the player's health and broadcasts changes so the UI can react.
/// Attach this to the Player GameObject.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Maximum / starting health (set to 5 for 5 hearts)")]
    public int maxHealth = 5;

    /// <summary>Current health — read from UI or other systems.</summary>
    public int CurrentHealth { get; private set; }

    [Header("Invincibility Frames")]
    [Tooltip("Seconds of invincibility after taking a hit")]
    public float invincibilityDuration = 1f;

    /// <summary>
    /// Set this to true while the player is guarding.
    /// PlayerController sets this every frame via SetGuarding().
    /// </summary>
    public bool IsGuarding { get; private set; }

    /// <summary>Fired whenever health changes. int param = new current health.</summary>
    public event Action<int> OnHealthChanged;

    /// <summary>Fired once when the player dies.</summary>
    public event Action OnPlayerDied;

    private float invincibilityTimer;
    private bool  isDead;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    /// <summary>Called by PlayerController every frame to sync guard state.</summary>
    public void SetGuarding(bool guarding)
    {
        IsGuarding = guarding;
    }

    /// <summary>Called by Demon1Controller on contact.</summary>
    public void TakeDamage(int amount)
    {
        if (isDead || invincibilityTimer > 0f) return;

        // Guard blocks all damage
        if (IsGuarding)
        {
            Debug.Log("[PlayerHealth] Damage blocked by guard!");
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        invincibilityTimer = invincibilityDuration;

        Debug.Log($"[PlayerHealth] Took {amount} damage. HP: {CurrentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[PlayerHealth] Player has died.");
        OnPlayerDied?.Invoke();

        // Uncomment to reload the scene on death:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
