using UnityEngine;

/// <summary>
/// Spawns exactly 3 Demon1 instances at the defined spawn points when the scene loads.
///
/// ── Setup ────────────────────────────────────────────────────────────────
///  1. Create an empty GameObject in the scene → name it "DemonSpawner"
///  2. Add this script to it
///  3. Assign the Demon1 Prefab to the "demonPrefab" field
///  4. Set "Spawn Count" to 3
///  5. Add 3 elements to "Spawn Points" — drag in empty GameObjects placed
///     at valid floor positions inside the dungeon (away from walls)
///     OR leave Spawn Points empty to use the random offset fallback.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class DemonSpawner : MonoBehaviour
{
    [Header("Demon Prefab")]
    [Tooltip("Drag the Demon1 Prefab here")]
    public GameObject demonPrefab;

    [Header("Spawn Settings")]
    [Tooltip("How many demons to spawn (should be 3)")]
    public int spawnCount = 3;

    [Tooltip("Specific world positions to spawn demons. " +
             "If fewer points than spawnCount are provided, extras use random offsets.")]
    public Transform[] spawnPoints;

    private void Start()
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
                float angle  = (360f / spawnCount) * i * Mathf.Deg2Rad;
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
