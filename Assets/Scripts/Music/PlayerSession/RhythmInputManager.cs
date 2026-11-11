using UnityEngine;
using UnityEngine.InputSystem;

public class RhythmInputManager : MonoBehaviour
{
    private Gamepad targetGamepad;
    [SerializeField] private MusicClock musicClock;

    // Optional: Only allow keyboard testing if no gamepad is assigned
    [SerializeField] private bool allowKeyboardFallback = true;

    public event System.Action<RhythmDirection, double> OnDirectionPressed;

    private void Awake()
    {
        musicClock = musicClock != null ? musicClock : FindAnyObjectByType<MusicClock>();
    }

    private void Update()
    {
        if (musicClock == null) return;
        double songTime = musicClock.SongTime;

        // If a gamepad is assigned, strictly use ONLY that gamepad
        if (targetGamepad != null)
        {
            if (targetGamepad.dpad.up.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Up, songTime);
            if (targetGamepad.dpad.down.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Down, songTime);
            if (targetGamepad.dpad.left.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Left, songTime);
            if (targetGamepad.dpad.right.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Right, songTime);
            return; // Prevent keyboard inputs from mixing with controller inputs
        }

        // Fallback to keyboard ONLY if no gamepad is assigned to this player
        if (allowKeyboardFallback)
        {
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Up, songTime);
                if (kb.sKey.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Down, songTime);
                if (kb.aKey.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Left, songTime);
                if (kb.dKey.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Right, songTime);
            }
        }
    }

    public void SetGamepad(Gamepad pad)
    {
        targetGamepad = pad;
    }
}