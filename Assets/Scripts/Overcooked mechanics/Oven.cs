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
    private GameObject finishedDish;
    private float cookingTimer;
    private bool isCooking;

    public void Interact(PlayerInteraction player)
    {
        // =========================================
        // 1. PICK UP FINISHED DISH
        // =========================================
        if (finishedDish != null && !player.IsHolding)
        {
            GameObject toPickup = finishedDish;
            finishedDish = null;
            toPickup.transform.SetParent(null, true);
            player.Pickup(toPickup);
            Debug.Log("Finished dish picked up!");
            return;
        }

        // =========================================
        // 2. START COOKING (Arms Rhythm Immediately)
        // =========================================
        if (!isCooking && finishedDish == null)
        {
            if (!player.IsHolding)
                return;

            Plate plate = player.HeldObject.GetComponent<Plate>();
            if (plate == null)
            {
                Debug.Log("You need a plate to cook!");
                return;
            }

            if (!plate.HasAllIngredients())
            {
                Debug.Log("The pizza is missing ingredients!");
                return;
            }

            // Take plate from player and place in oven
            plateCooking = player.RemoveHeldObject();
            plateCooking.transform.SetParent(transform);
            plateCooking.transform.localPosition = Vector3.zero;
            plateCooking.transform.localRotation = Quaternion.identity;

            // Start cooking timer
            cookingTimer = cookingTime;
            isCooking = true;

            if (timerUI != null)
                timerUI.SetActive(true);

            if (timerText != null)
                timerText.text = Mathf.Ceil(cookingTimer).ToString();

            // Arm the rhythm session immediately upon placing (matches CuttingBoard pattern)
            StartRhythmGame(player.GetComponent<PlayerRhythmSession>());
            Debug.Log("Pizza is cooking and rhythm is armed!");
            return;
        }

        // =========================================
        // 3. RHYTHM SYSTEM CONFIRMATION
        // =========================================
        if (isCooking && waitingForConfirm)
        {
            // If player interacts again while waiting, ensure session is bound to current player
            ArmRhythm(player);
        }
    }

    private void Update()
    {
        if (!isCooking)
            return;

        cookingTimer -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(cookingTimer).ToString();
        }

        if (cookingTimer <= 0f)
        {
            FinishCooking();
        }
    }

    private void StartRhythmGame(PlayerRhythmSession session)
    {
        rhythmSession = session;
        handled = false;

        if (rhythmSession == null)
        {
            Debug.LogWarning("Player has no PlayerRhythmSession!", this);
            return;
        }

        waitingForConfirm = true;

        rhythmSession.OnConfirmPressed -= HandleConfirm;
        rhythmSession.OnConfirmPressed += HandleConfirm;
    }

    private void ArmRhythm(PlayerInteraction player)
    {
        rhythmSession = player.GetComponent<PlayerRhythmSession>();
        if (rhythmSession == null)
        {
            Debug.LogWarning("Player has no PlayerRhythmSession!", this);
            return;
        }

        if (!waitingForConfirm)
        {
            waitingForConfirm = true;
            handled = false;
            rhythmSession.OnConfirmPressed -= HandleConfirm;
            rhythmSession.OnConfirmPressed += HandleConfirm;
        }
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
        if (cookingTimer <= 0f)
        {
            FinishCooking();
        }
        else
        {
            // Re-arm for subsequent rhythm checks if needed
            waitingForConfirm = false;
        }
    }

    private void HandleSequenceFailed()
    {
        if (handled) return;
        handled = true;
        Unsubscribe();

        cookingTimer += failTimePenalty;
        waitingForConfirm = false;
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

        // Spawn final dish and hold reference for pickup
        if (finalDishPrefab != null && dishSpawnPoint != null)
        {
            finishedDish = Instantiate(
                finalDishPrefab,
                dishSpawnPoint.position,
                finalDishPrefab.transform.rotation
            );
            finishedDish.transform.SetParent(transform);
        }
        else
        {
            Debug.LogWarning("No final dish prefab or spawn point assigned!", this);
        }
    }
}