using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach this to a CHILD GameObject of the Player called "AttackHitbox".
/// It needs a Collider2D set to isTrigger = TRUE.
/// </summary>
public class PlayerAttackHitbox : MonoBehaviour
{
    private static readonly int AnimIsAttacking = Animator.StringToHash("isAttacking");

    private Animator playerAnimator;
    private Collider2D hitboxCollider;

    [Header("Combat Settings")]
    [Tooltip("How much health/lives to remove from the enemy on a successful hit.")]
    public int attackDamage = 1;

    [Header("Knockback Settings")]
    [Tooltip("The speed force applied to shove the enemy backwards.")]
    public float knockbackForce = 12f;
    [Tooltip("How long the enemy stays physically paralyzed before recovering AI control.")]
    public float knockbackDuration = 0.2f;

    // Tracks all enemy gameobjects hit inside the current swing frame window
    private readonly HashSet<GameObject> hitThisSwing = new HashSet<GameObject>();

    // Were we attacking last frame? Used to detect the START of a new swing
    private bool wasAttackingLastFrame;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        playerAnimator = GetComponentInParent<Animator>();

        if (playerAnimator == null)
            Debug.LogWarning("[PlayerAttackHitbox] No Animator found on parent Player.");
        if (hitboxCollider == null)
            Debug.LogWarning("[PlayerAttackHitbox] No Collider2D found on AttackHitbox.");

        // Start with hitbox off
        if (hitboxCollider != null)
            hitboxCollider.enabled = false;
    }

    private void Update()
    {
        if (playerAnimator == null || hitboxCollider == null) return;

        bool isAttackingNow = playerAnimator.GetBool(AnimIsAttacking);

        // New swing just started — clear the already-hit list
        if (isAttackingNow && !wasAttackingLastFrame)
        {
            hitThisSwing.Clear();
            Debug.Log("[PlayerAttackHitbox] Attack swing started.");
        }

        // Enable collider only while attacking
        hitboxCollider.enabled = isAttackingNow;

        wasAttackingLastFrame = isAttackingNow;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryHit(other);
    }

    private void TryHit(Collider2D other)
    {
        if (playerAnimator == null || !playerAnimator.GetBool(AnimIsAttacking))
            return;

        // 🛡️ Safety check: Ensure the object hit is actually designated as an Enemy target
        if (!other.CompareTag("Enemy")) return;

        GameObject enemyTarget = other.gameObject;

        // Only register a hit once per unique target per single swing frame window
        if (hitThisSwing.Contains(enemyTarget)) return;
        hitThisSwing.Add(enemyTarget);

        Debug.Log($"[PlayerAttackHitbox] Hit connected on: {enemyTarget.name}");

        // 💥 1. APPLY PHYSICAL KNOCKBACK
        EnemyKnockback knockbackPhysics = enemyTarget.GetComponent<EnemyKnockback>();
        if (knockbackPhysics != null)
        {
            Vector2 attackSourcePosition = transform.parent != null ? (Vector2)transform.parent.position : (Vector2)transform.position;
            knockbackPhysics.ApplyKnockback(attackSourcePosition, knockbackForce, knockbackDuration);
        }

        // 💀 2. UNIVERSAL HEALTH REDUCTION STATE CONTROL
        EnemyHealth enemyHealth = enemyTarget.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            // ✅ SUCCESS: Deal standard damage to their unique inspector health pool!
            enemyHealth.TakeDamage(attackDamage);
        }
        else
        {
            // 🛡️ LEGACY FALLBACK: If you haven't dragged EnemyHealth onto a prefab yet, 
            // trigger their legacy code so the game continues to work seamlessly!
            Demon1Controller demon1 = enemyTarget.GetComponent<Demon1Controller>();
            if (demon1 != null)
            {
                demon1.Die();
            }

            Demon2Controller demon2 = enemyTarget.GetComponent<Demon2Controller>();
            if (demon2 != null)
            {
                demon2.Die();
            }
        }
    }
}