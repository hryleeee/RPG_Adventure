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
        if (value.isPressed)
        {
            isAttacking = true;
            isGuarding  = false;
        }
        else
        {
            isAttacking = false;
        }
    }

    /// <summary>Called by PlayerInput when Crouch action fires — used as Guard here.</summary>
    private void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            isGuarding  = true;
            isAttacking = false;
        }
        else
        {
            isGuarding = false;
        }
    }

    // -------------------------------------------------------
    // PHYSICS UPDATE
    // -------------------------------------------------------
    private void FixedUpdate()
    {
        if (rb == null) return;

        // Can't move while guarding
        if (isGuarding)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

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
