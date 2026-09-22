using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    private PlayerInteraction player;

    private void Awake()
    {
        player = GetComponentInParent<PlayerInteraction>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for a Counter first
        Counter counter = other.GetComponentInParent<Counter>();

        if (counter != null)
        {
            player.SetInteractable(counter);
            return;
        }

        // Otherwise check for any other interactable
        IInteractable interactable =
            other.GetComponentInParent<IInteractable>();

        if (interactable != null)
        {
            player.SetInteractable(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if we're leaving a Counter
        Counter counter = other.GetComponentInParent<Counter>();

        if (counter != null)
        {
            player.SetInteractable(null);
            return;
        }

        // Otherwise check for other interactables
        IInteractable interactable =
            other.GetComponentInParent<IInteractable>();

        if (interactable != null)
        {
            player.SetInteractable(null);
        }
    }
}