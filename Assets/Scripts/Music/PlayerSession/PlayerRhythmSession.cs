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
    [SerializeField] private bool useRandomDebugInput = true;
    [SerializeField] private Gamepad targetGamepad;
    [SerializeField] private int debugPatternLength = 4;
    private bool sessionActive;


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
    private void Update()
    {

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
    }

    private void HandleIngredientBurned()
    {
        sessionActive = false;
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
