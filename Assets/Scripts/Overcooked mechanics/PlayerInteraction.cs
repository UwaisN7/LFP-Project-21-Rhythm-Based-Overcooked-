using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// --- MODIFIED: Added IInteractable so players can interact with each other ---
public class PlayerInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform holdPoint;

    private IInteractable currentInteractable;
    private GameObject heldObject;

    public bool IsHolding => heldObject != null;
    public int PlayerId;

    private HashSet<IInteractable> nearbyInteractables =
    new HashSet<IInteractable>();

    private Outline currentOutline;

    // --- NEW: STEAL MECHANIC ADDITIONS ---
    private PlayerMovement playerMovement;
    private PlayerRhythmSession rhythmSession;
    private float stealCooldown = 0f;
    private PlayerInteraction currentStealOpponent;
    private bool isInStealBattle = false;
    // -------------------------------------

    private void Awake()
    {
        Debug.Log($"[PlayerInteraction] PlayerId set to {PlayerId}");

        // --- NEW: STEAL MECHANIC ADDITIONS ---
        playerMovement = GetComponent<PlayerMovement>();
        rhythmSession = GetComponent<PlayerRhythmSession>();
        // -------------------------------------
    }

    // --- NEW: STEAL MECHANIC ADDITIONS ---
    private void OnEnable()
    {
        if (rhythmSession != null)
        {
            rhythmSession.OnSequenceSucceeded += HandleStealWin;
            rhythmSession.OnSequenceFailed += HandleStealLoss;
        }
    }

    private void OnDisable()
    {
        if (rhythmSession != null)
        {
            rhythmSession.OnSequenceSucceeded -= HandleStealWin;
            rhythmSession.OnSequenceFailed -= HandleStealLoss;
        }
    }

    private void Update()
    {
        if (stealCooldown > 0f)
        {
            stealCooldown -= Time.deltaTime;
        }
    }

    // This makes the player an interactable target for other players!
    public void Interact(PlayerInteraction interactor)
    {
        // If I am holding something, the interactor isn't, and neither is on cooldown
        if (IsHolding && !interactor.IsHolding && stealCooldown <= 0f && interactor.stealCooldown <= 0f)
        {
            InitiateStealBattle(interactor);
        }
    }

    private void InitiateStealBattle(PlayerInteraction opponent)
    {
        Debug.Log($"Player {opponent.PlayerId} is attempting to steal from Player {PlayerId}!");

        isInStealBattle = true;
        opponent.isInStealBattle = true;
        currentStealOpponent = opponent;
        opponent.currentStealOpponent = this;

        // Freeze both players
        playerMovement?.FreezeMovement(true);
        opponent.playerMovement?.FreezeMovement(true);

        // Start 10-pattern rhythm session for both
        rhythmSession?.StartStealSession();
        opponent.rhythmSession?.StartStealSession();
    }

    private void HandleStealWin()
    {
        if (!isInStealBattle) return; // Ignore if this was a normal cooking rhythm win

        Debug.Log($"Player {PlayerId} WON the steal battle!");
        EndStealBattle(this, currentStealOpponent);
    }

    private void HandleStealLoss()
    {
        if (!isInStealBattle) return; // Ignore normal failures

        Debug.Log($"Player {PlayerId} LOST the steal battle!");
        // If I lost, the opponent wins
        EndStealBattle(currentStealOpponent, this);
    }

    private void EndStealBattle(PlayerInteraction winner, PlayerInteraction loser)
    {
        if (winner == null || loser == null) return;

        // Transfer the item to the winner
        if (loser.IsHolding)
        {
            GameObject stolenItem = loser.RemoveHeldObject();
            winner.Pickup(stolenItem);
        }

        // Apply knockback to the loser (pushes them away from the winner)
        loser.playerMovement?.PushAway(winner.transform.position, 4f);

        // Reset states, apply 5 second cooldown, unfreeze
        winner.ResetFromStealBattle();
        loser.ResetFromStealBattle();
    }

    private void ResetFromStealBattle()
    {
        isInStealBattle = false;
        currentStealOpponent = null;
        stealCooldown = 5f; // 5 Second Cooldown!

        playerMovement?.FreezeMovement(false);
        rhythmSession?.ForceCancelSession(); // Force cancel in case the other finished first

        // --- NEW: Nuke the remaining rhythm logic and arrows ---
        GetComponentInChildren<RhythmEvaluator>()?.NukeSession();
        GetComponentInChildren<RhythmLaneUI>()?.NukeSession();
    }
    // -------------------------------------

    public GameObject HeldObject
    {
        get { return heldObject; }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (currentInteractable != null)
        {
            // Remember what we were holding before interacting.
            GameObject objectBeforeInteraction = heldObject;

            currentInteractable.Interact(this);

            // If the interaction didn't use, place, or change
            // the held object, allow the player to drop it.
            if (heldObject == objectBeforeInteraction && heldObject != null)
            {
                Drop();
            }

            return;
        }

        if (heldObject != null)
        {
            Drop();
        }
    }

    public void SetInteractable(IInteractable newInteractable)
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }

        currentInteractable = newInteractable;

        if (currentInteractable != null)
        {
            MonoBehaviour interactableObject = currentInteractable as MonoBehaviour;

            if (interactableObject != null)
            {
                currentOutline = interactableObject.GetComponentInParent<Outline>();
                if (currentOutline != null)
                {
                    currentOutline.enabled = true;
                }
            }
        }
    }

    public void Pickup(GameObject objectToPickup)
    {
        heldObject = objectToPickup;
        objectToPickup.transform.SetParent(holdPoint);
        objectToPickup.transform.localPosition = Vector3.zero;

        RefreshBestInteractable();
    }

    public GameObject PlaceHeldObject(Transform placementPoint)
    {
        if (heldObject == null) return null;
        GameObject objectToPlace = heldObject;
        heldObject = null;
        objectToPlace.transform.SetParent(placementPoint);
        objectToPlace.transform.localPosition = Vector3.zero;

        RefreshBestInteractable();
        return objectToPlace;

     
    }

    private void Drop()
    {
        if (heldObject == null)
            return;

        GameObject objectToDrop = heldObject;

        heldObject = null;

        objectToDrop.transform.SetParent(null);
        objectToDrop.transform.position = transform.position + transform.forward;

        RefreshBestInteractable();
    }

    public GameObject RemoveHeldObject()
    {
        GameObject objectToRemove = heldObject;
        heldObject = null;

        RefreshBestInteractable();

        return objectToRemove;
    }

    public void RegisterInteractable(IInteractable interactable)
    {
        if (interactable == null)
            return;

        nearbyInteractables.Add(interactable);
        RefreshBestInteractable();
    }

    public void UnregisterInteractable(IInteractable interactable)
    {
        if (interactable == null)
            return;

        nearbyInteractables.Remove(interactable);
        RefreshBestInteractable();
    }

    private void RefreshBestInteractable()
    {
        IInteractable bestInteractable = null;
        int bestPriority = int.MaxValue;
        float closestDistance = float.MaxValue;

        foreach (IInteractable interactable in nearbyInteractables)
        {
            int priority = GetInteractionPriority(interactable);

            if (priority == int.MaxValue)
                continue;

            Component component = interactable as Component;

            if (component == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                component.transform.position
            );

            if (priority < bestPriority ||
                (priority == bestPriority && distance < closestDistance))
            {
                bestPriority = priority;
                closestDistance = distance;
                bestInteractable = interactable;
            }
        }

        SetInteractable(bestInteractable);
    }
    private int GetInteractionPriority(IInteractable interactable)
    {
        // Check what the player is holding.
        Plate heldPlate = heldObject != null
            ? heldObject.GetComponent<Plate>()
            : null;

        Ingredient heldIngredient = heldObject != null
            ? heldObject.GetComponent<Ingredient>()
            : null;

        FinalDish heldDish = heldObject != null
            ? heldObject.GetComponent<FinalDish>()
            : null;


        // ==========================================
        // PRIORITIES WHEN HOLDING NOTHING
        // ==========================================
        if (!IsHolding)
        {
            // Finished pizza at the oven.
            if (interactable is Oven oven && oven.HasFinishedDish)
                return 1;

            // Chopped ingredient at the chopping board.
            if (interactable is ChoppingBoard board &&
                board.HasChoppedIngredient)
                return 1;

            // Counter containing a plate.
            if (interactable is Counter counter && counter.HasPlate)
                return 2;

            // Plate box.
            if (interactable is PlateBox)
                return 3;

            // Ingredient box.
            if (interactable is IngredientBox)
                return 4;

            // Raw ingredient waiting to be chopped.
            if (interactable is ChoppingBoard rawBoard &&
                rawBoard.HasIngredient)
                return 5;

            // Loose ingredient on the floor.
            if (interactable is Ingredient)
                return 6;
        }


        // ==========================================
        // PRIORITIES WHEN HOLDING AN EMPTY PLATE
        // ==========================================
        if (heldPlate != null && heldPlate.IsEmpty)
        {
            // Prefer an empty counter.
            if (interactable is Counter emptyCounter &&
                emptyCounter.IsEmpty)
                return 1;
        }


        // ==========================================
        // PRIORITIES WHEN HOLDING A RAW INGREDIENT
        // ==========================================
        if (heldIngredient != null &&
            heldIngredient.State == IngredientState.Raw)
        {
            // Prefer an available chopping board.
            if (interactable is ChoppingBoard availableBoard &&
                availableBoard.CanAcceptIngredient)
                return 1;
        }


        // ==========================================
        // PRIORITIES WHEN HOLDING A CHOPPED INGREDIENT
        // ==========================================
        if (heldIngredient != null &&
            heldIngredient.State == IngredientState.Chopped)
        {
            // Prefer a counter with a plate.
            if (interactable is Counter plateCounter &&
                plateCounter.HasPlate)
                return 1;
        }


        // ==========================================
        // PRIORITIES WHEN HOLDING A PLATE WITH INGREDIENTS
        // ==========================================
        if (heldPlate != null && heldPlate.HasUncookedPizza)
        {
            // Prefer an available oven.
            if (interactable is Oven availableOven &&
                availableOven.CanAcceptPlate)
                return 1;
        }


        // ==========================================
        // PRIORITIES WHEN HOLDING A FINISHED PIZZA
        // ==========================================
        if (heldDish != null)
        {
            if (interactable is DeliveryStation)
                return 1;
        }


        // ==========================================
        // COUNTERS AS A FALLBACK
        // ==========================================
        // Counters remain interactable regardless of what
        // the player is holding, but have lower priority.
        if (interactable is Counter)
            return 10;


        // ==========================================
        // OTHER INTERACTABLES AS A FALLBACK
        // ==========================================
        return 100;
    }
}