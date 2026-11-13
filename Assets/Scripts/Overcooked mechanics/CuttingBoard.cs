using UnityEngine;

public class ChoppingBoard : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform ingredientPoint;
    [SerializeField] private PlayerRhythmSession rhythmSession;

    // Fix 1: Added a maximum distance to prevent remote-starting boards
    [SerializeField] private float maxInteractDistance = 2.5f;

    private PlayerInteraction waitingPlayer;
    private GameObject ingredient;
    private bool waitingForStart;
    private bool handled;

    public void Interact(PlayerInteraction player)
    {
        if (ingredient == null || ingredient.transform.parent != ingredientPoint)
        {
            ingredient = ingredientPoint.childCount > 0
                ? ingredientPoint.GetChild(0).gameObject
                : null;
        }

        // 1. Placing an item on an empty board
        if (player.IsHolding && ingredient == null)
        {
            ingredient = player.PlaceHeldObject(ingredientPoint);
            if (ingredient != null)
            {
                ingredient.transform.SetParent(ingredientPoint, true);
                StartRhythmGame(player.GetComponent<PlayerRhythmSession>());
                waitingPlayer = player;
                waitingForStart = true;
            }
            return;
        }

        // 2. Interacting with a board that already has an item
        if (!player.IsHolding && ingredient != null && waitingForStart)
        {
            // Fix 2: Changed from false to true so HandleConfirm doesn't get blocked
            waitingForStart = true;
            StartRhythmGame(player.GetComponent<PlayerRhythmSession>());
            waitingPlayer = player;
            return;
        }

        // 3. Picking up the item
        if (!player.IsHolding && ingredient != null)
        {
            // Fix 3: Unsubscribe so the board stops listening to this player
            UnsubscribeConfirm();

            GameObject toPickup = ingredient;
            ingredient = null;
            waitingForStart = false;
            waitingPlayer = null;
            toPickup.transform.SetParent(null, true);
            player.Pickup(toPickup);
        }
    }

    private void StartRhythmGame(PlayerRhythmSession session)
    {
        rhythmSession = session;
        handled = false;

        if (rhythmSession == null)
        {
            Debug.LogWarning("Player has no PlayerRhythmSession!");
            return;
        }

        // Unsubscribe first to prevent double-subscriptions if the player spams interact
        UnsubscribeConfirm();
        rhythmSession.OnConfirmPressed += HandleConfirm;
    }

    private void HandleSequenceSucceeded()
    {
        if (handled) return;
        handled = true;
        Unsubscribe();
        ReplaceWithChoppedIngredient();
    }

    private void HandleConfirm()
    {
        if (!waitingForStart) return;
        if (ingredient == null) return;

        // Fix 1 (Continued): Ensure the player who pressed confirm is actually standing at THIS board.
        if (waitingPlayer != null && Vector3.Distance(transform.position, waitingPlayer.transform.position) > maxInteractDistance)
        {
            return; // Player walked away, ignore the confirm button
        }

        waitingForStart = false;
        UnsubscribeConfirm();

        rhythmSession.OnSequenceSucceeded -= HandleSequenceSucceeded;
        rhythmSession.OnSequenceFailed -= HandleSequenceFailed;
        rhythmSession.OnSequenceSucceeded += HandleSequenceSucceeded;
        rhythmSession.OnSequenceFailed += HandleSequenceFailed;

        Ingredient ingScript = ingredient.GetComponent<Ingredient>();
        if (ingScript != null)
        {
            rhythmSession.debugPatternLength = ingScript.RequiredPatternLength;
        }

        rhythmSession.StartSession();
    }

    private void UnsubscribeConfirm()
    {
        if (rhythmSession == null) return;
        rhythmSession.OnConfirmPressed -= HandleConfirm;
    }

    private void HandleSequenceFailed()
    {
        if (handled) return;
        handled = true;
        Unsubscribe();
        Destroy(ingredient);
    }

    private void Unsubscribe()
    {
        if (rhythmSession == null) return;
        rhythmSession.OnSequenceSucceeded -= HandleSequenceSucceeded;
        rhythmSession.OnSequenceFailed -= HandleSequenceFailed;
    }

    private void ReplaceWithChoppedIngredient()
    {
        Ingredient ingredientScript = ingredient.GetComponent<Ingredient>();

        if (ingredientScript == null)
        {
            Debug.LogWarning("Ingredient does not have an Ingredient script!");
            return;
        }

        GameObject choppedPrefab = ingredientScript.ChoppedVersion;

        if (choppedPrefab == null)
        {
            Debug.LogWarning("No chopped version assigned!");
            return;
        }

        Destroy(ingredient);

        ingredient = Instantiate(
            choppedPrefab,
            ingredientPoint.position,
            choppedPrefab.transform.rotation,
            ingredientPoint
        );

        Debug.Log("Ingredient chopped!");
    }
}