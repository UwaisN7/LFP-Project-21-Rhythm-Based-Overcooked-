using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform holdPoint;

    private IInteractable currentInteractable;
    private GameObject heldObject;

    public bool IsHolding => heldObject != null;
    public int PlayerId;

    private Outline currentOutline;

    // --- STEAL MECHANIC STATES ---
    public enum StealState { None, Waiting, Passed, Failed }
    public StealState currentStealState = StealState.None;

    private PlayerMovement playerMovement;
    private PlayerRhythmSession rhythmSession;
    private float stealCooldown = 0f;
    private PlayerInteraction currentStealOpponent;
    private bool isInStealBattle = false;
    // -------------------------------------

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rhythmSession = GetComponent<PlayerRhythmSession>();
    }

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

    public void Interact(PlayerInteraction interactor)
    {
        if (IsHolding && !interactor.IsHolding && stealCooldown <= 0f && interactor.stealCooldown <= 0f)
        {
            InitiateStealBattle(interactor);
        }
    }

    private void InitiateStealBattle(PlayerInteraction opponent)
    {
        isInStealBattle = true;
        currentStealState = StealState.Waiting;
        currentStealOpponent = opponent;

        opponent.isInStealBattle = true;
        opponent.currentStealState = StealState.Waiting;
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
        if (!isInStealBattle) return;
        currentStealState = StealState.Passed;
        CheckBattleEnd();
    }

    private void HandleStealLoss()
    {
        if (!isInStealBattle) return;
        currentStealState = StealState.Failed;
        CheckBattleEnd();
    }

    private void CheckBattleEnd()
    {
        // 1. If either player is still playing, do nothing and wait.
        if (currentStealOpponent == null || currentStealState == StealState.Waiting || currentStealOpponent.currentStealState == StealState.Waiting)
        {
            return;
        }

        // Failsafe: If somehow neither is holding the object anymore, just reset both to avoid a permanent freeze.
        if (!IsHolding && !currentStealOpponent.IsHolding)
        {
            ResetFromStealBattle();
            currentStealOpponent?.ResetFromStealBattle();
            return;
        }

        // 2. Both players are done! ONLY the defender (holding the item) processes the result.
        if (IsHolding)
        {
            PlayerInteraction winner = null;
            PlayerInteraction loser = null;

            if (this.currentStealState == StealState.Passed && currentStealOpponent.currentStealState == StealState.Failed)
            {
                winner = this;
                loser = currentStealOpponent;
            }
            else if (this.currentStealState == StealState.Failed && currentStealOpponent.currentStealState == StealState.Passed)
            {
                winner = currentStealOpponent;
                loser = this;
            }
            else
            {
                // Tie -> Defender keeps it
                winner = this;
                loser = currentStealOpponent;
            }

            ExecuteBattleOutcome(winner, loser);
        }
        else
        {
            // 3. THE FIX: If the challenger finishes LAST, they tell the defender to process the outcome!
            currentStealOpponent.CheckBattleEnd();
        }
    }

    private void ExecuteBattleOutcome(PlayerInteraction winner, PlayerInteraction loser)
    {
        if (loser.IsHolding)
        {
            GameObject stolenItem = loser.RemoveHeldObject();
            winner.Pickup(stolenItem);
        }

        loser.playerMovement?.PushAway(winner.transform.position, 6f);

        winner.ResetFromStealBattle();
        loser.ResetFromStealBattle();
    }

    public void ResetFromStealBattle()
    {
        isInStealBattle = false;
        currentStealState = StealState.None;
        currentStealOpponent = null;
        stealCooldown = 5f;

        playerMovement?.FreezeMovement(false);
    }

    public GameObject HeldObject
    {
        get { return heldObject; }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        // Bypasses layer mask issues by actively checking for nearby players when empty-handed.
        if (heldObject == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
            foreach (Collider hit in hits)
            {
                PlayerInteraction otherPlayer = hit.GetComponentInParent<PlayerInteraction>();

                if (otherPlayer != null && otherPlayer != this && otherPlayer.IsHolding)
                {
                    otherPlayer.Interact(this);
                    return;
                }
            }
        }

        if (heldObject != null && currentInteractable != null)
        {
            Component component = currentInteractable as Component;
            if (component != null && component.gameObject.layer != LayerMask.NameToLayer("Ingredient"))
            {
                currentInteractable.Interact(this);
                return;
            }
        }

        if (heldObject == null && currentInteractable != null)
        {
            currentInteractable.Interact(this);
            return;
        }

        if (heldObject != null)
        {
            Drop();
            return;
        }

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
                if (currentOutline != null) currentOutline.enabled = true;
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