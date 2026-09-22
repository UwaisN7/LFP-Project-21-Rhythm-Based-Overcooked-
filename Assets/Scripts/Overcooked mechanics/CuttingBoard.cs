using UnityEngine;

public class ChoppingBoard : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform ingredientPoint;
    [SerializeField] private PlayerRhythmSession rhythmSession;
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
        if (player.IsHolding && ingredient == null)
        {
            ingredient = player.PlaceHeldObject(ingredientPoint);
            if (ingredient != null)
            {
                ingredient.transform.SetParent(ingredientPoint, true); 
                StartRhythmGame(player.GetComponent<PlayerRhythmSession>());
                waitingPlayer = player;
                waitingForStart=true;
            }
            return;
        }
        if (!player.IsHolding && ingredient != null && waitingForStart)
        {
            waitingForStart = false;
            StartRhythmGame(player.GetComponent<PlayerRhythmSession>());
            waitingPlayer = player;
            return;
        }
        if (!player.IsHolding && ingredient != null)
        {
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

        // Subscribe to confirm — this is what actually starts the run.
        rhythmSession.OnConfirmPressed -= HandleConfirm;
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

        waitingForStart = false;
        UnsubscribeConfirm();

        
        rhythmSession.OnSequenceSucceeded -= HandleSequenceSucceeded;
        rhythmSession.OnSequenceFailed -= HandleSequenceFailed;
        rhythmSession.OnSequenceSucceeded += HandleSequenceSucceeded;
        rhythmSession.OnSequenceFailed += HandleSequenceFailed;

        rhythmSession.StartSession();
    }

    private void UnsubscribeConfirm()
    {
        if (rhythmSession == null) return;
        rhythmSession.OnConfirmPressed -= HandleConfirm;
    }
    private void HandleSequenceFailed()  //They fail they lose hahahahahhahahahahahahahaahaghgahahahahahahsahauyjbiysbvihwbsyvibjewrvsby
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

        // Create chopped ingredient
        ingredient = Instantiate(
            choppedPrefab,
            ingredientPoint.position,
            ingredientPoint.rotation,
            ingredientPoint
        );

        Debug.Log("Ingredient chopped!");
    }
    
}