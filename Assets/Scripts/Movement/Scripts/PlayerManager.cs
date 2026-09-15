using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerInput player1;
    [SerializeField] private PlayerInput player2;

    [SerializeField] private PlayerRhythmSession session1;
    [SerializeField] private PlayerRhythmSession session2;

    private void Start()
    {
        AssignPlayers();
    }

    private void AssignPlayers()
    {
        if (Gamepad.all.Count < 2)
        {
            Debug.LogWarning("Two gamepads are required.");
            return;
        }

        AssignGamepad(player1, Gamepad.all[0]);
        AssignGamepad(player2, Gamepad.all[1]);
        session1?.SetGamepad(Gamepad.all[0]);
        session2?.SetGamepad(Gamepad.all[1]);
    }

    private void AssignGamepad(PlayerInput player, Gamepad gamepad)
    {
        if (player == null)
        {
            Debug.LogError("PlayerInput reference is missing.");
            return;
        }

        if (gamepad == null)
        {
            Debug.LogError("Gamepad reference is missing.");
            return;
        }

        player.SwitchCurrentControlScheme(gamepad);

        Debug.Log(
            $"{player.name} assigned to {gamepad.displayName}"
        );
    }

    
}