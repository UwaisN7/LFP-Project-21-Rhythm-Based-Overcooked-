using UnityEngine;

[CreateAssetMenu(fileName = "New Pizza Recipe", menuName = "Pizza/Recipe")]
public class PizzaRecipe : ScriptableObject
{
    public string recipeName;

    public IngredientType[] requiredIngredients;

    public GameObject finishedPizzaPrefab;

    public Sprite recipeIcon;

    public int points;
}