using System.Collections;
using UnityEngine;

/// <summary>
/// Demon1 AI — wanders freely around the dungeon, respects walls,
/// damages the player on contact, and dies when hit by the player's attack.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Demon1Controller : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────
    [Header("Movement")]
    [Tooltip("How fast the demon moves")]
    public float moveSpeed = 2f;

    [Tooltip("Minimum seconds to walk in one direction before picking a new one")]
    public float minWanderTime = 1.5f;

    [Tooltip("Maximum seconds to walk in one direction before picking a new one")]
    public float maxWanderTime = 3.5f;

    [Header("Combat")]
    [Tooltip("Damage dealt to the player per hit")]
    public int contactDamage = 1;

    [Tooltip("Seconds between each damage tick while touching the player")]
    public float damageCooldown = 1f;

    [Header("Death")]
    [Tooltip("Seconds the death animation plays before the object is destroyed")]
    public float deathAnimDuration = 0.8f;

    // ── Animator parameter hashes ─────────────────────────────────────────
    private static readonly int AnimIsWalking = Animator.StringToHash("isWalking");
    private static readonly int AnimIsDead = Animator.StringToHash("isDead");

    // ── Private state ─────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 wanderDirection;
    private float wanderTimer;

    private bool isDead;
    private float damageTimer;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Prevent demon from being rotated by physics
        rb.freezeRotation = true;
        rb.gravityScale = 0f;

        PickNewDirection();
    }

    private void Update()
    {
        if (isDead) return;

        // Count down wander timer
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
            PickNewDirection();

        // Flip sprite to face movement direction
        if (wanderDirection.x > 0.01f)
            sr.flipX = false;
        else if (wanderDirection.x < -0.01f)
            sr.flipX = true;

        // Keep damage cooldown ticking
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = wanderDirection * moveSpeed;
    }

    // ── Wall / obstacle collision ─────────────────────────────────────────
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;

        // Ignore collisions with other demons and the player body
        if (col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Player"))
            return;

        // Bounce: reflect direction off the collision normal and add randomness
        Vector2 normal = col.contacts[0].normal;
        wanderDirection = Vector2.Reflect(wanderDirection, normal).normalized;

        float angle = Random.Range(-45f, 45f) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
        wanderDirection = new Vector2(
            wanderDirection.x * cos - wanderDirection.y * sin,
            wanderDirection.x * sin + wanderDirection.y * cos
        ).normalized;

        wanderTimer = Random.Range(minWanderTime, maxWanderTime);
    }

    // ── Player contact damage ─────────────────────────────────────────────
    private void OnCollisionStay2D(Collision2D col)
    {
        if (isDead) return;
        if (!col.gameObject.CompareTag("Player")) return;
        if (damageTimer > 0f) return;

        PlayerHealth playerHealth = col.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(contactDamage);

        damageTimer = damageCooldown;
    }

    // ── Called by PlayerAttackHitbox when the player's attack lands ───────
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        // Disable the physics collider so the demon stops blocking
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Play death animation
        anim.SetBool(AnimIsWalking, false);
        anim.SetBool(AnimIsDead, true);

        // SAFELY reporting enemy defeat to the dynamic LevelManager counter
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterEnemyDeath();
        }

        // Destroy after the death animation finishes
        Destroy(gameObject, deathAnimDuration);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private void PickNewDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        wanderTimer = Random.Range(minWanderTime, maxWanderTime);

        if (anim != null)
            anim.SetBool(AnimIsWalking, true);
    }
}