using UnityEngine;
using System; // Required for Action/Events

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    [Header("Invincibility Configuration")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    private float invincibilityTimer;

    [Header("UI Manager Link")]
    [Tooltip("Drag your scene's PauseManager object here to trigger the Game Over screen")]
    public PauseMenu pauseMenuManager; // Fixed type naming convention to match your script explicitly

    // Team Hooks required by LifeUI.cs and PlayerController.cs
    public event Action<int> OnHealthChanged;
    private bool isGuarding;

    // Public property to expose current health safely to LifeUI.cs
    public int CurrentHealth => currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        // Broadcast initial health state to UI on start
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Call this function when an enemy hit registers on the player.
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        // If player is guarding or invincible, block the damage
        if (isGuarding || invincibilityTimer > 0)
        {
            Debug.Log("Hit blocked or player is invincible!");
            return;
        }

        currentHealth -= damageAmount;
        invincibilityTimer = invincibilityDuration; // Trigger i-frames

        Debug.Log($"Player hit! Current Health: {currentHealth}");

        // Safely tell LifeUI.cs to update your heart visuals
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            ExecuteGameOver();
        }
    }

    /// <summary>
    /// Called by PlayerController.cs to toggle shield blocking states
    /// </summary>
    public void SetGuarding(bool guardingState)
    {
        isGuarding = guardingState;
    }

    private void ExecuteGameOver()
    {
        Debug.Log("💀 Player health reached zero! Triggering Game Over panel.");

        if (pauseMenuManager != null)
        {
            pauseMenuManager.TriggerGameOver();
        }
        else
        {
            Debug.LogError("The PauseMenuManager reference is missing on the Player GameObject!");
        }
    }
}