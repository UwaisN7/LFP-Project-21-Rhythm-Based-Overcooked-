using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPush : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PointManager pointManager;
    [SerializeField] private int playerId;

    [Header("Push Settings")]
    [SerializeField] private float pushRange = 1.5f;
    [SerializeField] private float pushForce = 2f;
    [SerializeField] private float pushDuration = 0.2f;
    [SerializeField] private float cooldown = 2f;

    private CharacterController characterController;
    private bool canPush = true;
    private bool isBeingPushed = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void OnPush(InputValue value)
    {
        if (!value.isPressed) return;
        if (!canPush) return;

        TryPush();
    }

    private void TryPush()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, pushRange);
        bool hitSomeone = false;

        foreach (Collider collider in nearbyObjects)
        {
            PlayerPush otherPlayer = collider.GetComponentInParent<PlayerPush>();

            if (otherPlayer == null || otherPlayer == this) continue;

            Vector3 pushDirection = (otherPlayer.transform.position - transform.position).normalized;
            pushDirection.y = 0f;

            // Push the other player
            otherPlayer.ReceivePush(pushDirection);
            hitSomeone = true;

            // Reward the pusher in the feed
            if (pointManager != null)
            {
                pointManager.LogAction(playerId, "<color=yellow>BULLY! +0</color>");
            }

            StartCoroutine(PushCooldown());
            return; // Only push one person max
        }

        // SPAM PREVENTION: If they missed, penalize them so they can't just spam push
        if (!hitSomeone && pointManager != null)
        {
            pointManager.AddActionPoints(playerId, -25);
            pointManager.LogAction(playerId, "<color=grey>WHIFF! -25</color>");
            StartCoroutine(PushCooldown());
        }
    }

    public void ReceivePush(Vector3 pushDirection)
    {
        if (isBeingPushed) return;

        // Apply the -100 damage to the victim's combo meter
        if (pointManager != null)
        {
            pointManager.AddActionPoints(playerId, -100);
            pointManager.LogAction(playerId, "<color=red>SHOVED! -100</color>");
        }

        StartCoroutine(PushMovement(pushDirection));
    }

    private IEnumerator PushMovement(Vector3 pushDirection)
    {
        isBeingPushed = true;
        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            float movement = (pushForce / pushDuration) * Time.deltaTime;
            characterController.Move(pushDirection * movement);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isBeingPushed = false;
    }

    private IEnumerator PushCooldown()
    {
        canPush = false;
        yield return new WaitForSeconds(cooldown);
        canPush = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pushRange);
    }
}