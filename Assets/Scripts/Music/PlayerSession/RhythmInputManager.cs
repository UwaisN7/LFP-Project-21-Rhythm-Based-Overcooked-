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
    private void Awake()
    {
        musicClock = musicClock != null ? musicClock : FindAnyObjectByType<MusicClock>();
    }
    private void Update()
    {
        Keyboard kb = Keyboard.current;
        
        double songTime = musicClock.SongTime;
        if (kb != null)
        {
            if (kb.wKey.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Up, songTime);
            if (kb.sKey.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Down, songTime);
            if (kb.aKey.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Left, songTime);
            if (kb.dKey.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Right, songTime);
        }
        Gamepad pad = targetGamepad != null ? targetGamepad : Gamepad.current;
        if (pad == null || musicClock == null) return;

      

        if (pad.dpad.up.wasPressedThisFrame)    OnDirectionPressed?.Invoke(RhythmDirection.Up, songTime);
        if (pad.dpad.down.wasPressedThisFrame)  OnDirectionPressed?.Invoke(RhythmDirection.Down, songTime);
        if (pad.dpad.left.wasPressedThisFrame)  OnDirectionPressed?.Invoke(RhythmDirection.Left, songTime);
        if (pad.dpad.right.wasPressedThisFrame) OnDirectionPressed?.Invoke(RhythmDirection.Right, songTime);

    }


    public void SetGamepad(Gamepad pad)
    {
        targetGamepad = pad;
    }


}
