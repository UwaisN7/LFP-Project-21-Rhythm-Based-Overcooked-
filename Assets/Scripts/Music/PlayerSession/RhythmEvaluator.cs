using System.Collections.Generic;
using UnityEngine;


public class RhythmEvaluator : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("How close to the beat (seconds, either side) counts as 'on beat'.")]
    [SerializeField] private double hitWindowSeconds = 0.12;

    [Header("Refs")]
    [SerializeField] private MusicClock musicClock;
    [SerializeField] private RhythmInputManager inputManager;
    [SerializeField] private PointManager pointManager;
    [SerializeField] private int playerId = 0;

    public event System.Action<AnswerPrompt> OnPromptResolved;
    public event System.Action OnIngredientBurned;
    public event System.Action OnSequenceComplete;
    private Queue<AnswerPrompt> activePrompts = new Queue<AnswerPrompt>();
    private int strikes = 0;

    private void OnEnable()
    {
        if (inputManager != null)
            inputManager.OnDirectionPressed += HandleInput;
    }

    private void OnDisable()
    {
        if (inputManager != null)
            inputManager.OnDirectionPressed -= HandleInput;
    }
    //sdrrge5h
    public void BeginSequence(Queue<AnswerPrompt> prompts)
    {
        activePrompts = prompts;
        strikes = 0;
    }

    private void Update()
    {
        if (activePrompts.Count == 0 || musicClock == null) return;

        AnswerPrompt current = activePrompts.Peek();

        // Window closed with nothing hit -> Late strike.
        if (musicClock.SongTime > current.TargetSongTime + hitWindowSeconds)
        {
            Resolve(current, AnswerPrompt.Result.Late);
        }
       
    }

    private void HandleInput(RhythmDirection direction, double songTimeOfPress)
    { 
        if (activePrompts.Count == 0) return;

        AnswerPrompt current = activePrompts.Peek();

        double diff = songTimeOfPress - current.TargetSongTime;
Debug.Log($"[Eval] P{playerId} dir={direction} | press={songTimeOfPress:F4} " +
          $"target={current.TargetSongTime:F4} diff={diff * 1000:F1}ms " +
          $"window=±{hitWindowSeconds * 1000:F1}ms");
        //if (direction != current.RequiredDirection)
        //{
        //    Resolve(current, AnswerPrompt.Result.Wrong);
        //    return; //Comment this bitch out if its all returning wrong because yeah screw u u stupif pice of shit 
        //}
         if (diff < -hitWindowSeconds)
        {
            Resolve(current, AnswerPrompt.Result.Early);
        }
        else if (diff > hitWindowSeconds)
        {
            Resolve(current, AnswerPrompt.Result.Late);
        }
        else
        {
            Resolve(current, AnswerPrompt.Result.OnBeat);
        }
    }
    private void Resolve(AnswerPrompt prompt, AnswerPrompt.Result result)
    {
        activePrompts.Dequeue();
        prompt.CurrentResult = result;
        if (activePrompts.Count == 0)
        {
            OnSequenceComplete?.Invoke();
        }

        switch (result)
        {
            case AnswerPrompt.Result.Early:
                pointManager?.AddActionPoints(playerId, -25);
                Debug.Log("`Plyaer hit early no penalty but no points");
                break;

            case AnswerPrompt.Result.OnBeat:
                pointManager?.AddActionPoints(playerId, 50);
                Debug.Log("Added Points player hit on beat");
                break;

            case AnswerPrompt.Result.Wrong:
                pointManager?.AddActionPoints(playerId, -50);
                Debug.Log("Losing Points");
                RegisterStrike();
                break;

            case AnswerPrompt.Result.Late:
                Debug.Log("Player hit late so they might burn");
                RegisterStrike();
                break;
        }

        OnPromptResolved?.Invoke(prompt);
    }

    private void RegisterStrike()
    {
        strikes++;
        pointManager?.ReportStrike(playerId);

        if (strikes >= 2)
        {
            OnIngredientBurned?.Invoke();
        }
    }
}
