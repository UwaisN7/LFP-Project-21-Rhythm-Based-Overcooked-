using UnityEngine;

public class Ingredient : MonoBehaviour, IInteractable
{
    public void Interact(PlayerInteraction player)
    {
        player.TryPickup(gameObject);
    }
}