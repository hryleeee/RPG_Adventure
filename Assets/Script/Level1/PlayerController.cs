using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player movement, animation, facing direction, and attack hit detection.
/// Attack damage is applied via Physics2D.OverlapCircleAll at the moment of swing.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // This is now your permanent max speed!

    [Header("Attack Settings")]
    [Tooltip("Radius of the attack hit circle around the player")]
    public float attackRadius = 1.2f;

    [Tooltip("Duration the attack window stays open (should match animation length)")]
    public float attackDuration = 0.5f;

    // Component references
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;

    // Input state
    private Vector2 moveInput;
    private bool isAttacking;
    private bool isGuarding;

    // Animator parameter hashes
    private static readonly int AnimIsMoving = Animator.StringToHash("isMoving");
    private static readonly int AnimIsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int AnimIsGuarding = Animator.StringToHash("isGuarding");
    private static readonly int AnimMoveX = Animator.StringToHash("moveX");
    private static readonly int AnimMoveY = Animator.StringToHash("moveY");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();

        if (rb == null) Debug.LogWarning("[PlayerController] Rigidbody2D missing.");
        if (animator == null) Debug.LogWarning("[PlayerController] Animator missing.");
        if (playerHealth == null) Debug.LogWarning("[PlayerController] PlayerHealth missing.");
    }

    // ── Input callbacks (New Input System — Send Messages) ────────────────

    private void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }

    // ✅ FIXED: Completely deleted 'OnSprint' listener function from this block!

    private void OnAttack(InputValue value)
    {
        if (value.isPressed && !isAttacking && !isGuarding) // Cannot attack while actively holding a block
        {
            isAttacking = true;
            StartCoroutine(DoAttack());
        }
    }

    // ── Attack coroutine ──────────────────────────────────────────────────

    private IEnumerator DoAttack()
    {
        yield return new WaitForSeconds(0.1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // Kill Demon1
            Demon1Controller demon1 = hit.GetComponent<Demon1Controller>();
            if (demon1 != null)
            {
                Debug.Log($"[PlayerController] Attack hit: {demon1.gameObject.name}");
                demon1.Die();
            }

            // Kill Demon2
            Demon2Controller demon2 = hit.GetComponent<Demon2Controller>();
            if (demon2 != null)
            {
                Debug.Log($"[PlayerController] Attack hit: {demon2.gameObject.name}");
                demon2.Die();
            }
        }

        yield return new WaitForSeconds(attackDuration - 0.1f);
        isAttacking = false;
    }

    // ── Physics ───────────────────────────────────────────────────────────

    private void FixedUpdate()
    {
        if (rb == null) return;

        // ✅ FIXED: Removed 'isSprinting' calculation parameter check completely
        float speed = moveSpeed;

        if (isGuarding) speed *= 0.4f; // Slow movement down while holding up the guard

        Vector2 velocity = moveInput.sqrMagnitude > 1f
            ? moveInput.normalized * speed
            : moveInput * speed;

        rb.linearVelocity = velocity;
    }

    // ── Animation & facing ────────────────────────────────────────────────

    private void Update()
    {
        HandleBlockingInput(); // Process block hold check first
        UpdateAnimator();
        UpdateFacing();

        // Keep PlayerHealth in sync with guard state every frame
        if (playerHealth != null)
            playerHealth.SetGuarding(isGuarding);
    }

    private void HandleBlockingInput()
    {
        if (Keyboard.current != null)
        {
            isGuarding = Keyboard.current.cKey.isPressed;

            if (isGuarding)
            {
                isAttacking = false; // Instantly interrupt any attacks if they snap to defensive position
            }
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // Player is moving only if input is registered and they aren't hunkered down blocking
        bool isMoving = moveInput.sqrMagnitude > 0.01f && !isGuarding;

        animator.SetBool(AnimIsMoving, isMoving);
        animator.SetBool(AnimIsAttacking, isAttacking);
        animator.SetBool(AnimIsGuarding, isGuarding);

        if (isMoving)
        {
            animator.SetFloat(AnimMoveX, moveInput.x);
            animator.SetFloat(AnimMoveY, moveInput.y);
        }
    }

    private void UpdateFacing()
    {
        if (spriteRenderer == null) return;

        if (moveInput.x > 0.01f) spriteRenderer.flipX = false;
        else if (moveInput.x < -0.01f) spriteRenderer.flipX = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}