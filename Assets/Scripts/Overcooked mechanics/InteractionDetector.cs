using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    private PlayerInteraction player;

    // Tracks which colliders belong to each interactable.
    private Dictionary<IInteractable, HashSet<Collider>> nearbyInteractables
        = new Dictionary<IInteractable, HashSet<Collider>>();

    private void Awake()
    {
        player = GetComponentInParent<PlayerInteraction>();
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable =
            other.GetComponentInParent<IInteractable>();

        if (interactable == null)
            return;

        if (!nearbyInteractables.ContainsKey(interactable))
        {
            nearbyInteractables[interactable] = new HashSet<Collider>();
        }

        nearbyInteractables[interactable].Add(other);

        // Register only once, even if the object has multiple colliders.
        if (nearbyInteractables[interactable].Count == 1)
        {
            player.RegisterInteractable(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable =
            other.GetComponentInParent<IInteractable>();

        if (interactable == null)
            return;

        if (!nearbyInteractables.ContainsKey(interactable))
            return;

        nearbyInteractables[interactable].Remove(other);

        // Only unregister when all its colliders have exited.
        if (nearbyInteractables[interactable].Count == 0)
        {
            nearbyInteractables.Remove(interactable);
            player.UnregisterInteractable(interactable);
        }
    }
}