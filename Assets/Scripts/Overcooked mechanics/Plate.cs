using UnityEngine;

public class Plate : MonoBehaviour, IInteractable
{
    [Header("Ingredient Visuals")]
    [SerializeField] private GameObject tomatoVisual;
    [SerializeField] private GameObject cheeseVisual;
    [SerializeField] private GameObject doughVisual;
  

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

        // Only chopped ingredients can be added to a plate
        if (ingredientScript.State != IngredientState.Chopped)
        {
            Debug.Log("This ingredient needs to be chopped first!");
            return;
        }

        switch (ingredientScript.Type)
        {
            case IngredientType.Tomato:
                tomatoVisual.SetActive(true);
                break;

            case IngredientType.Cheese:
                cheeseVisual.SetActive(true);
                break;

            case IngredientType.Dough:
                doughVisual.SetActive(true);
                break;

        }

        Destroy(ingredient);
    }

    public bool HasAllIngredients()
    {
        return tomatoVisual.activeSelf &&
               cheeseVisual.activeSelf &&
               doughVisual.activeSelf;
    }
}
