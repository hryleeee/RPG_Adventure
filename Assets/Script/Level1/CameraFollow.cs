using UnityEngine;

/// <summary>
/// Attach this to the Main Camera.
/// It will smoothly follow the Player around the castle.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag your Player GameObject here")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows (lower = snappier, higher = floatier)")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.1f;

    [Tooltip("Offset from the player position (keep Z at -10 for 2D)")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private void LateUpdate()
    {
        if (target == null)
        {
            // Auto-find player if not assigned
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
            return;
        }

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, offset.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }
}
