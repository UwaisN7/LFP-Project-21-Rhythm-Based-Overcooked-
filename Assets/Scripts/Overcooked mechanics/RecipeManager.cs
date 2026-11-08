
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    [Header("Available Recipes")]
    [SerializeField] private PizzaRecipe[] recipes;

    [Header("Failed Pizza")]
    [SerializeField] private GameObject failedPizzaPrefab;

    public PizzaRecipe[] Recipes => recipes;

    public PizzaRecipe GetMatchingRecipe(Plate plate)
    {
        if (plate == null)
            return null;

        foreach (PizzaRecipe recipe in recipes)
        {
            if (recipe == null)
                continue;

            if (plate.MatchesRecipe(recipe))
            {
                Debug.Log("Recipe matched: " + recipe.recipeName);
                return recipe;
            }
        }

        Debug.Log("No matching recipe! Failed pizza.");
        return null;
    }

    public GameObject GetPizzaPrefab(Plate plate)
    {
        PizzaRecipe recipe = GetMatchingRecipe(plate);

        if (recipe != null)
            return recipe.finishedPizzaPrefab;

        return failedPizzaPrefab;
    }
}