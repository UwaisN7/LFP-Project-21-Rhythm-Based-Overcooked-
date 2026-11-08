
using TMPro;
using UnityEngine;

public class Oven : MonoBehaviour, IInteractable
{
    [Header("Cooking")]
    [SerializeField] private float cookingTime = 15f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject timerUI;

    [Header("Recipe System")]
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private Transform dishSpawnPoint;

    [Header("Rhythm")]
    [SerializeField] private float successTimeBonus = 10f;
    [SerializeField] private float failTimePenalty = 0f;

    private PlayerRhythmSession rhythmSession;
    private bool waitingForConfirm;
    private bool handled;

    private GameObject plateCooking;
    private GameObject finishedDish;
    private GameObject resultPizzaPrefab;

    private float cookingTimer;
    private bool isCooking;

    public void Interact(PlayerInteraction player)
    {
        // 1. PICK UP FINISHED DISH
        if (finishedDish != null && !player.IsHolding)
        {
            GameObject toPickup = finishedDish;
            finishedDish = null;

            toPickup.transform.SetParent(null, true);
            player.Pickup(toPickup);

            Debug.Log("Finished dish picked up!");
            return;
        }

        // 2. START COOKING
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

            if (!plate.HasIngredients())
            {
                Debug.Log("Your plate has no ingredients!");
                return;
            }

            if (recipeManager == null)
            {
                Debug.LogError("RecipeManager is missing!", this);
                return;
            }

            // Determine which pizza will be produced
            resultPizzaPrefab = recipeManager.GetPizzaPrefab(plate);

            if (resultPizzaPrefab == null)
            {
                Debug.LogError("No pizza prefab was returned!", this);
                return;
            }

            // Take plate from player and place in oven
            plateCooking = player.RemoveHeldObject();

            plateCooking.transform.SetParent(transform);
            plateCooking.transform.localPosition = Vector3.zero;
            plateCooking.transform.localRotation = Quaternion.identity;

            // Start timer
            cookingTimer = cookingTime;
            isCooking = true;

            if (timerUI != null)
                timerUI.SetActive(true);

            if (timerText != null)
                timerText.text = Mathf.Ceil(cookingTimer).ToString();

            StartRhythmGame(player.GetComponent<PlayerRhythmSession>());

            Debug.Log("Pizza is cooking!");
            return;
        }

        // 3. RHYTHM CONFIRMATION
        if (isCooking && waitingForConfirm)
        {
            ArmRhythm(player);
        }
    }

    private void Update()
    {
        if (!isCooking)
            return;

        cookingTimer -= Time.deltaTime;

        if (timerText != null)
            timerText.text = Mathf.Ceil(cookingTimer).ToString();

        if (cookingTimer <= 0f)
            FinishCooking();
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

        if (!isCooking)
        {
            UnsubscribeConfirm();
            waitingForConfirm = false;
            return;
        }

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
            FinishCooking();
        else
            waitingForConfirm = false;
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
        if (!isCooking) return;

        isCooking = false;
        waitingForConfirm = false;

        Unsubscribe();

        if (timerUI != null)
            timerUI.SetActive(false);

        Debug.Log("Pizza finished cooking!");

        // Remove uncooked plate
        if (plateCooking != null)
        {
            Destroy(plateCooking);
            plateCooking = null;
        }

        // Spawn correct pizza or failed pizza
        finishedDish = Instantiate(
            resultPizzaPrefab,
            dishSpawnPoint.position,
            resultPizzaPrefab.transform.rotation
        );

        finishedDish.transform.SetParent(transform);
        resultPizzaPrefab = null;
    }
}
