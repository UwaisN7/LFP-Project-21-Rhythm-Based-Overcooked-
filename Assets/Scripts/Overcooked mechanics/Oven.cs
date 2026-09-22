using TMPro;
using UnityEngine;

public class Oven : MonoBehaviour, IInteractable
{
    [Header("Cooking")]
    [SerializeField] private float cookingTime = 15f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject timerUI;

    [Header("Final Dish")]
    [SerializeField] private GameObject finalDishPrefab;
    [SerializeField] private Transform dishSpawnPoint;
    [Header("Rhythm")]
    [SerializeField] private float successTimeBonus = 10f;
    [SerializeField] private float failTimePenalty = 0f;

    private PlayerRhythmSession rhythmSession;
    private bool waitingForConfirm;
    private bool handled;
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
            // Player needs to be holding something
            if (!player.IsHolding)
                return;

            // Check if the player is holding a plate
            Plate plate = player.HeldObject.GetComponent<Plate>();

            if (plate == null)
            {
                Debug.Log("You need a plate to cook!");
                return;
            }

            // Check that all ingredients are on the plate
            if (!plate.HasAllIngredients())
            {
                Debug.Log("The pizza is missing ingredients!");
                return;
            }

            // Take the plate from the player's hands
            plateCooking = player.RemoveHeldObject();

            // Put the plate inside the oven
            plateCooking.transform.SetParent(transform);
            plateCooking.transform.localPosition = Vector3.zero;
            plateCooking.transform.localRotation = Quaternion.identity;

            // Start cooking timer
            cookingTimer = cookingTime;
            isCooking = true;

            // Show timer
            if (timerUI != null)
            {
                timerUI.SetActive(true);
            }

            if (timerText != null)
            {
                timerText.text = Mathf.Ceil(cookingTimer).ToString();
            }

            Debug.Log("Pizza is cooking!");

            return;
        }


        // =========================================
        // RHYTHM SYSTEM
        // =========================================

        if (isCooking)
        {
            ArmRhythm(player);
           
        }
    }


    private void Update()
    {
        if (!isCooking)
            return;

        // Count down
        cookingTimer -= Time.deltaTime;

        // Update visual timer
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(cookingTimer).ToString();
        }

        // Finish cooking
        if (cookingTimer <= 0f)
        {
            FinishCooking();
        }
    }


    private void ArmRhythm(PlayerInteraction player)
    {
        if (waitingForConfirm) return;

        rhythmSession = player.GetComponent<PlayerRhythmSession>();
        if (rhythmSession == null)
        {
            Debug.LogWarning("Player has no PlayerRhythmSession!");
            return;
        }

        waitingForConfirm = true;
        handled = false;

        rhythmSession.OnConfirmPressed -= HandleConfirm;
        rhythmSession.OnConfirmPressed += HandleConfirm;
    }

    private void HandleConfirm()
    {
        if (!waitingForConfirm) return;
        if (!isCooking) { UnsubscribeConfirm(); waitingForConfirm = false; return; }

        waitingForConfirm = false;
        UnsubscribeConfirm();

        handled = false;

        rhythmSession.OnSequenceSucceeded -= HandleSequenceSucceeded;
        rhythmSession.OnSequenceFailed -= HandleSequenceFailed;
        rhythmSession.OnSequenceSucceeded += HandleSequenceSucceeded;
        rhythmSession.OnSequenceFailed += HandleSequenceFailed;

        rhythmSession.StartSession();
    }

    private void HandleSequenceSucceeded()
    {
        if (handled) return;
        handled = true;
        Unsubscribe();

        cookingTimer -= successTimeBonus;
        if (cookingTimer <= 0f) FinishCooking();
    }

    private void HandleSequenceFailed()
    {
        if (handled) return;
        handled = true;
        Unsubscribe();

        cookingTimer += failTimePenalty;
    }

    private void Unsubscribe()
    {
        UnsubscribeConfirm();
        if (rhythmSession == null) return;
        rhythmSession.OnSequenceSucceeded -= HandleSequenceSucceeded;
        rhythmSession.OnSequenceFailed -= HandleSequenceFailed;
    }

    private void UnsubscribeConfirm()
    {
        if (rhythmSession == null) return;
        rhythmSession.OnConfirmPressed -= HandleConfirm;
    }


    private void FinishCooking()
    {
        isCooking = false;
        waitingForConfirm = false;  
        Unsubscribe();
        // Hide timer
        if (timerUI != null)
        {
            timerUI.SetActive(false);
        }

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