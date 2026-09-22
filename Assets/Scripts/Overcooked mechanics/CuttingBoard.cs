using UnityEngine;
using UnityEngine.InputSystem;

public class ChoppingBoard : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform ingredientPoint;

    private GameObject ingredient;

    public void Interact(PlayerInteraction player)
    {
        // Place ingredient on board
        if (player.IsHolding && ingredient == null)
        {
            ingredient = player.PlaceHeldObject(ingredientPoint);

            if (ingredient != null)
            {
                Debug.Log("Ingredient placed on chopping board!");
            }
        }
    }

    public void OnCook(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (ingredient == null)
            return;

        StartRhythmGame();
    }

    private void StartRhythmGame()
    {
        Debug.Log("RHYTHM GAME STARTED!");

        ReplaceWithChoppedIngredient();
    }

    private void ReplaceWithChoppedIngredient()
    {
        Ingredient ingredientScript = ingredient.GetComponent<Ingredient>();

        if (ingredientScript == null)
        {
            Debug.LogWarning("Ingredient does not have an Ingredient script!");
            return;
        }

        GameObject choppedPrefab = ingredientScript.ChoppedVersion;

        if (choppedPrefab == null)
        {
            Debug.LogWarning("No chopped version assigned!");
            return;
        }

        Destroy(ingredient);

        ingredient = Instantiate(
            choppedPrefab,
            ingredientPoint.position,
            ingredientPoint.rotation,
            ingredientPoint
        );
    }
}