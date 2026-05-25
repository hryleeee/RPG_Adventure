using UnityEngine;
using System.Collections.Generic;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Knockback Configurations")]
    [SerializeField] private float knockbackThrust = 8f;
    [SerializeField] private float knockbackDuration = 0.15f;

    // Use a HashSet buffer to prevent multi-hit registering inside a single swing frame loop
    private HashSet<Collider2D> alreadyHitTargets = new HashSet<Collider2D>();

    // Reference to the main player object to accurately pull transform calculations
    private Transform playerTransform;

    private void Awake()
    {
        // Finds the root player transform parent position automatically
        playerTransform = transform.root;
    }

    private void OnEnable()
    {
        // Empty out our double-hit prevention buffer cache whenever an attack animation initiates
        alreadyHitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Safety verification: Ensure we haven't already calculated damage/knockback for this specific target on this frame
        if (alreadyHitTargets.Contains(collision)) return;

        // 2. Filter targets by checking for your Enemy tags (e.g., "Enemy" or "Demon")
        if (collision.CompareTag("Enemy"))
        {
            alreadyHitTargets.Add(collision);

            // 3. SURGICAL LINK: Attempt to find the physics knockback engine attached to the incoming enemy object
            EnemyKnockback enemyKnockback = collision.GetComponent<EnemyKnockback>();

            if (enemyKnockback != null && playerTransform != null)
            {
                // Trigger the enemy's dedicated physics knockback function!
                enemyKnockback.TriggerKnockback(playerTransform.position, knockbackThrust, knockbackDuration);

                Debug.Log($"Successfully applied hit physics parameters onto target: {collision.name}");
            }

            // --- Your existing logic to decrease enemy health goes right here ---
            // Example: collision.GetComponent<EnemyHealth>()?.TakeDamage(damageAmount);
        }
    }
}