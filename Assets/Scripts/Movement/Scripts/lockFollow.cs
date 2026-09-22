using UnityEngine;

public class LockRotationFollow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform parentTarget;
    [SerializeField] private Vector3 positionOffset;

    [Header("Rotation Lock (90 Degrees)")]
    [SerializeField] bool lockX = false;
    [SerializeField] bool lockY = true; // Defaulting to Y axis
    [SerializeField] bool lockZ = false;

    void LateUpdate()
    {
        if (parentTarget == null) return;

        // Follow the parent's position with an optional offset
        transform.position = parentTarget.position + positionOffset;

        // Get current Euler angles
        Vector3 currentRotation = transform.rotation.eulerAngles;

        // Apply 90-degree lock to selected axes, otherwise inherit from parent
        float x = lockX ? 90f : parentTarget.rotation.eulerAngles.x;
        float y = lockY ? 90f : parentTarget.rotation.eulerAngles.y;
        float z = lockZ ? 90f : parentTarget.rotation.eulerAngles.z;

        transform.rotation = Quaternion.Euler(x, y, z);
    }
}