using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users; // Required for deep InputUser access

public class PlayerManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerInput player1;
    [SerializeField] private PlayerInput player2;

    [SerializeField] private PlayerRhythmSession session1;
    [SerializeField] private PlayerRhythmSession session2;

    public float player1FinalScore;
    public float player2FinalScore;

    [Header("UI Events (FIFA Menu Skeleton)")]
    public Action<Gamepad> OnControllerConnected;
    public Action<Gamepad> OnControllerDisconnected;

    private Gamepad pad1;
    private Gamepad pad2;

    private void OnEnable()
    {
        // 1. Force Unity to stop auto-switching devices on us
        if (player1 != null) player1.neverAutoSwitchControlSchemes = true;
        if (player2 != null) player2.neverAutoSwitchControlSchemes = true;

        InputSystem.onDeviceChange += HandleDeviceChange;

        // Scan for currently connected controllers
        foreach (var gamepad in Gamepad.all)
        {
            ConnectDevice(gamepad);
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= HandleDeviceChange;
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
            AssignGamepad(player1, session1, pad1);
        }
        else if (pad2 == null && pad1 != gamepad)
        {
            pad2 = gamepad;
            AssignGamepad(player2, session2, pad2);
        }
    }

    private void DisconnectDevice(Gamepad gamepad)
    {
        OnControllerDisconnected?.Invoke(gamepad);

        if (pad1 == gamepad)
        {
            pad1 = null;
            ClearGamepad(player1, session1);
        }
        else if (pad2 == gamepad)
        {
            pad2 = null;
            ClearGamepad(player2, session2);
        }
    }

    private void AssignGamepad(PlayerInput player, PlayerRhythmSession session, Gamepad gamepad)
    {
        if (player != null && gamepad != null)
        {
            // THE FIX: Bypass the high-level wrapper and hard-lock the device
            player.user.UnpairDevices(); // Strip away Unity's chaotic ghost-assignments
            InputUser.PerformPairingWithDevice(gamepad, player.user); // Hard-assign the exact pad
            player.SwitchCurrentControlScheme(gamepad); // Update the visual scheme

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