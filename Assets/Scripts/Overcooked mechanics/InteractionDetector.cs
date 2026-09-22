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
        IInteractable interactable =
            other.GetComponentInParent<IInteractable>();

        if (interactable != null)
        {
            player.SetInteractable(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable =
            other.GetComponentInParent<IInteractable>();

        if (interactable != null)
        {
            player.SetInteractable(null);
        }
    }
}