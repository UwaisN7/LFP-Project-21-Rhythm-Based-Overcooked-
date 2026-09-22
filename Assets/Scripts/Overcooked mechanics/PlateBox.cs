using UnityEngine;

public class PlateBox : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject platePrefab;

    public void Interact(PlayerInteraction player)
    {
        if (player.IsHolding)
            return;

        GameObject plate = Instantiate(platePrefab);

        player.Pickup(plate);
    }
}