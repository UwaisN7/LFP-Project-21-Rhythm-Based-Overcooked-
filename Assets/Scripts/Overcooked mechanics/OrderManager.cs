using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private Transform orderContainer;
    [SerializeField] private OrderCardUI orderCardPrefab;

    [Header("Order Settings")]
    [SerializeField] private float spawnInterval = 15f;
    [SerializeField] private float orderLifetime = 60f;
    [SerializeField] private int maxActiveOrders = 3;

    private float spawnTimer;

    private class ActiveOrder
    {
        public PizzaRecipe recipe;
        public float timeRemaining;
        public OrderCardUI card;
    }

    private List<ActiveOrder> activeOrders = new List<ActiveOrder>();

    private void Start()
    {
        // Spawn the first order immediately.
        SpawnOrder();
    }

    private void Update()
    {
        // Update existing orders.
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            ActiveOrder order = activeOrders[i];

            order.timeRemaining -= Time.deltaTime;

            if (order.card != null)
                order.card.UpdateTimer(order.timeRemaining);

            // Expire the order when its timer reaches zero.
            if (order.timeRemaining <= 0f)
            {
                Debug.Log("Order expired: " + order.recipe.recipeName);

                if (order.card != null)
                    Destroy(order.card.gameObject);

                activeOrders.RemoveAt(i);
            }
        }

        // Only spawn orders when there is an available slot.
        if (activeOrders.Count < maxActiveOrders)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                SpawnOrder();
                spawnTimer = 0f;
            }
        }
    }

    private void SpawnOrder()
    {
        if (recipeManager == null || orderContainer == null || orderCardPrefab == null)
        {
            Debug.LogError("OrderManager is missing a reference!", this);
            return;
        }

        // Find recipes that aren't already active.
        List<PizzaRecipe> availableRecipes = new List<PizzaRecipe>();

        foreach (PizzaRecipe recipe in recipeManager.Recipes)
        {
            if (recipe == null)
                continue;

            bool alreadyActive = false;

            foreach (ActiveOrder order in activeOrders)
            {
                if (order.recipe == recipe)
                {
                    alreadyActive = true;
                    break;
                }
            }

            if (!alreadyActive)
                availableRecipes.Add(recipe);
        }

        if (availableRecipes.Count == 0)
        {
            Debug.LogWarning("No available recipes to spawn!");
            return;
        }

        // Pick a random available recipe.
        PizzaRecipe selectedRecipe =
            availableRecipes[Random.Range(0, availableRecipes.Count)];

        // Create the order card.
        OrderCardUI newCard = Instantiate(orderCardPrefab, orderContainer);
        newCard.Setup(selectedRecipe);
        newCard.UpdateTimer(orderLifetime);

        // Store the active order.
        ActiveOrder newOrder = new ActiveOrder
        {
            recipe = selectedRecipe,
            timeRemaining = orderLifetime,
            card = newCard
        };

        activeOrders.Add(newOrder);

        Debug.Log("New order spawned: " + selectedRecipe.recipeName);
    }

    // Called when a pizza is delivered.
    public bool TryCompleteOrder(PizzaRecipe recipe, out int timeLeft)
    {
        timeLeft = 0;

        if (recipe == null)
            return false;

        for (int i = 0; i < activeOrders.Count; i++)
        {
            ActiveOrder order = activeOrders[i];

            if (order.recipe == recipe)
            {
                // Capture remaining time BEFORE removing the order.
                timeLeft = Mathf.CeilToInt(order.timeRemaining);

                if (order.card != null)
                    Destroy(order.card.gameObject);

                activeOrders.RemoveAt(i);

                Debug.Log(
                    "Order completed: " + recipe.recipeName +
                    " | Time remaining: " + timeLeft
                );

                return true;
            }
        }

        Debug.Log("No matching active order for this pizza!");
        return false;
    }
}