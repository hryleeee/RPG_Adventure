using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Healing Properties")]
    [Tooltip("How many hearts this item restores when touched by the player.")]
    public int healAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🔍 Check if the entity stepping into our space has the correct engine Tag
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Core Optimization: Only grab the item if the player is actually hurt!
                if (playerHealth.CurrentHealth < playerHealth.maxHealth)
                {
                    // 1. Pass the heal points to the player
                    playerHealth.Heal(healAmount);

                    // 2. Play a sound or drop a small splash effect prefab here if your artists make one!

                    // 3. Vaporize the item from the map matrix so they can't reuse it infinite times
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("[HealthPickup] Player walked over item, but is already at full capacity.");
                }
            }
        }
    }
}