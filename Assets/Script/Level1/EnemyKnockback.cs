using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyKnockback : MonoBehaviour
{
    private Rigidbody2D enemyRb;
    private bool isCurrentlyKnockedBack = false;

    // Optional: Reference to the enemy's AI movement script to turn off pathfinding during hit-stun states
    // private Demon1Controller demonAI; 

    private void Awake()
    {
        enemyRb = GetComponent<Rigidbody2D>();
        // demonAI = GetComponent<Demon1Controller>();
    }

    /// <summary>
    /// Public entry point called by the weapon's hitbox code to initiate physics pushes.
    /// </summary>
    public void TriggerKnockback(Vector3 attackerPosition, float thrustForce, float stunDuration)
    {
        if (isCurrentlyKnockedBack || enemyRb == null) return;

        // Start the physics manipulation loop sequence
        StartCoroutine(ExecuteKnockbackCoroutine(attackerPosition, thrustForce, stunDuration));
    }

    private IEnumerator ExecuteKnockbackCoroutine(Vector3 attackerPosition, float thrustForce, float stunDuration)
    {
        isCurrentlyKnockedBack = true;

        // 1. Turn off AI pathfinding tracking routines temporarily so the code doesn't try to move forward while flying backward
        // if (demonAI != null) demonAI.enabled = false;

        // 2. VECTOR MATH: Calculate the pure normalized trajectory directional array moving AWAY from the player asset
        Vector2 forceDirection = (transform.position - attackerPosition).normalized;

        // 3. PHYSICS INJECTION: Set the Rigidbody2D velocity vector to push the enemy backward instantly
        enemyRb.linearVelocity = forceDirection * thrustForce;

        // Wait out the frame delta processing arrays during active hit-stun windows
        yield return new WaitForSeconds(stunDuration);

        // 4. CLEAN RESET: Zero out structural velocities smoothly to stop sliding infinitely
        if (enemyRb != null)
        {
            enemyRb.linearVelocity = Vector2.zero;
        }

        // 5. Restore full structural runtime controls back over to your custom pathfinding steering modules
        // if (demonAI != null) demonAI.enabled = true;

        isCurrentlyKnockedBack = false;
    }
}