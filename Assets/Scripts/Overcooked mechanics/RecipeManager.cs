
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    [Header("Available Recipes")]
    [SerializeField] private PizzaRecipe[] recipes;

    [Header("Failed Pizza")]
    [SerializeField] private GameObject failedPizzaPrefab;

    public GameObject GetPizzaPrefab(Plate plate)
    {
        if (plate == null)
            return failedPizzaPrefab;

        foreach (PizzaRecipe recipe in recipes)
        {
            if (recipe == null)
                continue;

            if (plate.MatchesRecipe(recipe))
            {
                Debug.Log("Recipe matched: " + recipe.recipeName);
                return recipe.finishedPizzaPrefab;
            }
        }

        Debug.Log("No matching recipe! Failed pizza.");
        return failedPizzaPrefab;
    }
}
