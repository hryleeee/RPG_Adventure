using UnityEngine;

/// <summary>
/// Spawns exactly X Demon1 instances at the defined spawn points when the scene loads.
/// </summary>
public class DemonSpawner : MonoBehaviour
{
    [Header("Demon Prefab")]
    [Tooltip("Drag the Demon1 Prefab here")]
    public GameObject demonPrefab;

    [Header("Spawn Settings")]
    [Tooltip("How many demons to spawn")]
    public int spawnCount = 3;

    [Tooltip("Specific world positions to spawn demons.")]
    public Transform[] spawnPoints;

    // Shifted from Start to Awake to completely prevent the LevelManager race condition
    private void Awake()
    {
        if (demonPrefab == null)
        {
            Debug.LogError("[DemonSpawner] demonPrefab is not assigned!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos;

            if (spawnPoints != null && i < spawnPoints.Length && spawnPoints[i] != null)
            {
                // Use the designated spawn point
                spawnPos = spawnPoints[i].position;
            }
            else
            {
                // Fallback: spread around the spawner's own position
                float angle = (360f / spawnCount) * i * Mathf.Deg2Rad;
                float radius = 3f;
                spawnPos = transform.position + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f
                );
            }

            Instantiate(demonPrefab, spawnPos, Quaternion.identity);
        }
    }
}