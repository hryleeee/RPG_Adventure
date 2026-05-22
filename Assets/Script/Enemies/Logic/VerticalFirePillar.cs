using UnityEngine;
using System.Collections;

public class VerticalFirePillar : MonoBehaviour
{
    [Header("Damage Specifications")]
    public int fireDamagePerTick = 2;   // High hazard damage spec! Deducts 2 hearts
    public float activeLifetime = 1.8f;  // How long the column stays on screen before fading out

    [Header("Telegraph Delay Customization")]
    [Tooltip("How long (in seconds) the fire shows physically on screen BEFORE it actually turns dangerous and hurts the player.")]
    public float damageDelay = 0.5f;     // ⏳ GIVE THE PLAYER A 0.5 SECOND WINDOW TO ESCAPE!

    private bool isDamageActive = false; // Internal state lock
    private bool hasDealtDamage = false;

    private void Start()
    {
        // 1. Start the delay clock countdown
        StartCoroutine(TelegraphDelaySequence());

        // 2. Automatically destroy the object when its lifetime wraps up
        Destroy(gameObject, activeLifetime);
    }

    private IEnumerator TelegraphDelaySequence()
    {
        // The fire is completely harmless while it first shows up physically
        isDamageActive = false;

        yield return new WaitForSeconds(damageDelay);

        // 🔥 THE TELEGRAPH ENDS: The fire turns molten hot right now!
        isDamageActive = true;
        Debug.Log("🔥 FIRE HARMFUL STATE: The flame pillar is now actively burning!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EvaluateBurnContact(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        EvaluateBurnContact(collision);
    }

    /// <summary>
    /// Processes player damage contact only if the telegraph safety timer has passed.
    /// </summary>
    private void EvaluateBurnContact(Collider2D hitCollider)
    {
        // Check 1: Has the damage delay window cleared?
        // Check 2: Has this specific pillar already hit the player?
        // Check 3: Is the object we are touching tagged "Player"?
        if (isDamageActive && !hasDealtDamage && hitCollider.CompareTag("Player"))
        {
            hasDealtDamage = true; // Lock damage so it only hits once per spell cycle

            PlayerHealth health = hitCollider.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(fireDamagePerTick);
                Debug.Log("💥 STAMP BURN HIT: Player failed to escape the delayed fire zone in time!");
            }
        }
    }
}