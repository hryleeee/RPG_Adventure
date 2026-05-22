using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyJumper : MonoBehaviour
{
    [Header("Base Locomotion")]
    public float walkSpeed = 2f;
    public float playerDetectionRange = 8f;

    [Header("Leap Constraints")]
    public float jumpCooldown = 4f;
    public float jumpDuration = 0.8f;
    [Tooltip("Caps how far (in units) the demon can leap in a single jump.")]
    public float maxJumpDistance = 5f;

    [Header("Wall Radar System")]
    [Tooltip("Select your Wall/Tilemap layer so the demon can sense solid objects.")]
    public LayerMask obstacleLayer;

    [Header("Landing Stomp Specifications")]
    public int stompDamage = 3;
    public float stompRadius = 1.2f;
    public float knockbackForce = 16f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private Collider2D enemyCollider;
    private Collider2D playerCollider;

    private bool isJumping = false;
    private float nextJumpTime;
    private Vector3 initialScale;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        initialScale = transform.localScale;
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCollider = playerObj.GetComponent<Collider2D>();
        }
    }

    private void FixedUpdate()
    {
        if (player == null || isJumping) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 targetDir = (player.position - transform.position).normalized;

        if (distanceToPlayer <= playerDetectionRange)
        {
            if (Time.time >= nextJumpTime)
            {
                StartCoroutine(ExecuteSafeLeapSequence());
            }
            else
            {
                rb.linearVelocity = targetDir * walkSpeed;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        FlipSpriteFacingDirection();
    }

    private IEnumerator ExecuteSafeLeapSequence()
    {
        isJumping = true;
        nextJumpTime = Time.time + jumpCooldown;

        // Ignore body bumping with the player while mid-air
        if (enemyCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
        }

        Vector2 startPos = transform.position;
        Vector2 targetPos = player.position;
        Vector2 jumpDirection = (targetPos - startPos).normalized;
        float intendedDistance = Vector2.Distance(startPos, targetPos);

        // Clamp distance so he doesn't rocket across giant map spaces
        if (intendedDistance > maxJumpDistance)
        {
            intendedDistance = maxJumpDistance;
            targetPos = startPos + (jumpDirection * maxJumpDistance);
        }

        // ── 🚨 FIXED SELF-COLLISION: Offset the radar start point outside his body! ──
        float bodySafetyOffset = 0.5f;
        Vector2 radarOrigin = startPos + (jumpDirection * bodySafetyOffset);
        float radarDistance = Mathf.Max(0f, intendedDistance - bodySafetyOffset);

        RaycastHit2D wallCheck = Physics2D.Raycast(radarOrigin, jumpDirection, radarDistance, obstacleLayer);
        if (wallCheck.collider != null)
        {
            // If an actual wall is hit, land safely right in front of it
            targetPos = wallCheck.point - (jumpDirection * 0.4f);
        }
        // ─────────────────────────────────────────────────────────────────────────────

        // Calculate steady velocity to targetPos over the jump time duration
        Vector2 finalVelocity = (targetPos - startPos) / jumpDuration;
        rb.linearVelocity = finalVelocity;

        if (anim != null)
        {
            anim.SetBool("IsAirborne", true);
        }

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float heightFactor = Mathf.Sin((elapsed / jumpDuration) * Mathf.PI);
            transform.localScale = initialScale * (1f + heightFactor * 0.5f);
            yield return null;
        }

        transform.localScale = initialScale;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("IsAirborne", false);
        }

        CheckLandingStompImpact();

        if (enemyCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(enemyCollider, playerCollider, false);
        }

        isJumping = false;
    }

    private void CheckLandingStompImpact()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, stompRadius);

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth health = hit.GetComponent<PlayerHealth>();
                if (health != null) health.TakeDamage(stompDamage);

                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }
                break;
            }
        }
    }

    private void FlipSpriteFacingDirection()
    {
        if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
        {
            float targetedScale = rb.linearVelocity.x > 0f ? initialScale.x : -initialScale.x;
            transform.localScale = new Vector3(targetedScale, transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stompRadius);
    }
}