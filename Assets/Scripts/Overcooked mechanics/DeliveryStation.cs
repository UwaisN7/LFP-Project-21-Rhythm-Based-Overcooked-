using UnityEngine;

public class DeliveryStation : MonoBehaviour, IInteractable
{
    public void Interact(PlayerInteraction player)
    {
        // Player needs to be holding something
        if (!player.IsHolding)
        {
            Debug.Log("You need to be holding a finished pizza!");
            return;
        }

        // Check if the player is holding a finished dish
        FinalDish finalDish =
            player.HeldObject.GetComponent<FinalDish>();

        if (finalDish == null)
        {
            Debug.Log("This is not a finished pizza!");
            return;
        }

        // Remove the pizza from the player's hands
        GameObject deliveredPizza = player.RemoveHeldObject();

        if (deliveredPizza != null)
        {
            Destroy(deliveredPizza);

            // Give the player one point
            GivePoint(player);

            Debug.Log("Pizza delivered! +1 point");
        }
    }


    private void GivePoint(PlayerInteraction player)
    {
        // Point system will go here
        // For now, just a placeholder.

        Debug.Log("PLAYER SCORED 1 POINT!");
    }
}
