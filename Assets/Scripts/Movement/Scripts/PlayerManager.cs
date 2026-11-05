using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerInput player1;
    [SerializeField] private PlayerInput player2;

    [SerializeField] private PlayerRhythmSession session1;
    [SerializeField] private PlayerRhythmSession session2;

    [Header("UI Icons")]
    [SerializeField] private Image player1Icon;
    [SerializeField] private Image player2Icon;

    public float player1FinalScore;
    public float player2FinalScore;

    [Header("UI Events (FIFA Menu Skeleton)")]
    public Action<Gamepad> OnControllerConnected;
    public Action<Gamepad> OnControllerDisconnected;

    private Gamepad pad1;
    private Gamepad pad2;

    private Gamepad simulatedGamepad;
    private void OnEnable()
    {
        if (player1 != null) player1.neverAutoSwitchControlSchemes = true;
        if (player2 != null) player2.neverAutoSwitchControlSchemes = true;

        InputSystem.onDeviceChange += HandleDeviceChange;

        foreach (var gamepad in Gamepad.all)
        {
            ConnectDevice(gamepad);
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= HandleDeviceChange;
    }
    private void Update()
    {
        // Press 'C' to simulate connecting a controller
        if (Keyboard.current.cKey.wasPressedThisFrame && simulatedGamepad == null)
        {
            simulatedGamepad = InputSystem.AddDevice<Gamepad>();
        }

        // Press 'D' to simulate disconnecting the controller
        if (Keyboard.current.dKey.wasPressedThisFrame && simulatedGamepad != null)
        {
            InputSystem.RemoveDevice(simulatedGamepad);
            simulatedGamepad = null;
        }
    }
    private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad gamepad) return;

        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Reconnected:
                ConnectDevice(gamepad);
                break;
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Removed:
                DisconnectDevice(gamepad);
                break;
        }
    }

    private void ConnectDevice(Gamepad gamepad)
    {
        OnControllerConnected?.Invoke(gamepad);

        if (pad1 == null && pad2 != gamepad)
        {
            pad1 = gamepad;
            if (player1Icon != null) player1Icon.color = Color.green;
            AssignGamepad(player1, session1, pad1);
        }
        else if (pad2 == null && pad1 != gamepad)
        {
            pad2 = gamepad;
            if (player2Icon != null) player2Icon.color = Color.green;
            AssignGamepad(player2, session2, pad2);
        }
    }

    private void DisconnectDevice(Gamepad gamepad)
    {
        OnControllerDisconnected?.Invoke(gamepad);

        if (pad1 == gamepad)
        {
            pad1 = null;
            if (player1Icon != null) player1Icon.color = Color.red;
            ClearGamepad(player1, session1);
        }
        else if (pad2 == gamepad)
        {
            pad2 = null;
            if (player2Icon != null) player2Icon.color = Color.red;
            ClearGamepad(player2, session2);
        }
    }

    private void AssignGamepad(PlayerInput player, PlayerRhythmSession session, Gamepad gamepad)
    {
        if (player != null && gamepad != null)
        {
            player.user.UnpairDevices();
            InputUser.PerformPairingWithDevice(gamepad, player.user);
            player.SwitchCurrentControlScheme(gamepad);

            Debug.Log($"{player.name} strictly locked to {gamepad.displayName}");
        }

        if (session != null)
        {
            session.SetGamepad(gamepad);
        }
    }

    private void ClearGamepad(PlayerInput player, PlayerRhythmSession session)
    {
        Debug.LogWarning($"{player?.name ?? "A player"} lost their controller!");

        if (player != null)
        {
            player.user.UnpairDevices();
        }

        if (session != null)
        {
            session.SetGamepad(null);
        }
    }
}