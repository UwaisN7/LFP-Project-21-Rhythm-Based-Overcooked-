
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour, IInteractable
{
    [Header("Ingredient Visuals")]

    [SerializeField] public GameObject tomatoVisual;
    [SerializeField] public GameObject cheeseVisual;
    [SerializeField] public GameObject doughVisual;
    [SerializeField] public GameObject pineappleVisual;
    [SerializeField] public GameObject hamVisual;
    [SerializeField] public GameObject pepperoniVisual;
    [SerializeField] public GameObject chickenVisual;
    [SerializeField] public GameObject fetaVisual;

    // Stores all ingredients added to this plate
    private HashSet<IngredientType> addedIngredients =
        new HashSet<IngredientType>();

    public void Interact(PlayerInteraction player)
    {
        // Pick up the plate
        if (!player.IsHolding)
        {
            player.Pickup(gameObject);
        }
    }

    public void AddIngredient(GameObject ingredient)
    {
        Ingredient ingredientScript = ingredient.GetComponent<Ingredient>();

        if (ingredientScript == null)
        {
            Debug.LogWarning("Object does not have an Ingredient component!");
            return;
        }

        // Only chopped ingredients can be added
        if (ingredientScript.State != IngredientState.Chopped)
        {
            Debug.Log("This ingredient needs to be chopped first!");
            return;
        }

        IngredientType type = ingredientScript.Type;

        // Prevent duplicate ingredients
        if (addedIngredients.Contains(type))
        {
            Debug.Log("This ingredient is already on the plate!");
            return;
        }

        // Add ingredient to collection
        addedIngredients.Add(type);

        // Show corresponding visual
        SetIngredientVisual(type, true);

        // Remove physical ingredient
        Destroy(ingredient);
    }

    private void SetIngredientVisual(IngredientType type, bool active)
    {
        GameObject visual = null;

        switch (type)
        {
            case IngredientType.Tomato:
                visual = tomatoVisual;
                break;

            case IngredientType.Cheese:
                visual = cheeseVisual;
                break;

            case IngredientType.Dough:
                visual = doughVisual;
                break;

            case IngredientType.Pineapple:
                visual = pineappleVisual;
                break;

            case IngredientType.Ham:
                visual = hamVisual;
                break;

            case IngredientType.Pepperoni:
                visual = pepperoniVisual;
                break;

            case IngredientType.Chicken:
                visual = chickenVisual;
                break;

            case IngredientType.Feta:
                visual = fetaVisual;
                break;
        }

        if (visual != null)
        {
            visual.SetActive(active);
        }
        else
        {
            Debug.LogWarning("No visual assigned for " + type);
        }
    }

    // Returns all ingredients currently on the plate
    public HashSet<IngredientType> GetIngredients()
    {
        return new HashSet<IngredientType>(addedIngredients);
    }

    // Checks whether a specific recipe matches this plate
    public bool MatchesRecipe(PizzaRecipe recipe)
    {
        if (recipe == null)
            return false;

        if (addedIngredients.Count != recipe.requiredIngredients.Length)
            return false;

        foreach (IngredientType required in recipe.requiredIngredients)
        {
            if (!addedIngredients.Contains(required))
                return false;
        }

        return true;
    }

    public bool HasIngredients()
    {
        return addedIngredients.Count > 0;
    }
}
