using UnityEngine;

/// <summary>
/// Simple player health component.
/// Attach this to the Player GameObject.
///
/// The Demon1Controller calls TakeDamage() when it touches the player.
/// Extend this class to hook into UI health bars, game-over screens, etc.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Maximum / starting health")]
    public int maxHealth = 5;

    // Current health (read-only from outside)
    public int CurrentHealth { get; private set; }

    [Header("Invincibility Frames")]
    [Tooltip("Seconds of invincibility after taking a hit (prevents rapid damage)")]
    public float invincibilityDuration = 0.5f;

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

    /// <summary>
    /// Called by Demon1Controller when the demon touches the player.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead || invincibilityTimer > 0f) return;

        CurrentHealth -= amount;
        invincibilityTimer = invincibilityDuration;

        Debug.Log($"[PlayerHealth] Player took {amount} damage. HP: {CurrentHealth}/{maxHealth}");

        if (CurrentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[PlayerHealth] Player has died.");
        // TODO: trigger game-over screen / respawn logic here
        // e.g. SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
