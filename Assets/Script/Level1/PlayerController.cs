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
    public float moveSpeed = 5f;

    [Header("Attack Settings")]
    [Tooltip("Radius of the attack hit circle around the player")]
    public float attackRadius = 1.2f;

    [Tooltip("Duration the attack window stays open (should match animation length)")]
    public float attackDuration = 0.5f;

    [Header("Knockback Settings")]
    [Tooltip("The speed force applied to shove enemies backwards.")]
    public float knockbackForce = 12f;
    [Tooltip("How long enemies stay physically paralyzed before recovering AI control.")]
    public float knockbackDuration = 0.2f;

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

    private void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }

    private void OnAttack(InputValue value)
    {
        if (value.isPressed && !isAttacking && !isGuarding)
        {
            isAttacking = true;
            StartCoroutine(DoAttack());
        }
    }

    // ── Attack coroutine ──────────────────────────────────────────────────
    private IEnumerator DoAttack()
    {
        yield return new WaitForSeconds(0.1f);

        // Sweeps the red wire sphere zone for colliders
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (!hit.CompareTag("Enemy")) continue; // Safety validation line

            GameObject enemyTarget = hit.gameObject;

            // 💥 1. APPLY PHYSICAL KNOCKBACK (FIXED VARIABLE REFERENCE TYPO)
            EnemyKnockback knockback = enemyTarget.GetComponent<EnemyKnockback>();
            if (knockback != null)
            {
                // Correct variable reference pointer used here: 'knockback'
                knockback.TriggerKnockback(transform.position, knockbackForce, knockbackDuration);
            }

            // 💀 2. DAMAGE THE UNIVERSAL HEALTH SYSTEM
            EnemyHealth enemyHealth = enemyTarget.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1); // Deals 1 point of weapon damage
            }
            else
            {
                // 🛡️ LEGACY FALLBACK: Automatically handles unconfigured map prefabs
                Demon1Controller demon1 = enemyTarget.GetComponent<Demon1Controller>();
                if (demon1 != null) demon1.Die();

                Demon2Controller demon2 = enemyTarget.GetComponent<Demon2Controller>();
                if (demon2 != null) demon2.Die();
            }
        }

        yield return new WaitForSeconds(attackDuration - 0.1f);
        isAttacking = false;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        float speed = moveSpeed;
        if (isGuarding) speed *= 0.4f;

        Vector2 velocity = moveInput.sqrMagnitude > 1f
            ? moveInput.normalized * speed
            : moveInput * speed;

        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        HandleBlockingInput();
        UpdateAnimator();
        UpdateFacing();

        if (playerHealth != null)
            playerHealth.SetGuarding(isGuarding);
    }

    private void HandleBlockingInput()
    {
        if (Keyboard.current != null)
        {
            isGuarding = Keyboard.current.cKey.isPressed;
            if (isGuarding) isAttacking = false;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

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