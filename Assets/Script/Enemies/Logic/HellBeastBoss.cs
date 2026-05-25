using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class HellBeastBoss : MonoBehaviour
{
    [Header("Boss Core Sliders")]
    [SerializeField] private float bossSpeed = 5f;

    [Header("Ranged Mode Assets")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform blastPoint;
    [SerializeField] private float normalCooldown = 1.5f;

    [Header("Charged Mode Assets")]
    [SerializeField] private GameObject burnZonePrefab;
    [SerializeField] private float chargeCooldown = 3f;

    private Rigidbody2D rb;
    private Transform player;
    private float globalAttackTimer;
    private bool castingFrameActive;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (player == null || castingFrameActive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 trackDir = (player.position - transform.position).normalized;
        rb.linearVelocity = trackDir * bossSpeed;

        if (Time.time >= globalAttackTimer)
        {
            // Weight Distribution check: Charged attack is used frequently (75% probability bracket)
            if (Random.value <= 0.75f)
                StartCoroutine(TriggerChargedBurnZone());
            else
                StartCoroutine(FireNormalProjectile());
        }
    }

    private IEnumerator FireNormalProjectile()
    {
        castingFrameActive = true;
        globalAttackTimer = Time.time + normalCooldown;

        if (fireballPrefab != null && blastPoint != null)
        {
            GameObject ball = Instantiate(fireballPrefab, blastPoint.position, Quaternion.identity);
            Rigidbody2D bRb = ball.GetComponent<Rigidbody2D>();
            if (bRb != null) bRb.linearVelocity = (player.position - blastPoint.position).normalized * 8f;
        }

        yield return new WaitForSeconds(0.2f);
        castingFrameActive = false;
    }

    private IEnumerator TriggerChargedBurnZone()
    {
        castingFrameActive = true;
        globalAttackTimer = Time.time + chargeCooldown;
        rb.linearVelocity = Vector2.zero;

        Debug.Log("🔥 Boss charging heavy Burn floor layout zone...");
        yield return new WaitForSeconds(0.8f); // Charging wind-up

        if (burnZonePrefab != null)
        {
            GameObject zone = Instantiate(burnZonePrefab, player.position, Quaternion.identity);
            Destroy(zone, 2.5f); // Automatically scrub burn puddle after execution
        }

        castingFrameActive = false;
    }
}