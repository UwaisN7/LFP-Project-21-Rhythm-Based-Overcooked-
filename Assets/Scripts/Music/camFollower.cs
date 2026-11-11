using UnityEngine;

public class SmoothCamFollower : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("How long the camera takes to catch up. Higher = smoother/slower")]
    [Range(0f, 2f)]
    public float followDamping = 0.25f;

    [Tooltip("Optional offset from the target")]
    public Vector3 offset = Vector3.zero;

    [Header("Look Ahead (optional)")]
    [Tooltip("How far ahead to look based on target's velocity")]
    public float lookAheadAmount = 0f;
    public float lookAheadDamping = 0.5f;

    [Header("Axis Locking")]
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;

    private Vector3 _velocity = Vector3.zero;
    private Vector3 _currentLookAhead = Vector3.zero;
    private Vector3 _lastTargetPos;
    private Vector3 _targetVelocity;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("SmoothCamFollower: No target assigned!");
            return;
        }

        // Snap to target on start (keeps current position feel)
        _lastTargetPos = target.position;

        // If you want it to KEEP its current position and only follow from here,
        // this does NOT snap the camera. Remove the line below if you want a snap.
        // transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate target velocity for look-ahead
        _targetVelocity = (target.position - _lastTargetPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastTargetPos = target.position;

        // Build desired position with optional offset
        Vector3 desiredPos = target.position + offset;

        // Smooth look-ahead based on movement direction
        if (lookAheadAmount > 0f)
        {
            Vector3 desiredLookAhead = _targetVelocity * lookAheadAmount;
            _currentLookAhead = Vector3.Lerp(
                _currentLookAhead,
                desiredLookAhead,
                1f - Mathf.Exp(-lookAheadDamping * Time.deltaTime * 60f)
            );
            desiredPos += _currentLookAhead;
        }

        // Preserve axes we don't want to follow
        if (!followX) desiredPos.x = transform.position.x;
        if (!followY) desiredPos.y = transform.position.y;
        if (!followZ) desiredPos.z = transform.position.z;

        // SmoothDamp = Cinemachine-style smoothing (frame-rate independent)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref _velocity,
            followDamping
        );
    }

    // Optional: call this to instantly snap to target (e.g., on teleport/respawn)
    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        _velocity = Vector3.zero;
        _currentLookAhead = Vector3.zero;
    }
}