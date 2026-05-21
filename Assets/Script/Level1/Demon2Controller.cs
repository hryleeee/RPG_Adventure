using UnityEngine;

/// <summary>
/// Demon2 AI — uses the alien-enemy-flying sprites.
/// Same wander/wall-bounce/damage behaviour as Demon1 but tuned separately.
///
/// ── Setup on the Demon2 Prefab ──────────────────────────────────────────
///  • SpriteRenderer    : Source Image = alien-enemy-flying1
///  • Rigidbody2D       : Gravity Scale = 0, Freeze Rotation Z, Collision Detection = Continuous
///  • CircleCollider2D  : isTrigger = FALSE
///  • Animator          : Controller = Demon2Animator
///  • Demon2Controller  : this script
///  • Tag               : "Enemy"
/// ────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Demon2Controller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed    = 2.5f;
    public float minWanderTime = 1.2f;
    public float maxWanderTime = 3f;

    [Header("Combat")]
    public int   contactDamage  = 1;
    public float damageCooldown = 1f;

    [Header("Death")]
    public float deathAnimDuration = 0.8f;

    // Animator hashes — must match parameters in Demon2Animator
    private static readonly int AnimIsWalking = Animator.StringToHash("isWalking");
    private static readonly int AnimIsDead    = Animator.StringToHash("isDead");

    private Rigidbody2D    rb;
    private Animator       anim;
    private SpriteRenderer sr;

    private Vector2 wanderDir;
    private float   wanderTimer;
    private bool    isDead;
    private float   damageTimer;

    private void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
        rb.gravityScale   = 0f;

        PickNewDirection();
    }

    private void Update()
    {
        if (isDead) return;

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f) PickNewDirection();

        // Flip to face movement direction
        if (wanderDir.x > 0.01f)       sr.flipX = false;
        else if (wanderDir.x < -0.01f) sr.flipX = true;

        if (damageTimer > 0f) damageTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (isDead) { rb.linearVelocity = Vector2.zero; return; }
        rb.linearVelocity = wanderDir * moveSpeed;
    }

    // Bounce off walls
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;
        if (col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Player")) return;

        Vector2 normal = col.contacts[0].normal;
        wanderDir = Vector2.Reflect(wanderDir, normal).normalized;

        float angle = Random.Range(-45f, 45f) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
        wanderDir = new Vector2(
            wanderDir.x * cos - wanderDir.y * sin,
            wanderDir.x * sin + wanderDir.y * cos
        ).normalized;

        wanderTimer = Random.Range(minWanderTime, maxWanderTime);
    }

    // Damage player on contact
    private void OnCollisionStay2D(Collision2D col)
    {
        if (isDead) return;
        if (!col.gameObject.CompareTag("Player")) return;
        if (damageTimer > 0f) return;

        PlayerHealth ph = col.gameObject.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(contactDamage);

        damageTimer = damageCooldown;
    }

    // Called by PlayerController when attack hits this demon
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        anim.SetBool(AnimIsWalking, false);
        anim.SetBool(AnimIsDead,    true);

        Destroy(gameObject, deathAnimDuration);
    }

    private void PickNewDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDir   = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        wanderTimer = Random.Range(minWanderTime, maxWanderTime);

        if (anim != null) anim.SetBool(AnimIsWalking, true);
    }
}
