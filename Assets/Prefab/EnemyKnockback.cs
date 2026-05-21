using UnityEngine;
using System.Collections;

public class EnemyKnockback : MonoBehaviour
{
    private Rigidbody2D rb;
    private MonoBehaviour enemyAI; // Dynamically tracks either Demon1 or Demon2 script

    [Header("Default Physics Profiles")]
    [Tooltip("The mass damping multiplier. Higher values mean the enemy slows down faster after being pushed.")]
    public float linearDragOverride = 5f;
    private float originalDrag;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 🧠 SMART COUPLING: Dynamically searches for whatever AI controller your teammate attached!
        enemyAI = (MonoBehaviour)GetComponent("Demon1Controller") ?? (MonoBehaviour)GetComponent("Demon2Controller");

        if (rb != null)
        {
            originalDrag = rb.linearDamping;
        }
    }

    /// <summary>
    /// Computes and applies an immediate physical backward force vector away from the attack source.
    /// </summary>
    public void ApplyKnockback(Vector2 damageSourcePosition, float force, float duration)
    {
        if (rb == null) return;

        // Stop any active knockback routines if they get hit again mid-air (juggling combo support!)
        StopAllCoroutines();
        StartCoroutine(KnockbackRoutine(damageSourcePosition, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 sourcePosition, float force, float duration)
    {
        // 1. Cut the power to the AI script so it stops trying to walk toward the player during the push
        if (enemyAI != null) enemyAI.enabled = false;

        // 2. Set high drag so the push feels snappy and doesn't make them slide like they are on ice
        rb.linearDamping = linearDragOverride;

        // 3. Calculate directional vector (Target Position minus Source Position)
        Vector2 pushDirection = ((Vector2)transform.position - sourcePosition).normalized;

        // 4. Fire the instantaneous velocity burst
        rb.linearVelocity = pushDirection * force;

        // 5. Let the physics handle the slide during the stun duration window
        yield return new WaitForSeconds(duration);

        // 6. Calm the physics down cleanly and return control to the AI brain script
        rb.linearDamping = originalDrag;
        rb.linearVelocity = Vector2.zero;

        if (enemyAI != null) enemyAI.enabled = true;
    }
}