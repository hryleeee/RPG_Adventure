using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player movement, animation, and facing direction.
/// Uses Unity's New Input System (InputSystem_Actions).
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
    [Tooltip("Movement speed in units per second")]
    public float moveSpeed = 5f;

    [Tooltip("Sprint multiplier applied when Sprint is held")]
    public float sprintMultiplier = 1.8f;

    // Component references
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Input state
    private Vector2 moveInput;
    private bool isSprinting;
    private bool isAttacking;
    private bool isGuarding;

    // Animator parameter hashes (faster than string lookup)
    private static readonly int AnimIsMoving   = Animator.StringToHash("isMoving");
    private static readonly int AnimIsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int AnimIsGuarding  = Animator.StringToHash("isGuarding");
    private static readonly int AnimMoveX       = Animator.StringToHash("moveX");
    private static readonly int AnimMoveY       = Animator.StringToHash("moveY");

    private void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        animator       = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
            Debug.LogWarning("[PlayerController] Rigidbody2D missing. Add one to the Player.");
        if (animator == null)
            Debug.LogWarning("[PlayerController] Animator missing. Animations won't play.");
    }

    // -------------------------------------------------------
    // NEW INPUT SYSTEM — called automatically by PlayerInput
    // PlayerInput Behavior must be set to "Send Messages"
    // -------------------------------------------------------

    /// <summary>Called by PlayerInput when Move action fires.</summary>
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    /// <summary>Called by PlayerInput when Sprint action fires.</summary>
    private void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    /// <summary>Called by PlayerInput when Attack action fires (Left Click / Gamepad West).</summary>
    private void OnAttack(InputValue value)
    {
        // Only trigger attack on press, not on release
        // The animation's Exit Time will return it to Idle automatically
        if (value.isPressed && !isAttacking)
        {
            isAttacking = true;
            isGuarding  = false;
            // Auto-reset after the attack animation length
            StartCoroutine(ResetAttack());
        }
    }

    /// <summary>Called by PlayerInput when Crouch action fires — used as Guard toggle here.</summary>
    private void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            // Toggle guard on/off each time C is pressed
            isGuarding = !isGuarding;

            if (isGuarding)
            {
                isAttacking = false;
                Debug.Log("[PlayerController] Guard ON");
            }
            else
            {
                Debug.Log("[PlayerController] Guard OFF");
            }
        }
    }

    /// <summary>Resets isAttacking after a short delay matching the attack animation length.</summary>
    private System.Collections.IEnumerator ResetAttack()
    {
        // Wait for the attack animation to finish (adjust time to match your clip length)
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // -------------------------------------------------------
    // PHYSICS UPDATE
    // -------------------------------------------------------
    private void FixedUpdate()
    {
        if (rb == null) return;

        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        // Move slower while guarding
        if (isGuarding) speed *= 0.4f;

        Vector2 velocity = moveInput.sqrMagnitude > 1f
            ? moveInput.normalized * speed
            : moveInput * speed;

        rb.linearVelocity = velocity;
    }

    // -------------------------------------------------------
    // ANIMATION & FACING
    // -------------------------------------------------------
    private void Update()
    {
        UpdateAnimator();
        UpdateFacing();
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

    /// <summary>Flips sprite to face the direction of horizontal movement.</summary>
    private void UpdateFacing()
    {
        if (spriteRenderer == null) return;

        if (moveInput.x > 0.01f)
            spriteRenderer.flipX = false;  // facing right
        else if (moveInput.x < -0.01f)
            spriteRenderer.flipX = true;   // facing left
    }
}
