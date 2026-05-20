using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach this to a CHILD GameObject of the Player called "AttackHitbox".
/// It needs a Collider2D set to isTrigger = TRUE.
///
/// ── Setup ────────────────────────────────────────────────────────────────
///  1. Create a child GameObject under Player → name it "AttackHitbox"
///  2. Add a BoxCollider2D → isTrigger = TRUE
///     Size it to cover the player's weapon swing area
///  3. Add this script to that child object
///  4. The parent Player must have an Animator with bool "isAttacking"
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class PlayerAttackHitbox : MonoBehaviour
{
    private static readonly int AnimIsAttacking = Animator.StringToHash("isAttacking");

    private Animator   playerAnimator;
    private Collider2D hitboxCollider;

    // Track which demons we already hit this swing so we don't double-kill
    private readonly HashSet<Demon1Controller> hitThisSwing = new HashSet<Demon1Controller>();

    // Were we attacking last frame? Used to detect the START of a new swing
    private bool wasAttackingLastFrame;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        playerAnimator = GetComponentInParent<Animator>();

        if (playerAnimator == null)
            Debug.LogWarning("[PlayerAttackHitbox] No Animator found on parent Player.");
        if (hitboxCollider == null)
            Debug.LogWarning("[PlayerAttackHitbox] No Collider2D found on AttackHitbox.");

        // Start with hitbox off
        if (hitboxCollider != null)
            hitboxCollider.enabled = false;
    }

    private void Update()
    {
        if (playerAnimator == null || hitboxCollider == null) return;

        bool isAttackingNow = playerAnimator.GetBool(AnimIsAttacking);

        // New swing just started — clear the already-hit list
        if (isAttackingNow && !wasAttackingLastFrame)
        {
            hitThisSwing.Clear();
            Debug.Log("[PlayerAttackHitbox] Attack swing started.");
        }

        // Enable collider only while attacking
        hitboxCollider.enabled = isAttackingNow;

        wasAttackingLastFrame = isAttackingNow;
    }

    // Fires when the hitbox first overlaps something
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    // Fires every frame while overlapping — catches demons that were already
    // inside the hitbox when the attack started
    private void OnTriggerStay2D(Collider2D other)
    {
        TryHit(other);
    }

    private void TryHit(Collider2D other)
    {
        if (playerAnimator == null || !playerAnimator.GetBool(AnimIsAttacking))
            return;

        Demon1Controller demon = other.GetComponent<Demon1Controller>();
        if (demon == null) return;

        // Only kill each demon once per swing
        if (hitThisSwing.Contains(demon)) return;

        hitThisSwing.Add(demon);
        Debug.Log($"[PlayerAttackHitbox] Hit demon: {demon.gameObject.name}");
        demon.Die();
    }
}
