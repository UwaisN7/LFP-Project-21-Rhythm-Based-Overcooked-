using UnityEngine;

public class FinalDish : MonoBehaviour, IInteractable
{
    public void Interact(PlayerInteraction player)
    {
        // Only pick up if the player isn't already holding something
        if (!player.IsHolding)
        {
            player.Pickup(gameObject);
        }
    }
}
