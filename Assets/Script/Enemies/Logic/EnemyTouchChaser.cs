using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyTouchChaser : MonoBehaviour
{
    [Header("Base Locomotion")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float playerDetectionRange = 10f;

    [Header("Combat & Damage Profiles")]
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float damageCooldown = 1.5f;

    [Header("Tactical Hit-and-Run Adjustments")]
    [SerializeField] private float retreatSpeed = 3.5f;
    [SerializeField] private float retreatDuration = 0.8f;

    [Header("Context Steering Pathfinding")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float lookAheadDistance = 1.5f;
    [SerializeField] private float scanRadius = 0.35f;

    private Rigidbody2D rb;
    private Transform player;
    private float nextDamageTime;
    private bool isRetreating;

    // Default tracking scale cache to prevent distorting stretch values
    private float absoluteScaleX;

    private readonly Vector2[] steeringDirections = new Vector2[]
    {
        Vector2.up,
        new Vector2(1, 1).normalized,
        Vector2.right,
        new Vector2(1, -1).normalized,
        Vector2.down,
        new Vector2(-1, -1).normalized,
        Vector2.left,
        new Vector2(-1, 1).normalized
    };

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Remember the base size scale you configured in the Inspector layout panel
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
        Vector2 targetDirection = (player.position - transform.position).normalized;

        if (isRetreating)
        {
            rb.linearVelocity = -targetDirection * retreatSpeed;
        }
        else if (distanceToPlayer <= playerDetectionRange)
        {
            Vector2 chosenPath = EvaluateContextSteering(targetDirection);
            rb.linearVelocity = chosenPath * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        // ── 🔄 AUTOMATIC LEFT / RIGHT LOOK ROTATOR LAYER ──────────────────
        FlipSpriteFacingDirection();
        // ──────────────────────────────────────────────────────────────────
    }

    /// <summary>
    /// Analyzes physics engine lateral velocity vectors to flip scales automatically.
    /// </summary>
    private void FlipSpriteFacingDirection()
    {
        // Only trigger changes if the monster is actively moving left or right horizontally
        if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
        {
            // NOTE: This assumes your raw asset graphic is drawn facing RIGHT by default.
            // If it is drawn facing LEFT by default, swap the positive and negative absoluteScaleX results below!
            float targetedDirectionScale = rb.linearVelocity.x > 0f ? absoluteScaleX : -absoluteScaleX;

            transform.localScale = new Vector3(targetedDirectionScale, transform.localScale.y, transform.localScale.z);
        }
    }

    private Vector2 EvaluateContextSteering(Vector2 directHeading)
    {
        Vector2 bestPath = directHeading;
        float highestScore = -float.MaxValue;

        for (int i = 0; i < steeringDirections.Length; i++)
        {
            Vector2 candidateDir = steeringDirections[i];
            float interestScore = Vector2.Dot(candidateDir, directHeading);
            RaycastHit2D wallCheck = Physics2D.CircleCast(transform.position, scanRadius, candidateDir, lookAheadDistance, obstacleLayer);

            float dangerPenalty = 0f;

            if (wallCheck.collider != null)
            {
                dangerPenalty = 1f - (wallCheck.distance / lookAheadDistance);
                if (wallCheck.distance < 0.3f)
                {
                    dangerPenalty += 50f;
                }
            }

            float finalScore = interestScore - (dangerPenalty * 6f);

            if (finalScore > highestScore)
            {
                highestScore = finalScore;
                bestPath = candidateDir;
            }
        }

        return bestPath;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ProcessPlayerDamage(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        ProcessPlayerDamage(collider.gameObject);
    }

    private void ProcessPlayerDamage(GameObject hitObject)
    {
        if (hitObject.CompareTag("Player") && Time.time >= nextDamageTime && !isRetreating)
        {
            PlayerHealth playerHealth = hitObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                nextDamageTime = Time.time + damageCooldown;
                playerHealth.TakeDamage(contactDamage);
                StartCoroutine(TriggerRetreatSequence());
            }
        }
    }

    private IEnumerator TriggerRetreatSequence()
    {
        isRetreating = true;
        yield return new WaitForSeconds(retreatDuration);
        isRetreating = false;
    }
}