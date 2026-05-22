using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFireHaunt : MonoBehaviour
{
    [Header("Base Locomotion")]
    public float floatSpeed = 2.5f;
    public float playerDetectionRange = 10f;

    [Header("Ranged Distance Keeper (Kiting)")]
    public float shootingRange = 7f;   // Maximum range at which it starts firing
    public float retreatRange = 3.5f;   // Minimum range: if you get closer than this, it backs away!

    [Header("Casting Specifications")]
    public GameObject fireballPrefab;
    public Transform castPoint;
    public float fireRate = 2f;
    public float projectileSpeed = 6f;

    private Rigidbody2D rb;
    private Transform player;
    private float nextFireTime;
    private float absoluteScaleX;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        absoluteScaleX = Mathf.Abs(transform.localScale.x);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 targetDir = (player.position - transform.position).normalized;

        if (distanceToPlayer <= playerDetectionRange)
        {
            // ── 🏃‍♂️ UNINTERRUPTED LOCOMOTION STATE MACHINE ────────────────────
            // The monster now maintains its movement velocity on every single frame!
            if (distanceToPlayer < retreatRange)
            {
                // Move backward if the player gets right in its face
                rb.linearVelocity = -targetDir * floatSpeed;
            }
            else
            {
                // Keep walking/floating forward aggressively toward the player!
                rb.linearVelocity = targetDir * floatSpeed;
            }
            // ──────────────────────────────────────────────────────────────────

            // ── 🔥 INDEPENDENT FIREBALL SYSTEM ──────────────────────────────
            // This runs in parallel, allowing it to fire projectiles while moving
            if (distanceToPlayer <= shootingRange && Time.time >= nextFireTime)
            {
                CastFireballBolt(targetDir);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        FlipSpriteFacingDirection();
    }

    private void CastFireballBolt(Vector2 direction)
    {
        nextFireTime = Time.time + fireRate;

        if (fireballPrefab != null && castPoint != null)
        {
            GameObject bolt = Instantiate(fireballPrefab, castPoint.position, Quaternion.identity);

            Rigidbody2D boltRb = bolt.GetComponent<Rigidbody2D>();
            if (boltRb != null)
            {
                boltRb.linearVelocity = direction * projectileSpeed;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bolt.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void FlipSpriteFacingDirection()
    {
        if (player != null)
        {
            float targetedScale = player.position.x > transform.position.x ? absoluteScaleX : -absoluteScaleX;
            transform.localScale = new Vector3(targetedScale, transform.localScale.y, transform.localScale.z);
        }
    }
}