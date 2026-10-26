using UnityEngine;

public enum IngredientType { Tomato, Cheese, Dough }
public enum IngredientState { Raw, Chopped, burnt }

public class Ingredient : MonoBehaviour, IInteractable
{
    [SerializeField] private IngredientType ingredientType;
    [SerializeField] private IngredientState ingredientState;
    [SerializeField] private GameObject choppedVersion;

    // Set this per prefab in the Inspector (e.g., Tomato = 3, Dough = 5)
    [SerializeField] private int requiredPatternLength = 4;

    public IngredientType Type => ingredientType;
    public IngredientState State => ingredientState;
    public GameObject ChoppedVersion => choppedVersion;
    public int RequiredPatternLength => requiredPatternLength;

    public void Interact(PlayerInteraction player)
    {
        if (!player.IsHolding)
        {
            player.Pickup(gameObject);
        }
    }
}