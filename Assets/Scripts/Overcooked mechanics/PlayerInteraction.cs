using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;

    private IInteractable currentInteractable;
    private GameObject heldObject;

    public bool IsHolding => heldObject != null;
    public int PlayerId { get; set; }
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

        // If we're holding something and there's no station to interact with,
        // drop it.
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

    public void SetInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    public void Pickup(GameObject objectToPickup)
    {
        heldObject = objectToPickup;

        objectToPickup.transform.SetParent(holdPoint);
        objectToPickup.transform.localPosition = Vector3.zero;
        objectToPickup.transform.localRotation = Quaternion.identity;
    }

    public GameObject PlaceHeldObject(Transform placementPoint)
    {
        if (heldObject == null)
            return null;

        GameObject objectToPlace = heldObject;

        heldObject = null;

        objectToPlace.transform.SetParent(placementPoint);
        objectToPlace.transform.localPosition = Vector3.zero;
        objectToPlace.transform.localRotation = Quaternion.identity;

        return objectToPlace;
    }

    private void Drop()
    {
        heldObject.transform.SetParent(null);

        heldObject.transform.position =
            transform.position + transform.forward;

        heldObject = null;
    }

    public GameObject RemoveHeldObject()
    {
        GameObject objectToRemove = heldObject;

        heldObject = null;

        return objectToRemove;
    }
}