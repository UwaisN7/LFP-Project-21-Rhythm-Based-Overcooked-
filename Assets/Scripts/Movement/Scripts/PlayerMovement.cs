using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundedStickForce = -2f;

    [Header("Visuals")]
    public Color[] playerColors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow };
    public int colorIndex = 0;

   

    private PlayerInput playerInput;
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        ApplyPlayerColor();
       
    }

    private void ApplyPlayerColor()
    {
        Renderer rend = GetComponentInChildren<Renderer>();

        if (rend == null || playerColors.Length == 0)
            return;

        rend.material.color = playerColors[colorIndex % playerColors.Length];
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);

        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = groundedStickForce;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        controller.Move(verticalVelocity * Time.deltaTime);
    }

   
}