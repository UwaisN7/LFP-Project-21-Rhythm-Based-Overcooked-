using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads D-Pad input for one player using the new Input System
/// and raises an event with which direction was pressed and *when*,
/// stamped in SongTime so it's directly comparable to AnswerPrompt targets.
/// Starting with D-Pad only, as agreed - buttons can be added the same way later.
/// </summary>
public class RhythmInputManager : MonoBehaviour
{
   
    [SerializeField] private Gamepad targetGamepad;

  
    [SerializeField] private MusicClock musicClock;

    public event System.Action<RhythmDirection, double> OnDirectionPressed;

    private void Update()
    {
        Gamepad pad = targetGamepad != null ? targetGamepad : Gamepad.current;
        if (pad == null || musicClock == null) return;

        double songTime = musicClock.SongTime;

        if (pad.dpad.up.wasPressedThisFrame)    OnDirectionPressed?.Invoke(RhythmDirection.Up, songTime);
        if (pad.dpad.down.wasPressedThisFrame)  OnDirectionPressed?.Invoke(RhythmDirection.Down, songTime);
        if (pad.dpad.left.wasPressedThisFrame)  OnDirectionPressed?.Invoke(RhythmDirection.Left, songTime);
        if (pad.dpad.right.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Right, songTime);
    }
}
