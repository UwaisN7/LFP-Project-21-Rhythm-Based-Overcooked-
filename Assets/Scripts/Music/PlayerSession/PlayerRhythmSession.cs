using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// One player's rhythm session. Ingredients call BeginSequence() with
/// their fixed input pattern when the player picks them up / starts
/// handling them - that path works regardless of the debug toggle below.
///
/// Until ingredient data exists, hold B to fire a random test pattern.
/// Flip useRandomDebugInput off once ingredients are feeding real
/// patterns - no code needs to be deleted, it just goes unused.
/// </summary>
public class PlayerRhythmSession : MonoBehaviour
{
    [SerializeField] private MusicClock musicClock;
    [SerializeField] private AnswerMaker answerMaker;
    [SerializeField] private RhythmEvaluator evaluator;
    [Tooltip("Optional - assign to see the arrows travel across the lane.")]
    [SerializeField] private RhythmLaneUI laneUI;

    [Header("Debug / no ingredients yet")]
    [SerializeField] private bool useRandomDebugInput = true;
    [SerializeField] private Gamepad targetGamepad;
    [SerializeField] private int debugPatternLength = 4;

    private void Update()
    {
        if (!useRandomDebugInput) return;
        //This is the start point
        Gamepad pad = targetGamepad != null ? targetGamepad : Gamepad.current;
        if (pad != null && pad.bButton.wasPressedThisFrame)
        {
            RhythmDirection[] pattern = answerMaker.GenerateRandomPattern(debugPatternLength);
            BeginSequence(pattern);
        }
    }

    public void BeginSequence(RhythmDirection[] pattern)
    {
        Queue<AnswerPrompt> sequence = answerMaker.GenerateSequence(pattern, musicClock);

        // Snapshot for the UI before the evaluator starts consuming the queue.
        List<AnswerPrompt> promptList = sequence.ToList();

        evaluator.BeginSequence(sequence);
        laneUI?.Display(promptList);
    }
}
