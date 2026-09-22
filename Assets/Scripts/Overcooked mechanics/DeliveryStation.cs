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
        {
            Debug.Log("You need to be holding a finished pizza!");
            return;
        }

     
        FinalDish finalDish = player.HeldObject.GetComponent<FinalDish>();

        if (finalDish == null)
        {
            Debug.Log("This is not a finished pizza!");
            return;
        }

       
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
            Debug.Log($"[DeliveryStation] DEBUG: Space pressed — simulating delivery for Player {debugPlayerId}");

        DeliverPizza(debugPlayerId);
    }
}