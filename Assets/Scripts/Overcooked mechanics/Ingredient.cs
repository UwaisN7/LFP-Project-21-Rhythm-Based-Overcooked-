using UnityEngine;

public enum IngredientType
{
    Tomato,
    Cheese,
    Dough
}

public enum IngredientState
{
    Raw,
    Chopped
}

public class Ingredient : MonoBehaviour, IInteractable
{
    [SerializeField] private IngredientType ingredientType;
    [SerializeField] private IngredientState ingredientState;
    [SerializeField] private GameObject choppedVersion;

    public IngredientType Type
    {
        get { return ingredientType; }
    }

    public IngredientState State
    {
        get { return ingredientState; }
    }

    public GameObject ChoppedVersion
    {
        get { return choppedVersion; }
    }

    public void Interact(PlayerInteraction player)
    {
        if (!player.IsHolding)
        {
            player.Pickup(gameObject);
        }
    }

}