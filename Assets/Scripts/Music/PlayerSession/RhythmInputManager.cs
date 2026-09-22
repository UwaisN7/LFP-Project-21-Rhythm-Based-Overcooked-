using UnityEngine;
using UnityEngine.InputSystem;

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
