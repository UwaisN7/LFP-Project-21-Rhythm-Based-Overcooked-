using UnityEngine;
using UnityEngine.InputSystem;

// --- MODIFIED: Added IInteractable so players can interact with each other ---
public class PlayerInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform holdPoint;

    private IInteractable currentInteractable;
    private GameObject heldObject;

    public bool IsHolding => heldObject != null;
    public int PlayerId;

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

        // If we are holding something and the current interactable
        // is NOT an ingredient, interact with it.
        if (heldObject != null && currentInteractable != null)
        {
            Component component = currentInteractable as Component;

            if (component != null && component.gameObject.layer != LayerMask.NameToLayer("Ingredient"))
            {
                currentInteractable.Interact(this);
                return;
            }
        }

        // --- MODIFIED: Ensure we check if the currentInteractable is another player first before dropping ---
        if (heldObject == null && currentInteractable != null)
        {
            currentInteractable.Interact(this);
            return;
        }

        // If we're holding something and there's no station (or player) to interact with, drop it.
        if (heldObject != null)
        {
            Drop();
            return;
        }

        // If we're not holding anything, interact normally.
        if (currentInteractable != null)
        {
            currentInteractable.Interact(this);
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
    }

    public GameObject PlaceHeldObject(Transform placementPoint)
    {
        if (heldObject == null) return null;
        GameObject objectToPlace = heldObject;
        heldObject = null;
        objectToPlace.transform.SetParent(placementPoint);
        objectToPlace.transform.localPosition = Vector3.zero;
        return objectToPlace;
    }

    private void Drop()
    {
        heldObject.transform.SetParent(null);
        heldObject.transform.position = transform.position + transform.forward;
        heldObject = null;
    }

    public GameObject RemoveHeldObject()
    {
        GameObject objectToRemove = heldObject;
        heldObject = null;
        return objectToRemove;
    }
}