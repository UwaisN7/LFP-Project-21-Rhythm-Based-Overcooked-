using UnityEngine;

public class FinalDish : MonoBehaviour, IInteractable
{
    public PizzaRecipe Recipe { get; private set; }

    public void SetRecipe(PizzaRecipe recipe)
    {
        Recipe = recipe;
    }

    public void Interact(PlayerInteraction player)
    {
        if (!player.IsHolding)
        {
            player.Pickup(gameObject);
        }
    }
}