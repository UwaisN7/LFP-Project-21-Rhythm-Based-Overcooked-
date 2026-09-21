using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    private PlayerInteraction playerInteraction;

    private readonly List<IInteractable> nearbyInteractables = new();

    private void Awake()
    {
        playerInteraction = GetComponentInParent<PlayerInteraction>();
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable == null)
            return;

        if (!nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
        }

        UpdateCurrentInteractable();
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable == null)
            return;

        nearbyInteractables.Remove(interactable);

        UpdateCurrentInteractable();
    }

    private void UpdateCurrentInteractable()
    {
        IInteractable bestInteractable = null;
        float bestScore = -Mathf.Infinity;

        Vector3 playerPosition = transform.parent.position;
        Vector3 playerForward = transform.parent.forward;

        foreach (IInteractable interactable in nearbyInteractables)
        {
            if (interactable == null)
                continue;

            Component component = interactable as Component;

            if (component == null)
                continue;

            Vector3 directionToObject =
                component.transform.position - playerPosition;

            directionToObject.y = 0f;

            if (directionToObject.sqrMagnitude < 0.001f)
                continue;

            directionToObject.Normalize();

            float facingScore =
                Vector3.Dot(playerForward, directionToObject);

            if (facingScore > bestScore)
            {
                bestScore = facingScore;
                bestInteractable = interactable;
            }
        }

        playerInteraction.SetInteractable(bestInteractable);
    }
}