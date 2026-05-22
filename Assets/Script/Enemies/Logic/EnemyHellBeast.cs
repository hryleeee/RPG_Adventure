using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHellBeast : MonoBehaviour
{
    [Header("Base Sensing Range")]
    public float detectionRange = 16f;

    [Header("1. Normal Attack (Fireball Setup)")]
    public GameObject fireballPrefab;
    public Transform mouthFirePoint;
    public float fireballSpeed = 7f;
    public float fireballCooldown = 2.5f;

    [Header("2. Charged Attack (Vertical Burn Setup)")]
    public GameObject verticalFirePrefab;
    public float chargedAttackCooldown = 7.5f;
    public float pillarSpawnTelegraphDelay = 0.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;

    private bool isExecutingSpell = false;
    private float nextFireballTime;
    private float nextChargedAttackTime;
    private float absoluteScaleX;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        absoluteScaleX = Mathf.Abs(transform.localScale.x);

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = Vector2.zero;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        nextChargedAttackTime = Time.time + 2.5f;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = Vector2.zero;

        if (player == null || isExecutingSpell) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 targetDirection = (player.position - transform.position).normalized;

        if (distanceToPlayer <= detectionRange)
        {
            if (Time.time >= nextChargedAttackTime)
            {
                StartCoroutine(ExecuteChargedBurnAttack());
            }
            else if (Time.time >= nextFireballTime)
            {
                StartCoroutine(ExecuteNormalFireballAttack(targetDirection));
            }
        }

        FlipBossFacingDirection();
    }

    // 🚨 AIRTIGHT SAFETY FILTER: 
    // If the boss dies and EnemyHealth disables this script, kill all running spell loops instantly!
    private void OnDisable()
    {
        StopAllCoroutines();
        isExecutingSpell = false;
    }

    private IEnumerator ExecuteNormalFireballAttack(Vector2 shootDir)
    {
        isExecutingSpell = true;
        if (anim != null) anim.SetTrigger("Fireball");

        yield return new WaitForSeconds(0.2f);

        if (fireballPrefab != null && mouthFirePoint != null)
        {
            GameObject bolt = Instantiate(fireballPrefab, mouthFirePoint.position, Quaternion.identity);
            Rigidbody2D boltRb = bolt.GetComponent<Rigidbody2D>();
            if (boltRb != null)
            {
                boltRb.linearVelocity = shootDir * fireballSpeed;
            }

            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            bolt.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        nextFireballTime = Time.time + fireballCooldown;
        yield return new WaitForSeconds(0.3f);
        isExecutingSpell = false;
    }

    private IEnumerator ExecuteChargedBurnAttack()
    {
        isExecutingSpell = true;
        Vector2 targetStompCoordinates = player.position;

        if (anim != null) anim.SetTrigger("Burn");

        yield return new WaitForSeconds(pillarSpawnTelegraphDelay);

        if (verticalFirePrefab != null)
        {
            Instantiate(verticalFirePrefab, targetStompCoordinates, Quaternion.identity);
            Debug.Log($"🌋 CHRONO-BURN CAST: Fire pillar erupted on player coordinates: {targetStompCoordinates}");
        }

        nextChargedAttackTime = Time.time + chargedAttackCooldown;
        yield return new WaitForSeconds(0.6f);
        isExecutingSpell = false;
    }

    private void FlipBossFacingDirection()
    {
        if (player != null && !isExecutingSpell)
        {
            float targetedDirectionX = player.position.x > transform.position.x ? absoluteScaleX : -absoluteScaleX;
            transform.localScale = new Vector3(targetedDirectionX, transform.localScale.y, transform.localScale.z);
        }
    }
}