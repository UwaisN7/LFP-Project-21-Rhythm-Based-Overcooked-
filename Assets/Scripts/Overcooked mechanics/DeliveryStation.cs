using UnityEngine;

public class DeliveryStation : MonoBehaviour, IInteractable
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = true;

    [Header("References")]
    [SerializeField] private PointManager pointManager;
    [SerializeField] private OrderManager orderManager;

    public void Interact(PlayerInteraction player)
    {
        if (!player.IsHolding)
            return;

        FinalDish finalDish = player.HeldObject.GetComponent<FinalDish>();

        if (finalDish == null)
            return;

        PizzaRecipe recipe = finalDish.Recipe;

        if (recipe == null)
        {
            Debug.LogWarning("This pizza has no recipe assigned!");
            return;
        }

        // Check whether this pizza matches an active order.
        bool matchedOrder = false;
        int timeLeft = 0;

        if (orderManager != null)
        {
            matchedOrder = orderManager.TryCompleteOrder(
                recipe,
                out timeLeft
            );
        }
        else
        {
            Debug.LogError("OrderManager reference is missing!", this);
        }

        // Remove the delivered pizza.
        GameObject deliveredPizza = player.RemoveHeldObject();

        if (deliveredPizza != null)
            Destroy(deliveredPizza);

        // Only award points if the pizza matched an active order.
        if (!matchedOrder)
        {
            Debug.Log("Pizza delivered, but no points awarded.");
            return;
        }

        if (pointManager == null)
        {
            Debug.LogError("PointManager reference is missing!", this);
            return;
        }

        pointManager.ApplyFinalScore(
            player.PlayerId,
            recipe,
            timeLeft
        );

        if (debugLogging)
        {
            Debug.Log(
                $"[DeliveryStation] Player {player.PlayerId} delivered " +
                $"{recipe.recipeName} | Recipe Points: {recipe.points} | " +
                $"Time Left: {timeLeft}"
            );
        }
    }
}