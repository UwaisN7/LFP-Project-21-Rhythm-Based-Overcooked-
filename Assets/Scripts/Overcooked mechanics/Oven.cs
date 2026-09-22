using UnityEngine;

public class Oven : MonoBehaviour, IInteractable
{
    [Header("Cooking")]
    [SerializeField] private float cookingTime = 15f;

    [Header("Final Dish")]
    [SerializeField] private GameObject finalDishPrefab;
    [SerializeField] private Transform dishSpawnPoint;

    private GameObject plateCooking;
    private float cookingTimer;
    private bool isCooking;


    public void Interact(PlayerInteraction player)
    {
        // =========================================
        // START COOKING
        // =========================================

        if (!isCooking)
        {
            // Player must be holding something
            if (!player.IsHolding)
                return;

            // Check if player is holding a plate
            Plate plate = player.HeldObject.GetComponent<Plate>();

            if (plate == null)
            {
                Debug.Log("You need a plate to cook!");
                return;
            }

            // Check that all ingredients are present
            if (!plate.HasAllIngredients())
            {
                Debug.Log("The pizza is missing ingredients!");
                return;
            }

            // Take the plate from the player's hands
            plateCooking = player.RemoveHeldObject();

            // Put it at the oven
            plateCooking.transform.SetParent(transform);
            plateCooking.transform.localPosition = Vector3.zero;
            plateCooking.transform.localRotation = Quaternion.identity;

            // Start timer
            cookingTimer = cookingTime;
            isCooking = true;

            Debug.Log("Pizza is cooking!");

            return;
        }


        // =========================================
        // RHYTHM SYSTEM
        // =========================================

        if (isCooking)
        {
            StartRhythmInteraction();
        }
    }


    private void Update()
    {
        if (!isCooking)
            return;

        cookingTimer -= Time.deltaTime;

        if (cookingTimer <= 0f)
        {
            FinishCooking();
        }
    }


    private void StartRhythmInteraction()
    {
        // =========================================
        // RHYTHM SYSTEM GOES HERE
        // =========================================

        Debug.Log("Rhythm interaction!");

        // Later:
        // RhythmManager.StartRhythm();
        // or
        // cookingTimer -= rhythmBonus;
    }


    private void FinishCooking()
    {
        isCooking = false;

        Debug.Log("Pizza finished cooking!");

        // Remove the uncooked plate
        if (plateCooking != null)
        {
            Destroy(plateCooking);
            plateCooking = null;
        }

        // Spawn final dish
        if (finalDishPrefab != null)
        {
            Instantiate(
                finalDishPrefab,
                dishSpawnPoint.position,
                dishSpawnPoint.rotation
            );
        }
        else
        {
            Debug.LogWarning("No final dish prefab assigned!");
        }
    }
}