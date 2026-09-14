using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


// Attach this to your player prefab, alongside a CharacterController
// and a PlayerInput component (set PlayerInput's Behavior to "Send Messages").
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

    [Header("Input")]
    public int gamepadIndex = 0;

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
        Invoke(nameof(AssignGamepad), 0.1f);
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

    private void AssignGamepad()
    {
        if (playerInput == null)
        {
            Debug.LogError($"{name}: PlayerInput component is missing!");
            return;
        }

        if (Gamepad.all.Count <= gamepadIndex)
        {
            Debug.LogWarning(
                $"{name}: No gamepad found at index {gamepadIndex}. " +
                $"Connected gamepads: {Gamepad.all.Count}"
            );

            return;
        }

        Gamepad pad = Gamepad.all[gamepadIndex];

       
        if (!playerInput.user.valid)
        {
            Debug.LogWarning($"{name}: PlayerInput user is not valid yet.");
            return;
        }

       
        playerInput.user.UnpairDevices();

        
        InputUser.PerformPairingWithDevice(
            pad,
            playerInput.user
        );

        Debug.Log(
            $"{name} successfully assigned to Gamepad {gamepadIndex}: " +
            $"{pad.displayName}"
        );
    }
}