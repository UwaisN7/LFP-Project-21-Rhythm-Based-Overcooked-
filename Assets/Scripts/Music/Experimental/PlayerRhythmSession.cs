using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// One player's rhythm session. Ingredients call BeginSequence() with
/// their fixed input pattern when the player picks them up / starts
/// handling them. This is the only entry point the rest of the game
/// (ingredient scripts, UI) needs to know about.
/// </summary>
public class PlayerRhythmSession : MonoBehaviour
{
    [SerializeField] private MusicClock musicClock;
    [SerializeField] private AnswerMaker answerMaker;
    [SerializeField] private RhythmEvaluator evaluator;
    [SerializeField] private Gamepad targetGamepad;
    public void Update()
    {
        Gamepad pad = targetGamepad != null ? targetGamepad : Gamepad.current;

        // Ensure the gamepad is not null and check if the B button was pressed
        if (pad != null && pad.bButton.wasPressedThisFrame)
        {
            // Define or pass the pattern you want to trigger for the B button
            RhythmDirection[] pattern = new RhythmDirection[] { RhythmDirection.Right };
            BeginSequence(pattern);
        }
    }
    public void BeginSequence(RhythmDirection[] pattern)
    {
        Queue<AnswerPrompt> sequence = answerMaker.GenerateSequence(pattern, musicClock);
        evaluator.BeginSequence(sequence);
    }
}
