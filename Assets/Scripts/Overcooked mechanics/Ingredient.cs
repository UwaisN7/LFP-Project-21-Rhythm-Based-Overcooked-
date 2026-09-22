using UnityEngine;


public class Ingredient : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject choppedVersion;

    public GameObject ChoppedVersion => choppedVersion;

    public void Interact(PlayerInteraction player)
    {
        player.Pickup(gameObject);
    }
}