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
    [SerializeField] private int debugPatternLength = 4;
    private bool sessionActive;
    public event System.Action OnSequenceSucceeded;
    public event System.Action OnSequenceFailed;
    public event System.Action OnConfirmPressed;
    private bool burnedThisSequence;

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
    }
    private void HandleSequenceComplete()
    {
        sessionActive = false;

        if (burnedThisSequence) return; // already failed
        OnSequenceSucceeded?.Invoke();
    }

    private void HandleIngredientBurned()
    {
          if (burnedThisSequence) return; // already failed
    burnedThisSequence = true;
    sessionActive = false;
    OnSequenceFailed?.Invoke();
    }
    public void SetGamepad(Gamepad pad)
    {
        targetGamepad = pad;
    }
    //void CalcultateFinalScores(orderDifficulty, timeLeft)
    //{
    //    //Order Manager gives the order difficulty and the time left on the dish (Order Difficulty +Timer) * Combo Multiplier = Final Score
   //also tells me the player id and then it just adds that value to final score easy peasy

    //  
    //}
}
