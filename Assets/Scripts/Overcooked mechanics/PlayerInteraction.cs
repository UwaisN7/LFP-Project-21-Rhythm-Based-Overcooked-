using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Holding")]
    [SerializeField] private Transform holdPoint;

    private IInteractable currentInteractable;
    private GameObject heldObject;

    public bool IsHolding => heldObject != null;

    public GameObject HeldObject => heldObject;

    public Transform HoldPoint => holdPoint;

    public void SetInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (currentInteractable == null)
            return;

        currentInteractable.Interact(this);
    }

    public bool TryPickup(GameObject objectToPickup)
    {
        if (IsHolding)
            return false;

        if (objectToPickup == null)
            return false;

        heldObject = objectToPickup;

        heldObject.transform.SetParent(holdPoint);

        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        Collider objectCollider = heldObject.GetComponent<Collider>();

        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        return true;
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        DropHeldObject();
    }

    public GameObject DropHeldObject()
    {
        if (!IsHolding)
            return null;

        GameObject droppedObject = heldObject;

        heldObject = null;

        droppedObject.transform.SetParent(null);

        droppedObject.transform.position =
            transform.position + transform.forward * 1f;

        Collider objectCollider = droppedObject.GetComponent<Collider>();

        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }

        Rigidbody rb = droppedObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        return droppedObject;
    }
}
