using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyRangedHaunt : MonoBehaviour
{
    [Header("Mob Configs")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float fireRate = 2f;

    [Header("Projectile Reference")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform nozzlePoint;

    private Rigidbody2D rb;
    private Transform player;
    private float nextFireTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 direction = (player.position - transform.position).normalized;

        if (distance <= attackRange)
        {
            // Back off slightly or stay static to shoot
            rb.linearVelocity = Vector2.zero;
            if (Time.time >= nextFireTime)
            {
                FireProjectile(direction);
            }
        }
        else
        {
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    private void FireProjectile(Vector2 fireDirection)
    {
        nextFireTime = Time.time + fireRate;

        if (projectilePrefab != null && nozzlePoint != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, nozzlePoint.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = fireDirection * 6.5f; // Fast projectile speed spec
            }
        }
    }
}