using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player movement, animation, facing direction, and attack hit detection.
/// Attack damage is applied via Physics2D.OverlapCircleAll at the moment of swing —
/// no child hitbox object needed.
///
/// Required components on the Player GameObject:
///   - Rigidbody2D       (Gravity Scale = 0, Freeze Rotation Z)
///   - Animator          (with 'player' controller assigned)
///   - SpriteRenderer
///   - BoxCollider2D or CapsuleCollider2D
///   - PlayerInput       (Actions = InputSystem_Actions, Behavior = Send Messages)
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed      = 5f;
    public float sprintMultiplier = 1.8f;

    [Header("Attack Settings")]
    [Tooltip("Radius of the attack hit circle around the player")]
    public float attackRadius = 1.2f;

    [Tooltip("Duration the attack window stays open (should match animation length)")]
    public float attackDuration = 0.5f;

    // Component references
    private Rigidbody2D    rb;
    private Animator       animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth   playerHealth;

    // Input state
    private Vector2 moveInput;
    private bool    isSprinting;
    private bool    isAttacking;
    private bool    isGuarding;

    // Animator parameter hashes
    private static readonly int AnimIsMoving    = Animator.StringToHash("isMoving");
    private static readonly int AnimIsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int AnimIsGuarding  = Animator.StringToHash("isGuarding");
    private static readonly int AnimMoveX       = Animator.StringToHash("moveX");
    private static readonly int AnimMoveY       = Animator.StringToHash("moveY");

    private void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        animator       = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth   = GetComponent<PlayerHealth>();

        if (rb == null)           Debug.LogWarning("[PlayerController] Rigidbody2D missing.");
        if (animator == null)     Debug.LogWarning("[PlayerController] Animator missing.");
        if (playerHealth == null) Debug.LogWarning("[PlayerController] PlayerHealth missing.");
    }

    // ── Input callbacks (New Input System — Send Messages) ────────────────

    private void OnMove(InputValue value)   { moveInput  = value.Get<Vector2>(); }
    private void OnSprint(InputValue value) { isSprinting = value.isPressed; }

    private void OnAttack(InputValue value)
    {
        if (value.isPressed && !isAttacking)
        {
            isAttacking = true;
            isGuarding  = false;
            StartCoroutine(DoAttack());
        }
    }

    private void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            isGuarding = !isGuarding;
            if (isGuarding) isAttacking = false;
        }
    }

    // ── Attack coroutine — hits demons at the moment of swing ─────────────

    private IEnumerator DoAttack()
    {
        // Small delay so the animation has started before we scan for hits
        yield return new WaitForSeconds(0.1f);

        // Scan all colliders within attackRadius around the player
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        foreach (Collider2D hit in hits)
        {
            // Skip self
            if (hit.gameObject == gameObject) continue;

            Demon1Controller demon = hit.GetComponent<Demon1Controller>();
            if (demon != null)
            {
                Debug.Log($"[PlayerController] Attack hit: {demon.gameObject.name}");
                demon.Die();
            }
        }

        // Wait out the rest of the attack animation
        yield return new WaitForSeconds(attackDuration - 0.1f);
        isAttacking = false;
    }

    // ── Physics ───────────────────────────────────────────────────────────

    private void FixedUpdate()
    {
        if (rb == null) return;

        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        if (isGuarding) speed *= 0.4f;

        Vector2 velocity = moveInput.sqrMagnitude > 1f
            ? moveInput.normalized * speed
            : moveInput * speed;

        rb.linearVelocity = velocity;
    }

    // ── Animation & facing ────────────────────────────────────────────────

    private void Update()
    {
        UpdateAnimator();
        UpdateFacing();

        // Keep PlayerHealth in sync with guard state every frame
        if (playerHealth != null)
            playerHealth.SetGuarding(isGuarding);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f && !isGuarding;

        animator.SetBool(AnimIsMoving,    isMoving);
        animator.SetBool(AnimIsAttacking, isAttacking);
        animator.SetBool(AnimIsGuarding,  isGuarding);

        if (isMoving)
        {
            animator.SetFloat(AnimMoveX, moveInput.x);
            animator.SetFloat(AnimMoveY, moveInput.y);
        }
    }

    private void UpdateFacing()
    {
        if (spriteRenderer == null) return;

        if (moveInput.x > 0.01f)       spriteRenderer.flipX = false;
        else if (moveInput.x < -0.01f) spriteRenderer.flipX = true;
    }

    // ── Gizmo — visualise attack radius in Scene view ────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
