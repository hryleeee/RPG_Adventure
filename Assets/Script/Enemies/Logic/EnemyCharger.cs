using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCharger : MonoBehaviour
{
    [Header("Melee Approach Settings")]
    public float normalSpeed = 2.5f;
    public float playerDetectionRange = 8f;

    [Header("Cavalry Charge Settings")]
    public float chargeSpeed = 9f;
    public float chargeCooldown = 5f;
    public float chargeDuration = 0.6f;
    public int attackDamage = 2;

    [Header("Pass-Through Hit Detection")]
    [Tooltip("Size of the pass-through strike zone around the enemy's body.")]
    public float strikeRadius = 0.6f;

    [Header("Impact Shock & Knockback")]
    public float knockbackForce = 18f;

    [Header("Tactical Distancing (Retreat)")]
    public float retreatSpeed = 4f;
    public float retreatDuration = 1.5f;

    [Header("Wall Detection Radar")]
    public LayerMask obstacleLayer;
    public float lookAheadDistance = 1.2f;

    private Rigidbody2D rb;
    private Transform player;
    private Collider2D enemyCollider;
    private Collider2D playerCollider;

    private bool isCharging = false;
    private bool isRetreating = false;
    private bool hasHitPlayerThisCharge = false;
    private float nextChargeTime;
    private float absoluteScaleX;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        absoluteScaleX = Mathf.Abs(transform.localScale.x);

        // Automatically lock onto the player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCollider = playerObj.GetComponent<Collider2D>();
        }
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

        if (isRetreating)
        {
            // STATE 3: Move directly away from the player
            rb.linearVelocity = -targetDir * retreatSpeed;
        }
        else if (isCharging)
        {
            // STATE 2: Charging through the player
            if (!hasHitPlayerThisCharge)
            {
                CheckAutomaticStrike();
            }
        }
        else if (distanceToPlayer <= playerDetectionRange)
        {
            // STATE 1: Normal hunting movement
            if (Time.time >= nextChargeTime && !IsWallBlocking(targetDir))
            {
                StartCoroutine(ExecuteChargeSequence(targetDir));
            }
            else
            {
                rb.linearVelocity = targetDir * normalSpeed;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        FlipSpriteFacingDirection();
    }

    private IEnumerator ExecuteChargeSequence(Vector2 direction)
    {
        isCharging = true;
        hasHitPlayerThisCharge = false;
        nextChargeTime = Time.time + chargeCooldown;

        // Temporarily ignore solid physics collisions so it can slice through the player smoothly
        if (enemyCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
        }

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.25f);

        rb.linearVelocity = direction * chargeSpeed;
        yield return new WaitForSeconds(chargeDuration);

        if (!isRetreating)
        {
            ResetCollisionSafety();
            isCharging = false;
        }
    }

    /// <summary>
    /// Scans automatically for any colliders with the 'Player' tag nearby, 
    /// completely removing the need for a Player Layer dropdown box.
    /// </summary>
    private void CheckAutomaticStrike()
    {
        // Scan everything around the monster's body matching the strike width radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, strikeRadius);

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                hasHitPlayerThisCharge = true;

                // 1. Apply physical blowback force
                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackVector = (hit.transform.position - transform.position).normalized;
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.AddForce(knockbackVector * knockbackForce, ForceMode2D.Impulse);
                }

                // 2. Deal health point damage
                PlayerHealth health = hit.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(attackDamage);
                }

                Debug.Log($"🎯 {gameObject.name} hit the player automatically! Actively retreating now.");

                // 3. Stop the charge and go directly into distancing mode
                StopAllCoroutines();
                StartCoroutine(TriggerDistancingRetreat());
                break;
            }
        }
    }

    private IEnumerator TriggerDistancingRetreat()
    {
        isCharging = false;
        isRetreating = true;
        yield return new WaitForSeconds(retreatDuration);
        ResetCollisionSafety();
        isRetreating = false;
    }

    private void ResetCollisionSafety()
    {
        if (enemyCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(enemyCollider, playerCollider, false);
        }
    }

    private bool IsWallBlocking(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, lookAheadDistance, obstacleLayer);
        return hit.collider != null;
    }

    private void FlipSpriteFacingDirection()
    {
        if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
        {
            float targetedScale = rb.linearVelocity.x > 0f ? absoluteScaleX : -absoluteScaleX;
            transform.localScale = new Vector3(targetedScale, transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, strikeRadius);
    }
}