using System.Collections;
using UnityEngine;

public class Demon1Controller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float minWanderTime = 1.5f;
    public float maxWanderTime = 3.5f;

    [Header("Combat")]
    public int contactDamage = 1;
    public float damageCooldown = 1f;

    [Header("Death")]
    public float deathAnimDuration = 0.8f;

    private static readonly int AnimIsWalking = Animator.StringToHash("isWalking");
    private static readonly int AnimIsDead = Animator.StringToHash("isDead");

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 wanderDirection;
    private float wanderTimer;
    private bool isDead;
    private float damageTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
        rb.gravityScale = 0f;

        PickNewDirection();
    }

    private void Update()
    {
        if (isDead) return;

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f) PickNewDirection();

        if (wanderDirection.x > 0.01f) sr.flipX = false;
        else if (wanderDirection.x < -0.01f) sr.flipX = true;

        if (damageTimer > 0f) damageTimer -= Time.deltaTime;
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

    // ── CHANNEL A: Fires if colliders are SOLID ───────────────────────────
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;
        if (col.gameObject.CompareTag("Player")) { TryDealDamage(col.gameObject); return; }
        if (col.gameObject.CompareTag("Enemy")) return;

        if (col.contacts.Length > 0)
        {
            Vector2 normal = col.contacts[0].normal;
            wanderDirection = Vector2.Reflect(wanderDirection, normal).normalized;
            wanderTimer = Random.Range(minWanderTime, maxWanderTime);
        }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (isDead) return;
        if (col.gameObject.CompareTag("Player")) TryDealDamage(col.gameObject);
    }

    // ── CHANNEL B: Fires if colliders are TRIGGERS ────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag("Player")) TryDealDamage(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag("Player")) TryDealDamage(other.gameObject);
    }

    // ── Unified Damage Core Execution Loop ────────────────────────────────
    private void TryDealDamage(GameObject playerObj)
    {
        if (damageTimer > 0f) return;

        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
            damageTimer = damageCooldown;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        anim.SetBool(AnimIsWalking, false);
        anim.SetBool(AnimIsDead, true);

        if (LevelManager.Instance != null) LevelManager.Instance.RegisterEnemyDeath();
        Destroy(gameObject, deathAnimDuration);
    }

    private void PickNewDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        wanderTimer = Random.Range(minWanderTime, maxWanderTime);
        if (anim != null) anim.SetBool(AnimIsWalking, true);
    }
}