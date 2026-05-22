using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health & Lives Configuration")]
    [Tooltip("How many hit points or lives this specific monster variant has.")]
    public int maxHealth = 3;

    private int currentHealth;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"💥 {gameObject.name} took {damageAmount} damage! Remaining Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"💀 {gameObject.name} has been vanquished!");

        // 🎬 ANIMATION BRIDGE: If this is Demon1, pass control to its animated death loop
        Demon1Controller demon1 = GetComponent<Demon1Controller>();
        if (demon1 != null)
        {
            demon1.Die();
            return; // Exit here so Demon1Controller handles delayed destruction
        }

        // 🎬 ANIMATION BRIDGE: If this is Demon2, pass control to its animated death loop
        Demon2Controller demon2 = GetComponent<Demon2Controller>();
        if (demon2 != null)
        {
            demon2.Die();
            return; // Exit here so Demon2Controller handles delayed destruction
        }

        // 🌳 DEFAULT FALLBACK: For new enemies (Demon3, Treant, Vampire, Hell-Beast)
        Destroy(gameObject);
    }
}