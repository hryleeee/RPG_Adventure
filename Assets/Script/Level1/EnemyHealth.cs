using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return; // Prevent dead bodies from absorbing additional hits

        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} took damage. Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            ExecuteMonsterDeath();
        }
    }

    private void ExecuteMonsterDeath()
    {
        isDead = true;

        // 1. 🎬 TRIGGER DEATH ANIMATION
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 2. 🛡️ DISABLE COMBAT PHYSICS: Let the player walk through the corpse safely
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // 3. 🛑 FREEZE VECTOR MOMENTUM
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 4. 🧠 UNIVERSAL BRAIN SHUTDOWNMATRIX: 
        // Forcefully disable whichever custom brain script this monster is running!
        if (GetComponent<EnemyTouchChaser>() != null) GetComponent<EnemyTouchChaser>().enabled = false;
        if (GetComponent<EnemyCharger>() != null) GetComponent<EnemyCharger>().enabled = false;
        if (GetComponent<EnemyJumper>() != null) GetComponent<EnemyJumper>().enabled = false;
        if (GetComponent<EnemyFireHaunt>() != null) GetComponent<EnemyFireHaunt>().enabled = false;

        // Special Boss Handling: Wipe out his running magic coroutines instantly
        if (GetComponent<EnemyHellBeast>() != null)
        {
            EnemyHellBeast bossBrain = GetComponent<EnemyHellBeast>();
            bossBrain.StopAllCoroutines();
            bossBrain.enabled = false;
        }

        Debug.Log($"💀 {gameObject.name} has fallen. Disabling systems and cleaning up memory core layers.");

        // 5. Clean up game memory after death animation completes
        Destroy(gameObject, 0.8f);
    }
}