using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRhythmSession : MonoBehaviour
{
    [SerializeField] private MusicClock musicClock;
    [SerializeField] private AnswerMaker answerMaker;
    [SerializeField] private RhythmEvaluator evaluator;
    [Tooltip("Optional - assign to see the arrows travel across the lane.")]
    [SerializeField] private RhythmLaneUI laneUI;

    [Header("Debug / no ingredients yet")]
    [SerializeField] private bool useRandomDebugInput = false;
    [SerializeField] private Gamepad targetGamepad;
    public int debugPatternLength = 4;
    private bool sessionActive;
    public event System.Action OnSequenceSucceeded;
    public event System.Action OnSequenceFailed;
    public event System.Action OnConfirmPressed;
    private bool burnedThisSequence;

    // --- NEW: STEAL MECHANIC ADDITIONS ---
    public bool IsSessionActive => sessionActive;
    // -------------------------------------

    private void OnEnable()
    {
        evaluator.OnSequenceComplete += HandleSequenceComplete;
        evaluator.OnIngredientBurned += HandleIngredientBurned;
    }

    private void OnDisable()
    {
        evaluator.OnSequenceComplete -= HandleSequenceComplete;
        evaluator.OnIngredientBurned -= HandleIngredientBurned;
    }

    public void StartSession()
    {
        if (sessionActive)
        {
            Debug.LogWarning("StartSession called while a session is still active — forcing reset.");
        }

        sessionActive = false;
        burnedThisSequence = false;

        RhythmDirection[] pattern = answerMaker.GenerateRandomPattern(debugPatternLength);
        BeginSequence(pattern);
    }

    // --- NEW: STEAL MECHANIC ADDITIONS ---
    public void StartStealSession()
    {
        if (sessionActive) return;

        sessionActive = false;
        burnedThisSequence = false;

        // Forces 10 patterns for the competitive steal mechanic
        RhythmDirection[] pattern = answerMaker.GenerateRandomPattern(10);
        BeginSequence(pattern);
    }
    public void ForceCancelSession()
    {
        // Used to stop the loser's session immediately when the winner finishes
        sessionActive = false;
        burnedThisSequence = true;
        // We don't invoke OnSequenceFailed here to prevent looping callbacks.
    }
    // -------------------------------------

    private void Update()
    {
        if (!sessionActive)
        {
            Gamepad paad = targetGamepad != null ? targetGamepad : Gamepad.current;
            Keyboard kb = Keyboard.current;

            bool confirmPressed =
                (paad != null && paad.bButton.wasPressedThisFrame) ||
                (targetGamepad == null && kb != null && kb.eKey.wasPressedThisFrame);

            if (confirmPressed)
                OnConfirmPressed?.Invoke();
        }

        if (!useRandomDebugInput || sessionActive) return;

        bool startPressed = false;

        Gamepad pad = targetGamepad != null ? targetGamepad : Gamepad.current;
        if (pad != null && pad.bButton.wasPressedThisFrame)
            startPressed = true;

        if (targetGamepad == null)
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame)
                startPressed = true;
        }

        if (startPressed)
        {
            RhythmDirection[] pattern = answerMaker.GenerateRandomPattern(debugPatternLength);
            BeginSequence(pattern);
        }
    }

    public void BeginSequence(RhythmDirection[] pattern)
    {
        Queue<AnswerPrompt> sequence = answerMaker.GenerateSequence(pattern, musicClock);
        sessionActive = true;

        List<AnswerPrompt> promptList = sequence.ToList();

        evaluator.BeginSequence(sequence);
        laneUI?.Display(promptList);
        Debug.Log($"PlayerRhythmSession: BeginSequence with {promptList.Count} prompts.");
    }

    private void HandleSequenceComplete()
    {
        if (!sessionActive) return;
        sessionActive = false;

        if (burnedThisSequence) return; // already failed
        OnSequenceSucceeded?.Invoke();
    }

    private void HandleIngredientBurned()
    {
        if (!sessionActive || burnedThisSequence) return; // already failed
        burnedThisSequence = true;
        sessionActive = false;
        OnSequenceFailed?.Invoke();
    }

    public void SetGamepad(Gamepad pad)
    {
        targetGamepad = pad;
    }
}