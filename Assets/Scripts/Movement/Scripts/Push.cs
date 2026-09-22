using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPush : MonoBehaviour
{
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
        if (!value.isPressed)
            return;

        if (!canPush)
            return;

        TryPush();
    }


    private void TryPush()
    {
        // Find all colliders close to the player
        Collider[] nearbyObjects =
            Physics.OverlapSphere(transform.position, pushRange);

        foreach (Collider collider in nearbyObjects)
        {
            PlayerPush otherPlayer =
                collider.GetComponentInParent<PlayerPush>();

            // Don't push yourself
            if (otherPlayer == null || otherPlayer == this)
                continue;

            // Calculate direction from us to the other player
            Vector3 pushDirection =
                (otherPlayer.transform.position - transform.position).normalized;

            // Ignore vertical direction
            pushDirection.y = 0f;

            // Push the other player
            otherPlayer.ReceivePush(pushDirection);

            // Start cooldown
            StartCoroutine(PushCooldown());

            Debug.Log("Player pushed!");

            return;
        }
    }


    public void ReceivePush(Vector3 pushDirection)
    {
        if (isBeingPushed)
            return;

        StartCoroutine(PushMovement(pushDirection));
    }


    private IEnumerator PushMovement(Vector3 pushDirection)
    {
        isBeingPushed = true;

        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            float movement =
                (pushForce / pushDuration) * Time.deltaTime;

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
        Gizmos.DrawWireSphere(
            transform.position,
            pushRange
        );
    }
}