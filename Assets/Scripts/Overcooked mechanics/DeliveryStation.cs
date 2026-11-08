using UnityEngine;
using UnityEngine.InputSystem;

public class DeliveryStation : MonoBehaviour, IInteractable
{
    [Header("Score Values")]
    [SerializeField] private int baseOrderValue = 500;
    [SerializeField] private int timeBonus = 50;

    [Header("Debug")]
    [SerializeField] private bool debugLogging = true;
    [SerializeField] private int debugPlayerId = 0;

    [Header("Refs")]
    [SerializeField] private PointManager pointManager;

    [Header("Order System")]
    [SerializeField] private OrderManager orderManager;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DebugTestDelivery();
        }
    }

 
    public void Interact(PlayerInteraction player)
    {
        if (!player.IsHolding)
            return;

        FinalDish finalDish = player.HeldObject.GetComponent<FinalDish>();

        if (finalDish == null)
            return;

        // Check whether this pizza matches an active order
        if (orderManager != null)
        {
            bool matchedOrder = orderManager.TryCompleteOrder(
                finalDish.Recipe,
                out int orderPoints
            );

            if (matchedOrder)
            {
                Debug.Log("Order completed! Points available: " + orderPoints);
            }
            else
            {
                Debug.Log("No matching active order!");
            }
        }
        else
        {
            Debug.LogError("OrderManager is not assigned!", this);
        }

        // Remove the delivered pizza
        GameObject deliveredPizza = player.RemoveHeldObject();

        if (deliveredPizza != null)
        {
            Destroy(deliveredPizza);
            DeliverPizza(player.PlayerId);
        }
    }


    private void DeliverPizza(int playerId)
    {
        if (pointManager == null)
        {
            Debug.LogError("[DeliveryStation] PointManager reference is missing!");
            return;
        }
        float baseOrderValue = 500;
        float timeBonus = 50;   

        int comboMultiplier = pointManager.GetComboMultiplier(playerId);

        float finalScore = (baseOrderValue + timeBonus) * comboMultiplier;

        pointManager.AddFinalScore(playerId, finalScore);

        if (debugLogging)
        {
            Debug.Log(
                $"[DeliveryStation] Player {playerId} DELIVERED | " +
                $"({baseOrderValue} + {timeBonus}) * {comboMultiplier}x = {finalScore} final score");
        }
    }

    private void DebugTestDelivery()
    {
        if (debugLogging)
            Debug.Log("[DeliveryStation] DEBUG: Space pressed — simulating delivery for ALL players");

        DeliverPizza(0);
        DeliverPizza(1);
    }
}