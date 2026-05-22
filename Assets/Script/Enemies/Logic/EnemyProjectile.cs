using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int projectileDamage = 1; // Deals 1 heart of damage on contact
    public float selfDestructTime = 4f; // Safety cleanup timer if it misses everything

    private void Start()
    {
        // Automatically clean up object from computer memory if it flies off-screen
        Destroy(gameObject, selfDestructTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If it touches the player character asset
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(projectileDamage);
            }
            Destroy(gameObject); // Explode bullet on impact
        }
        // If it smashes into a tilemap wall environment block
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Walls") || collision.CompareTag("Walls"))
        {
            Destroy(gameObject); // Explode bullet against wall mesh bounds
        }
    }
}