using UnityEngine;

public class IngredientBox : MonoBehaviour, IInteractable
{
    [Header("Ingredient")]
    [SerializeField] private GameObject ingredientPrefab;

    public void Interact(PlayerInteraction player)
    {
        if (player.IsHolding)
            return;

        GameObject ingredient = Instantiate(ingredientPrefab);

        player.TryPickup(ingredient);
    }
}