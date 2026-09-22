using UnityEngine;

public class Counter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform placementPoint;

    private GameObject objectOnCounter;

    public void Interact(PlayerInteraction player)
    {
        // COUNTER IS EMPTY
        if (objectOnCounter == null)
        {
            // Place whatever the player is holding
            if (player.IsHolding)
            {
                objectOnCounter = player.PlaceHeldObject(placementPoint);

                if (objectOnCounter != null)
                {
                    Debug.Log("Object placed on counter!");
                }
            }

            return;
        }

        // COUNTER HAS SOMETHING ON IT
        if (player.IsHolding)
        {
            // Check if the object on the counter is a plate
            Plate plate = objectOnCounter.GetComponent<Plate>();

            if (plate != null)
            {
                // Check what the player is holding
                Ingredient ingredient =
                    player.HeldObject.GetComponent<Ingredient>();

                // Only allow chopped ingredients
                if (ingredient != null &&
                    ingredient.State == IngredientState.Chopped)
                {
                    GameObject ingredientObject =
                        player.RemoveHeldObject();

                    if (ingredientObject != null)
                    {
                        plate.AddIngredient(ingredientObject);

                        Debug.Log("Ingredient added to plate!");
                    }
                }
                else
                {
                    Debug.Log("This ingredient needs to be chopped first!");
                }
            }

            return;
        }

        // PLAYER IS NOT HOLDING ANYTHING
        // Pick up whatever is on the counter
        if (!player.IsHolding)
        {
            player.Pickup(objectOnCounter);
            objectOnCounter = null;
        }
    }
}